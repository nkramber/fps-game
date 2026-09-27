using System;
using System.Collections.Generic;
using System.Globalization;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>
/// The configuration of the cross-provider review through the Codex CLI (D-14). Each Codex
/// call names the model, the reasoning effort, the approval policy, and the sandbox on the
/// command line, so no value comes from the configuration file of the machine.
/// </summary>
public static class CodexReviewSettings
{
    /// <summary>The model of the review and of the probe.</summary>
    public const string Model = "gpt-6-luna";

    /// <summary>The reasoning effort of the review and of the probe.</summary>
    public const string ReasoningEffort = "medium";

    /// <summary>The approval policy. The review runs with no person at the terminal.</summary>
    public const string ApprovalPolicy = "never";

    /// <summary>The review sandbox. The reviewer runs `gh`, `git push`, and the build, so it needs the network and the caches.</summary>
    public const string ReviewSandbox = "danger-full-access";

    /// <summary>The probe runs no command, so it needs no write access.</summary>
    public const string ProbeSandbox = "read-only";

    /// <summary>The prompt of the model probe.</summary>
    public const string ProbePrompt = "Reply with the single word OK.";

    /// <summary>The login that each Codex call demands: the ChatGPT account, and never an API key (D-14, D-53).</summary>
    public const string ForcedLogin = "forced_login_method=\"chatgpt\"";

    /// <summary>The first line of `codex login status` for a ChatGPT login.</summary>
    public const string ChatGptLoginStatus = "Logged in using ChatGPT";

    /// <summary>The skill that the reviewer loads (D-46).</summary>
    public const string ReviewSkillPath = ".claude/skills/pr-review/SKILL.md";

    /// <summary>The folder under the root that takes the transcript of each review. Git ignores `artifacts/` (D-51).</summary>
    public const string TranscriptFolder = "artifacts/codex-review";

    /// <summary>
    /// The environment variables that give the CLI an API credential. The command removes each
    /// one from every Codex process that it starts, so no review uses API pricing (D-53).
    /// </summary>
    public static readonly IReadOnlyList<string> ApiCredentialVariables = ["OPENAI_API_KEY", "CODEX_API_KEY", "CODEX_ACCESS_TOKEN"];

    /// <summary>The arguments of `codex login status`.</summary>
    public static readonly IReadOnlyList<string> LoginStatusArguments = ["login", "status"];

    /// <summary>The oldest CLI that ran the model probe in what-you-carry (D-14).</summary>
    public static readonly CodexVersion MinimumVersion = new CodexVersion(0, 156, 1, string.Empty);

    /// <summary>The time limit of the review itself (D-50). At the limit, the command stops the review with a fault.</summary>
    public static readonly TimeSpan ReviewLimit = TimeSpan.FromMinutes(90);

    /// <summary>
    /// Gives the status text of `codex login status`. The CLI writes the status to stderr and
    /// nothing to stdout, so the text joins both streams, stderr first.
    /// </summary>
    /// <param name="result">The result of `codex login status`.</param>
    /// <returns>The status text.</returns>
    public static string LoginStatusText(ProcessResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return (result.StandardError + result.StandardOutput).Trim();
    }

    /// <summary>
    /// Gives the local branch of the review worktree (D-48). The pre-commit hook refuses a
    /// commit on no branch (D-43), so the worktree takes a branch of its own.
    /// </summary>
    /// <param name="pullRequest">The GitHub number of the PR.</param>
    /// <returns>The branch name `review/pr-&lt;n&gt;`.</returns>
    public static string ReviewBranch(int pullRequest)
    {
        return $"review/pr-{pullRequest.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>Gives the prompt of the review: the prompt of the owner, with the facts of a start by the command.</summary>
    /// <param name="pullRequest">The GitHub number of the PR.</param>
    /// <param name="branch">The branch of the PR on origin.</param>
    /// <returns>The prompt text.</returns>
    public static string ReviewPrompt(int pullRequest, string branch)
    {
        string number = pullRequest.ToString(CultureInfo.InvariantCulture);
        return $"Review PR #{number}.\n"
            + $"Load and follow `{ReviewSkillPath}`.\n"
            + $"The command `make codex-review` started this review in a worktree on the local branch `{ReviewBranch(pullRequest)}`, at the PR head.\n"
            + $"Push the review record and your session handoff entry as one metadata commit (D-14) with `git push origin HEAD:{branch}`.\n";
    }

    /// <summary>Gives the arguments of the review run. The transcript is the JSON event stream on stdout.</summary>
    /// <param name="worktree">The folder of the review worktree.</param>
    /// <param name="lastMessagePath">The file that takes the last message of the reviewer.</param>
    /// <param name="prompt">The review prompt.</param>
    /// <returns>The arguments of `codex`.</returns>
    public static IReadOnlyList<string> ReviewArguments(string worktree, string lastMessagePath, string prompt)
    {
        return
        [
            "exec",
            "-m", Model,
            "-c", $"model_reasoning_effort=\"{ReasoningEffort}\"",
            "-c", $"approval_policy=\"{ApprovalPolicy}\"",
            "-c", ForcedLogin,
            "-s", ReviewSandbox,
            "-C", worktree,
            "--json",
            "-o", lastMessagePath,
            prompt,
        ];
    }

    /// <summary>Gives the arguments of the model probe: one short call outside a repository, with no saved session.</summary>
    /// <param name="directory">The empty folder of the probe.</param>
    /// <returns>The arguments of `codex`.</returns>
    public static IReadOnlyList<string> ProbeArguments(string directory)
    {
        return
        [
            "exec",
            "-m", Model,
            "-c", $"model_reasoning_effort=\"{ReasoningEffort}\"",
            "-c", $"approval_policy=\"{ApprovalPolicy}\"",
            "-c", ForcedLogin,
            "-s", ProbeSandbox,
            "-C", directory,
            "--skip-git-repo-check",
            "--ephemeral",
            "--json",
            ProbePrompt,
        ];
    }
}
