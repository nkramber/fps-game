# Session handoff archive

This file holds the entries that the rotation moves out of `docs/session-handoff.md`, newest first.

## Session 49: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-11, round 1. Repository: iron-absolution. Branch: `docs/pr-11-gate-record`. PR: #15. Role: author. Base: `95546e35317aea9fff1f56a2ec2e9c549eceff84`.

### What this session did, and why

- The `ci` run 36495084063 of `main` at `95546e3` passed. So exit test 6 of PR-14 holds after the merge.
- `run.ps1 verify` passed on the Windows PC before the work.
- The owner gave D-108: a session measures M-8 with `git lfs ls-files --all --json` in a fresh clone. M-8 is 8,404 bytes in one LFS object, the test map.
- The owner gave D-109: headless evidence proves line 5 of the gate, and no editor window opens.
- Section 7.9 of the phase file names the evidence of each line of the gate. The gate of phase 1 passes.
- The search for line 11 found two leftovers of PR-14 (F-29). The owner chose to fix them in this PR (D-110). `.editorconfig` loses its Makefile section, and the comment of `PowerShellScript.cs` names `run.ps1`.
- `EditorConfigTests` failed on the old `.editorconfig` with the name Makefile, and it passes after the fix.

### The state of the build

- `run.ps1 verify` passed: 539 tests and 0 STE findings.
- The remote head is the commit of this entry on `docs/pr-11-gate-record`, pending merge.
- The PR changes code, so it needs the Codex review. The `review-override` label does not apply (D-110).

### What is in flight

- Round 1 of `run.ps1 codex-review -PR 15`.

### Traps and gotchas

- M-8 does not count an LFS object that only a deleted branch held. GitHub counts it in the quota (D-108).
- The billing API of GitHub needs the `user` scope, and the token of `gh` does not have it.
- F-28 stays open. The hosted stub test can fail one time. Run the failed job again before a change.

### The questions that block progress

- None. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- Answer the Codex review of #15. After approval, give the owner the merge summary.
- After the merge, the next PR comes from section 8 of `docs/design.md`: the gate of phase 2, or F-28.

## Session 48: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-14, round 2 close. Repository: iron-absolution. Branch: `feat/pr-14-windows-engine-commands`. PR: #14. Role: author. Base: `0351bbba52a4fddd91b559ad7ace9f1bb020b2f0`.

### What this session did, and why

- Round 2 approved the effective head `aed74bb`, and the five checks passed at `9e426cb`.
- The owner read the merge summary and confirmed the merge.
- One hosted test failed one time before a clean rerun. This PR adds it to the design doc as F-28, on the choice of the owner. Its fix is a separate concern (G-7).

### The state of the build

- The effective head stays `aed74bb`, because this commit changes `docs/` alone (D-49).
- The remote head is the commit of this entry, pending merge through auto-merge.

### What is in flight

- The auto-merge of #14 after the checks of this commit.

### Traps and gotchas

- F-28: the stub of D-103 can fail to start on the hosted runner. The assert on the exit code hides the error text, so a fix first proves the cause.

### The questions that block progress

- None. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt for PR-11, the gate record. Name F-28 as work for a later PR.

## Session 47: 2026-09-28, Codex

Author: Codex
Session: reviewer PR-14, round 2. Repository: iron-absolution. Branch: `feat/pr-14-windows-engine-commands`. PR: #14. Role: reviewer. Base: `0351bbba52a4fddd91b559ad7ace9f1bb020b2f0`.

### What this session did, and why

- The review checked the response to round 1, D-107, and the final effective head, `aed74bb`.
- The provider gate passed. The review found no defect in scope.
- The local verification passed. The hosted test rerun passed after one test failed in the first attempt.

### The state of the build

- `run.ps1 verify` passed with 538 tests, no skips, format, and ste-check.
- The hosted build, test, and format, coverage, doc-gate, and ste-check jobs passed. `review-gate` awaits this record.
- The work head is `aed74bb2c76d0840a555772b86f82696d175c59f`. The remote head before this metadata commit is `efcf922d9034dea504f415bce68a269051439219`.
- This review record and handoff entry are pending push as one metadata commit.

### What is in flight

- The review approves effective head `aed74bb`. The PR is ready for the owner merge step after the checks read this record.

### Traps and gotchas

- The first hosted attempt had one failed test. The rerun passed, and local verification passed.
- `review/pr-14` has no upstream. Check the remote head with `gh pr view` after the push.

### The questions that block progress

- None. OQ-16 still holds the separate gitar pass (D-7).

### The next concrete action

- Push this metadata commit, check the remote head and `review-gate`, then give the owner the merge summary.

