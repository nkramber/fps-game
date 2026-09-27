using System;
using System.Collections.Generic;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>
/// The two heads of a PR that the review reads. The effective head is the commit that a review
/// record names (D-49). The work head shows whether a commit other than a metadata commit
/// arrived during a review (D-14).
/// </summary>
public static class ReviewHeads
{
    /// <summary>
    /// The documents set (D-49). A commit that changes only these paths does not move the
    /// effective head. An entry that ends in a slash holds each path under it.
    /// </summary>
    public static readonly IReadOnlyList<string> DocumentPaths = ["docs/", ".claude/skills/", "CLAUDE.md", "AGENTS.md", "README.md", "LICENSE"];

    /// <summary>
    /// The metadata set (D-14). A commit that changes only these paths does not move the work
    /// head. The review commit holds the review record and the handoff entry, so it is always
    /// a metadata commit.
    /// </summary>
    public static readonly IReadOnlyList<string> MetadataPaths = ["docs/reviews/", "docs/session-handoff.md", "docs/session-handoff-archive.md"];

    /// <summary>The shortest hash that a review record can name.</summary>
    public const int ShortestHash = 7;

    /// <summary>Gives the newest commit from the merge base to the head that changes a path outside the documents set (D-49).</summary>
    /// <param name="git">The checkout.</param>
    /// <param name="baseBranch">The base branch of the PR on origin, such as `main`.</param>
    /// <param name="head">The head of the PR.</param>
    /// <returns>The hash, or null when every commit of the PR changes documents alone.</returns>
    public static string? EffectiveHead(GitRepository git, string baseBranch, string head)
    {
        ArgumentNullException.ThrowIfNull(git);
        string mergeBase = git.MergeBase($"refs/remotes/origin/{baseBranch}", head);
        return git.NewestCommitOutside(mergeBase, head, DocumentPaths);
    }

    /// <summary>Gives the newest commit from the merge base to the head that changes a path outside the metadata set (D-14).</summary>
    /// <param name="git">The checkout.</param>
    /// <param name="baseBranch">The base branch of the PR on origin, such as `main`.</param>
    /// <param name="head">The head of the PR.</param>
    /// <returns>The hash, or null when every commit of the PR is a metadata commit.</returns>
    public static string? WorkHead(GitRepository git, string baseBranch, string head)
    {
        ArgumentNullException.ThrowIfNull(git);
        string mergeBase = git.MergeBase($"refs/remotes/origin/{baseBranch}", head);
        return git.NewestCommitOutside(mergeBase, head, MetadataPaths);
    }

    /// <summary>Tells whether a hash of a review record names the full hash. A short hash matches by prefix.</summary>
    /// <param name="recorded">The hash in the record, with <see cref="ShortestHash"/> characters or more.</param>
    /// <param name="full">The full hash of the commit.</param>
    /// <returns>True when the recorded hash names the commit.</returns>
    public static bool HeadMatches(string recorded, string full)
    {
        ArgumentNullException.ThrowIfNull(recorded);
        ArgumentNullException.ThrowIfNull(full);

        if (recorded.Length < ShortestHash || recorded.Length > full.Length)
        {
            return false;
        }

        return full.StartsWith(recorded, StringComparison.OrdinalIgnoreCase);
    }
}
