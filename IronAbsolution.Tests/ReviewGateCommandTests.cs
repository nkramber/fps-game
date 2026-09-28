using System;
using System.IO;
using IronAbsolution.Tools;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.ReviewGate;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The `review-gate` command over real commits (D-64 to D-66): the exit code of each path, the
/// heads that the gate reads, and each fault with its context (T-2). Each commit of the fixture
/// has the committer time 2026-09-27T10:00:00Z.
/// </summary>
public sealed class ReviewGateCommandTests : IDisposable
{
    private const int PullRequest = 7;
    private const string RecordPath = "docs/reviews/pr-7.md";
    private const string NoLabels = """{ "labels": [], "overrideLabelEvents": [] }""";
    private const string LabelAfterTheCommits = """{ "labels": ["review-override"], "overrideLabelEvents": [{ "createdAt": "2026-09-27T11:00:00Z", "actor": "owner-login" }] }""";
    private const string LabelBeforeTheCommits = """{ "labels": ["review-override"], "overrideLabelEvents": [{ "createdAt": "2026-09-27T09:00:00Z", "actor": "owner-login" }] }""";

    private readonly TemporaryGitRepository repo = new TemporaryGitRepository();
    private readonly string labelsPath = Path.Combine(Path.GetTempPath(), "review-gate-labels-" + Guid.NewGuid().ToString("N") + ".json");

    [Fact]
    public void APullRequestWithNoRecordGivesTheFaultCode()
    {
        // Exit test 1 of PR-6 on real commits.
        string head = this.StartBranch();

        (int exitCode, string output, string errors) = this.Run(head, NoLabels);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("review-gate: Found: no record. Run `run.ps1 codex-review`, or the author session adds the label for a PR with no code (D-76).", output, StringComparison.Ordinal);
        Assert.DoesNotContain("owner adds", output, StringComparison.Ordinal);
        Assert.Contains($"review-gate: fail for PR #7 at {head}, the review gate fails.", output, StringComparison.Ordinal);
        Assert.Empty(errors);
    }

    [Fact]
    public void AnApprovingRecordOfTheEffectiveHeadGivesZero()
    {
        // Exit test 2 of PR-6 on real commits. The review commit is a metadata commit.
        string code = this.StartBranch();
        string head = this.repo.Commit("docs: review record of #7 (PR-6)", (RecordPath, ReviewRecordText.Record(code, ReviewRecord.ApprovedVerdict)));

        (int exitCode, string output, string errors) = this.Run(head, NoLabels);

        Assert.Equal(0, exitCode);
        Assert.Contains($"review-gate: Effective head: {code}.", output, StringComparison.Ordinal);
        Assert.Contains($"review-gate: Last change of the record: {head} \"docs: review record of #7 (PR-6)\".", output, StringComparison.Ordinal);
        Assert.Empty(errors);
    }

