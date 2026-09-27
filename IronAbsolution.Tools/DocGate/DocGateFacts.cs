using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools.CodexReview;

namespace IronAbsolution.Tools.DocGate;

/// <summary>
/// Every fact that the rules of the documents gate need, read one time from git and from the
/// file of the PR description. The rules do no I/O.
/// </summary>
/// <remarks>
/// The changed paths come from the merge base of the base branch and the head. The handoff
/// text comes from the head commit, never from the working tree, so an entry that is not
/// committed does not pass the gate. The PR description is data of the event, not a file of
/// the repository, so an edit of the Documents section moves no head (D-56).
/// </remarks>
public sealed class DocGateFacts
{
    /// <summary>Gets the PR title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the name of the PR branch.</summary>
    public required string Branch { get; init; }

    /// <summary>Gets the PR description. An empty description is an empty string.</summary>
    public required string Body { get; init; }

    /// <summary>Gets every path that the PR changes, from the merge base to the head, with forward slashes.</summary>
    public required IReadOnlyList<string> ChangedPaths { get; init; }

    /// <summary>Gets the first session entry of the handoff at the head, or null when the file or the entry is absent.</summary>
    public required string? NewestHandoffEntry { get; init; }

    /// <summary>Gets the full message of each commit from the base to the head, newest first. The attribution rule reads them (T-6).</summary>
    public required IReadOnlyList<CommitMessage> CommitMessages { get; init; }

    /// <summary>Reads the changed paths, the handoff, and the commit messages from git, and the description from its file.</summary>
    /// <param name="root">The root of the checkout.</param>
    /// <param name="baseRevision">The base branch, such as `origin/main`.</param>
    /// <param name="head">The head of the PR.</param>
    /// <param name="bodyPath">The file that holds the PR description.</param>
    /// <param name="title">The PR title.</param>
    /// <param name="branch">The PR branch.</param>
    /// <returns>The facts of the PR.</returns>
    /// <exception cref="InvalidOperationException">A git command failed. The message names the command and stderr.</exception>
    /// <exception cref="FileNotFoundException">The description file is absent. The message names the path.</exception>
    public static DocGateFacts Gather(string root, string baseRevision, string head, string bodyPath, string title, string branch)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentException.ThrowIfNullOrEmpty(baseRevision);
        ArgumentException.ThrowIfNullOrEmpty(head);
        ArgumentException.ThrowIfNullOrEmpty(bodyPath);
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentException.ThrowIfNullOrEmpty(branch);

        if (!File.Exists(bodyPath))
        {
            throw new FileNotFoundException(
                $"The PR description file '{bodyPath}' does not exist. The workflow writes it from the event before the gate runs.", bodyPath);
        }

        GitRepository git = new GitRepository(root);
        string mergeBase = git.MergeBase(baseRevision, head);
        string? handoff = git.ReadFileOrNull(head, DocGateRules.HandoffPath);
        return new DocGateFacts
        {
            Title = title,
            Branch = branch,
            Body = File.ReadAllText(bodyPath),
            ChangedPaths = git.ChangedPaths(mergeBase, head),
            NewestHandoffEntry = handoff is null ? null : DocGateRules.NewestHandoffEntry(handoff),
            CommitMessages = git.CommitMessages(baseRevision, head),
        };
    }
}
