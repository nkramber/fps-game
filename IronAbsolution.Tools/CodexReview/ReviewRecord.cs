using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>The two machine-read parts of a review record: the head in the Identity list and the verdict (D-14).</summary>
/// <remarks>
/// <para>
/// The Verdict section starts at the line that is exactly the Verdict heading and ends at the
/// next heading. It holds one verdict name. A heading that starts with the same words, such as
/// `## Earlier verdicts`, is another section. A section with two names is an error that names
/// both, never the first one.
/// </para>
/// <para>
/// The first line of the section with text starts with the verdict name in bold and a period,
/// as the skeleton of the pr-review skill writes it. A name inside other words, such as
/// "Not Ready for owner merge", or a name in another case, is an error. Thus the command never
/// reads a verdict that does not approve as an approval. The parse skips each fenced block, so
/// an example of a record inside a fence is not the record.
/// </para>
/// </remarks>
/// <param name="RecordedHead">The hash in backticks on the `- Head: ` line.</param>
/// <param name="Verdict">The one verdict name of the Verdict section.</param>
public sealed record ReviewRecord(string RecordedHead, string Verdict)
{
    /// <summary>The start of the head line of the Identity list.</summary>
    public const string HeadPrefix = "- Head: ";

    /// <summary>The heading of the Verdict section.</summary>
    public const string VerdictHeading = "## Verdict";

    /// <summary>The verdict that approves the recorded head.</summary>
    public const string ApprovedVerdict = "Ready for owner merge";

    /// <summary>The three verdict names of the pr-review skill.</summary>
    public static readonly IReadOnlyList<string> VerdictNames = [ApprovedVerdict, "Changes required", "Blocked"];

    /// <summary>Gives the path of the review record of a PR. The number is the GitHub number, not the roadmap id.</summary>
    /// <param name="pullRequest">The GitHub number of the PR.</param>
    /// <returns>The path from the root, with forward slashes.</returns>
    public static string FilePath(int pullRequest)
    {
        return $"docs/reviews/pr-{pullRequest.ToString(CultureInfo.InvariantCulture)}.md";
    }

    /// <summary>Reads the two machine-read parts of a review record.</summary>
    /// <param name="text">The full text of the record.</param>
    /// <param name="error">The part that is missing or wrong, or the empty text.</param>
    /// <returns>The record, or null when the text does not hold both parts.</returns>
    public static ReviewRecord? TryParse(string text, out string error)
    {
        ArgumentNullException.ThrowIfNull(text);

        List<string> lines = LinesOutsideFences(text.Replace("\r\n", "\n", StringComparison.Ordinal));
        string? head = FindHead(lines);
        if (head is null)
        {
            error = $"The review record has no line that starts with '{HeadPrefix}' and holds a hash in backticks.";
            return null;
        }

        string? verdict = FindVerdict(lines, out error);
        if (verdict is null)
        {
            return null;
        }

        return new ReviewRecord(head, verdict);
    }

