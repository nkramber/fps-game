# Phase roadmap: Phase 3, Core-feel prototype

Status: **active focused phase roadmap.** PR-20 adds it. This file gives each item of phase 3 its scope, its exit tests, its review focus, and its questions (D-11, D-18, D-70). It supersedes no earlier file. Written 2026-09-29 in ASD-STE100 (D-17).

The design doc holds the thesis of the game, the system map (section 3), and the cost model (section 4). It also holds the guardrails (section 6) and the global order of every PR (section 8). This file cites each decision by its id and never restates it. The index of this folder is `docs/roadmaps/readme.md`.

External facts: this file states no new external fact. Each engine PR below reads the Epic pages of its part, and names each page with a date.

Labels: each claim of a plan is evidence (with a link), a recommendation, an assumption, or an unknown.

## 1. Thesis

Phase 3 makes movement, aim, and fire feel fast and exact in a graybox gym, before any other system (G-5). It ships the first play of the game. At the end of it, the owner signs off the feel of the packaged gym. M-3 has a profile on Windows, and the movement metrics have values.

The order follows dependency (D-126, D-127). The gym and the movement come first, because each other verb needs a character in a world. The frame-time capture comes second, so that each later PR can show the budget of D-32 (G-12). The aim settings, mantle and interact, the weapon, and the melee attack follow, with one group of verbs in each PR. The feel pass and the gate record come last, because the owner plays the whole gym.

C++ holds the rules, and data assets hold the tuning (D-29). So the feel pass of PR-27 changes data-asset values, and a change of rules gets its own PR (D-131). Unreal best practices govern each implementation (D-34).

Hosted runners have no engine (F-8). Each engine PR attaches its logs in the evidence form (D-31). A session runs the builds and the tests on the Windows PC (D-92). It asks the owner before each command that opens a game window (D-96).

Section 7 of `docs/design.md` let phase 5 run beside phase 3. The owner moved the start of phase 5 after the gate of phase 3, so phase 5 runs beside phase 4 (D-129).

## 5. Findings that bind this phase

The register in section 5 of `docs/design.md` holds every finding. These rows bind an item of phase 3.

| # | Finding | Binds |
|---|---|---|
| F-8 | Hosted runners have no Unreal Engine | Each engine PR attaches local logs (D-31) |
| F-28 | A hosted test with the stub program fails at times after 6 or 7 ms, and its rerun passes | Each PR of the phase: run the failed job again one time, and record each failure in the handoff entry. A later PR fixes the cause |
| F-30 | D-116 puts the verb "change weapon" in phase 3, and section 7 of the design doc gave phase 3 one weapon | PR-25: one weapon rule with two data assets (D-128) |

## 7. Roadmap

Each entry below gives one item of phase 3 its scope, its exit tests, its review focus, and its questions (D-70). Each entry ends with a plain-English paragraph for a reader who does not know the code.

An exit test is a test or a job that the PR adds and that must pass before the merge. The PR gate of `AGENTS.md` still applies to each PR. These tests are the ones that this PR alone can fail.

Each engine PR of this phase also passes these checks, with the logs in the evidence form (D-31, D-80):

1. `run.ps1 verify` passes on the Windows PC.
2. `run.ps1 editor-build` passes.
3. `run.ps1 editor-test` passes, with each automation test headless.
4. `run.ps1 package-build` and `run.ps1 package-run` pass, after the confirmation of the owner (D-96).
5. From PR-23 onward, the frame-time capture of PR-22 passes inside the budget of D-32.

### 7.1 PR-21: the gym, the character, and the movement

**Scope.**

- A gym map with the basic shapes of the engine and metric markers (PR-21). The markers show gaps for a jump, steps, ramps, and halls of set widths.
- Each marker has a label with its size in centimeters.
- The gym is the default map of the game and of the editor. The empty test map stays for the automation tests and the timed run of D-89.
- The start command of `run.ps1 package-run` names the test map in its arguments. The success line names the test map, so a new default map must not break the timed run.
- A player character class in C++, with the character movement component of the engine (D-29, D-34). The player moves at full speed with no run key (D-132), and jumps.
- A first-person camera. The mouse turns the view, for the verb "aim" (D-116).
- Enhanced Input: one input action for each verb of this PR, and one mapping context for the keyboard and the mouse (D-32).
- Recommendation: no C++ names a key. A later gamepad then adds a mapping context alone (OQ-21).
- A data asset holds the tuning of the movement: the speeds, the acceleration, the jump height, the air control, and the camera values (D-29).
- The file `docs/game/movement-metrics.md` gives the first values of the movement metrics, each with a unit (PR-21, D-114). Phase 5 and phase 6 read them.
- Automation tests of the movement, headless through `run.ps1 editor-test`.
- Recommendation: a test puts the character in a test world, gives it input values, and reads its position after a set time. The PR reads the Epic pages of the method, with a date.

