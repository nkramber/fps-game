## Session 22: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-6, round 2. Repository: iron-absolution. Branch: `feat/pr-5-main-ruleset`. PR: #6. Role: reviewer. Base: `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`.

### What this session did, and why

- Re-reviewed PR-5 at effective head `1fe466b`. The review found that the new D-61 test fixes P2-1.
- Updated `docs/reviews/pr-6.md` with the round-two result and preserved the earlier verdict.

### The state of the build

- `make` passes: 288 tests, clean format, and 0 findings from ste-check.
- All four hosted checks pass on the PR branch. The effective work head is `1fe466b`.
- The review record and this handoff entry are in one metadata commit on the PR branch.
- The live ruleset does not exist yet. The author session must apply it and compare it with the PR file before merge (D-63).

### What is in flight

- The PR awaits the live ruleset comparison and the merge gate.

### Traps and gotchas

- The ruleset test keeps the four checks of D-61 as a subset. Roadmap PR-6 adds `review-gate`.
- The review approves the effective work head. Later work changes need a new review.

### The questions that block progress

- None. OQ-16 still holds gitar under D-7.

### The next concrete action

- Apply the live ruleset from the PR head and run the comparison in `docs/runbooks/main-ruleset.md` (exit test 2, D-63).

## Session 21: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-5, round 2. Repository: iron-absolution. Branch: `feat/pr-5-main-ruleset`. PR: #6. Role: author. Base: `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`.

### What this session did, and why

- Review round 1 gave `Changes required` with the exit code 10. P2-1: no test held the four checks of D-61, so a check could leave the file and its workflow together.
- The trigger reproduced. `1fe466b` adds `TheRulesetRequiresEachCheckOfTheDecision`, which holds the four names as a subset. `docs/reviews/pr-6-response.md` records the answer.

### The state of the build

- The effective head is `1fe466b`. `make` passes on the Mac: 288 tests, a clean format, and 0 findings of ste-check.
- The remote head is the commit of this entry. The state is pending merge.
- No live ruleset exists yet. `main` has no branch protection.

### What is in flight

- The review round 2 of `make codex-review PR=6`.
- After an approval and green checks, the session applies the live ruleset from the PR head and runs the comparison of `docs/runbooks/main-ruleset.md` (exit test 2, D-63).

### Traps and gotchas

- The ruleset requires the four checks by name. A renamed or removed job blocks every merge until the live ruleset changes.
- A required check that no workflow of `main` has yet can block the PR that adds it. PR-6 must plan the order of `review-gate`.

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- Run `make codex-review PR=6` after the checks are green.
- For a fresh session: the session number check of `ste-check` reads a heading number with `int.Parse`, so a number too large for an int stops the check with no context (T-2).

## Session 20: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-5, round 1. Repository: iron-absolution. Branch: `feat/pr-5-main-ruleset`. PR: #6. Role: reviewer. Base: `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`.

### What this session did, and why

- Reviewed the ruleset, its tests, the setup runbook, and the related documents.
- Found that the tests allow one of the four required checks and its matching job to disappear together.
- Wrote `docs/reviews/pr-6.md` with verdict `Changes required` for effective head `2d0f013`.

### The state of the build

- `make` passes on the Mac: 287 tests, a clean format, and 0 findings of ste-check.
- The four hosted checks pass on PR head `2d0f013`.
- The pushed metadata commit will be the remote head. The PR work head remains `2d0f013`.

### What is in flight

- P2-1 needs a test that fixes the four check names of D-61.
- The live ruleset does not exist yet. D-63 assigns its setup and comparison to the author session after review approval and before merge.

### Traps and gotchas

- Ruleset tests currently derive their expected checks from the ruleset file and workflows.
- OQ-16 keeps the gitar pass out of this PR under D-7.

### The questions that block progress

- None. OQ-16 still holds the gitar pass.

### The next concrete action

- The author fixes P2-1, runs its regression check, and starts the next review round.

## Session 19: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-5, round 1. Repository: iron-absolution. Branch: `feat/pr-5-main-ruleset`. PR: #6. Role: author. Base: `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`.

### What this session did, and why

- The exit tests of PR-4 on a clean checkout of `main` passed: 272 tests, a clean format, and 0 findings. `make handoff-rotate` moved no entry and changed no file.
- PR-5 ports `.github/rulesets/main.json`, `RulesetTests`, and `docs/runbooks/main-ruleset.md` from what-you-carry.
- The owner answered four divergences of D-13: D-60 (admin bypass through a PR merge), D-61 (four required checks, a test in both directions), D-62 (the ruleset alone), and D-63 (the session applies the live ruleset before the merge).
- The agent files, the PR template, and the merge procedure now forbid `gh pr merge --admin` (D-60).

