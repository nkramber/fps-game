# PR-7 review

Date: 2026-09-27

## Identity

- PR: 7
- Target: `main`
- Base: `5871f2c870d5eb110eccbadd67c5f633c5619155`
- Merge base: `5871f2c870d5eb110eccbadd67c5f633c5619155`
- Head: `2338304d6a448277f96270caaf90ee7213df6106`
- Branch: `feat/pr-6-review-gate`

## Provider gate

Claude Code authored the PR. The newest author-role handoff entry for `feat/pr-6-review-gate` names Claude Code. Codex is the other provider. The provider gate passes under T-4 and D-6.

## Intended behavior and scope

PR-6 adds the `review-gate` command and workflow, the `review-override` label path, tests, and their documents. Round 1 inspected the complete diff, the PR description, comments and review threads, the PR-6 roadmap entry and exit tests, sections 6 to 8 of `docs/design.md`, D-35, D-49, D-64 to D-67, OQ-16, changed files in context, and the review procedures. No focused roadmap exists for phase 0. Unreal code, game content, binary assets, engine budgets, and Windows build logs do not apply because this PR changes none of them.

Round 2 read the author response and verified D-68 and F-18. Commit `2338304` adds only a C# comment and changes decision, design, response, and handoff documents. It does not change executable behavior. The finding trigger remains true and the owner accepted that risk. The review rechecked the provider gate, full PR comments and threads, the effective head, the override rule and its tests, the PR checks, and the complete local gate.

The review traced the base-branch trust boundary, Git reads, effective-head and work-head calculations, record parsing, label facts, decision rules, workflow permissions and triggers, and tests. It checked the PR document matrix and the consistency of `AGENTS.md` and `CLAUDE.md`.

## Findings

### P2-1: A backdated document commit passes the override label check

Status: accepted risk, D-68.

Open at: `7f4cb583772c28e2bdb2a1a32c8f2e921442a88c`.

File: `IronAbsolution.Tools/ReviewGate/ReviewGateRules.cs:79`.

Trigger: The owner adds `review-override`, then a PR author pushes a document commit whose Git committer timestamp is earlier than the label event.

Expected: D-65 requires a new label event after a work commit. The owner must see and cover each document change with the label.

Actual: The rule compares the label event time with the committer timestamp from the Git commit. A PR author controls that timestamp, so an after-label commit can appear older and pass.

Consequence: The PR can change documents after the owner labels it, while the required check still passes without another owner review.

Evidence: Round 1 reproduced this with a temporary repository. A document commit after a label event at `2026-09-27T12:00:00Z`, with committer time `2020-01-02T00:00:00Z`, passed. `GitRepository.CommitTime` reads `%cI` from the commit object. The author response confirms the trigger. D-68 and F-18 record the owner's accepted risk: one GitHub account can add the label again (F-9), and the time rule stops accidents rather than attacks.

Correction: None required under the owner's disposition in D-68. The rule cites D-68 and F-9.

Regression check: `dotnet test --solution IronAbsolution.slnx --no-build --filter-class IronAbsolution.Tests.ReviewGateCommandTests` passed 17 tests. The existing before-label and after-label tests pass. The backdated-commit trigger remains accepted under D-68.

## Out of scope

The GitHub proof of exit tests 1 and 2 runs on the first PR after PR-6 reaches `main` (D-67). PR-7 cannot run its own `pull_request_target` workflow from its head.

## PR comments

None. `gh pr view 7 --comments` returned no issue comments. The GraphQL review-thread query returned no threads.

## Description edits

None.

## Verification

- `git fetch origin`, `git status --short --branch`, and revision lookup: clean worktree on `review/pr-7`; base and merge base `5871f2c870d5eb110eccbadd67c5f633c5619155`; PR branch tip before this review commit `13e4f6bb22747b7289639dee06ab74f3cf5f4c88`.
- Effective-head lookup: `2338304d6a448277f96270caaf90ee7213df6106` is the newest commit outside the documents set.
- `git diff 7f4cb583772c28e2bdb2a1a32c8f2e921442a88c..HEAD -- IronAbsolution.Tools/ReviewGate/ReviewGateRules.cs`: only two explanatory comment lines changed; no behavior changed.
- `make` on macOS at `13e4f6b`: build passed with 0 warnings and 0 errors; 363 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- `dotnet test --solution IronAbsolution.slnx --no-build --filter-class IronAbsolution.Tests.ReviewGateCommandTests`: 17 passed, 0 failed, and 0 skipped.
- Initial focused command with `-- --filter-class '*ReviewGateCommandTests'`: the runner returned `No test projects were found`; no test result came from that invocation.
- `gh pr checks 7`: `build, test, and format`, `coverage report`, `doc-gate`, and `ste-check` passed on PR tip `13e4f6bb22747b7289639dee06ab74f3cf5f4c88`.
- `review-gate` has no check on PR-7. D-67 says GitHub runs this workflow only after it reaches `main`.
- Local `review-gate` command on the committed record at `0b0ff00548896af067a88e2da64541b67d97a14e`: passed for PR #7 and approved effective head `2338304d6a448277f96270caaf90ee7213df6106`.
- `git diff --check origin/main...HEAD` and `cmp -s AGENTS.md CLAUDE.md`: passed.
- `gh pr view 7 --comments` and GraphQL review-thread query: no comments or threads.
- OQ-16 keeps gitar outside this review under D-7. It does not affect the reviewed behavior.
- Push: this metadata commit is the head of `origin/feat/pr-6-review-gate`, verified with `gh pr view`.

## Open questions and accepted risks

OQ-16 remains open for gitar under D-7. It does not block this review. P2-1 is an accepted risk under D-68.

## Earlier verdicts

Round 1 at head `7f4cb583772c28e2bdb2a1a32c8f2e921442a88c`: **Changes required**. P2-1 was open.

## Verdict

**Ready for owner merge.** This verdict applies to effective head `2338304d6a448277f96270caaf90ee7213df6106`. The provider gate, local gates, focused tests, and available hosted checks pass. P2-1 is resolved by the owner's accepted-risk decision D-68.
