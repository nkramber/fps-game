# PR-5 review

Date: 2026-09-27

## Identity

- PR: 5
- Target: `main`
- Base: `6acc6a85798182781a6fc1193c6e37d90dd03020`
- Merge base: `6acc6a85798182781a6fc1193c6e37d90dd03020`
- Head: `614847a4ab3538b89e907f12552d1847cfb6a846`
- Branch: `feat/pr-4-doc-gate-rotation`

## Provider gate

Claude Code authored the substantive change. The newest author handoff entry for this branch names Claude Code and Role: author. Codex is the other provider. The gate passes under T-4 and D-6.

## Intended behavior and scope

PR-4 adds the `doc-gate` command and workflow, and the `handoff-rotate` command and Makefile target. This repeat review read the PR description, the earlier review and response, the PR comments, the PR-4 entry and exit tests, section 6 of `docs/design.md`, D-56 to D-59, and OQ-16. It inspected all changed files: the workflow, pull request template, agent and skill instructions, commands, rules, Git integration, tests, Makefile, and project documents. It checked the `doc-gate` inputs and permissions, its diff and commit reads, the handoff parse and rotation, and the error paths. No focused roadmap exists for PR-4. Unreal code, game content, binary assets, and game platform budgets do not apply because this PR changes none of them.

The prior P2-1 trigger now returns a contextual fault in both commands. Its two regression tests cover the exit code and error. A separate boundary probe found that the largest accepted session number wraps when the command adds one.

## Findings

### P2-1: An out-of-range session number crashes rotation

Status: fixed in `614847a4ab3538b89e907f12552d1847cfb6a846`.

Open at: `b5993f4a8eed3353208619a50a1d6448d9434dfd`.

File: `IronAbsolution.Tools/HandoffRotate/HandoffRotateRules.cs:66`.

Trigger: Run `make handoff-rotate` when a handoff heading contains a numeric session value greater than `Int32.MaxValue`.

Expected: The command reports invalid handoff data with its file and session context, makes no file change, and exits 1 (T-2, D-40, D-58).

Actual: `int.Parse` threw an unhandled `OverflowException`, and the process exited 134.

Consequence: A damaged or unexpected session heading aborted the session-end command without a contextual fault report.

Evidence: The trigger reproduced in review round 1. In this round, `HandoffRotateTests.ASessionNumberTooLargeForAnIntIsAFaultThatChangesNoFile` and `DocGateTests.ASessionNumberTooLargeForAnIntIsAFaultOfTheCommand` pass in the full test run. The command now uses `int.TryParse` and reports the file and value.

Correction: Parse each heading with `int.TryParse` and raise a contextual fault when its value exceeds the supported range. Do not change either file on that fault.

Regression check: Both named tests require exit 1 and the path and value in the error. The rotation test also requires both files to stay unchanged.

### P2-2: The next session number wraps at the largest accepted value

Status: open.

Open at: `614847a4ab3538b89e907f12552d1847cfb6a846`.

File: `IronAbsolution.Tools/HandoffRotate/HandoffRotateRules.cs:110`.

Trigger: Run `make handoff-rotate` when the newest heading is `## Session 2147483647:`.

Expected: The command reports a contextual fault and changes no file, or represents the next session number without overflow. The command promises the highest session number plus one (D-58), and T-2 forbids a silent invalid result.

Actual: The unchecked addition wraps to `-2147483648`. The command exits 0 and reports that value as the next session number.

Consequence: The session end instructions can assign an invalid negative session number after a successful command.

Evidence: A direct run of the built tool on 11 ordered entries, with the newest numbered `2147483647`, exited 0 and printed `The next session number is -2147483648.`

Correction: Check the increment before rotation writes either file. Report the handoff path and maximum value, and exit 1 without changing either file when no positive next number can be represented.

Regression check: Add a command test for a newest entry numbered `2147483647`. Require exit 1, a contextual error, and unchanged handoff and archive files.

## Out of scope

The `ste-check` session number parser has a separate `int.Parse` path. The author handoff assigns that PR-2 issue to a fresh session. This review does not widen P2-1 to that separate command.

## PR comments

None. `gh pr view 5 --comments` and the GraphQL review-thread query returned no comments or threads.

## Description edits

- Changed the test count from 37 new and 269 total to 39 new and 271 total. `make` verified 271 passing tests at the reviewed head.

## Verification

- `git fetch origin`, `git status --short --branch`, base lookup, and merge-base lookup: the review checkout is `review/pr-5`; base and merge base are `6acc6a85798182781a6fc1193c6e37d90dd03020`.
- Effective-head lookup: `614847a4ab3538b89e907f12552d1847cfb6a846` is the newest commit outside the documents set. Later commits change documents alone.
- `make` on macOS at effective head `614847a4ab3538b89e907f12552d1847cfb6a846`: build passed with 0 warnings and 0 errors; 271 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- `dotnet test IronAbsolution.Tests/IronAbsolution.Tests.csproj --no-build --filter ...`: this filter invocation did not run under the repository's Microsoft Testing Platform setup. The full test suite ran through `make`.
- Direct boundary probe with the built tool: newest session `2147483647`; exit 0; output incorrectly reported next session `-2147483648` (P2-2).
- `gh pr checks 5`: `build, test, and format`, `coverage report`, `doc-gate`, and `ste-check` passed at the effective head. The `doc-gate` check also passed after the verified test-count edit to the PR description.
- `cmp -s AGENTS.md CLAUDE.md`: identical.
- OQ-16: gitar remains outside this review under D-7. It does not block this PR.
- Push: the review record and handoff entry share one metadata commit on `origin/feat/pr-4-doc-gate-rotation`; `gh pr view` verified the pushed head.

## Open questions and accepted risks

OQ-16 remains open for the gitar plan under D-7. It does not block this review.

## Earlier verdicts

- Round 1 at `b5993f4a8eed3353208619a50a1d6448d9434dfd`: **Changes required**. P2-1 showed that an out-of-range session number crashed the rotation command.

## Verdict

**Changes required.** P2-1 is fixed at the reviewed head. P2-2 shows that the command reports a negative next session number for a valid `Int32.MaxValue` heading, so this revision needs a boundary correction.