## Session 46: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-14, round 2. Repository: iron-absolution. Branch: `feat/pr-14-windows-engine-commands`. PR: #14. Role: author. Base: `0351bbba52a4fddd91b559ad7ace9f1bb020b2f0`.

### What this session did, and why

- Round 1 gave `Blocked` with no finding. It read D-96 as a rule for each build and test, and it recorded `f6f1d35` as the head.
- The owner confirmed that D-96 covers a window alone. D-107 records it.
- `docs/reviews/pr-14-response.md` answers both causes. The effective head stays `aed74bb`, because that commit changes three files under `Game/Source/` (D-49).

### The state of the build

- `run.ps1 verify` passed at `aed74bb`: 538 tests, no skips, format, and ste-check with 0 findings.
- Four hosted checks passed at `aed74bb`. `review-gate` waits for an approval record.
- The remote head is the commit of this entry, pending merge.

### What is in flight

- Round 2 of `run.ps1 codex-review -PR 14`, then the merge summary.

### Traps and gotchas

- Round 1 left its worktree in the temporary folder. Round 2 removes it through the worktree check of D-106.
- A commit of C++ comments moves the effective head, because `Game/` is outside the documents set (D-49).

### The questions that block progress

- None. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt for PR-11, the gate record.

## Session 45: 2026-09-28, Codex

Author: Codex
Session: reviewer PR-14, round 1. Repository: iron-absolution. Branch: `feat/pr-14-windows-engine-commands`. PR: #14. Role: reviewer. Base: `0351bbba52a4fddd91b559ad7ace9f1bb020b2f0`.

### What this session did, and why

- The review read the PR-14 diff, its focused roadmap, the owner decisions, the PR comment, and the hosted checks.
- The provider gate passed. The review found no defect in scope.
- Local verification awaits the owner's confirmation under D-96. The review record gives the blocked verdict until that check completes.

### The state of the build

- The owner reported that `run.ps1 verify` passed with 538 tests, no skips, format, and ste-check.
- The owner provided Windows evidence for the engine exit tests. The hosted build, test, and format, coverage, doc-gate, and ste-check jobs passed.
- The remote head before this metadata push was `aed74bb2c76d0840a555772b86f82696d175c59f`. The work head is `f6f1d35f122e7769f2733ba69cd6395459a8cafc`.
- The review record, this entry, and the rotated archive entry are pending push as one metadata commit.

### What is in flight

- The owner must confirm local verification under D-96. Then update the review record and rerun the review gate.

### Traps and gotchas

- `review/pr-14` has no upstream. Compare `git rev-parse HEAD` with the head from `gh pr view` after the push.
- `package-run` opens a game window. D-96 requires the owner's confirmation before that command.

### The questions that block progress

- The local build and test checks await the owner's confirmation under D-96.
- OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- Wait for the owner's answer about local verification. If confirmed, run `run.ps1 verify`, update the review record, and check `review-gate` again.

## Session 44: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-14, round 1. Repository: iron-absolution. Branch: `feat/pr-14-windows-engine-commands`. PR: #14. Role: author. Base: `0351bbba52a4fddd91b559ad7ace9f1bb020b2f0`.

### What this session did, and why

- Exit test 6 of PR-13 held: the five checks passed on the final head of #13, and `ci` passed on `main`.
- `run.ps1` got the five engine targets. `toolchain-check` reads the Windows pins in C#. It gives the same lines as the old script (D-104).
- The Makefile, `scripts/`, the Xcode and Metal pins, the Mac paths, and the Mac and Linux sections of `DefaultEngine.ini` went (D-97, D-105).
- The package gets `-abslog`, because the game is a program of the Windows subsystem.
- The worktree check of `codex-review` reads the forward slashes of git on Windows (D-106). The regression tests fail on the old code.
- The runbooks, the agent files, the README, and the evidence form of the PR template name the targets of the entry.
- The owner gave D-104 to D-106.

### The state of the build

- `run.ps1 verify` passed: 538 tests, no skips, format, and ste-check with 0 findings.
- A fresh clone passed exit tests 1 to 5 and 7 from the entry. The PR comment "Windows evidence" holds the output.
- M-1 is 3.53 GB. M-2 is 41.6 s for the editor and 101.25 s for the package.
- The remote head is the commit of this entry, pending merge.

### What is in flight

- The Codex review of #14, then the merge summary.

### Traps and gotchas

- The Bash tool of Claude Code turns a doubled backslash into one backslash, so `.\run.ps1` in a heredoc became a carriage return. Use the Edit tool for a backslash.
- The editor log has no `LoadMap` line. It writes `MAP LOAD FILE="...L_Test.umap"` when the map loads.
- Unreal on Windows quotes each argument again (`LaunchWindows.cpp`), so `ArgumentList` works for `-ExecCmds`.

### The questions that block progress

- None. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt for PR-11, the gate record.

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
