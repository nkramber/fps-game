## Session 43: 2026-09-28, Codex

Author: Codex
Session: reviewer PR-13, round 1. Repository: iron-absolution. Branch: `feat/pr-13-windows-dev-tools`. PR: #13. Role: reviewer. Base: `2509d791d1769c7602fc802eb78158fd0c28dff6`.

### What this session did, and why

- Reviewed PR #13 at effective head `989a50592a924761358d775ed94a5f43918e1bd5` under D-14.
- The provider gate passed. No blocking defect was found.
- Added the review record and this entry as one metadata commit for `origin/feat/pr-13-windows-dev-tools`.

### The state of the build

- `run.ps1 verify` passed on Windows: build, 540 tests, no skips, format, and ste-check with 0 findings.
- The remote head will be the metadata commit of this entry. The review gate must rerun after the push.

### What is in flight

- The owner reads the review, confirms the merge, then merges PR-13.

### Traps and gotchas

- The first `review-gate` run failed because the review record was absent. The other four required checks passed.
- PR-14 removes the remaining Mac engine commands and paths (D-94, D-97).

### The questions that block progress

- None. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt for PR-14: the engine commands on Windows alone, and the removal of each Mac part.

## Session 42: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-13, round 1. Repository: iron-absolution. Branch: `feat/pr-13-windows-dev-tools`. PR: #13. Role: author. Base: `2509d791d1769c7602fc802eb78158fd0c28dff6`.

### What this session did, and why

- This is the first session on the Windows PC (D-92). `run.ps1` is the new entry of each development command (D-99). The Makefile keeps the Mac engine targets until PR-14.
- The owner answered three questions at the start. They are D-101 to D-103: PowerShell 7 as an owner step, the program path of the platform, and the stub program of the tests.
- 27 tests failed on Windows before this PR. Two were real defects: a forward-slash constant into `Path.Combine` gave a mixed-separator path in `handoff-rotate` and in the Codex transcript. `ToolPaths.UnderFolder` now holds that rule one time.
- 25 tests threw `PlatformNotSupportedException` and cited D-55, which D-92 superseded. Each stub of a program of the engine is now `IronAbsolution.TestStub`, so Windows can start it (D-103).
- `EntryScriptTests` covers each start condition of the entry: an unknown target, a missing `-PR`, an absent .NET SDK, an absent git, and an absent npm.
- 28 files that named a `make` development target now name the entry target. The runbook lists each owner install step.

### The state of the build

- `run.ps1 verify` passes on the Windows PC: build, 540 tests, format, and ste-check with 0 findings. No test is skipped. 527 tests ran before, with 27 failures and 13 skips.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The Codex review of PR-13 (D-14), then the merge summary and the merge confirmation of the owner.

### Traps and gotchas

- The Bash tool of this harness eats a backslash in a quoted heredoc. Write a file that needs a backslash escape with the Write tool or the Edit tool, and not with a heredoc.
- The stub program runs because the host of .NET looks for its own assembly name, and not for its own file name. So a copy under another name still finds `IronAbsolution.TestStub.dll` beside it.
- `ToolchainScriptTests` points `ProgramFiles(x86)` at an empty folder. Without it, the Visual Studio pin passes on a PC that has Visual Studio, and the test fails.
- Ask the owner before each command that opens a game window (D-96).

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-14: the engine commands on Windows alone, and the removal of each Mac part (D-97).

## Session 41: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-12, round 1. Repository: iron-absolution. Branch: `docs/pr-12-windows-only-plan`. PR: #12. Role: author. Base: `7f87dbc5b0313eddae55140b027bb7e72aed58ad`.

### What this session did, and why

- Recorded the three answers of PR-10 as D-91 to D-93: Windows alone, the sessions on the Windows PC, and PR-12 before PR-11.
- Listed each Mac part, then asked the owner. The answers are D-94 to D-100: three PRs, M-9 retired, the window confirmation, the Mac removal, M-1 on Windows, the PowerShell entry, and the Windows paths.
- Marked D-55, D-75, D-81, D-83, D-85, D-87, and D-90 as superseded, and D-14, D-28, D-32, D-33, D-41, D-71, and D-72 as revised in part. Each live citation names the new decision.
- The phase 1 file has PR-12 to PR-14, the new PR-11, the gate for Windows alone, and the new order. The PR-14 entry holds the inventory of Mac parts.
- The design doc, G-12, the agent files, the `pr-review` skill, and two runbooks follow D-91 and D-92.

### The state of the build

- `make ste-check` gives 0 findings on the Mac. The PR changes documents alone.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The author session adds the `review-override` label (D-35, D-76). Then the owner confirms the squash merge after the merge summary.

### Traps and gotchas

- The reference check flags each line that cites a superseded decision and does not name its successor (REF 3). Each line that PR-13 or PR-14 keeps with a citation of D-55, D-75, or D-81 to D-90 must name the successor.
- The Makefile stays for the Mac engine targets until PR-14 (section 7.6 of the phase file). PR-13 removes only the development targets.
- Ask the owner before each command that opens a game window (D-96).

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-13. The session of PR-13 runs on the Windows PC (D-92). Its first action is the owner install of the development tools there.

## Session 40: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-10, merge. Repository: iron-absolution. Branch: `feat/pr-10-packaged-build`. PR: #11. Role: author. Base: `b46321ed8a4909c36e7c305a0e07af859c7320bb`.

### What this session did, and why

- Round 1 of the review gave `Ready for owner merge` for the effective head `5d268b707becefc0101fd9814faa9ec51d12d335`, with no finding.
- Each check of the PR passed after the review record, `review-gate` included.

### The state of the build

- `make` passes on the Mac: 527 tests, a clean format, and 0 findings.
- The remote head is the commit of this entry. The state is pending merge.

### What is in flight

- The owner confirms the squash merge after the merge summary. Then auto-merge runs (D-67).

### Traps and gotchas

- This entry is a metadata commit, so the approval of `5d268b7` stands (D-14).

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- After the merge, the next PR is PR-12, and it comes before PR-11. On 2026-09-28 the owner gave two answers that no D-# id holds yet:
  1. "We will ONLY support Windows, not Mac." This supersedes the macOS part of D-32.
  2. The engine work moves to the Windows PC, and the sessions run there. This reverses D-55 and revises D-33 in part.
- PR-12 records both answers as decisions, and it plans the change first. The Mac pins, the Mac targets, and the Mac M-9 of PR-11 change.

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
