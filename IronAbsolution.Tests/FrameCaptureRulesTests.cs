using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IronAbsolution.Tools;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.FrameCapture;
using IronAbsolution.Tools.ToolchainCheck;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The pass rule of the frame-time capture of M-3 against fixture captures (D-137). Each bad capture
/// fails with the path and the cause (exit test 3 of PR-22). A value over the budget fails, and the
/// message names the value and the budget (exit test 4 of PR-22).
/// </summary>
public sealed class FrameCaptureRulesTests
{
    private const string LogPath = "/checkout/Game/Saved/Logs/frame-capture.log";
    private const string CsvPath = "/checkout/Game/Saved/Logs/frame-capture.csv";
    private const string PassedLog = "LogFrameTimeCapture: Display: Frame-time capture: pass. The map /Game/Maps/L_Gym wrote the file /checkout/Game/Saved/Logs/frame-capture.csv.\n";

    private static readonly TimeSpan Limit = TimeSpan.FromMinutes(5);

    [Fact]
    public void EachCheckPassesOnACaptureInsideTheBudget()
    {
        IReadOnlyList<CheckResult> results = FrameCaptureRules.Evaluate(Facts(0, PassedLog, FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(200, 4.0))));

        Assert.Equal(
            [
                "Package exit: pass. The package gave the exit code 0.",
                $"Success line: pass. The log '{LogPath}' holds the line 'Frame-time capture: pass. The map /Game/Maps/L_Gym wrote the file {CsvPath}.'.",
                $"Capture file: pass. The capture file '{CsvPath}' holds 200 frames in 0.80 seconds.",
                "Capture settings: pass. A Development package, WindowedFullscreen at 2560x1440, with VSync off and no frame-rate cap, over 4 views.",
                "Mean frame time: pass. The mean of 200 frames is 4.00 ms, inside the budget of 8.33 ms (120 fps, D-32).",
                "99th percentile frame time: pass. The 99th percentile of 200 frames is 4.00 ms, inside the budget of 8.33 ms (120 fps, D-32).",
            ],
            results.Select(result => result.Line()));
    }

    [Fact]
    public void AMeanOverTheBudgetFailsAndNamesTheValueAndTheBudget()
    {
        // Exit test 4 of PR-22.
        IReadOnlyList<CheckResult> results = FrameCaptureRules.Evaluate(Facts(0, PassedLog, FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(200, 9.5))));

        CheckResult mean = results.Single(result => result.Check == "Mean frame time");
        Assert.False(mean.Holds);
        Assert.Equal("Mean frame time: fail. The mean of 200 frames is 9.50 ms, over the budget of 8.33 ms (120 fps, D-32).", mean.Line());
    }

    [Fact]
    public void HitchesOverTheBudgetFailTheNinetyNinthPercentileAloneWhenTheMeanIsInside()
    {
        // 97 fast frames and 3 hitches: the mean stays inside, and the 99th percentile shows the hitches.
        IEnumerable<double> frames = FrameCaptureFixtures.Frames(97, 4.0).Concat(FrameCaptureFixtures.Frames(3, 40.0));

        IReadOnlyList<CheckResult> results = FrameCaptureRules.Evaluate(Facts(0, PassedLog, FrameCaptureFixtures.Capture(frames)));

        Assert.True(results.Single(result => result.Check == "Mean frame time").Holds);
        CheckResult percentile = results.Single(result => result.Check == "99th percentile frame time");
        Assert.Equal("99th percentile frame time: fail. The 99th percentile of 100 frames is 40.00 ms, over the budget of 8.33 ms (120 fps, D-32).", percentile.Line());
    }

    [Fact]
    public void AValueAtTheBudgetPasses()
    {
        IReadOnlyList<CheckResult> results = FrameCaptureRules.Evaluate(Facts(0, PassedLog, FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(120, 1000.0 / 120.0))));

        Assert.All(results, result => Assert.True(result.Holds, result.Line()));
    }

    [Fact]
    public void AnAbsentCaptureFileFailsWithThePath()
    {
        // Exit test 3 of PR-22: an absent file.
        IReadOnlyList<CheckResult> results = FrameCaptureRules.Evaluate(Facts(0, PassedLog, csv: null));

        Assert.Equal(3, results.Count);
        Assert.Equal($"Capture file: fail. No capture file: no file '{CsvPath}'.", results[2].Line());
    }

    [Fact]
    public void AnEmptyCaptureFileFailsWithThePath()
    {
        // Exit test 3 of PR-22: an empty file.
        IReadOnlyList<CheckResult> results = FrameCaptureRules.Evaluate(Facts(0, PassedLog, string.Empty));

        Assert.Equal($"Capture file: fail. The capture file '{CsvPath}' is empty.", results[2].Line());
    }

    [Fact]
    public void ACaptureWithTooFewFramesFailsWithThePathAndTheCount()
    {
        // Exit test 3 of PR-22: a file that is too short.
        IReadOnlyList<CheckResult> results = FrameCaptureRules.Evaluate(Facts(0, PassedLog, FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(99, 4.0))));

        Assert.Equal(3, results.Count);
        Assert.Equal($"Capture file: fail. The capture file '{CsvPath}' holds 99 frames, and a capture needs 100 or more.", results[2].Line());
    }

    [Fact]
    public void ACaptureThatDidNotEndFailsWithThePathAndTheCause()
    {
        string text = $"{FrameCaptureFixtures.Header}\n,4.0000,2.0000,1.5000,3.0000\n";

        IReadOnlyList<CheckResult> results = FrameCaptureRules.Evaluate(Facts(0, PassedLog, text));

        Assert.Equal($"Capture file: fail. The capture file did not read: the file '{CsvPath}' has no metadata on its last line, line 2. The capture did not end.", results[2].Line());
    }

    [Fact]
    public void EachSettingOtherThanTheSettingsOfTheMethodFails()
    {
        List<(string Key, string Value)> metadata = FrameCaptureFixtures.GoodMetadata()
            .Select(pair => pair.Key switch
            {
                "config" => (pair.Key, "Shipping"),
                "ironabsolution.windowmode" => (pair.Key, "Windowed"),
                "ironabsolution.viewportheight" => (pair.Key, "720"),
                _ => pair,
            })
            .Where(pair => pair.Key != "ironabsolution.vsync")
            .ToList();

        IReadOnlyList<CheckResult> results = FrameCaptureRules.Evaluate(Facts(0, PassedLog, FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(200, 4.0), metadata)));

        CheckResult settings = results.Single(result => result.Check == "Capture settings");
        Assert.Equal(
            $"Capture settings: fail. The metadata of '{CsvPath}' does not match D-137. "
            + "The key 'Config' has 'Shipping', and D-137 needs 'Development'. "
            + "The key 'IronAbsolution.WindowMode' has 'Windowed', and D-137 needs 'WindowedFullscreen'. "
            + "The key 'IronAbsolution.ViewportHeight' has '720', and D-137 needs '1440'. "
            + "The key 'IronAbsolution.VSync' is absent, and D-137 needs '0'.",
            settings.Line());
    }

    [Fact]
    public void AnExitCodeOfZeroWithNoSuccessLineFailsAndNamesTheLog()
    {
        IReadOnlyList<CheckResult> results = FrameCaptureRules.Evaluate(Facts(0, "LogFrameTimeCapture: Error: Frame-time capture: fail. The map has no AFrameTimeView.\n", FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(200, 4.0))));

        Assert.Equal(
            $"Success line: fail. The log '{LogPath}' has no line 'Frame-time capture: pass. The map /Game/Maps/L_Gym wrote the file {CsvPath}.'. Read the lines of LogFrameTimeCapture in it.",
            results[1].Line());
    }

    [Fact]
    public void ANonzeroExitCodeAndATimeoutFailAndNameTheLog()
    {
        string csv = FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(200, 4.0));

        CheckResult crash = FrameCaptureRules.Evaluate(Facts(1, PassedLog, csv))[0];
        CheckResult timeout = FrameCaptureRules.Evaluate(new FrameCaptureFacts(new FileRunResult(-1, TimedOut: true), Limit, ToolOutput.Found(LogPath, PassedLog), ToolOutput.Found(CsvPath, csv)))[0];

        Assert.Equal($"Package exit: fail. The package gave the exit code 1. Read the log '{LogPath}'.", crash.Line());
        Assert.Equal($"Package exit: fail. The time limit of 5 minutes stopped the package. Read the log '{LogPath}'.", timeout.Line());
    }

    [Fact]
    public void TheNinetyNinthPercentileIsTheNearestRank()
    {
        // Rank ceiling(0.99 × 200) = 198 of the sorted values, so the two slowest frames drop out.
        List<double> values = Enumerable.Range(1, 200).Select(value => (double)value).Reverse().ToList();

        Assert.Equal(198.0, FrameCaptureRules.Percentile99(values));
        Assert.Equal(7.0, FrameCaptureRules.Percentile99([7.0]));
        Assert.Throws<ArgumentException>(() => FrameCaptureRules.Percentile99([]));
    }

    [Fact]
    public void TheSuccessLineMatchesTheFormatOfTheGame()
    {
        // The game writes the line, and this command looks for it. A change on one side alone fails here.
        string source = File.ReadAllText(RepositoryRoot.PathTo("Game/Source/IronAbsolution/Private/FrameTimeCaptureSubsystem.cpp"));
        Match format = Regex.Match(source, @"TEXT\(""(Frame-time capture: pass\.[^""]*)""\)");
        Assert.True(format.Success, "FrameTimeCaptureSubsystem.cpp has no format of the success line.");

        // The format holds two fields: the map, then the path of the file.
        string[] parts = format.Groups[1].Value.Split("%s");
        Assert.Equal(3, parts.Length);
        string fromGame = parts[0] + FrameCaptureRules.GymMapPackage + parts[1] + CsvPath + parts[2];

        Assert.Equal(fromGame, FrameCaptureRules.SuccessLine(FrameCaptureRules.GymMapPackage, CsvPath));
    }

    [Fact]
    public void TheMetadataKeysAndTheWindowModeMatchTheGame()
    {
        // The game writes each key and the name of the window mode, and this rule reads them.
        string header = File.ReadAllText(RepositoryRoot.PathTo("Game/Source/IronAbsolution/Public/FrameTimeCaptureSubsystem.h"));
        string source = File.ReadAllText(RepositoryRoot.PathTo("Game/Source/IronAbsolution/Private/FrameTimeCaptureSubsystem.cpp"));

        foreach (string key in new[] { "WindowMode", "ViewportWidth", "ViewportHeight", "VSync", "MaxFps", "ViewCount" })
        {
            Assert.Contains($"TEXT(\"IronAbsolution.{key}\")", header, StringComparison.Ordinal);
        }

        Assert.Contains($"return TEXT(\"{FrameCaptureRules.WindowMode}\");", source, StringComparison.Ordinal);
    }

    private static FrameCaptureFacts Facts(int exitCode, string log, string? csv)
    {
        ToolOutput csvOutput = csv is null ? ToolOutput.Absent(CsvPath, $"no file '{CsvPath}'") : ToolOutput.Found(CsvPath, csv);
        return new FrameCaptureFacts(new FileRunResult(exitCode, TimedOut: false), Limit, ToolOutput.Found(LogPath, log), csvOutput);
    }
}
