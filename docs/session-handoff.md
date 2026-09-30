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

## Session 59: 2026-09-29, Codex

Author: Codex
Session: reviewer PR-21, round 1. Repository: iron-absolution. Branch: `feat/pr-21-gym-movement`. PR: #22. Role: reviewer. Base: `ce25cfd4982d5d4be2c0eee0a0a2658f419b182f`.

### What this session did, and why

- Reviewed work head `e1d5667ccd6e7e5ac3534390dcb2beea1b1a7dac` for PR #22.
- Recorded two P2 findings in `docs/reviews/pr-22.md`: a zero-gravity failure leaves partial tuning, and NaN values pass range checks.
- The review verdict is Blocked. Exit test 1 still needs the owner play test of the gym in the editor and package.
- CI passed `ste-check`, `doc-gate`, and build, test, and format. The first coverage run failed two package-run tests. Its required rerun passed. `review-gate` failed before this review record existed.
- The owner-reported Windows checks pass at `e1d5667`. No local build or test ran because this session had no owner confirmation.

### The state of the build

- The remote PR head is `923f0ddbfa3c7801b761056af93fe5c0060118d7` on `feat/pr-21-gym-movement`. The work head remains `e1d5667ccd6e7e5ac3534390dcb2beea1b1a7dac`.

### What is in flight

- The author must answer the two findings.
- The owner must run the play test for exit test 1 before merge.
- The `doc-gate` check failed because this entry named the local review branch. This correction names the PR branch.
- The `review-gate` check fails while the verdict is Blocked. It needs an approval after the author answers the findings and the owner completes the play test.

### Traps and gotchas

- `run.ps1 content-build` rewrites `L_Gym` and `IMC_KeyboardMouse` with new internal ids. Restore them if the script did not change.
- The gym play test remains open even though the owner reported that package-run passes on `L_Test`.

### The questions that block progress

- None for GitHub PR #22. OQ-25 applies to the later roadmap PR-22.

### The next concrete action

- Verify the `doc-gate` check for this corrected entry.

## Session 58: 2026-09-29, Claude Code

Author: Claude Code
Session: author PR-21, round 1. Repository: iron-absolution. Branch: `feat/pr-21-gym-movement`. PR: #22. Role: author. Base: `ce25cfd4982d5d4be2c0eee0a0a2658f419b182f`.

### What this session did, and why

- At the base, `run.ps1 verify` passed with 539 tests, and `run.ps1 toolchain-check` held each of the 5 pins.
- The owner gave D-133 to D-136: the map `L_Gym`, a Python script for the content, the first pace of 900 cm/s, and the keys W, A, S, D, Space, and the mouse.
- C++: `UIronMovementTuning` (each default 0, so the asset holds each value), `AIronPlayerCharacter`, and `AIronPlayerController`. Each absent asset or invalid value writes an error line (T-2).
- `Game/Scripts/build_content.py` makes the input, the tuning, the Blueprints, and the gym. `run.ps1 content-build` runs it headless (D-134).
- Five new automation tests. The run test measures 900.0 cm/s and the jump test 120.0 cm. A wrong jump formula fails both jump tests.
- `package-run` names the test map as its first argument, because the gym is now the default map.
- New files: `docs/game/movement-metrics.md` and `docs/research/movement-test-method.md`.

### The state of the build

- The remote head is the commit of this entry on `feat/pr-21-gym-movement`, pending merge.
- `run.ps1 verify` passes with 552 tests. `editor-test` passes 8 automation tests. `package-build` passes, and `package-run` passes on `L_Test`, with owner consent (D-96).

### What is in flight

- The Codex review of PR #22.
- Exit test 1: the owner plays the gym in the editor and in the package. The owner chose to do it later, so it waits before the merge.

### Traps and gotchas

- The script is the source of each asset that it makes. A change in the editor alone goes away at the next `content-build`.
- Each `content-build` writes `L_Gym` and `IMC_KeyboardMouse` again with new internal ids. Restore them when the script did not change.
- A test world has no local player. The movement tests set `bRunPhysicsWithNoController`, then `SetDefaultMovementMode`, or the character never moves.
- The project has `bEnableLegacyInputScales` on, so a positive pitch input turns the view down. The mapping context negates the Y axis of the mouse.
- The C++ copies the ranges of the tuning from the clamps of the header, because a package has no metadata. Change both together.
- F-28 can fail the `coverage report` job. Run the job again one time, and record the failure.

### The questions that block progress

- None for PR-21. OQ-25 blocks PR-22, and OQ-26 blocks the sound part of PR-25. OQ-21 blocks phase 8. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- Answer the Codex review, then ask the owner for the play test of exit test 1, then give the merge summary.

