using System;
using System.Collections.Generic;
using IronAbsolution.Tools.ToolchainCheck;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The pins of the Windows toolchain against fixture facts (D-28, D-30, D-74). Exit test 5 of
/// PR-14: a failure names each pin when that pin does not hold (T-2).
/// </summary>
public sealed class ToolchainRulesTests
{
    private const string VswhereSource = "vswhere.exe -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -format json";
    private const string VisualStudio2026 = """[ { "displayName": "Visual Studio Community 2026", "installationVersion": "18.10.12217.157", "installationPath": "C:\\VS" } ]""";
    private const string MsvcSource = "C:\\VS\\VC\\Tools\\MSVC";
    private const string SdkSource = "C:\\Program Files (x86)\\Windows Kits\\10\\Include";
    private const string EnginePinned = """{ "MajorVersion": 5, "MinorVersion": 8, "PatchVersion": 3, "Changelist": 0, "BranchName": "++UE5+Release-5.8" }""";
    private const string EngineSource = "C:\\Program Files\\Epic Games\\UE_5.8\\Engine\\Build\\Build.version";
    private const string GitLfsFound = "git-lfs/3.7.1 (GitHub; windows amd64; go 1.25.1)\n";

    [Fact]
    public void EachPinHoldsOnThePinnedToolchain()
    {
        IReadOnlyList<PinResult> results = ToolchainRules.Evaluate(Facts());

        Assert.Equal(["Visual Studio", "MSVC", "Windows SDK", "Unreal Engine", "Git LFS"], [results[0].Tool, results[1].Tool, results[2].Tool, results[3].Tool, results[4].Tool]);
        Assert.All(results, result => Assert.True(result.Holds, result.Line()));
        Assert.Equal("Visual Studio: pass. Expected Visual Studio 2026, major version 18 (D-74), found Visual Studio Community 2026 18.10.12217.157.", results[0].Line());
        Assert.Equal("MSVC: pass. Expected an installed MSVC toolset with a cl.exe from 14.50.35723 to before 14.51.0 (D-74), found cl.exe 14.50.35739, installed toolsets: 14.50.35717 (cl.exe 14.50.35739).", results[1].Line());
        Assert.Equal("Windows SDK: pass. Expected 10.0.22621.0 or later (the minimum of Epic, D-74), found 10.0.26100.0. Installed: 10.0.19041.0, 10.0.26100.0.", results[2].Line());
        Assert.Equal("Unreal Engine: pass. Expected 5.8.3 (D-28), found 5.8.3.", results[3].Line());
        Assert.Equal("Git LFS: pass. Expected an install of any version (D-30), found 3.7.1.", results[4].Line());
    }

    [Theory]
    [InlineData("17.14.36310.24")]
    [InlineData("19.0.1.0")]
    public void VisualStudioOfAnotherMajorVersionFails(string installationVersion)
    {
        string output = $$"""[ { "displayName": "Visual Studio Community", "installationVersion": "{{installationVersion}}", "installationPath": "C:\\VS" } ]""";

        PinResult visualStudio = ToolchainRules.Evaluate(Facts(visualStudio: ToolOutput.Found(VswhereSource, output)))[0];

        Assert.False(visualStudio.Holds);
        Assert.Equal($"Visual Studio: fail. Expected Visual Studio 2026, major version 18 (D-74), found Visual Studio Community {installationVersion}.", visualStudio.Line());
    }

    [Fact]
    public void AbsentVswhereFailsAndNamesThePath()
    {
        ToolOutput absent = ToolOutput.Absent("C:\\x86\\vswhere.exe", "no file 'C:\\x86\\vswhere.exe'");

        PinResult visualStudio = ToolchainRules.Evaluate(Facts(visualStudio: absent))[0];

        Assert.False(visualStudio.Holds);
        Assert.Equal("Visual Studio: fail. Expected Visual Studio 2026, major version 18 (D-74), found no file 'C:\\x86\\vswhere.exe'. Install Visual Studio 2026 from docs/runbooks/engine-setup.md.", visualStudio.Line());
    }

