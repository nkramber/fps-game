# Weapon tuning

Status: the first values of the weapon rule of phase 3. PR-25 adds this file (D-128). Written 2026-10-01 in ASD-STE100 (D-17).

This file gives each value of the two data assets of the weapon rule, with a unit and a reason. The owner picked each value in PR-25 (D-151 to D-168). The feel pass of PR-27 tunes the values (D-131). Phase 4 builds the weapon roster of D-124 on the same rule.

## Where each value lives

- The data assets `DA_WeaponRifle` and `DA_WeaponScatter` hold each value of the tuning (D-29). The C++ holds no tuning value.
- The script `Game/Scripts/build_content.py` makes the data assets and writes each value (D-134). To change a value, change the script, then run `run.ps1 content-build`.
- The class `UIronWeaponComponent` holds the rule: the hitscan shot, the ammo, the rate of fire, the raise, the recoil, and the spread.
- A new data asset makes a new weapon with no change of C++. The test `IronAbsolution.Player.Weapon.ThirdTuning` shows this (exit test 4).

## The values of the tuning

| Value | Rifle | Scatter gun | Unit | Field of the tuning | Label |
|---|---|---|---|---|---|
| Fire | automatic | semi-automatic | none | `bAutomatic` | Owner pick (D-151). A held key fires an automatic weapon at its rate. |
| Rate of fire | 10 | 1.5 | shots each second | `ShotsPerSecond` | Owner pick (D-153). |
| Rounds of a full weapon | 60 | 12 | rounds | `AmmoCapacity` | Owner pick (D-153). Each shot spends one round. |
| Range | 10000 | 10000 | cm | `Range` | Owner pick (D-153). The gym is 100 m long. |
| Pellets | 1 | 8 | pellets each shot | `PelletCount` | Owner pick (D-153). |
| Spread | 0 | 2 | degrees, the half angle of the cone | `SpreadAngle` | Owner pick (D-160). Each pellet goes to a random point of the cone. |
| Raise time | 0.25 | 0.35 | s | `RaiseTime` | Owner pick (D-157). |
| Recoil kick | 0.5 | 3 | degrees up, each shot | `RecoilKick` | Owner pick (D-159). |
| Side kick | 0.25 | 1 | degrees, the largest random turn to the left or the right, each shot | `RecoilSideKick` | Owner pick (D-161). |
| Recoil recovery | 0.15 | 0.35 | s, from any height, after the last shot | `RecoilRecoveryTime` | Owner pick (D-159). The rule of the return is D-162. |

The data assets also hold the content of each weapon:

- the name on the HUD.
- the sound of a shot and the sound of an empty weapon. A hit on a target has no sound (D-167).
- the class of the flash.
- the mesh, the scale, and the place of the weapon in the view.

## Values that follow from the tuning

| Value | Rifle | Scatter gun | How it follows |
|---|---|---|---|
| Time from one shot to the next | 0.1 s | 0.67 s | 1 divided by the rate of fire. |
| Time to spend a full weapon | 5.9 s | 7.3 s | The first shot is at the press. Each other round takes one interval. |
| Largest distance of a pellet from the crosshair at 10 m | 0 cm | 35 cm | 1000 cm times the tangent of 2 degrees. |
| The same at 25 m | 0 cm | 87 cm | 2500 cm times the tangent of 2 degrees. |
| Height of the view in a held fire | near 0.75 degrees | none | Each shot adds 0.5 degrees. Each 0.1 s takes back two thirds of the kick of the moment, so the kick settles where the two are equal. |

The height of a held rifle is a result of the rules of the recoil (D-162). At 25 m, 0.75 degrees is about 33 cm, so the view stays on a target at 25 m. Each shot also turns the view to a random side, so a burst wanders a little (D-161). The whole kick comes back in the recovery time after the last shot, also after a full weapon.

The recoil turns the view, not the body. The control rotation of the player does not change, so the view returns to the aim of the player when the recoil is gone.

