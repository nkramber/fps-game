using System;
using System.Collections.Generic;

namespace IronAbsolution.Tools.SteCheck;

/// <summary>
/// The rule of the instructions files. AGENTS 1: `CLAUDE.md` and `AGENTS.md` stay identical
/// (D-12). The ste-check job runs on every PR, so a documents PR that edits one file alone
/// gets a red check.
/// </summary>
public static class AgentFileRules
{
    /// <summary>The instructions file that one provider reads (D-12).</summary>
    public const string ClaudePath = "CLAUDE.md";

    /// <summary>The instructions file that the other provider reads (D-12).</summary>
    public const string AgentsPath = "AGENTS.md";

    /// <summary>The id of the rule, as the `ste-writing` skill names it.</summary>
    public const string RuleId = "AGENTS 1";

    /// <summary>Reads the two instructions files, and gives one finding when they differ.</summary>
    /// <param name="documents">The file set of the checkout.</param>
    /// <returns>No finding, or one finding at the first line that differs.</returns>
    /// <exception cref="InvalidOperationException">One of the two files is absent.</exception>
    public static IReadOnlyList<Finding> Check(DocumentSet documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        foreach (string path in new[] { ClaudePath, AgentsPath })
        {
            if (!documents.Holds(path))
            {
                throw new InvalidOperationException($"The instructions file '{path}' is absent (D-12, T-2).");
            }
        }

        IReadOnlyList<string> claudeLines = documents.ReadLines(ClaudePath);
        IReadOnlyList<string> agentsLines = documents.ReadLines(AgentsPath);
        int shared = Math.Min(claudeLines.Count, agentsLines.Count);
        for (int index = 0; index < shared; index++)
        {
            if (!string.Equals(claudeLines[index], agentsLines[index], StringComparison.Ordinal))
            {
                return [DifferenceAt(index + 1, $"line {index + 1} differs from line {index + 1} of `{ClaudePath}`")];
            }
        }

        if (claudeLines.Count != agentsLines.Count)
        {
            return [DifferenceAt(
                shared + 1,
                $"the file holds {agentsLines.Count} lines, and `{ClaudePath}` holds {claudeLines.Count}")];
        }

        // A read of lines drops each line end, so two files that differ in a line end alone, or
        // in the end of the last line, pass the loop above. "Identical" means each byte, so this
        // rule compares each byte too.
        if (!documents.ReadBytes(ClaudePath).AsSpan().SequenceEqual(documents.ReadBytes(AgentsPath)))
        {
            return [DifferenceAt(
                Math.Max(agentsLines.Count, 1),
                $"each line matches `{ClaudePath}`, and the bytes differ in a line end or in the end of the file")];
        }

        return [];
    }

    private static Finding DifferenceAt(int line, string detail)
    {
        return new Finding(
            AgentsPath,
            line,
            RuleId,
            $"{detail}. Edit both files together, because they stay identical (D-12)");
    }
}
