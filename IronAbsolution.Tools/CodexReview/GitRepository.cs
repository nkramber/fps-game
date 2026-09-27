using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>The hash and the full message of one commit.</summary>
/// <param name="Sha">The full hash of the commit.</param>
/// <param name="Message">The subject and the body, as `git log --format=%B` gives them.</param>
public sealed record CommitMessage(string Sha, string Message);

/// <summary>The hash and the subject line of one commit.</summary>
/// <param name="Sha">The full hash of the commit.</param>
/// <param name="Subject">The first line of the message.</param>
public sealed record CommitSubject(string Sha, string Subject);

/// <summary>Runs git in one checkout. Every failure carries the command, the exit code, and stderr (T-2).</summary>
public sealed class GitRepository
{
    private readonly string path;

    /// <summary>Makes a runner for the checkout at the path.</summary>
    /// <param name="path">The root of the checkout.</param>
    public GitRepository(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        this.path = path;
    }

    /// <summary>Gives the text of a file at a revision.</summary>
    /// <param name="revision">The revision, such as a hash or a ref.</param>
    /// <param name="filePath">The path from the root, with forward slashes.</param>
    /// <returns>The file text, or null when the revision holds no file at the path.</returns>
    /// <exception cref="InvalidOperationException">The revision is not valid, or git failed.</exception>
    public string? ReadFileOrNull(string revision, string filePath)
    {
        // `ls-tree` prints nothing for an absent path and exits 0. It still fails on a bad revision.
        string entry = this.Run(["ls-tree", revision, "--", filePath]);
        if (entry.Trim().Length == 0)
        {
            return null;
        }

        return this.Run(["show", $"{revision}:{filePath}"]);
    }

    /// <summary>Gives the merge base of two revisions.</summary>
    /// <param name="first">The first revision.</param>
    /// <param name="second">The second revision.</param>
    /// <returns>The hash of the merge base.</returns>
    /// <exception cref="InvalidOperationException">The two revisions have no merge base, or git failed.</exception>
    public string MergeBase(string first, string second)
    {
        return this.Run(["merge-base", first, second]).Trim();
    }

    /// <summary>
    /// Gives every path that changes from the first revision to the second. A move gives both
    /// paths, because the rename detection of git hides the old path. A code file that moves into
    /// `docs/` then still counts as a code change.
    /// </summary>
    /// <param name="mergeBase">The start of the diff.</param>
    /// <param name="head">The end of the diff.</param>
    /// <returns>Each changed path, with forward slashes, in the order of git.</returns>
    /// <exception cref="InvalidOperationException">A revision is not valid, or git failed.</exception>
    public IReadOnlyList<string> ChangedPaths(string mergeBase, string head)
    {
        string output = this.Run(["diff", "--name-only", "--no-renames", mergeBase, head]);
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>Gives the full message of each commit that the head holds and the base does not, newest first.</summary>
    /// <param name="baseRevision">The base of the range. The range does not hold it or its ancestors.</param>
    /// <param name="head">The end of the range.</param>
    /// <returns>The hash and the message of each commit.</returns>
    /// <exception cref="InvalidOperationException">A revision is not valid, git failed, or git gave a record of another form.</exception>
    public IReadOnlyList<CommitMessage> CommitMessages(string baseRevision, string head)
    {
        // `-z` ends each record with a NUL byte, so a message with blank lines stays one record.
        string output = this.Run(["log", "-z", "--format=%H%n%B", $"{baseRevision}..{head}"]);
        List<CommitMessage> messages = [];
        foreach (string record in output.Split('\0', StringSplitOptions.RemoveEmptyEntries))
        {
            int lineEnd = record.IndexOf('\n', StringComparison.Ordinal);
            if (lineEnd < 0)
            {
                throw new InvalidOperationException(
                    $"`git log {baseRevision}..{head}` in '{this.path}' gave the record '{record}'. The expected form is the hash, a line end, and the message.");
            }

            messages.Add(new CommitMessage(record[..lineEnd], record[(lineEnd + 1)..]));
        }

        return messages;
    }

    /// <summary>
    /// Gives the newest commit in the range that changes a path outside the excluded paths. An
    /// entry that ends in a slash excludes each path under it. Any other entry excludes the one
    /// file of that name.
    /// </summary>
    /// <param name="mergeBase">The start of the range. The range does not hold it.</param>
    /// <param name="head">The end of the range.</param>
    /// <param name="excludedPaths">The paths that do not count.</param>
    /// <returns>The hash of the commit, or null when no commit of the range changes a path outside the excluded paths.</returns>
    /// <exception cref="InvalidOperationException">The two candidate commits lie on separate lines of the history, so neither is the newer one.</exception>
    public string? NewestCommitOutside(string mergeBase, string head, IReadOnlyList<string> excludedPaths)
    {
        ArgumentNullException.ThrowIfNull(excludedPaths);

        // A git pathspec of a file name also matches a folder of that name, so ':(exclude)LICENSE'
        // hides the path 'LICENSE/evil.cs' too. A second walk finds the paths under such a folder.
        List<string> outsidePathspecs = ["."];
        List<string> fileNamedFolderPathspecs = [];
        List<string> folderExclusions = [];
        foreach (string excluded in excludedPaths)
        {
            outsidePathspecs.Add($":(exclude){excluded}");
            if (excluded.EndsWith('/'))
            {
                folderExclusions.Add($":(exclude){excluded}");
            }
            else
            {
                fileNamedFolderPathspecs.Add(excluded + "/");
            }
        }

        string? outside = this.NewestCommitIn(mergeBase, head, outsidePathspecs);
        if (fileNamedFolderPathspecs.Count == 0)
        {
            return outside;
        }

        fileNamedFolderPathspecs.AddRange(folderExclusions);
        string? underFileNamedFolder = this.NewestCommitIn(mergeBase, head, fileNamedFolderPathspecs);
        return this.Newer(outside, underFileNamedFolder, mergeBase, head);
    }

    /// <summary>Gives the committer time of a commit.</summary>
    /// <param name="sha">The commit.</param>
    /// <returns>The committer time, with its offset.</returns>
    /// <exception cref="InvalidOperationException">The commit is not valid, git failed, or git gave a time of another form.</exception>
    public DateTimeOffset CommitTime(string sha)
    {
        ArgumentException.ThrowIfNullOrEmpty(sha);

        string text = this.Run(["show", "--no-patch", "--format=%cI", sha]).Trim();
        if (!DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset time))
        {
            throw new InvalidOperationException(
                $"`git show --no-patch --format=%cI {sha}` in '{this.path}' gave '{text}'. The expected form is a strict ISO 8601 time.");
        }

        return time;
    }

