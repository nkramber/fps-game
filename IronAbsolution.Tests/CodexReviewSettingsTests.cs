using System;
using System.Collections.Generic;
using IronAbsolution.Tools.CodexReview;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The configuration of the review: the Codex arguments, the prompt, the branch of the
/// worktree, the time limit, and the API credential variables (D-14, D-48, D-50, D-53).
/// </summary>
public sealed class CodexReviewSettingsTests
{
    [Fact]
    public void TheReviewRunNamesTheModelTheEffortTheLoginAndTheSandbox()
    {
        // The effort comes from the command line, and never from the configuration file of the machine.
        IReadOnlyList<string> args = CodexReviewSettings.ReviewArguments("/tmp/worktree", "/tmp/last.md", "Review PR #4.");

        Assert.Equal("exec", args[0]);
        AssertPair(args, "-m", "gpt-6-luna");
        AssertPair(args, "-s", "danger-full-access");
        AssertPair(args, "-C", "/tmp/worktree");
        AssertPair(args, "-o", "/tmp/last.md");
        Assert.Contains("model_reasoning_effort=\"medium\"", args);
        Assert.Contains("approval_policy=\"never\"", args);
        Assert.Contains("forced_login_method=\"chatgpt\"", args);
        Assert.Contains("--json", args);
        Assert.Equal("Review PR #4.", args[^1]);
    }

    [Fact]
    public void TheProbeNamesTheSameModelAndEffortWithNoWriteAccess()
    {
        IReadOnlyList<string> args = CodexReviewSettings.ProbeArguments("/tmp/probe");

        AssertPair(args, "-m", CodexReviewSettings.Model);
        AssertPair(args, "-s", "read-only");
        AssertPair(args, "-C", "/tmp/probe");
        Assert.Contains("model_reasoning_effort=\"medium\"", args);
        Assert.Contains("forced_login_method=\"chatgpt\"", args);
        Assert.Contains("--skip-git-repo-check", args);
        Assert.Contains("--ephemeral", args);
    }

    [Fact]
    public void TheReviewPromptNamesTheSkillTheBranchAndThePush()
    {
        string prompt = CodexReviewSettings.ReviewPrompt(4, "feat/pr-3-codex-review");

        Assert.StartsWith("Review PR #4.\n", prompt, StringComparison.Ordinal);
        Assert.Contains("`.claude/skills/pr-review/SKILL.md`", prompt, StringComparison.Ordinal);
        Assert.Contains("`review/pr-4`", prompt, StringComparison.Ordinal);
        Assert.Contains("`git push origin HEAD:feat/pr-3-codex-review`", prompt, StringComparison.Ordinal);
        Assert.Contains("one metadata commit (D-14)", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public void TheReviewSkillExists()
    {
        Assert.True(System.IO.File.Exists(RepositoryRoot.PathTo(CodexReviewSettings.ReviewSkillPath)));
    }

    [Fact]
    public void TheWorktreeTakesABranchOfItsOwn()
    {
        // D-48: the pre-commit hook refuses a commit on no branch (D-43).
        Assert.Equal("review/pr-4", CodexReviewSettings.ReviewBranch(4));
    }

    [Fact]
    public void TheReviewStopsAfterNinetyMinutes()
    {
        Assert.Equal(TimeSpan.FromMinutes(90), CodexReviewSettings.ReviewLimit);
    }

    [Fact]
    public void EveryApiCredentialVariableLeavesTheCodexEnvironment()
    {
        // D-53: the CLI reads a credential from each of these three variables.
        Assert.Equal(["OPENAI_API_KEY", "CODEX_API_KEY", "CODEX_ACCESS_TOKEN"], CodexReviewSettings.ApiCredentialVariables);
    }

    [Fact]
    public void TheLoginStatusJoinsStderrAndStdout()
    {
        // The CLI writes its status line to stderr, and stdout stays empty.
        ProcessResult result = new ProcessResult("codex login status", "/tmp", 0, string.Empty, "Logged in using ChatGPT\n");

        Assert.Equal(CodexReviewSettings.ChatGptLoginStatus, CodexReviewSettings.LoginStatusText(result));
    }

    private static void AssertPair(IReadOnlyList<string> args, string option, string value)
    {
        int index = -1;
        for (int i = 0; i < args.Count; i++)
        {
            if (args[i] == option)
            {
                index = i;
                break;
            }
        }

        Assert.True(index >= 0 && index + 1 < args.Count, $"The arguments hold no '{option}' with a value.");
        Assert.Equal(value, args[index + 1]);
    }
}
