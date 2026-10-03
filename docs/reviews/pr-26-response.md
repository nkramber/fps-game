# PR-26 response

Written in ASD-STE100 (D-17). The author answers the review of `docs/reviews/pr-26.md`, round 1, at the effective head `3becf704562b81a2a497e3b5738391ad8bb8333c`.

### P2-1

- Disposition: full merit.
- Evidence: each comparison with a NaN is false, so `!(Value > 0.0f)` refuses a NaN. Positive infinity is above 0, so the check let it pass. The new test reproduced it on the old code on 2026-10-03. A flash with an infinite `Lifetime` or `Diameter` stayed in the world, and the HUD gave no error for an infinite `HitMarkerSeconds`.
- Correction: `Game/Source/IronAbsolution/Private/IronHitFlash.cpp` and `Game/Source/IronAbsolution/Private/IronHUD.cpp`. Each check now needs `FMath::IsFinite` and a value above 0. The flash checks its diameter too, because an infinite diameter gives an infinite scale. Each error line names the actor and the value (T-2). No new decision.
- Regression check: `IronAbsolution.Player.Weapon.Errors` spawns the flash of the Blueprint with an infinite `Lifetime` and with an infinite `Diameter`. Each must give the error line and remove itself. The test also gives the HUD of the Blueprint an infinite `HitMarkerSeconds`, and expects one error that names the value and the HUD. The test failed on the old code with three failed assertions, and passes on the new code. The 39 automation tests pass headless.

New D-# ids: none. New F-# ids: none.
