using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace IronAbsolution.Tools.HandoffRotate;

/// <summary>One session entry: its number, and its text from its heading to the next heading.</summary>
/// <param name="Number">The session number of the heading.</param>
/// <param name="Text">The text of the entry, with its heading.</param>
public sealed record HandoffEntry(int Number, string Text);

/// <summary>A handoff file, split into the text before the first entry and the entries in file order.</summary>
/// <param name="Preamble">The text before the first heading. The handoff has none, and the archive has its title.</param>
/// <param name="Entries">Each entry, in the order of the file.</param>
public sealed record HandoffFile(string Preamble, IReadOnlyList<HandoffEntry> Entries);

/// <summary>The result of one rotation.</summary>
/// <param name="Handoff">The new text of the handoff.</param>
/// <param name="Archive">The new text of the archive.</param>
/// <param name="Moved">The number of each entry that moved to the archive, newest first.</param>
/// <param name="Reordered">The number of each entry that the sort put back in its place, in the old file order (D-58).</param>
/// <param name="NextSession">The number of the next session: the highest number plus one.</param>
public sealed record HandoffRotation(string Handoff, string Archive, IReadOnlyList<int> Moved, IReadOnlyList<int> Reordered, int NextSession);

/// <summary>
/// The rotation of the session handoff (D-20, D-58). The handoff keeps the 10 newest entries,
/// newest first. Each older entry moves to the top of the archive with its text intact.
/// </summary>
/// <remarks>
/// An entry that sits under an older one goes back to its place by number, and the rotation
/// names it (D-58). This sort is not a silent repair: the command prints each number (T-2).
/// Two entries of one number stay an error, and nothing moves, because a number is the
/// identity of a session, and no tool can know which of the two is newer (L-2).
/// </remarks>
public static class HandoffRotateRules
{
    /// <summary>The number of entries that the handoff keeps (D-20).</summary>
    public const int KeepCount = 10;

    /// <summary>The handoff file.</summary>
    public const string HandoffPath = "docs/session-handoff.md";

    /// <summary>The archive file. Its entries run newest first below its title.</summary>
    public const string ArchivePath = "docs/session-handoff-archive.md";

    // The same heading form as the session number check of ste-check (D-20). A heading of
    // another level is a finding of that check, so this parse does not read it.
    private static readonly Regex SessionHeading = new Regex(
        @"^## Session (\d+):", RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>Splits a handoff text at each line that starts with `## Session N:`.</summary>
    /// <param name="text">The text of the handoff or of the archive.</param>
    /// <param name="path">The path of the file, for the message of a fault.</param>
    /// <returns>The text before the first entry, and each entry in file order.</returns>
    /// <exception cref="InvalidOperationException">A session number is too large for an int. The message names the file and the number.</exception>
    public static HandoffFile Parse(string text, string path)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrEmpty(path);

        MatchCollection matches = SessionHeading.Matches(text);
        List<HandoffEntry> entries = [];
        for (int index = 0; index < matches.Count; index++)
        {
            Match match = matches[index];
            int end = index + 1 < matches.Count ? matches[index + 1].Index : text.Length;
            string digits = match.Groups[1].Value;
            if (!int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out int number))
            {
                throw new InvalidOperationException(
                    $"'{path}' has the heading 'Session {digits}', and that number is larger than {int.MaxValue} (T-2). Give the entry its correct session number.");
            }

            entries.Add(new HandoffEntry(number, text[match.Index..end]));
        }

