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

Claude Code authored the substantive change. The newest author handoff for `feat/pr-3-codex-review` confirms this. Codex is the other provider and can review this PR under T-4 and D-6.

## Intended behavior and scope

PR-3 adds the `codex-review` command, its tests, and its review skills. This repeat pass read both earlier records and the author response, checked the change to the launcher and its tests, and reviewed the current documents-only changes. It checked the PR description, exit tests, provider gate, review command contracts, decisions, questions, and affected consumers. D-55 says the tools and review run on macOS and need no Windows run. Unreal code, content, binary assets, and engine budgets do not apply because the PR changes none of them.

## Findings

### P2-1: The Codex launcher used a missing Windows shim

Status: fixed in `2551eb31c274db277159c779a5e98b7bc67e4c51`.

Open at: `b0596f5accb3c1e4d8ad5dc05ed668763c2447ed`, `2551eb31c274db277159c779a5e98b7bc67e4c51`.

File: `Makefile:10`, `IronAbsolution.Tools/CodexReview/CodexLauncher.cs:35-63`, `IronAbsolution.Tests/CodexLauncherTests.cs:29-48`.

Trigger: Run `make codex-review PR=4` with the default Windows npm install path.

Expected: Start the installed CLI entry script through `node` and pass the multi-line prompt unchanged (D-47, T-2).

Actual: The earlier Makefile path named a shim that npm does not create on Windows. Commit `2551eb3` now resolves the package entry script under `npm root --global`, starts it through `node`, and routes each Codex call through that launcher.

Consequence: The old command could not start the review on Windows. D-55 later confirmed that the tools and review need to work on macOS alone, so Windows execution is not a required contract.

Evidence: `make` on macOS at the effective head passed with 232 tests, including `TheVersionComesFromTheEntryScript`, `AMultiLinePromptReachesTheCliUnchanged`, and `TheMakefileGivesTheEntryScriptOfTheNpmPackage`. These tests exercise the old failure: the fake CLI entry script must run, and its received prompt must preserve all lines. D-55 resolves the prior request for a Windows run.

Correction: Commit `2551eb3` uses the npm package entry script and the `node` launcher.

Regression check: `make` passes with all 232 tests on macOS. No Windows run is required by D-55.

## Out of scope

None.

## PR comments

None. The PR has no issue comments or review threads.

## Description edits

None.

## Verification

- Effective head: `2551eb31c274db277159c779a5e98b7bc67e4c51`. Commits after it change documents only, so D-49 keeps this effective head.
- `make` on macOS: build passed with 0 warnings and 0 errors; 232 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- `gh pr checks 4`: `ste-check`, `build, test, and format`, and `coverage report` all passed at published metadata tip `241375c5b9693ca4fdb316afec437ff07fcefb39`. The later commits change documents only.
- Windows launcher tests and review command: not run. D-55 says the development tools and review need no Windows run.
- PR comments and review threads: none, verified with `gh pr view 4 --comments` and the GitHub GraphQL thread query.
- Real `make codex-review PR=4`: this review is the active run. The command judges this record after the metadata push.
- Push: the metadata commit is the head of `origin/feat/pr-3-codex-review`, verified with `gh pr view`.

## Open questions and accepted risks

OQ-16 remains open for the gitar plan. D-7 keeps gitar out of this PR. It does not block this review.

## Earlier verdicts

- Head `b0596f5accb3c1e4d8ad5dc05ed668763c2447ed`: **Changes required.** P2-1 identified the missing Windows CLI shim.
- Head `2551eb31c274db277159c779a5e98b7bc67e4c51`: **Blocked.** The prior round asked for Windows evidence, which D-55 later made unnecessary for the tools and review.

## Verdict

**Ready for owner merge.** P2-1 is fixed by the launcher correction, its regression tests pass, and D-55 resolves the remaining platform evidence request. The required local and hosted checks pass for this effective head.
