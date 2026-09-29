# Level brief

Status: the numeric targets of the first level. PR-19 adds this file (D-111, D-114). Written 2026-09-29 in ASD-STE100 (D-17). The owner confirmed each target during PR-19 (D-122 to D-125).

This file gives the size of the level as numbers that a test can check. The pillars and the core loop are in `docs/game/pillars.md`. The setting and the three zones are in `docs/game/art-proposals.md` (D-117). The combat rules are in D-115.

Labels: each target is a decision of the owner, with its D-# id. Each other claim is evidence (with a source), a recommendation, an assumption, or an unknown.

## Terms

| Term | Use for | Do not use |
|---|---|---|
| first clear | one play from the start of the level to the end, by a player who does not know the level | first run, playthrough |
| space | one area of the level with its own graybox PR in phase 6. A short hall is part of a neighbor space | room, area, when the text means this |
| combat space | a space that holds one set fight | arena, when the text means the level |
| other space | a space with no set fight | quiet room, connector |
| zone | one of the three parts of the level that share a light mood: the cloister, the scriptorium, and the vaults | level section, act |
| secret | a hidden place off the main path, with a reward | easter egg |
| checkpoint | the place where the player starts again after a death | save point, respawn |
| harvest tool | the original tool that uses fuel, kills an enemy at once, and gives ammo (D-125) | chainsaw, when the text means the tool of this game |

## The targets

| Target | Value | Unit | Decision |
|---|---|---|---|
| First-clear time | 30 to 45 | minutes | D-122 |
| Combat spaces | 10 | spaces | D-122 |
| Other spaces | 6 | spaces | D-122 |
| All spaces | 16 | spaces | D-122 |
| Weapon roster | 4: three guns and the harvest tool | weapons | D-124, D-125 |
| Enemy roster | 4, with no unique boss | enemy types | D-124 |
| Secrets | 6, two in each zone | secrets | D-123 |
| Checkpoints | 10, one at the entry of each combat space | checkpoints | D-123 |

M-7 in phase 6 reads the range of the first-clear time. The gate of phase 6 passes only when the recorded plays of the graybox fall in the range.

Melee is a verb, not a weapon (D-116). So the weapon roster does not count it.

## The spaces of each zone

The level goes down the cliff, one zone after the other (D-117). The last fight is in the vaults.

| Zone | Light mood | Combat spaces | Other spaces |
|---|---|---|---|
| Cloister | cold daylight | 3 | 2: the start, and the transition to the scriptorium |
| Scriptorium | candle light | 3 | 2: an exploration hub, and the transition to the vaults |
| Vaults | dark | 4, with the last fight | 2: an exploration hub, and the end |
| All | | 10 | 6 |

The start and the end are other spaces. The level has no checkpoint at the start, because the player starts the level there.

Recommendation: phase 6 puts most secrets in the exploration hubs and the transitions. Unknown: the reward of a secret. Phase 6 sets it.

## The time of a first clear

This estimate checks that the counts of spaces can fill the range. It is not a target. M-7 gives the measured value in phase 6.

| Part | Count | Minutes each | Minutes in all |
|---|---|---|---|
| Fights | 10 | 2 to 3 | 20 to 30 |
| Other spaces, with the halls | 6 | 1 to 2 | 6 to 12 |
| Deaths and the second attempt at a fight | | | 4 to 6 |
| First clear | | | 30 to 48 |

- Assumption: a fight takes 2 to 3 minutes. The first test of the sandbox in phase 4 records the time of a fight (`docs/game/combat-proposals.md`).
- Assumption: a player who does not know the level dies 2 to 3 times in a first clear. Each death costs about 2 minutes.
- Recommendation: phase 6 tunes the fights toward the low end of 2 to 3 minutes, when the plays go past 45 minutes.

## The rosters

The weapon roster has three guns and the harvest tool (D-124, D-125). The harvest tool takes the role of the chainsaw of D-115, with the same rule. Phase 4 proposes the kind of the harvest tool (OQ-24).

