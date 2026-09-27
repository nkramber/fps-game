using System;
using System.Collections.Generic;
using System.Linq;
using IronAbsolution.Tools.CodexReview;

namespace IronAbsolution.Tools.ReviewGate;

/// <summary>A commit and its committer time.</summary>
/// <param name="Sha">The full hash of the commit.</param>
/// <param name="CommitTime">The committer time of the commit.</param>
public sealed record CommitStamp(string Sha, DateTimeOffset CommitTime);

/// <summary>
/// Every fact that the rules of the review gate need, read one time from git and from the
/// labels file. The rules do no I/O.
/// </summary>
/// <remarks>
/// The workflow runs the tool from the base branch, and the PR head is data in the object
/// store alone (D-64). Every fact of the head comes from git, never from the working tree, so
/// a PR cannot change the code that judges it.
/// </remarks>
public sealed class ReviewGateFacts
{
    /// <summary>Gets the GitHub number of the PR. It names the review record.</summary>
    public required int PullRequest { get; init; }

    /// <summary>Gets the full hash of the PR head.</summary>
    public required string Head { get; init; }

    /// <summary>Gets every path that the PR changes, from the merge base to the head, with forward slashes.</summary>
    public required IReadOnlyList<string> ChangedPaths { get; init; }

    /// <summary>Gets the newest commit outside the documents set, or null when the PR changes documents alone (D-49).</summary>
    public required string? EffectiveHead { get; init; }

    /// <summary>Gets the newest commit outside the metadata set, or null when every commit is a metadata commit. The override label reads it (D-65).</summary>
    public required CommitStamp? WorkHead { get; init; }

    /// <summary>Gets the text of the review record at the head, or null when the head holds no record.</summary>
    public required string? ReviewRecordText { get; init; }

    /// <summary>Gets the newest commit that changed the review record, or null when no commit did (F-9).</summary>
    public required CommitSubject? ReviewRecordCommit { get; init; }

    /// <summary>Gets a value that tells whether the PR has the override label now.</summary>
    public required bool HasOverrideLabel { get; init; }

    /// <summary>Gets the newest labeled event of the override label, or null when the timeline holds none.</summary>
    public required LabelEvent? NewestOverrideEvent { get; init; }

    /// <summary>Reads the facts of a PR from git and from its labels.</summary>
    /// <param name="root">The root of the checkout of the base branch. The object store holds the PR head.</param>
    /// <param name="baseRevision">The base branch, such as `origin/main`.</param>
    /// <param name="head">The full hash of the PR head.</param>
    /// <param name="pullRequest">The GitHub number of the PR.</param>
    /// <param name="labels">The labels and the override label events of the PR.</param>
    /// <returns>The facts of the PR.</returns>
    /// <exception cref="InvalidOperationException">A git command failed. The message names the command and stderr.</exception>
    public static ReviewGateFacts Gather(string root, string baseRevision, string head, int pullRequest, PullRequestLabels labels)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentException.ThrowIfNullOrEmpty(baseRevision);
        ArgumentException.ThrowIfNullOrEmpty(head);
        ArgumentNullException.ThrowIfNull(labels);

        GitRepository git = new GitRepository(root);
        string mergeBase = git.MergeBase(baseRevision, head);
        string recordPath = ReviewRecord.FilePath(pullRequest);
        string? workHead = git.NewestCommitOutside(mergeBase, head, ReviewHeads.MetadataPaths);
        return new ReviewGateFacts
        {
            PullRequest = pullRequest,
            Head = head,
            ChangedPaths = git.ChangedPaths(mergeBase, head),
            EffectiveHead = git.NewestCommitOutside(mergeBase, head, ReviewHeads.DocumentPaths),
            WorkHead = workHead is null ? null : new CommitStamp(workHead, git.CommitTime(workHead)),
            ReviewRecordText = git.ReadFileOrNull(head, recordPath),
            ReviewRecordCommit = git.NewestCommitThatChanged(head, recordPath),
            HasOverrideLabel = labels.Names.Contains(ReviewGateRules.OverrideLabel),
            NewestOverrideEvent = NewestEvent(labels.OverrideEvents),
        };
    }

    private static LabelEvent? NewestEvent(IReadOnlyList<LabelEvent> events)
    {
        LabelEvent? newest = null;
        foreach (LabelEvent candidate in events)
        {
            if (newest is null || candidate.CreatedAt > newest.CreatedAt)
            {
                newest = candidate;
            }
        }

        return newest;
    }
}
