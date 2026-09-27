using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IronAbsolution.Tools;
using IronAbsolution.Tools.HandoffRotate;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The handoff rotation (D-20, D-58): the move of each entry after the tenth to the archive top,
/// the text of each entry, the sort of an entry out of place, the faults, and the exit codes.
/// </summary>
public sealed class HandoffRotateTests
{
    // The handoff has no title, so its first line is the newest entry (D-20).
    private const string HandoffPreamble = "";

    private const string ArchivePreamble = "# Session handoff archive\n\nThis file holds the entries that the rotation moves.\n\n";

    [Fact]
    public void TheEleventhEntryMovesToTheArchiveTop()
    {
        // Exit test 2 of PR-4.
        string handoff = Entries(12, 2);
        string archive = ArchivePreamble + Entries(1, 1);

        HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, archive);

        Assert.Equal([2], rotation.Moved);
        Assert.Equal(13, rotation.NextSession);
        Assert.Equal(Entries(12, 3), rotation.Handoff);
        Assert.Equal(ArchivePreamble + Entries(2, 1), rotation.Archive);
    }

    [Fact]
    public void TwelveEntriesMoveTheTwoOldestToTheArchiveTop()
    {
        string handoff = HandoffPreamble + Entries(112, 101);
        string archive = ArchivePreamble + Entries(100, 99);

        HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, archive);

        Assert.Equal([102, 101], rotation.Moved);
        Assert.Equal(113, rotation.NextSession);
        Assert.Equal(HandoffPreamble + Entries(112, 103), rotation.Handoff);
        Assert.Equal(ArchivePreamble + Entries(102, 99), rotation.Archive);
    }

    [Fact]
    public void AnArchiveWithNoEntryTakesTheMovedEntriesUnderItsTitle()
    {
        HandoffRotation rotation = HandoffRotateRules.Rotate(Entries(11, 1), ArchivePreamble);

        Assert.Equal([1], rotation.Moved);
        Assert.Equal(ArchivePreamble + Entries(1, 1), rotation.Archive);
    }

    [Fact]
    public void TenEntriesChangeNothing()
    {
        string handoff = HandoffPreamble + Entries(20, 11) + "\n\n";
        string archive = ArchivePreamble + Entries(10, 9);

        HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, archive);

        Assert.Empty(rotation.Moved);
        Assert.Empty(rotation.Reordered);
        Assert.Equal(21, rotation.NextSession);
        Assert.Same(handoff, rotation.Handoff);
        Assert.Same(archive, rotation.Archive);
    }

    [Fact]
    public void EachEntryKeepsItsTextOverManySeeds()
    {
        // Each seed makes a random count of entries, random bodies, and random blank lines.
        for (int seed = 1; seed <= 300; seed++)
        {
            Random random = new Random(seed);
            int handoffCount = random.Next(1, 26);
            int archiveCount = random.Next(0, 6);
            int newest = 1000 + random.Next(0, 50);
            string handoff = HandoffPreamble + RandomEntries(random, newest, handoffCount);
            string archive = ArchivePreamble + RandomEntries(random, newest - handoffCount - random.Next(0, 3), archiveCount);

            HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, archive);

            List<string> before = TrimmedEntries(handoff).Concat(TrimmedEntries(archive)).ToList();
            List<string> after = TrimmedEntries(rotation.Handoff).Concat(TrimmedEntries(rotation.Archive)).ToList();
            Assert.True(before.SequenceEqual(after, StringComparer.Ordinal), $"Seed {seed}: the entry texts or their order changed.");
            Assert.True(HandoffRotateRules.Parse(rotation.Handoff, HandoffRotateRules.HandoffPath).Entries.Count == Math.Min(handoffCount, HandoffRotateRules.KeepCount), $"Seed {seed}: the handoff holds the wrong count.");
            Assert.True(rotation.Archive.StartsWith(ArchivePreamble, StringComparison.Ordinal), $"Seed {seed}: the archive title changed.");
            Assert.True(rotation.Moved.Count == Math.Max(0, handoffCount - HandoffRotateRules.KeepCount), $"Seed {seed}: the moved count is wrong.");
            HandoffFile oldArchive = HandoffRotateRules.Parse(archive, HandoffRotateRules.ArchivePath);
            if (oldArchive.Entries.Count > 0)
            {
                string oldEntries = archive[oldArchive.Preamble.Length..];
                Assert.True(rotation.Archive.EndsWith(oldEntries, StringComparison.Ordinal), $"Seed {seed}: the old archive entries changed.");
            }

            HandoffRotation second = HandoffRotateRules.Rotate(rotation.Handoff, rotation.Archive);
            Assert.True(second.Moved.Count == 0 && second.Handoff == rotation.Handoff, $"Seed {seed}: a second rotation changed the files.");
        }
    }

    [Fact]
    public void TheRepositoryFilesHoldTheRule()
    {
        // The committed handoff holds 10 entries or fewer, newest first, above an older archive
        // top. The command sorts an entry out of place, so the committed file must need no sort (D-58).
        string handoff = File.ReadAllText(RepositoryRoot.PathTo(HandoffRotateRules.HandoffPath));
        string archive = File.ReadAllText(RepositoryRoot.PathTo(HandoffRotateRules.ArchivePath));

        HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, archive);

        Assert.Empty(rotation.Moved);
        Assert.Empty(rotation.Reordered);
        Assert.Equal(handoff, rotation.Handoff);
        Assert.True(HandoffRotateRules.Parse(archive, HandoffRotateRules.ArchivePath).Entries[0].Number < HandoffRotateRules.Parse(handoff, HandoffRotateRules.HandoffPath).Entries[^1].Number);
    }

    [Fact]
    public void ADuplicateSessionNumberFailsAndNamesIt()
    {
        string handoff = Entry(12) + "\n\n" + Entry(12) + "\n\n" + Entries(11, 1);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => HandoffRotateRules.Rotate(handoff, ArchivePreamble));

        Assert.Contains("holds Session 12 two times (L-2)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnOlderEntryAboveANewerOneGoesBackInPlace()
    {
        string handoff = Entry(5) + "\n\n" + Entry(6) + "\n";

        HandoffRotation rotation = HandoffRotateRules.Rotate(handoff, ArchivePreamble);

        // Both entries changed place, so the rotation names both.
        Assert.Equal([5, 6], rotation.Reordered);
        Assert.Empty(rotation.Moved);
        Assert.Equal(7, rotation.NextSession);
        Assert.Equal(Entry(6) + "\n\n" + Entry(5) + "\n", rotation.Handoff);

        HandoffRotation again = HandoffRotateRules.Rotate(rotation.Handoff, ArchivePreamble);
        Assert.Empty(again.Reordered);
        Assert.Equal(rotation.Handoff, again.Handoff);
    }

    [Fact]
    public void AnEntryAtTheEndOfAFullHandoffGoesBackInPlace()
    {
        // Eleven entries, newest first, with the newest one at the end of the file.
        string ordered = Entries(111, 101);
        HandoffFile parsed = HandoffRotateRules.Parse(ordered, HandoffRotateRules.HandoffPath);
        string outOfOrder = string.Join("\n\n", parsed.Entries.Skip(1).Select(entry => entry.Text.TrimEnd())) + "\n\n" + parsed.Entries[0].Text.TrimEnd() + "\n";

        HandoffRotation rotation = HandoffRotateRules.Rotate(outOfOrder, ArchivePreamble);

        Assert.Equal(112, rotation.NextSession);
        Assert.Equal([101], rotation.Moved);
        Assert.Contains(111, rotation.Reordered);
        Assert.Equal(HandoffRotateRules.Rotate(ordered, ArchivePreamble).Handoff, rotation.Handoff);
    }

    [Fact]
    public void AnArchiveTopThatIsNotOlderFails()
    {
        // A rotation that stopped after the archive write leaves the moved entries at the archive top.
        string handoff = Entries(111, 101);
        string archive = ArchivePreamble + Entries(101, 99);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => HandoffRotateRules.Rotate(handoff, archive));

        Assert.Contains("starts with Session 101", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AHandoffWithNoEntryFails()
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => HandoffRotateRules.Rotate("# A title alone\n", ArchivePreamble));

        Assert.Contains("has no '## Session <number>:' heading", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AHeadingWithNoColonIsNoEntry()
    {
        // The parse reads the heading form of the session number check of ste-check (D-20).
        HandoffFile parsed = HandoffRotateRules.Parse("## Session 3: 2026-09-27, Codex\n\n## Session notes\n\n## Session 2 2026-09-27\n", HandoffRotateRules.HandoffPath);

        Assert.Equal([3], parsed.Entries.Select(entry => entry.Number));
    }

    [Fact]
    public void ASessionNumberTooLargeForAnIntIsAFaultThatChangesNoFile()
    {
        // Review P2-1 of PR #5: `int.Parse` threw an unhandled overflow, and the process exited 134.
        string root = Path.Combine(Path.GetTempPath(), "handoff-rotate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "docs"));
        string handoffPath = Path.Combine(root, "docs", "session-handoff.md");
        string archivePath = Path.Combine(root, "docs", "session-handoff-archive.md");
        string handoff = "## Session 99999999999999999999: 2026-09-27, Codex\n\n- The work.\n\n" + Entries(11, 1);
        try
        {
            File.WriteAllText(handoffPath, handoff);
            File.WriteAllText(archivePath, ArchivePreamble);

            (int exitCode, string output, string errors) = RunCommand("--root", root);

            Assert.Equal(Program.FaultExitCode, exitCode);
            Assert.Empty(output);
            Assert.Contains("'docs/session-handoff.md' has the heading 'Session 99999999999999999999'", errors, StringComparison.Ordinal);
            Assert.Contains("No file changed.", errors, StringComparison.Ordinal);
            Assert.Equal(handoff, File.ReadAllText(handoffPath));
            Assert.Equal(ArchivePreamble, File.ReadAllText(archivePath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void TheCommandMovesEntriesAndExitsZeroOrOne()
    {
        string root = Path.Combine(Path.GetTempPath(), "handoff-rotate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "docs"));
        string handoffPath = Path.Combine(root, "docs", "session-handoff.md");
        string archivePath = Path.Combine(root, "docs", "session-handoff-archive.md");
        try
        {
            File.WriteAllText(handoffPath, Entries(11, 1));
            File.WriteAllText(archivePath, ArchivePreamble);
            (int exitCode, string output, string errors) = RunCommand("--root", root);
            Assert.Equal(0, exitCode);
            Assert.Contains("moved Session 1 to 'docs/session-handoff-archive.md'. The next session number is 12.", output, StringComparison.Ordinal);
            Assert.Empty(errors);
            Assert.Equal(Entries(11, 2), File.ReadAllText(handoffPath));
            Assert.Equal(ArchivePreamble + Entries(1, 1), File.ReadAllText(archivePath));

            // A second run moves no entry and changes no file.
            (exitCode, output, _) = RunCommand("--root", root);
            Assert.Equal(0, exitCode);
            Assert.Contains("No entry moved. The next session number is 12.", output, StringComparison.Ordinal);
            Assert.Equal(Entries(11, 2), File.ReadAllText(handoffPath));

            // An entry out of place goes back, and the command names it.
            File.WriteAllText(handoffPath, Entry(3) + "\n\n" + Entry(4) + "\n");
            (exitCode, output, _) = RunCommand("--root", root);
            Assert.Equal(0, exitCode);
            Assert.Contains("Session 3, Session 4 changed place", output, StringComparison.Ordinal);
            Assert.Equal(Entry(4) + "\n\n" + Entry(3) + "\n", File.ReadAllText(handoffPath));

            // A duplicate number exits 1 and leaves both files as they were.
            string broken = Entry(3) + "\n\n" + Entry(3) + "\n";
            File.WriteAllText(handoffPath, broken);
            (exitCode, _, errors) = RunCommand("--root", root);
            Assert.Equal(Program.FaultExitCode, exitCode);
            Assert.Contains("Session 3 two times", errors, StringComparison.Ordinal);
            Assert.Contains("No file changed.", errors, StringComparison.Ordinal);
            Assert.Equal(broken, File.ReadAllText(handoffPath));
            Assert.Equal(ArchivePreamble + Entries(1, 1), File.ReadAllText(archivePath));

            File.Delete(archivePath);
            (exitCode, _, errors) = RunCommand("--root", root);
            Assert.Equal(Program.FaultExitCode, exitCode);
            Assert.Contains($"'{archivePath}' does not exist", errors, StringComparison.Ordinal);

            Assert.Equal(Program.FaultExitCode, RunCommand("--root").ExitCode);
            Assert.Equal(Program.FaultExitCode, RunCommand("--root", root, "extra").ExitCode);
            Assert.Equal(Program.FaultExitCode, RunCommand("--file", root).ExitCode);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static (int ExitCode, string Output, string Errors) RunCommand(params string[] args)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();
        int exitCode = Program.Run([HandoffRotateCommand.Name, .. args], output, errors);
        return (exitCode, output.ToString(), errors.ToString());
    }

    private static string Entry(int number)
    {
        return $"## Session {number}: 2026-09-27, Codex\n\nAuthor: Codex\n\n### What this session did, and why\n\n- The work of session {number}.";
    }

    /// <summary>Gives the entries from the newest number down to the oldest, one blank line apart, with a final line end.</summary>
    private static string Entries(int newest, int oldest)
    {
        IEnumerable<string> entries = Enumerable.Range(oldest, newest - oldest + 1).Reverse().Select(Entry);
        return string.Join("\n\n", entries) + "\n";
    }

    private static string RandomEntries(Random random, int newest, int count)
    {
        StringBuilder text = new StringBuilder();
        for (int index = 0; index < count; index++)
        {
            text.Append("## Session ").Append(newest - index).Append(": 2026-09-27, Claude Code\n");
            int lines = random.Next(0, 6);
            for (int line = 0; line < lines; line++)
            {
                text.Append(random.Next(0, 3) == 0 ? "\n" : $"- Line {random.Next()} with ## Session {random.Next(0, 9)}: inside.\n");
            }

            text.Append('\n', random.Next(0, 4));
        }

        return text.ToString();
    }

    private static List<string> TrimmedEntries(string text)
    {
        return HandoffRotateRules.Parse(text, HandoffRotateRules.HandoffPath).Entries.Select(entry => entry.Text.TrimEnd()).ToList();
    }
}
