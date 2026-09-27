using System;
using System.Collections.Generic;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>Everything that the start checks read, gathered one time from the CLI, git, and GitHub. The checks do no I/O.</summary>
public sealed class StartFacts
{
    /// <summary>Gets the GitHub number of the PR.</summary>
    public required int PullRequestNumber { get; init; }

    /// <summary>Gets the version of the Codex CLI.</summary>
    public required CodexVersion Version { get; init; }

    /// <summary>Gets the output of `codex login status`, with every API credential variable removed (D-53).</summary>
    public required string LoginStatus { get; init; }

    /// <summary>Gets the state of the PR on GitHub: `OPEN`, `CLOSED`, or `MERGED`.</summary>
    public required string PullRequestState { get; init; }

    /// <summary>Gets the branch of the PR.</summary>
    public required string PullRequestBranch { get; init; }

    /// <summary>Gets the head of the PR on GitHub.</summary>
    public required string PullRequestHead { get; init; }

    /// <summary>Gets the branch of the local checkout, or `HEAD` when the checkout is on no branch.</summary>
    public required string LocalBranch { get; init; }

    /// <summary>Gets the head of the local checkout.</summary>
    public required string LocalHead { get; init; }

    /// <summary>Gets the head of the PR branch on origin after the fetch.</summary>
    public required string OriginHead { get; init; }

    /// <summary>Gets the output of `git status --porcelain`. The empty text means a clean tree.</summary>
    public required string WorkingTreeStatus { get; init; }

    /// <summary>Gets the newest commit outside the documents set, or null when the PR changes documents alone (D-49).</summary>
    public required string? EffectiveHead { get; init; }

    /// <summary>Gets the count of review threads of the PR that are not resolved (D-52).</summary>
    public required int UnresolvedThreadCount { get; init; }
}

/// <summary>
/// The conditions that must hold before a review round starts (D-14). Each problem names the
/// fact that failed and the value found (T-2). The command refuses the round when the list is
/// not empty. The gitar start check stays out until the owner answers OQ-16 (D-7).
/// </summary>
public static class StartChecks
{
    /// <summary>The state of an open PR on GitHub.</summary>
    public const string OpenState = "OPEN";

    /// <summary>Gives the problem of a PR that is not open.</summary>
    /// <param name="pullRequest">The GitHub number of the PR.</param>
    /// <param name="state">The state of the PR on GitHub.</param>
    /// <returns>The text of the problem.</returns>
    public static string NotOpenProblem(int pullRequest, string state)
    {
        return $"PR #{pullRequest} is {state}, and a review needs an open PR.";
    }

    /// <summary>Gives every problem of the start facts.</summary>
    /// <param name="facts">The facts of the CLI, the PR, and the checkout.</param>
    /// <returns>Each problem, or none when the round can start.</returns>
    public static IReadOnlyList<string> Problems(StartFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        List<string> problems = [];
        if (!facts.Version.IsAtLeast(CodexReviewSettings.MinimumVersion))
        {
            problems.Add($"The Codex CLI is {facts.Version}, and the minimum is {CodexReviewSettings.MinimumVersion} (D-14). Run `npm install -g @openai/codex@latest`.");
        }

        if (!facts.LoginStatus.StartsWith(CodexReviewSettings.ChatGptLoginStatus, StringComparison.Ordinal))
        {
            problems.Add($"`codex login status` gives '{facts.LoginStatus.Trim()}', and a review needs '{CodexReviewSettings.ChatGptLoginStatus}', so it never uses API pricing (D-53). Run `codex login` and choose ChatGPT.");
        }

        if (facts.PullRequestState != OpenState)
        {
            problems.Add(NotOpenProblem(facts.PullRequestNumber, facts.PullRequestState));
        }

        if (facts.EffectiveHead is null)
        {
            problems.Add($"No commit of PR #{facts.PullRequestNumber} changes a path outside the documents set, so the review has nothing to approve (D-49). The owner starts Codex by hand for such a PR until PR-6 adds the review-override label (D-35).");
        }

        AddCheckoutProblems(facts, problems);
        if (facts.UnresolvedThreadCount > 0)
        {
            problems.Add($"PR #{facts.PullRequestNumber} has {facts.UnresolvedThreadCount} unresolved review thread(s). Answer and resolve each one (D-52).");
        }

        return problems;
    }

    /// <summary>Checks that the local checkout is the PR branch, clean, and at the head that origin and GitHub hold.</summary>
    private static void AddCheckoutProblems(StartFacts facts, List<string> problems)
    {
        if (facts.LocalBranch != facts.PullRequestBranch)
        {
            problems.Add($"The checkout is on '{facts.LocalBranch}', and PR #{facts.PullRequestNumber} has the branch '{facts.PullRequestBranch}'.");
        }

        if (facts.LocalHead != facts.OriginHead)
        {
            problems.Add($"The local head {facts.LocalHead} differs from origin/{facts.PullRequestBranch} at {facts.OriginHead}. Push or pull first.");
        }

        if (facts.OriginHead != facts.PullRequestHead)
        {
            problems.Add($"origin/{facts.PullRequestBranch} is at {facts.OriginHead}, and GitHub gives the PR head {facts.PullRequestHead}.");
        }

        if (facts.WorkingTreeStatus.Trim().Length > 0)
        {
            problems.Add($"The working tree has changes. git status --porcelain:\n{facts.WorkingTreeStatus.TrimEnd()}");
        }
    }
}
