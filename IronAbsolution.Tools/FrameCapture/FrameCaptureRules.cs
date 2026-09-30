using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.ToolchainCheck;

namespace IronAbsolution.Tools.FrameCapture;

/// <summary>
/// Every fact of one frame-time capture, read one time after the package stopped. The rules do no I/O.
/// </summary>
/// <param name="Run">The exit code of the package, and whether the time limit stopped it.</param>
/// <param name="Limit">The time limit of the run.</param>
/// <param name="Log">The text of the log of the package, or the reason that it is absent.</param>
/// <param name="Csv">The text of the CSV file of the capture, or the reason that it is absent.</param>
public sealed record FrameCaptureFacts(FileRunResult Run, TimeSpan Limit, ToolOutput Log, ToolOutput Csv);

/// <summary>
/// The pass rule of the frame-time capture of M-3 (D-137). A pass needs the exit code 0, the success
/// line in the log, a complete CSV file with enough frames, the settings of D-137, and a mean and a
/// 99th percentile of the frame time inside the budget of D-32. Each failure names the file and the
/// cause (T-2). `run.ps1 frame-capture` applies it on the Windows PC (D-99).
/// </summary>
public static class FrameCaptureRules
{
    /// <summary>The package name of the gym, the map of the capture (D-133).</summary>
    public const string GymMapPackage = "/Game/Maps/L_Gym";

    /// <summary>
    /// The least number of frames of a capture. With fewer frames, the 99th percentile is the
    /// slowest frame, and the statistic does not mean what D-137 asks for.
    /// </summary>
    public const int MinimumFrames = 100;

    /// <summary>The frame rate of the budget of D-32.</summary>
    public const int BudgetFramesPerSecond = 120;

    /// <summary>The width of the game viewport of D-32 and D-137, in pixels.</summary>
    public const int ViewportWidth = 2560;

    /// <summary>The height of the game viewport of D-32 and D-137, in pixels.</summary>
    public const int ViewportHeight = 1440;

    /// <summary>The configuration of the package of D-137, as the profiler writes it.</summary>
    public const string Configuration = "Development";

    /// <summary>The window mode of D-137 and D-138, as the game writes it: borderless fullscreen.</summary>
    public const string WindowMode = "WindowedFullscreen";

    /// <summary>The budget of one frame in milliseconds: 1000 / 120, or 8.33 ms (D-32).</summary>
    public const double BudgetMilliseconds = 1000.0 / BudgetFramesPerSecond;

    /// <summary>
    /// Gives the success line of a capture. It is a copy of `UFrameTimeCaptureSubsystem::SuccessLine`
    /// of the game, and a test compares the two.
    /// </summary>
    /// <param name="mapPackage">The package name of the map of the capture.</param>
    /// <param name="csvPath">The full path of the CSV file.</param>
    /// <returns>The line, such as `Frame-time capture: pass. The map /Game/Maps/L_Gym wrote the file C:/x.csv.`</returns>
    public static string SuccessLine(string mapPackage, string csvPath)
    {
        return $"Frame-time capture: pass. The map {mapPackage} wrote the file {csvPath}.";
    }

    /// <summary>
    /// Gives the 99th percentile by the nearest rank: the value at rank ceiling(0.99 × n) of the
    /// values in ascending order.
    /// </summary>
    /// <param name="values">The values, in any order. The list must not be empty.</param>
    /// <returns>The 99th percentile.</returns>
    public static double Percentile99(IReadOnlyList<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0)
        {
            throw new ArgumentException("the 99th percentile needs one value or more", nameof(values));
        }