    [Theory]
    [InlineData("[]", "`" + VswhereSource + "` gave no install with the C++ tools")]
    [InlineData("{}", "the output of `" + VswhereSource + "` is not a JSON array")]
    [InlineData("not json", "the output of `" + VswhereSource + "` is not JSON")]
    [InlineData("""[ { "displayName": "VS", "installationPath": "C:\\VS" } ]""", "the output of `" + VswhereSource + "` has no text in the field 'installationVersion'")]
    [InlineData("""[ { "displayName": "VS", "installationVersion": "18.0", "installationPath": 7 } ]""", "the output of `" + VswhereSource + "` has no text in the field 'installationPath'")]
    public void VswhereOutputOfAnotherFormFailsAndNamesTheCommandAndTheField(string output, string fault)
    {
        PinResult visualStudio = ToolchainRules.Evaluate(Facts(visualStudio: ToolOutput.Found(VswhereSource, output)))[0];

        Assert.False(visualStudio.Holds);
        Assert.Contains($"found {fault}", visualStudio.Line(), StringComparison.Ordinal);
    }

    [Fact]
    public void AVersionOfAnotherFormFailsAndQuotesTheVersion()
    {
        const string output = """[ { "displayName": "VS", "installationVersion": "eighteen", "installationPath": "C:\\VS" } ]""";

        PinResult visualStudio = ToolchainRules.Evaluate(Facts(visualStudio: ToolOutput.Found(VswhereSource, output)))[0];

        Assert.False(visualStudio.Holds);
        Assert.Contains($"found the version 'eighteen' from `{VswhereSource}`, with no version form", visualStudio.Line(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(14, 50, 35722)]
    [InlineData(14, 50, 0)]
    [InlineData(14, 44, 35211)]
    [InlineData(14, 51, 0)]
    [InlineData(14, 51, 36260)]
    public void AToolsetOutsideTheRangeOfTheBuildToolFails(int major, int minor, int build)
    {
        // Windows_SDK.json of 5.8.3 bans 14.50.0 to 14.50.35722, and the next family is 14.51 (D-74).
        MsvcToolset toolset = new MsvcToolset("14.50.35717", new Version(major, minor, build));

        PinResult msvc = MsvcResult(toolset);

        Assert.False(msvc.Holds);
        Assert.Equal(
            $"MSVC: fail. Expected an installed MSVC toolset with a cl.exe from 14.50.35723 to before 14.51.0 (D-74), found installed toolsets: 14.50.35717 (cl.exe {major}.{minor}.{build}). {ToolchainRules.MsvcAdvice}",
            msvc.Line());
    }

    [Fact]
    public void TheCompilerVersionAndNotTheFolderNameDecidesTheToolset()
    {
        // A servicing update changes cl.exe and keeps the folder name, so the folder 14.50.35717 can
        // hold a cl.exe that the build tool accepts. A folder with a good name and an old cl.exe fails.
        PinResult serviced = MsvcResult(new MsvcToolset("14.50.35717", new Version(14, 50, 35723)));
        PinResult oldCompiler = MsvcResult(new MsvcToolset("14.50.35739", new Version(14, 50, 35717)));

        Assert.True(serviced.Holds, serviced.Line());
        Assert.False(oldCompiler.Holds, oldCompiler.Line());
    }

    [Fact]
    public void ANewerDefaultToolsetBesideTheGoodOneStillHolds()
    {
        PinResult msvc = MsvcResult(
            new MsvcToolset("14.50.35717", new Version(14, 50, 35739)),
            new MsvcToolset("14.51.36231", new Version(14, 51, 36260)));

        Assert.True(msvc.Holds, msvc.Line());
        Assert.Contains("found cl.exe 14.50.35739, installed toolsets: 14.50.35717 (cl.exe 14.50.35739), 14.51.36231 (cl.exe 14.51.36260)", msvc.Line(), StringComparison.Ordinal);
    }

    [Fact]
    public void AToolsetWithNoCompilerFailsAndSaysSo()
    {
        PinResult msvc = MsvcResult(new MsvcToolset("14.50.35717", null));

        Assert.False(msvc.Holds);
        Assert.Contains("found installed toolsets: 14.50.35717 (no x64 cl.exe)", msvc.Line(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentToolsetFolderFailsWithTheReason()
    {
        FolderListing<MsvcToolset> absent = FolderListing<MsvcToolset>.Absent(MsvcSource, $"no folder '{MsvcSource}'");

        PinResult msvc = ToolchainRules.Evaluate(Facts(msvc: absent))[1];

        Assert.False(msvc.Holds);
        Assert.Equal(
            $"MSVC: fail. Expected an installed MSVC toolset with a cl.exe from 14.50.35723 to before 14.51.0 (D-74), found no folder '{MsvcSource}'. {ToolchainRules.MsvcAdvice}",
            msvc.Line());
    }

    [Fact]
    public void AnSdkBeforeTheMinimumFailsAndNamesEachInstalledSdk()
    {
        PinResult sdk = SdkResult("10.0.19041.0", "10.0.22000.0");

        Assert.False(sdk.Holds);
        Assert.Equal(
            "Windows SDK: fail. Expected 10.0.22621.0 or later (the minimum of Epic, D-74), found 10.0.22000.0. Installed: 10.0.19041.0, 10.0.22000.0. Add a Windows 11 SDK in the Visual Studio Installer.",
            sdk.Line());
    }

    [Fact]
    public void AnSdkFolderWithNoVersionFolderFailsAndNamesTheFolder()
    {
        // The Include folder can hold names such as `wdf`, and a name of three numbers is no SDK folder.
        PinResult sdk = SdkResult("wdf", "10.0.22621");

        Assert.False(sdk.Holds);
        Assert.Contains($"found no version folder in '{SdkSource}'", sdk.Line(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentSdkFolderFailsWithTheReason()
    {
        FolderListing<string> absent = FolderListing<string>.Absent("$ProgramFiles(x86)", "the variable ProgramFiles(x86) is not set, so this is not a Windows PC");

        PinResult sdk = ToolchainRules.Evaluate(Facts(sdk: absent))[2];

        Assert.False(sdk.Holds);
        Assert.Contains("found the variable ProgramFiles(x86) is not set, so this is not a Windows PC.", sdk.Line(), StringComparison.Ordinal);
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

        PinResult engine = ToolchainRules.Evaluate(Facts(engine: unset))[3];

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

        PinResult gitLfs = ToolchainRules.Evaluate(Facts(gitLfs: absent))[4];

        Assert.False(gitLfs.Holds);
        Assert.Equal(
            "Git LFS: fail. Expected an install of any version (D-30), found no Git LFS. `git lfs version` failed: the exit code 1, stderr: git: 'lfs' is not a git command. See 'git --help'. Install Git for Windows with Git LFS, then run `git lfs install`.",
            gitLfs.Line());
    }

    [Fact]
    public void GitLfsOutputOfAnotherFormFails()
    {
        PinResult gitLfs = ToolchainRules.Evaluate(Facts(gitLfs: ToolOutput.Found("git lfs version", "lfs 3.7.1\n")))[4];

        Assert.False(gitLfs.Holds);
        Assert.Contains("found the line 'lfs 3.7.1' from `git lfs version`", gitLfs.Line(), StringComparison.Ordinal);
    }

    private static PinResult MsvcResult(params MsvcToolset[] toolsets)
    {
        return ToolchainRules.Evaluate(Facts(msvc: FolderListing<MsvcToolset>.Found(MsvcSource, toolsets)))[1];
    }

    private static PinResult SdkResult(params string[] folders)
    {
        return ToolchainRules.Evaluate(Facts(sdk: FolderListing<string>.Found(SdkSource, folders)))[2];
    }

    private static PinResult EngineResult(string text)
    {
        return ToolchainRules.Evaluate(Facts(engine: ToolOutput.Found(EngineSource, text)))[3];
    }

    private static ToolchainFacts Facts(
        ToolOutput? visualStudio = null,
        FolderListing<MsvcToolset>? msvc = null,
        FolderListing<string>? sdk = null,
        ToolOutput? engine = null,
        ToolOutput? gitLfs = null)
    {
        return new ToolchainFacts(
            visualStudio ?? ToolOutput.Found(VswhereSource, VisualStudio2026),
            msvc ?? FolderListing<MsvcToolset>.Found(MsvcSource, [new MsvcToolset("14.50.35717", new Version(14, 50, 35739))]),
            sdk ?? FolderListing<string>.Found(SdkSource, ["10.0.19041.0", "10.0.26100.0"]),
            engine ?? ToolOutput.Found(EngineSource, EnginePinned),
            gitLfs ?? ToolOutput.Found("git lfs version", GitLfsFound));
    }
}