## Session 57: 2026-09-29, Claude Code

Author: Claude Code
Session: author PR-20, round 1. Repository: iron-absolution. Branch: `docs/pr-20-phase-3-roadmap`. PR: #21. Role: author. Base: `de3143ea61b5ab5caa08f792b5b8e63469657e2a`.

### What this session did, and why

- `run.ps1 verify` passed at the base `de3143e`: 539 tests and 0 STE findings.
- The session wrote `docs/roadmaps/phase-3-core-feel.md`: PR-21 to PR-27, the gate of phase 3, the order, and the questions.
- The owner gave D-126 to D-132 in this PR:
  - D-126 and D-127: seven PRs after the phase file, with the frame-time capture right after the movement.
  - D-128: one weapon rule and two data assets, so "change weapon" has a test. It resolves F-30.
  - D-129: the phase file of phase 5 comes after the gate of phase 3. Phase 5 runs beside phase 4.
  - D-130: the weapon of phase 3 is hitscan.
  - D-131: the feel pass keeps data-asset changes in PR-27. A rule change gets its own PR.
  - D-132: the player runs at full speed by default, with no run key.
- OQ-25 asks the method of M-3, and blocks PR-22. OQ-26 asks the sound of the gym, and blocks the sound part of PR-25.
- The design doc: the fifteenth pass, F-30, the phase 3 entry, the start of phase 5, M-3, and section 8. The readme lists the new file.
- The glossary of the ste-writing skill adds "gym" and "sandbox".

### The state of the build

- The remote head is the commit of this entry on `docs/pr-20-phase-3-roadmap`, pending merge.
- The PR changes documents alone, so the `review-override` label replaces the Codex review (D-35, D-66, D-76).

### What is in flight

- The checks of PR #21, then the merge summary and the owner confirmation.

### Traps and gotchas

- GitHub PR #21 is roadmap item PR-20. The ids of phase 3 continue from PR-20.
- PR-21 makes the gym the default map. `PackageRunRules` expects the test map in the success line, so the start command of `run.ps1 package-run` must name the test map.
- `ste-check` reads tracked files alone. Stage a new file before you run it, or its PR headings and paths fail the reference check.
- F-28 can fail the `coverage report` job of any PR. Run the job again one time, and record the failure.

### The questions that block progress

- None for PR-20. OQ-25 blocks PR-22, and OQ-26 blocks the sound part of PR-25. OQ-21 blocks phase 8, and OQ-24 blocks the harvest tool in phase 4. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-21: the gym, the character, and the movement.

## Session 56: 2026-09-29, Claude Code

Author: Claude Code
Session: author PR-19, round 1. Repository: iron-absolution. Branch: `docs/pr-19-level-brief`. PR: #20. Role: author. Base: `4c1dce4200caec2d34b4d9fe4fa59a0b9a8bcd12`.

### What this session did, and why

- `run.ps1 verify` passed at the base `4c1dce4`: 539 tests and 0 STE findings.
- The session wrote `docs/game/level-brief.md`: the terms, the targets, the spaces of each zone, a time estimate, the rosters, and the content cost of the length (F-15).
- The owner confirmed the targets:
  - D-122: a first clear of 30 to 45 minutes, 10 combat spaces, and 6 other spaces.
  - D-123: 6 secrets and 10 checkpoints.
  - D-124: three guns and the harvest tool, and four enemy types with no boss.
  - D-125: an original harvest tool replaces the chainsaw, with the same rule. It revises D-115 and D-116 in part.
- OQ-24 asks the kind of the harvest tool. It blocks the tool in phase 4.
- M-8 at the end of phase 2: 8,404 bytes in one LFS object, from a fresh clone at `4c1dce4` (D-108).
- Section 7.5 of the phase 2 file gives the evidence of each line of the gate. The five checks passed on PR #16 to PR #19.
- The design doc, the pillars, the two proposal files, and the glossary of the ste-writing skill name the harvest tool.

### The state of the build

- The remote head is the commit of this entry on `docs/pr-19-level-brief`, pending merge.
- The PR changes documents alone, so the `review-override` label replaces the Codex review (D-35, D-66, D-76).

### What is in flight

- The checks of PR #20, then the merge summary and the owner confirmation.

### Traps and gotchas

- GitHub PR #20 is roadmap item PR-19.
- The owner first answered "3 guns + something other than a chainsaw". That conflicted with D-115 and D-116, so the session asked again. The answer: the same rule, with a new tool.
- The docs of PR-16 and PR-17 keep the word chainsaw as history, with a dated correction.
- The time estimate of the brief gives 30 to 48 minutes. It is an assumption, and M-7 measures it in phase 6.
- The `coverage report` job of PR #20 failed two times on a stub test, one in `ToolchainCheckCommandTests` and one in `EditorTestCommandTests`. Each rerun passed. This is F-28, and the design doc now records the two runs.

