## Session 32: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-8, round 2. Repository: iron-absolution. Branch: `feat/pr-8-engine-toolchain`. PR: #9. Role: author. Base: `2133abdbd714b3ba2dddf916adc1aef2b693a120`.

### What this session did, and why

- Answered round 1 of the review in `docs/reviews/pr-9-response.md`. Both findings have full merit.
- P2-1: each source of `scripts/toolchain-check.ps1` now fails its own pin, and the script reports each later pin and the total (T-2). The Visual Studio block had the same fault, so the correction covers it.
- P2-2: the exit tests of PR-9 read 1 to 11. Exit test 8 stays the cache test (D-83).
- New `ToolchainScriptTests` runs the script under PowerShell. All 7 cases fail on the script of `d57ed33` and pass on the new script.

### The state of the build

- `make` passes on the Mac: 405 tests, with 7 skipped where PowerShell is absent, a clean format, and 0 findings of ste-check. With a portable PowerShell on the path, 405 of 405 pass.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- Round 2 of `make codex-review PR=9` runs after the checks are green (D-14).
- The owner confirms the squash merge after the merge summary.

### Traps and gotchas

- The Mac has no PowerShell. A portable PowerShell from the release archive in the scratch folder runs the script tests without an install.
- `Join-Path` with a backslash path names one file on Linux. Join each part of a path that a test also reads on the runner.
- One PR comment (the evidence of exit tests 1 and 4). No review thread.

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-9. Work from the clone on the `IronAbsolution` volume (D-81). Set the cache path at the first editor start (D-83).

## Session 31: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-8, round 1. Repository: iron-absolution. Branch: `feat/pr-8-engine-toolchain`. PR: #9. Role: reviewer. Base: `2133abdbd714b3ba2dddf916adc1aef2b693a120`.

### What this session did, and why

- Reviewed the complete PR-8 diff at effective head `d57ed3312cb3479166ab48858287cdd862443cc2`.
- Added `docs/reviews/pr-9.md` with two findings: invalid Windows engine version data stops the report, and two PR-9 exit tests share number 8.
- `make` passed on macOS: 398 tests, clean format, and 0 findings of ste-check.

### The state of the build

- The effective head is `d57ed3312cb3479166ab48858287cdd862443cc2`. The PR tip before the review commit is `223269d7a640a3724c3ab338b7158d0ec6e02d72`.
- The review metadata commit is the remote head of this entry. The verdict is Changes required.

### What is in flight

- The author answers P2-1 and P2-2 in `docs/reviews/pr-9.md`.
- The `review-gate` check must read the published record. It fails for the open findings.

### Traps and gotchas

- The Windows script could not run here because this Mac has no PowerShell runtime. The owner posted the Windows result in the PR comment.
- The cache-path exit test belongs to PR-9 (D-83).
- OQ-16 keeps gitar outside this review (D-7).

### The questions that block progress

- None. The findings need code and roadmap corrections, not an owner decision.

### The next concrete action

- The author corrects the findings, adds regression evidence, and requests the next review round.

## Session 30: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-8, round 1. Repository: iron-absolution. Branch: `feat/pr-8-engine-toolchain`. PR: #9. Role: author. Base: `2133abdbd714b3ba2dddf916adc1aef2b693a120`.

### What this session did, and why

- Added `toolchain-check` for the Mac (`make toolchain-check`) and `scripts/toolchain-check.ps1` for the Windows PC (D-28, D-72, D-74). Each line names the pin, the expected value, and the found value (T-2).
- Added `docs/runbooks/engine-setup.md` and the `## Evidence` section of the PR template (D-80).
- The label text of `review-gate`, `make codex-review`, their tests, and the template follows D-76. The session changed the live label on the instruction of the owner (D-77, D-82).
- The owner answered six questions: D-78 to D-83. The owner installed during PR-8 (D-78). The SSD is case-sensitive, so a new volume `IronAbsolution` holds the engine, Xcode, the cache, and a clone (F-21, D-81).
- Exit test 1 passed on the Mac: 3 of 3 pins. Exit test 4 passed on the Windows PC: 5 of 5 pins. The Windows run first failed a good toolset, because the script read the folder name (F-22). The script now reads `cl.exe`.

### The state of the build

- `make` passes on the Mac: 398 tests, a clean format, and 0 findings of ste-check.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The Codex review of PR #9 runs after the push (D-14).
- The owner confirms the squash merge after the merge summary.

### Traps and gotchas

- The launcher picker lists `5.8.0`, and it installs the newest hotfix. It also upgrades the hotfix without a question (D-28).
- On an external volume, `xip` can leave `Xcode.app` in a folder with a UUID name.
- VS 2026 18.6 and later default to MSVC 14.51. Unreal 5.8 prefers 14.50 and bans a `cl.exe` before 14.50.35723 (`Windows_SDK.json` of the engine).
- The Zen cache goes to the internal disk until the owner sets the editor setting in PR-9 (D-83).

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-9. Work from the clone `/Volumes/IronAbsolution/iron-absolution` (D-81). The first action of PR-9 is exit test 8 setup: the cache path at the first editor start (D-83).

