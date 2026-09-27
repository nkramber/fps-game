using System;
using IronAbsolution.Tools.CodexReview;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>The version of the Codex CLI: the parse, the minimum, and each form that the parse refuses (D-14).</summary>
public sealed class CodexVersionTests
{
    [Theory]
    [InlineData("codex-cli 0.156.1\n", 0, 156, 1, "")]
    [InlineData("codex-cli 0.155.0-alpha.9.2", 0, 155, 0, "alpha.9.2")]
    public void TheParseReadsTheNumbersAndThePrerelease(string output, int major, int minor, int patch, string prerelease)
    {
        Assert.Equal(new CodexVersion(major, minor, patch, prerelease), CodexVersion.Parse(output));
    }

    [Theory]
    [InlineData("codex-cli 0.156.1", true)]
    [InlineData("codex-cli 0.156.2", true)]
    [InlineData("codex-cli 0.157.0-alpha.11", true)]
    [InlineData("codex-cli 1.0.0", true)]
    [InlineData("codex-cli 0.156.1-alpha.1", false)]
    [InlineData("codex-cli 0.155.0-alpha.9.2", false)]
    [InlineData("codex-cli 0.39.0", false)]
    public void AVersionPassesAtTheMinimumOrNewer(string output, bool passes)
    {
        // A prerelease of the minimum is older than the minimum itself.
        Assert.Equal(passes, CodexVersion.Parse(output).IsAtLeast(CodexReviewSettings.MinimumVersion));
    }

    [Theory]
    [InlineData("codex 0.156.1")]
    [InlineData("codex-cli 0.156")]
    [InlineData("codex-cli 0.x.1")]
    public void TheParseRefusesAnotherForm(string output)
    {
        FormatException exception = Assert.Throws<FormatException>(() => CodexVersion.Parse(output));

        Assert.Contains("version", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTextOfAVersionHoldsThePrerelease()
    {
        Assert.Equal("0.155.0-alpha.9.2", CodexVersion.Parse("codex-cli 0.155.0-alpha.9.2").ToString());
        Assert.Equal("0.156.1", CodexVersion.Parse("codex-cli 0.156.1").ToString());
    }
}
