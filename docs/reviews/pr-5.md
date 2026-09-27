# PR-5 review

Date: 2026-09-27

## Identity

- PR: 5
- Target: `main`
- Base: `6acc6a85798182781a6fc1193c6e37d90dd03020`
- Merge base: `6acc6a85798182781a6fc1193c6e37d90dd03020`
- Head: `eb76650f9ae45b4e9a0b7b0479147f16b83bf261`
- Branch: `feat/pr-4-doc-gate-rotation`

## Provider gate

Claude Code authored the substantive changes, as the author handoff entries for this branch state. The response and handoff confirm that Claude Code made the fixes in `614847a` and `eb76650`. Codex is the other provider. The gate passes under T-4 and D-6.

## Intended behavior and scope

PR-4 adds the `doc-gate` command and workflow, and the `handoff-rotate` command and Makefile target. This round read the PR description, the response file, both earlier review rounds, all PR comments, the PR-4 entry and exit tests, design guardrails, D-42 and D-56 to D-59, OQ-16, the session runbook, and the complete change set. It rechecked the handoff parse and rotation, the documents gate inputs and permissions, the changed workflow, command errors, tests, documents, and the P2-2 boundary fix. No focused roadmap exists for PR-4. Unreal code, game content, binary assets, and game platform budgets do not apply because this PR changes none of them.

The round 2 fix checks the largest session number before it creates either output. Its regression test requires exit 1, a contextual error, and unchanged files. No blocking finding remains at effective head `eb76650`.

## Findings

### P2-1: An out-of-range session number crashes rotation

Status: fixed in `614847a4ab3538b89e907f12552d1847cfb6a846`.

Open at: `b5993f4a8eed3353208619a50a1d6448d9434dfd`.

File: `IronAbsolution.Tools/HandoffRotate/HandoffRotateRules.cs:66`.

Trigger: Run `make handoff-rotate` when a handoff heading contains a number greater than `Int32.MaxValue`.

Expected: The command reports invalid handoff data with file and session context, changes no file, and exits 1 (T-2, D-40, D-58).

Actual: The initial parse threw an unhandled `OverflowException` and exited 134. Commit `614847a` changed the parse to `int.TryParse` and added contextual faults in both commands.

Consequence: The session-end command stopped without its required contextual fault report.

Evidence: The trigger reproduced in round 1. In round 2, both `HandoffRotateTests.ASessionNumberTooLargeForAnIntIsAFaultThatChangesNoFile` and `DocGateTests.ASessionNumberTooLargeForAnIntIsAFaultOfTheCommand` passed in `make`.

Correction: Parse each heading with `int.TryParse` and raise a contextual fault when its value exceeds the supported range. Do not change either file on that fault.

Regression check: The two named tests require exit 1 and the file and value in the error. The rotation test also requires both files to stay unchanged. Both pass in `make` at `eb76650`.

### P2-2: The next session number wraps at the largest accepted value

Status: fixed in `eb76650f9ae45b4e9a0b7b0479147f16b83bf261`.

Open at: `614847a4ab3538b89e907f12552d1847cfb6a846`.

File: `IronAbsolution.Tools/HandoffRotate/HandoffRotateRules.cs:110`.

Trigger: Run `make handoff-rotate` when the newest heading is `## Session 2147483647:`.

Expected: The command reports a contextual fault and changes no file when it cannot represent the next session number (T-2, D-40, D-58).

Actual: At `614847a`, unchecked addition wrapped to `-2147483648`; the command exited 0 and printed the invalid number. At `eb76650`, `Rotate` rejects `int.MaxValue` before either file write.

Consequence: Without the fix, the session-end instructions could assign a negative session number after a successful command.

Evidence: The trigger reproduced in round 2. `HandoffRotateTests.TheLargestSessionNumberIsAFaultThatChangesNoFile` requires exit 1, the handoff path and value in the error, and unchanged handoff and archive files. The test passes in the 272-test `make` run at `eb76650`.

Correction: Check the increment before rotation writes either file. Report the handoff path and maximum value, and exit 1 without changing either file when no positive next number can be represented.

Regression check: `HandoffRotateTests.TheLargestSessionNumberIsAFaultThatChangesNoFile` passes at `eb76650` and fails when the new check is absent.

## Out of scope

The separate session-number parser in `ste-check` remains assigned to the PR-2 follow-up named in the author handoff. This PR does not change that command.

## PR comments

None. The issue-comment export and GraphQL review-thread query returned no comments or threads.

## Description edits

None.

## Verification

- `git fetch origin`, `git status --short --branch`, and revision lookup: branch `review/pr-5`; base and merge base `6acc6a85798182781a6fc1193c6e37d90dd03020`; current PR tip before this metadata commit `b964463dd7d63d1752c80e26b20ff426b6dc40dd`.
- Effective-head lookup: `eb76650f9ae45b4e9a0b7b0479147f16b83bf261` is the newest commit outside the documents set. Later commits change documents alone.
- `make` on macOS at `eb76650`: build passed with 0 warnings and 0 errors; 272 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- `gh pr checks 5`: `build, test, and format`, `coverage report`, `doc-gate`, and `ste-check` passed on the current PR revision. The current branch tip changes only review and handoff metadata after the effective head.
- `git diff --check origin/main...HEAD`: passed.
- `cmp -s AGENTS.md CLAUDE.md`: passed; the files are identical.
- OQ-16: gitar remains outside this review under D-7. It does not block this PR.
- Push: this record and the handoff entry are one metadata commit. `gh pr view` will verify the pushed head.

## Open questions and accepted risks

OQ-16 remains open for the gitar plan under D-7. It does not block this review.

## Earlier verdicts

- Round 1 at `b5993f4a8eed3353208619a50a1d6448d9434dfd`: **Changes required**. P2-1 showed that an out-of-range session number crashed the rotation command.
- Round 2 at `614847a4ab3538b89e907f12552d1847cfb6a846`: **Changes required**. P2-2 showed that the largest accepted session number produced a negative next number.

## Verdict

**Ready for owner merge.** P2-1 and P2-2 pass their regression checks at effective head `eb76650`. The local build, tests, format, document checks, and hosted checks pass.
