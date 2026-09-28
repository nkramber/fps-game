# PR-9 review

Date: 2026-09-27

## Identity

- PR: 9
- Target: `main`
- Base: `2133abdbd714b3ba2dddf916adc1aef2b693a120`
- Merge base: `2133abdbd714b3ba2dddf916adc1aef2b693a120`
- Head: `bc930a946d214c805ac5eefc77a31d4599fc5625`
- Branch: `feat/pr-8-engine-toolchain`

## Provider gate

Claude Code authored the PR. The author-role handoff for `feat/pr-8-engine-toolchain` names Claude Code. Codex is the other provider. The gate passes under T-4 and D-6.

## Intended behavior and scope

PR-8 adds the Mac toolchain command, the Windows toolchain script, their tests, the setup runbook, and the engine evidence form. It also updates review-label wording. Round 1 inspected the full PR diff and contracts. This round inspected the response, the full diff since `d57ed3312cb3479166ab48858287cdd862443cc2`, the changed PowerShell script and tests, the exit tests, the PR description, the owner comment, the review threads, and the prior findings. The review checked T-1 to T-6, G-6 to G-8, F-3, F-20 to F-22, D-28, D-30, D-49, D-70 to D-83, and OQ-16 and OQ-22.

Round 1 traced the Mac command, the Windows pins, the evidence form, the runbook, the document updates, and the affected review-gate text. The Windows script ran this round under portable PowerShell. Unreal project code, content, binary assets, and engine builds do not apply because PR-8 adds no Unreal project.

## Findings

### P2-1: Invalid Windows engine version data stops the remaining checks

Status: fixed in `bc930a946d214c805ac5eefc77a31d4599fc5625`.

Open at: `d57ed3312cb3479166ab48858287cdd862443cc2`.

File: `scripts/toolchain-check.ps1:131-132` at the first review.

Trigger: The engine variable points to a folder with a present but malformed or unreadable `Engine/Build/Build.version` file.

Expected: The Windows check reports the Unreal Engine pin as failed with the file path and cause, then reports the remaining pins and exits with code 1 (T-2, D-72).

Actual: At the first review, a read or parse error stopped the script before later pins. The revised script catches read and parse faults, reports the failed pin, and continues.

Consequence: The first revision omitted later pin results. The correction keeps the full report available after an invalid engine file.

Evidence: `Get-EngineVersion` in the current script reports malformed JSON and missing or invalid fields with the file path. `ToolchainScriptTests` exercises four malformed files and an absent file. Each test asserts the Unreal Engine line, Git LFS line, failure total, and exit code. The response reports that all seven cases fail on `d57ed33` and pass on the correction.

Correction: The fix catches source faults per pin, reports each error, and continues to later checks.

Regression check: `PATH='/tmp/claude-501/-Volumes-SSD-1TB-iron-absolution/b5561f08-4b7e-53f5-8194-dfc664d50700/scratchpad/pwsh':$PATH make` passed, including the PowerShell tests.

### P2-2: Two PR-9 exit tests have the same number

Status: fixed in `bc930a946d214c805ac5eefc77a31d4599fc5625`.

Open at: `d57ed3312cb3479166ab48858287cdd862443cc2`.

File: `docs/roadmaps/phase-1-engine-proof.md:135-136` at the first review.

Trigger: A reader follows exit test 8 of PR-9 or reports its result by number.

Expected: Each required exit test has one unique number, so its evidence can be identified without ambiguity (D-70).

Actual: The first revision gave both the cache check and the hosted-tests check number 8. The revised list uses the sequence 1 through 11.

Consequence: The first revision gave two checks the same reference. The correction gives each check a unique reference and preserves exit test 8 for the cache check (D-83).

Evidence: The PR-9 list in `docs/roadmaps/phase-1-engine-proof.md` now uses unique numbers 1 through 11. `make ste-check` reports no findings.

Correction: The hosted-tests check is number 9, and the following two checks are numbers 10 and 11.

Regression check: The list uses unique numbers 1 through 11. `make ste-check` passed.

## Out of scope

The cache-location exit test runs in PR-9, which creates the Unreal project and starts the editor (D-83).

## PR comments

- The owner comment "Exit tests 1 and 4" reports three Mac pins and five Windows pins as passing at `d57ed33`. It also reports that the earlier script rejected a valid MSVC toolset because it read the folder name. The review verified that the current script reads the `cl.exe` product version. The Windows result remains owner-reported evidence.
- The GraphQL review-thread query returned no threads.

## Description edits

None.

## Verification

- `git fetch origin`, `git status --short --branch`, and revision lookup: clean worktree on `review/pr-9`; base and merge base are `2133abdbd714b3ba2dddf916adc1aef2b693a120`.
- Effective-head lookup: `bc930a946d214c805ac5eefc77a31d4599fc5625` is the newest commit outside the documents set.
- `make` with portable PowerShell on `PATH`: build passed with 0 warnings and 0 errors; 405 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- `gh pr checks 9` before this review record: build, test, and format; coverage report; doc-gate; and ste-check passed. `review-gate` failed because the record still held the earlier verdict. The check must run again after this metadata commit.
- PR comment export: one owner issue comment and no review threads.
- `cmp -s AGENTS.md CLAUDE.md` and `git diff --check origin/main...HEAD`: passed in round 1. The current diff also passes `git diff --check`.
- OQ-16 keeps gitar outside this review under D-7. The Windows owner run stays owner-reported. No other required check waits for evidence.
- Push: the review and handoff metadata commit must reach `origin/feat/pr-8-engine-toolchain` and pass `gh pr view` verification.

## Open questions and accepted risks

OQ-16 remains open for gitar under D-7. It blocks only the gitar pass. No risk has an owner disposition.

## Earlier verdicts

- Round 1 at `d57ed3312cb3479166ab48858287cdd862443cc2`: Changes required.

## Verdict

**Ready for owner merge.** This verdict applies to effective head `bc930a946d214c805ac5eefc77a31d4599fc5625`. Both earlier findings pass their stated regression checks, and the local verification passes. The published `review-gate` result must be read after this metadata commit.
