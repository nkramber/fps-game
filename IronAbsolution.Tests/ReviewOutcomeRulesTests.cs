using System;
using IronAbsolution.Tools.CodexReview;
using Xunit;
using static IronAbsolution.Tests.ReviewRecordText;

namespace IronAbsolution.Tests;

/// <summary>
/// The outcome of one review round: the verdict, the stale head, the three-strike count, and
/// each record that contradicts itself (D-14, D-49).
/// </summary>
public sealed class ReviewOutcomeRulesTests
{
    private const string Head = "1111111111111111111111111111111111111111";
    private const string RoundOne = "aaaaaaa";
    private const string RoundTwo = "bbbbbbb";
    private const string ReviewFile = "docs/reviews/pr-4.md";
    private const string Approve = "Ready for owner merge";
    private const string Changes = "Changes required";

    [Fact]
    public void AnApprovingRecordOfTheEffectiveHeadApproves()
    {
        ReviewOutcome outcome = Judge(Record(Head, Approve));

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
        Assert.Empty(outcome.OpenFindingIds);
    }

    [Fact]
    public void AShortHashOfTheEffectiveHeadApproves()
    {
        Assert.Equal(CodexReviewExit.Approve, Judge(Record(Head[..7], Approve)).Exit);
    }

    [Theory]
    [InlineData("Not Ready for owner merge")]
    [InlineData("Changes Required. Ready for owner merge after P1-1")]
    public void AVerdictThatDoesNotStartWithTheNameIsAFault(string verdict)
    {
        // A verdict that holds the approving name inside other words is never an approval.
        ReviewOutcome outcome = Judge(Record(Head, verdict));

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains("## Verdict", outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFindingOpenInRoundsOneAndTwoDoesNotStop()
    {
        ReviewOutcome outcome = Judge(Record(Head, Changes, Finding("P1-1", "open", RoundOne, Head)));

        Assert.Equal(CodexReviewExit.ChangesRequired, outcome.Exit);
        Assert.Equal(["P1-1"], outcome.OpenFindingIds);
        Assert.Empty(outcome.StrikeFindingIds);
    }

    [Fact]
    public void AFindingOpenInRoundThreeStops()
    {
        // D-14: the third round in which one id is open stops the fix loop.
        ReviewOutcome outcome = Judge(Record(Head, Changes, Finding("P2-1", "open", RoundOne, RoundTwo, Head), Finding("P1-2", "open", Head)));

        Assert.Equal(CodexReviewExit.ThreeStrikes, outcome.Exit);
        Assert.Equal(11, (int)outcome.Exit);
        Assert.Equal(["P2-1"], outcome.StrikeFindingIds);
        Assert.Equal(["P2-1", "P1-2"], outcome.OpenFindingIds);
    }

    [Fact]
    public void AFixedAndReopenedFindingCountsEachOpenRound()
    {
        // The same id counts one time for each round in which it is open. The rounds need not follow each other.
        ReviewOutcome third = Judge(Record(Head, Changes, Finding("P1-1", "open", RoundOne, RoundTwo, Head)));
        ReviewOutcome second = Judge(Record(Head, Changes, Finding("P1-1", "open", RoundOne, Head)));

        Assert.Equal(CodexReviewExit.ThreeStrikes, third.Exit);
        Assert.Equal(CodexReviewExit.ChangesRequired, second.Exit);
    }

    [Fact]
    public void AP3FindingNeverStops()
    {
        // A P3 never blocks the merge, so it never drives the fix loop.
        ReviewOutcome outcome = Judge(Record(Head, Changes, Finding("P3-1", "open", RoundOne, RoundTwo, Head), Finding("P2-1", "open", Head)));

        Assert.Equal(CodexReviewExit.ChangesRequired, outcome.Exit);
        Assert.Equal(10, (int)outcome.Exit);
        Assert.Empty(outcome.StrikeFindingIds);
    }

    [Fact]
    public void AnApprovalWithAnOpenP3Approves()
    {
        ReviewOutcome outcome = Judge(Record(Head, Approve, Finding("P3-1", "open", RoundOne, RoundTwo, Head)));

        Assert.Equal(CodexReviewExit.Approve, outcome.Exit);
        Assert.Equal(["P3-1"], outcome.OpenFindingIds);
    }

    [Theory]
    [InlineData("P0-1")]
    [InlineData("P1-1")]
    [InlineData("P2-1")]
    public void AnApprovalWithAnOpenBlockingFindingIsAFault(string id)
    {
        // An approval needs no blocking finding, so the command never reports such a record as an approval.
        ReviewOutcome outcome = Judge(Record(Head, Approve, Finding(id, "open", Head)));

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(id, outcome.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("### P4-1: A defect", "severity P4")]
    [InlineData("### P10-1: A defect", "severity P10")]
    [InlineData("### P1: A defect", "not a finding heading")]
    [InlineData("### Notes", "not a finding heading")]
    [InlineData("#### P1-1: A defect", "not a finding heading")]
    [InlineData("###P1-1: A defect", "not a finding heading")]
    public void AFindingHeadingOutsideTheFormatIsAFault(string heading, string expected)
    {
        // A heading that the parser skips would pass in silence under an approval (T-2).
        ReviewOutcome outcome = Judge(Record(Head, Approve, heading + "\n\nStatus: open.\n\nOpen at: `" + Head + "`.\n"));

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(expected, outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnApprovalWithAnAcceptedRiskApproves()
    {
        Assert.Equal(CodexReviewExit.Approve, Judge(Record(Head, Approve, Finding("P2-1", "accepted risk, D-53", RoundOne, RoundTwo))).Exit);
    }

    [Fact]
    public void ABlockedRecordAsksForChanges()
    {
        Assert.Equal(CodexReviewExit.ChangesRequired, Judge(Record(Head, "Blocked")).Exit);
    }

    [Fact]
    public void AClosedFindingNeedsNoHeadOfThisRound()
    {
        Assert.Equal(CodexReviewExit.Approve, Judge(Record(Head, Approve, Finding("P1-1", "fixed in `2222222`", RoundOne, RoundTwo))).Exit);
    }

    [Fact]
    public void NoRecordIsAFault()
    {
        ReviewOutcome outcome = ReviewOutcomeRules.Judge(null, ReviewFile, Head);

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains(ReviewFile, outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStaleHeadIsAFault()
    {
        ReviewOutcome outcome = Judge(Record("2222222", Approve));

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains("stale", outcome.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("no 'Open at:' line")]
    [InlineData("does not list the head of this review")]
    [InlineData("two times")]
    [InlineData("seven characters")]
    public void AnOpenFindingWithAWrongHeadListIsAFault(string expected)
    {
        // T-2: a count that a reviewer forgot to extend is wrong in silence, so the rule stops.
        string finding = expected switch
        {
            "no 'Open at:' line" => Finding("P1-1", "open"),
            "does not list the head of this review" => Finding("P1-1", "open", RoundOne, RoundTwo),
            "two times" => Finding("P1-1", "open", RoundOne, "aaaaaaaaaa", Head),
            _ => Finding("P1-1", "open", "abc", Head),
        };

        ReviewOutcome outcome = Judge(Record(Head, Changes, finding));

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains("P1-1", outcome.Message, StringComparison.Ordinal);
        Assert.Contains(expected, outcome.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Open")]
    [InlineData("fixed")]
    [InlineData("fixed in 2222222")]
    [InlineData("accepted risk")]
    public void AFindingWithAnUnknownStatusIsAFault(string status)
    {
        // A status outside the complete forms never reads as closed under an approval.
        ReviewOutcome outcome = Judge(Record(Head, Approve, Finding("P1-1", status, Head)));

        Assert.Equal(CodexReviewExit.Fault, outcome.Exit);
        Assert.Contains($"the status '{status}.'", outcome.Message, StringComparison.Ordinal);
    }

    private static ReviewOutcome Judge(string record)
    {
        return ReviewOutcomeRules.Judge(record, ReviewFile, Head);
    }
}
