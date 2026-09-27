# PR-4 review

Date: 2026-09-27

## Identity

- PR: 4
- Target: `main`
- Base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`
- Merge base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`
- Head: `2551eb31c274db277159c779a5e98b7bc67e4c51`
- Branch: `feat/pr-3-codex-review`

## Provider gate

Claude Code authored the substantive change. The newest handoff entry for `feat/pr-3-codex-review` names Claude Code as author. Codex is the other provider and can review this PR under T-4 and D-6.

## Intended behavior and scope

PR-3 adds the `codex-review` command, its tests, and its review skills. This repeat pass read the prior review and response, the full correction diff, the affected launcher and command paths, the launcher tests, the PR description, the PR-3 exit tests, the relevant decisions and questions, and the review contracts. It checked the provider gate, the Windows CLI launch correction, the process argument and environment paths, and the effective and work heads. Unreal code, content, binary assets, and engine budgets do not apply because the PR changes none of them.

## Findings

### P2-1: Windows launch correction lacks Windows run evidence

Status: open.

Open at: `b0596f5accb3c1e4d8ad5dc05ed668763c2447ed`, `2551eb31c274db277159c779a5e98b7bc67e4c51`.

File: `Makefile:10`, `IronAbsolution.Tools/CodexReview/CodexLauncher.cs:35-63`, `IronAbsolution.Tests/CodexLauncherTests.cs:29-48`.

Trigger: Run the launcher regression tests and `make codex-review PR=4` on Windows after the correction.

Expected: The entry script starts through `node`, the multi-line prompt reaches the CLI unchanged, and the review reaches the record judge on Windows (D-32, D-33, D-47).

Actual: The correction uses `npm root --global` and starts `bin/codex.js` through `node`. The new tests cover version output and a multi-line prompt. This review ran them on macOS only. No Windows run log or Windows end-to-end run is present, so the platform-specific regression check remains unverified.

Consequence: The review cannot confirm that the corrected process launch works on the required Windows platform.

Evidence: `make` at `2551eb31c274db277159c779a5e98b7bc67e4c51` passed on macOS with 232 tests, including the launcher tests. `gh pr checks 4` passed `ste-check`, `build, test, and format`, and `coverage report` at metadata tip `5561e3f3f808117234dfc193d9365f33d8241427`; those jobs run on Ubuntu. No Windows evidence was available in this checkout.

Correction: Run the focused launcher tests on Windows, then run `make codex-review PR=4` on Windows with a ChatGPT login. Record both results in the PR before the next review.

Regression check: Require the fake CLI to report its version and receive the multi-line prompt unchanged on Windows. Require the real review command to reach the record judge on Windows.

## Out of scope

None.

## PR comments

None. The PR has no issue comments or review threads.

## Description edits

None.

## Verification

- `make` at `2551eb31c274db277159c779a5e98b7bc67e4c51` on macOS: build passed with 0 warnings and 0 errors; 232 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- `gh pr checks 4`: all three jobs passed at published metadata tip `5561e3f3f808117234dfc193d9365f33d8241427`. The jobs cover the effective head because the later commit changes metadata only.
- Windows launcher regression and end-to-end review: not run. This checkout has no Windows runner. D-33 assigns the Windows run to the owner, and its result remains required evidence.
- Real `make codex-review PR=4`: this repeat review is the active run. The command judges this record after the metadata push.
- Push: the single metadata commit for this review and handoff is verified as the head of `origin/feat/pr-3-codex-review` with `gh pr view`.

## Open questions and accepted risks

OQ-16 remains open for the gitar plan. D-7 keeps gitar out of this PR. No other open question blocks this review.

## Earlier verdicts

- Head `b0596f5accb3c1e4d8ad5dc05ed668763c2447ed`: **Changes required.** P2-1 identified the Windows CLI launch failure.

## Verdict

**Blocked.** This review cannot close P2-1 until the Windows regression test and end-to-end command run have results. The local tests and all three published CI jobs pass, but none runs on Windows.
