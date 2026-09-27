using System;
using System.Collections.Generic;
using IronAbsolution.Tools.CodexReview;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>The conditions before a review round starts (D-14, D-49, D-52, D-53). Each failed condition gives one problem.</summary>
public sealed class StartChecksTests
{
    private const string Head = "1111111111111111111111111111111111111111";
    private const string Branch = "feat/pr-3-codex-review";

    [Fact]
    public void TheChecksPassWhenEveryConditionHolds()
    {
        Assert.Empty(StartChecks.Problems(Facts()));
    }

    public static TheoryData<string, string> FailedConditions()
    {
        return new TheoryData<string, string>
        {
            { "old-cli", "minimum is 0.156.1" },
            { "api-login", "never uses API pricing" },
            { "no-login", "never uses API pricing" },
            { "merged", "is MERGED" },
            { "other-branch", "The checkout is on 'main'" },
            { "local-ahead", "differs from origin" },
            { "github-behind", "GitHub gives the PR head" },
            { "changes", "The working tree has changes" },
            { "open-thread", "2 unresolved review thread(s)" },
            { "documents-only", "merges through the review-override label" },
        };
    }

    [Theory]
    [MemberData(nameof(FailedConditions))]
    public void EachFailedConditionGivesOneProblem(string change, string expected)
    {
        StartFacts facts = change switch
        {
            "old-cli" => Facts(version: "codex-cli 0.155.0-alpha.9.2"),
            "api-login" => Facts(loginStatus: "Logged in using an API key - sk-proj-***\n"),
            "no-login" => Facts(loginStatus: "Not logged in\n"),
            "merged" => Facts(state: "MERGED"),
            "other-branch" => Facts(localBranch: "main"),
            "local-ahead" => Facts(localHead: "3333333333333333333333333333333333333333"),
            "github-behind" => Facts(pullRequestHead: "4444444444444444444444444444444444444444"),
            "changes" => Facts(status: " M Makefile\n"),
            "open-thread" => Facts(unresolved: 2),
            "documents-only" => Facts(effectiveHead: null),
            _ => throw new ArgumentException($"Unknown change '{change}'."),
        };

        string problem = Assert.Single(StartChecks.Problems(facts));

        Assert.Contains(expected, problem, StringComparison.Ordinal);
    }

    [Fact]
    public void AnApiKeyLoginRefusesTheStartAndNamesTheLogin()
    {
        // Exit test 3 of PR-3 (D-53): a login with an API key alone never starts a review.
        string problem = Assert.Single(StartChecks.Problems(Facts(loginStatus: "Logged in using an API key - sk-proj-***")));

        Assert.Contains("D-53", problem, StringComparison.Ordinal);
        Assert.Contains("codex login", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void ADocumentsOnlyPullRequestNamesTheRuleAndTheLabel()
    {
        string problem = Assert.Single(StartChecks.Problems(Facts(effectiveHead: null)));

        Assert.Contains("D-49", problem, StringComparison.Ordinal);
        Assert.Contains("merges through the review-override label", problem, StringComparison.Ordinal);
        Assert.Contains("D-66", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryFailedConditionGivesItsOwnProblem()
    {
        IReadOnlyList<string> problems = StartChecks.Problems(Facts(version: "codex-cli 0.155.0", status: " M Makefile\n", unresolved: 1));

        Assert.Equal(3, problems.Count);
    }

    private static StartFacts Facts(
        string version = "codex-cli 0.156.1",
        string loginStatus = "Logged in using ChatGPT\n",
        string state = "OPEN",
        string localBranch = Branch,
        string localHead = Head,
        string pullRequestHead = Head,
        string status = "",
        string? effectiveHead = Head,
        int unresolved = 0)
    {
        return new StartFacts
        {
            PullRequestNumber = 4,
            Version = CodexVersion.Parse(version),
            LoginStatus = loginStatus,
            PullRequestState = state,
            PullRequestBranch = Branch,
            PullRequestHead = pullRequestHead,
            LocalBranch = localBranch,
            LocalHead = localHead,
            OriginHead = Head,
            WorkingTreeStatus = status,
            EffectiveHead = effectiveHead,
            UnresolvedThreadCount = unresolved,
        };
    }
}
