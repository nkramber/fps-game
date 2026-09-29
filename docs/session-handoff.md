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

## Session 53: 2026-09-29, Claude Code

Author: Claude Code
Session: author PR-16, round 1. Repository: iron-absolution. Branch: `docs/pr-16-pillars-and-recovery`. PR: #17. Role: author. Base: `c1a2f9f8181b91c08933210f68928d8c18662911`.

### What this session did, and why

- `run.ps1 verify` passed at the base `c1a2f9f`: 539 tests and 0 STE findings.
- The session created `docs/game/` (D-114) with `pillars.md` and `combat-proposals.md`.
- The session wrote three original proposals for OQ-10: Reclaim, Relay, and Iron. The owner answered "Same rules as Doom 2016".
- The session showed the conflict with D-2, G-1, D-1, and D-36. The owner chose to keep D-2, so the session wrote proposal D, Press.
- The owner then said "Nah, back to Doom 2016. Use those mechanics." The owner confirmed the text of D-115.
- D-115: the combat rules follow Doom (2016). Names, art, audio, layouts, and cues stay original. It resolves OQ-10 and revises D-1, D-2, D-36, and G-1 in part.
- D-116: the player verbs. The owner accepted the four pillars as written.
- The design doc, the phase 2 file, and the glossary of the ste-writing skill cite the new rows.

### The state of the build

- The remote head is the commit of this entry on `docs/pr-16-pillars-and-recovery`, pending merge.
- The PR changes documents alone, so the `review-override` label replaces the Codex review (D-35, D-66, D-76).

### What is in flight

- The checks of PR #17, then the merge summary and the owner confirmation.

### Traps and gotchas

- GitHub PR #17 is roadmap item PR-16.
- A new branch from `origin/main` tracks `main`. Remove the upstream, and push with `-u origin <branch>`.
- doomwiki.org and the Fandom wiki refuse WebFetch. The Wikipedia article is the one source of D-115.
- D-115 copies rules alone. Each PR of phase 4 checks that no name, art, sound, or cue of Doom (2016) enters the game.

### The questions that block progress

- None for PR-16. OQ-9 blocks PR-17. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-17 from section 8 of the phase 2 file.

## Session 52: 2026-09-29, Claude Code

Author: Claude Code
Session: author PR-15, round 1. Repository: iron-absolution. Branch: `docs/pr-15-phase-2-roadmap`. PR: #16. Role: author. Base: `708dbf14c573db8fc4e5fc78a74fcd189f8e84f6`.

### What this session did, and why

- `run.ps1 verify` passed at the base `708dbf1`: 539 tests and 0 STE findings.
- The session added `docs/roadmaps/phase-2-direction-brief.md`, the focused roadmap of phase 2 (D-70). It holds PR-16 to PR-19, the gate, and the order.
- The owner gave D-111 to D-114: four PRs, the order PR-16 to PR-19, the pick in the proposal PR, and the folder `docs/game/`.
- The design doc gets the eleventh pass line, the focused roadmap of phase 2, and a dated correction. No PR of phase 2 ran beside phase 1.
- OQ-9, OQ-10, and OQ-12 each get a plan line that names its PR.

### The state of the build

- The remote head is the commit of this entry on `docs/pr-15-phase-2-roadmap`, pending merge.
- The PR changes documents alone, so it has no effective head. The `review-override` label replaces the Codex review (D-35, D-66, D-76).

### What is in flight

- The checks of PR #16, then the merge summary and the owner confirmation.

### Traps and gotchas

- GitHub PR #16 is roadmap item PR-15. Roadmap item PR-16 gets a later GitHub number. Write "PR #n" for GitHub and "PR-n" for the roadmap.
- `run.ps1 ste-check` reads tracked files alone. Stage a new phase file before the check, or its PR headings define no id.
- A line that names a path under `docs/game/` must also name a PR id until PR-16 creates the folder (REF 2).

### The questions that block progress

- None for PR-15. OQ-10 blocks PR-16, and the owner answers it during PR-16 (D-113). OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt of PR-16 from section 8 of the phase 2 file.

## Session 51: 2026-09-28, Claude Code

Author: Claude Code
Session: author PR-11, round 1 close. Repository: iron-absolution. Branch: `docs/pr-11-gate-record`. PR: #15. Role: author. Base: `95546e35317aea9fff1f56a2ec2e9c549eceff84`.

### What this session did, and why

- Round 1 of `run.ps1 codex-review -PR 15` gave the exit code 0. The record says `Ready for owner merge` for the effective head `720ded3`, with no finding.
- The reviewer ran `EditorConfigTests` at the base `95546e3`, and it failed on the Makefile section. So the test is a true regression test.
- This entry closes the author loop before the merge summary.

### The state of the build

- The effective head stays `720ded3`, because the later commits change `docs/` alone (D-49).
- The remote head is the commit of this entry on `docs/pr-11-gate-record`, pending merge.

### What is in flight

- The owner confirmation of the merge of #15, then auto-merge after the checks.

### Traps and gotchas

- The review record calls the "M-8 evidence" comment an owner comment. The session posted it through the account of the owner. The values in it are the output of the session.
- F-28 stays open. The hosted stub test can fail one time. Run the failed job again before a change.

### The questions that block progress

- None. OQ-16 still holds the gitar pass (D-7).

### The next concrete action

- After the merge, write the transitional prompt of the next PR from section 8 of `docs/design.md`. The gate of phase 1 passes, so the next item is in phase 2 or F-28.

## Session 50: 2026-09-28, Codex

Author: Codex
Session: review PR-11, round 1. Repository: iron-absolution. Branch: `docs/pr-11-gate-record`. PR: #15. Role: reviewer. Base: `95546e35317aea9fff1f56a2ec2e9c549eceff84`.

### What this session did, and why

- Reviewed PR #15 at effective head `720ded38699038c9cf0f093cbd5377e5f8c484e7` under D-14.
- The provider gate passed. The review found no blocking defect.
- Added `docs/reviews/pr-15.md` and this entry as one metadata commit for `origin/docs/pr-11-gate-record`.
- The new regression test fails on the base `Makefile` section and passes on the PR head.

### The state of the build

- `run.ps1 verify` passed on Windows: 539 tests, format, and STE passed.
- CI passed the build, test, and format, coverage, document, and STE checks at the effective head.
- The review gate failed before the review record existed. The metadata push triggers a new check.
- The first metadata push passed `review-gate`, but `doc-gate` found the local worktree branch in this entry. The corrected entry names the PR branch. All five hosted checks then passed at PR tip `9b653bf`.
- The remote work head outside the metadata set is `720ded38699038c9cf0f093cbd5377e5f8c484e7`.

### What is in flight

- The review record and handoff entry need one metadata commit and push.

### Traps and gotchas

- M-8 counts the LFS objects in the fresh clone, not objects that only a deleted branch held (D-108).
- `package-run` opens a game window and needs owner confirmation (D-96).

### The questions that block progress

- None. OQ-16 still holds gitar (D-7).

### The next concrete action

- The author checks the review gate, then gives the owner the merge summary.

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
