using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>
/// One finding of a review record: the id, the severity, the text of its status line, and each
/// effective head at which a review round found it open (D-14). The status line has one of the
/// forms of <see cref="ReviewFindings.StatusForms"/>, and the parser refuses any other status.
/// </summary>
/// <param name="Id">The stable id of the finding, such as `P1-1`.</param>
/// <param name="Severity">The severity number, from 0 to 3.</param>
/// <param name="Status">The text after `Status:`.</param>
/// <param name="OpenAt">Each hash in backticks on the `Open at:` line, in order.</param>
public sealed record ReviewFinding(string Id, int Severity, string Status, IReadOnlyList<string> OpenAt)
{
    /// <summary>Gets a value that tells whether the status is open.</summary>
    public bool IsOpen => this.Status.StartsWith(ReviewFindings.OpenStatus, StringComparison.Ordinal);
}

/// <summary>Reads the findings from the `## Findings` section of a review record.</summary>
public static partial class ReviewFindings
{
    /// <summary>The heading of the section that holds the findings.</summary>
    public const string SectionHeading = "## Findings";

    /// <summary>The start of the status line of a finding.</summary>
    public const string StatusPrefix = "Status:";

    /// <summary>The start of the line that lists the heads of each open round.</summary>
    public const string OpenAtPrefix = "Open at:";

    /// <summary>The status of an open finding.</summary>
    public const string OpenStatus = "open";

    /// <summary>
    /// The complete forms of a status. A closed status names its revision or its decision, so a
    /// status such as `fixed.` never closes a finding with no evidence.
    /// </summary>
    public const string StatusForms = "open. | fixed in `<sha>`. | accepted risk, D-<n>. | withdrawn.";

    /// <summary>The highest severity number: P0 to P3.</summary>
    public const int HighestSeverity = 3;

    /// <summary>
    /// Reads every finding under the section heading, in the order of the file. A finding
    /// heading is `### P&lt;severity&gt;-&lt;index&gt;: &lt;title&gt;`, with a severity from P0 to P3.
    /// </summary>
    /// <param name="recordText">The full text of the review record.</param>
    /// <returns>Each finding. A finding with no `Open at:` line has an empty list, and the outcome rules judge that.</returns>
    /// <exception cref="FormatException">
    /// The record has no Findings section, a line of three or more hashes in the section is no
    /// finding heading, a severity is above P3, or a finding has no status line or a status of
    /// another form. A skipped finding would block nothing in silence (T-2).
    /// </exception>
    public static IReadOnlyList<ReviewFinding> Parse(string recordText)
    {
        ArgumentNullException.ThrowIfNull(recordText);

        string[] lines = recordText.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        int start = Array.FindIndex(lines, static line => line.TrimEnd() == SectionHeading);
        if (start < 0)
        {
            throw new FormatException($"The review record has no '{SectionHeading}' section.");
        }

        List<ReviewFinding> findings = [];
        string? id = null;
        int severity = 0;
        string? status = null;
        List<string> openAt = [];
        for (int index = start + 1; index < lines.Length && !lines[index].StartsWith("## ", StringComparison.Ordinal); index++)
        {
            string line = lines[index].Trim();
            Match heading = FindingHeading().Match(line);

            // A line of three or more hashes is a heading of level 3 or deeper, with a space or without one.
            if (!heading.Success && line.StartsWith("###", StringComparison.Ordinal))
            {
                throw new FormatException($"The heading '{line}' in the '{SectionHeading}' section is not a finding heading of the form '### P<0 to {HighestSeverity}>-<index>: <title>'.");
            }

            if (heading.Success)
            {
                AddFinding(findings, id, severity, status, openAt);
                id = heading.Groups["id"].Value;
                severity = int.Parse(heading.Groups["severity"].Value, CultureInfo.InvariantCulture);
                if (severity > HighestSeverity)
                {
                    throw new FormatException($"The finding {id} has the severity P{severity}, and a severity is P0 to P{HighestSeverity}.");
                }

                status = null;
                openAt = [];
                continue;
            }

            if (id is null)
            {
                continue;
            }

            if (status is null && line.StartsWith(StatusPrefix, StringComparison.Ordinal))
            {
                status = line[StatusPrefix.Length..].Trim();
            }
            else if (openAt.Count == 0 && line.StartsWith(OpenAtPrefix, StringComparison.Ordinal))
            {
                openAt = ReadHashes(line[OpenAtPrefix.Length..]);
            }
        }

        AddFinding(findings, id, severity, status, openAt);
        return findings;
    }

    private static void AddFinding(List<ReviewFinding> findings, string? id, int severity, string? status, List<string> openAt)
    {
        if (id is null)
        {
            return;
        }

        if (status is null)
        {
            throw new FormatException($"The finding {id} in the '{SectionHeading}' section has no '{StatusPrefix}' line.");
        }

        // An unknown status is an error, because a closed default lets a finding such as "Open" block nothing.
        if (!StatusForm().IsMatch(status))
        {
            throw new FormatException($"The finding {id} has the status '{status}', and a status is one of the forms: {StatusForms}");
        }

        findings.Add(new ReviewFinding(id, severity, status, openAt));
    }

    /// <summary>Gives each text in backticks on the line, in order.</summary>
    private static List<string> ReadHashes(string text)
    {
        List<string> hashes = [];
        foreach (Match match in BacktickText().Matches(text))
        {
            hashes.Add(match.Groups["text"].Value.Trim());
        }

        return hashes;
    }

    [GeneratedRegex(@"^### (?<id>P(?<severity>[0-9]{1,3})-[0-9]+):")]
    private static partial Regex FindingHeading();

    [GeneratedRegex("`(?<text>[^`]+)`")]
    private static partial Regex BacktickText();

    [GeneratedRegex(@"^(open|fixed in `[0-9a-f]{7,40}`|accepted risk, D-[0-9]+|withdrawn)\.$")]
    private static partial Regex StatusForm();
}
