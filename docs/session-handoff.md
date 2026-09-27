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
