## Session 73: 2026-10-03, Claude Code

Author: Claude Code
Session: author PR-25, round 1. Repository: iron-absolution. Branch: `feat/pr-25-weapon`. PR: #26. Role: author. Base: `d2dfaa395dbeac6c5480da10fed6f881fe579d1b`.

### What this session did, and why

- Added the weapon rule, two data assets, the ammo, change weapon, the HUD, the gym target, the ammo station, the flash, the recoil, and the sounds of the gym (D-151 to D-168).
- Five play tests of the package changed the recoil, the spread, the reach of interact, and the sounds (D-159 to D-168).
- The owner removed the hit sound, because the hit marker is enough (D-167). The owner set the mix of the shots: the rifle 0.35, the scatter gun 1.0 (D-168).
- A test keeps each rifle shot quieter than a scatter shot.

### The state of the build

- The remote head is the head of `feat/pr-25-weapon`, pending merge.
- On the Windows PC: `verify`, `toolchain-check`, `editor-build`, `content-build`, 39 headless tests, `package-build`, and `package-run` pass.
- The owner played the last package. Exit tests 1 and 2 pass.
- M-3 on record: a mean of 3.33 ms and a 99th percentile of 3.65 ms, from the earlier package of this PR. The changes since then are sound alone. The last capture had another game open, and the owner kept the earlier values.

### What is in flight

- The cross-provider review of PR #26.

### Traps and gotchas

- Each sound asset holds its volume, and `Game/Scripts/build_content.py` sets it (D-29). Edit the script, not the asset.
- The rifle file and the scatter file have about the same level. The tails of a rifle burst overlap, so the rifle sounds louder.
- A frame capture with another program open is not a clean value of M-3.
- The token of freesound.org stays out of the repository (G-6).

### The questions that block progress

- None.

### The next concrete action

- Run `run.ps1 codex-review -PR 26`, and answer the findings with the review-response skill.

## Session 72: 2026-10-01, Codex

Author: Codex
Session: reviewer PR-25, round 2. Repository: iron-absolution. Branch: `feat/pr-24-mantle-interact`. PR: #25. Role: reviewer. Base: `a32d9904eb465f81946988a0c08272a70b656b84`.

### What this session did, and why

- Reviewed PR #25 at effective head `97ca8eea7de8013ea196c563c9cd3482248bea65`.
- Verified that the door rejects NaN and infinite offsets before state or transform changes (T-2).
- Verified the regression test checks both offsets and their error and state results.
- Updated P2-1 to fixed and set the verdict to Ready for owner merge.

### The state of the build

- The effective head is `97ca8eea7de8013ea196c563c9cd3482248bea65` on `feat/pr-24-mantle-interact`.
- The author reports that `verify`, editor build, 29 headless tests, and package build pass at that head.
- CI passes all five required checks after the metadata push. The first coverage run lost its NuGet connection. The rerun passed.
- This session did not run a build or test. Owner confirmation was not present (D-96).

### What is in flight

- Give the owner the merge summary after the required checks pass.
- Give the owner the merge summary after the required checks pass.

### Traps and gotchas

- The prior `review-gate` failure read the earlier verdict. The check passes after the record update.

### The questions that block progress

- None.

### The next concrete action

- Give the owner the merge summary after the required checks pass.

## Session 71: 2026-10-01, Claude Code

Author: Claude Code
Session: author PR-24, round 2. Repository: iron-absolution. Branch: `feat/pr-24-mantle-interact`. PR: #25. Role: author. Base: `a32d9904eb465f81946988a0c08272a70b656b84`.

### What this session did, and why

- Answered round 1 of the review of PR #25 in `docs/reviews/pr-25-response.md`. P2-1 has full merit.
- `AIronDoor::Toggle` now refuses an open offset that is not finite, before any change of state (T-2).
- `IronAbsolution.Player.Interact.Errors` now checks a NaN and an infinite offset. It failed on the old code and passes now.
- Session 69 holds the rest of the work of this PR.

### The state of the build

- The remote head is the commit of this entry on `feat/pr-24-mantle-interact`, pending merge.
- `verify`, `editor-build`, and `package-build` pass. `editor-test` passes 29 tests headless.
- The fix changes an error path alone, so the play test and the frame-time capture of session 69 still hold. The session did not run `package-run` or `frame-capture` again, because each opens a game window (D-96).

### What is in flight

- Round 2 of the Codex review of PR #25.

### Traps and gotchas

- `FVector::IsNearlyZero` is false for a NaN or an infinity. Check `ContainsNaN` first, which tests `FMath::IsFinite` on each component.
- `frame-capture` needs a desktop at 2560x1440 (D-137, D-138).

### The questions that block progress

- None.

### The next concrete action

- Read the verdict of round 2. On approval, give the owner the merge summary and ask for the merge.

## Session 70: 2026-10-01, Codex

Author: Codex
Session: reviewer PR-25, round 1. Repository: iron-absolution. Branch: `feat/pr-24-mantle-interact`. PR: #25. Role: reviewer. Base: `a32d9904eb465f81946988a0c08272a70b656b84`.

### What this session did, and why

- Reviewed PR #25 at effective head `6f9f91b7dafa4509196c2bf836d0a22ab181ceca`.
- Added finding P2-1: a non-finite door offset corrupts the panel transform without an error (T-2).
- Reviewed the mantle, interact, cue, quit binding, rendering settings, content script, tests, documents, owner evidence, and CI.

### The state of the build

- The effective head is `6f9f91b7dafa4509196c2bf836d0a22ab181ceca`. The remote branch holds this session's metadata commit.
- The owner reports successful Windows build, content build, 29 automation tests, package checks, and frame-time capture at that head.
- CI passes `ste-check`, `doc-gate`, build, test, and format, and coverage. `review-gate` failed before the review record existed.
- Local `run.ps1 verify` did not run. Owner confirmation is pending (D-96).

### What is in flight

- P2-1 needs a finite-offset check and a regression test.
- The corrected review record and this entry need one metadata commit and a push to `feat/pr-24-mantle-interact`.

### Traps and gotchas

- The local review worktree is `review/pr-25`, with no upstream. Verify the pushed head with `gh pr view`.
- The PR number is 25. Its roadmap id is PR-24.

### The questions that block progress

- Local build and test verification awaits owner confirmation under D-96.

### The next concrete action

- Commit the correction to the record and entry, then push it to the PR branch.

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
