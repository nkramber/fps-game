# PR-6 review

Date: 2026-09-27

## Identity

- PR: 6
- Target: `main`
- Base: `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`
- Merge base: `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`
- Head: `1fe466bb4265714bb49ef8d110e1ab36834b3a87`
- Branch: `feat/pr-5-main-ruleset`

## Provider gate

Claude Code authored the PR, as the newest handoff entry for `feat/pr-5-main-ruleset` states. Codex is the other provider. The provider gate passes under T-4 and D-6.

## Intended behavior and scope

PR-5 adds the ruleset of `main`, its tests, and its setup runbook. This repeat review inspected the complete PR diff, the PR description and comments, the PR-5 roadmap entry and exit tests, sections 6 to 8 of `docs/design.md`, D-60 to D-63, OQ-16, changed files in context, the round-one review and response, and the session and review procedures. No focused roadmap exists for phase 0. Unreal code, game content, binary assets, and game platform budgets do not apply because this PR changes none of them. The review checked the ruleset fields, required-check bindings, bypass, runbook commands, document consistency, and CI evidence.

## Findings

### P2-1: The ruleset test allows a required check to disappear

Status: fixed in `1fe466bb4265714bb49ef8d110e1ab36834b3a87`.

Open at: `2d0f013af92bd8c2d634e4d87383de9fed3fc96a`.

File: `IronAbsolution.Tests/RulesetTests.cs:38-45` at the round-one head.

Trigger: Remove one required check and its matching job from the PR workflows.

Expected: The ruleset test requires the four checks named by D-61, and it binds each one to exactly one PR job.

Actual: Round one found that the tests checked only that the list was nonempty and unique. The other tests derived both sides from the ruleset and current workflows. Removing the same check from both could pass.

Consequence: A required gate such as `doc-gate` could disappear from the ruleset and workflows without a test failure.

Evidence: Commit `1fe466b` adds `TheRulesetRequiresEachCheckOfTheDecision`. It names all four checks from D-61 and requires each name in the ruleset. The PR response reports that the old trigger fails this test at the corrected revision. The current `make` passes all 288 tests.

Correction: Assert the four check names from D-61 independently of the ruleset and workflow files.

Regression check: `TheRulesetRequiresEachCheckOfTheDecision` must fail if a D-61 check leaves the ruleset and its workflow together. `make` passes with the current files.

## Out of scope

The review-gate check and its addition to the ruleset belong to roadmap PR-6. The auto-merge setting also belongs to roadmap PR-6. Gitar remains under D-7 and OQ-16.

## PR comments

None. The issue-comment export and GraphQL review-thread query returned no comments or threads.

## Description edits

None.

## Verification

- `git fetch origin`, `git status --short --branch`, and revision lookup: clean worktree on `review/pr-6`; base and merge base `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`; PR branch tip `cc0925ddf0384276694dee2c2fa8d9949876a504`.
- Effective-head lookup: `1fe466bb4265714bb49ef8d110e1ab36834b3a87` is the newest commit outside the documents set.
- `make` at PR branch tip `cc0925d`: build passed with 0 warnings and 0 errors; 288 tests passed, 0 failed, and 0 skipped; format passed; ste-check reported 0 findings.
- `gh pr checks 6`: `build, test, and format`, `coverage report`, `doc-gate`, and `ste-check` passed on PR branch tip `cc0925d`.
- `git diff --check origin/main...HEAD` and `cmp -s AGENTS.md CLAUDE.md`: passed.
- Live ruleset query: `gh api repos/nkramber/iron-absolution/rulesets` returned an empty list. The first-setup procedure and exit test 2 remain for the author session after this review, as D-63 directs. The live comparison must pass before merge.
- OQ-16: gitar remains outside this review under D-7. It does not block PR-5.
- Push: this review record and session handoff are published together on `origin/feat/pr-5-main-ruleset`; verified with `gh pr view`.

## Open questions and accepted risks

OQ-16 remains open for the gitar plan under D-7. It does not block this PR.

## Earlier verdicts

- **Changes required** at `2d0f013af92bd8c2d634e4d87383de9fed3fc96a`: P2-1 showed that a check could leave the ruleset and workflows without a regression failure.

## Verdict

**Ready for owner merge.** The review covers effective head `1fe466bb4265714bb49ef8d110e1ab36834b3a87`, and the earlier P2-1 finding now has a passing regression test. The PR checks pass; the author session must complete the live ruleset comparison before merge.