        string preamble = matches.Count == 0 ? text : text[..matches[0].Index];
        return new HandoffFile(preamble, entries);
    }

    /// <summary>
    /// Moves each entry after the tenth to the top of the archive. When the handoff holds 10
    /// entries or fewer in order, both texts come back unchanged. The entries of each written
    /// file have one blank line between them, and the file ends with one line end. The archive
    /// text below its old first entry never changes.
    /// </summary>
    /// <param name="handoffText">The text of the handoff.</param>
    /// <param name="archiveText">The text of the archive.</param>
    /// <returns>The two new texts, the moved and the sorted entries, and the next session number.</returns>
    /// <exception cref="InvalidOperationException">
    /// The handoff has no entry, it holds one number two times, its newest number has no next
    /// number, or the archive top is not older than each moved entry. The message names the
    /// file and the number.
    /// </exception>
    public static HandoffRotation Rotate(string handoffText, string archiveText)
    {
        ArgumentNullException.ThrowIfNull(handoffText);
        ArgumentNullException.ThrowIfNull(archiveText);

        HandoffFile handoff = Parse(handoffText, HandoffPath);
        if (handoff.Entries.Count == 0)
        {
            throw new InvalidOperationException(
                $"'{HandoffPath}' has no '## Session <number>:' heading, so the newest session is unknown (T-2).");
        }

        CheckNoDuplicate(handoff.Entries);
        IReadOnlyList<HandoffEntry> ordered = SortNewestFirst(handoff.Entries, out IReadOnlyList<int> reordered);
        if (ordered[0].Number == int.MaxValue)
        {
            // The next number would wrap to a negative number (T-2).
            throw new InvalidOperationException(
                $"'{HandoffPath}' has Session {int.MaxValue}, the largest number that the command holds, so no next session number exists. Give the entry its correct session number.");
        }

        int nextSession = ordered[0].Number + 1;
        if (ordered.Count <= KeepCount)
        {
            string sorted = reordered.Count == 0 ? handoffText : handoff.Preamble + JoinEntries(ordered) + "\n";
            return new HandoffRotation(sorted, archiveText, [], reordered, nextSession);
        }

        List<HandoffEntry> kept = ordered.Take(KeepCount).ToList();
        List<HandoffEntry> moved = ordered.Skip(KeepCount).ToList();
        HandoffFile archive = Parse(archiveText, ArchivePath);
        if (archive.Entries.Count > 0 && archive.Entries[0].Number >= moved[^1].Number)
        {
            // A rotation that stopped after the write of the archive leaves this state.
            throw new InvalidOperationException(
                $"'{ArchivePath}' starts with Session {archive.Entries[0].Number}, and the rotation moves Session {moved[^1].Number} to it. " +
                "The archive top must be older than each moved entry. " +
                $"Remove the copies of the moved entries from the top of '{ArchivePath}', then run the command again.");
        }

        string newHandoff = handoff.Preamble + JoinEntries(kept) + "\n";
        string movedText = JoinEntries(moved);
        string newArchive;
        if (archive.Entries.Count == 0)
        {
            newArchive = WithBlankLineEnd(archiveText) + movedText + "\n";
        }
        else
        {
            string oldEntries = archiveText[archive.Preamble.Length..];
            newArchive = WithBlankLineEnd(archive.Preamble) + movedText + "\n\n" + oldEntries;
        }

        return new HandoffRotation(newHandoff, newArchive, moved.Select(entry => entry.Number).ToList(), reordered, nextSession);
    }

    /// <summary>
    /// Gives the entries by number, newest first, each with its text intact. It names each
    /// entry whose place the sort changed, in the old file order (D-58). A swap of two entries
    /// names both.
    /// </summary>
    /// <param name="entries">The entries in file order.</param>
    /// <param name="reordered">The number of each entry that changed place.</param>
    /// <returns>The entries, newest first.</returns>
    public static IReadOnlyList<HandoffEntry> SortNewestFirst(IReadOnlyList<HandoffEntry> entries, out IReadOnlyList<int> reordered)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<HandoffEntry> ordered = entries.OrderByDescending(entry => entry.Number).ToList();
        List<int> changedPlace = [];
        for (int index = 0; index < entries.Count; index++)
        {
            if (entries[index].Number != ordered[index].Number)
            {
                changedPlace.Add(entries[index].Number);
            }
        }

        reordered = changedPlace;
        return ordered;
    }

    /// <summary>Stops a file that holds one session number two times (L-2, T-2).</summary>
    private static void CheckNoDuplicate(IReadOnlyList<HandoffEntry> entries)
    {
        HashSet<int> seen = [];
        foreach (HandoffEntry entry in entries)
        {
            if (!seen.Add(entry.Number))
            {
                throw new InvalidOperationException(
                    $"'{HandoffPath}' holds Session {entry.Number} two times (L-2). Give the newer entry the next free number, then run the command again.");
            }
        }
    }

    private static string JoinEntries(IEnumerable<HandoffEntry> entries)
    {
        return string.Join("\n\n", entries.Select(entry => entry.Text.TrimEnd()));
    }

    private static string WithBlankLineEnd(string text)
    {
        string trimmed = text.TrimEnd();
        return trimmed.Length == 0 ? string.Empty : trimmed + "\n\n";
    }
}