### The state of the build

- `make` passes on the Mac: 287 tests, a clean format, and 0 findings of ste-check.
- The remote head is the commit of this entry. The state is pending merge.
- No live ruleset exists yet. `main` has no branch protection.

### What is in flight

- The review round 1 of `make codex-review PR=6`.
- After an approval and green checks, the session applies the live ruleset from the PR head and runs the comparison of `docs/runbooks/main-ruleset.md` (exit test 2).

### Traps and gotchas

- The ruleset requires the four checks by name. A renamed or removed job blocks every merge until the live ruleset changes.
- A required check that no workflow of `main` has yet can block the PR that adds it. PR-6 must plan the order of `review-gate`.

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- Run `make codex-review PR=6` after the checks are green.
- For a fresh session: the session number check of `ste-check` reads a heading number with `int.Parse`, so a number too large for an int stops the check with no context (T-2).

## Session 18: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-4, merge. Repository: iron-absolution. Branch: `feat/pr-4-doc-gate-rotation`. PR: #5. Role: author. Base: `6acc6a85798182781a6fc1193c6e37d90dd03020`.

### What this session did, and why

- Review round 3 approved the effective head `eb76650` with the verdict `Ready for owner merge`. `make codex-review` gave the exit code 0.
- The three rounds used the exit codes 10, 10, and 0. P2-1 is fixed in `614847a`, and P2-2 is fixed in `eb76650`.
- PR-4 added the `doc-gate` and `handoff-rotate` commands, the `doc-gate` workflow, and `make handoff-rotate` (D-56 to D-59).

### The state of the build

- The effective head is `eb76650`. `make` passes on the Mac: 272 tests, a clean format, and 0 findings of ste-check.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The owner confirms the squash merge of PR #5.

### Traps and gotchas

- The session end now runs `make handoff-rotate` after the new entry, before the commit.
- The session line writes `` Branch: `<branch>` `` with a colon. The `doc-gate` job looks for that exact form.

### The questions that block progress

- None blocks PR-4. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-5.
- For a fresh session: the session number check of `ste-check` reads a heading number with `int.Parse`, so a number too large for an int stops the check with no context (T-2).

## Session 17: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-5, round 3. Repository: iron-absolution. Branch: `feat/pr-4-doc-gate-rotation`. PR: #5. Role: reviewer. Base: `6acc6a85798182781a6fc1193c6e37d90dd03020`.

### What this session did, and why

- Rechecked P2-2 at the new effective head, `eb76650`.
- The fix rejects Session `2147483647` before either file write, with a contextual fault.
- Updated `docs/reviews/pr-5.md`. Both earlier findings now pass their regression checks.
- `make` passed with 272 tests. All four hosted checks passed.

### The state of the build

- The effective head is `eb76650`. The current remote tip before this metadata commit was `b964463`.
- The review record and this handoff entry form one metadata commit. The state is pending merge.

### What is in flight

- The owner reads the merge summary and confirms the merge of PR #5.

### Traps and gotchas

- A heading at `2147483647` parses as an int, but the next session value does not fit.
- The current branch tip has review and handoff metadata after the effective head.

### The questions that block progress

- None blocks this PR. OQ-16 still holds the gitar plan under D-7.

### The next concrete action

- Give the owner the merge summary for PR #5.

## Session 16: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-4, round 3. Repository: iron-absolution. Branch: `feat/pr-4-doc-gate-rotation`. PR: #5. Role: author. Base: `6acc6a85798182781a6fc1193c6e37d90dd03020`.

### What this session did, and why

- Review round 2 marked P2-1 fixed in `614847a`, and it found P2-2: at Session 2147483647, the next session number wrapped to a negative number, and the command exited 0.
- The trigger reproduced, so the finding has full merit. `eb76650` makes that number a contextual fault with exit 1 and no file change.
- One regression test fails with the check off. `docs/reviews/pr-5-response.md` holds the answer under "Round 2".

### The state of the build

- The effective head is `eb76650`. `make` passes on the Mac: 272 tests, a clean format, and 0 findings of ste-check.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The checks of the new head, then review round 3 with `make codex-review PR=5` (D-54).

### Traps and gotchas

- Exit test 1 ran on PR #5 itself: the first description had an empty `docs/runbooks/` line, and `doc-gate` went red. The edit made it green, and the build jobs did not run again.
- The session line writes `` Branch: `<branch>` `` with a colon. The gate looks for that exact form.

