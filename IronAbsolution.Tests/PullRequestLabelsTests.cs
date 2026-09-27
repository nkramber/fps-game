using System;
using System.IO;
using IronAbsolution.Tools.ReviewGate;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The labels file that the workflow writes from the GitHub API (D-64, D-65). Every field is
/// required, and each fault names the source and the field (T-2).
/// </summary>
public sealed class PullRequestLabelsTests
{
    private const string Source = "labels.json";

    [Fact]
    public void TheParseReadsTheNamesAndTheEvents()
    {
        PullRequestLabels labels = PullRequestLabels.Parse(
            """{ "labels": ["review-override", "docs"], "overrideLabelEvents": [{ "createdAt": "2026-09-27T11:00:00Z", "actor": "owner-login" }] }""",
            Source);

        Assert.Equal(["review-override", "docs"], labels.Names);
        LabelEvent labelEvent = Assert.Single(labels.OverrideEvents);
        Assert.Equal(new DateTimeOffset(2026, 9, 27, 11, 0, 0, TimeSpan.Zero), labelEvent.CreatedAt);
        Assert.Equal("owner-login", labelEvent.Actor);
    }

    [Fact]
    public void EmptyListsAreValid()
    {
        PullRequestLabels labels = PullRequestLabels.Parse("""{ "labels": [], "overrideLabelEvents": [] }""", Source);

        Assert.Empty(labels.Names);
        Assert.Empty(labels.OverrideEvents);
    }

    [Theory]
    [InlineData("""{ "overrideLabelEvents": [] }""", "has no field 'labels' in the root")]
    [InlineData("""{ "labels": [] }""", "has no field 'overrideLabelEvents' in the root")]
    [InlineData("""{ "labels": null, "overrideLabelEvents": [] }""", "The field 'labels' of the labels file 'labels.json' holds a JSON Null")]
    [InlineData("""{ "labels": [null], "overrideLabelEvents": [] }""", "The field 'labels[0]' of the labels file 'labels.json' holds a JSON Null with no text")]
    [InlineData("""{ "labels": [""], "overrideLabelEvents": [] }""", "The field 'labels[0]' of the labels file 'labels.json' holds a JSON String with no text")]
    [InlineData("""{ "labels": [], "overrideLabelEvents": [{ "actor": "a" }] }""", "has no field 'createdAt' in overrideLabelEvents[0]")]
    [InlineData("""{ "labels": [], "overrideLabelEvents": [{ "createdAt": "2026-09-27T11:00:00Z" }] }""", "has no field 'actor' in overrideLabelEvents[0]")]
    [InlineData("""{ "labels": [], "overrideLabelEvents": [{ "createdAt": "yesterday", "actor": "a" }] }""", "The field 'overrideLabelEvents[0].createdAt' of the labels file 'labels.json' holds 'yesterday'")]
    [InlineData("""{ "labels": [], "overrideLabelEvents": [7] }""", "The field 'overrideLabelEvents[0]' of the labels file 'labels.json' holds a JSON Number")]
    [InlineData("""[]""", "The labels file 'labels.json' holds a JSON Array")]
    [InlineData("""{ "labels": [""", "The labels file 'labels.json' is not valid JSON")]
    public void AFileOfAnotherFormIsAnErrorThatNamesTheField(string json, string expected)
    {
        InvalidDataException error = Assert.Throws<InvalidDataException>(() => PullRequestLabels.Parse(json, Source));

        Assert.Contains(expected, error.Message, StringComparison.Ordinal);
    }
}
