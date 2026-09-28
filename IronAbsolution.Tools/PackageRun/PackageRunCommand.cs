using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.ToolchainCheck;

namespace IronAbsolution.Tools.PackageRun;

/// <summary>
/// The `package-run` command of the Mac, the start command of the package (D-89). It starts the
/// package of `make package-build` with the timed-run option, and applies the pass rule of
/// <see cref="PackageRunRules"/>. `make package-run` runs it. The Windows PC runs
/// `scripts/package-run.ps1` instead (D-72).
/// </summary>
public static class PackageRunCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "package-run";

    /// <summary>
    /// Gets the program of the package of this platform, under the root (D-102). The build of the
    /// package writes it there. Windows has one program file, and the Mac has an application bundle.
    /// </summary>
    public static string PackageProgram { get; } = OperatingSystem.IsWindows()
        ? "Game/Saved/Packages/Windows/IronAbsolution/Binaries/Win64/IronAbsolution.exe"
        : "Game/Saved/Packages/Mac/IronAbsolution.app/Contents/MacOS/IronAbsolution";

    /// <summary>The file that takes the stdout of the package, under the root. The evidence form takes it (D-31).</summary>
    public const string LogFile = "Game/Saved/Logs/package-run.log";

    /// <summary>The file that takes the stderr of the package, under the root.</summary>
    public const string ErrorLogFile = "Game/Saved/Logs/package-run.stderr.log";

    private const string RootOption = "--root";

    /// <summary>
    /// Gets the time limit of one run (D-89). A package that does not stop turns into a failure
    /// that names the log (T-2).
    /// </summary>
    public static TimeSpan Limit { get; } = TimeSpan.FromMinutes(5);

    /// <summary>Starts the package for a timed run, and reports each check.</summary>
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
        return RunPackage(root, Limit, output, errors);
    }

    /// <summary>Starts the package, waits for it to stop, then reads the log.</summary>
    /// <param name="root">The full path of the root of the checkout.</param>
    /// <param name="limit">The time limit of the run.</param>
    /// <param name="output">The writer that takes each line of a check that passes.</param>
    /// <param name="errors">The writer that takes each line of a check that fails, and each fault.</param>
    /// <returns>0 when each check passes, or 1.</returns>
    public static int RunPackage(string root, TimeSpan limit, TextWriter output, TextWriter errors)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        string package = ToolPaths.UnderFolder(root, PackageProgram);
        if (!File.Exists(package))
        {
            errors.WriteLine($"{Name}: no file '{package}'. Run `make package-build` first, and --root names the checkout.");
            return Program.FaultExitCode;
        }

        string logPath = ToolPaths.UnderFolder(root, LogFile);
        string errorLogPath = ToolPaths.UnderFolder(root, ErrorLogFile);
        try
        {
            RemoveOldLogs(logPath, errorLogPath);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            errors.WriteLine($"{Name}: the logs of the last run did not go away, so a new run cannot prove a pass: {fault.Message}");
            return Program.FaultExitCode;
        }

        output.WriteLine($"{Name}: run the package for {PackageRunRules.RunSeconds} seconds on the test map, with a limit of {limit.TotalMinutes} minutes. The log goes to '{logPath}'.");
        FileRunResult run;
        try
        {
            run = ExternalProcess.RunToFiles(package, PackageArguments(), root, logPath, errorLogPath, [], limit);
        }
        catch (InvalidOperationException fault)
        {
            errors.WriteLine($"{Name}: {fault.Message}");
            return Program.FaultExitCode;
        }

        PackageRunFacts facts = new PackageRunFacts(run, limit, ReadLog(logPath));
        return CheckReport.Write(Name, PackageRunRules.Evaluate(facts), output, errors);
    }

    /// <summary>Gives each argument of the timed run of the package.</summary>
    /// <returns>The arguments, in order.</returns>
    public static IReadOnlyList<string> PackageArguments()
    {
        // The Mac package runs in the App Sandbox, so it cannot write a log file outside its
        // container, and `-abslog` fails with no error (F-27). So the log comes from stdout. A
        // window keeps the desktop free during the run.
        return
        [
            $"-TimedRunSeconds={PackageRunRules.RunSeconds}",
            "-unattended",
            "-windowed",
            "-ResX=1280",
            "-ResY=720",
            "-stdout",
            "-FullStdOutLogOutput",
        ];
    }

    /// <summary>
    /// Removes the logs of the last run, so a run that writes no log cannot pass on an old one (T-2).
    /// </summary>
    private static void RemoveOldLogs(string logPath, string errorLogPath)
    {
        // File.Delete fails when the folder is absent, so the folder comes first.
        string? logFolder = Path.GetDirectoryName(logPath);
        if (logFolder is null)
        {
            throw new IOException($"the log path '{logPath}' has no folder");
        }

        Directory.CreateDirectory(logFolder);
        File.Delete(logPath);
        File.Delete(errorLogPath);
    }

    private static ToolOutput ReadLog(string path)
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
