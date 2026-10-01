## Session 69: 2026-10-01, Claude Code

Author: Claude Code
Session: author PR-24, round 1. Repository: iron-absolution. Branch: `feat/pr-24-mantle-interact`. PR: #25. Role: author. Base: `a32d9904eb465f81946988a0c08272a70b656b84`.

### What this session did, and why

- Built section 7.4 of `docs/roadmaps/phase-3-core-feel.md`: the mantle and interact. The owner gave D-143 to D-150.
- The mantle: `UIronCharacterMovementComponent` adds a custom mode. In the air, a forward move into a ledge from 50 cm to 130 cm above the feet starts a climb of 0.4 s (D-143).
- Interact: the interface `IIronInteractable`, with `AIronDoor` and `AIronSwitch`. The E key uses a target up to 200 cm from the eye (D-144, D-146).
- The cue: an outline before the bloom and an overlay glow, only while a target is in reach (D-145, D-148).
- The owner put two more changes in this PR and set G-7 aside: no motion blur (D-149), and Escape closes the game (D-150).
- D-147 records the animation of the arms for a later PR.
- The owner asked for a PowerShell command `iron-absolution` that starts the package. It is in the profile of the owner, not in the repository.

### The state of the build

- The remote head is the commit of this entry on `feat/pr-24-mantle-interact`, pending merge.
- `verify`, `toolchain-check`, `editor-build`, `content-build`, and `package-build` pass. `editor-test` passes 29 tests headless.
- `package-run` passes. `frame-capture` gives a mean of 3.33 ms and a 99th percentile of 3.64 ms at 2560x1440.
- The owner played the package on 2026-10-01. Each exit test passed.

### What is in flight

- The Codex review of PR #25.

### Traps and gotchas

- `frame-capture` needs a desktop at 2560x1440 (D-137, D-138). The first capture of this session ran at 1920x1080 and failed its settings check.
- The game target builds the automation tests with no editor-only data. A test that reads a material graph needs `WITH_EDITORONLY_DATA`.
- The test of the quit key reads the binding alone, because a quit closes the editor that runs the tests.
- `content-build` rewrites each scripted asset. Run it after each change of `Game/Scripts/build_content.py`, and commit the assets.

### The questions that block progress

- None.

### The next concrete action

- Run the review loop of PR #25 with `run.ps1 codex-review -PR 25`, and answer each finding.

## Session 68: 2026-09-30, Codex

Author: Codex
Session: reviewer PR-24, round 1. Repository: iron-absolution. Branch: `feat/pr-23-aim-settings`. PR: #24. Role: reviewer. Base: `aa9fc2a2556815dc4ef2f6b55f1f17abd1892940`.

### What this session did, and why

- Reviewed the aim settings of PR-24, from the implementation head `f2a27b2` (D-139 to D-142).
- Found no defect. Wrote `docs/reviews/pr-24.md` with the verdict and the evidence.

### The state of the build

- The reviewed work head is `f2a27b220ab0e96646a7a27ae6313a127fd38f96`. The remote head is the metadata commit of this entry on `feat/pr-23-aim-settings`.
- `run.ps1 verify`, `editor-build`, `editor-test`, and `content-build` pass. The package and play-test evidence comes from the owner comment.
- The coverage job failed once in `PackageRunCommandTests.TheSuccessLineOnStdoutAloneDoesNotPass`: it saw the package path, not the expected error line. Its one rerun passed (F-28).

### What is in flight

- PR #24 is pending merge. The review record approves the implementation head.

### Traps and gotchas

- `content-build` rewrites the input mapping and gym assets. The review restored those generated changes.
- `package-run` and `frame-capture` open a game window. D-96 needs owner confirmation before a session runs them.

### The questions that block progress

- None.

### The next concrete action

- Complete the PR gate with the review record and the five green required checks.

## Session 67: 2026-09-30, Claude Code

Author: Claude Code
Session: author PR-23, round 1. Repository: iron-absolution. Branch: `feat/pr-23-aim-settings`. PR: #24. Role: author. Base: `aa9fc2a2556815dc4ef2f6b55f1f17abd1892940`.

### What this session did, and why

