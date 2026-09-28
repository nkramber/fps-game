using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.ToolchainCheck;

namespace IronAbsolution.Tools.EditorTest;

/// <summary>
/// The `editor-test` command of the Mac (D-55, D-71). It starts the editor with no window, runs
/// each automation test of the project, and applies the pass rule of <see cref="EditorTestRules"/>.
/// `make editor-test` runs it. The Windows PC runs `scripts/editor-test.ps1` instead (D-72).
/// </summary>
public static class EditorTestCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "editor-test";

    /// <summary>The project file, under the root of the checkout (D-73).</summary>
    public const string ProjectFile = "Game/IronAbsolution.uproject";

    /// <summary>The editor program of the Mac, under the engine folder (D-79).</summary>
    public const string EditorProgram = "Engine/Binaries/Mac/UnrealEditor.app/Contents/MacOS/UnrealEditor";

    /// <summary>The folder of the test report, under the root. The ignore rules hold `Saved/` (D-9).</summary>
    public const string ReportFolder = "Game/Saved/Automation/editor-test";

    /// <summary>The file of the report that the automation controller writes in the report folder.</summary>
    public const string ReportFile = "index.json";

    /// <summary>The file that takes the stdout of the editor, under the root. The evidence form takes it (D-31).</summary>
    public const string LogFile = "Game/Saved/Logs/editor-test.log";

    /// <summary>The file that takes the stderr of the editor, under the root.</summary>
    public const string ErrorLogFile = "Game/Saved/Logs/editor-test.stderr.log";

    /// <summary>The filter of the tests. It matches each test whose name holds it.</summary>
    public const string TestFilter = "IronAbsolution";

    private const string RootOption = "--root";

    /// <summary>
    /// Gets the time limit of one run. A run that loads no test module can wait with no end, so
    /// the limit turns that fault into a failure (T-2).
    /// </summary>
    public static TimeSpan Limit { get; } = TimeSpan.FromMinutes(30);

    /// <summary>Runs each automation test of the project headless, and reports each check.</summary>
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
        string? engineFolder = Environment.GetEnvironmentVariable(ToolchainPins.EngineVariable);
        return RunTests(root, engineFolder, Limit, output, errors);
    }

    /// <summary>Runs the editor from an engine folder, then reads the report and the log.</summary>
    /// <param name="root">The full path of the root of the checkout.</param>
    /// <param name="engineFolder">The value of the engine variable, or null when it is not set (D-79).</param>
    /// <param name="limit">The time limit of the run.</param>
    /// <param name="output">The writer that takes each line of a check that passes.</param>
    /// <param name="errors">The writer that takes each line of a check that fails, and each fault.</param>
    /// <returns>0 when each check passes, or 1.</returns>
    public static int RunTests(string root, string? engineFolder, TimeSpan limit, TextWriter output, TextWriter errors)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        if (string.IsNullOrWhiteSpace(engineFolder))
        {
            errors.WriteLine($"{Name}: set {ToolchainPins.EngineVariable} to the folder that holds 'Engine' (D-79).");
            return Program.FaultExitCode;
        }

        string editor = UnderFolder(engineFolder, EditorProgram);
        string project = UnderFolder(root, ProjectFile);
        foreach (string path in new[] { editor, project })
        {
            if (!File.Exists(path))
            {
                errors.WriteLine($"{Name}: no file '{path}'. {ToolchainPins.EngineVariable} names the engine folder, and --root names the checkout.");
                return Program.FaultExitCode;
            }
        }

        string reportFolder = UnderFolder(root, ReportFolder);
        string logPath = UnderFolder(root, LogFile);
        string errorLogPath = UnderFolder(root, ErrorLogFile);
        try
        {
            RemoveOldResults(reportFolder, logPath, errorLogPath);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            errors.WriteLine($"{Name}: the results of the last run did not go away, so a new run cannot prove a pass: {fault.Message}");
            return Program.FaultExitCode;
        }

        output.WriteLine($"{Name}: run the tests '{TestFilter}' with no window, with a limit of {limit.TotalMinutes} minutes. The log goes to '{logPath}'.");
        FileRunResult run;
        try
        {
            run = ExternalProcess.RunToFiles(editor, EditorArguments(project, reportFolder), root, logPath, errorLogPath, [], limit);
        }
        catch (InvalidOperationException fault)
        {
            errors.WriteLine($"{Name}: {fault.Message}");
            return Program.FaultExitCode;
        }

        EditorTestFacts facts = new EditorTestFacts(run, limit, ReadFile(Path.Combine(reportFolder, ReportFile)), ReadFile(logPath));
        return CheckReport.Write(Name, EditorTestRules.Evaluate(facts), output, errors);
    }

    /// <summary>Gives each argument of the headless run of the editor.</summary>
    /// <param name="project">The full path of the project file.</param>
    /// <param name="reportFolder">The full path of the folder of the test report.</param>
    /// <returns>The arguments, in order.</returns>
    public static IReadOnlyList<string> EditorArguments(string project, string reportFolder)
    {
        // `Quit` inside the automation command makes the controller write the success line and
        // set the exit code from the results. `-nullrhi` starts no renderer.
        return
        [
            project,
            $"-ExecCmds=Automation RunTests {TestFilter};Quit",
            $"-ReportExportPath={reportFolder}",
            "-unattended",
            "-nullrhi",
            "-nosplash",
            "-nopause",
            "-nosound",
            "-stdout",
            "-FullStdOutLogOutput",
        ];
    }

    private static string UnderFolder(string folder, string relativePath)
    {
        return Path.Combine(folder, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    /// <summary>
    /// Removes the report and the logs of the last run, so a run that writes no report cannot
    /// pass on an old one (T-2).
    /// </summary>
    private static void RemoveOldResults(string reportFolder, string logPath, string errorLogPath)
    {
        if (Directory.Exists(reportFolder))
        {
            Directory.Delete(reportFolder, recursive: true);
        }

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