## The HUD

The Blueprint `BP_PlayerHUD` holds each value (D-29). Each size is in pixels.

| Value | Value of the HUD | Label |
|---|---|---|
| Color | the color of the labels of the gym | Recommendation. |
| Color of an empty weapon | red | Owner pick (D-158). |
| Crosshair | four lines of 8 px, 4 px from the center, 2 px thick | Recommendation. |
| Hit marker | an X of four lines of 8 px on each axis, 8 px from the center on each axis | Owner pick of the form (D-154). The sizes are a recommendation. |
| Time of the hit marker | 0.15 s | Owner pick (D-154). |
| Ammo | the name and the rounds of the weapon in hand, at the lower right, 40 px from each edge | Recommendation. The pillar "Every round counts". |

## The hit feedback

- A shot that hits a gym target shows the hit marker, and plays the sound of the hit, one time for each shot (D-154, D-155).
- Each pellet that hits a surface makes a flash at the point of the hit. The flash is a sphere of 8 cm, and it fades from full brightness to 0 in 0.1 s (D-154). It has no collision.
- The Blueprint `BP_HitFlash` holds the size, the life, the mesh, and the material of the flash.

## The sounds

`docs/game/provenance.md` holds the record of each file (D-155). The volume of each sound asset sets the mix (D-168). A hit on a gym target has no sound, and the hit marker alone shows the hit (D-167).

| Sound | Asset | Volume | Start | Label |
|---|---|---|---|---|
| Shot of the rifle | `S_RifleShot` | 0.35 | 0 s | Owner pick (D-165, D-168). The shots of a burst overlap, so each plays about 9 dB under a scatter shot. |
| Shot of the scatter gun | `S_ScatterShot` | 1.0 | 0.05 s | Owner pick (D-165, D-168). The file has 56 ms of silence at its start. |
| Empty weapon | `S_EmptyClick` | 1.0 | 0 s | Owner pick (D-158, D-168). |

Each sound plays for the player alone, as a sound of the game. A pause of the game stops it.

## The stations of the gym

| Station | Place | What the player tests |
|---|---|---|
| Targets | Three boards 100 cm wide and 180 cm high, between the distance row and the ledge row, at 10 m, 25 m, and 50 m from the start | The hit, the spread, and the recoil. Each target shows its count of hits. It has no health. |
| Ammo station | A box 50 cm wide and 100 cm high, 4 m to the left of the start | The verb "interact" fills each weapon (D-156). |

## Keys

The left mouse button fires (D-151). Each step of the mouse wheel takes the next weapon, and the 1 and 2 keys take a slot (D-152). No C++ names a key (OQ-21).

## Evidence

The automation tests of PR-25 check each rule in a test world at 120 frames each second. The rule tests make their own tunings, so a new value in this file does not change them.

| Test | What it checks |
|---|---|
| `IronAbsolution.Player.Weapon.Hit` | A hit on a target in the line, a miss on a target out of the line, and a wall that takes the shot |
| `IronAbsolution.Player.Weapon.Ammo` | One round for each shot, also with 4 pellets, no shot from an empty weapon, and the ammo station |
| `IronAbsolution.Player.Weapon.RateOfFire` | 10 shots in a hold of 0.95 s at 10 shots each second, and one shot for each press of a semi-automatic weapon |
| `IronAbsolution.Player.Weapon.Change` | The other data asset gives its tuning after the change and after its raise time |
| `IronAbsolution.Player.Weapon.ThirdTuning` | A third data asset gives a third tuning with no change of C++ |
| `IronAbsolution.Player.Weapon.Pellets` | Each pellet inside the cone, and an even spread over the cone |
| `IronAbsolution.Player.Weapon.Recoil` | The kick up, the random side, the recovery, the return after a burst of 3 s, and the pitch limit |

`docs/research/weapon-and-sound.md` gives the facts of the engine and of freesound.org.