    /// <summary>Gives the newest commit up to the head that changes a file, with its subject.</summary>
    /// <param name="head">The end of the history.</param>
    /// <param name="filePath">The path from the root, with forward slashes.</param>
    /// <returns>The commit, or null when no commit up to the head changes the file.</returns>
    /// <exception cref="InvalidOperationException">The head is not valid, git failed, or git gave a record of another form.</exception>
    public CommitSubject? NewestCommitThatChanged(string head, string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(head);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        string output = this.Run(["log", "-1", "--format=%H%x00%s", head, "--", filePath]).TrimEnd('\n');
        if (output.Length == 0)
        {
            return null;
        }

        int separator = output.IndexOf('\0', StringComparison.Ordinal);
        if (separator < 0)
        {
            throw new InvalidOperationException(
                $"`git log -1 {head} -- {filePath}` in '{this.path}' gave '{output}'. The expected form is the hash, a NUL byte, and the subject.");
        }

        return new CommitSubject(output[..separator], output[(separator + 1)..]);
    }

    /// <summary>Runs git and gives stdout.</summary>
    /// <param name="args">Each argument after `git`.</param>
    /// <returns>The full text of stdout.</returns>
    /// <exception cref="InvalidOperationException">git did not start, or it gave an exit code other than 0.</exception>
    public string Run(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        (int exitCode, string standardOutput, string standardError) = this.Execute(args);
        if (exitCode != 0)
        {
            throw new InvalidOperationException(
                $"`git {string.Join(' ', args)}` gave the exit code {exitCode} in '{this.path}'. stderr: {standardError.Trim()}");
        }

        return standardOutput;
    }

    /// <summary>Tells whether the commit is the revision or an ancestor of it. git exits 1 when it is not.</summary>
    private bool IsAncestor(string commit, string revision)
    {
        string[] args = ["merge-base", "--is-ancestor", commit, revision];
        (int exitCode, string _, string standardError) = this.Execute(args);
        if (exitCode is not (0 or 1))
        {
            throw new InvalidOperationException(
                $"`git {string.Join(' ', args)}` gave the exit code {exitCode} in '{this.path}'. stderr: {standardError.Trim()}");
        }

        return exitCode == 0;
    }

    /// <summary>Gives the newest commit in the range that changes a path of the pathspecs, or null when no commit does.</summary>
    private string? NewestCommitIn(string mergeBase, string head, IReadOnlyList<string> pathspecs)
    {
        List<string> args = ["log", "-1", "--format=%H", $"{mergeBase}..{head}", "--"];
        args.AddRange(pathspecs);
        string sha = this.Run(args).Trim();
        return sha.Length == 0 ? null : sha;
    }

    /// <summary>
    /// Gives the later of two commits of the range by ancestry, or the one that is not null.
    /// The commit time does not decide, because a rebase can keep the time of an older commit.
    /// </summary>
    private string? Newer(string? first, string? second, string mergeBase, string head)
    {
        if (first is null || second is null)
        {
            return first ?? second;
        }

        if (this.IsAncestor(first, second))
        {
            return second;
        }

        if (this.IsAncestor(second, first))
        {
            return first;
        }

        throw new InvalidOperationException(
            $"The commits {first} and {second} lie on separate lines of the history from {mergeBase} to {head}, so neither is the newest commit outside the excluded paths.");
    }

    private (int ExitCode, string StandardOutput, string StandardError) Execute(IReadOnlyList<string> args)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = this.path,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (string arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        Process? started;
        try
        {
            started = Process.Start(startInfo);
        }
        catch (Win32Exception exception)
        {
            throw new InvalidOperationException($"git did not start in '{this.path}': {exception.Message}", exception);
        }

        using Process process = started ?? throw new InvalidOperationException($"git gave no process in '{this.path}'.");

        // The two streams drain at the same time. git blocks on a full stderr pipe, so a read of
        // stdout to its end first never ends.
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        string standardOutput = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, standardOutput, standardError.GetAwaiter().GetResult());
    }
}
