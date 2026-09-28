using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using IronAbsolution.Tools;
using IronAbsolution.Tools.ToolchainCheck;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The I/O of the toolchain check: vswhere, the toolset folders, the SDK folders, the engine
/// version file, an absent program, and the report (D-28, D-74, D-79). A temporary folder stands
/// in for `ProgramFiles(x86)`, and <see cref="StubProgram"/> stands in for vswhere (D-103).
/// </summary>
public sealed class ToolchainCheckCommandTests : IDisposable
{
    private const string AbsentProgram = "/no/such/folder/git";

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"toolchain-check-{Guid.NewGuid():N}");
    private readonly string programFilesX86;
    private readonly string visualStudio;

    public ToolchainCheckCommandTests()
    {
        this.programFilesX86 = Path.Combine(this.folder, "x86");
        this.visualStudio = Path.Combine(this.folder, "VS");
        Directory.CreateDirectory(this.programFilesX86);
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

        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        Assert.Equal(ToolOutput.Found(versionFile, File.ReadAllText(versionFile)), facts.Engine);
        Assert.True(ToolchainRules.Evaluate(facts)[3].Holds);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AnUnsetEngineVariableGivesAnAbsentEngine(string? engineFolder)
    {
        ToolchainFacts facts = ToolchainFacts.Gather(engineFolder, this.programFilesX86, AbsentProgram, this.folder);

        Assert.Null(facts.Engine.Text);
        Assert.Equal("the variable IRON_ABSOLUTION_ENGINE_DIR is not set (D-79)", facts.Engine.Absence);
    }

    [Fact]
    public void AnEngineFolderWithNoVersionFileGivesAnAbsentEngineThatNamesThePath()
    {
        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        string versionFile = Path.Combine(this.folder, "Engine", "Build", "Build.version");
        Assert.Null(facts.Engine.Text);
        Assert.Equal($"no file '{versionFile}'. IRON_ABSOLUTION_ENGINE_DIR names the folder that holds 'Engine'", facts.Engine.Absence);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AnUnsetProgramFolderFailsEachWindowsPinAndTheOtherPinsStillReport(string? programFilesX86)
    {
        // One absent source never hides the other pins (T-2).
        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, programFilesX86, AbsentProgram, this.folder);
        IReadOnlyList<PinResult> results = ToolchainRules.Evaluate(facts);

        Assert.Equal(5, results.Count);
        Assert.Equal("the variable ProgramFiles(x86) is not set, so this is not a Windows PC", facts.VisualStudio.Absence);
        Assert.Equal("the variable ProgramFiles(x86) is not set, so this is not a Windows PC", facts.WindowsSdk.Absence);
        Assert.StartsWith("no Visual Studio with the C++ tools: the variable ProgramFiles(x86) is not set", facts.Msvc.Absence, StringComparison.Ordinal);
        Assert.False(results[0].Holds);
        Assert.False(results[1].Holds);
        Assert.False(results[2].Holds);
    }

    [Fact]
    public void AnAbsentVswhereGivesAnAbsentVisualStudioThatNamesThePath()
    {
        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        string vswhere = ToolPaths.UnderFolder(this.programFilesX86, ToolchainPins.VswherePath);
        Assert.Equal($"no file '{vswhere}'", facts.VisualStudio.Absence);
        Assert.Equal($"no Visual Studio with the C++ tools: no file '{vswhere}'", facts.Msvc.Absence);
    }

    [Fact]
    public void VswhereGetsTheArgumentsOfTheNewestInstallWithTheCppTools()
    {
        string arguments = Path.Combine(this.folder, "arguments.txt");
        new StubProgram().RecordsArgumentsTo(arguments).WritesLine("[]").ExitsWith(this.Vswhere(), 0);

        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        Assert.Equal(
            ["-latest", "-products", "*", "-requires", "Microsoft.VisualStudio.Component.VC.Tools.x86.x64", "-format", "json"],
            File.ReadAllLines(arguments));
        Assert.Equal("[]", facts.VisualStudio.Text?.Trim());
        Assert.Contains("gave no install with the C++ tools", facts.Msvc.Absence, StringComparison.Ordinal);
    }

    [Fact]
    public void AVswhereWithAFaultExitCodeGivesAnAbsentOutputWithTheCode()
    {
        new StubProgram().ExitsWith(this.Vswhere(), 3);

        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        Assert.Null(facts.VisualStudio.Text);
        Assert.StartsWith("the exit code 3, stderr: ", facts.VisualStudio.Absence, StringComparison.Ordinal);
        Assert.False(ToolchainRules.Evaluate(facts)[0].Holds);
    }

    [Fact]
    public void TheGatherReadsEachToolsetFolderOfTheInstallThatVswhereGives()
    {
        this.WriteVswhere();
        string msvc = ToolPaths.UnderFolder(this.visualStudio, ToolchainPins.MsvcFolder);
        string compiler = ToolPaths.UnderFolder(Path.Combine(msvc, "14.50.35717"), ToolchainPins.CompilerPath);
        Directory.CreateDirectory(Path.GetDirectoryName(compiler)!);
        File.WriteAllText(compiler, "not a program");
        Directory.CreateDirectory(Path.Combine(msvc, "14.51.36231"));

        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        Assert.Equal(msvc, facts.Msvc.Source);
        IReadOnlyList<MsvcToolset> toolsets = facts.Msvc.Items ?? throw new InvalidOperationException(facts.Msvc.Absence);
        Assert.Equal(["14.50.35717", "14.51.36231"], [toolsets[0].Folder, toolsets[1].Folder]);

        // A file with no version resource gives the version 0.0.0, so the pin fails with the value.
        Assert.NotNull(toolsets[0].Compiler);
        Assert.Null(toolsets[1].Compiler);
        Assert.False(ToolchainRules.Evaluate(facts)[1].Holds);
    }

    [Fact]
    public void AnInstallWithNoToolsetFolderGivesAnAbsentMsvcThatNamesTheFolder()
    {
        this.WriteVswhere();
        string msvc = ToolPaths.UnderFolder(this.visualStudio, ToolchainPins.MsvcFolder);

        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        Assert.Equal($"no folder '{msvc}'", facts.Msvc.Absence);

        Directory.CreateDirectory(msvc);
        facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        Assert.Equal($"no toolset in '{msvc}'", facts.Msvc.Absence);
    }

    [Fact]
    public void TheGatherReadsEachSdkFolderName()
    {
        string include = ToolPaths.UnderFolder(this.programFilesX86, ToolchainPins.WindowsSdkFolder);
        Directory.CreateDirectory(Path.Combine(include, "10.0.22621.0"));
        Directory.CreateDirectory(Path.Combine(include, "wdf"));

        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        Assert.Equal(include, facts.WindowsSdk.Source);
        IReadOnlyList<string> names = facts.WindowsSdk.Items ?? throw new InvalidOperationException(facts.WindowsSdk.Absence);
        Assert.Equal(["10.0.22621.0", "wdf"], names.Order(StringComparer.Ordinal));
        Assert.True(ToolchainRules.Evaluate(facts)[2].Holds);
    }

    [Fact]
    public void AnAbsentSdkFolderGivesAnAbsentSdkThatNamesTheFolder()
    {
        string include = ToolPaths.UnderFolder(this.programFilesX86, ToolchainPins.WindowsSdkFolder);

        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        Assert.Equal($"no folder '{include}'", facts.WindowsSdk.Absence);
    }

    [Fact]
    public void AGitThatDoesNotStartGivesAnAbsentGitLfsWithItsReason()
    {
        ToolchainFacts facts = ToolchainFacts.Gather(this.folder, this.programFilesX86, AbsentProgram, this.folder);

        Assert.Null(facts.GitLfs.Text);
        Assert.Equal($"{AbsentProgram} lfs version", facts.GitLfs.Source);
        Assert.Contains($"'{AbsentProgram}' did not start", facts.GitLfs.Absence, StringComparison.Ordinal);
        Assert.False(ToolchainRules.Evaluate(facts)[4].Holds);
    }

    [Fact]
    public void TheReportGivesZeroWhenEachPinHolds()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();
        PinResult[] results = [new PinResult("Unreal Engine", true, "5.8.3 (D-28)", "5.8.3", string.Empty)];

        int exitCode = ToolchainCheckCommand.Report(results, output, errors);

        Assert.Equal(0, exitCode);
        Assert.Equal(
            $"toolchain-check: Unreal Engine: pass. Expected 5.8.3 (D-28), found 5.8.3.{Environment.NewLine}toolchain-check: each of the 1 pins holds.{Environment.NewLine}",
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
            new PinResult("Git LFS", true, "an install of any version (D-30)", "3.7.1", string.Empty),
            new PinResult("Unreal Engine", false, "5.8.3 (D-28)", "5.8.4", "Each hotfix upgrade is its own PR with build evidence (D-28)."),
        ];

        int exitCode = ToolchainCheckCommand.Report(results, output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Equal($"toolchain-check: Git LFS: pass. Expected an install of any version (D-30), found 3.7.1.{Environment.NewLine}", output.ToString());
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

    private string Vswhere()
    {
        return ToolPaths.UnderFolder(this.programFilesX86, ToolchainPins.VswherePath);
    }

    /// <summary>Writes a vswhere stub that gives one install in the temporary folder.</summary>
    private void WriteVswhere()
    {
        // The build turns off the reflection of System.Text.Json, so the test escapes the path alone.
        string path = JsonEncodedText.Encode(this.visualStudio).ToString();
        string install = $$"""[ { "displayName": "Visual Studio Community 2026", "installationVersion": "18.10.12217.157", "installationPath": "{{path}}" } ]""";
        new StubProgram().WritesLine(install).ExitsWith(this.Vswhere(), 0);
    }
}
