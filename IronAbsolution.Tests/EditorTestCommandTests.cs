using System;
using System.IO;
using IronAbsolution.Tools;
using IronAbsolution.Tools.EditorTest;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The I/O of the headless test command with a stub editor at the path of the editor, under a
/// temporary engine folder (D-71, D-79, D-102). The stub reads the report
/// folder from its arguments, as the editor does, and it writes the report with the UTF-8 byte
/// order mark of the editor of 5.8.3. <see cref="StubProgram"/> writes a stub that runs on each
/// platform (D-103).
/// </summary>
public sealed class EditorTestCommandTests : IDisposable
{
    private const string SuccessLine = "LogAutomationCommandLine: Display: **** TEST COMPLETE. EXIT CODE: 0 ****";
    private const string PassedReport = """{ "succeeded": 1, "succeededWithWarnings": 0, "failed": 0, "notRun": 0, "inProcess": 0, "tests": [ { "fullTestPath": "IronAbsolution.Project.Settings", "state": "Success" } ] }""";

    private static readonly TimeSpan Limit = TimeSpan.FromMinutes(2);

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"editor-test-{Guid.NewGuid():N}");
    private readonly string root;
    private readonly string engine;

    public EditorTestCommandTests()
    {
        this.root = Path.Combine(this.folder, "checkout");
        this.engine = Path.Combine(this.folder, "engine");
        Directory.CreateDirectory(Path.Combine(this.root, "Game"));
        File.WriteAllText(Path.Combine(this.root, "Game", "IronAbsolution.uproject"), "{}");
    }

    public void Dispose()
    {
        Directory.Delete(this.folder, recursive: true);
    }

    [Fact]
    public void AStubRunThatWritesTheReportAndTheSuccessLinePasses()
    {
        this.WriteEditor(new StubProgram().WritesReport(PassedReport).WritesLine(SuccessLine), exitCode: 0);

        (int exitCode, string output, string errors) = this.Run();

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, errors);
        Assert.Contains("editor-test: Test report: pass.", output, StringComparison.Ordinal);
        Assert.Contains("editor-test: pass. Each of the 3 checks passes.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentReportFailsWithThePathEvenWhenAnOldReportWasThere()
    {
        // Exit test 6 of PR-9. The report of an earlier run must not make a new run pass (T-2).
        string reportFolder = Path.Combine(this.root, "Game", "Saved", "Automation", "editor-test");
        Directory.CreateDirectory(reportFolder);
        File.WriteAllText(Path.Combine(reportFolder, EditorTestCommand.ReportFile), PassedReport);
        this.WriteEditor(new StubProgram().WritesLine(SuccessLine), exitCode: 0);

        (int exitCode, _, string errors) = this.Run();

        Assert.Equal(Program.FaultExitCode, exitCode);
        string reportPath = Path.Combine(reportFolder, EditorTestCommand.ReportFile);
        Assert.Contains($"editor-test: Test report: fail. No test report: no file '{reportPath}'.", errors, StringComparison.Ordinal);
        Assert.Contains("editor-test: fail. 1 of the 3 checks fail.", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void AnExitCodeOfZeroWithNoSuccessLineFails()
    {
        this.WriteEditor(new StubProgram().WritesReport(PassedReport).WritesLine("LogInit: Display: Engine is initialized."), exitCode: 0);

        (int exitCode, _, string errors) = this.Run();

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("editor-test: Success line: fail.", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void TheEditorGetsTheProjectFirstAndTheAutomationCommand()
    {
        string argumentsFile = Path.Combine(this.folder, "arguments.txt");
        this.WriteEditor(new StubProgram().RecordsArgumentsTo(argumentsFile), exitCode: 0);

        this.Run();

        string[] arguments = File.ReadAllLines(argumentsFile);
        Assert.Equal(Path.Combine(this.root, "Game", "IronAbsolution.uproject"), arguments[0]);
        Assert.Contains("-ExecCmds=Automation RunTests IronAbsolution;Quit", arguments);
        Assert.Contains("-nullrhi", arguments);
        Assert.Contains($"-ReportExportPath={Path.Combine(this.root, "Game", "Saved", "Automation", "editor-test")}", arguments);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void AnUnsetEngineVariableFailsAndNamesTheVariable(string? engineFolder)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = EditorTestCommand.RunTests(this.root, engineFolder, Limit, output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Equal("editor-test: set IRON_ABSOLUTION_ENGINE_DIR to the folder that holds 'Engine' (D-79).", errors.ToString().Trim());
    }

    [Fact]
    public void AnEngineFolderWithNoEditorFailsWithThePath()
    {
        (int exitCode, _, string errors) = this.Run();

        Assert.Equal(Program.FaultExitCode, exitCode);
        string editor = ToolPaths.UnderFolder(this.engine, EditorTestCommand.EditorProgram);
        Assert.Contains($"editor-test: no file '{editor}'.", errors, StringComparison.Ordinal);
    }

    private void WriteEditor(StubProgram stub, int exitCode)
    {
        stub.ExitsWith(ToolPaths.UnderFolder(this.engine, EditorTestCommand.EditorProgram), exitCode);
    }

    private (int ExitCode, string Output, string Errors) Run()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();
        int exitCode = EditorTestCommand.RunTests(this.root, this.engine, Limit, output, errors);
        return (exitCode, output.ToString(), errors.ToString());
    }
}
