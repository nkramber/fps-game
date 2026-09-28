using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using IronAbsolution.Tools;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.PackageRun;
using IronAbsolution.Tools.ToolchainCheck;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The pass rule of the timed run of a package against fixture logs (D-89). An exit code of 0 alone
/// is not a pass, and each failure names the log (exit test 4 of PR-10, T-2).
/// </summary>
public sealed class PackageRunRulesTests
{
    private const string LogPath = "/checkout/Game/Saved/Logs/package-run.log";
    private const string PassedLog = "LogTimedRun: Display: Timed run: pass. The map /Game/Maps/L_Test ran for 10 seconds.\n";

    private static readonly TimeSpan Limit = TimeSpan.FromMinutes(5);

    [Fact]
    public void EachCheckPassesOnACleanRun()
    {
        IReadOnlyList<CheckResult> results = PackageRunRules.Evaluate(Facts(0, PassedLog));

        Assert.Equal("Package exit: pass. The package gave the exit code 0.", results[0].Line());
        Assert.Equal($"Success line: pass. The log '{LogPath}' holds the line 'Timed run: pass. The map /Game/Maps/L_Test ran for 10 seconds.'.", results[1].Line());
    }

    [Fact]
    public void AnExitCodeOfZeroWithNoSuccessLineFailsAndNamesTheLog()
    {
        // Exit test 4 of PR-10: a run with no success line fails, and the message names the log.
        IReadOnlyList<CheckResult> results = PackageRunRules.Evaluate(Facts(0, "LogInit: Display: Engine is initialized.\n"));

        Assert.True(results[0].Holds);
        Assert.False(results[1].Holds);
        Assert.Equal(
            $"Success line: fail. The log '{LogPath}' has no line 'Timed run: pass. The map /Game/Maps/L_Test ran for 10 seconds.'. An exit code of 0 alone is not a pass.",
            results[1].Line());
    }

    [Theory]
    [InlineData("LogTimedRun: Display: Timed run: pass. The map /Game/Maps/L_Other ran for 10 seconds.\n")]
    [InlineData("LogTimedRun: Display: Timed run: pass. The map /Game/Maps/L_Test ran for 1 seconds.\n")]
    [InlineData("LogTimedRun: Error: Timed run: fail. The option -TimedRunSeconds= has the value 'abc'.\n")]
    public void ASuccessLineOfAnotherMapOrAnotherTimeFails(string log)
    {
        CheckResult line = PackageRunRules.Evaluate(Facts(0, log))[1];

        Assert.False(line.Holds);
    }

    [Fact]
    public void ANonzeroExitCodeFailsAndNamesTheLog()
    {
        CheckResult exit = PackageRunRules.Evaluate(Facts(139, PassedLog))[0];

        Assert.Equal($"Package exit: fail. The package gave the exit code 139. Read the log '{LogPath}'.", exit.Line());
    }

    [Fact]
    public void ATimeLimitStopFailsAndNamesTheLimitAndTheLog()
    {
        PackageRunFacts facts = new PackageRunFacts(new FileRunResult(-1, TimedOut: true), Limit, ToolOutput.Found(LogPath, "LogInit: Display: Engine is initialized.\n"));

        CheckResult exit = PackageRunRules.Evaluate(facts)[0];

        Assert.Equal($"Package exit: fail. The time limit of 5 minutes stopped the package. Read the log '{LogPath}'.", exit.Line());
    }

    [Fact]
    public void AnAbsentLogFailsWithItsPath()
    {
        PackageRunFacts facts = new PackageRunFacts(new FileRunResult(0, TimedOut: false), Limit, ToolOutput.Absent(LogPath, $"no file '{LogPath}'"));

        CheckResult line = PackageRunRules.Evaluate(facts)[1];

        Assert.Equal($"Success line: fail. No log: no file '{LogPath}'.", line.Line());
    }

    [Fact]
    public void TheSuccessLineMatchesTheFormatOfTheGame()
    {
        // The game writes the line, and the start command looks for it. A change on one side alone
        // makes every run fail, so this test reads the format from the C++ source.
        string source = File.ReadAllText(RepositoryRoot.PathTo("Game/Source/IronAbsolution/Private/TimedRunSubsystem.cpp"));
        Match format = Regex.Match(source, @"TEXT\(""(Timed run: pass\.[^""]*)""\)");
        Assert.True(format.Success, "TimedRunSubsystem.cpp has no format of the success line.");

        string fromGame = format.Groups[1].Value
            .Replace("%s", PackageRunRules.TestMapPackage, StringComparison.Ordinal)
            .Replace("%d", PackageRunRules.RunSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Equal(fromGame, PackageRunRules.SuccessLine(PackageRunRules.TestMapPackage, PackageRunRules.RunSeconds));
    }

    private static PackageRunFacts Facts(int exitCode, string log)
    {
        return new PackageRunFacts(new FileRunResult(exitCode, TimedOut: false), Limit, ToolOutput.Found(LogPath, log));
    }
}