## Session 29: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-7, round 1. Repository: iron-absolution. Branch: `docs/pr-7-phase-1-roadmap`. PR: #8. Role: author. Base: `6e879a3d1f73295c5c8565369abac65e50e31178`.

### What this session did, and why

- Ran the exit tests of PR-6 on `main` first. Test 1: `make` on a clean worktree of `origin/main` gave 363 tests, a clean format, and 0 findings. Test 2: the comparison with `ref=origin/main` printed "The live ruleset matches the file." with five required checks, `review-gate` included. Auto-merge is on, and the label `review-override` exists.
- Added `docs/roadmaps/phase-1-engine-proof.md`, the focused roadmap of phase 1, in the form of the-thing-below (D-70).
- The owner answered nine questions: D-69 to D-77. The Mac had no engine, no Xcode app, and no Git LFS, so step 9 of section 8 was not done. D-69 moves the install after PR-8 (F-19).
- Phase 1 has four PRs (D-71): PR-8 toolchain, PR-9 scaffold with the first headless test, PR-10 package, PR-11 first M-9 and the gate record.
- Updated section 8 and the phase 1 entry of `docs/design.md`, the index of `docs/roadmaps/`, and the design-doc-style skill.
- The owner told the session to add the `review-override` label each time (D-76). The agent files and the review skills now say so. PR-8 corrects the text of the tools and the live label (D-77, F-20).

### The state of the build

- `make` passes on the Mac: 363 tests, a clean format, and 0 findings of ste-check.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- Exit test 3 of PR-6 passed: `review-gate` was red on the first head `847ad04`, with "Found: no record".
- Exit test 4 of PR-6 passed: after the session added `review-override` at 19:42 UTC, `review-gate` turned green on `47e8203`. It read 15 changed paths, each in the documents set (D-65, D-66, D-76).
- `make codex-review` refuses this PR, because it changes documents alone (D-49).
- The five required checks are green. The owner confirms the squash merge.

### Traps and gotchas

- A new phase file defines its PR ids only after `git add`, because ste-check reads tracked files.
- The phase file heading form `### 7.N PR-#:` is the form that the reference check reads.
- Add the label only after the last commit outside the metadata set (D-65).

### The questions that block progress

- None. OQ-22 and OQ-23 are resolved (D-74, D-75). OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-8. PR-8 has two concerns: the engine toolchain and the label text of D-77.

## Session 28: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-6, merge. Repository: iron-absolution. Branch: `feat/pr-6-review-gate`. PR: #7. Role: author. Base: `5871f2c870d5eb110eccbadd67c5f633c5619155`.

### What this session did, and why

- Review round 2 approved the effective head `2338304` with the verdict `Ready for owner merge`. The two rounds used the exit codes 10 and 0.
- P2-1 has the status `accepted risk, D-68.` No finding stays open.

### The state of the build

- The effective head is `2338304`. `make` passes on the Mac: 363 tests, a clean format, and 0 findings of ste-check.
- The four checks of the head are green. PR #7 gets no `review-gate` check, because GitHub runs that workflow only from `main` (D-67).
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The owner confirms the squash merge of PR #7. Auto-merge is off, so the session runs `gh pr merge 7 --squash` after the confirmation.

### Traps and gotchas

- The live ruleset requires four checks until the owner instructs the change of D-67.
- The label `review-override` does not exist yet, and auto-merge is off. Both are owner steps of `docs/runbooks/main-ruleset.md`.

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, on the explicit instruction of the owner, apply the ruleset file of `main` to the live ruleset, and run the comparison (D-67). Then write the transitional prompt of PR-7.

## Session 27: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-6, round 2. Repository: iron-absolution. Branch: `feat/pr-6-review-gate`. PR: #7. Role: reviewer. Base: `5871f2c870d5eb110eccbadd67c5f633c5619155`.

### What this session did, and why

- Rechecked P2-1 against the author response and D-68. The owner accepts the backdated commit risk.
- Verified that commit `2338304` changes no behavior. It adds the risk rationale to the rule comment and records the owner decision.
- Updated `docs/reviews/pr-7.md`. P2-1 now records the accepted risk, and the round 1 verdict stays in the history.

### The state of the build

- The effective head is `2338304d6a448277f96270caaf90ee7213df6106`.
- `make` passes on the Mac: 363 tests, a clean format, and 0 findings of ste-check.
- The focused review-gate command tests pass: 17 tests. All four available hosted checks pass.
- The remote head is the commit of this entry. The state is ready for owner merge.

### What is in flight

- The owner reads the merge summary and confirms the merge of PR #7.

### Traps and gotchas

- PR #7 has no `review-gate` check. D-67 defers its GitHub proof to the first PR after this one reaches `main`.
- Run filtered MTP tests with `dotnet test --solution IronAbsolution.slnx --no-build --filter-class <type>`. Put the filter option before no extra `--` separator.

### The questions that block progress

- None blocks this review. OQ-16 still holds gitar under D-7.

### The next concrete action

- Give the owner the merge summary for PR #7. Wait for the owner's merge confirmation.
- After the merge, apply the ruleset file of `main` only on the explicit instruction of the owner, then run the comparison (D-67).

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

- The deferral rule of `doc-gate` reads the PR description and this entry. Its regular expressions match a document verb close to the words that name the time of the merge.
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
