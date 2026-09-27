# Review records

Status: the index and the format of the review records. Started 2026-09-27. Written in ASD-STE100 (D-17).

This folder holds one review record for each PR, and the response file of the author when one exists. The format comes from what-you-carry (D-14). PR-3 moves this format into the pr-review skill with the `make codex-review` command. Until then, this file is the format.

## Names

| File | Holds |
|---|---|
| `pr-<number>.md` | the review record. The number is the GitHub PR number, not the roadmap id. |
| `pr-<number>-response.md` | the answer of the author to each finding |

Provider names are permitted here and in the author field of the session handoff. They are not permitted in a PR description or a GitHub comment (T-6).

## The review record

Use this skeleton. Keep the heading text and the order. On a repeat review, edit the same file, keep each finding id, and move the old verdict under `## Earlier verdicts`.

```markdown
# PR-<number> review

Date: <YYYY-MM-DD>

## Identity

- PR: <number>
- Target: `main`
- Base: `<sha>`
- Merge base: `<sha>`
- Head: `<effective head sha>`
- Branch: `<branch>`

## Provider gate

State the author provider, the source of that fact, and the reviewer provider. State the result against T-4 and D-6.

## Intended behavior and scope

State the intent, what the review inspected, and every affected contract. Name any area that the review did not inspect.

## Findings

One subsection for each finding, in severity order. Write "No finding." when the review found none.

### P<severity>-<index>: <the defect in a few words>

- Status: open | fixed in <sha> | refuted
- Open at: <each head where it stayed open>
- File: <path:line>
- Trigger: <the input or state>
- Expected: <the contract, with its D-# id>
- Actual: <the behavior and its result>
- Evidence: <command, trace, or file>
- Correction: <direction, and the regression check>

## Out of scope

One line for each concern that a later PR holds. Write "None." when there is none.

## PR comments

One line for each comment thread: the claim, the answer of the author, and what the review verified. Write "None." when there is none.

## Description edits

One line for each correction to the PR description, with the old and the new value. Write "None." when there is none.

## Verification

One line for each command or check, with its result. Name each check that did not run, and the reason.

## Open questions and accepted risks

Name each open OQ-# and each accepted risk with its D-# id.

## Earlier verdicts

The verdicts of earlier rounds, with their heads. Write "None." on the first round.

## Verdict

**<Blocked | Changes required | Ready for owner merge>.** This verdict applies to head `<sha>`.
```

## Severity

| Severity | Meaning |
|---|---|
| P0 | An immediate critical failure, for example data loss or a leaked secret. |
| P1 | A major correctness failure, or a failed required gate. Resolve before merge. |
| P2 | A concrete defect or a contract gap. Resolve before merge, or get an owner disposition. |
| P3 | An optional improvement. It does not block the merge. |

A concern needs one of two answers of yes to become a finding. Does the change break a contract that this PR names? Does an exit test of this PR fail? Else it goes under "Out of scope". A P0 to P2 finding that stays open in three rounds stops the loop, and the author asks the owner (D-14).

## The response file

```markdown
# PR-<number> response

### <finding id>
- Disposition: full | partial | no merit
- Evidence: <the reason, with references>
- Correction: <sha>
- Regression check: <the test that fails on the old code>
```
