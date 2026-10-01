# PR-25 response

Written in ASD-STE100 (D-17). The author answers the review of `docs/reviews/pr-25.md`, round 1, at the effective head `6f9f91b7dafa4509196c2bf836d0a22ab181ceca`.

### P2-1

- Disposition: full merit.
- Evidence: `FVector::IsNearlyZero` compares the absolute value of each component with a tolerance. Each comparison with a NaN is false, and an infinity is not near zero. So `AIronDoor::Toggle` let a non-finite offset pass, set the door open, and wrote the offset to the panel. The new test reproduced it on the old code on 2026-10-01: `IronAbsolution.Player.Interact.Errors` failed, and the engine raised the ensure `NewTransform.IsValid()` in `SceneComponent.cpp`.
- Correction: `Game/Source/IronAbsolution/Private/IronDoor.cpp`. `Toggle` checks `OpenOffset.ContainsNaN()` before it changes the state. The engine function returns true for a NaN and for an infinity, because it tests `FMath::IsFinite` on each component. The error line names the door and the offset, and the door keeps its state and its panel (T-2). No new decision.
- Regression check: `IronAbsolution.Player.Interact.Errors` now uses the door with a NaN offset and with an infinite offset. Each use must give the error line, keep the door closed, and keep the panel at the closed place. The test failed on the old code and passes on the new code. The 29 automation tests pass headless.

New D-# ids: none. New F-# ids: none.
