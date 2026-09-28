using System;
using System.IO;
using IronAbsolution.Tools;
using IronAbsolution.Tools.CodexReview;
using Xunit;
using static IronAbsolution.Tests.ReviewRecordText;

namespace IronAbsolution.Tests;

/// <summary>
/// The `codex-review` command: the command line, the refusal of a missing CLI, the judge of one
/// round against real commits, the worktree list, and the transcript files (D-14, D-47, D-48, D-51).
/// </summary>
public sealed class CodexReviewCommandTests
{
    private const int PullRequest = 4;
    private const string Branch = "feat/pr-3-codex-review";
    private const string ReviewFile = "docs/reviews/pr-4.md";
    private static readonly PullRequestView View = new PullRequestView("OPEN", Branch, "unused", "main");

    [Theory]
    [InlineData(new[] { "--root", ".", "--codex", "codex" }, "needs --pr <number>")]
    [InlineData(new[] { "--root", ".", "--pr", "x4", "--codex", "codex" }, "Found 'x4'")]
    [InlineData(new[] { "--root", ".", "--pr", "0", "--codex", "codex" }, "Found '0'")]
    [InlineData(new[] { "--root", ".", "--pr", "4" }, "needs --codex <path>")]
    [InlineData(new[] { "--pr", "4", "--codex", "codex", "--skip-gitar-review" }, "'--skip-gitar-review' is unknown")]
    public void AnIncompleteCommandLineIsAUsageFault(string[] args, string expected)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["codex-review", .. args], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains(expected, errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AMissingCliRefusesTheStart()
    {
        // The refusal comes before any call to GitHub, so the test needs no network.
        string missing = Path.Combine(Path.GetTempPath(), "no-codex-" + Guid.NewGuid().ToString("N"));
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = CodexReviewCommand.Run(["--root", Path.GetTempPath(), "--pr", "4", "--codex", missing], output, errors);

        Assert.Equal((int)CodexReviewExit.Refused, exitCode);
        Assert.Equal(3, exitCode);
        Assert.Contains($"The Codex CLI is missing: no file at '{missing}'", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("D-47", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ARoundThatPushesAnApprovingRecordApproves()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        string review = repo.Commit("docs: review", (ReviewFile, Record(reviewed, "Ready for owner merge")), ("docs/session-handoff.md", "entry"));
        repo.SetOrigin(Branch, review);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Root), PullRequest, View, reviewed, reviewed);

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
    }

    [Fact]
    public void ARoundThatPushesNoCommitIsAFault()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        repo.SetOrigin(Branch, reviewed);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Root), PullRequest, View, reviewed, reviewed);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains("pushed no commit", outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARoundThatPushesNoRecordIsAFault()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        string handoff = repo.Commit("docs: handoff", ("docs/session-handoff.md", "entry"));
        repo.SetOrigin(Branch, handoff);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Root), PullRequest, View, reviewed, reviewed);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(ReviewFile, outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACodeCommitDuringTheRoundIsAFault()
    {
        // D-14: the reviewer pushes a metadata commit alone, so the work head must not move.
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        repo.Commit("docs: review", (ReviewFile, Record(reviewed, "Ready for owner merge")));
        string code = repo.Commit("fix: a change", ("IronAbsolution.Tools/B.cs", "// b"));
        repo.SetOrigin(Branch, code);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Root), PullRequest, View, reviewed, reviewed);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(code, outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADocumentsCommitDuringTheRoundIsAFault()
    {
        // A documents commit keeps the effective head, and it moves the work head. The reviewer
        // pushes a metadata commit alone, so the round fails.
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        repo.Commit("docs: review", (ReviewFile, Record(reviewed, "Ready for owner merge")));
        string documents = repo.Commit("docs: a design change", ("docs/design.md", "text"));
        repo.SetOrigin(Branch, documents);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Root), PullRequest, View, reviewed, reviewed);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(documents, outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordOfTheEffectiveHeadApprovesAfterADocumentsCommit()
    {
        // D-49: the round starts after a documents commit. The record names the code commit, and
        // that commit is the effective head.
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string reviewed = StartBranch(repo);
        string documents = repo.Commit("docs: a roadmap mark", ("docs/roadmaps/readme.md", "mark"));
        string review = repo.Commit("docs: review", (ReviewFile, Record(reviewed, "Ready for owner merge")), ("docs/session-handoff.md", "entry"));
        repo.SetOrigin(Branch, review);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Root), PullRequest, View, documents, documents);

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
    }

    [Fact]
    public void ARecordOfTheDocumentsCommitIsStale()
    {
        // The record must name the effective head, not the tip of the branch.
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        StartBranch(repo);
        string documents = repo.Commit("docs: a roadmap mark", ("docs/roadmaps/readme.md", "mark"));
        string review = repo.Commit("docs: review", (ReviewFile, Record(documents, "Ready for owner merge")));
        repo.SetOrigin(Branch, review);

        ReviewOutcome outcome = CodexReviewCommand.JudgeRound(new GitRepository(repo.Root), PullRequest, View, documents, documents);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains("stale", outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheWorktreeListNamesTheFolderWithTheForwardSlashesOfGit()
    {
        // D-106: git on Windows gives `C:/Users/.../iron-absolution-review-pr-4`, and the old check
        // compared that text with the backslash path, so a second round refused the folder.
        string worktree = Path.Combine(Path.GetTempPath(), "iron-absolution-review-pr-4");
        string full = Path.GetFullPath(worktree).TrimEnd(Path.DirectorySeparatorChar);
        string gitForm = full.Replace('\\', '/');
        string list = $"worktree /repo\nHEAD abc\nbranch refs/heads/main\n\nworktree {gitForm}\nHEAD def\nbranch refs/heads/review/pr-4\n";

        Assert.True(CodexReviewCommand.ListsWorktree(list, worktree));
        Assert.True(CodexReviewCommand.ListsWorktree($"worktree {full}\n", worktree));
        Assert.True(CodexReviewCommand.ListsWorktree($"worktree {gitForm}\r\n", worktree));
        Assert.False(CodexReviewCommand.ListsWorktree("worktree /repo\n", worktree));
        Assert.False(CodexReviewCommand.ListsWorktree("worktree \n", worktree));
    }

    [Fact]
    public void TheWorktreeListReadsTheCaseOfAPathAsWindowsDoes()
    {
        string worktree = Path.Combine(Path.GetTempPath(), "iron-absolution-review-pr-4");
        string upper = Path.GetFullPath(worktree).ToUpperInvariant().Replace('\\', '/');

        // Windows reads two paths that differ in case as one path. Linux does not.
        Assert.Equal(OperatingSystem.IsWindows(), CodexReviewCommand.ListsWorktree($"worktree {upper}\n", worktree));
    }

    [Fact]
    public void ARepeatReviewOfTheSameHeadTakesANewTranscript()
    {
        // D-51: the transcripts go under artifacts/codex-review/, and no run writes over an earlier one.
        string root = Path.Combine(Path.GetTempPath(), "codex-review-root-" + Guid.NewGuid().ToString("N"));
        const string Head = "0123456789abcdef0123456789abcdef01234567";
        try
        {
            (string first, string errorLog, string lastMessage) = CodexReviewCommand.TranscriptPaths(root, PullRequest, Head);
            File.WriteAllText(first, "{}");
            (string second, string _, string _) = CodexReviewCommand.TranscriptPaths(root, PullRequest, Head);

            string folder = Path.Combine(root, "artifacts", "codex-review");
            Assert.Equal(Path.Combine(folder, "pr-4-0123456.jsonl"), first);
            Assert.Equal(Path.Combine(folder, "pr-4-0123456.err.log"), errorLog);
            Assert.Equal(Path.Combine(folder, "pr-4-0123456.last.md"), lastMessage);
            Assert.Equal(Path.Combine(folder, "pr-4-0123456-2.jsonl"), second);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>A root commit on main, then one code commit on the PR branch. Gives the code commit.</summary>
    private static string StartBranch(TemporaryGitRepository repo)
    {
        string root = repo.Commit("chore: root", ("README.md", "root"));
        repo.SetOrigin("main", root);
        repo.CreateBranch(Branch);
        return repo.Commit("feat: first", ("IronAbsolution.Tools/A.cs", "// a"));
    }
}
