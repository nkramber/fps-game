# Pillars and the core loop

Status: the pillars, the core loop, and the player verbs of the first level. PR-16 adds this file (D-111, D-114). Written 2026-09-29 in ASD-STE100 (D-17). Correction of 2026-09-29: an original harvest tool replaces the chainsaw, with the same rule (PR-19, D-125).

This file states what the game is, in terms that a design choice can test. The design doc holds the roadmap, and `docs/decisions.md` holds each pick of the owner. The proposals of the recovery mechanic are in `docs/game/combat-proposals.md`.

## Pillars

Each pillar is one sentence that a design choice can test. When a choice breaks a pillar, change the choice. Only a decision row of the owner changes a pillar.

| Pillar | Statement | A choice that breaks it |
|---|---|---|
| Attack pays | The player gets resources back by attack, never by retreat or a pause. | Cover or a pause is the best way to get health or ammo back. |
| Every round counts | Each resource is scarce, and the player can read each one at a glance. | A resource that the player cannot see, or a supply so large that no choice matters. |
| Fast and exact | Movement and aim are fast and precise, and each death has a cause that the player can see. | Slow movement, random spread that aim cannot control, or damage from an enemy off the screen with no cue. |
| One level, complete | One hand-made level of 30 minutes or more, with variety of spaces and enemies, polished before any new scope. | A second level, or a new system that the level does not need (D-1, D-37, G-4). |

The owner accepted the pillars as written in PR-16. They come from D-1 and D-37. Doom (2016) informs the combat intensity, the pacing, and the combat rules (D-115). The names, the art, the audio, and the layouts stay original (D-2).

## The core loop of a fight

The loop below runs once for each combat space. The recovery step follows the combat rules of D-115. `docs/game/combat-proposals.md` gives the detail and the first test for the sandbox.

1. The player enters a combat space, and sees the threats and the exits.
2. The player moves and attacks. Each shot spends ammo, and each hit on the player spends health.
3. The player finishes a stunned enemy for health, and uses the harvest tool for ammo (D-115, D-125).
4. The player clears the space, or dies and starts again at the last checkpoint.
5. Between fights, the player explores, finds pickups and secrets, and reaches the next space.

Assumption: the level places a small count of pickups between fights. `docs/game/level-brief.md` gives the count of secrets and checkpoints (D-123), and phase 6 places the pickups.

## Player verbs

D-116 records the list of player verbs, because phase 3 builds them (D-111).

| Verb | What it does | Phase |
|---|---|---|
| move | walk and run on the ground | 3 |
| jump | leave the ground | 3 |
| aim | turn the view and the weapon | 3 |
| shoot | fire the weapon in hand | 3 |
| change weapon | take out a different weapon | 3 |
| melee | a close attack, and the finish of a stunned enemy | 3 for the attack, 4 for the finish |
| mantle | climb onto a ledge at chest height | 3 |
| interact | use a switch, a door, or a pickup that needs a key | 3 |
| use the harvest tool | kill an enemy with fuel, for ammo (D-125) | 4 |

The list has no dash and no reload (D-115, D-116).