The enemy roster has four types and no unique boss (D-124). The last fight mixes the types at a high count.

- Risk: four types over ten fights can repeat. Recommendation: phase 6 gives variety by the mix of types, the count of enemies, and the shape of each space.
- Recommendation: each zone brings at least one new type or one new weapon. The beat chart of phase 6 places each one.
- Unknown: the source of the enemy animation. Phase 5 plans it (section 7 of `docs/design.md`).

## The content cost of the length (F-15)

A level of 30 to 45 minutes needs 16 spaces. Each space multiplies the work of phases 6 and 7. This section gives the cost of that length. Each value is an assumption until phase 5 and phase 6 measure it.

### Spaces and PRs

| Phase | Work for each space | PRs |
|---|---|---|
| Phase 6 | one graybox PR for each space | 16 |
| Phase 7 | one PR group for each space: kit, props, light, audio, effects, and performance | 16 groups, up to 96 PRs |

- Assumption: a small space combines some passes of phase 7 in one PR. So the count of phase 7 is likely below 96.
- Assumption: each PR costs the mean of a code PR of the-thing-below, 68.4 million context tokens (section 4 of `docs/design.md`). Then phase 6 costs about 1.1 billion context tokens for the spaces.
- Recommendation: phase 7 measures the cost of its first space, and the owner then confirms the plan for the other 15.

### Kit pieces

One architecture kit makes the whole level, with two trim sheets (D-117, `docs/game/art-proposals.md`). The three zones use the same kit.

| Kind of piece | Pieces |
|---|---|
| Walls and plate walls | 8 to 12 |
| Arches and pillars | 8 to 12 |
| Vault and ceiling modules | 6 to 10 |
| Floors and edges | 6 to 8 |
| Stairs and ramps | 4 to 6 |
| Doors and window frames | 4 to 6 |
| Trims and caps | 4 to 6 |
| All | 40 to 60 |

- Assumption: the counts come from the shapes of the pick: tall arches, ribbed vaults, stairs, and stacked plate walls. Phase 5 sets the kit grid after the metrics of phase 3.
- Assumption: a zone changes the light, the dressing, and the decals, not the kit.

### Props

| Kind of prop | Props | Source |
|---|---|---|
| Dressing of the cloister | 8 to 10 | original work |
| Dressing of the scriptorium | 10 to 12 | original work |
| Dressing of the vaults | 8 to 10 | original work |
| Focal props | 6 to 10 | Meshy on a paid plan (D-118) |
| Pickups: health, ammo, and fuel | 3 to 5 | original work |
| Doors, switches, and keys | 4 to 6 | original work |
| All | 39 to 53 | |

- Assumption: the pickups are one for health, one for fuel, and one to three for ammo. Phase 4 sets the kinds of ammo.
- Assumption: a focal prop is a reliquary, a lectern, a candle stand, or a chained plate (`docs/game/art-proposals.md`). The Meshy trial of phase 5 makes one to three of them (D-118).
- Each prop has a provenance record (D-119 to D-121).

### The rosters

The rosters cost art and animation in phases 4 and 5, not in phases 6 and 7. They go here, because the length sets their size.

- Four weapons: each one needs a first-person mesh, animations, effects, and audio.
- Four enemy types: each one needs a mesh, animations, and an AI archetype.

### Storage

- Unknown: the LFS size of the level. M-8 at the end of phase 5 gives the size of one room. Recommendation: multiply that size by 16 for a first estimate, and compare it with the free quota of 10 GiB (section 4 of `docs/design.md`).

## What this brief does not set

- The beat chart, the flow diagram, and the layout. Phase 6 holds them.
- The place of each weapon, enemy type, pickup, and secret. Phase 6 holds them.
- The tuning values of each weapon and enemy type. Phase 4 holds them.
- The kind of the harvest tool. Phase 4 proposes it (OQ-24).
