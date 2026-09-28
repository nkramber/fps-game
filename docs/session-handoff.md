## Session 39: 2026-09-28, Codex

Author: Codex
Session: review PR-10, round 1. Repository: iron-absolution. Branch: `feat/pr-10-packaged-build`. PR: #11. Role: reviewer. Base: `b46321ed8a4909c36e7c305a0e07af859c7320bb`.

### What this session did, and why

- Reviewed PR #11 at effective head `5d268b707becefc0101fd9814faa9ec51d12d335` under D-14.
- The provider gate passed. The review found no blocking defect.
- Added `docs/reviews/pr-11.md` and this entry in one metadata commit for `origin/feat/pr-10-packaged-build`.

### The state of the build

- The CI build, test, and format, coverage, STE, and document gate checks passed. The review gate failed before the review record existed.
- Local `make` passed. It skipped 28 PowerShell tests because `pwsh` was absent. CI ran those tests.
- The owner posted Mac and Windows package logs and run results in PR #11.
- The remote work head before the metadata commit was `2cf6a526ae61d5d5608d9fa59aa224de4e2f2f2c`.

### What is in flight

- The metadata commit must pass the review gate at the effective head.

### Traps and gotchas

- The tools project does not install PowerShell. CI runs the PowerShell script tests.
- The Unreal Engine build and package need the owner machine and engine cache.

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- The author gives the owner the merge summary after all required checks pass.

## Session 38: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-10, round 1. Repository: iron-absolution. Branch: `feat/pr-10-packaged-build`. PR: #11. Role: author. Base: `b46321ed8a4909c36e7c305a0e07af859c7320bb`.

### What this session did, and why

- Before the work, the `ci` run of `main` at `b46321e` passed (exit test 9 of PR-9). `make toolchain-check` and `make editor-test` passed.
- `make package-build`, `make package-run`, and the two Windows scripts. The game subsystem `UTimedRunSubsystem` runs the first map for a set time and writes the success line.
- The owner gave D-89 (10 seconds, a limit of 5 minutes) and D-90 (the bundle id `com.nkramber.ironabsolution`).
- The first package runs found F-25 (the PR-9 test in the game target), F-26 (the `-package` step of the Mac), and F-27 (the App Sandbox stops `-abslog`).

### The state of the build

- `make` passes on the Mac: 527 tests, a clean format, and 0 findings. CI ran the PowerShell tests with 0 skips.
- A fresh clone passed `make package-build` (136.5 s) and `make package-run` on the Mac. The Windows scripts passed in the checkout of the owner (105.6 s). The PR comments "Mac evidence" and "Windows evidence" hold the output.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The review of round 1 by `make codex-review PR=11`.

### Traps and gotchas

- The Mac package writes its log in the container `~/Library/Containers/com.nkramber.ironabsolution`. `make package-run` reads stdout.
- The owner gives Windows output as text in the chat. The session posts it in the PR.

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, the next PR is not PR-11. The owner changed the scope on 2026-09-28, and no D-# id holds it yet. The next session records these two answers as decisions:
  1. "We will ONLY support Windows, not Mac." This supersedes the macOS part of D-32.
  2. The engine work moves to the Windows PC, and the sessions run there. This reverses D-55 and revises D-33 in part.
- That PR takes the id PR-12, and it comes before PR-11. It plans the change first. The Mac pins, the Mac targets, and the Mac M-9 of PR-11 change.

## Session 37: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-9, merge. Repository: iron-absolution. Branch: `feat/pr-9-project-scaffold`. PR: #10. Role: author. Base: `830bf40cd47f8f707d060a894017c29bff446566`.

### What this session did, and why

- Round 1 of the review gave `Ready for owner merge` for the effective head `124e1ae1e449af46cceaadc475a17f3510b9b669`, with no finding.
- The record notes a blank line at the end of `Game/Config/DefaultInput.ini`, from the template of Epic. No check reads it, so the file stays.

### The state of the build

- `make` passes on the Mac: 494 tests, a clean format, and 0 findings. CI ran the PowerShell tests.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The owner confirms the squash merge after the merge summary. Then auto-merge runs (D-67).

### Traps and gotchas

- This entry is a metadata commit, so the approval of `124e1ae` stands (D-14).

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-10, the packaged build. Work from the clone on the `IronAbsolution` volume (D-81).

## Session 36: 2026-09-28, Codex

Author: Codex
Session: reviewer PR-9, round 1. Repository: iron-absolution. Branch: `feat/pr-9-project-scaffold`. PR: #10. Role: reviewer. Base: `830bf40cd47f8f707d060a894017c29bff446566`.

### What this session did, and why

- Reviewed the full PR-9 diff and its contracts. No finding holds at effective head `124e1ae1e449af46cceaadc475a17f3510b9b669`.
- Added the review record for PR #10. The provider gate passes under T-4 and D-6.

### The state of the build

- `make` passed: 474 tests passed, 20 PowerShell tests skipped because `pwsh` is not installed, formatting passed, and ste-check reported 0 findings.
- CI passed build, test, and format; coverage; doc-gate; and ste-check on remote head `f3f015537d8c0e5da54b765dbdcae23fd599f236`. The review-gate check failed because the review record did not exist yet.
- The Mac and Windows build and test results are in the owner comments. The Windows run is owner-reported.
- The PR state is pending merge. The metadata commit must reach `origin/feat/pr-9-project-scaffold`.

### What is in flight

- The review-gate check must run after the metadata commit.

### Traps and gotchas

- The local Mac cannot run the PowerShell tests. CI ran the full test set.
- `git diff --check` reports one extra blank line at EOF in `Game/Config/DefaultInput.ini`.

### The questions that block progress

- None. OQ-16 still holds the gitar pass under D-7.

### The next concrete action

- Read the review-gate result after the metadata push. If it passes, prepare the four-part merge summary for the owner.

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
