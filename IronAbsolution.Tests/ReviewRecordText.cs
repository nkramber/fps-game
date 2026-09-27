using System;
using System.Globalization;
using System.Text;

namespace IronAbsolution.Tests;

/// <summary>Builds the text of a review record in the skeleton of the pr-review skill, for the tests of the review rules.</summary>
public static class ReviewRecordText
{
    /// <summary>Builds a record with the Identity list, the findings, and one verdict.</summary>
    /// <param name="head">The hash of the `- Head:` line.</param>
    /// <param name="verdict">The verdict name, or any text in its place.</param>
    /// <param name="findings">Each finding, as <see cref="Finding"/> gives it. None gives "No finding."</param>
    /// <returns>The record text.</returns>
    public static string Record(string head, string verdict, params string[] findings)
    {
        ArgumentNullException.ThrowIfNull(findings);

        StringBuilder text = new StringBuilder();
        text.Append("# PR-4 review\n\nDate: 2026-09-27\n\n## Identity\n\n- PR: 4\n");
        text.Append(CultureInfo.InvariantCulture, $"- Head: `{head}`\n\n## Findings\n\n");
        text.Append(findings.Length == 0 ? "No finding.\n" : string.Join("\n", findings));
        text.Append(CultureInfo.InvariantCulture, $"\n## Out of scope\n\nNone.\n\n## Earlier verdicts\n\nNone.\n\n## Verdict\n\n**{verdict}.** This verdict applies to head `{head}`.\n");
        return text.ToString();
    }

    /// <summary>Builds one finding in the format of the pr-review skill. No head gives no `Open at:` line.</summary>
    /// <param name="id">The finding id, such as `P1-1`.</param>
    /// <param name="status">The status text before the period.</param>
    /// <param name="openAt">Each head of the `Open at:` line.</param>
    /// <returns>The finding text.</returns>
    public static string Finding(string id, string status, params string[] openAt)
    {
        ArgumentNullException.ThrowIfNull(openAt);

        StringBuilder text = new StringBuilder();
        text.Append(CultureInfo.InvariantCulture, $"### {id}: A defect\n\nStatus: {status}.\n\n");
        if (openAt.Length > 0)
        {
            text.Append("Open at: ");
            text.Append(string.Join(", ", Array.ConvertAll(openAt, head => $"`{head}`")));
            text.Append(".\n\n");
        }

        text.Append("File: `a.cs:1`.\n");
        return text.ToString();
    }
}
