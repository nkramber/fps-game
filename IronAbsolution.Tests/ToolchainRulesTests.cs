using System;
using System.Collections.Generic;
using IronAbsolution.Tools.ToolchainCheck;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The pins of the Mac toolchain against fixture output (D-28, D-30). Exit test 2 of PR-8:
/// the rules refuse Xcode 26.4 and later, and each Xcode before 26.0. Exit test 3: a failure
/// names the pin when Xcode, the engine, or Git LFS is absent (T-2).
/// </summary>
public sealed class ToolchainRulesTests
{
    private const string XcodePinned = "Xcode 26.1.1\nBuild version 17B100\n";
    private const string EnginePinned = """{ "MajorVersion": 5, "MinorVersion": 8, "PatchVersion": 3, "Changelist": 0, "BranchName": "++UE5+Release-5.8" }""";
    private const string GitLfsFound = "git-lfs/3.7.1 (GitHub; darwin arm64; go 1.25.3)\n";
    private const string EngineSource = "/Volumes/IronAbsolution/Epic Games/UE_5.8/Engine/Build/Build.version";

    [Fact]
    public void EachPinHoldsOnThePinnedToolchain()
    {
        IReadOnlyList<PinResult> results = ToolchainRules.Evaluate(Facts());

        Assert.Equal(["Xcode", "Unreal Engine", "Git LFS"], [results[0].Tool, results[1].Tool, results[2].Tool]);
        Assert.All(results, result => Assert.True(result.Holds, result.Line()));
        Assert.Equal("Xcode: pass. Expected 26.1.1 (D-28), found 26.1.1.", results[0].Line());
        Assert.Equal("Unreal Engine: pass. Expected 5.8.3 (D-28), found 5.8.3.", results[1].Line());
        Assert.Equal("Git LFS: pass. Expected an install of any version (D-30), found 3.7.1.", results[2].Line());
    }

    [Theory]
    [InlineData("Xcode 26.4", "26.4.0")]
    [InlineData("Xcode 26.4.1", "26.4.1")]
    [InlineData("Xcode 26.5", "26.5.0")]
    [InlineData("Xcode 27.0", "27.0.0")]
    public void XcodeFromTheFirstRefusedVersionOnFails(string firstLine, string found)
    {
        PinResult xcode = XcodeResult($"{firstLine}\nBuild version 17E000\n");

        Assert.False(xcode.Holds);
        Assert.Equal($"Xcode: fail. Expected 26.1.1 (D-28), found {found}. Xcode 26.4.0 and later do not work with Unreal Engine 5.8 (F-3).", xcode.Line());
    }

    [Theory]
    [InlineData("Xcode 16.2", "16.2.0")]
    [InlineData("Xcode 25.9.9", "25.9.9")]
    public void XcodeBeforeTheMinimumFails(string firstLine, string found)
    {
        PinResult xcode = XcodeResult($"{firstLine}\nBuild version 16C5032a\n");

        Assert.False(xcode.Holds);
        Assert.Equal($"Xcode: fail. Expected 26.1.1 (D-28), found {found}. Unreal Engine 5.8 needs Xcode 26.0.0 or later (F-3).", xcode.Line());
    }

    [Theory]
    [InlineData("Xcode 26.0")]
    [InlineData("Xcode 26.1")]
    [InlineData("Xcode 26.3")]
    public void XcodeInsideTheRangeOfEpicFailsWhenItIsNotThePin(string firstLine)
    {
        // D-28 pins one version, so a version that Epic accepts still fails.
        PinResult xcode = XcodeResult($"{firstLine}\nBuild version 17A000\n");

        Assert.False(xcode.Holds);
        Assert.EndsWith("D-28 pins one Xcode version, 26.1.1.", xcode.Line(), StringComparison.Ordinal);
    }

