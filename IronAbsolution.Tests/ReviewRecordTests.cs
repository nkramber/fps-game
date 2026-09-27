using System;
using IronAbsolution.Tools.CodexReview;
using Xunit;
using static IronAbsolution.Tests.ReviewRecordText;

namespace IronAbsolution.Tests;

/// <summary>The two machine-read parts of a review record: the head and the verdict (D-14).</summary>
public sealed class ReviewRecordTests
{
    private const string Head = "1111111111111111111111111111111111111111";

    [Fact]
    public void TheParseReadsTheHeadAndTheVerdict()
    {
        ReviewRecord? record = ReviewRecord.TryParse(Record(Head, "Changes required"), out string error);

        Assert.Equal(new ReviewRecord(Head, "Changes required"), record);
        Assert.Empty(error);
    }

    [Fact]
    public void AnEarlierVerdictsSectionIsNotTheVerdict()
    {
        // The skeleton keeps the verdicts of earlier rounds under a heading that starts with another word.
        string text = Record(Head, "Ready for owner merge").Replace(
            "## Earlier verdicts\n\nNone.",
            "## Earlier verdicts\n\n- `aaaaaaa`: Changes required.",
            StringComparison.Ordinal);

        Assert.Equal("Ready for owner merge", ReviewRecord.TryParse(text, out string _)?.Verdict);
    }

    [Fact]
    public void AVerdictSectionWithTwoNamesIsAnError()
    {
        string text = Record(Head, "Ready for owner merge") + "Changes required before this.\n";

        Assert.Null(ReviewRecord.TryParse(text, out string error));
        Assert.Contains("2 verdicts", error, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordWithNoHeadLineIsAnError()
    {
        string text = Record(Head, "Blocked").Replace($"- Head: `{Head}`", "- Head: none", StringComparison.Ordinal);

        Assert.Null(ReviewRecord.TryParse(text, out string error));
        Assert.Contains(ReviewRecord.HeadPrefix, error, StringComparison.Ordinal);
    }

    [Fact]
    public void ARecordInsideAFenceIsNotTheRecord()
    {
        // An example of a record inside a fence does not count.
        string text = "# Notes\n\n```markdown\n- Head: `2222222`\n\n## Verdict\n\n**Ready for owner merge.**\n```\n";

        Assert.Null(ReviewRecord.TryParse(text, out string error));
        Assert.Contains(ReviewRecord.HeadPrefix, error, StringComparison.Ordinal);
    }

    [Fact]
    public void ATildeLineInsideABacktickFenceDoesNotCloseIt()
    {
        string text = "```\n~~~\n- Head: `2222222`\n```\n" + Record(Head, "Blocked");

        Assert.Equal(Head, ReviewRecord.TryParse(text, out string _)?.RecordedHead);
    }

    [Fact]
    public void TheFileNameTakesTheGitHubNumber()
    {
        Assert.Equal("docs/reviews/pr-4.md", ReviewRecord.FilePath(4));
    }
}