**Out of scope.**

- Crouch, dash, and a second jump. D-116 has none of them.
- The aim settings: PR-23. Mantle and interact: PR-24.
- The weapon, and each mesh of the hands: PR-25.
- Each asset from outside the engine. Recommendation: the gym uses the basic shapes of the engine alone, so each new file of the phase is original. Phase 5 adds the provenance records (D-121).
- The gamepad (OQ-21).

**Exit tests.**

1. The player moves, jumps, and aims in the gym, in the editor and in the package (D-96).
2. The automation tests of the movement pass headless, with the log. They read the speed and the jump height from the data asset.
3. A test shows that a new value in the data asset changes the movement, with no change of C++ (D-29).
4. `run.ps1 package-run` still passes on the test map, and it finds the success line.
5. The movement metrics file gives each metric with a value and a unit.

**Review focus.** The character code against the Epic coding standard and Unreal practice (D-34). The split of the rules and the tuning (D-29). The input actions against a later gamepad (OQ-21). The pass rule of the movement tests.

**Questions.** None open. D-132 answers the run speed.

**State.** ✅ done in PR #22. The owner named the gym `L_Gym` (D-133), picked a script for the content (D-134), the first pace (D-135), and the keys (D-136). `docs/research/movement-test-method.md` gives the Epic pages and the test method. Exit test 1 passes. On 2026-09-29, the owner played the gym in the package and in the editor. The player moves, jumps, and aims.

> *In plain English:* The game has no player today, only an empty map. This change adds a room of measured blocks and a player who runs, jumps, and looks around. Tests check the speed and the height of the jump.

### 7.2 PR-22: the frame-time capture and the first value of M-3

**Scope.**

- A frame-time capture of the packaged gym on the Windows PC, at 1440p (D-32).
- A new target of `run.ps1` starts the capture, and a command of the tools project reads the result (D-15, D-99).
- The method of OQ-25: the build configuration, the scene, and the statistic. The owner picks at the start of the PR.
- Recommendation: the CSV profiler of the engine. The PR reads the Epic pages again and records each fact with a date.
- The command fails with the path and the cause when the capture file is absent, empty, or too short (T-2).
- The command compares the statistic with the budget of D-32, 8.33 ms for 120 fps. A value over the budget gives a nonzero exit code, and the message names the value and the budget.
- The first value of M-3 in section 4 of `docs/design.md`, with the commit, the hardware, and the settings of the run.
- Tests of the tools project with fixture files: a good capture, an absent file, an empty file, and a value over the budget.

**Out of scope.**

- A frame-time job in CI. Hosted runners have no engine (F-8).
- Changes for speed. T-1 permits tuning only on a measurement, and this PR gives the first one.
- M-4. Phase 4 measures it in the combat sandbox.

**Exit tests.**

1. The capture runs on the packaged gym after the confirmation of the owner (D-96). The log and the result go in the evidence form.
2. M-3 has a first value in section 4 of `docs/design.md`.
3. Tests show a failure that names the path and the cause for each bad capture (T-2).
4. A test shows a nonzero exit code for a value over the budget, and the message names both numbers.

**Review focus.** The method against the Epic pages, with dates. The statistic against D-32. The error messages against T-2.

**Questions.** OQ-25, the method of M-3. The owner picks at the start of the PR.

**State.** 🔧 planned.

> *In plain English:* We do not know yet how fast the game runs. This change adds one command that plays the gym and records the time of each frame. It compares that time with the target of 120 frames each second.

### 7.3 PR-23: the aim settings

**Scope.**

- The mouse sensitivity, the invert of the vertical aim, and the field of view (section 7 of `docs/design.md`).
- The game keeps each value in the settings file of the user, and reads it at the start (D-34). Recommendation: a subclass of the user settings class of the engine.
- A console command for each value, so that the owner can change it in the package. Phase 8 adds the menu.
- Bounds for each value. A value out of bounds gives an error in the log with the name, the value, and the bounds (T-2).
- The default values come from the config of the project, not from a literal in the code (D-29, D-34).
- Tests of each value: it changes the aim or the view, and it comes back after a restart.
- A test of the bounds: a value out of bounds gives the error.