### The questions that block progress

- None for PR-19. OQ-24 blocks the harvest tool in phase 4. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt of the first PR of phase 3. Phase 3 has no focused roadmap yet, so that PR adds it.
- F-28 now fails more often. A later PR proves its cause and fixes the start of the stub (T-2, T-3). The owner chooses when.

## Session 55: 2026-09-29, Claude Code

Author: Claude Code
Session: author PR-18, round 1. Repository: iron-absolution. Branch: `docs/pr-18-provenance-meshy`. PR: #19. Role: author. Base: `e9f34481c6d269e985d9cbc46b20143475862555`.

### What this session did, and why

- `run.ps1 verify` passed at the base `e9f3448`: 539 tests and 0 STE findings.
- The session checked the Meshy facts again on 2026-09-29, with a source for each. They stand. The credit costs now depend on the model version, and Pro gives 1,000 credits.
- The session wrote `docs/game/provenance.md`: the record fields, the rule for each kind of source, the rule of F-16, the manifest place, and the Meshy choice.
- The owner answered four questions with the recommendation each time:
  - D-118: Meshy on a paid plan, for focal props alone, with a manual FBX export. It resolves OQ-12.
  - D-119: a third-party asset enters only under CC0 1.0 or CC BY 4.0. It revises D-24 and D-38 in part.
  - D-120: AI art follows the rule of D-38.
  - D-121: one JSON manifest beside the project file.
- The design doc (header, section 1, F-7, F-16, phase 5), the phase 2 file (7.3, section 9), and the research file cite the new rows.

### The state of the build

- The remote head is the commit of this entry on `docs/pr-18-provenance-meshy`, pending merge.
- The PR changes documents alone, so the `review-override` label replaces the Codex review (D-35, D-66, D-76).

### What is in flight

- The checks of PR #19, then the merge summary and the owner confirmation.

### Traps and gotchas

- GitHub PR #19 is roadmap item PR-18.
- The repository is public. The Fab Standard License allows a private repository alone, so no Fab asset fits (D-119).
- The manifest path has no backticks in the docs, because the file does not exist yet and the reference check reads backticked paths (REF 2).
- PR-17 did not add its status to the phase 2 entry of `docs/design.md`. This PR adds it beside the status of PR-18.
- The Meshy pricing page shows its prices in cards that a fetch does not read. The price of each paid plan stays unknown.
- An untracked new file fails REF 2 in `ste-check`. Stage it first.

### The questions that block progress

- None for PR-18. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-19 from section 8 of the phase 2 file.

## Session 54: 2026-09-29, Claude Code

Author: Claude Code
Session: author PR-17, round 1. Repository: iron-absolution. Branch: `docs/pr-17-art-direction`. PR: #18. Role: author. Base: `bdf7ec5a53bbac25c9e24744612be963be16b73e`.

### What this session did, and why

- `run.ps1 verify` passed at the base `bdf7ec5`: 539 tests and 0 STE findings.
- The session wrote `docs/game/art-proposals.md` with three original proposals for OQ-9: Penitent Iron, Spillway, and Tribunal.
- Each proposal gives the setting and the tone, the visual style with a palette, the content cost, the budget fit, Meshy, and originality.
- The owner picked proposal A, Penitent Iron, the recommendation. D-117 records it and resolves OQ-9.
- The design doc (section 1) and section 7.2 of the phase 2 file cite D-117.

### The state of the build

- The remote head is the commit of this entry on `docs/pr-17-art-direction`, pending merge.
- The PR changes documents alone, so the `review-override` label replaces the Codex review (D-35, D-66, D-76).

### What is in flight

- The checks of PR #18, then the merge summary and the owner confirmation.

### Traps and gotchas

- GitHub PR #18 is roadmap item PR-17.
- The Wikipedia article on Doom (2016) does not state the color of the stunned cue. The proposals label the blue and orange glow as an assumption.
- Doom (2016) has a foundry. Penitent Iron keeps no molten metal and no fire as a theme.
- The first `coverage report` job of PR #18 failed in `ToolchainCheckCommandTests.AnInstallWithNoToolsetFolderGivesAnAbsentMsvcThatNamesTheFolder`, and its rerun passed. It is one more case of F-28.

### The questions that block progress

- None for PR-17. OQ-12 (Meshy) is next, in PR-18. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-18 from section 8 of the phase 2 file.
