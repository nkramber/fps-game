# PR-22 response

The response of the author to the review of round 1 at the work head `e1d5667`. The review commits `923f0dd` and `52bb6e6` came through `git pull --ff-only` before the corrections.

### P2-1
- Disposition: full merit.
- Evidence: `ApplyMovementTuning` wrote eight fields of the movement component before the check of the gravity. The header says that a refused tuning makes no change. The trigger reproduces in the new test below.
- Correction: the next commit on `feat/pr-21-gym-movement`, `Game/Source/IronAbsolution/Private/IronPlayerCharacter.cpp`. The function now reads the gravity of the world from `UMovementComponent::GetGravityZ`, multiplies it by the gravity scale of the new tuning, and checks it before any write. The base class gives the gravity with no scale, so the scale of the old tuning does not enter the check. T-2.
- Regression check: `IronAbsolution.Player.Movement.RefusedTuning` sets the gravity of the test world to 0, applies a valid tuning, and compares every value that the function writes, before and after. On the old code the test fails: "The tuning in a world with no gravity changes nothing". With the correction it passes.

### P2-2
- Disposition: full merit.
- Evidence: a NaN fails both `<` and `>`, so `FindInvalidValues` gave no error for it. The trigger reproduces in the new test below.
- Correction: the same commit, `Game/Source/IronAbsolution/Private/IronMovementTuning.cpp`. Each value that is not finite is now an error, with the name of the value and the path of the asset. The header comment states the new contract.
- Regression check: `IronAbsolution.Player.Movement.RefusedTuning` sets `RunSpeed` to NaN. It expects one error that names `RunSpeed`, a refusal, and no change of state. On the old code the test fails: "A tuning with one NaN has one error" was 0. With the correction it passes.

### Note on the record

- The Verification section says that the first coverage run failed two package-run tests. The log of CI run 36616780058 shows one failed test, `ToolchainCheckCommandTests.TheGatherReadsEachToolsetFolderOfTheInstallThatVswhereGives`. F-28 records it. The record stays as the reviewer wrote it.

### Exit test 1

- The play test of the owner in the editor and in the package still waits. The owner chose to do it before the merge.

### New ids and the head

- New D-# ids: none in this round. New F-# ids: none. F-28 has a dated line for PR #22.
- The final PR head is the commit of this file and the corrections.
