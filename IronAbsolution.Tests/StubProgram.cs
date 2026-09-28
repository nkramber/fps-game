using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace IronAbsolution.Tests;

/// <summary>
/// Puts a stub program of the engine at a path, and gives it a plan (D-103). Windows starts a
/// program file alone, so a POSIX shell script at a path that ends with `.exe` does not run there.
/// The stub is the build output of `IronAbsolution.TestStub`, and the host of that build looks for
/// its own assembly in its own folder, and not for its own file name. So a copy of the host runs
/// under the name of the program that it stands in for.
/// </summary>
public sealed class StubProgram
{
    /// <summary>The name of the built stub, before a copy takes the name of a program of the engine.</summary>
    private const string StubName = "IronAbsolution.TestStub";

    /// <summary>
    /// The name of the plan file beside the stub. `IronAbsolution.TestStub` holds the same name.
    /// The test project takes no reference to the assembly of the stub, so the name stands here too.
    /// </summary>
    private const string PlanFile = "stub-plan.txt";

    private readonly List<string> plan = new List<string>();

    /// <summary>Appends each argument of the run to a file, one argument on each line.</summary>
    /// <param name="path">The full path of the file that takes the arguments.</param>
    /// <returns>This stub, so the calls chain.</returns>
    public StubProgram RecordsArgumentsTo(string path)
    {
        return this.With("arguments", path);
    }

    /// <summary>Writes one line to stdout.</summary>
    /// <param name="line">The line, with no line end.</param>
    /// <returns>This stub, so the calls chain.</returns>
    public StubProgram WritesLine(string line)
    {
        return this.With("stdout", line);
    }

    /// <summary>Writes the test report in the folder of the argument `-ReportExportPath=`.</summary>
    /// <param name="report">The whole text of `index.json`.</param>
    /// <returns>This stub, so the calls chain.</returns>
    public StubProgram WritesReport(string report)
    {
        return this.With("report", report);
    }

    /// <summary>Appends one line to the file of the argument `-abslog=`.</summary>
    /// <param name="line">The line, with no line end.</param>
    /// <returns>This stub, so the calls chain.</returns>
    public StubProgram WritesToLog(string line)
    {
        return this.With("abslog", line);
    }

    /// <summary>Waits before the stub stops, so a test can reach a time limit.</summary>
    /// <param name="seconds">The number of seconds of the wait.</param>
    /// <returns>This stub, so the calls chain.</returns>
    public StubProgram WaitsForSeconds(int seconds)
    {
        return this.With("seconds", seconds.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Writes the stub and its plan at a path, and gives it the exit code. Each earlier call of
    /// this stub sets what the run does before it gives that code.
    /// </summary>
    /// <param name="path">The full path of the program that the stub stands in for.</param>
    /// <param name="exitCode">The exit code of the run.</param>
    public void ExitsWith(string path, int exitCode)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        string folder = Path.GetDirectoryName(path) ?? throw new ArgumentException($"the stub path '{path}' has no folder", nameof(path));
        Directory.CreateDirectory(folder);
        foreach (string file in Directory.GetFiles(BuiltStubFolder()))
        {
            File.Copy(file, Path.Combine(folder, Path.GetFileName(file)), overwrite: true);
        }

        // A plan value holds one line, so a line end inside a value takes the escape of the plan.
        List<string> lines = new List<string>(this.plan) { "exit=" + exitCode.ToString(CultureInfo.InvariantCulture) };
        File.WriteAllLines(Path.Combine(folder, PlanFile), lines);
        if (OperatingSystem.IsWindows() && IsBatchFile(path))
        {
            // Windows reads `Build.bat` and `RunUAT.bat` as batch files, and it does not start a
            // program file under such a name. So the batch file gives each argument to the stub.
            File.WriteAllText(path, $"@echo off\r\n\"%~dp0{HostName()}\" %*\r\n");
            return;
        }

        File.Copy(Path.Combine(folder, HostName()), path, overwrite: true);
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    /// <summary>Tells whether Windows reads the path as a batch file.</summary>
    private static bool IsBatchFile(string path)
    {
        string extension = Path.GetExtension(path);
        return extension.Equals(".bat", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Gives the folder that holds the build output of the stub, beside the tests.</summary>
    private static string BuiltStubFolder()
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "stub");
        if (!Directory.Exists(folder))
        {
            throw new InvalidOperationException(
                $"no folder '{folder}'. The `CopyTestStub` target of the test project puts the build of {StubName} there (D-103).");
        }

        return folder;
    }

    /// <summary>Gives the file name of the built host of the stub on this platform.</summary>
    private static string HostName()
    {
        return OperatingSystem.IsWindows() ? StubName + ".exe" : StubName;
    }

    private StubProgram With(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        StringBuilder escaped = new StringBuilder(value.Length);
        foreach (char letter in value)
        {
            escaped.Append(letter switch
            {
                '\\' => "\\\\",
                '\n' => "\\n",
                '\r' => string.Empty,
                _ => letter.ToString(),
            });
        }

        this.plan.Add(key + "=" + escaped.ToString());
        return this;
    }
}
