## Session 35: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-9, round 1. Repository: iron-absolution. Branch: `feat/pr-9-project-scaffold`. PR: #10. Role: author. Base: `830bf40cd47f8f707d060a894017c29bff446566`.

### What this session did, and why

- Added the Unreal project in `Game/` from the Blank C++ template of 5.8.3: one module, the Game and Editor targets, and the test `IronAbsolution.Project.Settings` (D-73).
- The owner made the empty map `L_Test` with no World Partition (D-84). Git LFS stores it (D-86).
- Added `make editor-build`, `make editor-test`, and two PowerShell scripts (D-72). A pass needs the exit code 0, a report with passed tests alone, and the success line.
- The owner answered D-84 to D-88. F-23: Zen starts before Editor Preferences, so the owner wrote the cache path first (D-85). F-24: the check passed with no Metal Toolchain, so the check has a fourth pin (D-87). D-88 turns off the Android File Server, which wrote a token.
- Mac evidence: a fresh clone builds in 35.5 s (M-2) and restores the map from LFS. The test passes headless. A test that failed on purpose gave the exit code 3, and the session removed it. M-1 is 6.42 GB. The internal disk holds the Zen programs alone.

### The state of the build

- `make` passes: 494 tests, a clean format, and 0 findings. The script tests ran under a portable pwsh 7.5.3 in the scratchpad.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- Windows evidence: the owner ran both scripts in a fresh clone at `C:\dev\iron-absolution`. The build took 45.3 s (M-2), and each check of the test passed. The PR comments hold both logs.
- The review of `make codex-review PR=10` is next.

### Traps and gotchas

- A shell of the harness does not read `~/.zshrc`. Export `IRON_ABSOLUTION_ENGINE_DIR` in each engine command.
- The old clone on `/Volumes/SSD-1TB` has the same name. Run `pwd` first.
- The report `index.json` starts with a UTF-8 byte order mark.
- `GetGameDefaultMap` gives the package name, with no object name.

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- Read the verdict of the review, and answer each finding with the review-response skill.

## Session 34: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-8, merge. Repository: iron-absolution. Branch: `feat/pr-8-engine-toolchain`. PR: #9. Role: author. Base: `2133abdbd714b3ba2dddf916adc1aef2b693a120`.

### What this session did, and why

- Round 2 of the review gave `Ready for owner merge` for the effective head `bc930a946d214c805ac5eefc77a31d4599fc5625`, with no open finding.
- The five required checks are green on `743e61d`, `review-gate` included. No review thread is open.

### The state of the build

- `make` passes on the Mac: 405 tests, with 7 skipped where PowerShell is absent. CI ran 405 of 405 with none skipped.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The owner confirms the squash merge after the merge summary. Then auto-merge runs (D-67).

### Traps and gotchas

- This entry is a metadata commit, so the approval of `bc930a9` stands (D-14).

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-9. Work from the clone on the `IronAbsolution` volume (D-81). Set the cache path at the first editor start (D-83).

## Session 33: 2026-09-27, Codex

Author: Codex
Session: reviewer PR-8, round 2. Repository: iron-absolution. Branch: `feat/pr-8-engine-toolchain`. PR: #9. Role: reviewer. Base: `2133abdbd714b3ba2dddf916adc1aef2b693a120`.

### What this session did, and why

- Reviewed the author response and the changes since the first review.
- Verified both findings as fixed in `bc930a9`. The script tests cover malformed and absent engine version files. The PR-9 exit tests use unique numbers 1 to 11.
- Updated `docs/reviews/pr-9.md` for effective head `bc930a946d214c805ac5eefc77a31d4599fc5625`.

### The state of the build

- `make` passed with portable PowerShell: 405 tests, no skips, clean format, and 0 findings of ste-check.
- The remote head is the metadata commit of this entry. PR #9 is pending merge. The fresh `review-gate` result needs verification.

### What is in flight

- Read `gh pr checks 9` after the metadata push. Confirm that `review-gate` passes for this record.
- The owner confirms the squash merge after the merge summary.

### Traps and gotchas

- The owner posted the Windows pin results in the PR comment. The Windows run remains owner-reported evidence.
- The cache-path exit test belongs to PR-9 (D-83). OQ-16 keeps gitar outside this review (D-7).

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- Verify the published checks, then give the owner the merge summary if all required checks pass.

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