- Added the aim settings of section 7.3 of the phase 3 roadmap: the class `UIronGameUserSettings`, the mouse sensitivity, the field of view, and two console commands.
- The owner gave D-139 to D-142. The sensitivity uses the Quake scale, with a default of 2.0 and bounds of 0.1 to 20. The field of view has a default of 100 and bounds of 80 to 120. The game has no vertical invert (D-140).
- Turned off the input scales of the engine, and set the mouse axis sensitivity to 1. The mapping context has no Negate on the mouse now.
- Removed the field of view from the movement tuning. The views of the capture read the project default.
- Added 6 automation tests. The owner played the package: each command worked, 25 gave the error, and each value came back after a restart.

### The state of the build

- The remote head is the commit of this entry on `feat/pr-23-aim-settings`, pending merge.
- On Windows: verify, editor-build, content-build, 19 editor tests, package-build, package-run, and frame-capture pass. M-3: mean 3.33 ms, 99th percentile 3.66 ms.

### What is in flight

- PR #24, pending merge: the Codex review.

### Traps and gotchas

- Run one `run.ps1` target at a time. Two targets at the same time lock the tools DLL.
- The aim tests write the settings file of the editor, and put back the values at the end.
- `package-build` removes the `Saved` folder of the package, with its logs and its settings file. Copy a play-test log before a rebuild.

### The questions that block progress

- None.

### The next concrete action

- Run `run.ps1 codex-review -PR 24`, and answer the findings.

## Session 66: 2026-09-30, Codex

Author: Codex
Session: reviewer PR-22, round 2. Repository: iron-absolution. Branch: `feat/pr-22-frame-time-capture`. PR: #23. Role: reviewer. Base: `09633599fc73f3c93e52535830c78b852a120cf4`.

### What this session did, and why

- Reviewed PR #23 at work head `09ac7904991b5c6cb301e5cfd82605bbbd011a1c`.
- Verified the two fixes and their regression tests. P2-1 and P2-2 are fixed. The Linux CI issue is F-28, and its required rerun passed.
- Updated the test count in the PR description to 46 C# tests.
- `run.ps1 verify` passes on Windows with 598 tests, format, and STE checks.
- The review record gives `Ready for owner merge` for the current work head.

### The state of the build

- The reviewed work head is `09ac7904991b5c6cb301e5cfd82605bbbd011a1c`.
- The metadata commit of this entry and the review record is the remote head on `feat/pr-22-frame-time-capture`.
- The current GitHub CI code checks pass. The review-gate check awaits this record.

### What is in flight

- PR #23, pending merge: the merge summary and owner confirmation.

### Traps and gotchas

- Do not run `frame-capture` without owner confirmation. It opens a game window (D-96).
- F-28 caused three short CI failures on this PR. Each failed job passed on its required rerun.

### The questions that block progress

- None.

### The next concrete action

- Give the owner the merge summary for PR #23 after the review-gate check passes.

## Session 65: 2026-09-30, Claude Code

Author: Claude Code
Session: author PR-22, round 2. Repository: iron-absolution. Branch: `feat/pr-22-frame-time-capture`. PR: #23. Role: author. Base: `09633599fc73f3c93e52535830c78b852a120cf4`.

### What this session did, and why

- Answered the review of `13bdbb1` in `docs/reviews/pr-23-response.md`.
- P1-1, partial merit: the Linux failure is F-28. The rerun passed. The F-28 row of `docs/design.md` records each failure of PR #23.
- P2-1, full merit: the reader needs `[HasHeaderRowAtEnd],1`.
- P2-2, partial merit: an absent or invalid view count now fails. The session refuted a second copy of "4" in C#, because the content script and the gym views test hold the set (D-134).
- Six new test cases fail on the old code and pass on the new code.

### The state of the build

- The new work head is the commit of this entry on `feat/pr-22-frame-time-capture`. `verify` passes on Windows with 598 tests.
- The game code, the content, and the value of M-3 do not change. The change is in the C# rules alone.

### What is in flight

- PR #23, pending merge: round 2 of the Codex review, then the merge summary.

### Traps and gotchas

- F-28 hit PR #23 three times, the third in the `coverage report` job of `09ac790`. Run each failed job again one time, and record it here.

### The questions that block progress

- None for PR-22.

### The next concrete action

- When each check is green, run `run.ps1 codex-review -PR 23`.

