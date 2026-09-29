using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// Runs `run.ps1`, the entry of the development commands, under PowerShell (D-99). Each test of a
/// start condition gives the child a PATH that holds no program, or one program alone, so the
/// result does not read the machine. Each fault names the target and what to do (T-2). A test of
/// an engine target copies the script into a temporary checkout, because the script takes the
/// checkout from its own folder, and <see cref="StubProgram"/> stands in for the batch files of
/// the engine (D-103). <see cref="PowerShellScript"/> runs the script, and CI must run these tests.
/// </summary>
public sealed class EntryScriptTests : IDisposable
{
    /// <summary>Each target that `help` prints, and that the fault of an unknown target names.</summary>
    private static readonly string[] EveryTarget =
    [
        "verify", "build", "test", "format", "ste-check",
        "handoff-rotate", "codex-review", "hooks", "where", "clean",
        "toolchain-check", "editor-build", "editor-test", "content-build", "package-build", "package-run", "help",
    ];

    private const string EngineVariable = "IRON_ABSOLUTION_ENGINE_DIR";

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"entry-script-{Guid.NewGuid():N}");
    private readonly string root;
    private readonly string engine;

    public EntryScriptTests()
    {
        this.root = Path.Combine(this.folder, "checkout");
        this.engine = Path.Combine(this.folder, "engine");
        Directory.CreateDirectory(this.root);
        File.Copy(RepositoryRoot.PathTo("run.ps1"), Path.Combine(this.root, "run.ps1"));
    }

    public void Dispose()
    {
        Directory.Delete(this.folder, recursive: true);
    }

    [Fact]
    public void TheHelpTargetPrintsEveryTargetWithItsLine()
    {
        (int exitCode, string output) = this.Run(["help"], path: null);

        Assert.Equal(0, exitCode);
        foreach (string target in EveryTarget)
        {
            Assert.Contains(target, output, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void AnUnknownTargetFailsAndNamesEveryTarget()
    {
        (int exitCode, string output) = this.Run(["nope"], path: null);

        Assert.Equal(1, exitCode);
        Assert.Contains("run.ps1: no target 'nope'.", output, StringComparison.Ordinal);
        foreach (string target in EveryTarget)
        {
            Assert.Contains(target, output, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheCodexReviewTargetWithNoPullRequestNumberFailsAndNamesTheOption()
    {
        // D-14 gives the target the PR number, and a run with none must not start a review.
        (int exitCode, string output) = this.Run(["codex-review"], path: null);

        Assert.Equal(1, exitCode);
        Assert.Contains("run.ps1: codex-review: set -PR <number>", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("verify")]
    [InlineData("build")]
    [InlineData("test")]
    [InlineData("format")]
    [InlineData("ste-check")]
    [InlineData("handoff-rotate")]
    [InlineData("clean")]
    [InlineData("toolchain-check")]
    [InlineData("editor-test")]
    [InlineData("package-run")]
    public void ATargetOfTheToolsFailsWithThePathWhenTheDotnetSdkIsAbsent(string target)
    {
        (int exitCode, string output) = this.Run([target], path: this.folder);

        Assert.Equal(1, exitCode);
        Assert.Contains($"run.ps1: {target}: 'dotnet' is not on the PATH.", output, StringComparison.Ordinal);
        Assert.Contains("Install the .NET SDK of global.json.", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("hooks")]
    [InlineData("where")]
    public void ATargetOfGitFailsAndNamesGitWhenItIsAbsent(string target)
    {
        (int exitCode, string output) = this.Run([target], path: this.folder);

        Assert.Equal(1, exitCode);
        Assert.Contains($"run.ps1: {target}: 'git' is not on the PATH.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCodexReviewTargetFailsAndNamesNpmWhenItIsAbsent()
    {
        // The PATH holds the .NET SDK alone, so the run reaches the npm check of D-47.
        string dotnetFolder = Path.GetDirectoryName(PowerShellScript.ProgramPath("dotnet"))!;
        Assert.True(Directory.Exists(dotnetFolder), $"No folder of 'dotnet' on the PATH: '{dotnetFolder}'.");

        (int exitCode, string output) = this.Run(["codex-review", "-PR", "13"], path: dotnetFolder);

        Assert.Equal(1, exitCode);
        Assert.Contains("run.ps1: codex-review: 'npm' is not on the PATH.", output, StringComparison.Ordinal);
        Assert.Contains("(D-47)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheHelpLineOfThePackageRunSaysThatItOpensAGameWindow()
    {
        // D-96: a session asks the owner before a command that opens a game window.
        (_, string output) = this.Run(["help"], path: null);

        Assert.Contains("start the package for a timed run of the test map. It opens a game window (D-89, D-96).", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("editor-build")]
    [InlineData("content-build")]
    [InlineData("package-build")]
    public void ABuildTargetFailsAndNamesTheVariableWhenItIsNotSet(string target)
    {
        (int exitCode, string output) = this.RunEngineTarget(target, engineFolder: null);

        Assert.Equal(1, exitCode);
        Assert.Equal($"run.ps1: {target}: set IRON_ABSOLUTION_ENGINE_DIR to the folder that holds 'Engine' (D-79).", output.Trim());
    }

    [Theory]
    [InlineData("editor-build", "Build.bat")]
    [InlineData("package-build", "RunUAT.bat")]
    public void ABuildTargetFailsWithThePathWhenTheBatchFileIsAbsent(string target, string batchFile)
    {
        (int exitCode, string output) = this.RunEngineTarget(target, this.engine);

        Assert.Equal(1, exitCode);
        Assert.Contains($"run.ps1: {target}: no file '{this.BatchFile(batchFile)}'.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheContentBuildFailsWithThePathWhenTheEditorIsAbsent()
    {
        (int exitCode, string output) = this.RunEngineTarget("content-build", this.engine);

        Assert.Equal(1, exitCode);
        Assert.Contains($"run.ps1: content-build: no file '{this.EditorProgram()}'.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheContentBuildGivesTheExitCodeTheArgumentsAndTheLogOfTheEditor()
    {
        // A fault of the script gives a nonzero exit code, and the target names the log (D-134, T-2).
        string argumentsFile = Path.Combine(this.folder, "arguments.txt");
        new StubProgram().RecordsArgumentsTo(argumentsFile).WritesLine("LogPython: Error: Traceback").ExitsWith(this.EditorProgram(), exitCode: 7);

        (int exitCode, string output) = this.RunEngineTarget("content-build", this.engine);

        Assert.Equal(7, exitCode);
        string log = Path.Combine(this.root, "Game", "Saved", "Logs", "content-build.log");
        Assert.Contains($"run.ps1: content-build failed with the exit code 7. Read the log '{log}'.", output, StringComparison.Ordinal);
        Assert.Contains("LogPython: Error: Traceback", File.ReadAllText(log), StringComparison.Ordinal);
        Assert.Equal(
            [
                Path.Combine(this.root, "Game", "IronAbsolution.uproject"),
                "-run=pythonscript",
                $"-script={Path.Combine(this.root, "Game", "Scripts", "build_content.py")}",
                "-unattended",
                "-nullrhi",
                "-nosplash",
                "-nosound",
                "-stdout",
                "-FullStdOutLogOutput",
            ],
            File.ReadAllLines(argumentsFile));
    }

    [Fact]
    public void TheContentBuildPassesWhenTheEditorGivesZero()
    {
        new StubProgram().WritesLine("LogPython: build_content: pass.").ExitsWith(this.EditorProgram(), exitCode: 0);

        (int exitCode, string output) = this.RunEngineTarget("content-build", this.engine);

        Assert.Equal(0, exitCode);
        Assert.Contains("run.ps1: content-build passed.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheEditorBuildGivesTheExitCodeOfBuildBatAndTheArguments()
    {
        string argumentsFile = Path.Combine(this.folder, "arguments.txt");
        new StubProgram().RecordsArgumentsTo(argumentsFile).ExitsWith(this.BatchFile("Build.bat"), exitCode: 7);

        (int exitCode, string output) = this.RunEngineTarget("editor-build", this.engine);

        Assert.Equal(7, exitCode);
        Assert.Contains("run.ps1: editor-build failed with the exit code 7.", output, StringComparison.Ordinal);
        Assert.Equal(
            [
                "IronAbsolutionEditor",
                "Win64",
                "Development",
                $"-Project={Path.Combine(this.root, "Game", "IronAbsolution.uproject")}",
                "-WaitMutex",
                $"-Log={Path.Combine(this.root, "Game", "Saved", "Logs", "editor-build.log")}",
            ],
            File.ReadAllLines(argumentsFile));
    }

    [Fact]
    public void TheEditorBuildPassesWhenBuildBatGivesZero()
    {
        new StubProgram().ExitsWith(this.BatchFile("Build.bat"), exitCode: 0);

        (int exitCode, string output) = this.RunEngineTarget("editor-build", this.engine);

        Assert.Equal(0, exitCode);
        Assert.Contains("run.ps1: editor-build passed.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePackageBuildRemovesTheOldPackageAndGivesTheExitCodeTheArgumentsAndTheLog()
    {
        // A failed build must leave no old package for `package-run` (T-2).
        string oldPackage = Path.Combine(this.root, "Game", "Saved", "Packages", "Windows");
        Directory.CreateDirectory(oldPackage);
        string argumentsFile = Path.Combine(this.folder, "arguments.txt");
        new StubProgram().RecordsArgumentsTo(argumentsFile).WritesLine("BUILD FAILED").ExitsWith(this.BatchFile("RunUAT.bat"), exitCode: 7);

        (int exitCode, string output) = this.RunEngineTarget("package-build", this.engine);

        Assert.Equal(7, exitCode);
        string log = Path.Combine(this.root, "Game", "Saved", "Logs", "package-build.log");
        Assert.Contains($"run.ps1: package-build failed with the exit code 7. Read the log '{log}'.", output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(oldPackage), "The target left the old package.");
        Assert.Contains("BUILD FAILED", File.ReadAllText(log), StringComparison.Ordinal);
        string archive = Path.Combine(this.root, "Game", "Saved", "Packages");
        Assert.Equal(
            [
                "BuildCookRun",
                $"-project={Path.Combine(this.root, "Game", "IronAbsolution.uproject")}",
                "-platform=Win64",
                "-clientconfig=Development",
                "-build",
                "-cook",
                "-stage",
                "-package",
                "-pak",
                "-archive",
                $"-archivedirectory={archive}",
                "-unattended",
                "-utf8output",
                "-nop4",
            ],
            File.ReadAllLines(argumentsFile));
    }

    [Fact]
    public void ThePackageBuildPassesWhenRunUatGivesZero()
    {
        new StubProgram().WritesLine("BUILD SUCCESSFUL").ExitsWith(this.BatchFile("RunUAT.bat"), exitCode: 0);

        (int exitCode, string output) = this.RunEngineTarget("package-build", this.engine);

        Assert.Equal(0, exitCode);
        Assert.Contains("run.ps1: package-build passed.", output, StringComparison.Ordinal);
    }

    /// <summary>Gives the path of the console editor of the engine, under the temporary engine folder.</summary>
    private string EditorProgram()
    {
        return Path.Combine(this.engine, "Engine", "Binaries", "Win64", "UnrealEditor-Cmd.exe");
    }

    /// <summary>Gives the path of one batch file of the engine, under the temporary engine folder.</summary>
    private string BatchFile(string name)
    {
        return Path.Combine(this.engine, "Engine", "Build", "BatchFiles", name);
    }

    /// <summary>
    /// Runs an engine target of the copy of the script in the temporary checkout, so the logs and
    /// the package go there. The child gets the engine variable of the test alone.
    /// </summary>
    private (int ExitCode, string Output) RunEngineTarget(string target, string? engineFolder)
    {
        return PowerShellScript.Run(
            Path.Combine(this.root, "run.ps1"),
            this.folder,
            new Dictionary<string, string?> { [EngineVariable] = engineFolder },
            [target]);
    }

    /// <summary>Runs the entry script with arguments, and with a PATH of this test.</summary>
    /// <param name="arguments">The arguments after the script path.</param>
    /// <param name="path">The PATH of the child, or null to keep the PATH of this process.</param>
    private (int ExitCode, string Output) Run(IReadOnlyList<string> arguments, string? path)
    {
        Dictionary<string, string?> environment = new Dictionary<string, string?>();
        if (path is not null)
        {
            environment["PATH"] = path;
        }

        return PowerShellScript.Run(RepositoryRoot.PathTo("run.ps1"), this.folder, environment, arguments);
    }
}
