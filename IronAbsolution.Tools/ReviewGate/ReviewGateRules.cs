using System;
using System.Collections.Generic;
using System.Globalization;
using IronAbsolution.Tools.CodexReview;

namespace IronAbsolution.Tools.ReviewGate;

/// <summary>The result of the review gate: pass or fail, a title, and the lines that explain it.</summary>
/// <param name="Passes">True when the gate passes.</param>
/// <param name="Title">One line that names the result.</param>
/// <param name="Details">The lines of the result. A failure gives the rule, the expected value, and the value found (T-2).</param>
public sealed record ReviewGateResult(bool Passes, string Title, IReadOnlyList<string> Details);

/// <summary>
/// The rules of the review gate (D-35, D-49, D-64 to D-66). The facts go in, and a result comes
/// out. The rules do no I/O.
/// </summary>
/// <remarks>
/// With the override label, the label path alone decides (D-65). Without it, the review path
/// decides: the record at the head approves the effective head. One shared GitHub account
/// cannot prove who wrote a record or who added a label (F-9). So each result names the commit
/// of the record or the account of the label, and the owner reads that line before a merge.
/// </remarks>
public static class ReviewGateRules
{
    /// <summary>The name of the job and of the required check (D-64).</summary>
    public const string CheckName = "review-gate";

    /// <summary>The label that the author session adds to a PR with no code (D-35, D-76).</summary>
    public const string OverrideLabel = "review-override";

    /// <summary>Applies the rules of the gate to the facts of one PR.</summary>
    /// <param name="facts">The facts of the PR.</param>
    /// <returns>The result of the gate.</returns>
    public static ReviewGateResult Evaluate(ReviewGateFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        if (facts.HasOverrideLabel)
        {
            return EvaluateOverride(facts);
        }

        return EvaluateReview(facts);
    }

    /// <summary>The label path (D-65): documents alone, a labeled event, and no work commit after that event.</summary>
    private static ReviewGateResult EvaluateOverride(ReviewGateFacts facts)
    {
        List<string> codePaths = [];
        foreach (string path in facts.ChangedPaths)
        {
            if (!ReviewHeads.IsDocument(path))
            {
                codePaths.Add(path);
            }
        }

        if (codePaths.Count > 0)
        {
            return Fail(
                $"with the label '{OverrideLabel}', every changed path is in the documents set ({string.Join(", ", ReviewHeads.DocumentPaths)}) (D-35, D-65)",
                "no changed path outside the documents set",
                $"{codePaths.Count} path(s) outside the documents set: {string.Join(", ", codePaths)}. Remove the label, and get a review record");
        }

        LabelEvent? labelEvent = facts.NewestOverrideEvent;
        if (labelEvent is null)
        {
            return Fail(
                $"with the label '{OverrideLabel}', the PR timeline holds a labeled event for it (D-65)",
                "one labeled event or more",
                "no labeled event in the timeline");
        }

        // The label reads the work head, not the effective head. A documents commit after the
        // label then needs the label again, so the owner sees each change that the label covers (D-65).
        // The commit author sets the committer time, so a backdated commit passes. The owner
        // accepts that risk: the rule stops an accident, and one account can add the label again (D-68, F-9).
        string labelTime = labelEvent.CreatedAt.ToString("O", CultureInfo.InvariantCulture);
        if (facts.WorkHead is not null && facts.WorkHead.CommitTime > labelEvent.CreatedAt)
        {
            return Fail(
                $"with the label '{OverrideLabel}', no commit outside the metadata set is newer than the newest labeled event (D-65)",
                $"a work head with a commit time at or before {labelTime}",
                $"the work head {facts.WorkHead.Sha} with the commit time {facts.WorkHead.CommitTime.ToString("O", CultureInfo.InvariantCulture)}. Remove the label, and add it again");
        }

        string workHead = facts.WorkHead?.Sha ?? "none, because every commit is a metadata commit";
        return new ReviewGateResult(
            true,
            $"override by the label '{OverrideLabel}'",
            [
                $"Label: {OverrideLabel}, added by {labelEvent.Actor} at {labelTime}.",
                $"Work head: {workHead}.",
                $"Each of the {facts.ChangedPaths.Count} changed path(s) is in the documents set (D-35, D-65).",
            ]);
    }

    /// <summary>The review path: the record at the head gives the approving verdict for the effective head (D-49, D-66).</summary>
    private static ReviewGateResult EvaluateReview(ReviewGateFacts facts)
    {
        string recordPath = ReviewRecord.FilePath(facts.PullRequest);
        if (facts.ReviewRecordText is null)
        {
            return Fail(
                $"the review record '{recordPath}' is on the PR head (T-4)",
                $"the record on the head {facts.Head}",
                "no record. Run `make codex-review`, or the author session adds the label for a PR with no code (D-76)");
        }

        ReviewRecord? record = ReviewRecord.TryParse(facts.ReviewRecordText, out string parseError);
        if (record is null)
        {
            return Fail(
                $"the review record '{recordPath}' holds a head and a verdict",
                $"the '{ReviewRecord.HeadPrefix.Trim()}' line and the '{ReviewRecord.VerdictHeading}' section",
                $"{parseError} {RecordCommitLine(facts)}");
        }

        if (record.Verdict != ReviewRecord.ApprovedVerdict)
        {
            return Fail(
                $"the verdict of '{recordPath}' approves the PR",
                $"'{ReviewRecord.ApprovedVerdict}'",
                $"'{record.Verdict}'. {RecordCommitLine(facts)}");
        }

        if (facts.EffectiveHead is null)
        {
            return Fail(
                "the review record names the effective head (D-49, D-66)",
                "one commit outside the documents set",
                $"no such commit. Every changed path is a document, so a review has nothing to approve. The author session adds the label '{OverrideLabel}' to such a PR after the last commit outside the metadata set (D-76)");
        }

        if (!ReviewHeads.HeadMatches(record.RecordedHead, facts.EffectiveHead))
        {
            return Fail(
                "the review record names the effective head (D-49)",
                $"'{facts.EffectiveHead}'",
                $"'{record.RecordedHead}'. A commit outside the documents set came after the review. Run `make codex-review` again");
        }

        return new ReviewGateResult(
            true,
            "an approving review of the effective head",
            [
                $"Review record: {recordPath}.",
                $"Verdict: {record.Verdict}.",
                $"Effective head: {facts.EffectiveHead}.",
                RecordCommitLine(facts) + ".",
            ]);
    }

    /// <summary>
    /// Names the commit that last changed the review record. One shared account cannot prove
    /// the reviewer (F-9), so the owner reads this line and sees a commit of another source.
    /// </summary>
    private static string RecordCommitLine(ReviewGateFacts facts)
    {
        if (facts.ReviewRecordCommit is null)
        {
            return "Last change of the record: no commit";
        }

        return $"Last change of the record: {facts.ReviewRecordCommit.Sha} \"{facts.ReviewRecordCommit.Subject}\"";
    }

    private static ReviewGateResult Fail(string rule, string expected, string found)
    {
        return new ReviewGateResult(false, "the review gate fails", [$"Rule: {rule}.", $"Expected: {expected}.", $"Found: {found}."]);
    }
}
