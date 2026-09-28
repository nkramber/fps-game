# Session handoff archive

This file holds the entries that the rotation moves out of `docs/session-handoff.md`, newest first.

## Session 23: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-5, merge. Repository: iron-absolution. Branch: `feat/pr-5-main-ruleset`. PR: #6. Role: author. Base: `f5e9514ebae89c1a5a6a189a8213f2b44f9d1bf5`.

### What this session did, and why

- Review round 2 approved the effective head `1fe466b` with the verdict `Ready for owner merge`. The two rounds used the exit codes 10 and 0.
- On the instruction of the owner, the session created the live ruleset of `main` from the file of the PR head (D-63). The ruleset id is 24080063, with the enforcement `active`.
- Exit test 2 passed: the comparison of `docs/runbooks/main-ruleset.md` gave an empty diff. PR #6 then read `CLEAN` and `MERGEABLE` under the live ruleset.

### The state of the build

- The effective head is `1fe466b`. `make` passes on the Mac: 288 tests, a clean format, and 0 findings of ste-check.
- The remote head is the commit of this entry. The state is pending merge.
- The live ruleset of `main` requires `ste-check`, `build, test, and format`, `coverage report`, and `doc-gate` (D-61).

### What is in flight

- The owner confirms the squash merge of PR #6. PR-5 merges under its own ruleset.

### Traps and gotchas

- A renamed or removed required job blocks every merge until the live ruleset changes. The runbook gives the order.
- PR-6 must plan when `review-gate` joins the live ruleset, because a required check that never reports blocks the merge.
- No session merges with `gh pr merge --admin` (D-60).

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-6.
- For a fresh session: the session number check of `ste-check` reads a heading number with `int.Parse`, so a number too large for an int stops the check with no context (T-2).

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

## Session 12: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-4, round 1. Repository: iron-absolution. Branch: `feat/pr-4-doc-gate-rotation`. PR: #5. Role: author. Base: `6acc6a85798182781a6fc1193c6e37d90dd03020`.

### What this session did, and why

- Ran the exit tests of PR-3 on a clean `main` first: `make hooks`, then `make`. The result was 232 tests, a clean format, and 0 findings.
- Ported the `doc-gate` and `handoff-rotate` commands of what-you-carry, with their tests. The owner resolved four divergences as D-56 to D-59.
- D-56 gives the gate its own workflow, `.github/workflows/doc-gate.yml`, because the `edited` event in `ci.yml` would cancel the build.
- D-57 takes the full rule set. D-58 sorts an entry out of place back in order. D-59 adds `make handoff-rotate`.
- Updated the agent files, the PR template, the one-pr-one-session skill, the runbook, and the PR-4 entry of the design doc.
- This session ran `make handoff-rotate` on this entry. It moved session 2 to the archive, which is exit test 2 on the live files.

### The state of the build

- `make` passes on the Mac. The test count and the checks of the pushed head are in the PR.
- The remote head is the commit of this entry on `feat/pr-4-doc-gate-rotation`. The state is pending merge.

### What is in flight

- The checks of PR #5, then `make codex-review PR=5` (D-54).
- Exit test 1 runs on PR #5 itself: the first description leaves one Documents line empty, and the `doc-gate` check must go red.

### Traps and gotchas

- The handoff has no title. Its first line is the newest heading, so a parse that looks for a line end before the heading skips the newest entry.
- The session line writes `` Branch: `<branch>` `` with a colon. The gate looks for that exact form.
- The `doc-gate` check reads the description of the PR. Edit the description, not a file, to fix a Documents line.

### The questions that block progress

- None blocks PR-4. OQ-16 still holds gitar (D-7).

### The next concrete action

- Finish the review loop of PR #5, then give the owner the merge summary.

## Session 11: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-3, merge. Repository: iron-absolution. Branch: `feat/pr-3-codex-review`. PR: #4. Role: author. Base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`.

### What this session did, and why

- Review round 3 approved the effective head `2551eb3` with the verdict `Ready for owner merge`. `make codex-review` gave the exit code 0.
- The three rounds used the exit codes 10, 10, and 0. The command pushed and judged each record, then removed the worktree and the branch.
- The session moved session 1 to `docs/session-handoff-archive.md` by hand, to keep ten entries. PR-4 adds the rotation command.

### The state of the build

- The effective head is `2551eb3`. `make` passes on the Mac: 232 tests, a clean format, and 0 findings of ste-check.
- This entry commit is the remote head. The state is pending merge.

### What is in flight

- The owner confirms the squash merge of PR #4.

### Traps and gotchas

- Each review round adds a Codex entry. Rotate by hand until PR-4 merges.
- The first line of `docs/session-handoff.md` is the newest entry. The file has no title.

### The questions that block progress

- None blocks PR-3. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-4.

## Session 10: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-3, round 3. Repository: iron-absolution. Branch: `feat/pr-3-codex-review`. PR: #4. Role: reviewer. Base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`.

### What this session did, and why

- The review checked the launcher correction and its regression tests again.
- D-55 says the tools and review need no Windows run. P2-1 is fixed in `2551eb3`.
- The review record now approves effective head `2551eb3`.

### The state of the build

- `make` passed on macOS: 232 tests, clean format, and 0 ste-check findings.
- All three CI jobs passed at metadata tip `241375c`.
- This metadata commit is the remote head.

### What is in flight

- The owner reads the review and gives the merge confirmation after the merge summary.

### Traps and gotchas

