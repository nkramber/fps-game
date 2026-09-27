## Session 5: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-3, round 1. Repository: iron-absolution. Branch: `feat/pr-3-codex-review`. PR: #4. Role: author. Base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`.

### What this session did, and why

- The exit tests of PR-2 passed on `main`: `make hooks`, then `make` gave 100 tests, a clean format, and 0 findings.
- The session ported the `codex-review` command of what-you-carry into `IronAbsolution.Tools/CodexReview/` (D-14). The gitar start check stays out (D-7).
- A subagent of the session ported the pr-review, review-response, and one-pr-one-session skills (D-46). The session read and corrected the three SKILL.md files and the merge steps.
- The owner answered eight divergences and one conflict: D-46 to D-53. D-53 corrects exit test 3 of PR-3.
- The merge steps now give the two options of the owner: `Yes, squash-merge it` and `No, not yet`.

### The state of the build

- The work head is `b0596f5`. `make` passes on the Mac: 226 tests, a clean format, and 0 findings of ste-check.
- This entry commit is the remote head. The CI of PR #4 runs on it.

### What is in flight

- Exit test 2 needs a real run of `make codex-review PR=4`. It installs the newest Codex CLI with npm and uses the ChatGPT login of the owner.

### Traps and gotchas

- `make ste-check` reads tracked files alone. Run `git add` on a new document before the check.
- The pre-commit hook refuses a commit on no branch, so the review worktree takes the branch `review/pr-<n>` (D-48).
- The PR-1 and PR-2 entries of `docs/design.md` have no done mark. The PR-3 entry has one, as the one-pr-one-session skill asks.
- The records of PR #2 and PR #3 use the old finding form `- Status:`. The new parser refuses that form, and those records stay as history.

### The questions that block progress

- None blocks PR-3.

### The next concrete action

- The owner confirms the first run of `make codex-review PR=4`. The session then answers the verdict with the review-response skill.

## Session 4: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-2, round 2. Repository: iron-absolution. Branch: `feat/pr-2-tools-ste-check`. PR: #3. Role: author. Base: `d9561582b1fd9cb25086705770a8d6529bdad5a5`.

### What this session did, and why

- The Codex review of head `b73d3c7` gave the verdict Changes required, with one finding: P2-1.
- P2-1: rule STE 6.6 counted sentences across a heading. The fix ends the paragraph at a heading, a list item, a table row, and a fence.
- `docs/reviews/pr-3-response.md` answers the finding. The regression test failed on the old code in each of its five cases.

### The state of the build

- The effective head is `cbc4689`. `make` passes on the Mac: 100 tests, a clean format, and 0 findings of ste-check.
- This entry commit is the remote head. The session reads its CI before the owner starts the next review.

### What is in flight

- PR #3 needs a repeat Codex review of the new head.

### Traps and gotchas

- The same paragraph defect is in the checker of the-thing-below. This repository does not change that repository.
- The hook of `make hooks` runs ste-check on each commit in this checkout.

### The questions that block progress

- None blocks PR-2.

### The next concrete action

- The owner starts the repeat Codex review of PR #3.

## Session 3: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-2, round 1. Repository: iron-absolution. Branch: `feat/pr-2-tools-ste-check`. PR: #3. Role: author. Base: `d9561582b1fd9cb25086705770a8d6529bdad5a5`.

### What this session did, and why

- Steam lists an upcoming game with the name Emberline. The session screened names, and the owner picked Iron Absolution (D-39). `docs/research/name-screen.md` holds the screen.
- On the instruction of the owner, the session renamed the GitHub repository to `nkramber/iron-absolution` and the local folder to `iron-absolution`. GitHub redirects the old URL.
- The session asked the owner about each divergence of the role models (D-13). D-40 to D-45 record the answers: the stack, the Makefile, the CI layout, the hook, the C# skill, and the coverage.
- The session ported the `ste-check` command of the-thing-below into `IronAbsolution.Tools`, with 94 tests. The port drops AGENTS 2 and DOCS 1.
- The Codex review of PR-1 changed no phase or gate, so the roadmap acceptance of D-27 holds without its condition.

### The state of the build

- The effective head is `979ba5d`. CI of that commit passed on the hosted runner: `ste-check`, `build, test, and format`, and `coverage report` (exit test 1). This entry commit is the remote head.
- `make` passes on the Mac: 94 tests, a clean format, and 0 findings of ste-check.
- Exit test 2: commit `ab988ae` added a semicolon to `README.md`. The `ste-check` job failed with `README.md:37: rule STE 8.1: semicolon`. Commit `d1e0bad` reverted it.
- Exit test 3: the test `EveryLiveDocumentOfThisRepositoryPassesEveryRule` passes.

