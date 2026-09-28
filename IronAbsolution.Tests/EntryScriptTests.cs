using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// Runs `run.ps1`, the entry of the development commands, under PowerShell (D-99). Each test of a
/// start condition gives the child a PATH that holds no program, or one program alone, so the
/// result does not read the machine. Each fault names the target and what to do (T-2).
/// <see cref="PowerShellScript"/> runs the script, and CI must run these tests.
/// </summary>
public sealed class EntryScriptTests : IDisposable
{
    /// <summary>Each target that `help` prints, and that the fault of an unknown target names.</summary>
    private static readonly string[] EveryTarget =
    [
        "verify", "build", "test", "format", "ste-check",
        "handoff-rotate", "codex-review", "hooks", "where", "clean", "help",
    ];

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"entry-script-{Guid.NewGuid():N}");

    public EntryScriptTests()
    {
        Directory.CreateDirectory(this.folder);
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
