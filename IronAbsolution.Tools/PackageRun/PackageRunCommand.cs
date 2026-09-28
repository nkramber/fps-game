using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.ToolchainCheck;

namespace IronAbsolution.Tools.PackageRun;

/// <summary>
/// The `package-run` command, the start command of the Windows package (D-89). It starts the
/// package of `run.ps1 package-build` with the timed-run option, and applies the pass rule of
/// <see cref="PackageRunRules"/>. `run.ps1 package-run` runs it (D-99).
/// </summary>
public static class PackageRunCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "package-run";

    /// <summary>
    /// The program of the package, under the root (D-102). The build of the package writes it
    /// there. The program under Binaries is the game itself. The program at the top of the package
    /// only starts it, so its exit code does not prove the run.
    /// </summary>
    public const string PackageProgram = "Game/Saved/Packages/Windows/IronAbsolution/Binaries/Win64/IronAbsolution.exe";

    /// <summary>The log that the package writes through `-abslog`, under the root. The evidence form takes it (D-31).</summary>
    public const string LogFile = "Game/Saved/Logs/package-run.log";

    /// <summary>The file that takes the stdout of the package, under the root.</summary>
    public const string OutputLogFile = "Game/Saved/Logs/package-run.stdout.log";

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
            errors.WriteLine($"{Name}: no file '{package}'. Run `run.ps1 package-build` first, and --root names the checkout.");
            return Program.FaultExitCode;
        }

        string logPath = ToolPaths.UnderFolder(root, LogFile);
        string outputLogPath = ToolPaths.UnderFolder(root, OutputLogFile);
        string errorLogPath = ToolPaths.UnderFolder(root, ErrorLogFile);
        try
        {
            RemoveOldLogs(logPath, outputLogPath, errorLogPath);
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
            run = ExternalProcess.RunToFiles(package, PackageArguments(logPath), root, outputLogPath, errorLogPath, [], limit);
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
    /// <param name="logPath">The full path of the log that the package writes.</param>
    /// <returns>The arguments, in order.</returns>
    public static IReadOnlyList<string> PackageArguments(string logPath)
    {
        // The game is a program of the Windows subsystem, so its stdout is not its log. The game
        // writes its log to the path of `-abslog`, and the Windows package has no sandbox that
        // stops that write. A window keeps the desktop free during the run.
        return
        [
            $"-TimedRunSeconds={PackageRunRules.RunSeconds}",
            "-unattended",
            "-windowed",
            "-ResX=1280",
            "-ResY=720",
            $"-abslog={logPath}",
        ];
    }

    /// <summary>
    /// Removes the logs of the last run, so a run that writes no log cannot pass on an old one (T-2).
    /// </summary>
    private static void RemoveOldLogs(string logPath, string outputLogPath, string errorLogPath)
    {
        // File.Delete fails when the folder is absent, so the folder comes first.
        string? logFolder = Path.GetDirectoryName(logPath);
        if (logFolder is null)
        {
            throw new IOException($"the log path '{logPath}' has no folder");
        }

        Directory.CreateDirectory(logFolder);
        File.Delete(logPath);
        File.Delete(outputLogPath);
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
