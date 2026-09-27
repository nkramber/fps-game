using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools.SteCheck;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The rule of ste-check that `CLAUDE.md` and `AGENTS.md` stay identical (D-12). The
/// ste-check job runs on every PR, a documents PR too, so this rule is the check of D-12.
/// </summary>
public sealed class SteCheckAgentFileTests
{
    [Fact]
    public void TwoIdenticalFilesGiveNoFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();

        IReadOnlyList<Finding> findings = AgentFileRules.Check(DocumentSet.Read(checkout.Root));

        Assert.Empty(findings);
    }

    [Fact]
    public void AChangeToOneFileAloneIsAFindingAtTheLineThatDiffers()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Write(
            "AGENTS.md",
            "# The fixture instructions",
            string.Empty,
            "This file holds a rule that the other file does not hold.");

        IReadOnlyList<Finding> findings = AgentFileRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal(AgentFileRules.RuleId, found.Rule);
        Assert.Equal("AGENTS.md", found.File);
        Assert.Equal(3, found.Line);
        Assert.Contains("D-12", found.Detail, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("AGENTS.md", 4, "holds 4 lines, and `CLAUDE.md` holds 3")]
    [InlineData("CLAUDE.md", 4, "holds 3 lines, and `CLAUDE.md` holds 4")]
    public void ALineAddedToOneFileAloneIsAFinding(string path, int line, string detail)
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Append(path, "An added rule.");

        IReadOnlyList<Finding> findings = AgentFileRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal("AGENTS.md", found.File);
        Assert.Equal(line, found.Line);
        Assert.Contains(detail, found.Detail, StringComparison.Ordinal);
    }

    /// <summary>
    /// A rule that reads the lines alone passes a change of the last line end, or of each line
    /// end. The byte comparison of the rule catches it.
    /// </summary>
    [Theory]
    [InlineData("# Rules\nA rule.\n", "# Rules\nA rule.", 2)]
    [InlineData("# Rules\nA rule.", "# Rules\nA rule.\n", 2)]
    [InlineData("# Rules\nA rule.\n", "# Rules\r\nA rule.\r\n", 2)]
    [InlineData("# Rules\nA rule.\n", "# Rules\nA rule.\n\n", 3)]
    public void TwoFilesThatDifferInALineEndAloneGiveAFinding(string claudeText, string agentsText, int line)
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        File.WriteAllText(Path.Combine(checkout.Root, "CLAUDE.md"), claudeText);
        File.WriteAllText(Path.Combine(checkout.Root, "AGENTS.md"), agentsText);

        IReadOnlyList<Finding> findings = AgentFileRules.Check(DocumentSet.Read(checkout.Root));

        Finding found = Assert.Single(findings);
        Assert.Equal(AgentFileRules.RuleId, found.Rule);
        Assert.Equal("AGENTS.md", found.File);
        Assert.Equal(line, found.Line);
    }

    [Fact]
    public void TwoFilesWithTheSameBytesGiveNoFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        File.WriteAllText(Path.Combine(checkout.Root, "CLAUDE.md"), "# Rules\r\nA rule.");
        File.WriteAllText(Path.Combine(checkout.Root, "AGENTS.md"), "# Rules\r\nA rule.");

        Assert.Empty(AgentFileRules.Check(DocumentSet.Read(checkout.Root)));
    }

    [Theory]
    [InlineData("AGENTS.md")]
    [InlineData("CLAUDE.md")]
    public void AnAbsentFileStopsTheCheck(string path)
    {
        // An absent file is an error, never a pass (T-2).
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        File.Delete(Path.Combine(checkout.Root, path));

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => AgentFileRules.Check(DocumentSet.Read(checkout.Root)));

        Assert.Contains(path, fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandReportsTheFinding()
    {
        using SteCheckCheckout checkout = SteCheckCheckout.Build();
        checkout.Append("CLAUDE.md", "An added rule.");
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = SteCheckCommand.Run([SteCheckCommand.RootOption, checkout.Root], output, errors);

        Assert.Equal(1, exitCode);
        Assert.Contains($"AGENTS.md:4: rule {AgentFileRules.RuleId}:", output.ToString(), StringComparison.Ordinal);
    }
}
