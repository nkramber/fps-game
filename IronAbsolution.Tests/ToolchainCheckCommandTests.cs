using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools;
using IronAbsolution.Tools.ToolchainCheck;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The I/O of the toolchain check: the read of the engine version file, an absent program, the
/// report, and the Windows script that names the same pins (D-28, D-72, D-79).
/// </summary>
public sealed class ToolchainCheckCommandTests : IDisposable
{
    private const string AbsentProgram = "/no/such/folder/xcodebuild";

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"toolchain-check-{Guid.NewGuid():N}");

    public ToolchainCheckCommandTests()
    {
        Directory.CreateDirectory(this.folder);
    }

    public void Dispose()
    {
        Directory.Delete(this.folder, recursive: true);
    }

    [Fact]
    public void TheGatherReadsTheVersionFileUnderTheEngineFolder()
    {
        string buildFolder = Path.Combine(this.folder, "Engine", "Build");
        Directory.CreateDirectory(buildFolder);
        string versionFile = Path.Combine(buildFolder, "Build.version");
        File.WriteAllText(versionFile, """{ "MajorVersion": 5, "MinorVersion": 8, "PatchVersion": 3 }""");

        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, AbsentProgram, AbsentProgram, this.folder);

        Assert.Equal(ToolOutput.Found(versionFile, File.ReadAllText(versionFile)), facts.Engine);
        Assert.True(ToolchainRules.Evaluate(facts)[1].Holds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AnUnsetEngineVariableGivesAnAbsentEngine(string? engineFolder)
    {
        ToolchainFacts facts = ToolchainFacts.Gather(engineFolder, AbsentProgram, AbsentProgram, this.folder);

        Assert.Null(facts.Engine.Text);
        Assert.Equal("the variable IRON_ABSOLUTION_ENGINE_DIR is not set (D-79)", facts.Engine.Absence);
    }

    [Fact]
    public void AnEngineFolderWithNoVersionFileGivesAnAbsentEngineThatNamesThePath()
    {
        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, AbsentProgram, AbsentProgram, this.folder);

        string versionFile = Path.Combine(this.folder, "Engine", "Build", "Build.version");
        Assert.Null(facts.Engine.Text);
        Assert.Equal($"no file '{versionFile}'. IRON_ABSOLUTION_ENGINE_DIR names the folder that holds 'Engine'", facts.Engine.Absence);
    }

    [Fact]
    public void AProgramThatDoesNotStartGivesAnAbsentOutputWithItsReason()
    {
        // Exit test 3 of PR-8 on a real process start: no exception escapes, and the pin fails with the reason (T-2).
        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, AbsentProgram, AbsentProgram, this.folder);

        Assert.Null(facts.Xcode.Text);
        Assert.Equal($"{AbsentProgram} -version", facts.Xcode.Source);
        Assert.Contains($"'{AbsentProgram}' did not start", facts.Xcode.Absence, StringComparison.Ordinal);
        Assert.Null(facts.GitLfs.Text);
        Assert.Equal($"{AbsentProgram} lfs version", facts.GitLfs.Source);
        Assert.Null(facts.MetalToolchain.Text);
        Assert.Equal($"{AbsentProgram} -showComponent MetalToolchain", facts.MetalToolchain.Source);

        IReadOnlyList<PinResult> results = ToolchainRules.Evaluate(facts);
        Assert.All(results, result => Assert.False(result.Holds));
    }

    [Fact]
    public void AProgramWithAFaultExitCodeGivesAnAbsentOutputWithItsStderr()
    {
        // `git -version` stands in for an xcodebuild of the Command Line Tools: it starts, and it gives a fault exit code.
        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, "git", AbsentProgram, this.folder);

        Assert.Null(facts.Xcode.Text);
        Assert.Equal("git -version", facts.Xcode.Source);
        Assert.StartsWith("the exit code 129, stderr: ", facts.Xcode.Absence, StringComparison.Ordinal);
        Assert.Contains("-version", facts.Xcode.Absence, StringComparison.Ordinal);
        Assert.False(ToolchainRules.Evaluate(facts)[0].Holds);
    }

    [Fact]
    public void TheReportGivesZeroWhenEachPinHolds()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();
        PinResult[] results = [new PinResult("Xcode", true, "26.1.1 (D-28)", "26.1.1", string.Empty)];

        int exitCode = ToolchainCheckCommand.Report(results, output, errors);

        Assert.Equal(0, exitCode);
        Assert.Equal(
            $"toolchain-check: Xcode: pass. Expected 26.1.1 (D-28), found 26.1.1.{Environment.NewLine}toolchain-check: each of the 1 pins holds.{Environment.NewLine}",
            output.ToString());
        Assert.Empty(errors.ToString());
    }

    [Fact]
    public void TheReportWritesEachFailedPinToTheErrorsAndGivesTheFaultCode()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();
        PinResult[] results =
        [
            new PinResult("Xcode", true, "26.1.1 (D-28)", "26.1.1", string.Empty),
            new PinResult("Unreal Engine", false, "5.8.3 (D-28)", "5.8.4", "Each hotfix upgrade is its own PR with build evidence (D-28)."),
        ];

        int exitCode = ToolchainCheckCommand.Report(results, output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Equal($"toolchain-check: Xcode: pass. Expected 26.1.1 (D-28), found 26.1.1.{Environment.NewLine}", output.ToString());
        Assert.Equal(
            $"toolchain-check: Unreal Engine: fail. Expected 5.8.3 (D-28), found 5.8.4. Each hotfix upgrade is its own PR with build evidence (D-28).{Environment.NewLine}toolchain-check: 1 of the 2 pins fail.{Environment.NewLine}",
            errors.ToString());
    }

    [Fact]
    public void TheCommandRefusesAnArgument()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run([ToolchainCheckCommand.Name, "--root", "."], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("the option '--root' is unknown", errors.ToString(), StringComparison.Ordinal);
        Assert.Empty(output.ToString());
    }

    [Fact]
    public void TheWindowsScriptNamesTheSamePinsAsTheMacCommand()
    {
        // D-72: the PowerShell script matches the Makefile target, so both read one pin and one variable.
        string script = File.ReadAllText(RepositoryRoot.PathTo("scripts/toolchain-check.ps1"));

        Assert.Contains($"$EngineVariable = '{ToolchainPins.EngineVariable}'", script, StringComparison.Ordinal);
        Assert.Contains($"$EnginePin = '{ToolchainPins.Engine}'", script, StringComparison.Ordinal);
        Assert.Contains("Join-Path (Join-Path (Join-Path $EngineFolder 'Engine') 'Build') 'Build.version'", script, StringComparison.Ordinal);
        Assert.Equal(ToolchainPins.BuildVersionPath, "Engine/Build/Build.version");
    }

    [Fact]
    public void TheWindowsScriptPinsTheToolchainOfTheEpicPage()
    {
        // D-74: Visual Studio 2026 and the MSVC toolset 14.50, from the Epic page read on 2026-09-27. The first
        // 14.50 build that Windows_SDK.json of 5.8.3 does not ban is 14.50.35723.
        string script = File.ReadAllText(RepositoryRoot.PathTo("scripts/toolchain-check.ps1"));

        Assert.Contains("$VisualStudioMajorPin = 18", script, StringComparison.Ordinal);
        Assert.Contains("$MsvcMinimum = [version]'14.50.35723'", script, StringComparison.Ordinal);
        Assert.Contains("$MsvcNextFamily = [version]'14.51.0'", script, StringComparison.Ordinal);

        // The ban reads the product version of cl.exe, not the folder name, as UnrealBuildTool does. The folder
        // 14.50.35717 can hold a serviced cl.exe of 14.50.35723 or later.
        Assert.Contains("bin\\Hostx64\\x64\\cl.exe", script, StringComparison.Ordinal);
        Assert.Contains("$info.ProductMajorPart, $info.ProductMinorPart, $info.ProductBuildPart", script, StringComparison.Ordinal);
        Assert.Contains("$WindowsSdkMinimum = [version]'10.0.22621.0'", script, StringComparison.Ordinal);
        Assert.Contains("read on 2026-09-27 (D-74)", script, StringComparison.Ordinal);
    }
}
