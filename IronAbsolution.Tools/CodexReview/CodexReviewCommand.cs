using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>The facts of a PR that `gh pr view` gives.</summary>
/// <param name="State">The state of the PR: `OPEN`, `CLOSED`, or `MERGED`.</param>
/// <param name="Branch">The branch of the PR.</param>
/// <param name="Head">The head of the PR on GitHub.</param>
/// <param name="BaseBranch">The base branch of the PR, such as `main`.</param>
public sealed record PullRequestView(string State, string Branch, string Head, string BaseBranch);

/// <summary>
/// The `codex-review` command (D-14). It checks the start conditions, probes the model, runs one
/// Codex review round in a worktree at the PR head, and judges the review record that the
/// round pushed. The exit code names the outcome (<see cref="CodexReviewExit"/>). The entry script
/// target `codex-review` installs the CLI and runs the command (D-47).
/// </summary>
public static class CodexReviewCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "codex-review";

    /// <summary>The option that names the root of the author checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that gives the GitHub number of the PR.</summary>
    public const string PullRequestOption = "--pr";

    /// <summary>The option that gives the path of the entry script `bin/codex.js` of the Codex CLI (D-47).</summary>
    public const string CodexOption = "--codex";

    /// <summary>Reads the options and runs one review round.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes the report.</param>
    /// <param name="errors">The writer that takes each fault and each refusal.</param>
    /// <returns>The exit code of <see cref="CodexReviewExit"/>. A usage fault gives 1 (D-40).</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, PullRequestOption, CodexOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? pullRequestText = options.Value(PullRequestOption);
        if (pullRequestText is null
            || !int.TryParse(pullRequestText, NumberStyles.None, CultureInfo.InvariantCulture, out int pullRequest)
            || pullRequest <= 0)
        {
            errors.WriteLine($"Error: {Name} needs {PullRequestOption} <number>, the GitHub number of the PR, such as `run.ps1 codex-review -PR 4`. Found '{pullRequestText}'.");
            return Program.FaultExitCode;
        }

        string? codex = options.Value(CodexOption);
        if (codex is null)
        {
            errors.WriteLine($"Error: {Name} needs {CodexOption} <path>, the path of `bin/codex.js` of the Codex CLI. `run.ps1 codex-review` gives it (D-47).");
            return Program.FaultExitCode;
        }

        string root = Path.GetFullPath(options.ValueOr(RootOption, "."));
        try
        {
            return (int)Review(root, pullRequest, codex, output, errors);
        }
        catch (Exception fault) when (fault is InvalidOperationException or FormatException or IOException or UnauthorizedAccessException or JsonException)
        {
            errors.WriteLine($"{Name}: {CodexReviewExit.Fault} (exit {(int)CodexReviewExit.Fault}). PR #{pullRequest} in '{root}'. {fault.Message}");
            return (int)CodexReviewExit.Fault;
        }
    }

    /// <summary>
    /// Judges the round from the branch on origin after the fetch. The round must push a new
    /// commit, and that commit must keep the work head, because a reviewer pushes a metadata
    /// commit alone (D-14). The record must name the effective head (D-49).
    /// </summary>
    /// <param name="git">The author checkout, after the fetch of the PR branch and the base branch.</param>
    /// <param name="pullRequest">The GitHub number of the PR.</param>
    /// <param name="view">The facts of the PR.</param>
    /// <param name="headBefore">The head of the PR branch on origin before the round.</param>
    /// <param name="workBefore">The work head before the round.</param>
    /// <returns>The outcome of the round.</returns>
    public static ReviewOutcome JudgeRound(GitRepository git, int pullRequest, PullRequestView view, string headBefore, string workBefore)
    {
        ArgumentNullException.ThrowIfNull(git);
        ArgumentNullException.ThrowIfNull(view);

        string headAfter = git.Run(["rev-parse", $"refs/remotes/origin/{view.Branch}"]).Trim();
        if (headAfter == headBefore)
        {
            return new ReviewOutcome(CodexReviewExit.Fault, "none", [], [], $"origin/{view.Branch} is still at {headBefore}. The review pushed no commit.");
        }

        string? workAfter = ReviewHeads.WorkHead(git, view.BaseBranch, headAfter);
        if (workAfter != workBefore)
        {
            return new ReviewOutcome(CodexReviewExit.Fault, "none", [], [], $"The work head moved from {workBefore} to {workAfter ?? "none"} during the review. A commit outside the metadata set arrived (D-14).");
        }

        // The work head stayed, so every new commit is a metadata commit, and the effective head stayed too.
        string effectiveAfter = ReviewHeads.EffectiveHead(git, view.BaseBranch, headAfter)
            ?? throw new InvalidOperationException($"origin/{view.Branch} at {headAfter} has the work head {workAfter} and no effective head.");
        string reviewFile = ReviewRecord.FilePath(pullRequest);
        return ReviewOutcomeRules.Judge(git.ReadFileOrNull(headAfter, reviewFile), reviewFile, effectiveAfter);
    }

    /// <summary>Tells whether the output of `git worktree list --porcelain` names the folder.</summary>
    /// <param name="list">The output of `git worktree list --porcelain`.</param>
    /// <param name="worktree">The full path of the folder.</param>
    /// <returns>True when git lists a worktree at the folder.</returns>
    public static bool ListsWorktree(string list, string worktree)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(worktree);

        const string Prefix = "worktree ";
        string full = Path.GetFullPath(worktree).TrimEnd(Path.DirectorySeparatorChar);
        foreach (string rawLine in list.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (!line.StartsWith(Prefix, StringComparison.Ordinal))
            {
                continue;
            }

            // macOS gives the temporary folder under `/var`, and git gives it under `/private/var`.
            string listed = line[Prefix.Length..].TrimEnd(Path.DirectorySeparatorChar);
            if (listed == full || listed == "/private" + full)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gives the three files of one review under the transcript folder (D-51): the transcript,
    /// the error log, and the last message. A repeat review of the same head takes a number.
    /// </summary>
    /// <param name="root">The root of the author checkout.</param>
    /// <param name="pullRequest">The GitHub number of the PR.</param>
    /// <param name="effectiveHead">The full hash of the effective head.</param>
    /// <returns>The three full paths. The folder exists after the call.</returns>
    public static (string Transcript, string ErrorLog, string LastMessage) TranscriptPaths(string root, int pullRequest, string effectiveHead)
    {
        ArgumentNullException.ThrowIfNull(effectiveHead);

        string folder = ToolPaths.UnderFolder(root, CodexReviewSettings.TranscriptFolder);
        Directory.CreateDirectory(folder);
        string stem = $"pr-{pullRequest.ToString(CultureInfo.InvariantCulture)}-{effectiveHead[..ReviewHeads.ShortestHash]}";
        string name = stem;
        for (int run = 2; File.Exists(Path.Combine(folder, name + ".jsonl")); run++)
        {
            name = $"{stem}-{run.ToString(CultureInfo.InvariantCulture)}";
        }

        return (Path.Combine(folder, name + ".jsonl"), Path.Combine(folder, name + ".err.log"), Path.Combine(folder, name + ".last.md"));
    }

    private static CodexReviewExit Review(string root, int pullRequest, string codex, TextWriter output, TextWriter errors)
    {
        GitRepository git = new GitRepository(root);
        if (!File.Exists(codex))
        {
            return Refuse(pullRequest, [CodexLauncher.MissingProblem(codex)], errors);
        }

        CodexVersion version = CodexLauncher.ReadVersion(codex, root);
        PullRequestView view = ReadPullRequest(root, pullRequest);
        if (view.State != StartChecks.OpenState)
        {
            // GitHub can delete the branch of a closed PR, so the refusal comes before the fetch.
            return Refuse(pullRequest, [StartChecks.NotOpenProblem(pullRequest, view.State)], errors);
        }

        string loginStatus = CodexReviewSettings.LoginStatusText(CodexLauncher.Run(codex, CodexReviewSettings.LoginStatusArguments, root));
        StartFacts facts = GatherStartFacts(root, git, pullRequest, view, version, loginStatus);
        List<string> problems = [.. StartChecks.Problems(facts)];
        if (problems.Count == 0)
        {
            string? probeProblem = ProbeModel(codex);
            if (probeProblem is not null)
            {
                problems.Add(probeProblem);
            }
        }

        if (problems.Count > 0)
        {
            return Refuse(pullRequest, problems, errors);
        }

        // The start checks refuse a PR with no effective head. The documents set holds the
        // metadata set, so a PR with an effective head has a work head too.
        string effectiveBefore = facts.EffectiveHead ?? throw new InvalidOperationException($"PR #{pullRequest} passed the start checks with no effective head.");
        string workBefore = ReviewHeads.WorkHead(git, view.BaseBranch, facts.OriginHead)
            ?? throw new InvalidOperationException($"PR #{pullRequest} has the effective head {effectiveBefore} and no work head.");
        (string transcript, string errorLog, string lastMessage) = TranscriptPaths(root, pullRequest, effectiveBefore);
        string localBranch = CodexReviewSettings.ReviewBranch(pullRequest);
        string worktree = PrepareWorktree(git, pullRequest, localBranch, facts.OriginHead);
        output.WriteLine($"{Name}: PR #{pullRequest}, effective head {effectiveBefore}, model {CodexReviewSettings.Model} at effort {CodexReviewSettings.ReasoningEffort}, CLI {version}.");
        output.WriteLine($"{Name}: the review runs in '{worktree}' on the branch '{localBranch}'. The limit is {CodexReviewSettings.ReviewLimit.TotalMinutes} minutes (D-50).");
        output.WriteLine($"{Name}: transcript: {transcript}");

        IReadOnlyList<string> arguments = CodexReviewSettings.ReviewArguments(worktree, lastMessage, CodexReviewSettings.ReviewPrompt(pullRequest, view.Branch));
        FileRunResult run = CodexLauncher.RunToFiles(codex, arguments, worktree, transcript, errorLog, CodexReviewSettings.ReviewLimit);
        if (run.TimedOut)
        {
            errors.WriteLine($"{Name}: {CodexReviewExit.Fault} (exit {(int)CodexReviewExit.Fault}). The review ran longer than {CodexReviewSettings.ReviewLimit.TotalMinutes} minutes, and the command stopped it (D-50). Read the transcript {transcript} and the log {errorLog}. The worktree stays at {worktree}.");
            return CodexReviewExit.Fault;
        }

        if (run.ExitCode != 0)
        {
            errors.WriteLine($"{Name}: {CodexReviewExit.Fault} (exit {(int)CodexReviewExit.Fault}). Codex gave the exit code {run.ExitCode}. Read the transcript {transcript} and the log {errorLog}. The worktree stays at {worktree}.");
            return CodexReviewExit.Fault;
        }

        FetchBranch(git, view.Branch);
        FetchBranch(git, view.BaseBranch);
        ReviewOutcome outcome = JudgeRound(git, pullRequest, view, facts.OriginHead, workBefore);
        Print(outcome, effectiveBefore, transcript, lastMessage, output);
        if (outcome.Exit == CodexReviewExit.Fault)
        {
            output.WriteLine($"{Name}: the worktree stays at '{worktree}' for the fault. The next run removes it.");
        }
        else
        {
            git.Run(["worktree", "remove", "--force", worktree]);
            git.Run(["branch", "--quiet", "-D", localBranch]);
        }

        return outcome.Exit;
    }

    private static PullRequestView ReadPullRequest(string root, int pullRequest)
    {
        string number = pullRequest.ToString(CultureInfo.InvariantCulture);
        string json = ExternalProcess.Run("gh", ["pr", "view", number, "--json", "state,headRefName,headRefOid,baseRefName"], root, []).RequireSuccess();
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement pr = document.RootElement;
        return new PullRequestView(
            RequiredString(pr, "state", pullRequest),
            RequiredString(pr, "headRefName", pullRequest),
            RequiredString(pr, "headRefOid", pullRequest),
            RequiredString(pr, "baseRefName", pullRequest));
    }

    private static StartFacts GatherStartFacts(string root, GitRepository git, int pullRequest, PullRequestView view, CodexVersion version, string loginStatus)
    {
        FetchBranch(git, view.Branch);
        FetchBranch(git, view.BaseBranch);
        string repository = ExternalProcess.Run("gh", ["repo", "view", "--json", "nameWithOwner", "--jq", ".nameWithOwner"], root, []).RequireSuccess().Trim();
        string originHead = git.Run(["rev-parse", $"refs/remotes/origin/{view.Branch}"]).Trim();
        return new StartFacts
        {
            PullRequestNumber = pullRequest,
            Version = version,
            LoginStatus = loginStatus,
            PullRequestState = view.State,
            PullRequestBranch = view.Branch,
            PullRequestHead = view.Head,
            LocalBranch = git.Run(["rev-parse", "--abbrev-ref", "HEAD"]).Trim(),
            LocalHead = git.Run(["rev-parse", "HEAD"]).Trim(),
            OriginHead = originHead,
            WorkingTreeStatus = git.Run(["status", "--porcelain"]),
            EffectiveHead = ReviewHeads.EffectiveHead(git, view.BaseBranch, originHead),
            UnresolvedThreadCount = CountUnresolvedThreads(root, repository, pullRequest),
        };
    }

    private static int CountUnresolvedThreads(string root, string repository, int pullRequest)
    {
        int slash = repository.IndexOf('/', StringComparison.Ordinal);
        if (slash <= 0)
        {
            throw new InvalidOperationException($"`gh repo view` gave '{repository}', which is no owner and name.");
        }

        string owner = repository[..slash];
        string name = repository[(slash + 1)..];
        const string Query = "query($owner: String!, $name: String!, $number: Int!, $endCursor: String) { repository(owner: $owner, name: $name) { pullRequest(number: $number) { reviewThreads(first: 100, after: $endCursor) { pageInfo { hasNextPage endCursor } nodes { isResolved } } } } }";
        string lines = ExternalProcess.Run(
            "gh",
            ["api", "graphql", "--paginate", "-F", $"owner={owner}", "-F", $"name={name}", "-F", $"number={pullRequest.ToString(CultureInfo.InvariantCulture)}", "-f", $"query={Query}", "--jq", ".data.repository.pullRequest.reviewThreads.nodes[] | .isResolved"],
            root,
            []).RequireSuccess();
        int unresolved = 0;
        foreach (string line in lines.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line == "false")
            {
                unresolved++;
            }
            else if (line != "true")
            {
                throw new FormatException($"The thread query of PR #{pullRequest} gave the line '{line}', and a line is 'true' or 'false'.");
            }
        }

        return unresolved;
    }

    /// <summary>Makes one short call with the model of the review. Gives null when the model answers, or the problem with the output.</summary>
    private static string? ProbeModel(string codex)
    {
        string directory = Path.Combine(Path.GetTempPath(), "iron-absolution-codex-probe-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        ProcessResult result = CodexLauncher.Run(codex, CodexReviewSettings.ProbeArguments(directory), directory);
        Directory.Delete(directory, recursive: true);
        if (result.ExitCode == 0 && result.StandardOutput.Contains("agent_message", StringComparison.Ordinal))
        {
            return null;
        }

        return $"The model probe of '{CodexReviewSettings.Model}' failed with the exit code {result.ExitCode} (D-14). stdout: {result.StandardOutput.Trim()} stderr: {result.StandardError.Trim()}";
    }

    /// <summary>
    /// Makes the review worktree on the local branch of the review, at the PR head (D-48). A
    /// worktree that an earlier run left goes first. A folder that git does not list stays, and
    /// the run stops.
    /// </summary>
    private static string PrepareWorktree(GitRepository git, int pullRequest, string localBranch, string originHead)
    {
        string worktree = Path.Combine(Path.GetTempPath(), $"iron-absolution-review-pr-{pullRequest.ToString(CultureInfo.InvariantCulture)}");
        if (Directory.Exists(worktree))
        {
            if (!ListsWorktree(git.Run(["worktree", "list", "--porcelain"]), worktree))
            {
                throw new InvalidOperationException($"The folder '{worktree}' exists, and git lists no worktree there. Move the folder out of the way.");
            }

            git.Run(["worktree", "remove", "--force", worktree]);
        }

        git.Run(["worktree", "prune"]);
        git.Run(["worktree", "add", "--quiet", "-B", localBranch, worktree, originHead]);
        return worktree;
    }

    private static CodexReviewExit Refuse(int pullRequest, IReadOnlyList<string> problems, TextWriter errors)
    {
        errors.WriteLine($"{Name}: {CodexReviewExit.Refused} (exit {(int)CodexReviewExit.Refused}). PR #{pullRequest} does not meet the start conditions:");
        foreach (string problem in problems)
        {
            errors.WriteLine($"- {problem}");
        }

        return CodexReviewExit.Refused;
    }

    private static void FetchBranch(GitRepository git, string branch)
    {
        git.Run(["fetch", "--quiet", "origin", $"+refs/heads/{branch}:refs/remotes/origin/{branch}"]);
    }

    private static string RequiredString(JsonElement element, string property, int pullRequest)
    {
        if (!element.TryGetProperty(property, out JsonElement value) || value.ValueKind != JsonValueKind.String)
        {
            throw new FormatException($"`gh pr view {pullRequest}` gave no text field '{property}'.");
        }

        return value.GetString() ?? throw new FormatException($"`gh pr view {pullRequest}` gave a null '{property}'.");
    }

    private static void Print(ReviewOutcome outcome, string effectiveHead, string transcript, string lastMessage, TextWriter output)
    {
        output.WriteLine($"{Name}: verdict: {outcome.Verdict}");
        output.WriteLine($"{Name}: effective head: {effectiveHead}");
        output.WriteLine($"{Name}: open findings: {JoinOrNone(outcome.OpenFindingIds)}");
        output.WriteLine($"{Name}: three-strike findings: {JoinOrNone(outcome.StrikeFindingIds)}");
        output.WriteLine($"{Name}: {outcome.Message}");
        output.WriteLine($"{Name}: transcript: {transcript}");
        output.WriteLine($"{Name}: last message: {lastMessage}");
        output.WriteLine($"{Name}: {outcome.Exit} (exit {(int)outcome.Exit}).");
    }

    private static string JoinOrNone(IReadOnlyList<string> ids)
    {
        return ids.Count == 0 ? "none" : string.Join(", ", ids);
    }
}