**Out of scope.**

- The menu, the key remap, and the graphics options. Phase 8 holds them.
- The gamepad sensitivity (OQ-21).

**Exit tests.**

1. Each value changes the aim or the view in the package (D-96).
2. The tests pass headless, with the log.
3. A value out of bounds gives the error, and the game keeps the last good value.

**Review focus.** The settings class against Unreal practice (D-34). The bounds and the text of each error (T-2).

**Questions.** None open.

**State.** 🔧 planned.

> *In plain English:* Each player aims in a different way. This change lets the player set how fast the view turns, turn the vertical aim upside down, and set the width of the view.

### 7.4 PR-24: mantle and interact

**Scope.**

- Mantle: the player climbs onto a ledge at chest height (D-116). A data asset holds the band of heights and the time of the climb (D-29).
- Recommendation: a trace in front of the player finds a ledge in the band. The PR reads the Epic pages of the method, with a date (D-34).
- The gym gets four ledges: one below the band, one at each bound, and one above it.
- Interact: the player uses a switch or a door in reach (D-116). An interface in C++ lets a later actor take the verb.
- A test switch and a test door in the gym. A cue shows when a target is in reach.
- The movement metrics file gets the band of the mantle.
- Tests: a mantle at each bound, no mantle above the band, and an interact in reach and out of reach.

**Out of scope.**

- Keys, locked doors, and pickups. Phase 4 and phase 6 hold them.
- The finish of a stunned enemy (D-116). Phase 4 holds it.
- The art of each door and switch. Phase 5 and phase 7 hold it.

**Exit tests.**

1. The player climbs each ledge in the band, and no ledge above it, in the package (D-96).
2. The player opens the test door with the test switch.
3. The tests pass headless, with the log.
4. The movement metrics file gives the band of the mantle, with a unit.

**Review focus.** The trace of the mantle against the collision of the gym and Unreal practice (D-34). The interface of interact against the doors, the switches, and the pickups of later phases.

**Questions.** None open.

**State.** 🔧 planned.

> *In plain English:* The player cannot climb or use a switch yet. This change lets the player climb onto a ledge at chest height and use a switch that opens a door.

### 7.5 PR-25: the weapon, the ammo, and change weapon

**Scope.**

- One weapon rule in C++: the shot, a hitscan trace from the view, and the hit (D-29, D-130).
- Two data assets of the rule, with different tuning: for example the rate of fire, the spread, and the ammo. The verb "change weapon" switches between them (D-128).
- Ammo for each weapon. Each shot spends one round, and an empty weapon does not fire. No weapon needs a reload (D-115).
- A gym target that counts and shows each hit. Recommendation: the target has no health, because phase 4 builds damage and health.
- Hit feedback: a hit marker, and an effect at the point of the hit. The sound follows the answer to OQ-26.
- A small HUD: a crosshair, and the ammo of the weapon in hand (the pillar "Every round counts" of `docs/game/pillars.md`).
- A mesh of basic shapes for the weapon in the view.
- Tests of the hit: a hit on a target in the line, and a miss on a target out of the line.
- Tests of the ammo: one round for each shot, and no shot from an empty weapon.
- A test of change weapon: the other data asset gives its tuning after the change.

**Out of scope.**

- The weapon roster of D-124, the projectile rule, damage, health, the ammo pickups, and the harvest tool. Phase 4 holds them.
- The final art, effects, and sound. Phase 5 and phase 7 hold them.

**Exit tests.**

1. In the package, the player fires, hits the gym target, sees the hit feedback, and spends all the ammo (D-96).
2. Change weapon switches between the two data assets, and the HUD shows the ammo of the weapon in hand.
3. The tests pass headless, with the log.
4. A test shows that a third data asset makes a third tuning, with no change of C++ (D-29).

**Review focus.** The trace against Unreal practice (D-34). The ammo rule against D-115. The split of the rule and the data assets (D-29, D-128).

**Questions.** OQ-26, the sound of the gym. The owner answers at the start of the PR.

**State.** 🔧 planned.

> *In plain English:* The player cannot shoot yet. This change adds one gun with two sets of values, a target that shows each hit, and a count of the ammo. The gun needs no reload.

### 7.6 PR-26: the melee attack

