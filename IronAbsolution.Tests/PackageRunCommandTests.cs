using System;
using System.IO;
using IronAbsolution.Tools;
using IronAbsolution.Tools.PackageRun;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The I/O of the start command of the package, with a stub package at the path of the program of
/// the package under a temporary checkout (D-89, D-102). The stub writes its log to the path of
/// `-abslog`, as the game does. <see cref="StubProgram"/> writes a stub that runs on Windows and
/// on the hosted Linux runner (D-103).
/// </summary>
public sealed class PackageRunCommandTests : IDisposable
{
    private const string SuccessLine = "LogTimedRun: Display: Timed run: pass. The map /Game/Maps/L_Test ran for 10 seconds.";
    private const string StartLine = "LogInit: Display: Engine is initialized.";

    private static readonly TimeSpan Limit = TimeSpan.FromMinutes(2);

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"package-run-{Guid.NewGuid():N}");
    private readonly string root;
    private readonly string logPath;

    public PackageRunCommandTests()
    {
        this.root = Path.Combine(this.folder, "checkout");
        this.logPath = Path.Combine(this.root, "Game", "Saved", "Logs", "package-run.log");
        Directory.CreateDirectory(this.root);
    }

    public void Dispose()
    {
        Directory.Delete(this.folder, recursive: true);
    }

    [Fact]
    public void AStubRunThatWritesTheSuccessLinePasses()
    {
        // Exit test 3 of PR-10: the package starts and stops, and the command finds the success line.
        this.WritePackage(new StubProgram().WritesToLog(SuccessLine), exitCode: 0);

        (int exitCode, string output, string errors) = this.Run(Limit);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, errors);
        Assert.Contains($"package-run: Success line: pass. The log '{this.logPath}' holds the line", output, StringComparison.Ordinal);
        Assert.Contains("package-run: pass. Each of the 2 checks passes.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ARunWithNoSuccessLineFailsAndNamesTheLogEvenWithAnOldLog()
    {
        // Exit test 4 of PR-10. The log of an earlier run must not make a new run pass (T-2).
        Directory.CreateDirectory(Path.GetDirectoryName(this.logPath)!);
        File.WriteAllText(this.logPath, SuccessLine);
        this.WritePackage(new StubProgram().WritesToLog(StartLine), exitCode: 0);

        (int exitCode, _, string errors) = this.Run(Limit);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"package-run: Success line: fail. The log '{this.logPath}' has no line", errors, StringComparison.Ordinal);
        Assert.Contains("package-run: fail. 1 of the 2 checks fail.", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void APackageThatDoesNotStopFailsAtTheLimitAndNamesTheLog()
    {
        this.WritePackage(new StubProgram().WritesToLog(StartLine).WaitsForSeconds(60), exitCode: 0);

        (int exitCode, _, string errors) = this.Run(TimeSpan.FromSeconds(1));

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"package-run: Package exit: fail. The time limit of {TimeSpan.FromSeconds(1).TotalMinutes} minutes stopped the package. Read the log '{this.logPath}'.", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void ACrashFailsWithTheExitCodeAndTheLog()
    {
        this.WritePackage(new StubProgram().WritesToLog(SuccessLine), exitCode: 3);

        (int exitCode, _, string errors) = this.Run(Limit);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"package-run: Package exit: fail. The package gave the exit code 3. Read the log '{this.logPath}'.", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePackageGetsTheTimedRunOptionAndTheLogPath()
    {
        string argumentsFile = Path.Combine(this.folder, "arguments.txt");
        this.WritePackage(new StubProgram().RecordsArgumentsTo(argumentsFile), exitCode: 0);

        this.Run(Limit);

        Assert.Equal(
            ["-TimedRunSeconds=10", "-unattended", "-windowed", "-ResX=1280", "-ResY=720", $"-abslog={this.logPath}"],
            File.ReadAllLines(argumentsFile));
    }

    [Fact]
    public void TheSuccessLineOnStdoutAloneDoesNotPass()
    {
        // The log of the run is the file of `-abslog`. The stdout of the package goes to another file.
        this.WritePackage(new StubProgram().WritesLine(SuccessLine).WritesToLog(StartLine), exitCode: 0);

        (int exitCode, _, string errors) = this.Run(Limit);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"package-run: Success line: fail. The log '{this.logPath}' has no line", errors, StringComparison.Ordinal);
        string outputLog = ToolPaths.UnderFolder(this.root, PackageRunCommand.OutputLogFile);
        Assert.Contains(SuccessLine, File.ReadAllText(outputLog), StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentPackageFailsWithThePathAndTheBuildTarget()
    {
        (int exitCode, _, string errors) = this.Run(Limit);

        Assert.Equal(Program.FaultExitCode, exitCode);
        string package = ToolPaths.UnderFolder(this.root, PackageRunCommand.PackageProgram);
        Assert.Equal($"package-run: no file '{package}'. Run `run.ps1 package-build` first, and --root names the checkout.", errors.Trim());
    }

    [Fact]
    public void TheCommandLineReadsTheRootOption()
    {
        this.WritePackage(new StubProgram().WritesToLog(SuccessLine), exitCode: 0);
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["package-run", "--root", this.root], output, errors);

        Assert.Equal(0, exitCode);
        Assert.Contains("package-run: pass.", output.ToString(), StringComparison.Ordinal);
    }

    private void WritePackage(StubProgram stub, int exitCode)
    {
        stub.ExitsWith(ToolPaths.UnderFolder(this.root, PackageRunCommand.PackageProgram), exitCode);
    }

    private (int ExitCode, string Output, string Errors) Run(TimeSpan limit)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();
        int exitCode = PackageRunCommand.RunPackage(this.root, limit, output, errors);
        return (exitCode, output.ToString(), errors.ToString());
    }
}