## Session 64: 2026-09-30, Codex

Author: Codex
Session: reviewer PR-22, round 1. Repository: iron-absolution. Branch: `feat/pr-22-frame-time-capture`. PR: #23. Role: reviewer. Base: `09633599fc73f3c93e52535830c78b852a120cf4`.

### What this session did, and why

- Reviewed PR #23 at effective head `13bdbb1c6cd187e867b644be9afaf5c4ec95db2c`.
- Recorded P1-1 for a failing Linux CI test, P2-1 for an incomplete CSV marker, and P2-2 for the missing four-view check.
- `run.ps1 verify` passed with 592 tests, format, and STE checks.
- The review record gives `Changes required`. A metadata update records the post-push CI failure.

### The state of the build

- The effective work head is `13bdbb1c6cd187e867b644be9afaf5c4ec95db2c`.
- The remote PR tip is the metadata commit on `feat/pr-22-frame-time-capture`.
- GitHub build, coverage, doc-gate, and ste-check pass after a rerun. Review-gate rejects the verdict `Changes required`.

### What is in flight

- The author must answer both findings.
- The author must fix the Linux CI test failure.

### Traps and gotchas

- Do not run `frame-capture` without owner confirmation. It opens a game window (D-96).

### The questions that block progress

- None.

### The next concrete action

- The author answers P1-1, P2-1, and P2-2, then requests a repeat review.

## Session 63: 2026-09-30, Claude Code

Author: Claude Code
Session: author PR-22, round 1. Repository: iron-absolution. Branch: `feat/pr-22-frame-time-capture`. PR: #23. Role: author. Base: `09633599fc73f3c93e52535830c78b852a120cf4`.

### What this session did, and why

- Ran `verify` (552 tests) and `toolchain-check` (5 pins) on `main` before any change.
- Asked OQ-25. The owner picked the Development package, a fixed set of views, the mean and the 99th percentile, and borderless fullscreen with no cap (D-137).
- The owner wants the game borderless fullscreen (D-138). The engine default already gives it, so the owner asked for no change.
- Added `AFrameTimeView`, `UFrameTimeCaptureSubsystem`, 4 views in the content script, 4 automation tests, the `frame-capture` command with 40 new C# tests, and `run.ps1 frame-capture`.
- First value of M-3: a mean of 3.33 ms and a 99th percentile of 3.67 ms, inside 8.33 ms. Section 4 of `docs/design.md` and `docs/research/frame-time-method.md` give the settings.

### The state of the build

- The effective head is `13bdbb1`. A documents commit follows it. The remote head is the tip of `feat/pr-22-frame-time-capture`.
- On the Windows PC: `verify` passes with 592 tests. `editor-build`, `editor-test` (13 tests), `content-build`, `package-build`, `frame-capture`, and `package-run` pass.

### What is in flight

- PR #23, pending merge: the Codex review, then the merge summary.

### Traps and gotchas

- Close each other game before `frame-capture`. A game in the background gave a mean of 7.36 ms, and that capture is void.
- The CSV profiler adds columns during a capture, and the place of `FrameTime` changes between captures. The reader uses the closing header and the column name.
- `content-build` writes `IMC_KeyboardMouse` again with new ids. This PR restored it.
- F-28 on the first push: `EditorTestCommandTests.AStubRunThatWritesTheReportAndTheSuccessLinePasses` failed after 9 ms in the `coverage report` job. The session ran the failed job again one time.
- A Python heredoc through the Bash tool halves each backslash. Check each Windows path after such an edit.

### The questions that block progress

- None for PR-22. OQ-16 stays out of each PR (D-7).

### The next concrete action

- Run `run.ps1 codex-review -PR 23`, answer the findings, then give the owner the merge summary.

## Session 62: 2026-09-29, Codex

Author: Codex
Session: reviewer PR-21, round 3. Repository: iron-absolution. Branch: `feat/pr-21-gym-movement`. PR: #22. Role: reviewer. Base: `ce25cfd4982d5d4be2c0eee0a0a2658f419b182f`.

### What this session did, and why