        List<double> sorted = values.Order().ToList();
        int rank = (int)Math.Ceiling(0.99 * sorted.Count);
        return sorted[rank - 1];
    }

    /// <summary>Applies each check to the facts.</summary>
    /// <param name="facts">The facts of one capture.</param>
    /// <returns>
    /// One result for the exit code, one for the log, and one for the CSV file. When the file is
    /// complete, one result for the settings, one for the mean, and one for the 99th percentile follow.
    /// </returns>
    public static IReadOnlyList<CheckResult> Evaluate(FrameCaptureFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        List<CheckResult> results =
        [
            CheckExitCode(facts.Run, facts.Limit, facts.Log.Source),
            CheckLog(facts.Log, facts.Csv.Source),
        ];

        (CheckResult fileCheck, FrameTimeCsv? capture) = CheckFile(facts.Csv);
        results.Add(fileCheck);
        if (capture is null)
        {
            return results;
        }

        results.Add(CheckSettings(capture.Metadata, facts.Csv.Source));
        double mean = capture.FrameTimes.Average();
        results.Add(CheckBudget("Mean frame time", "mean", mean, capture.FrameTimes.Count));
        results.Add(CheckBudget("99th percentile frame time", "99th percentile", Percentile99(capture.FrameTimes), capture.FrameTimes.Count));
        return results;
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

    private static CheckResult CheckLog(ToolOutput log, string csvPath)
    {
        const string check = "Success line";
        string successLine = SuccessLine(GymMapPackage, csvPath);
        if (log.Text is null)
        {
            return new CheckResult(check, false, $"No log: {log.Absence}");
        }

        if (log.Text.Contains(successLine, StringComparison.Ordinal))
        {
            return new CheckResult(check, true, $"The log '{log.Source}' holds the line '{successLine}'");
        }

        return new CheckResult(check, false, $"The log '{log.Source}' has no line '{successLine}'. Read the lines of LogFrameTimeCapture in it");
    }

    private static (CheckResult Check, FrameTimeCsv? Capture) CheckFile(ToolOutput csv)
    {
        const string check = "Capture file";
        if (csv.Text is null)
        {
            return (new CheckResult(check, false, $"No capture file: {csv.Absence}"), null);
        }

        if (csv.Text.Length == 0)
        {
            return (new CheckResult(check, false, $"The capture file '{csv.Source}' is empty"), null);
        }

        FrameTimeCsv capture;
        try
        {
            capture = FrameTimeCsv.Read(csv.Source, csv.Text);
        }
        catch (FormatException fault)
        {
            return (new CheckResult(check, false, $"The capture file did not read: {fault.Message}"), null);
        }

        if (capture.FrameTimes.Count < MinimumFrames)
        {
            return (new CheckResult(check, false, $"The capture file '{csv.Source}' holds {capture.FrameTimes.Count} frames, and a capture needs {MinimumFrames} or more"), null);
        }

        double seconds = capture.FrameTimes.Sum() / 1000.0;
        return (new CheckResult(check, true, $"The capture file '{csv.Source}' holds {capture.FrameTimes.Count} frames in {Format(seconds)} seconds"), capture);
    }

    private static CheckResult CheckSettings(IReadOnlyDictionary<string, string> metadata, string csvPath)
    {
        const string check = "Capture settings";
        (string Key, string Expected)[] settings =
        [
            ("Config", Configuration),
            ("IronAbsolution.WindowMode", WindowMode),
            ("IronAbsolution.ViewportWidth", ViewportWidth.ToString(CultureInfo.InvariantCulture)),
            ("IronAbsolution.ViewportHeight", ViewportHeight.ToString(CultureInfo.InvariantCulture)),
            ("IronAbsolution.VSync", "0"),
            ("IronAbsolution.MaxFps", "0"),
        ];

        List<string> wrong = new List<string>();
        foreach ((string key, string expected) in settings)
        {
            if (!metadata.TryGetValue(key, out string? found))
            {
                wrong.Add($"The key '{key}' is absent, and D-137 needs '{expected}'");
            }
            else if (!found.Equals(expected, StringComparison.Ordinal))
            {
                wrong.Add($"The key '{key}' has '{found}', and D-137 needs '{expected}'");
            }
        }

        // The content script is the source of the set of views, and the automation test of the gym
        // views checks it (D-134). This rule needs a count, so no capture passes with no views.
        const string viewCountKey = "IronAbsolution.ViewCount";
        int views = 0;
        if (!metadata.TryGetValue(viewCountKey, out string? count))
        {
            wrong.Add($"The key '{viewCountKey}' is absent, and D-137 needs the number of views");
        }
        else if (!int.TryParse(count, NumberStyles.None, CultureInfo.InvariantCulture, out views) || views < 1)
        {
            wrong.Add($"The key '{viewCountKey}' has '{count}', and D-137 needs a whole number of views of 1 or more");
        }

        if (wrong.Count > 0)
        {
            return new CheckResult(check, false, $"The metadata of '{csvPath}' does not match D-137. {string.Join(". ", wrong)}");
        }

        return new CheckResult(check, true, $"A {Configuration} package, {WindowMode} at {ViewportWidth}x{ViewportHeight}, with VSync off and no frame-rate cap, over {views} views");
    }

    private static CheckResult CheckBudget(string check, string statistic, double value, int frames)
    {
        string budget = $"the budget of {Format(BudgetMilliseconds)} ms ({BudgetFramesPerSecond} fps, D-32)";
        if (value <= BudgetMilliseconds)
        {
            return new CheckResult(check, true, $"The {statistic} of {frames} frames is {Format(value)} ms, inside {budget}");
        }

        return new CheckResult(check, false, $"The {statistic} of {frames} frames is {Format(value)} ms, over {budget}");
    }

    private static string Format(double value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }
}