### The questions that block progress

- None blocks PR-4. OQ-16 still holds gitar (D-7).

### The next concrete action

- Finish the review loop of PR #5, then give the owner the merge summary.
- For a fresh session: the session number check of `ste-check` reads a heading number with `int.Parse`, so a number too large for an int stops the check with no context (T-2).

## Session 15: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-5, round 2. Repository: iron-absolution. Branch: `feat/pr-4-doc-gate-rotation`. PR: #5. Role: reviewer. Base: `6acc6a85798182781a6fc1193c6e37d90dd03020`.

### What this session did, and why

- Rechecked the fix for P2-1 and the full PR-4 scope.
- Ran the review in the `review/pr-5` worktree.
- Found P2-2: session `2147483647` makes the next session value wrap to `-2147483648`.
- Updated `docs/reviews/pr-5.md` with the fixed finding, the new finding, and the verdict for effective head `614847a`.
- Corrected the stale test count in the PR description. Its `doc-gate` check passed again.

### The state of the build

- `make` passes at effective head `614847a`: 271 tests, clean format, and 0 ste-check findings.
- The hosted build, coverage, `doc-gate`, and `ste-check` checks pass at that head.
- The review record and this handoff entry are metadata changes on `feat/pr-4-doc-gate-rotation`. They do not move the effective head.

### What is in flight

- P2-2 needs a checked increment and a regression test that proves no file changes on overflow.

### Traps and gotchas

- `int.TryParse` rejects values above `Int32.MaxValue`, but `Int32.MaxValue` itself still overflows when the command adds one.
- A description edit runs `doc-gate` again. The corrected test count passed that check.

### The questions that block progress

- None blocks this review. OQ-16 still holds the gitar plan under D-7.

### The next concrete action

- Correct P2-2, add its regression test, and start a new review round on the corrected head.

## Session 14: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-4, round 2. Repository: iron-absolution. Branch: `feat/pr-4-doc-gate-rotation`. PR: #5. Role: author. Base: `6acc6a85798182781a6fc1193c6e37d90dd03020`.

### What this session did, and why

- Review round 1 gave `Changes required` with one finding, P2-1: a session number too large for an int crashed `handoff-rotate` with exit 134.
- The trigger reproduced, so the finding has full merit. `614847a` makes the parse a contextual fault with exit 1 in both commands.
- Two regression tests fail on the old parse. `docs/reviews/pr-5-response.md` holds the answer.

### The state of the build

- The effective head is `614847a`. `make` passes on the Mac: 271 tests, a clean format, and 0 findings of ste-check.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The checks of the new head, then review round 2 with `make codex-review PR=5` (D-54).

### Traps and gotchas

- Exit test 1 ran on PR #5 itself: the first description had an empty `docs/runbooks/` line, and `doc-gate` went red. The edit made it green, and the build jobs did not run again.
- The session line writes `` Branch: `<branch>` `` with a colon. The gate looks for that exact form.

### The questions that block progress

- None blocks PR-4. OQ-16 still holds gitar (D-7).

### The next concrete action

- Finish the review loop of PR #5, then give the owner the merge summary.
- For a fresh session: the session number check of `ste-check` reads a heading number with `int.Parse`, so a number too large for an int stops the check with no context (T-2).

## Session 13: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-5, round 1. Repository: iron-absolution. Branch: `review/pr-5`. PR: #5. Role: reviewer. Base: `6acc6a85798182781a6fc1193c6e37d90dd03020`.

### What this session did, and why

- Reviewed PR #5 against its exit tests, guardrails, decisions, and PR comments.
- Ran `make`. The build, 269 tests, format, and ste-check passed.
- Reproduced P2-1: an out-of-range session number crashes `handoff-rotate` with exit 134.
- Added `docs/reviews/pr-5.md` with the revision-specific review.

### The state of the build

- The reviewed work head is `b5993f4a8eed3353208619a50a1d6448d9434dfd`. All four hosted checks pass at this head.
- The review record and this handoff entry await one metadata commit and push to `feat/pr-4-doc-gate-rotation`.

### What is in flight

- P2-1 needs a contextual range check and a regression test.
- The PR needs a new review round after the correction.

### Traps and gotchas

- `HandoffRotateRules.Parse` uses `int.Parse` for a session number. An out-of-range value bypasses the command fault handler.

### The questions that block progress

- OQ-16 still holds the gitar plan under D-7. It does not block this review.

### The next concrete action

- Correct P2-1, add its regression test, and run the focused command test.