    [Fact]
    public void AbsentXcodeFailsAndNamesThePin()
    {
        ToolOutput commandLineToolsOnly = ToolOutput.Absent(
            "xcodebuild -version",
            "the exit code 1, stderr: xcode-select: error: tool 'xcodebuild' requires Xcode, but active developer directory '/Library/Developer/CommandLineTools' is a command line tools instance");

        PinResult xcode = ToolchainRules.Evaluate(Facts(xcode: commandLineToolsOnly))[0];

        Assert.False(xcode.Holds);
        Assert.StartsWith("Xcode: fail. Expected 26.1.1 (D-28), found no Xcode app. `xcodebuild -version` failed: the exit code 1, stderr: xcode-select: error:", xcode.Line(), StringComparison.Ordinal);
        Assert.EndsWith("Select the Xcode app with `sudo xcode-select -s <path of Xcode-26.1.1.app>`.", xcode.Line(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Apple Xcode 26.1.1")]
    [InlineData("Xcode twenty-six")]
    [InlineData("")]
    public void XcodeOutputOfAnotherFormFailsAndQuotesTheLine(string firstLine)
    {
        PinResult xcode = XcodeResult($"{firstLine}\n");

        Assert.False(xcode.Holds);
        Assert.Contains($"found the line '{firstLine}' from `xcodebuild -version`", xcode.Line(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherEngineHotfixFailsAndNamesTheUpgradeRule()
    {
        // The launcher installs each new hotfix by itself (D-28).
        PinResult engine = EngineResult("""{ "MajorVersion": 5, "MinorVersion": 8, "PatchVersion": 4 }""");

        Assert.False(engine.Holds);
        Assert.Equal("Unreal Engine: fail. Expected 5.8.3 (D-28), found 5.8.4. Each hotfix upgrade is its own PR with build evidence (D-28).", engine.Line());
    }

    [Fact]
    public void AnUnsetEngineVariableFailsAndNamesTheVariable()
    {
        ToolOutput unset = ToolOutput.Absent("$IRON_ABSOLUTION_ENGINE_DIR", "the variable IRON_ABSOLUTION_ENGINE_DIR is not set (D-79)");

        PinResult engine = ToolchainRules.Evaluate(Facts(engine: unset))[1];

        Assert.False(engine.Holds);
        Assert.Equal("Unreal Engine: fail. Expected 5.8.3 (D-28), found no engine: the variable IRON_ABSOLUTION_ENGINE_DIR is not set (D-79).", engine.Line());
    }

    [Theory]
    [InlineData("""{ "MajorVersion": 5, "MinorVersion": 8 }""", "has no whole number in the field 'PatchVersion'")]
    [InlineData("""{ "MajorVersion": "5", "MinorVersion": 8, "PatchVersion": 3 }""", "has no whole number in the field 'MajorVersion'")]
    [InlineData("""[5, 8, 3]""", "has no whole number in the field 'MajorVersion'")]
    [InlineData("not json", "is not JSON")]
    public void AVersionFileOfAnotherFormFailsAndNamesTheFileAndTheField(string text, string fault)
    {
        PinResult engine = EngineResult(text);

        Assert.False(engine.Holds);
        Assert.Contains($"found the file '{EngineSource}' {fault}", engine.Line(), StringComparison.Ordinal);
    }

    [Fact]
    public void AbsentGitLfsFailsAndNamesThePin()
    {
        ToolOutput absent = ToolOutput.Absent("git lfs version", "the exit code 1, stderr: git: 'lfs' is not a git command. See 'git --help'.");

        PinResult gitLfs = ToolchainRules.Evaluate(Facts(gitLfs: absent))[2];

        Assert.False(gitLfs.Holds);
        Assert.Equal(
            "Git LFS: fail. Expected an install of any version (D-30), found no Git LFS. `git lfs version` failed: the exit code 1, stderr: git: 'lfs' is not a git command. See 'git --help'. Install it with `brew install git-lfs`, then run `git lfs install`.",
            gitLfs.Line());
    }

    [Fact]
    public void GitLfsOutputOfAnotherFormFails()
    {
        PinResult gitLfs = ToolchainRules.Evaluate(Facts(gitLfs: ToolOutput.Found("git lfs version", "lfs 3.7.1\n")))[2];

        Assert.False(gitLfs.Holds);
        Assert.Contains("found the line 'lfs 3.7.1' from `git lfs version`", gitLfs.Line(), StringComparison.Ordinal);
    }

    private static PinResult XcodeResult(string output)
    {
        return ToolchainRules.Evaluate(Facts(xcode: ToolOutput.Found("xcodebuild -version", output)))[0];
    }

    private static PinResult EngineResult(string text)
    {
        return ToolchainRules.Evaluate(Facts(engine: ToolOutput.Found(EngineSource, text)))[1];
    }

    private static ToolchainFacts Facts(ToolOutput? xcode = null, ToolOutput? engine = null, ToolOutput? gitLfs = null)
    {
        return new ToolchainFacts(
            xcode ?? ToolOutput.Found("xcodebuild -version", XcodePinned),
            engine ?? ToolOutput.Found(EngineSource, EnginePinned),
            gitLfs ?? ToolOutput.Found("git lfs version", GitLfsFound));
    }
}