**Scope.**

- The melee attack: a close attack at short range (D-116). A data asset holds the range and the time between two attacks (D-29).
- The gym target counts melee hits and shots apart.
- Hit feedback for the melee attack, of the same kind as PR-25.
- Recommendation: the attack code lets phase 4 add the finish of a stunned enemy with no rewrite (D-115, D-116).
- Tests: a hit in range, a miss out of range, and no second attack before the set time.

**Out of scope.**

- The finish of a stunned enemy that gives health. Phase 4 holds it (D-115, D-116).
- Damage and health. Phase 4 holds them.

**Exit tests.**

1. The player hits the gym target with a melee attack in the package (D-96).
2. The tests pass headless, with the log.

**Review focus.** The attack against Unreal practice (D-34). The place for the finish of phase 4.

**Questions.** None open.

**State.** 🔧 planned.

> *In plain English:* The player can only shoot today. This change adds a close attack. In phase 4, the same attack finishes a stunned enemy for health.

### 7.7 PR-27: the feel pass and the gate record

**Scope.**

- The owner plays the packaged gym, and a D-# row records the sign-off or a list of changes (section 7 of `docs/design.md`).
- A change of data-asset values stays in this PR, and the owner plays again. A list that needs a change of C++ rules follows D-131.
- The final values of the movement metrics, from the data assets.
- M-3 at the gate, with the method of PR-22.
- The value of M-8 at the end of phase 3 (D-108).
- The evidence of each line of the gate of phase 3 (section 7.8).

**Out of scope.**

- A new verb or a new rule. A change of C++ rules gets its own PR (D-131).

**Exit tests.**

1. A D-# row records the sign-off of the owner.
2. Each value of the movement metrics file is the same as the value in its data asset.
3. M-3 and M-8 have values at the end of phase 3.
4. Each line of section 7.8 names its evidence.

**Review focus.** Each change of tuning against D-131. The evidence of each line of the gate.

**Questions.** None open. The owner gives the sign-off during the PR.

**State.** 🔧 planned.

> *In plain English:* The owner plays the gym and says if moving and shooting feel right. Small changes of numbers go in this change. It also collects the proof that phase 3 is complete.

### 7.8 The gate of phase 3

**The gate.** The gate of phase 3 passes when every line holds:

1. After a play of the packaged gym, a D-# row records the feel sign-off of the owner (PR-27).
2. M-3 has a profile on Windows inside the budget of D-32, with the method of OQ-25 (PR-22, PR-27).
3. The movement metrics file gives each metric with a value and a unit (PR-21, PR-24, PR-27).
4. In the gym, the player can move, jump, aim, shoot, change weapon, melee, mantle, and interact (D-116, PR-21 to PR-26).
5. The automation tests of the movement and the weapon pass headless, with the log (PR-21 to PR-26).
6. A clean clone makes a packaged Development build, and the timed run passes (D-89).
7. M-8 has a value at the end of phase 3 (D-108).
8. The five required checks of `main` are green on each PR of the phase (D-61, D-64).

**State.** 🔧 planned.

**What the gate does not ask.** No enemy, no damage, no health, and no final art or sound. Phase 4 and phase 5 hold them. The verb "melee" needs the attack alone, because phase 4 builds the finish (D-116).

> *In plain English:* At this point, the player can move and shoot in a plain room, and the owner says that it feels right. The game still has no enemies.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`. Phase 3 holds this order (D-126, D-127):

1. PR-20: this file.
2. PR-21: the gym, the character, and the movement.
3. PR-22: the frame-time capture and the first value of M-3. The owner answers OQ-25.
4. PR-23: the aim settings.
5. PR-24: mantle and interact.
6. PR-25: the weapon, the ammo, and change weapon. The owner answers OQ-26.
7. PR-26: the melee attack.
8. PR-27: the feel pass and the gate record.
9. **← GATE of phase 3.** Section 7.8 holds each line.

After the gate, the phase file of phase 5 comes in its own PR, and phase 5 runs beside phase 4 (D-129).

## 9. Open questions

The register is `docs/questions.md`. These questions block an item of phase 3. Each PR asks its new questions when it starts.

| Question | Subject | Blocks |
|---|---|---|
| OQ-25 | The method of M-3 | PR-22 |
| OQ-26 | The sound of the gym | The sound part of PR-25 |
| OQ-21 | Gamepad support | Phase 8. PR-21 keeps the input ready for it |
