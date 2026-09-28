using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// Runs `scripts/toolchain-check.ps1` under PowerShell with a temporary engine folder (D-72). A
/// fault in one source fails its own pin, and the script still reports each later pin and the
/// total (T-2). <see cref="PowerShellScript"/> runs the script, and CI must run these tests.
/// </summary>
public sealed class ToolchainScriptTests : IDisposable
{
    private const string EngineVariable = "IRON_ABSOLUTION_ENGINE_DIR";

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"toolchain-script-{Guid.NewGuid():N}");

    public ToolchainScriptTests()
    {
        Directory.CreateDirectory(this.folder);
    }

    public void Dispose()
    {
        Directory.Delete(this.folder, recursive: true);
    }

    [Theory]
    [InlineData("not json", "is not JSON")]
    [InlineData("""{ "MajorVersion": 5, "MinorVersion": 8 }""", "has no whole number in the field 'PatchVersion'")]
    [InlineData("""{ "MajorVersion": "5", "MinorVersion": 8, "PatchVersion": 3 }""", "has no whole number in the field 'MajorVersion'")]
    [InlineData("""[5, 8, 3]""", "has no whole number in the field 'MajorVersion'")]
    public void AnInvalidVersionFileFailsThePinAndTheLaterPinsStillReport(string text, string fault)
    {
        // P2-1 of the review of PR #9: on the old script, a bad file stopped the run before the pin, Git LFS, and the total.
        string versionFile = this.WriteVersionFile(text);

        (int exitCode, string output) = this.RunScript(this.folder);

        Assert.Equal(1, exitCode);
        Assert.Contains($"toolchain-check: Unreal Engine: fail. Expected 5.8.3 (D-28), found the file '{versionFile}' {fault}", output, StringComparison.Ordinal);
        Assert.Contains("toolchain-check: Git LFS: ", output, StringComparison.Ordinal);
        Assert.Contains(" of the 5 pins fail.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentVersionFileFailsThePinAndTheLaterPinsStillReport()
    {
        (int exitCode, string output) = this.RunScript(this.folder);

        Assert.Equal(1, exitCode);
        Assert.Contains("toolchain-check: Unreal Engine: fail. Expected 5.8.3 (D-28), found no engine: no file ", output, StringComparison.Ordinal);
        Assert.Contains("toolchain-check: Git LFS: ", output, StringComparison.Ordinal);
        Assert.Contains(" of the 5 pins fail.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePinnedVersionFilePassesThePin()
    {
        this.WriteVersionFile("""{ "MajorVersion": 5, "MinorVersion": 8, "PatchVersion": 3, "BranchName": "++UE5+Release-5.8" }""");

        (_, string output) = this.RunScript(this.folder);

        Assert.Contains("toolchain-check: Unreal Engine: pass. Expected 5.8.3 (D-28), found 5.8.3.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void AMachineWithNoVisualStudioFailsEachWindowsPinAndStillReportsTheRest()
    {
        // The runner is not a Windows PC, so the Visual Studio, MSVC, and SDK sources each fail with a reason.
        (int exitCode, string output) = this.RunScript(engineFolder: null);

        Assert.Equal(1, exitCode);
        Assert.Contains("toolchain-check: Visual Studio: fail.", output, StringComparison.Ordinal);
        Assert.Contains("toolchain-check: MSVC: fail.", output, StringComparison.Ordinal);
        Assert.Contains("toolchain-check: Windows SDK: fail.", output, StringComparison.Ordinal);
        Assert.Contains("toolchain-check: Unreal Engine: fail. Expected 5.8.3 (D-28), found no engine: the variable IRON_ABSOLUTION_ENGINE_DIR is not set (D-79).", output, StringComparison.Ordinal);
        Assert.Contains("toolchain-check: Git LFS: ", output, StringComparison.Ordinal);
        Assert.Contains(" of the 5 pins fail.", output, StringComparison.Ordinal);
    }

    private string WriteVersionFile(string text)
    {
        string buildFolder = Path.Combine(this.folder, "Engine", "Build");
        Directory.CreateDirectory(buildFolder);
        string versionFile = Path.Combine(buildFolder, "Build.version");
        File.WriteAllText(versionFile, text);
        return versionFile;
    }

    private (int ExitCode, string Output) RunScript(string? engineFolder)
    {
        return PowerShellScript.Run(RepositoryRoot.PathTo("scripts/toolchain-check.ps1"), this.folder, EngineVariable, engineFolder);
    }
}
