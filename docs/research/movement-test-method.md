# Movement test method

Status: research, checked 2026-09-29 for PR-21. Written in ASD-STE100 (D-17).

Section 7.1 of `docs/roadmaps/phase-3-core-feel.md` asks PR-21 to read the Epic pages of the test method, with a date. This file records each page, each fact of the engine source, and the method that the tests use.

## 1. Epic pages, read 2026-09-29

| Page | Version | Fact that PR-21 uses |
|---|---|---|
| [Enhanced Input](https://dev.epicgames.com/documentation/en-us/unreal-engine/enhanced-input-in-unreal-engine) | 5.8 | An input action is a data asset. A mapping context binds keys to actions, with modifiers such as Negate and Swizzle. The local player subsystem adds a context through `AddMappingContext`. |
| [Write C++ tests](https://dev.epicgames.com/documentation/en-us/unreal-engine/write-cplusplus-tests-in-unreal-engine) | 5.8 | A simple test uses `IMPLEMENT_SIMPLE_AUTOMATION_TEST` with the flag `EditorContext`. Latent commands run a test across frames of the engine. |
| [Scripting the Unreal Editor using Python](https://dev.epicgames.com/documentation/en-us/unreal-engine/scripting-the-unreal-editor-using-python) | 5.8 | The Python Editor Script Plugin turns Python on. Python runs in the editor alone, never in a packaged game. The commandlet `-run=pythonscript -script=` runs a script with no editor window. |

The page of the API of the movement component did not load on 2026-09-29. Section 2 gives the facts of that component from the engine source.

## 2. Facts of the engine source, Unreal Engine 5.8.3

- `FTestWorldWrapper` in `Engine/Public/Tests/AutomationCommon.h` makes a game world for a test. It starts play, and a test ticks it frame by frame with `TickTestWorld`. The engine module exports it, so the project needs no new module.
- The movement component moves a character with no controller only when `bRunPhysicsWithNoController` is on. A controller of a player with no local player is not a local controller, so a test world uses the flag.
- `ACharacter::PostInitializeComponents` sets the first movement mode when the flag is on at the spawn. A test that sets the flag after the spawn calls `SetDefaultMovementMode`.
- The falling physics of the movement component moves by the mean of the old and the new velocity in each frame. Under a constant gravity, the height of a jump is then exact, and the test measures 120.0 cm for a tuning of 120 cm.
- The first-person template of 5.8 puts the mapping contexts on the player controller, and the input actions on the character. PR-21 uses the same split.
- The project settings have `bEnableLegacyInputScales` on. A positive pitch input then turns the view down, so the mapping context negates the Y axis of the mouse, as the template does.
- The Python commandlet gives the exit code -1 when the script raises, and 0 when it ends. `run.ps1 content-build` reads that code.

## 3. The method of the tests

1. The test makes a game world, starts play, and places a floor of 100 m by 100 m.
2. The test spawns the Blueprint of the player, so the tuning comes from the data asset.
3. Each frame is 1/120 s, the frame rate of the budget (D-32).
4. The run test holds the move input forward for 1 s, then measures the distance of the next 0.5 s.
5. The jump test jumps from the floor and records the highest point of the character.
6. The pass rule is a value within 2 percent of the value of the tuning.

A wrong formula of the jump speed fails both jump tests. The session checked this on 2026-09-29 with a changed formula, then restored it.
