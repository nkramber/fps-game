using System;
using IronAbsolution.Tools.CodexReview;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The effective head and the work head against real commits (D-14, D-49). The effective head
/// skips the documents set, and the work head skips the metadata set alone.
/// </summary>
public sealed class ReviewHeadsTests
{
    private const string Branch = "feat/pr-3-codex-review";

    [Fact]
    public void BothHeadsSkipTheMetadataCommits()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string code = StartBranch(repo);
        string metadata = repo.Commit("docs: handoff", ("docs/session-handoff.md", "entry"), ("docs/reviews/pr-4-response.md", "answer"));
        GitRepository git = new GitRepository(repo.Root);

        Assert.Equal(code, ReviewHeads.EffectiveHead(git, "main", metadata));
        Assert.Equal(code, ReviewHeads.WorkHead(git, "main", metadata));
    }

    [Fact]
    public void TheEffectiveHeadSkipsTheDocumentsCommitsAndTheWorkHeadDoesNot()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string code = StartBranch(repo);
        string documents = repo.Commit("docs: a skill and the agent files", (".claude/skills/pr-review/SKILL.md", "skill"), ("AGENTS.md", "agent"));
        GitRepository git = new GitRepository(repo.Root);

        Assert.Equal(code, ReviewHeads.EffectiveHead(git, "main", documents));
        Assert.Equal(documents, ReviewHeads.WorkHead(git, "main", documents));
    }

    [Fact]
    public void ADocumentsOnlyPullRequestHasNoEffectiveHead()
    {
        // D-49: the start checks refuse such a PR.
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string root = repo.Commit("chore: root", ("README.md", "root"));
        repo.SetOrigin("main", root);
        repo.CreateBranch(Branch);
        string documents = repo.Commit("docs: a design change", ("docs/design.md", "text"));
        GitRepository git = new GitRepository(repo.Root);

        Assert.Null(ReviewHeads.EffectiveHead(git, "main", documents));
        Assert.Equal(documents, ReviewHeads.WorkHead(git, "main", documents));
    }

    [Fact]
    public void ACodeFileUnderAFolderWithTheNameOfADocumentMovesTheEffectiveHead()
    {
        // The pathspec ':(exclude)LICENSE' also hides the folder 'LICENSE/', so a second walk reads it.
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        StartBranch(repo);
        string hidden = repo.Commit("feat: a file under a folder named LICENSE", ("LICENSE/evil.cs", "// evil"));
        repo.Commit("docs: a design change", ("docs/design.md", "text"));
        string tip = repo.Git("rev-parse", "HEAD").Trim();

        Assert.Equal(hidden, ReviewHeads.EffectiveHead(new GitRepository(repo.Root), "main", tip));
    }

    [Theory]
    [InlineData("1111111", true)]
    [InlineData("1111111111111111111111111111111111111111", true)]
    [InlineData("111111", false)]
    [InlineData("2222222", false)]
    [InlineData("11111111111111111111111111111111111111111", false)]
    public void AShortHashMatchesByPrefixWithSevenCharactersOrMore(string recorded, bool matches)
    {
        Assert.Equal(matches, ReviewHeads.HeadMatches(recorded, "1111111111111111111111111111111111111111"));
    }

    [Theory]
    [InlineData("docs/design.md", true)]
    [InlineData("docs/reviews/pr-7.md", true)]
    [InlineData(".claude/skills/pr-review/SKILL.md", true)]
    [InlineData("CLAUDE.md", true)]
    [InlineData("AGENTS.md", true)]
    [InlineData("README.md", true)]
    [InlineData("LICENSE", true)]
    [InlineData("LICENSE/tool.cs", false)]
    [InlineData("README.md.cs", false)]
    [InlineData(".claude/settings.json", false)]
    [InlineData("docs", false)]
    [InlineData("tools/docs/a.cs", false)]
    [InlineData(".github/workflows/review-gate.yml", false)]
    public void ThePathTestReadsTheDocumentsSet(string path, bool isDocument)
    {
        Assert.Equal(isDocument, ReviewHeads.IsDocument(path));
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
