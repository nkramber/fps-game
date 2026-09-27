# PR-7 review

Date: 2026-09-27

## Identity

- PR: 7
- Target: `main`
- Base: `5871f2c870d5eb110eccbadd67c5f633c5619155`
- Merge base: `5871f2c870d5eb110eccbadd67c5f633c5619155`
- Head: `7f4cb583772c28e2bdb2a1a32c8f2e921442a88c`
- Branch: `feat/pr-6-review-gate`

## Provider gate

Claude Code authored the PR. The newest handoff entry for `feat/pr-6-review-gate` names Claude Code as author and the role as author. Codex is the other provider. The provider gate passes under T-4 and D-6.

## Intended behavior and scope

PR-6 adds the `review-gate` command and workflow, the `review-override` label path, tests, and their documents. This review inspected the complete diff, the PR description, the PR comments and review threads, the PR-6 roadmap entry and exit tests, sections 6 to 8 of `docs/design.md`, D-35, D-49, and D-64 to D-67, OQ-16, the changed files in context, and the review and session procedures. No focused roadmap exists for phase 0. Unreal code, game content, binary assets, engine budgets, and Windows build logs do not apply because this PR changes none of them.

The review traced the base-branch trust boundary, Git reads, effective-head and work-head calculations, record parsing, label facts, decision rules, workflow permissions and triggers, and the tests. It also checked the PR document matrix and the consistency of `AGENTS.md` and `CLAUDE.md`.

## Findings

### P2-1: A backdated document commit passes the override label check

Status: open.

Open at: `7f4cb583772c28e2bdb2a1a32c8f2e921442a88c`.

File: `IronAbsolution.Tools/ReviewGate/ReviewGateRules.cs:79`.

Trigger: The owner adds `review-override`, then a PR author pushes a document commit whose Git committer timestamp is earlier than the label event.

Expected: D-65 requires a new label event after a work commit. The owner must see and cover each document change with the label.

Actual: The rule compares the label event time with the committer timestamp from the Git commit. A PR author controls that timestamp, so an after-label commit can appear older and pass.

Consequence: The PR can change documents after the owner labels it, while the required check still passes without another owner review.

Evidence: A temporary repository had a `docs/design.md` commit after a label event at `2026-09-27T12:00:00Z`, with a committer timestamp of `2020-01-02T00:00:00Z`. The command passed and printed the override result. `GitRepository.CommitTime` reads `%cI`, which comes from the commit object.

Correction: Base freshness on trusted event order, or require a fresh label after each PR head update. Do not use a PR-controlled Git timestamp to decide whether a commit followed the label.

Regression check: Add a command-level test that creates a document commit after the label event with an earlier committer timestamp. The gate must fail and require the owner to add the label again.

## Out of scope

The GitHub proof of exit tests 1 and 2 runs on the first PR after PR-6 reaches `main` (D-67). This PR cannot run its own `pull_request_target` workflow from its head.

## PR comments

None. `gh pr view 7 --comments` returned no issue comments. The GraphQL review-thread query returned no threads.

## Description edits

None.

## Verification

- `git fetch origin`, `git status --short --branch`, and revision lookup: clean worktree on `review/pr-7`; base and merge base `5871f2c870d5eb110eccbadd67c5f633c5619155`; PR branch tip `7f4cb583772c28e2bdb2a1a32c8f2e921442a88c`.
- Effective-head lookup: `7f4cb583772c28e2bdb2a1a32c8f2e921442a88c` is the newest commit outside the documents set.
- `make` at PR head `7f4cb583`: build passed with 0 warnings and 0 errors; 363 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- `gh pr checks 7`: `build, test, and format`, `coverage report`, `doc-gate`, and `ste-check` passed on the PR head.
- `git diff --check origin/main...HEAD` and `cmp -s AGENTS.md CLAUDE.md`: passed.
- `review-gate` has no check on this PR. D-67 says GitHub runs this workflow only after it reaches `main`; the PR description also records this limit.
- Focused override-label reproduction: failed to reject a document commit pushed after the label when its committer timestamp was backdated; the command passed unexpectedly.
- OQ-16 keeps gitar outside this review under D-7. It does not affect the reviewed behavior.
- Push: the review record and handoff will be published together on `origin/feat/pr-6-review-gate`; `gh pr view 7` will verify the remote head.

## Open questions and accepted risks

OQ-16 remains open for gitar under D-7. It does not block this review. No accepted risk applies.

## Earlier verdicts

None.

## Verdict

**Changes required.** The override-label freshness check trusts a PR-controlled commit timestamp and accepts document changes made after the owner's label. Fix P2-1 and prove the backdated-commit case fails.
