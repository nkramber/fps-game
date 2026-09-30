using System;
using System.IO;
using IronAbsolution.Tools;
using IronAbsolution.Tools.FrameCapture;
using IronAbsolution.Tools.PackageRun;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The I/O of the frame-time capture, with a stub package at the path of the program of the package
/// under a temporary checkout (D-137). The stub writes its log to the path of `-abslog`, and the CSV
/// file to the path of `-FrameTimeCsv`, as the game does. <see cref="StubProgram"/> writes a stub
/// that runs on Windows and on the hosted Linux runner (D-103).
/// </summary>
public sealed class FrameCaptureCommandTests : IDisposable
{
    private static readonly TimeSpan Limit = TimeSpan.FromMinutes(2);

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"frame-capture-{Guid.NewGuid():N}");
    private readonly string root;
    private readonly string csvPath;
    private readonly string logPath;
    private readonly string successLine;

    public FrameCaptureCommandTests()
    {
        this.root = Path.Combine(this.folder, "checkout");
        this.csvPath = Path.Combine(this.root, "Game", "Saved", "Logs", "frame-capture.csv");
        this.logPath = Path.Combine(this.root, "Game", "Saved", "Logs", "frame-capture.log");
        this.successLine = $"LogFrameTimeCapture: Display: Frame-time capture: pass. The map /Game/Maps/L_Gym wrote the file {this.csvPath}.";
        Directory.CreateDirectory(this.root);
    }

    public void Dispose()
    {
        Directory.Delete(this.folder, recursive: true);
    }

    [Fact]
    public void AStubCaptureInsideTheBudgetPasses()
    {
        string csv = FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(300, 5.0));
        this.WritePackage(new StubProgram().WritesCsv(csv).WritesToLog(this.successLine), exitCode: 0);

        (int exitCode, string output, string errors) = this.Run(Limit);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, errors);
        Assert.Contains("frame-capture: Mean frame time: pass. The mean of 300 frames is 5.00 ms, inside the budget of 8.33 ms (120 fps, D-32).", output, StringComparison.Ordinal);
        Assert.Contains("frame-capture: pass. Each of the 6 checks passes.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void AStubCaptureOverTheBudgetGivesANonzeroExitCodeAndNamesBothNumbers()
    {
        // Exit test 4 of PR-22.
        string csv = FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(300, 12.0));
        this.WritePackage(new StubProgram().WritesCsv(csv).WritesToLog(this.successLine), exitCode: 0);

        (int exitCode, _, string errors) = this.Run(Limit);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("frame-capture: Mean frame time: fail. The mean of 300 frames is 12.00 ms, over the budget of 8.33 ms (120 fps, D-32).", errors, StringComparison.Ordinal);
        Assert.Contains("frame-capture: fail. 2 of the 6 checks fail.", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void AnOldCaptureFileDoesNotPassANewRunThatWritesNone()
    {
        // Exit test 3 of PR-22: the file of an earlier run must not make a new run pass (T-2).
        Directory.CreateDirectory(Path.GetDirectoryName(this.csvPath)!);
        File.WriteAllText(this.csvPath, FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(300, 5.0)));
        this.WritePackage(new StubProgram().WritesToLog(this.successLine), exitCode: 0);

        (int exitCode, _, string errors) = this.Run(Limit);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"frame-capture: Capture file: fail. No capture file: no file '{this.csvPath}'.", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePackageGetsTheGymTheCsvPathAndTheLogPathWithNoWindowOption()
    {
        // D-137 and D-138: no window option, so the game runs borderless fullscreen.
        string argumentsFile = Path.Combine(this.folder, "arguments.txt");
        this.WritePackage(new StubProgram().RecordsArgumentsTo(argumentsFile), exitCode: 0);

        this.Run(Limit);

        Assert.Equal(
            ["/Game/Maps/L_Gym", $"-FrameTimeCsv={this.csvPath}", "-unattended", $"-abslog={this.logPath}"],
            File.ReadAllLines(argumentsFile));
    }

    [Fact]
    public void AnAbsentPackageFailsWithThePathAndTheBuildTarget()
    {
        (int exitCode, _, string errors) = this.Run(Limit);

        Assert.Equal(Program.FaultExitCode, exitCode);
        string package = ToolPaths.UnderFolder(this.root, PackageRunCommand.PackageProgram);
        Assert.Equal($"frame-capture: no file '{package}'. Run `run.ps1 package-build` first, and --root names the checkout.", errors.Trim());
    }

    [Fact]
    public void TheCommandLineReadsTheRootOption()
    {
        string csv = FrameCaptureFixtures.Capture(FrameCaptureFixtures.Frames(300, 5.0));
        this.WritePackage(new StubProgram().WritesCsv(csv).WritesToLog(this.successLine), exitCode: 0);
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["frame-capture", "--root", this.root], output, errors);

        Assert.Equal(0, exitCode);
        Assert.Contains("frame-capture: pass.", output.ToString(), StringComparison.Ordinal);
    }

    private void WritePackage(StubProgram stub, int exitCode)
    {
        stub.ExitsWith(ToolPaths.UnderFolder(this.root, PackageRunCommand.PackageProgram), exitCode);
    }

    private (int ExitCode, string Output, string Errors) Run(TimeSpan limit)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();
        int exitCode = FrameCaptureCommand.RunCapture(this.root, limit, output, errors);
        return (exitCode, output.ToString(), errors.ToString());
    }
}
