using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IronAbsolution.Tools.SteCheck;

namespace IronAbsolution.Tests;

/// <summary>
/// A small checkout in a temporary folder, with one register for each id prefix. A test writes
/// the document that it reads, and the folder goes away at the end of the test.
/// </summary>
public sealed class SteCheckCheckout : IDisposable
{
    private SteCheckCheckout(string root) => Root = root;

    /// <summary>The full path of the root of the fixture checkout.</summary>
    public string Root { get; }

    /// <summary>Builds a checkout that passes every reference rule and every session rule.</summary>
    /// <returns>The fixture checkout. The caller disposes it.</returns>
    public static SteCheckCheckout Build()
    {
        string root = Path.Combine(Path.GetTempPath(), "ste-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        SteCheckCheckout checkout = new SteCheckCheckout(root);

        checkout.Write(
            "docs/decisions.md",
            "# Decisions",
            string.Empty,
            "| # | Date | Topic | Decision | Effect |",
            "|---|---|---|---|---|",
            "| D-1 | 2026-09-12 | The first answer | The first answer of the owner. | Superseded by D-2 on 2026-09-13: the second answer stands. |",
            "| D-2 | 2026-09-13 | The second answer | The second answer of the owner. | Supersedes D-1. |");

        checkout.Write(
            "docs/questions.md",
            "# Open questions",
            string.Empty,
            "1. **OQ-1. The first question.** The first question of the register.");

        checkout.Write(
            "docs/design.md",
            "# Design",
            string.Empty,
            "- **T-1. The first tenet.** The first tenet of the project.",
            "- **L-1. The first lesson.** The first lesson of the project.",
            "- **G-1. The first guardrail.** The first guardrail of the project.",
            "- M-1: the first measurement of the project.",
            string.Empty,
            "| # | Finding | Date | Status |",
            "|---|---|---|---|",
            "| F-1 | The first finding of the project | 2026-09-12 | done |",
            string.Empty,
            "## 8. Sequence",
            string.Empty,
            "1. PR-1, PR-2.",
            string.Empty,
            "## 9. Open questions",
            string.Empty,
            "The register is another file.");

        checkout.Write(
            "docs/roadmaps/phase-1-foundations.md",
            "# Phase 1",
            string.Empty,
            "### 7.1 PR-3: retired",
            string.Empty,
            "This entry holds a retired id, and no later item takes it.");

        checkout.Write(
            "docs/session-handoff.md",
            "# Session handoff",
            string.Empty,
            "## Session 2: 2026-09-13, Codex",
            string.Empty,
            "The second session.",
            string.Empty,
            "## Session 1: 2026-09-12, Claude Code",
            string.Empty,
            "The first session.");

        checkout.Write(
            "docs/session-handoff-archive.md",
            "# Session handoff archive",
            string.Empty,
            "The archive holds no entry yet.");

        // The size rules read the start set: the two instructions files and the skills (D-17).
        string[] instructions =
        [
            "# The fixture instructions",
            string.Empty,
            "This file stands for the instructions of a session. It stays small.",
        ];
        checkout.Write("CLAUDE.md", instructions);
        checkout.Write("AGENTS.md", instructions);

        checkout.Write(
            ".claude/skills/ste-writing/SKILL.md",
            "# The fixture skill",
            string.Empty,
            "This file stands for a skill of the task. It stays small.");

        checkout.Write("docs/reviews/readme.md", "# The fixture reviews", string.Empty, "The reviews of the fixture.");
        checkout.Write("README.md", "# The fixture", string.Empty, "The description of the fixture. The reviews are in `docs/reviews/`.");

        return checkout;
    }

    /// <summary>Writes one document of the fixture checkout.</summary>
    /// <param name="relativePath">The path under the root, with forward slashes.</param>
    /// <param name="lines">Each line of the document, in order.</param>
    public void Write(string relativePath, params string[] lines)
    {
        ArgumentException.ThrowIfNullOrEmpty(relativePath);
        ArgumentNullException.ThrowIfNull(lines);

        string full = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        string? folder = Path.GetDirectoryName(full);
        if (folder is not null)
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllLines(full, lines);
    }

    /// <summary>Writes a document of an exact byte count, as the size rules count it.</summary>
    /// <param name="relativePath">The path under the root, with forward slashes.</param>
    /// <param name="bytes">The count that <see cref="SizeRules.ByteCount"/> must give. One byte at least.</param>
    public void WriteOfSize(string relativePath, int bytes)
    {
        ArgumentException.ThrowIfNullOrEmpty(relativePath);
        ArgumentOutOfRangeException.ThrowIfLessThan(bytes, 1);

        // Each line takes its own bytes and one byte for the line ending.
        const int LineBytes = 64;
        List<string> lines = [];
        int remaining = bytes;
        while (remaining > LineBytes)
        {
            lines.Add(new string('x', LineBytes - 1));
            remaining -= LineBytes;
        }

        lines.Add(new string('x', remaining - 1));
        Write(relativePath, [.. lines]);
    }

    /// <summary>Adds lines to the end of a document of the fixture checkout.</summary>
    /// <param name="relativePath">The path under the root, with forward slashes.</param>
    /// <param name="lines">Each line to add, in order.</param>
    public void Append(string relativePath, params string[] lines)
    {
        ArgumentException.ThrowIfNullOrEmpty(relativePath);
        ArgumentNullException.ThrowIfNull(lines);

        List<string> all = [.. File.ReadAllLines(
            Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar)))];
        all.AddRange(lines);
        Write(relativePath, [.. all]);
    }

    /// <summary>
    /// Gives the fixture its git data, and puts every file that it holds now into the index.
    /// A file that a test writes after this call is untracked (D-17).
    /// </summary>
    public void TrackEveryFile()
    {
        // The force option keeps an ignore rule of the machine out of the fixture.
        RunGit("init", "--quiet");
        RunGit("add", "--all", "--force");
    }

    /// <summary>Puts one file of the fixture checkout into the index.</summary>
    /// <param name="relativePath">The path under the root, with forward slashes.</param>
    public void Track(string relativePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(relativePath);
        RunGit("add", "--force", "--", relativePath);
    }

    /// <summary>Removes the fixture checkout from the temporary folder.</summary>
    public void Dispose()
    {
        if (!Directory.Exists(Root))
        {
            return;
        }

        // Git writes each object file as read-only, and Windows refuses the delete of such a file.
        string gitFolder = Path.Combine(Root, ".git");
        if (Directory.Exists(gitFolder))
        {
            foreach (string file in Directory.EnumerateFiles(gitFolder, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }
        }

        Directory.Delete(Root, recursive: true);
    }

    private void RunGit(params string[] arguments)
    {
        ProcessStartInfo start = new ProcessStartInfo("git")
        {
            WorkingDirectory = Root,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(start)
            ?? throw new InvalidOperationException($"`git {string.Join(' ', arguments)}` gave no process in '{Root}'.");
        string errorText = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"`git {string.Join(' ', arguments)}` gave the exit code {process.ExitCode} in '{Root}'. {errorText.Trim()}");
        }
    }
}
