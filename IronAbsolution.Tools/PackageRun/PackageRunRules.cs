using System;
using System.Collections.Generic;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.ToolchainCheck;

namespace IronAbsolution.Tools.PackageRun;

/// <summary>
/// Every fact of one timed run of a package, read one time after the package stopped. The rules do no I/O.
/// </summary>
/// <param name="Run">The exit code of the package, and whether the time limit stopped it.</param>
/// <param name="Limit">The time limit of the run (D-89).</param>
/// <param name="Log">The text of the log of the package, or the reason that it is absent.</param>
public sealed record PackageRunFacts(FileRunResult Run, TimeSpan Limit, ToolOutput Log);

/// <summary>
/// The pass rule of the timed run of a package (D-89). A pass needs two facts: the exit code 0, and
/// the success line of the timed run in the log. The success line names the test map and the set
/// time, so a package that loads another map or stops early does not pass. Each failure names the
/// log (T-2). `scripts/package-run.ps1` applies the same rule on the Windows PC (D-72).
/// </summary>
public static class PackageRunRules
{
    /// <summary>The set time of the run, in seconds, after the map loads (D-89).</summary>
    public const int RunSeconds = 10;

    /// <summary>The package name of the test map of phase 1 (D-84).</summary>
    public const string TestMapPackage = "/Game/Maps/L_Test";

    /// <summary>
    /// Gives the success line of a timed run. It is a copy of `UTimedRunSubsystem::SuccessLine` of
    /// the game, and a test compares the two.
    /// </summary>
    /// <param name="mapPackage">The package name of the map that ran.</param>
    /// <param name="seconds">The set time of the run.</param>
    /// <returns>The line, such as `Timed run: pass. The map /Game/Maps/L_Test ran for 10 seconds.`</returns>
    public static string SuccessLine(string mapPackage, int seconds)
    {
        return $"Timed run: pass. The map {mapPackage} ran for {seconds} seconds.";
    }

    /// <summary>Applies each check to the facts.</summary>
    /// <param name="facts">The facts of one run.</param>
    /// <returns>One result for the exit code, then one for the log.</returns>
    public static IReadOnlyList<CheckResult> Evaluate(PackageRunFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        return [CheckExitCode(facts.Run, facts.Limit, facts.Log.Source), CheckLog(facts.Log)];
    }

    private static CheckResult CheckExitCode(FileRunResult run, TimeSpan limit, string logPath)
    {
        const string check = "Package exit";
        if (run.TimedOut)
        {
            return new CheckResult(check, false, $"The time limit of {limit.TotalMinutes} minutes stopped the package. Read the log '{logPath}'");
        }

        if (run.ExitCode != 0)
        {
            return new CheckResult(check, false, $"The package gave the exit code {run.ExitCode}. Read the log '{logPath}'");
        }

        return new CheckResult(check, true, "The package gave the exit code 0");
    }

    private static CheckResult CheckLog(ToolOutput log)
    {
        const string check = "Success line";
        string successLine = SuccessLine(TestMapPackage, RunSeconds);
        if (log.Text is null)
        {
            return new CheckResult(check, false, $"No log: {log.Absence}");
        }

        if (log.Text.Contains(successLine, StringComparison.Ordinal))
        {
            return new CheckResult(check, true, $"The log '{log.Source}' holds the line '{successLine}'");
        }

        return new CheckResult(check, false, $"The log '{log.Source}' has no line '{successLine}'. An exit code of 0 alone is not a pass");
    }
}
