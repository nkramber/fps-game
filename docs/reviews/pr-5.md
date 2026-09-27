# PR-5 review

Date: 2026-09-27

## Identity

- PR: 5
- Target: `main`
- Base: `6acc6a85798182781a6fc1193c6e37d90dd03020`
- Merge base: `6acc6a85798182781a6fc1193c6e37d90dd03020`
- Head: `b5993f4a8eed3353208619a50a1d6448d9434dfd`
- Branch: `feat/pr-4-doc-gate-rotation`

## Provider gate

Claude Code authored the substantive change. The newest handoff entry for `feat/pr-4-doc-gate-rotation` names Claude Code and Role: author. Codex is the other provider. The gate passes under T-4 and D-6.

## Intended behavior and scope

PR-4 adds the `doc-gate` command and workflow, and the `handoff-rotate` command and Makefile target. The review read the PR description and its Documents matrix, section 6 and the PR-4 entry of `docs/design.md`, D-56 to D-59, OQ-16, the prior PR-4 review and response, all changed paths, and the review comments. No focused roadmap file exists for PR-4. Section 7 of `docs/design.md` holds its exit tests. The review examined the workflow inputs and permissions, the Git facts, both commands and their callers, the rules, the tests, the Makefile, and the changed project documents. Unreal code, game content, binary assets, and game platform budgets do not apply because this PR changes none of them.

## Findings

### P2-1: An out-of-range session number crashes rotation

Status: open.

Open at: `b5993f4a8eed3353208619a50a1d6448d9434dfd`.

File: `IronAbsolution.Tools/HandoffRotate/HandoffRotateRules.cs:66`.

Trigger: Run `make handoff-rotate` when a handoff heading contains a numeric session value greater than `Int32.MaxValue`.

Expected: The command reports invalid handoff data with its file and session context, makes no file change, and exits 1 (T-2, D-40, D-58).

Actual: `int.Parse` throws `OverflowException`. `HandoffRotateCommand.Run` does not catch it, so the process terminates with exit 134 and an unhandled exception.

Consequence: A damaged or unexpected session heading aborts the session-end command without its promised contextual fault report.

Evidence: A focused run with `## Session 999999999999999999999999999999999999: bad` printed an unhandled `System.OverflowException` at `HandoffRotateRules.Parse`, line 66, and exited 134.

Correction: Validate the number with `int.TryParse` and raise a contextual `InvalidOperationException` when it is out of range. Keep both files unchanged on this fault.

Regression check: Add a command test with an out-of-range heading. Require exit 1, an error that names `docs/session-handoff.md` and the value, and unchanged handoff and archive files.

## Out of scope

None.

## PR comments

None. `gh pr view 5 --comments` and the GraphQL review-thread query returned no comments or threads.

## Description edits

None.

## Verification

- `git fetch origin`, `git status --short --branch`, base lookup, and merge-base lookup: the checkout is `review/pr-5` at `b5993f4a8eed3353208619a50a1d6448d9434dfd`; both base values are `6acc6a85798182781a6fc1193c6e37d90dd03020`.
- Effective-head lookup: `b5993f4a8eed3353208619a50a1d6448d9434dfd` is the only work commit outside the documents set.
- `make` on macOS at `b5993f4a8eed3353208619a50a1d6448d9434dfd`: build passed with 0 warnings and 0 errors; 269 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- Focused `handoff-rotate` probe on macOS: failed as described in P2-1 with process exit 134.
- `gh pr checks 5`: `build, test, and format`, `coverage report`, `doc-gate`, and `ste-check` passed at head `b5993f4a8eed3353208619a50a1d6448d9434dfd`.
- `cmp -s AGENTS.md CLAUDE.md`: identical.
- OQ-16: gitar remains outside this review under D-7. It does not block this PR.
- Push: this metadata commit is the head of `origin/feat/pr-4-doc-gate-rotation`, verified with `gh pr view`.

## Open questions and accepted risks

OQ-16 remains open for the gitar plan under D-7. It does not block this review.

## Earlier verdicts

None.

## Verdict

**Changes required.** P2-1 shows that an out-of-range session number crashes the rotation command instead of producing a contextual fault with exit 1. The required local and hosted checks pass, but this focused failure needs correction.
