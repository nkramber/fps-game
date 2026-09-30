using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.PackageRun;
using IronAbsolution.Tools.ToolchainCheck;

namespace IronAbsolution.Tools.FrameCapture;

/// <summary>
/// The `frame-capture` command, the frame-time capture of M-3 (D-137). It starts the package of
/// `run.ps1 package-build` on the gym with the frame-time option, and applies the pass rule of
/// <see cref="FrameCaptureRules"/> to the log and the CSV file. `run.ps1 frame-capture` runs it
/// (D-99). The package opens a game window, so a session asks the owner first (D-96).
/// </summary>
public static class FrameCaptureCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "frame-capture";

    /// <summary>The CSV file of the capture, under the root. The evidence form takes it (D-31).</summary>
    public const string CsvFile = "Game/Saved/Logs/frame-capture.csv";

    /// <summary>The log that the package writes through `-abslog`, under the root. The evidence form takes it (D-31).</summary>
    public const string LogFile = "Game/Saved/Logs/frame-capture.log";

    /// <summary>The file that takes the stdout of the package, under the root.</summary>
    public const string OutputLogFile = "Game/Saved/Logs/frame-capture.stdout.log";

    /// <summary>The file that takes the stderr of the package, under the root.</summary>
    public const string ErrorLogFile = "Game/Saved/Logs/frame-capture.stderr.log";

    private const string RootOption = "--root";

    /// <summary>
    /// Gets the time limit of one capture. The capture takes less than one minute, so a package
    /// that does not stop turns into a failure that names the log (T-2).
    /// </summary>
    public static TimeSpan Limit { get; } = TimeSpan.FromMinutes(5);

    /// <summary>Starts the package for a capture, and reports each check.</summary>
    /// <param name="args">The arguments after the command name: `--root &lt;folder&gt;`, the root of the checkout.</param>
    /// <param name="output">The writer that takes each line of a check that passes.</param>
    /// <param name="errors">The writer that takes each line of a check that fails, and each fault.</param>
    /// <returns>0 when each check passes, or 1.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = Path.GetFullPath(options.ValueOr(RootOption, "."));
        return RunCapture(root, Limit, output, errors);
    }

    /// <summary>Starts the package, waits for it to stop, then reads the log and the CSV file.</summary>
    /// <param name="root">The full path of the root of the checkout.</param>
    /// <param name="limit">The time limit of the run.</param>
    /// <param name="output">The writer that takes each line of a check that passes.</param>
    /// <param name="errors">The writer that takes each line of a check that fails, and each fault.</param>
    /// <returns>0 when each check passes, or 1.</returns>
    public static int RunCapture(string root, TimeSpan limit, TextWriter output, TextWriter errors)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        // The capture uses the package of `package-run` (D-137).
        string package = ToolPaths.UnderFolder(root, PackageRunCommand.PackageProgram);
        if (!File.Exists(package))
        {
            errors.WriteLine($"{Name}: no file '{package}'. Run `run.ps1 package-build` first, and --root names the checkout.");
            return Program.FaultExitCode;
        }

        string csvPath = ToolPaths.UnderFolder(root, CsvFile);
        string logPath = ToolPaths.UnderFolder(root, LogFile);
        string outputLogPath = ToolPaths.UnderFolder(root, OutputLogFile);
        string errorLogPath = ToolPaths.UnderFolder(root, ErrorLogFile);
        try
        {
            RemoveOldFiles(csvPath, logPath, outputLogPath, errorLogPath);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            errors.WriteLine($"{Name}: the files of the last capture did not go away, so a new capture cannot prove a pass: {fault.Message}");
            return Program.FaultExitCode;
        }

        output.WriteLine($"{Name}: capture the frame time of {FrameCaptureRules.GymMapPackage}, with a limit of {limit.TotalMinutes} minutes. The CSV file goes to '{csvPath}', and the log goes to '{logPath}'.");
        FileRunResult run;
        try
        {
            run = ExternalProcess.RunToFiles(package, PackageArguments(csvPath, logPath), root, outputLogPath, errorLogPath, [], limit);
        }
        catch (InvalidOperationException fault)
        {
            errors.WriteLine($"{Name}: {fault.Message}");
            return Program.FaultExitCode;
        }

        FrameCaptureFacts facts = new FrameCaptureFacts(run, limit, ReadFile(logPath), ReadFile(csvPath));
        return CheckReport.Write(Name, FrameCaptureRules.Evaluate(facts), output, errors);
    }

    /// <summary>Gives each argument of the capture.</summary>
    /// <param name="csvPath">The full path of the CSV file that the package writes.</param>
    /// <param name="logPath">The full path of the log that the package writes.</param>
    /// <returns>The arguments, in order.</returns>
    public static IReadOnlyList<string> PackageArguments(string csvPath, string logPath)
    {
        // The first argument names the gym. No window option: the game then runs borderless
        // fullscreen at the resolution of the desktop, the mode of D-137 and D-138.
        return
        [
            FrameCaptureRules.GymMapPackage,
            $"-FrameTimeCsv={csvPath}",
            "-unattended",
            $"-abslog={logPath}",
        ];
    }

    /// <summary>
    /// Removes the files of the last capture, so a capture that writes no file cannot pass on an old one (T-2).
    /// </summary>
    private static void RemoveOldFiles(string csvPath, string logPath, string outputLogPath, string errorLogPath)
    {
        // File.Delete fails when the folder is absent, so the folder comes first.
        string? folder = Path.GetDirectoryName(csvPath);
        if (folder is null)
        {
            throw new IOException($"the path '{csvPath}' has no folder");
        }

        Directory.CreateDirectory(folder);
        File.Delete(csvPath);
        File.Delete(logPath);
        File.Delete(outputLogPath);
        File.Delete(errorLogPath);
    }

    private static ToolOutput ReadFile(string path)
    {
        if (!File.Exists(path))
        {
            return ToolOutput.Absent(path, $"no file '{path}'");
        }

        try
        {
            return ToolOutput.Found(path, File.ReadAllText(path));
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            return ToolOutput.Absent(path, $"the file '{path}' did not read: {fault.Message}");
        }
    }
}
