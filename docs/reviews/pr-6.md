# PR-6 review

Date: 2026-09-27

## Identity

- PR: 6
- Target: `main`
- Base: `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`
- Merge base: `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`
- Head: `2d0f013af92bd8c2d634e4d87383de9fed3fc96a`
- Branch: `feat/pr-5-main-ruleset`

## Provider gate

Claude Code authored the PR, as the newest handoff entry for `feat/pr-5-main-ruleset` states. Codex is the other provider. The provider gate passes under T-4 and D-6.

## Intended behavior and scope

PR-5 adds the ruleset of `main`, its tests, and its setup runbook. This review read the full diff, the PR description and comments, the PR-5 roadmap entry and exit tests, sections 6 to 8 of `docs/design.md`, D-60 to D-63, OQ-16, the changed files in context, and the review and session procedures. No focused roadmap exists for phase 0. Unreal code, game content, binary assets, and game platform budgets do not apply because this PR changes none of them. The review checked the ruleset fields, required-check bindings, bypass, runbook commands, documents, and CI evidence.

## Findings

### P2-1: The ruleset test allows a required check to disappear

Status: open.

Open at: `2d0f013af92bd8c2d634e4d87383de9fed3fc96a`.

File: `IronAbsolution.Tests/RulesetTests.cs:38-45`.

Trigger: Remove one required check and its matching job from the PR workflows.

Expected: The ruleset test requires the four checks named by D-61, and it binds each one to exactly one PR job.

Actual: The test checks only that the list is nonempty and unique. The other tests derive both sides from the ruleset and current workflows. If both lose the same check, all checks can pass.

Consequence: A required gate such as `doc-gate` can disappear from the ruleset and workflows without a test failure. A PR can then merge without that gate.

Evidence: At this head, `TheRulesetRequiresEachCheckOnceFromGitHubActions`, `EachRequiredCheckIsTheNameOfOnePullRequestJob`, and `EachPullRequestJobIsRequired` contain no assertion for the four fixed names. D-61 names those four checks.

Correction: Assert the exact four check names from D-61 before testing their job bindings.

Regression check: Run `make`. The ruleset tests must fail if any named check is removed from both the file and its workflow, and pass with all four checks present.

## Out of scope

The `review-gate` check and its addition to the ruleset belong to PR-6, as the PR-5 roadmap states. The auto-merge setting also belongs to PR-6.

## PR comments

None. The issue-comment export and GraphQL review-thread query returned no comments or threads.

## Description edits

None.

## Verification

- `git fetch origin`, `git status --short --branch`, and revision lookup: clean worktree on `review/pr-6`; base and merge base `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`; PR head `2d0f013af92bd8c2d634e4d87383de9fed3fc96a`.
- Effective-head lookup: `2d0f013af92bd8c2d634e4d87383de9fed3fc96a` is the newest commit outside the documents set.
- `make` at `2d0f013`: build passed with 0 warnings and 0 errors; 287 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- `gh pr checks 6`: `build, test, and format`, `coverage report`, `doc-gate`, and `ste-check` passed on PR head `2d0f013`.
- `git diff --check origin/main...HEAD` and `cmp -s AGENTS.md CLAUDE.md`: passed.
- Live ruleset query: `gh api repos/nkramber/iron-absolution/rulesets` returned an empty list. Exit test 2 remains for the author session after this review, as D-63 directs, and must pass before merge.
- OQ-16: gitar remains outside this review under D-7. It does not block PR-5.
- Push: This metadata commit is the head of `origin/feat/pr-5-main-ruleset`, verified with `gh pr view`.

## Open questions and accepted risks

OQ-16 remains open for the gitar plan under D-7. It does not block this PR.

## Earlier verdicts

None.

## Verdict

**Changes required.** P2-1 leaves the required-check contract without a regression guard. The current ruleset names all four checks, but the test must prevent a later removal from passing silently.
