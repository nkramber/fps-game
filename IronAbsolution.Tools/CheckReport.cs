using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IronAbsolution.Tools;

/// <summary>The result of one check of an engine run: its name, its state, and what it saw.</summary>
/// <param name="Check">The name of the check, such as `Success line`.</param>
/// <param name="Holds">True when the check passes.</param>
/// <param name="Detail">What the check saw, with the path of each file that it read (T-2).</param>
public sealed record CheckResult(string Check, bool Holds, string Detail)
{
    /// <summary>Gives the one report line of the check.</summary>
    /// <returns>The line, such as `Editor exit: pass. The editor gave the exit code 0.`</returns>
    public string Line()
    {
        string state = this.Holds ? "pass" : "fail";
        return $"{this.Check}: {state}. {this.Detail.TrimEnd('.')}.";
    }
}

/// <summary>
/// Writes the checks of one engine run: `editor-test` (D-71) and `package-run` (D-89). Each line
/// starts with the name of the command, and the total comes last.
/// </summary>
public static class CheckReport
{
    /// <summary>Writes one line for each check and the total.</summary>
    /// <param name="command">The name of the command, such as `editor-test`.</param>
    /// <param name="results">The result of each check.</param>
    /// <param name="output">The writer that takes each line of a check that passes, and the total when each check passes.</param>
    /// <param name="errors">The writer that takes each line of a check that fails, and the total when a check fails.</param>
    /// <returns>0 when each check passes, or 1.</returns>
    public static int Write(string command, IReadOnlyList<CheckResult> results, TextWriter output, TextWriter errors)
    {
        ArgumentException.ThrowIfNullOrEmpty(command);
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        foreach (CheckResult result in results)
        {
            TextWriter writer = result.Holds ? output : errors;
            writer.WriteLine($"{command}: {result.Line()}");
        }

        int failed = results.Count(result => !result.Holds);
        if (failed == 0)
        {
            output.WriteLine($"{command}: pass. Each of the {results.Count} checks passes.");
            return 0;
        }

        errors.WriteLine($"{command}: fail. {failed} of the {results.Count} checks fail.");
        return Program.FaultExitCode;
    }
}
