## Session 26: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-6, round 2. Repository: iron-absolution. Branch: `feat/pr-6-review-gate`. PR: #7. Role: author. Base: `5871f2c870d5eb110eccbadd67c5f633c5619155`.

### What this session did, and why

- Round 1 gave `Changes required` with one finding, P2-1 (exit 10). A backdated commit after the `review-override` label passes the time rule, because the rule reads the committer time.
- The trigger reproduces. The owner accepted the risk (D-68, F-18): one account can add the label again anyway (F-9).
- The rule cites D-68, and `docs/reviews/pr-7-response.md` gives the evidence. No behavior changed.
- The first `doc-gate` run failed on the words "update ... after the merge" in the description. The reworded description passes.

### The state of the build

- `make` passes on the Mac: 363 tests, a clean format, and 0 findings of ste-check.
- The remote head is the commit of this entry. The state is pending review round 2.
- The live ruleset still requires four checks (D-67).

### What is in flight

- Round 2 of `make codex-review PR=7`.

### Traps and gotchas

- The deferral rule of `doc-gate` reads the PR description too. Do not write "update" near "after the merge" there.
- The label `review-override` does not exist yet, and auto-merge is off. Both are owner steps of `docs/runbooks/main-ruleset.md`.

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the approval, ask the owner to confirm the merge. Auto-merge is off, so merge with `gh pr merge 7 --squash`.
- After the merge, on the explicit instruction of the owner, apply the ruleset file of `main` to the live ruleset, and run the comparison (D-67).

## Session 25: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-6, round 1. Repository: iron-absolution. Branch: `feat/pr-6-review-gate`. PR: #7. Role: reviewer. Base: `5871f2c870d5eb110eccbadd67c5f633c5619155`.

### What this session did, and why

- Reviewed PR #7 at effective head `7f4cb583772c28e2bdb2a1a32c8f2e921442a88c`.
- Found that the override label accepts a document commit pushed after the label when its Git committer time is earlier. The temporary reproduction passed the gate.
- Wrote `docs/reviews/pr-7.md` with verdict `Changes required` and P2-1.

### The state of the build

- `make` passes on the Mac: 363 tests, a clean format, and 0 ste-check findings.
- All four checks available to PR #7 pass on head `7f4cb58`. GitHub does not run `review-gate` until the workflow reaches `main` (D-67).
- The remote work head is `7f4cb583772c28e2bdb2a1a32c8f2e921442a88c`. This review record and handoff must publish together.

### What is in flight

- P2-1 needs a freshness check that does not trust a PR-controlled commit time.

### Traps and gotchas

- The override rule reads Git `%cI`. A PR author controls the committer timestamp.
- This PR does not get a `review-gate` check because the workflow runs from `main` (D-67).

### The questions that block progress

- None. OQ-16 still holds gitar under D-7.

### The next concrete action

- The author corrects P2-1 and adds a regression test for a backdated document commit.

## Session 24: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-6, round 1. Repository: iron-absolution. Branch: `feat/pr-6-review-gate`. PR: #7. Role: author. Base: `5871f2c870d5eb110eccbadd67c5f633c5619155`.

### What this session did, and why

- The exit tests of PR-5 passed on `main` first: `make` gave 288 tests, a clean format, and 0 findings. The comparison of the live ruleset gave an empty diff.
- The owner answered four divergences of the port (D-64 to D-67). The job is the check, with no mode file. The label rules of what-you-carry apply. A PR of documents alone merges through the label alone. The live ruleset takes `review-gate` after the merge.
- The `review-gate` command reads the record at the PR head through git, and the label facts from a JSON file. It reuses `ReviewRecord`, `ReviewHeads`, and `GitRepository`.
- The workflow runs on `pull_request_target` from the base, with read rights alone, and fetches the head as data. The job shell has pipefail (T-2).
- The ruleset file requires `review-gate`. The agent files, the skills, the runbooks, and the design doc now describe the live gate and auto-merge.

### The state of the build

- `make` passes on the Mac: 363 tests, a clean format, and 0 findings of ste-check.
- The remote head is the commit of this entry. The state is pending review.
- The live ruleset still requires four checks. PR #7 gets no `review-gate` check, because GitHub runs that workflow only from `main` (D-67).

### What is in flight

- Round 1 of `make codex-review PR=7`.

### Traps and gotchas

- A run step with no shell starts `bash -e` with no pipefail. The review-gate job sets `shell: bash` for each step.
- The label `review-override` does not exist yet, and auto-merge is off. Both are owner steps of `docs/runbooks/main-ruleset.md`.
- The fixture commits all have the time 2026-09-27T10:00:00Z. The label tests put the label one hour before or after it.

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the approval, ask the owner to confirm the merge. Auto-merge is off, so merge with `gh pr merge 7 --squash`.
- After the merge, on the explicit instruction of the owner, update the live ruleset from `main`, and run the comparison (D-67).

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