- Documents commits do not move the effective head (D-49).
- OQ-16 does not block PR-3. D-7 keeps gitar out.

### The questions that block progress

- None.

### The next concrete action

- Give the owner the merge summary, then wait for the merge decision.

## Session 9: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-3, round 3. Repository: iron-absolution. Branch: `feat/pr-3-codex-review`. PR: #4. Role: author. Base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`.

### What this session did, and why

- Review round 2 gave `Blocked`: P2-1 stayed open, because no Windows run proved the launch fix.
- The owner refuted that demand: all development work runs on the Mac (D-55). D-33 has a dated note of the scope.
- G-12, `CLAUDE.md`, `AGENTS.md`, and the review standard of the pr-review skill now bind Windows to the game alone.
- `docs/reviews/pr-4-response.md` gives the round 2 answer: no merit, D-55.

### The state of the build

- The effective head stays `2551eb3`, because this round changed documents alone. `make` passes on the Mac: 232 tests, a clean format, and 0 findings of ste-check.
- This entry commit is the remote head.

### What is in flight

- Review round 3 runs when the checks of this head are green (D-54).

### Traps and gotchas

- P2-1 lists two heads. The effective head of round 3 is `2551eb3` again, so an open P2-1 keeps two heads and no three-strike stop follows.
- Handoff rotation comes in PR-4. The file holds nine entries now, and the limit is ten.

### The questions that block progress

- None blocks PR-3.

### The next concrete action

- Read the verdict of round 3, then write the merge summary or answer the findings.

## Session 8: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-3, round 2. Repository: iron-absolution. Branch: `feat/pr-3-codex-review`. PR: #4. Role: reviewer. Base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`.

### What this session did, and why

- The review checked the correction to P2-1 at effective head `2551eb3`.
- The correction starts the npm entry script through `node`. The local regression tests pass.
- P2-1 stays open because no Windows test log or end-to-end run is present (D-33).
- The review record gives the verdict `Blocked` until the Windows evidence arrives.

### The state of the build

- The effective head is `2551eb31c274db277159c779a5e98b7bc67e4c51`.
- `make` passes on macOS with 232 tests, clean format, and 0 ste-check findings.
- All three CI jobs pass at metadata tip `5561e3f3f808117234dfc193d9365f33d8241427`.
- This metadata commit publishes the review record and this entry.

### What is in flight

- The owner must run the launcher tests and the real review command on Windows.
- PR #4 needs another Codex review after the Windows evidence arrives.

### Traps and gotchas

- CI runs on Ubuntu. It does not prove the Windows process launch.
- Do not move the work head during a review round (D-14).

### The questions that block progress

- None. OQ-16 remains open but does not affect this review (D-7).

### The next concrete action

- The owner posts the Windows test and command results. Then start a new review round.

## Session 7: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-3, round 2. Repository: iron-absolution. Branch: `feat/pr-3-codex-review`. PR: #4. Role: author. Base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`.

### What this session did, and why

- The first real `make codex-review PR=4` ran to its end with the exit code 10. Codex pushed the record and its handoff entry as one commit, `8b40ff6`. Exit test 2 holds.
- P2-1 had full merit: the CLI path of the Makefile does not exist on Windows. `2551eb3` starts `node` with the entry script `bin/codex.js` on each platform.
- The owner set D-54: the author starts each review round with no question first.
- The subject of the review commit named PR-4 in place of PR-3. The pr-review skill and the agent files now give an example with both numbers.

### The state of the build

- The effective head is `2551eb3`. `make` passes on the Mac: 232 tests, a clean format, and 0 findings of ste-check.
- This entry commit is the remote head.

### What is in flight

- The second review round runs when the checks of this head are green (D-54).

### Traps and gotchas

- Commit no file outside the metadata set while a round runs. The round then fails (D-14).
- A Windows run of the tests is open to the owner (D-33). CI runs on Ubuntu alone.

### The questions that block progress

- None blocks PR-3.

### The next concrete action

- Read the verdict of round 2, then answer it or write the merge summary.

## Session 6: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-3, round 1. Repository: iron-absolution. Branch: `feat/pr-3-codex-review`. PR: #4. Role: reviewer. Base: `e97da5a5238807c29c7a3bcdc5d4184dfb97f1b4`.

### What this session did, and why

- The session reviewed the full PR-3 diff against the roadmap, exit tests, decisions, questions, and review contracts.
- The provider gate passed. Claude Code authored the change, and Codex reviewed it.
- The record has finding P2-1. The Make target cannot launch the Windows Codex command shim.

### The state of the build

- The effective head is `b0596f5accb3c1e4d8ad5dc05ed668763c2447ed`.
- `make` passed on macOS: 226 tests, clean format, and 0 findings of ste-check.
- All three CI checks passed on the reviewed tip. The remote head is the published metadata commit on `origin/feat/pr-3-codex-review`.

### What is in flight

- P2-1 needs a platform-aware CLI path and launch test on Windows.
- The real `make codex-review PR=4` run judges the review record after this metadata push.

### Traps and gotchas

- The CI workflow runs on Ubuntu only. It does not test the Windows process launch.
- OQ-16 still holds the gitar plan. D-7 keeps gitar out of this PR.

### The questions that block progress

- None. The Windows defect has a concrete correction and regression test.

### The next concrete action

- The author answers P2-1, then reruns the Windows process-launch test and the review.

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

- The effective head is `b0596f5`. `make` passes on the Mac: 226 tests, a clean format, and 0 findings of ste-check.
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
