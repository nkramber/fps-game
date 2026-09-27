# PR-4 review

Date: 2026-09-27

## Identity

- PR: 4
- Target: `main`
- Base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`
- Merge base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`
- Head: `b0596f5accb3c1e4d8ad5dc05ed668763c2447ed`
- Branch: `feat/pr-3-codex-review`

## Provider gate

Claude Code authored the substantive change, as the newest handoff entry for this branch states. Codex is the other provider and is eligible to review under T-4 and D-6.

## Intended behavior and scope

PR-4 adds the `codex-review` command, its tests, and the review skills. The review read the full diff, the PR-3 roadmap entry and exit tests, the affected decisions and questions, the PR description, the handoff, the command and tests, the Makefile, and the review contracts. It checked the provider gate, review startup and outcome rules, process handling, effective and work heads, review record parsing, documentation rules, and CI evidence. Unreal code and content, binary assets, and engine budgets do not apply because the PR changes none of them.

## Findings

### P2-1: The Make target cannot launch the Windows Codex shim

Status: open.

Open at: `b0596f5accb3c1e4d8ad5dc05ed668763c2447ed`.

File: `Makefile:8`, `IronAbsolution.Tools/CodexReview/ExternalProcess.cs:112-135`.

Trigger: Run `make codex-review PR=4` on Windows after npm installs the CLI globally.

Expected: The review command must start on Windows under G-12 and D-32.

Actual: The Makefile builds the executable path as `<npm-prefix>/bin/codex`. npm places Windows global command shims directly under the prefix and names them `codex.cmd` ([npm folder documentation](https://docs.npmjs.com/cli/v9/configuring-npm/folders/), [npm package executable documentation](https://docs.npmjs.com/cli/v11/configuring-npm/package-json/)). The C# process runner uses `UseShellExecute = false` and passes the path directly to `Process.Start`, so it cannot launch the Windows command shim ([Microsoft ProcessStartInfo documentation](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.useshellexecute?view=net-10.0)).

Consequence: The review command fails before it can run the version check or review on Windows.

Evidence: `Makefile:8` appends `/bin/codex`; `ExternalProcess.Start` directly starts `fileName` with shell execution disabled. The CI workflow runs only on Ubuntu, so its green checks do not exercise this path. `make` passed on macOS with 226 tests, format verification, and 0 ste-check findings.

Correction: Resolve the platform-specific npm executable and launch its Windows shim correctly. Add a Windows regression test that runs a fake CLI through the same resolution and launch path.

Regression check: Run the focused process-launch test on Windows and require the fake CLI to report its version. Then run `make codex-review PR=4` on Windows with a ChatGPT login and require the review to reach its record judge.

## Out of scope

None.

## PR comments

None. The PR has no issue comments or review threads.

## Description edits

None.

## Verification

- `make` at `2e55d1ffd90a009a60e77601470d006ab50d8be6` on macOS: build passed, 226 tests passed, format passed, and ste-check reported 0 findings.
- `gh pr checks 4`: `ste-check`, `build, test, and format`, and `coverage report` passed on the published tip `2e55d1ffd90a009a60e77601470d006ab50d8be6`.
- Windows process launch: not run. This checkout has no Windows runner; the direct path and shim mismatch are documented by the cited platform sources.
- Real `make codex-review PR=4`: this review is the real run started by the owner. The command judges the record after the metadata push.
- Push: the published metadata commit is the head of `origin/feat/pr-3-codex-review`, verified with `gh pr view`.

## Open questions and accepted risks

OQ-16 remains open for the gitar plan. The PR keeps the gitar check out under D-7, so it does not block this review.

## Earlier verdicts

None.

## Verdict

**Changes required.** P2-1 shows that the Make target cannot start the Codex CLI on Windows. The other-provider gate, local checks, and all three published CI checks pass for this revision.