    [Fact]
    public void ADocumentsCommitAfterTheApprovalKeepsTheGateGreen()
    {
        string code = this.StartBranch();
        this.repo.Commit("docs: review record of #7 (PR-6)", (RecordPath, ReviewRecordText.Record(code, ReviewRecord.ApprovedVerdict)));
        string head = this.repo.Commit("docs: a design correction (PR-6)", ("docs/design.md", "corrected"), ("AGENTS.md", "agent"));

        (int exitCode, string output, _) = this.Run(head, NoLabels);

        Assert.Equal(0, exitCode);
        Assert.Contains($"review-gate: Effective head: {code}.", output, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("IronAbsolution.Tools/Later.cs")]
    [InlineData(".github/workflows/ci.yml")]
    [InlineData("LICENSE/tool.cs")]
    public void ACommitOutsideTheDocumentsSetAfterTheApprovalFails(string path)
    {
        string code = this.StartBranch();
        this.repo.Commit("docs: review record of #7 (PR-6)", (RecordPath, ReviewRecordText.Record(code, ReviewRecord.ApprovedVerdict)));
        string head = this.repo.Commit("feat: a later change (PR-6)", (path, "later"));

        (int exitCode, string output, _) = this.Run(head, NoLabels);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"review-gate: Expected: '{head}'.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void AnApprovingRecordOfADocumentsOnlyPullRequestFails()
    {
        // D-66: the review path has nothing to approve, and the failure names the label.
        this.StartBranch(("docs/design.md", "a design change"));
        string documents = this.repo.Git("rev-parse", "HEAD").Trim();
        string head = this.repo.Commit("docs: review record of #7 (PR-6)", (RecordPath, ReviewRecordText.Record(documents, ReviewRecord.ApprovedVerdict)));

        (int exitCode, string output, _) = this.Run(head, NoLabels);

        Assert.Equal(Program.FaultExitCode, exitCode);
        // D-76: the author session adds the label, and no message says that the owner adds it (F-20).
        Assert.Contains("The author session adds the label 'review-override' to such a PR after the last commit outside the metadata set (D-76).", output, StringComparison.Ordinal);
        Assert.DoesNotContain("owner adds", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLabelAfterTheLastCommitPassesADocumentsOnlyPullRequest()
    {
        string head = this.StartBranch(("docs/design.md", "a design change"), ("README.md", "a new line"));

        (int exitCode, string output, _) = this.Run(head, LabelAfterTheCommits);

        Assert.Equal(0, exitCode);
        Assert.Contains("review-gate: Label: review-override, added by owner-login at 2026-09-27T11:00:00.0000000+00:00.", output, StringComparison.Ordinal);
        Assert.Contains($"review-gate: Work head: {head}.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLabelBeforeTheLastDocumentsCommitFails()
    {
        // D-65: the label reads the work head, so a documents commit after it needs the label again.
        string head = this.StartBranch(("docs/design.md", "a design change"));

        (int exitCode, string output, _) = this.Run(head, LabelBeforeTheCommits);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"the work head {head} with the commit time 2026-09-27T10:00:00.0000000+00:00", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheNewestLabeledEventDecides()
    {
        string head = this.StartBranch(("docs/design.md", "a design change"));
        const string twoEvents = """{ "labels": ["review-override"], "overrideLabelEvents": [{ "createdAt": "2026-09-27T11:00:00Z", "actor": "owner-login" }, { "createdAt": "2026-09-27T09:00:00Z", "actor": "other-login" }] }""";

        (int exitCode, string output, _) = this.Run(head, twoEvents);

        Assert.Equal(0, exitCode);
        Assert.Contains("added by owner-login at 2026-09-27T11:00:00", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLabelFailsForACodePullRequest()
    {
        string head = this.StartBranch();

        (int exitCode, string output, _) = this.Run(head, LabelAfterTheCommits);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("review-gate: Found: 1 path(s) outside the documents set: IronAbsolution.Tools/Sample.cs.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentLabelsFileIsAFaultThatNamesThePath()
    {
        string head = this.StartBranch();

        (int exitCode, string output, string errors) = this.Run(head, labels: null);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Empty(output);
        Assert.Contains($"The labels file '{this.labelsPath}' does not exist.", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownHeadIsAFaultThatNamesTheGitCommand()
    {
        this.StartBranch();

        (int exitCode, _, string errors) = this.Run("0000000000000000000000000000000000000000", NoLabels);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("`git merge-base origin/main 0000000000000000000000000000000000000000`", errors, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-7")]
    [InlineData("seven")]
    public void APullRequestNumberThatIsNotPositiveIsAFault(string number)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = ReviewGateCommand.Run(["--base", "origin/main", "--head", "HEAD", "--pr", number, "--labels", this.labelsPath], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"Found '{number}'.", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentOptionIsAFaultThatNamesEachOption()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = ReviewGateCommand.Run(["--base", "origin/main"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("review-gate needs each of --base, --head, --pr, --labels, each with a value.", errors.ToString(), StringComparison.Ordinal);
    }

    /// <summary>Removes the fixture and the labels file.</summary>
    public void Dispose()
    {
        this.repo.Dispose();
        if (File.Exists(this.labelsPath))
        {
            File.Delete(this.labelsPath);
        }
    }

    /// <summary>Makes `main` with one commit, and a PR branch with one commit of the files. No file gives one code file.</summary>
    private string StartBranch(params (string Path, string Text)[] files)
    {
        string root = this.repo.Commit("chore: root", ("README.md", "root"), ("docs/design.md", "design"));
        this.repo.SetOrigin("main", root);
        this.repo.CreateBranch("feat/pr-6-review-gate");
        (string Path, string Text)[] change = files.Length == 0 ? [("IronAbsolution.Tools/Sample.cs", "// code")] : files;
        return this.repo.Commit("feat: the change (PR-6)", change);
    }

    /// <summary>Writes the labels file, or none for null, and runs the command through the command line of the tools.</summary>
    private (int ExitCode, string Output, string Errors) Run(string head, string? labels)
    {
        if (labels is not null)
        {
            File.WriteAllText(this.labelsPath, labels);
        }

        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();
        int exitCode = Program.Run(
            ["review-gate", "--root", this.repo.Root, "--base", "origin/main", "--head", head, "--pr", PullRequest.ToString(System.Globalization.CultureInfo.InvariantCulture), "--labels", this.labelsPath],
            output,
            errors);
        return (exitCode, output.ToString(), errors.ToString());
    }
}
