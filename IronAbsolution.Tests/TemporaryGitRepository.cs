using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace IronAbsolution.Tests;

/// <summary>
/// A git repository in a temporary folder, with commits that a test writes. The origin refs
/// stand in for a fetch: a test writes them with `update-ref`. The folder goes away at the end
/// of the test.
/// </summary>
public sealed class TemporaryGitRepository : IDisposable
{
    /// <summary>The commit time of every commit, so each hash is the same on each run.</summary>
    private const string CommitTime = "2026-09-27T10:00:00Z";

    /// <summary>Makes an empty repository on the branch `main`.</summary>
    public TemporaryGitRepository()
    {
        this.Root = Path.Combine(Path.GetTempPath(), "codex-review-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(this.Root);
        this.Git("init", "--quiet", "--initial-branch=main");
    }

    /// <summary>Gets the full path of the root of the repository.</summary>
    public string Root { get; }

    /// <summary>Writes each file, stages every change, and commits.</summary>
    /// <param name="message">The commit message.</param>
    /// <param name="files">Each path under the root, with forward slashes, and its text.</param>
    /// <returns>The full hash of the new commit.</returns>
    public string Commit(string message, params (string Path, string Text)[] files)
    {
        ArgumentNullException.ThrowIfNull(files);

        foreach ((string relativePath, string text) in files)
        {
            string full = Path.Combine(this.Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            string folder = Path.GetDirectoryName(full)
                ?? throw new InvalidOperationException($"The path '{full}' has no folder.");
            Directory.CreateDirectory(folder);
            File.WriteAllText(full, text);
        }

        this.Git("add", "--all");
        this.Git("commit", "--quiet", "--allow-empty", "-m", message);
        return this.Git("rev-parse", "HEAD").Trim();
    }

    /// <summary>Makes a branch at the current commit and checks it out.</summary>
    /// <param name="name">The branch name.</param>
    public void CreateBranch(string name)
    {
        this.Git("checkout", "--quiet", "-b", name);
    }

    /// <summary>Points the ref of a branch on origin at a commit, as a fetch would.</summary>
    /// <param name="branch">The branch name on origin.</param>
    /// <param name="sha">The commit.</param>
    public void SetOrigin(string branch, string sha)
    {
        this.Git("update-ref", $"refs/remotes/origin/{branch}", sha);
    }

    /// <summary>Runs git in the repository, with a fixed identity and commit time.</summary>
    /// <param name="args">Each argument after `git`.</param>
    /// <returns>The full text of stdout.</returns>
    /// <exception cref="InvalidOperationException">git gave an exit code other than 0. The message holds stderr.</exception>
    public string Git(params string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        ProcessStartInfo start = new ProcessStartInfo("git")
        {
            WorkingDirectory = this.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // The machine configuration does not reach the fixture: no hook, no signature, no line-ending change.
        foreach (string setting in new[] { "user.name=Test", "user.email=test@example.invalid", "commit.gpgsign=false", "core.autocrlf=false", "core.hooksPath=/dev/null" })
        {
            start.ArgumentList.Add("-c");
            start.ArgumentList.Add(setting);
        }

        foreach (string arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        start.Environment["GIT_AUTHOR_DATE"] = CommitTime;
        start.Environment["GIT_COMMITTER_DATE"] = CommitTime;
        using Process process = Process.Start(start)
            ?? throw new InvalidOperationException($"`git {string.Join(' ', args)}` gave no process in '{this.Root}'.");
        Task<string> errorText = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"`git {string.Join(' ', args)}` gave the exit code {process.ExitCode.ToString(CultureInfo.InvariantCulture)} in '{this.Root}'. {errorText.GetAwaiter().GetResult().Trim()}");
        }

        return output;
    }

    /// <summary>Removes the repository from the temporary folder.</summary>
    public void Dispose()
    {
        if (!Directory.Exists(this.Root))
        {
            return;
        }

        // Git writes each object file as read-only, and Windows refuses the delete of such a file.
        foreach (string file in Directory.EnumerateFiles(this.Root, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(this.Root, recursive: true);
    }
}
