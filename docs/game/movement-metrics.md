# Movement metrics

Status: the first values of the movement of the player. PR-21 adds this file (D-114). Written 2026-09-29 in ASD-STE100 (D-17).

This file gives each metric of the movement with a value and a unit. Phase 5 reads it for the kit grid, and phase 6 reads it for the layout of the level. The owner picked the first pace in PR-21 (D-135). The feel pass of PR-27 tunes the values (D-131).

## Where each value lives

- The data asset `DA_PlayerMovement` holds each value of the tuning (D-29). The C++ holds no tuning value.
- The script `Game/Scripts/build_content.py` makes that asset and writes each value (D-134). To change a value, change the script, then run `run.ps1 content-build`.
- The class of the player character holds the collision size. The level geometry depends on it, so it is a rule and not a value of the tuning.

## The values of the tuning

| Metric | Value | Unit | Field of the tuning | Label |
|---|---|---|---|---|
| Run speed | 900 | cm/s | `RunSpeed` | Owner pick (D-135). The player has no run key (D-132). |
| Acceleration | 8000 | cm/s² | `Acceleration` | Recommendation: full speed in about 0.11 s, so the start feels instant. |
| Braking deceleration | 8000 | cm/s² | `BrakingDeceleration` | Recommendation: a stop in about 0.11 s after the player releases the keys. |
| Ground friction | 8 | none | `GroundFriction` | The default of the engine. It sets how sharp a turn on the ground is. |
| Jump height | 120 | cm | `JumpHeight` | Owner pick (D-135). The height of the feet at the top of the jump. |
| Gravity scale | 1.5 | none | `GravityScale` | Recommendation: a shorter time in the air than the default of 1.0, for a fast feel. |
| Air control | 0.5 | none, from 0.01 to 1 | `AirControl` | Recommendation: the value of the first-person template of the engine. |
| Step height | 45 | cm | `StepHeight` | The default of the engine. The player climbs a step of this height with no jump. |
| Walkable slope | 45 | degrees | `WalkableSlope` | Recommendation: close to the default of the engine, 44.765 degrees. |
| Eye height | 160 | cm | `EyeHeight` | Recommendation: the height of the camera above the feet. |
| Lowest mantle height | 50 | cm, above the feet | `MantleMinHeight` | Owner pick (D-143). It is above the step height, so a step is never a mantle. |
| Highest mantle height | 130 | cm, above the feet | `MantleMaxHeight` | Owner pick (D-143). Chest height (D-116). |
| Mantle time | 0.4 | s | `MantleTime` | Owner pick (D-143). The player has no control in the climb. |
| Interact reach | 200 | cm, from the eye along the view | `InteractReach` | Owner pick (D-144). |

## The rules of the class

| Metric | Value | Unit | Label |
|---|---|---|---|
| Collision radius | 35 | cm | Recommendation: the player fits a hall of 70 cm and more. |
| Collision height | 180 | cm | Recommendation: twice the half height of 90 cm. |

## Values that follow from the tuning

The gravity of the world is 980 cm/s². The character computes the start speed of a jump from the jump height, so a new gravity keeps the height.

| Metric | Value | Unit | How it follows |
|---|---|---|---|
| Gravity on the player | 1470 | cm/s² | 980 times the gravity scale of 1.5. |
| Start speed of a jump | 594 | cm/s | The square root of 2 times 1470 times 120. |
| Time in the air of a jump on flat ground | 0.81 | s | 2 times 594, divided by 1470. |
| Longest gap on flat ground at full speed | 727 | cm | 900 times 0.81. |
| Tallest ledge that a mantle climbs from the floor | 250 | cm | The jump height of 120 plus the highest mantle height of 130. |

Assumption: the longest gap is a limit of the rules of motion. The gym has gaps from 200 cm to 800 cm, so the owner sees the real limit in play.

## Evidence

The automation tests of PR-21 measure the movement in a test world at 120 frames each second. The pass rule is a value within 2 percent of the tuning.

| Test | Tuning | Measured on 2026-09-29 |
|---|---|---|
| `IronAbsolution.Player.Movement.RunSpeed` | 900 cm/s | 900.0 cm/s |
| `IronAbsolution.Player.Movement.JumpHeight` | 120 cm | 120.0 cm |
| `IronAbsolution.Player.Movement.TuningChange` | 1350 cm/s and 180 cm | 1350.0 cm/s and 180.0 cm |

## The stations of the gym

The gym `L_Gym` has five rows of stations along the view at the start (D-133). A label above each station gives its size in centimeters.

| Row | Stations | What the player tests |
|---|---|---|
| Gaps | Gaps of 200, 300, 400, 500, 600, 700, and 800 cm between platforms 40 cm high | The longest jump |
| Ledges | Blocks 40, 50, 75, 100, 125, 150, 200, 245, and 255 cm high | The highest ledge that a jump reaches, and the band of the mantle |
| Distance | A line on the floor each 500 cm, for 50 m | The run speed and the time to full speed |
| Steps and ramps | Stairs with steps of 15, 30, 45, and 60 cm, and ramps of 30, 40, 45, and 50 degrees | The step height and the walkable slope |
| Halls | Halls 100, 150, 200, 300, and 400 cm wide, with walls 300 cm high | The width that feels right for a fight or a path |
| Door | A wall 300 cm high to the right of the start, with a door 200 cm wide and 250 cm high, and a switch | The verb "interact" and its reach (D-146) |

## The mantle

PR-24 adds the mantle (D-143). While the player is in the air and moves forward into a ledge, the player climbs onto it. The ledge top must be from 50 cm to 130 cm above the feet at that moment. The climb goes up, then forward, in 0.4 s.

On the ground, a jump starts the mantle. The jump of 120 cm then climbs a ledge from 50 cm to 250 cm high. A ledge of 50 cm to 120 cm is also in reach of the jump alone.

The top of a jump falls between two frames, so a ledge of exactly 250 cm can fail by a fraction of a centimeter. The gym has a station 5 cm inside the limit and one 5 cm outside it. The station of 40 cm is below the band: the player steps onto it.

A thin wall is not a ledge. The capsule must fit on the top, 40 cm past the edge, with room above it.

`docs/research/mantle-and-interact.md` gives the method and the evidence.

## Interact

The E key uses the switch or the door on the line of the view, up to 200 cm from the eye (D-144). A wall between the eye and the target blocks the use. An outline and a glow in the color of the labels show the target in reach (D-145, D-148).

## The aim settings

PR-23 moves the field of view out of the tuning. The settings file of the user holds the aim settings, and `Game/Config/DefaultGameUserSettings.ini` holds the defaults of the project. The class `UIronGameUserSettings` holds the bounds.

| Setting | Default | Bounds | Unit | Console command | Label |
|---|---|---|---|---|---|
| Mouse sensitivity | 2.0 | 0.1 to 20 | 0.022 degrees for each mouse count | `Iron.MouseSensitivity` | Owner pick (D-139, D-142) |
| Field of view | 100 | 80 to 120 | degrees, horizontal | `Iron.FieldOfView` | Owner pick (D-141) |

The game has no vertical invert (D-140). A forward move of the mouse turns the view up.

To change a setting in the package, open the console with the tilde key, and type the command and a number. The game saves the value in the settings file of the user. A command with no number prints the value and the bounds. A value out of bounds gives an error line, and the game keeps the last good value.

The views of the frame-time capture use the default of the project file, so a change of the setting does not change M-3. `docs/research/aim-settings.md` gives the facts of the engine.
