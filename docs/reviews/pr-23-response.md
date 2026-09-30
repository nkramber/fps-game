# PR-23 response

The author answers the review of work head `13bdbb1c6cd187e867b644be9afaf5c4ec95db2c` in `docs/reviews/pr-23.md`.

### P1-1
- Disposition: partial merit.
- Evidence: the CI failure is real. The cause is F-28, not a fault of this PR. The test failed after 9 ms, and its error starts with the path of the stub. So the stub program did not start, and the command did not run its checks. The rerun of the same job passed at run 36660139514. The same failure, at 9 ms, hit a test of `EditorTestCommandTests` on the first push of this PR, and that PR does not change that test. Section 5 of `docs/roadmaps/phase-3-core-feel.md` binds each PR of phase 3 to F-28: "run the failed job again one time, and record each failure in the handoff entry. A later PR fixes the cause". A fix of the start of the stub in this PR is a second concern (G-7).
- Correction: the F-28 row of section 5 of `docs/design.md` records each failure of PR #23 with a dated correction and the run ids. The third failure came in the `coverage report` job of `09ac790`, and its rerun passed. The handoff entry records each rerun.
- Regression check: none in this PR, because F-28 holds the cause. `run.ps1 verify` passes on Windows with 598 tests.

### P2-1
- Disposition: full merit.
- Evidence: the engine always writes `[HasHeaderRowAtEnd],1` in `FCsvStreamWriter::Finalize`. The reader checked the key alone, so a value other than `1` passed.
- Correction: `IronAbsolution.Tools/FrameCapture/FrameTimeCsv.cs` needs the value `1`. Any other value fails with the path, the line, and the value (T-2).
- Regression check: `FrameTimeCsvTests.AMarkOfTheFinishedFileOtherThanOneFails` with the values `0` and an empty value. Both cases fail on the old code, and both pass on the new code.

### P2-2
- Disposition: partial merit.
- Evidence: the part with merit: the rule wrote "an unknown number of" views when the key `IronAbsolution.ViewCount` was absent, and it passed. That is a silent default (T-2). The part with no merit: a rule of exactly four views in the tools. D-137 asks for "a fixed set of views in the gym", and it gives no number. The content script is the source of the set (D-134). The automation test `IronAbsolution.FrameTimeCapture.GymViews` fails unless the gym holds the views 1 to 4, and the package cooks the same content. A second copy of the number in C# gives two sources for one fact, and a later change of the views then needs two edits (T-1).
- Correction: `IronAbsolution.Tools/FrameCapture/FrameCaptureRules.cs` needs the key, with a whole number of 1 or more. An absent key or another value fails the settings check with the key and the value.
- Regression check: `FrameCaptureRulesTests.AnAbsentOrInvalidViewCountFailsTheSettings` with an absent key and the values `0`, `-3`, and `four`. Each case fails on the old code, and each passes on the new code. The count of 4 passes in `EachCheckPassesOnACaptureInsideTheBudget`.

## New ids

- No new D-# or F-# id. F-28 gets a dated correction.

## Final head

- The commit of this response is the new work head. The next round of `run.ps1 codex-review -PR 23` names it.
