using System;
using System.Collections.Generic;
using IronAbsolution.Tools.CodexReview;
using Xunit;
using static IronAbsolution.Tests.ReviewRecordText;

namespace IronAbsolution.Tests;

/// <summary>The findings of a review record: the heading, the status forms, and the `Open at:` line (D-14).</summary>
public sealed class ReviewFindingsTests
{
    private const string Head = "1111111111111111111111111111111111111111";

    [Theory]
    [InlineData("open", true)]
    [InlineData("fixed in `2222222`", false)]
    [InlineData("fixed in `0123456789abcdef0123456789abcdef01234567`", false)]
    [InlineData("accepted risk, D-53", false)]
    [InlineData("withdrawn", false)]
    public void EveryKnownStatusParses(string status, bool open)
    {
        ReviewFinding finding = Assert.Single(ReviewFindings.Parse(Record(Head, "Changes required", Finding("P1-1", status, Head))));

        Assert.Equal(open, finding.IsOpen);
    }

    [Theory]
    [InlineData("Open")]
    [InlineData("OPEN.")]
    [InlineData("closed")]
    [InlineData("Fixed in `2222222`.")]
    [InlineData("")]
    [InlineData("fixed without evidence")]
    [InlineData("fixed in `2222222` and more")]
    [InlineData("withdrawn for now")]
    [InlineData("refuted")]
    public void AnUnknownStatusIsAnErrorThatNamesTheFinding(string status)
    {
        FormatException error = Assert.Throws<FormatException>(() => ReviewFindings.Parse(Record(Head, "Changes required", Finding("P2-3", status, Head))));

        Assert.Contains("P2-3", error.Message, StringComparison.Ordinal);
        Assert.Contains(ReviewFindings.StatusForms, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFindingWithNoStatusIsAnError()
    {
        FormatException error = Assert.Throws<FormatException>(() => ReviewFindings.Parse(Record(Head, "Changes required", "### P1-1: A defect\n\nFile: `a.cs:1`.\n")));

        Assert.Contains("P1-1", error.Message, StringComparison.Ordinal);
        Assert.Contains("Status:", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheOpenAtLineGivesEachHeadInOrder()
    {
        ReviewFinding finding = Assert.Single(ReviewFindings.Parse(Record(Head, "Changes required", Finding("P1-1", "open", "aaaaaaa", "bbbbbbb", Head))));

        Assert.Equal(["aaaaaaa", "bbbbbbb", Head], finding.OpenAt);
        Assert.Equal(1, finding.Severity);
    }

    [Fact]
    public void AFindingHeadingOutsideTheFindingsSectionDoesNotCount()
    {
        string record = Record(Head, "Ready for owner merge") + "\n## Earlier notes\n\n### P1-9: Old\n\nStatus: open.\n";

        Assert.Empty(ReviewFindings.Parse(record));
    }

    [Fact]
    public void ARecordWithNoFindingsSectionIsAnError()
    {
        FormatException error = Assert.Throws<FormatException>(() => ReviewFindings.Parse("# PR-4 review\n\n## Verdict\n\n**Blocked.**\n"));

        Assert.Contains(ReviewFindings.SectionHeading, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AWindowsLineEndingReadsTheSame()
    {
        string record = Record(Head, "Changes required", Finding("P1-1", "open", Head)).Replace("\n", "\r\n", StringComparison.Ordinal);

        IReadOnlyList<ReviewFinding> findings = ReviewFindings.Parse(record);

        Assert.True(Assert.Single(findings).IsOpen);
    }
}
