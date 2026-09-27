using System;
using System.Collections.Generic;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.ReviewGate;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The rules of the review gate over sample facts (D-49, D-64 to D-66): the review path, the
/// label path, and the text of each result.
/// </summary>
public sealed class ReviewGateRulesTests
{
    private const int PullRequest = 7;
    private const string Head = "1111111111111111111111111111111111111111";
    private const string EffectiveHead = "2222222222222222222222222222222222222222";
    private const string RecordCommit = "3333333333333333333333333333333333333333";

    private static readonly DateTimeOffset WorkTime = new DateTimeOffset(2026, 9, 27, 10, 0, 0, TimeSpan.Zero);

    private static readonly string[] CodeAndDocs = ["IronAbsolution.Tools/Sample.cs", "docs/design.md", "docs/reviews/pr-7.md"];

    [Fact]
    public void APullRequestWithNoRecordFails()
    {
        // Exit test 1 of PR-6: no approving record for the head gives a red check.
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(record: null));

        Assert.False(result.Passes);
        Assert.Contains("Rule: the review record 'docs/reviews/pr-7.md' is on the PR head (T-4).", result.Details);
        Assert.Contains($"Expected: the record on the head {Head}.", result.Details);
    }

    [Fact]
    public void AnApprovalOfTheEffectiveHeadPasses()
    {
        // Exit test 2 of PR-6: an approving record for the effective head gives a green check.
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(ReviewRecordText.Record(EffectiveHead, ReviewRecord.ApprovedVerdict)));

        Assert.True(result.Passes, string.Join("\n", result.Details));
        Assert.Contains($"Effective head: {EffectiveHead}.", result.Details);
    }

    [Fact]
    public void AShortHashOfTheEffectiveHeadPasses()
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(ReviewRecordText.Record(EffectiveHead[..7], ReviewRecord.ApprovedVerdict)));

        Assert.True(result.Passes, string.Join("\n", result.Details));
    }

    [Theory]
    [InlineData("Changes required")]
    [InlineData("Blocked")]
    public void AVerdictThatDoesNotApproveFails(string verdict)
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(ReviewRecordText.Record(EffectiveHead, verdict)));

        Assert.False(result.Passes);
        Assert.Contains($"Found: '{verdict}'. Last change of the record: {RecordCommit} \"docs: review record of #7 (PR-6)\".", result.Details);
    }

    [Fact]
    public void ARecordWithTwoVerdictsFailsWithTheErrorOfTheParse()
    {
        string record = ReviewRecordText.Record(EffectiveHead, ReviewRecord.ApprovedVerdict) + "Blocked.\n";

        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(record));

        Assert.False(result.Passes);
        Assert.Contains(result.Details, line => line.Contains("names 2 verdicts: Ready for owner merge, Blocked", StringComparison.Ordinal));
    }

    [Fact]
    public void AnApprovalOfAnOlderHeadFails()
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(ReviewRecordText.Record("4444444", ReviewRecord.ApprovedVerdict)));

        Assert.False(result.Passes);
        Assert.Contains($"Expected: '{EffectiveHead}'.", result.Details);
        Assert.Contains(result.Details, line => line.StartsWith("Found: '4444444'. A commit outside the documents set came after the review.", StringComparison.Ordinal));
    }

    [Fact]
    public void AnApprovalOfADocumentsOnlyPullRequestFailsAndNamesTheLabel()
    {
        // D-66: such a PR merges through the label alone, also with an approving record.
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(
            ReviewRecordText.Record(Head, ReviewRecord.ApprovedVerdict),
            paths: ["docs/design.md"],
            effectiveHead: null));

        Assert.False(result.Passes);
        Assert.Contains(result.Details, line => line.Contains("adds the label 'review-override'", StringComparison.Ordinal));
    }

    [Fact]
    public void TheLabelBeatsARecordThatDoesNotApprove()
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(
            ReviewRecordText.Record(Head, "Changes required"),
            paths: ["docs/design.md", "docs/session-handoff.md"],
            effectiveHead: null,
            labelTime: WorkTime.AddMinutes(5)));

        Assert.True(result.Passes, string.Join("\n", result.Details));
        Assert.Equal("override by the label 'review-override'", result.Title);
        Assert.Contains("Label: review-override, added by owner-login at 2026-09-27T10:05:00.0000000+00:00.", result.Details);
    }

    [Theory]
    [InlineData("docs/design.md")]
    [InlineData(".claude/skills/pr-review/SKILL.md")]
    [InlineData("CLAUDE.md")]
    [InlineData("AGENTS.md")]
    [InlineData("README.md")]
    [InlineData("LICENSE")]
    public void TheLabelPassesForEachPathOfTheDocumentsSet(string path)
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(record: null, paths: [path], effectiveHead: null, labelTime: WorkTime));

        Assert.True(result.Passes, string.Join("\n", result.Details));
    }

    [Theory]
    [InlineData("IronAbsolution.Tools/Sample.cs")]
    [InlineData(".github/workflows/review-gate.yml")]
    [InlineData(".claude/settings.json")]
    [InlineData("LICENSE/tool.cs")]
    [InlineData("Makefile")]
    public void TheLabelFailsForAPathOutsideTheDocumentsSet(string path)
    {
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(
            ReviewRecordText.Record(EffectiveHead, ReviewRecord.ApprovedVerdict),
            paths: ["docs/design.md", path],
            labelTime: WorkTime.AddHours(1)));

        Assert.False(result.Passes);
        Assert.Contains(result.Details, line => line.StartsWith($"Found: 1 path(s) outside the documents set: {path}.", StringComparison.Ordinal));
    }

    [Fact]
    public void TheLabelFailsWithNoLabeledEvent()
    {
        ReviewGateFacts facts = Facts(record: null, paths: ["docs/design.md"], effectiveHead: null, labelTime: WorkTime);

        ReviewGateResult result = ReviewGateRules.Evaluate(Copy(facts, newestEvent: null, workHead: facts.WorkHead));

        Assert.False(result.Passes);
        Assert.Contains("Found: no labeled event in the timeline.", result.Details);
    }

    [Fact]
    public void TheLabelFailsWhenTheWorkHeadIsNewerThanTheLabel()
    {
        // D-65: a documents commit after the label needs the label again.
        ReviewGateResult result = ReviewGateRules.Evaluate(Facts(record: null, paths: ["docs/design.md"], effectiveHead: null, labelTime: WorkTime.AddSeconds(-1)));

        Assert.False(result.Passes);
        Assert.Contains(result.Details, line => line.EndsWith("Remove the label, and add it again.", StringComparison.Ordinal));
    }

    [Fact]
    public void TheLabelPassesWhenEveryCommitIsAMetadataCommit()
    {
        ReviewGateFacts facts = Facts(record: null, paths: ["docs/session-handoff.md"], effectiveHead: null, labelTime: WorkTime);

        ReviewGateResult result = ReviewGateRules.Evaluate(Copy(facts, newestEvent: facts.NewestOverrideEvent, workHead: null));

        Assert.True(result.Passes, string.Join("\n", result.Details));
        Assert.Contains("Work head: none, because every commit is a metadata commit.", result.Details);
    }

    private static ReviewGateFacts Facts(
        string? record,
        IReadOnlyList<string>? paths = null,
        string? effectiveHead = EffectiveHead,
        DateTimeOffset? labelTime = null)
    {
        return new ReviewGateFacts
        {
            PullRequest = PullRequest,
            Head = Head,
            ChangedPaths = paths ?? CodeAndDocs,
            EffectiveHead = effectiveHead,
            WorkHead = new CommitStamp(Head, WorkTime),
            ReviewRecordText = record,
            ReviewRecordCommit = record is null ? null : new CommitSubject(RecordCommit, "docs: review record of #7 (PR-6)"),
            HasOverrideLabel = labelTime is not null,
            NewestOverrideEvent = labelTime is null ? null : new LabelEvent(labelTime.Value, "owner-login"),
        };
    }

    private static ReviewGateFacts Copy(ReviewGateFacts facts, LabelEvent? newestEvent, CommitStamp? workHead)
    {
        return new ReviewGateFacts
        {
            PullRequest = facts.PullRequest,
            Head = facts.Head,
            ChangedPaths = facts.ChangedPaths,
            EffectiveHead = facts.EffectiveHead,
            WorkHead = workHead,
            ReviewRecordText = facts.ReviewRecordText,
            ReviewRecordCommit = facts.ReviewRecordCommit,
            HasOverrideLabel = facts.HasOverrideLabel,
            NewestOverrideEvent = newestEvent,
        };
    }
}
