using System;
using IronAbsolution.Tools.CodexReview;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>The git runner of the review: the read of a file at a revision, and each fault with its context (T-2).</summary>
public sealed class GitRepositoryTests
{
    [Fact]
    public void TheReadGivesTheFileAtTheRevision()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string first = repo.Commit("docs: first", ("docs/reviews/pr-4.md", "round one"));
        repo.Commit("docs: second", ("docs/reviews/pr-4.md", "round two"));
        GitRepository git = new GitRepository(repo.Root);

        Assert.Equal("round one", git.ReadFileOrNull(first, "docs/reviews/pr-4.md"));
        Assert.Equal("round two", git.ReadFileOrNull("HEAD", "docs/reviews/pr-4.md"));
    }

    [Fact]
    public void AnAbsentFileGivesNull()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        repo.Commit("chore: root", ("README.md", "root"));

        Assert.Null(new GitRepository(repo.Root).ReadFileOrNull("HEAD", "docs/reviews/pr-4.md"));
    }

    [Fact]
    public void ABadRevisionIsAnErrorThatNamesTheCommand()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        repo.Commit("chore: root", ("README.md", "root"));

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => new GitRepository(repo.Root).ReadFileOrNull("no-such-revision", "README.md"));

        Assert.Contains("git ls-tree no-such-revision", error.Message, StringComparison.Ordinal);
        Assert.Contains(repo.Root, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheNewestCommitOutsideIsNullWhenEveryCommitIsExcluded()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string root = repo.Commit("chore: root", ("README.md", "root"));
        string documents = repo.Commit("docs: a change", ("docs/design.md", "text"));

        Assert.Null(new GitRepository(repo.Root).NewestCommitOutside(root, documents, ["docs/"]));
    }
}