    private static string? FindHead(List<string> lines)
    {
        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (!line.StartsWith(HeadPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            string rest = line[HeadPrefix.Length..];
            int open = rest.IndexOf('`', StringComparison.Ordinal);
            int close = open < 0 ? -1 : rest.IndexOf('`', open + 1);
            if (open < 0 || close <= open + 1)
            {
                return null;
            }

            return rest[(open + 1)..close].Trim();
        }

        return null;
    }

    /// <summary>
    /// Gives the one verdict name of the Verdict section, or null with an error: no section with
    /// the exact heading, no verdict name in it, more than one, or a first line of another form.
    /// </summary>
    private static string? FindVerdict(List<string> lines, out string error)
    {
        int headingLine = lines.FindIndex(static line => line.TrimEnd() == VerdictHeading);
        if (headingLine < 0)
        {
            error = $"The review record has no '{VerdictHeading}' section that names one of: {string.Join(", ", VerdictNames)}.";
            return null;
        }

        StringBuilder section = new StringBuilder();
        string? firstLine = null;
        for (int index = headingLine + 1; index < lines.Count && !lines[index].StartsWith("## ", StringComparison.Ordinal); index++)
        {
            section.Append(lines[index]).Append('\n');
            if (firstLine is null && lines[index].Trim().Length > 0)
            {
                firstLine = lines[index].Trim();
            }
        }

        List<string> found = NamesInOrder(section.ToString());
        if (found.Count == 0)
        {
            error = $"The review record has no '{VerdictHeading}' section that names one of: {string.Join(", ", VerdictNames)}.";
            return null;
        }

        if (found.Count > 1)
        {
            error = $"The '{VerdictHeading}' section of the review record names {found.Count} verdicts: {string.Join(", ", found)}. It must name one. An earlier verdict goes under '## Earlier verdicts'.";
            return null;
        }

        string bold = $"**{found[0]}.**";
        if (firstLine is null || !firstLine.StartsWith(bold, StringComparison.Ordinal))
        {
            error = $"The first line of the '{VerdictHeading}' section must start with the verdict name in bold and a period, as in '**{ApprovedVerdict}.**'. The section names '{found[0]}', and its first line is '{firstLine}'.";
            return null;
        }

        error = string.Empty;
        return found[0];
    }

    /// <summary>Gives every verdict name in the text, in the order of the text.</summary>
    private static List<string> NamesInOrder(string body)
    {
        List<(int At, string Name)> hits = [];
        foreach (string name in VerdictNames)
        {
            int at = body.IndexOf(name, StringComparison.Ordinal);
            while (at >= 0)
            {
                hits.Add((at, name));
                at = body.IndexOf(name, at + name.Length, StringComparison.Ordinal);
            }
        }

        hits.Sort(static (first, second) => first.At.CompareTo(second.At));
        List<string> names = [];
        foreach ((int _, string name) in hits)
        {
            names.Add(name);
        }

        return names;
    }

    /// <summary>
    /// Gives the lines of a text that are outside a fenced block, in order. A run of three or
    /// more backticks or tildes, after no more than three spaces, opens a fence. Only a line of
    /// the same character, with a run at least as long and nothing after it, closes that fence,
    /// as in Markdown. A line with four spaces or a tab first is no fence, and a backtick fence
    /// has no backtick after its run. The method skips the fence lines too.
    /// </summary>
    private static List<string> LinesOutsideFences(string text)
    {
        List<string> lines = [];
        char fenceCharacter = '\0';
        int fenceLength = 0;
        foreach (string line in text.Split('\n'))
        {
            string start = FenceStart(line);
            int run = FenceRun(start);
            if (fenceLength == 0)
            {
                if (run > 0 && !(start[0] == '`' && start[run..].Contains('`', StringComparison.Ordinal)))
                {
                    fenceCharacter = start[0];
                    fenceLength = run;
                    continue;
                }

                lines.Add(line);
                continue;
            }

            if (run >= fenceLength && start[0] == fenceCharacter && start[run..].Trim().Length == 0)
            {
                fenceLength = 0;
            }
        }

        return lines;
    }

    /// <summary>
    /// Gives the line after up to three spaces, where a fence can start. A line with four spaces
    /// or a tab first is an indented code line in Markdown, so it gives the empty text.
    /// </summary>
    private static string FenceStart(string line)
    {
        int spaces = 0;
        while (spaces < line.Length && spaces <= 3 && line[spaces] == ' ')
        {
            spaces++;
        }

        if (spaces > 3 || (spaces < line.Length && line[spaces] == '\t'))
        {
            return string.Empty;
        }

        return line[spaces..];
    }

    /// <summary>Gives the length of the run of backticks or tildes at the start of a line when it is three or more, or zero.</summary>
    private static int FenceRun(string start)
    {
        if (start.Length == 0 || (start[0] != '`' && start[0] != '~'))
        {
            return 0;
        }

        int run = 0;
        while (run < start.Length && start[run] == start[0])
        {
            run++;
        }

        return run >= 3 ? run : 0;
    }
}
