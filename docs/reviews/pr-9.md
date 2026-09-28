# PR-9 review

Date: 2026-09-27

## Identity

- PR: 9
- Target: `main`
- Base: `2133abdbd714b3ba2dddf916adc1aef2b693a120`
- Merge base: `2133abdbd714b3ba2dddf916adc1aef2b693a120`
- Head: `d57ed3312cb3479166ab48858287cdd862443cc2`
- Branch: `feat/pr-8-engine-toolchain`

## Provider gate

Claude Code authored the PR. The newest author-role handoff entry for `feat/pr-8-engine-toolchain` names Claude Code. Codex is the other provider. The provider gate passes under T-4 and D-6.

## Intended behavior and scope

PR-8 adds the Mac toolchain command, the Windows toolchain script, their tests, the setup runbook, and the engine evidence form. It also updates review-label wording. Round 1 inspected the full diff, the PR description, its issue comment and review threads, the PR-8 roadmap entry and exit tests, sections 6 and 7 of `docs/design.md`, D-28, D-30 to D-34, D-39, D-41 to D-45, D-55 to D-66, D-69, D-71 to D-83, OQ-16, OQ-22, changed files in context, and the review procedures.

The review traced the Mac command inputs, version parsing, output, exit codes, and missing-tool behavior. It traced the Windows script pins, installed toolset scan, engine version read, and error paths. It checked the changed review-gate messages and tests, the template matrix, roadmap status, register updates, machine-specific paths, and the equality of `AGENTS.md` and `CLAUDE.md`. Game code, content, binary assets, and engine build evidence do not apply because this PR adds no Unreal project. The Windows tool script could not run in this macOS environment.

## Findings

### P2-1: Invalid Windows engine version data stops the remaining checks

Status: open.

Open at: `d57ed3312cb3479166ab48858287cdd862443cc2`.

File: `scripts/toolchain-check.ps1:131-132`.

Trigger: The engine variable points to a folder with a present but malformed or unreadable `Engine/Build/Build.version` file.

Expected: The Windows check reports the Unreal Engine pin as failed with the file path and cause, then reports the remaining pins and exits with code 1 (T-2, D-72).

Actual: `Get-Content` or `ConvertFrom-Json` raises a terminating error under `$ErrorActionPreference = 'Stop'`. The script exits before it reports the Unreal Engine pin, checks Git LFS, or writes the total.

Consequence: The Windows toolchain report stops at invalid engine data and omits other pin results. The owner cannot use the promised per-pin report to diagnose the machine.

Evidence: At the reviewed head, lines 131-132 pipe the file directly into `ConvertFrom-Json` without a local error result. The preceding missing-file branch handles only an absent file. `ToolchainCheckCommandTests` covers malformed JSON for the Mac command, while its Windows-script checks only inspect script text. `make` passed on macOS, but no PowerShell runtime was available here.

Correction: Catch read and parse faults for this file. Report a failed Unreal Engine pin with the path and fault, then continue to the Git LFS check.

Regression check: Add a PowerShell test with a malformed version file and a missing version file. Each case must report the Unreal Engine pin, report Git LFS, write the final failure count, and exit 1.

### P2-2: Two PR-9 exit tests have the same number

Status: open.

Open at: `d57ed3312cb3479166ab48858287cdd862443cc2`.

File: `docs/roadmaps/phase-1-engine-proof.md:135-136`.

Trigger: A reader follows exit test 8 of PR-9 or reports its result by number.

Expected: Each required exit test has one unique number, so its evidence can be identified without ambiguity (D-70).

Actual: The new cache-path test and the existing hosted-tests check both use number 8.

Consequence: The roadmap gives two different required checks the same reference. A handoff or review that reports exit test 8 does not identify which check passed.

Evidence: The PR-9 exit-test list at the reviewed head has the cache check at line 135 and the hosted checks at line 136, both prefixed `8.`.

Correction: Number the hosted-tests check 9, and increase the following test numbers by one.

Regression check: Read the PR-9 exit-test list and confirm that it uses the unique sequence 1 through 11. Run `make ste-check` after the correction.

## Out of scope

The cache-location exit test runs in PR-9, which creates the Unreal project and starts the editor. It is outside PR-8's implementation scope (D-83).

## PR comments

- The owner comment "Exit tests 1 and 4" reports three Mac pins and five Windows pins as passing at `d57ed33`. It also says an earlier Windows run rejected a valid toolset because the script read the folder name. The review verified that `d57ed33` reads the `cl.exe` product version and the tests assert this path. The Windows pass remains owner-reported evidence because this review ran on macOS.
- The GraphQL review-thread query returned no threads.

## Description edits

None.

## Verification

- `git fetch origin`, `git status --short --branch`, and revision lookup: clean worktree on `review/pr-9`; base and merge base `2133abdbd714b3ba2dddf916adc1aef2b693a120`; PR branch tip before this review commit `223269d7a640a3724c3ab338b7158d0ec6e02d72`.
- Effective-head lookup: `d57ed3312cb3479166ab48858287cdd862443cc2` is the newest commit outside the documents set. The later tip `223269d7a640a3724c3ab338b7158d0ec6e02d72` changes documents alone.
- `make` on macOS at PR tip `223269d`: build passed with 0 warnings and 0 errors; 398 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- `gh pr checks 9` at PR tip `223269d`: `build, test, and format`, `coverage report`, `doc-gate`, and `ste-check` passed. `review-gate` failed because `docs/reviews/pr-9.md` was not on the PR head yet. The review record reports `Changes required`, so the gate must remain red until the author addresses the findings.
- `gh run view 36371243046 --job 108767889614 --log-failed`: confirmed that the only reported `review-gate` fault was the missing review record at head `223269d`.
- `gh pr view 9 --comments` and the GraphQL review-thread query: one owner issue comment, no review threads.
- `cmp -s AGENTS.md CLAUDE.md` and `git diff --check origin/main...HEAD`: passed.
- The Windows script did not run because this macOS environment has no PowerShell runtime. The owner posted its output at `d57ed33`; all five pins passed there.
- OQ-16 keeps gitar outside this review under D-7. It does not affect the reviewed behavior.
- Push: this metadata commit is the head of `origin/feat/pr-8-engine-toolchain`, verified with `gh pr view`.

## Open questions and accepted risks

OQ-16 remains open for gitar under D-7. It blocks only the gitar pass. No risk has an owner disposition.

## Earlier verdicts

None.

## Verdict

**Changes required.** This verdict applies to effective head `d57ed3312cb3479166ab48858287cdd862443cc2`. The Windows script does not contain malformed engine data as a failed pin, and the changed roadmap gives two required checks the same exit-test number.
