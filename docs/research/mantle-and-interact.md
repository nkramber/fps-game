# Mantle and interact

Status: research, checked 2026-09-30 for PR-24. Revised 2026-10-01 for the glow of the cue, the motion blur, and the Escape key (D-148 to D-150). Written in ASD-STE100 (D-17).

Section 7.4 of `docs/roadmaps/phase-3-core-feel.md` asks PR-24 to read the Epic pages of the method of the mantle, with a date (D-34). This file records each page, each fact of the engine source, and the method of the mantle, of interact, and of the outline.

## 1. Epic pages, read 2026-09-30

| Page | Version | Fact that PR-24 uses |
|---|---|---|
| [Understanding Networked Movement in the Character Movement Component](https://dev.epicgames.com/documentation/en-us/unreal-engine/understanding-networked-movement-in-the-character-movement-component-for-unreal-engine) | 5.8 | The mode `MOVE_Custom` stops each other movement physics, so a custom movement runs with no interference. A C++ subclass of the movement component overrides `PhysCustom`. A move through `SetLocation` works in a game for one player alone. |
| [Traces with Raycasts](https://dev.epicgames.com/documentation/en-us/unreal-engine/traces-with-raycasts-in-unreal-engine) | 5.8 | A line trace returns the first blocking hit on a channel. A shape trace sweeps a shape from a start to an end, as a line trace does. |
| [Post Process Materials](https://dev.epicgames.com/documentation/en-us/unreal-engine/post-process-materials-in-unreal-engine) | 5.8 | The custom depth is a second depth buffer for the objects that turn it on. A post-process material reads it through the node `SceneTexture`, and the page shows an outline from it. The location "After Tonemapping" runs after the tonemapper and the color grade. |

## 2. Facts of the engine source, Unreal Engine 5.8.3

- `UCharacterMovementComponent::PerformMovement` calls `UpdateCharacterStateBeforeMovement` after it sets the acceleration from the input, and before the physics of the mode. The crouch of the engine starts in this function. The mantle starts in it too.
- `StartNewPhysics` calls `PhysCustom` for `MOVE_Custom`. `CanAttemptJump` is false in a custom mode, so a jump key does nothing during a climb.
- `SetMovementMode(MOVE_Walking)` finds the floor under the capsule. With no floor, the walk physics starts a fall.
- `FSceneView::OverridePostProcessSettings` adds a blendable only when its weight is more than 0. A material with no weight on the camera then adds no pass.
- `FPostProcessSettings::AddBlendable` sets the weight of a blendable that the list already holds, and adds it at the end when the list does not.
- The node `SceneTexture` takes a UV of the viewport, from 0 to 1. The translator of the material changes it into a UV of the buffer.
- `EBlendableLocation` in `BlendableInterface.h` gives the location "Scene Color Before Bloom". It runs after the temporal upscale and before the bloom, with linear color.
- The default of `r.DefaultFeature.MotionBlur` is 1 in `SceneView.cpp`. With 0, the view sets the amount of the motion blur to 0, unless a camera or a volume overrides the amount.
- `UMeshComponent::SetOverlayMaterial` draws the mesh a second time with a material, on top of its own materials.
- The default of `r.CustomDepth` is 1: the engine makes the buffer when a primitive first asks for it. The tooltip of `r.CustomDepthTemporalAAJitter` says to turn the jitter off when a material reads the custom depth after the tonemapper.

## 3. The method of the mantle

1. The mantle checks each frame while the player is in the air and the input moves the player forward.
2. A sweep of the capsule of the player, 30 cm forward, finds a wall that faces the player.
3. A line down, past the edge of the wall, finds the ledge top in the band above the feet.
4. A ledge above the band holds the start of the line, so the check finds no top in the band.
5. An overlap test checks that the capsule fits on the ledge.
6. Two sweeps check that the path up and then forward is clear.
7. The climb moves the capsule along the path at a steady speed, in the custom mode.
8. At the end, the walk mode finds the floor of the ledge.

The size of the capsule sets the reach of the sweep, the place on the ledge, and the margins. So these values are rules of the class, and the tuning holds the band and the time (D-29).

## 4. The method of interact and the cue

- The player character traces a line from the eye point of the pawn along the view, on the channel `Visibility`. The reach of the tuning sets its length. The first blocking hit decides, so a wall blocks a use.
- An actor takes the verb through the C++ interface `IIronInteractable`. The test door and the test switch take it (D-146).
- The target in reach turns on the custom depth of its primitives. The camera sets the weight of the outline material to 1, and to 0 when no target is in reach (D-145).
- The outline material compares the custom depth of the pixel with the custom depth 3 pixels away in four directions. A step of more than 100 cm marks a pixel just outside the target.
- Correction of 2026-10-01: the outline material runs before the bloom, not after the tonemapper (D-148). Its color is 12 times the color of the labels, so the bloom makes a glow around it.
- The target in reach also gets the overlay material `M_InteractGlow`: an additive, unlit color, brighter at the edges through a Fresnel term (D-148).
- The game has no motion blur (D-149). `Game/Config/DefaultEngine.ini` turns off the default, and a test checks the camera of the player and each volume of the gym.
- The Escape key closes the game through the player controller, until the menu of phase 8 (D-150).

## 5. The evidence of the tests

The automation tests of PR-24 ran headless on 2026-09-30 at 120 frames each second.

| Test | Result |
|---|---|
| `IronAbsolution.Player.Mantle.Band` | A ledge at 50 cm and at 130 cm above the feet starts a mantle. A ledge at 49 cm and at 131 cm does not. |
| `IronAbsolution.Player.Mantle.Climb` | From the floor, the player climbs a ledge 245 cm high in 48 frames, 0.4 s. A ledge 255 cm high gets no climb. |
| `IronAbsolution.Player.Mantle.NeedsForwardMove` | A jump next to a ledge in the band, with no forward move, does not climb it. |
| `IronAbsolution.Player.Interact.Reach` | A switch 190 cm from the eye opens and closes the door. A switch 210 cm away, or behind a wall, is not a target. |
| `IronAbsolution.Player.Interact.Cue` | The switch in reach has the custom depth and the glow overlay, and the outline has full weight. Out of reach, each is off. |
| `IronAbsolution.Project.NoMotionBlur` | The project setting and the console variable turn off the motion blur. No camera of the player and no volume of the gym override the amount. |
| `IronAbsolution.Player.Quit.Binding` | The controller binds the quit action to its input component. The test does not press the key, because the quit closes the editor. |

On 2026-09-30, the session removed the check of the forward move and the check of the height. It also moved the start of the line 20 cm up. The three mantle tests then failed, and the session restored the code.
