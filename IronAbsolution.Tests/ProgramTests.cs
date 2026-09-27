using System;
using System.IO;
using IronAbsolution.Tools;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The command line of the tools project. A run with no command, an unknown command, or a
/// planned command gives the fault code and a message that names the fault (T-2, G-8).
/// </summary>
public sealed class ProgramTests
{
    [Fact]
    public void ARunWithNoCommandIsAFaultThatListsTheCommands()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run([], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("no command", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("ste-check: ready", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("codex-review: ready", errors.ToString(), StringComparison.Ordinal);
        Assert.Empty(output.ToString());
    }

    [Fact]
    public void AnUnknownCommandIsAFaultThatNamesTheCommand()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["no-such-command"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("unknown command 'no-such-command'", errors.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("doc-gate", "PR-4")]
    [InlineData("handoff-rotate", "PR-4")]
    [InlineData("review-gate", "PR-6")]
    public void APlannedCommandIsAFaultThatNamesThePullRequest(string command, string pullRequest)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run([command], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"{pullRequest} adds it", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void EachPlannedCommandNamesAPullRequestOfTheSequence()
    {
        // Section 8 of the design doc gives every PR, so a planned command names one of them (G-8).
        string sequence = File.ReadAllText(RepositoryRoot.PathTo("docs/design.md"));

        foreach (string pullRequest in Program.PlannedCommands.Values)
        {
            Assert.Contains($"{pullRequest}:", sequence, StringComparison.Ordinal);
        }
    }
}