### What is in flight

- PR #3 needs the Codex review. Until PR-3 merges, the owner starts Codex by hand with the prompt of `AGENTS.md`.

### Traps and gotchas

- Run `make hooks` one time in each checkout. The hook then runs ste-check on each commit.
- A folder symlink `fps-game` points to `iron-absolution` on the SSD. Remove it when no tool uses the old path.
- The path rule knows no Unreal file type yet. Phase 1 adds them to `ReferenceRules.PathExtensions`.
- The checker reads the ids of this repository alone. A code comment cites no id of a role model.

### The questions that block progress

- None blocks PR-2. OQ-9, OQ-10, and OQ-12 wait for phase 2. OQ-16 waits for gitar. OQ-21 waits for phase 8.

### The next concrete action

- The owner starts the Codex review of PR #3.

## Session 2: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-1, round 2. Repository: fps-game. Branch: `docs/pr-1-foundation`. PR: #2. Role: reviewer. Base: `bfb71cfcf715e35b0df336605a759606cc4d7345`.

### What this session did, and why

- Reviewed the full PR-1 diff against the roadmap, decisions, questions, and exit tests.
- Checked local and external links, ids, the author provider, and the primary-source claims.
- Added `docs/reviews/pr-2.md`. No finding changes a phase, its order, or its gate (D-27).
- Committed and pushed the review record and this handoff entry.

### The state of the build

- The effective implementation head is `a9fb5caf0fa6860fe33426aa7672bf24b6597ba3`. No CI checks exist on PR #2. PR-2 creates the ste-check job.

### What is in flight

- PR #2 needs the owner's merge decision.

### Traps and gotchas

- The role-model `ste-check` reports `DOCS 1` and `AGENTS 2` findings that do not apply here. See `docs/reviews/pr-2.md`.
- Xcode 16.2 cannot build Unreal Engine 5.8. Phase 1 needs Xcode 26.1.1 (D-28).

### The questions that block progress

- None blocks PR-1. OQ-9, OQ-10, and OQ-12 wait for phase 2. OQ-16 waits for gitar. OQ-21 waits for phase 8.

### The next concrete action

- The owner can review the four-part merge summary and decide whether to merge.

## Session 1: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-1, round 1. Repository: fps-game. Branch: `docs/pr-1-foundation`. PR: #2. Role: author. Base: `bfb71cf`.

### What this session did, and why

- Studied the two role models, and re-checked the engine, Meshy, pipeline, and level research against primary sources (`docs/research/`).
- Wrote the registers, the design doc with the high-level roadmap (sections 7 and 8), the agent files, the PR template, the review format, and a session runbook.
- The owner made the role models paramount for all infrastructure (D-12), with a question for each divergence (D-13, D-14 to D-23). PR-1 moved to new commits, and GitHub PR #1 closed (D-23).
- The owner then answered the open questions (D-24 to D-38): MIT license, Emberline, Unreal Engine 5.8, macOS and Windows budgets, and a level of 30 minutes or more. Replay value is not a primary goal. Unreal best practices govern engine work.
- On the instruction of the owner, the session replaced `LICENSE` with MIT (D-24). It also discarded the local prototype: it reset the local `main` to `origin/main` and deleted the prototype files (D-25).

### The state of the build

- `origin/main` is `bfb71cf`. No CI, ruleset, or Unreal project exists. The repository setting for auto-merge is off.
- The ste-check binary of the-thing-below, run by hand on a scratch copy, gives no finding that applies here. PR-2 ports the checker.

### What is in flight

- PR-1 needs a new Codex review, because the owner stopped the earlier one before this round. The prompt is in `AGENTS.md`. Then the owner merges.

### Traps and gotchas

- The ste-check of the-thing-below needs stubs of two of its skills, and its rules AGENTS 2 and DOCS 1 do not apply here. Run it on a scratch copy only.
- Xcode 16.2 cannot build Unreal Engine 5.8. It needs Xcode 26.1.1, never 26.4 or later (D-28).
- Unreal cannot build Windows packages on the Mac. The owner runs the Windows builds (D-33).
- 60 fps at 4K output on the base M4 is a hard target (F-14). Measure M-9 early.
- A harness reminder asks for co-author lines. T-6 wins (D-16).

### The questions that block progress

- None blocks PR-2. OQ-9, OQ-10, and OQ-12 wait for phase 2. OQ-16 waits for gitar. OQ-21 waits for phase 8.

### The next concrete action

- The owner starts the Codex review of PR-1 and merges it after a clean verdict.
- After the merge, write the transitional prompt for PR-2: the Emberline.Tools project and ste-check.
