using System;
using System.Collections.Generic;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>The exit code of `codex-review` for each outcome (D-14). A usage fault gives 1, as in every command (D-40).</summary>
public enum CodexReviewExit
{
    /// <summary>The review approves the effective head.</summary>
    Approve = 0,

    /// <summary>A fault of the run, the review, or the record.</summary>
    Fault = 1,

    /// <summary>The PR or the checkout does not meet the start conditions, and no review ran.</summary>
    Refused = 3,

    /// <summary>The verdict does not approve. The author answers the findings.</summary>
    ChangesRequired = 10,

    /// <summary>A P0 to P2 finding is open for the third round. The fix loop stops, and the owner decides.</summary>
    ThreeStrikes = 11,
}

/// <summary>The outcome of one review round, with the facts that the command prints.</summary>
/// <param name="Exit">The exit code of the outcome.</param>
/// <param name="Verdict">The verdict of the record, or `none`.</param>
/// <param name="OpenFindingIds">Each open finding, in the order of the record.</param>
/// <param name="StrikeFindingIds">Each finding that stops the fix loop.</param>
/// <param name="Message">What the outcome means, and what to do next.</param>
public sealed record ReviewOutcome(
    CodexReviewExit Exit,
    string Verdict,
    IReadOnlyList<string> OpenFindingIds,
    IReadOnlyList<string> StrikeFindingIds,
    string Message);

/// <summary>
/// Judges the review record that the round pushed. The record text and the effective head go
/// in, and one outcome comes out. Every fault names the file, the finding, and the value found (T-2).
/// </summary>
public static class ReviewOutcomeRules
{
    /// <summary>The round in which one finding is open for the third time stops the fix loop (D-14).</summary>
    public const int StrikeLimit = 3;

    /// <summary>P0 to P2 findings count. A P3 never blocks the merge, so it never drives the fix loop.</summary>
    public const int HighestCountedSeverity = 2;

    /// <summary>Judges one review record against the effective head.</summary>
    /// <param name="recordText">The text of the record on origin, or null when origin has no record.</param>
    /// <param name="reviewFile">The path of the record, for each message.</param>
    /// <param name="effectiveHead">The full hash of the effective head after the round.</param>
    /// <returns>The outcome.</returns>
    public static ReviewOutcome Judge(string? recordText, string reviewFile, string effectiveHead)
    {
        if (recordText is null)
        {
            return Fault($"The branch on origin has no '{reviewFile}'. The review pushed no record.");
        }

        ReviewRecord? record = ReviewRecord.TryParse(recordText, out string parseError);
        if (record is null)
        {
            return Fault($"'{reviewFile}': {parseError}");
        }

        if (!ReviewHeads.HeadMatches(record.RecordedHead, effectiveHead))
        {
            return Fault($"'{reviewFile}' records the head '{record.RecordedHead}', and the effective head is '{effectiveHead}' (D-49). The record is stale.");
        }

        IReadOnlyList<ReviewFinding> findings;
        try
        {
            findings = ReviewFindings.Parse(recordText);
        }
        catch (FormatException exception)
        {
            return Fault($"'{reviewFile}': {exception.Message}");
        }

        List<string> openIds = [];
        List<string> blockingIds = [];
        List<string> strikeIds = [];
        foreach (ReviewFinding finding in findings)
        {
            if (!finding.IsOpen)
            {
                continue;
            }

            string? openAtError = CheckOpenAt(finding, record.RecordedHead);
            if (openAtError is not null)
            {
                return Fault($"'{reviewFile}': {openAtError}");
            }

            openIds.Add(finding.Id);
            if (finding.Severity <= HighestCountedSeverity)
            {
                blockingIds.Add(finding.Id);
                if (finding.OpenAt.Count >= StrikeLimit)
                {
                    strikeIds.Add(finding.Id);
                }
            }
        }

        if (record.Verdict == ReviewRecord.ApprovedVerdict && blockingIds.Count > 0)
        {
            // An approval needs no blocking finding. A P2 with an owner disposition carries the
            // status "accepted risk", so an open P0 to P2 finding under an approval is a record
            // that contradicts itself.
            return new ReviewOutcome(
                CodexReviewExit.Fault,
                record.Verdict,
                openIds,
                strikeIds,
                $"'{reviewFile}' gives '{record.Verdict}' with the open blocking finding(s) {string.Join(", ", blockingIds)}. An approval needs no open P0 to P2 finding.");
        }

        if (record.Verdict == ReviewRecord.ApprovedVerdict)
        {
            return new ReviewOutcome(CodexReviewExit.Approve, record.Verdict, openIds, [], $"The review approves the effective head {effectiveHead}.");
        }

        if (strikeIds.Count > 0)
        {
            return new ReviewOutcome(
                CodexReviewExit.ThreeStrikes,
                record.Verdict,
                openIds,
                strikeIds,
                $"The review returned {string.Join(", ", strikeIds)} for round {StrikeLimit} or later. Stop the fix loop and ask the owner (D-14).");
        }

        return new ReviewOutcome(CodexReviewExit.ChangesRequired, record.Verdict, openIds, [], $"The verdict is '{record.Verdict}'. Answer the findings with the review-response skill.");
    }

    /// <summary>
    /// Checks that an open finding lists the head of this round, and lists each head one time.
    /// Otherwise the count of rounds is wrong in silence, so the rule gives an error that names the finding.
    /// </summary>
    private static string? CheckOpenAt(ReviewFinding finding, string recordedHead)
    {
        if (finding.OpenAt.Count == 0)
        {
            return $"the open finding {finding.Id} has no '{ReviewFindings.OpenAtPrefix}' line with a head in backticks (D-14).";
        }

        for (int first = 0; first < finding.OpenAt.Count; first++)
        {
            string head = finding.OpenAt[first];
            if (head.Length < ReviewHeads.ShortestHash)
            {
                return $"the finding {finding.Id} lists '{head}' in its '{ReviewFindings.OpenAtPrefix}' line, and a head has seven characters or more.";
            }

            for (int second = first + 1; second < finding.OpenAt.Count; second++)
            {
                if (SameCommit(head, finding.OpenAt[second]))
                {
                    return $"the finding {finding.Id} lists the head '{head}' two times in its '{ReviewFindings.OpenAtPrefix}' line. A round adds its head one time.";
                }
            }
        }

        foreach (string head in finding.OpenAt)
        {
            if (SameCommit(head, recordedHead))
            {
                return null;
            }
        }

        return $"the open finding {finding.Id} does not list the head of this review, '{recordedHead}', in its '{ReviewFindings.OpenAtPrefix}' line (D-14).";
    }

    /// <summary>Tells whether two hashes name the same commit: the shorter one is a prefix of the longer one.</summary>
    private static bool SameCommit(string first, string second)
    {
        string shorter = first.Length <= second.Length ? first : second;
        string longer = first.Length <= second.Length ? second : first;
        return longer.StartsWith(shorter, StringComparison.OrdinalIgnoreCase);
    }

    private static ReviewOutcome Fault(string message)
    {
        return new ReviewOutcome(CodexReviewExit.Fault, "none", [], [], message);
    }
}
