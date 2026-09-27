using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IronAbsolution.Tools.CodexReview;

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
