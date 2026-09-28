using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// Runs `scripts/editor-build.ps1` and `scripts/editor-test.ps1` under PowerShell with stub
/// programs of the engine (D-72). Each script takes the checkout from its own folder, so each
/// test copies the script into a temporary checkout. <see cref="StubProgram"/> writes each stub
/// at the path of the Windows program, and that stub runs on Windows and on the hosted Linux
/// runner (D-103). <see cref="PowerShellScript"/> runs each script, and CI must run these tests.
/// </summary>
public sealed class EditorScriptTests : IDisposable
{
    private const string EngineVariable = "IRON_ABSOLUTION_ENGINE_DIR";
    private const string SuccessLine = "LogAutomationCommandLine: Display: **** TEST COMPLETE. EXIT CODE: 0 ****";
    private const string PassedReport = """{ "succeeded": 1, "succeededWithWarnings": 0, "failed": 0, "notRun": 0, "inProcess": 0, "tests": [ { "fullTestPath": "IronAbsolution.Project.Settings", "state": "Success" } ] }""";
    private const string FailedReport = """{ "succeeded": 1, "succeededWithWarnings": 0, "failed": 1, "notRun": 0, "inProcess": 0, "tests": [ { "fullTestPath": "IronAbsolution.Project.Settings", "state": "Success" }, { "fullTestPath": "IronAbsolution.Project.FailOnPurpose", "state": "Fail" } ] }""";

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"editor-script-{Guid.NewGuid():N}");
    private readonly string root;
    private readonly string engine;
    private readonly string reportFolder;

    public EditorScriptTests()
    {
        this.root = Path.Combine(this.folder, "checkout");
        this.engine = Path.Combine(this.folder, "engine");
        this.reportFolder = Path.Combine(this.root, "Game", "Saved", "Automation", "editor-test");
        Directory.CreateDirectory(Path.Combine(this.root, "scripts"));
        Directory.CreateDirectory(Path.Combine(this.root, "Game"));
        File.WriteAllText(Path.Combine(this.root, "Game", "IronAbsolution.uproject"), "{}");
        foreach (string script in new[] { "editor-build.ps1", "editor-test.ps1" })
        {
            File.Copy(RepositoryRoot.PathTo($"scripts/{script}"), Path.Combine(this.root, "scripts", script));
        }
    }

    public void Dispose()
    {
        Directory.Delete(this.folder, recursive: true);
    }

    [Fact]
    public void TheTestScriptPassesAStubRunWithTheReportAndTheSuccessLine()
    {
        this.WriteEditor(Passes(PassedReport, SuccessLine), exitCode: 0);

        (int exitCode, string output) = this.RunScript("editor-test.ps1", this.engine);

        Assert.Equal(0, exitCode);
        Assert.Contains("editor-test: Editor exit: pass. The editor gave the exit code 0.", output, StringComparison.Ordinal);
        Assert.Contains($"editor-test: Test report: pass. The report '{Path.Combine(this.reportFolder, "index.json")}' shows 1 passed tests: IronAbsolution.Project.Settings.", output, StringComparison.Ordinal);
        Assert.Contains("editor-test: pass. Each of the 3 checks passes.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTestScriptFailsAFailedTestAndNamesIt()
    {
        // Exit test 5 of PR-9 on the Windows script: a test that fails gives a nonzero exit code.
        this.WriteEditor(Passes(FailedReport, "LogAutomationCommandLine: Display: **** TEST COMPLETE. EXIT CODE: -1 ****"), exitCode: 255);

        (int exitCode, string output) = this.RunScript("editor-test.ps1", this.engine);

        Assert.Equal(1, exitCode);
        Assert.Contains("editor-test: Editor exit: fail. The editor gave the exit code 255.", output, StringComparison.Ordinal);
        Assert.Contains("these tests did not pass: IronAbsolution.Project.FailOnPurpose (Fail)", output, StringComparison.Ordinal);
        Assert.Contains("editor-test: fail. 3 of the 3 checks fail.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTestScriptFailsWithThePathWhenTheReportIsAbsentEvenWithAnOldReport()
    {
        // Exit test 6 of PR-9 on the Windows script (T-2).
        Directory.CreateDirectory(this.reportFolder);
        File.WriteAllText(Path.Combine(this.reportFolder, "index.json"), PassedReport);
        this.WriteEditor(new StubProgram().WritesLine(SuccessLine), exitCode: 0);

        (int exitCode, string output) = this.RunScript("editor-test.ps1", this.engine);

        Assert.Equal(1, exitCode);
        Assert.Contains($"editor-test: Test report: fail. No test report: no file '{Path.Combine(this.reportFolder, "index.json")}'.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTestScriptFailsAnExitCodeOfZeroWithNoSuccessLine()
    {
        this.WriteEditor(Passes(PassedReport, "LogInit: Display: Engine is initialized."), exitCode: 0);

        (int exitCode, string output) = this.RunScript("editor-test.ps1", this.engine);

        Assert.Equal(1, exitCode);
        Assert.Contains("editor-test: Success line: fail.", output, StringComparison.Ordinal);
        Assert.Contains("An exit code of 0 alone is not a pass.", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("not json", "is not JSON")]
    [InlineData("""{ "succeeded": 1, "succeededWithWarnings": 0, "notRun": 0, "inProcess": 0, "tests": [] }""", "has no whole number in the field 'failed'")]
    [InlineData("""{ "succeeded": 1, "succeededWithWarnings": 0, "failed": 0, "notRun": 0, "inProcess": 0 }""", "has no array in the field 'tests'")]
    [InlineData("""{ "succeeded": 1, "succeededWithWarnings": 0, "failed": 0, "notRun": 0, "inProcess": 0, "tests": [ { "state": "Success" } ] }""", "has a test with no text in the field 'fullTestPath'")]
    public void TheTestScriptFailsAnInvalidReportWithThePathAndTheField(string report, string fault)
    {
        this.WriteEditor(Passes(report, SuccessLine), exitCode: 0);

        (int exitCode, string output) = this.RunScript("editor-test.ps1", this.engine);

        Assert.Equal(1, exitCode);
        Assert.Contains($"editor-test: Test report: fail. The report '{Path.Combine(this.reportFolder, "index.json")}' {fault}", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTestScriptGivesTheEditorTheAutomationCommandAsOneArgument()
    {
        string argumentsFile = Path.Combine(this.folder, "arguments.txt");
        this.WriteEditor(new StubProgram().RecordsArgumentsTo(argumentsFile), exitCode: 0);

        this.RunScript("editor-test.ps1", this.engine);

        string[] arguments = File.ReadAllLines(argumentsFile);
        Assert.Equal(Path.Combine(this.root, "Game", "IronAbsolution.uproject"), arguments[0]);
        Assert.Contains("-ExecCmds=Automation RunTests IronAbsolution;Quit", arguments);
        Assert.Contains($"-ReportExportPath={this.reportFolder}", arguments);
        Assert.Contains("-nullrhi", arguments);
    }

    [Theory]
    [InlineData("editor-build.ps1")]
    [InlineData("editor-test.ps1")]
    public void EachScriptFailsAndNamesTheVariableWhenItIsNotSet(string script)
    {
        (int exitCode, string output) = this.RunScript(script, engineFolder: null);

        Assert.Equal(1, exitCode);
        string name = script[..^".ps1".Length];
        Assert.Equal($"{name}: set IRON_ABSOLUTION_ENGINE_DIR to the folder that holds 'Engine' (D-79).", output.Trim());
    }

    [Fact]
    public void TheBuildScriptFailsWithThePathWhenBuildBatIsAbsent()
    {
        (int exitCode, string output) = this.RunScript("editor-build.ps1", this.engine);

        Assert.Equal(1, exitCode);
        string buildBatch = Path.Combine(this.engine, "Engine", "Build", "BatchFiles", "Build.bat");
        Assert.Contains($"editor-build: no file '{buildBatch}'.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBuildScriptGivesTheExitCodeOfBuildBatAndTheArguments()
    {
        string argumentsFile = Path.Combine(this.folder, "arguments.txt");
        string buildBatch = Path.Combine(this.engine, "Engine", "Build", "BatchFiles", "Build.bat");
        new StubProgram().RecordsArgumentsTo(argumentsFile).ExitsWith(buildBatch, exitCode: 7);

        (int exitCode, string output) = this.RunScript("editor-build.ps1", this.engine);

        Assert.Equal(7, exitCode);
        Assert.Contains("editor-build: Build.bat gave the exit code 7.", output, StringComparison.Ordinal);
        string[] arguments = File.ReadAllLines(argumentsFile);
        Assert.Equal(
            [
                "IronAbsolutionEditor",
                "Win64",
                "Development",
                $"-Project={Path.Combine(this.root, "Game", "IronAbsolution.uproject")}",
                "-WaitMutex",
                $"-Log={Path.Combine(this.root, "Game", "Saved", "Logs", "editor-build.log")}",
            ],
            arguments);
    }

    /// <summary>A stub editor that writes a report and one log line, then exits.</summary>
    private static StubProgram Passes(string report, string logLine)
    {
        return new StubProgram().WritesReport(report).WritesLine(logLine);
    }

    private void WriteEditor(StubProgram stub, int exitCode)
    {
        stub.ExitsWith(Path.Combine(this.engine, "Engine", "Binaries", "Win64", "UnrealEditor-Cmd.exe"), exitCode);
    }

    private (int ExitCode, string Output) RunScript(string script, string? engineFolder)
    {
        return PowerShellScript.Run(
            Path.Combine(this.root, "scripts", script),
            this.folder,
            new Dictionary<string, string?> { [EngineVariable] = engineFolder });
    }
}