- Reviewed PR #22 at effective head `7e2e3f73c1c17b50c34505d7c5ce6d6c0cfc7408`.
- Verified the owner play test in the package and editor from the PR comment. The owner reports movement, jump, and aim pass.
- Findings P2-1 and P2-2 remain fixed. The review record now gives `Ready for owner merge`.
- The metadata commit is pushed. All five required checks pass, including `review-gate`.

### The state of the build

- The remote PR tip is `40a64015a9c16c7b1a6ff0f673ff3408e32129dd` on `feat/pr-21-gym-movement`.
- The effective code head remains `7e2e3f73c1c17b50c34505d7c5ce6d6c0cfc7408`.

### What is in flight

- The owner merge summary and merge decision.

### Traps and gotchas

- The `review-gate` result before the metadata push reflects the prior Blocked record.
- OQ-25 blocks roadmap PR-22, not GitHub PR #22.

### The questions that block progress

- No open question affects GitHub PR #22. The owner play test now passes.

### The next concrete action

- Give the owner the merge summary, then wait for the merge decision.

## Session 61: 2026-09-29, Codex

Author: Codex
Session: reviewer PR-21, round 2. Repository: iron-absolution. Branch: `feat/pr-21-gym-movement`. PR: #22. Role: reviewer. Base: `ce25cfd4982d5d4be2c0eee0a0a2658f419b182f`.

### What this session did, and why

- Reviewed PR #22 at work head `7e2e3f73c1c17b50c34505d7c5ce6d6c0cfc7408`.
- Verified that P2-1 and P2-2 pass the new regression test. The review record keeps both findings and their first-round evidence.
- Ran `run.ps1 verify`, `run.ps1 editor-build`, and `run.ps1 editor-test`. Each passed. All 9 editor tests passed.
- The review record gives the verdict `Blocked`. The owner's play test in the gym still waits for exit test 1.

### The state of the build

- The remote work head is `7e2e3f73c1c17b50c34505d7c5ce6d6c0cfc7408` on `feat/pr-21-gym-movement`.
- The local .NET gates, editor build, and editor tests pass.

### What is in flight

- The metadata commit for review round 2.
- The owner play test of the gym in the editor and package.

### Traps and gotchas

- The first editor test failed because the worktree had no compiled editor module. Build the editor target before the test.
- The `review-gate` fails until the review record approves the effective head.

### The questions that block progress

- No open question affects this PR. The owner play test remains required exit evidence.

### The next concrete action

- Push the review record and this entry together. The owner runs the play test before the merge.

## Session 60: 2026-09-29, Claude Code

Author: Claude Code
Session: author PR-21, round 2. Repository: iron-absolution. Branch: `feat/pr-21-gym-movement`. PR: #22. Role: author. Base: `ce25cfd4982d5d4be2c0eee0a0a2658f419b182f`.

### What this session did, and why

- The Codex review of round 1 gave `Blocked` with P2-1 and P2-2. Both have full merit. `docs/reviews/pr-22-response.md` gives each answer.
- P2-1: `ApplyMovementTuning` now checks the gravity before any write, so a refused tuning changes nothing (T-2).
- P2-2: `FindInvalidValues` now rejects each value that is not finite.
- The new test `IronAbsolution.Player.Movement.RefusedTuning` fails on the old code and passes with the correction.
- The `coverage report` job failed one time in a test of `ToolchainCheckCommandTests`, and its rerun passed. F-28 has a dated line for it.
- Round 2 of the review marked P2-1 and P2-2 fixed at `7e2e3f7`. It stayed `Blocked` for exit test 1 alone.
- Exit test 1 passes. With owner consent, the package at `7e2e3f7` opened in `L_Gym`, then the editor. The owner played both and reported "All good".

### The state of the build

- The remote head is the commit of this entry on `feat/pr-21-gym-movement`, pending merge.
- `editor-test` passes 9 automation tests. `run.ps1 verify` passes.

### What is in flight

- Round 3 of the Codex review of PR #22, with the evidence of exit test 1.

### Traps and gotchas

- Session 58 lists the traps of this PR. They all still hold.
- `TNumericLimits<float>` has no NaN in 5.8. The test uses `std::numeric_limits<float>::quiet_NaN()`.

### The questions that block progress

- None for PR-21. OQ-25 blocks PR-22, and OQ-26 blocks the sound part of PR-25. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- Read the verdict of round 3, then give the merge summary.
