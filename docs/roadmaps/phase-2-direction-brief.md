# Phase roadmap: Phase 2, Game direction and the level brief

Status: **active focused phase roadmap.** PR-15 adds it. This file gives each item of phase 2 its scope, its exit tests, its review focus, and its questions (D-11, D-18, D-70). It supersedes no earlier file. Written 2026-09-29 in ASD-STE100 (D-17).

The design doc holds the thesis of the game, the system map (section 3), and the cost model (section 4). It also holds the guardrails (section 6) and the global order of every PR (section 8). This file cites each decision by its id and never restates it. The index of this folder is `docs/roadmaps/readme.md`.

External facts: this file states no new external fact. The Meshy facts come from `docs/research/technology-and-art-pipeline.md`, which the session checked on 2026-09-26. PR-18 checked them again on 2026-09-29, in section 6 of `docs/game/provenance.md`.

Labels: each claim of a plan is evidence (with a link), a recommendation, an assumption, or an unknown.

## 1. Thesis

Phase 2 turns the intent of the owner into a short brief that a test can check. It ships no code and no asset. At the end of it, a decision row holds each pick of the owner, and the brief gives numbers for the level.

The order follows dependency (D-112). The pillars and the core loop come first, because phase 3 needs the player verbs, and phase 3 is on the critical path. The art direction comes second. The provenance policy and the Meshy choice come third, because the Meshy choice waits for the art direction (OQ-12). The brief comes last, because its roster sizes and its content cost need both picks.

The owner picks from the proposals during the PR that gives them, and that PR records the pick (D-113). The documents of the game direction go in the folder `docs/game/`, and PR-16 creates it (D-114). Each PR of this phase changes documents alone. So the `review-override` label replaces the Codex review (D-35, D-66, D-76).

Section 7 of `docs/design.md` let phase 2 run beside phase 1. No PR of phase 2 started before the gate of phase 1, so this phase starts after that gate. The start of phase 3 does not change, because phase 3 needs the gates of both phases.

## 5. Findings that bind this phase

The register in section 5 of `docs/design.md` holds every finding. These rows bind an item of phase 2.

| # | Finding | Binds |
|---|---|---|
| F-7 | The Meshy plugin has Windows builds for Unreal Engine 5.4 to 5.7 only. Its bridge needs Meshy Pro | PR-18: a new check of the facts, with a date, before the owner answers OQ-12 |
| F-15 | A level of 30 minutes or more multiplies the content cost of phases 6 and 7 | PR-19: the brief states the content cost of the length (D-37) |
| F-16 | MIT covers the whole repository. An asset with terms that forbid redistribution, or free Meshy output under CC BY, cannot enter it as MIT content | PR-18: the provenance policy gives the rule for each license (D-24, D-38) |

## 7. Roadmap

Each entry below gives one item of phase 2 its scope, its exit tests, its review focus, and its questions (D-70). Each entry ends with a plain-English paragraph for a reader who does not know the code.

An exit test is a check that the PR must pass before the merge. The PR gate of `AGENTS.md` still applies to each PR. Each PR of this phase changes documents alone, so the `ste-check` job replaces the tests (T-3).

### 7.1 PR-16: the pillars, the core loop, and the recovery mechanic

**Scope.**

- The pillars of the game, and the core loop of a fight, in `docs/game/pillars.md` (PR-16). A recommendation: three to five pillars, each one sentence that a design choice can test.
- The list of player verbs. Section 1 of `docs/design.md` gives a proposal: move, jump, aim, shoot, change weapon, and perhaps dash or melee. The owner confirms the list as a decision row, because phase 3 builds it.
- Two or three original proposals for the limited resources and the recovery mechanic, in `docs/game/combat-proposals.md` (PR-16, D-36, OQ-10). Each proposal gives:
  - the limited resources, and how the player spends each one.
  - the action of the player that gives resources back in a fight.
  - the tension that the mechanic makes, and how it rewards attack. OQ-10 recommends a mechanic that rewards attack.
  - its risks, and its cost in rule code and in content.
  - a first test for the sandbox of phase 4 (D-36).
  - a note on originality: the proposal copies no mechanic of a reference game (D-2, G-1).
- The pick of the owner as a decision row. OQ-10 gets its resolved mark in the same PR (D-113).
- Section 1 of `docs/design.md`: the rows of the player verbs and of the limited resources cite the new decisions.

**Out of scope.**

- The numbers of the weapons, the enemies, and the resources. PR-19 gives the roster sizes, and phase 4 tunes the values.
- Rule code. Phase 3 builds the verbs, and phase 4 builds the recovery mechanic.
- The setting and the art. PR-17 holds them.

**Exit tests.**

1. The `ste-check` job passes, and the `doc-gate` job accepts the Documents section.
2. Each proposal gives each part of the scope list, with a label for each claim.
3. Each proposal names the reference games that it is not like, and why (D-2).
4. A decision row records the pick, and OQ-10 has the mark "Resolved" with that D-# id (D-113).
5. A decision row records the list of player verbs.

**Review focus.** The originality of each proposal (D-2, G-1). The fit of each proposal with D-1 and D-37. The claim that each first test can run in the sandbox of phase 4.

**Questions.** OQ-10, the pick of the owner. The session asks it during the PR (D-113).

**State.** ✅ done in PR #17. Correction of 2026-09-29: the PR gave four proposals, not two or three. The owner asked for a fourth during the PR. The owner then picked none of them, and the combat rules follow Doom (2016) (D-115). D-116 records the player verbs. Exit test 3 applies to the four proposals.

> *In plain English:* The game has no rules for its resources today. This change writes two or three ideas for how the player gets ammo or health back in a fight. The owner picks one, and a test room tries it later.

### 7.2 PR-17: the setting, the tone, and the art direction

**Scope.**

- Two or three original proposals for the setting, the tone, and the visual style, in `docs/game/art-proposals.md` (PR-17, D-36, OQ-9). Each proposal gives:
  - the setting and the tone in a short paragraph, and how they fit the pillars of PR-16.
  - the visual style: shapes, materials, palette, and light mood.
  - its content cost for a small team. OQ-9 recommends a style that modular kits and trim sheets can make.
  - its fit with the budget of the Windows PC (D-32).
  - what it asks from Meshy, if anything (OQ-12, D-8).
  - a note on originality: it copies no name, asset, or look of a reference game (D-2, G-1).
- The proposals use words and original sketches alone. No image of another game or artist goes into the repository (D-2, D-24).
- The pick of the owner as a decision row. OQ-9 gets its resolved mark in the same PR (D-113).
- Section 1 of `docs/design.md`: the row of the setting, tone, and art direction cites the new decision.

**Out of scope.**

- Each asset, and the kit grid. Phase 5 sets the grid after the gate of phase 3.
- The light method. Phase 5 chooses it by a measurement (M-5).
- The Meshy choice. PR-18 holds it.

**Exit tests.**

1. The `ste-check` job passes, and the `doc-gate` job accepts the Documents section.
2. Each proposal gives each part of the scope list, with a label for each claim.
3. No file of the PR holds an image from another game or artist (D-2).
4. A decision row records the pick, and OQ-9 has the mark "Resolved" with that D-# id (D-113).

**Review focus.** The originality of each proposal (D-2, G-1). The content cost against F-15 and the likely art needs of `docs/research/technology-and-art-pipeline.md`. The fit with the budget of D-32.

**Questions.** OQ-9, the pick of the owner. The session asks it during the PR (D-113).

**State.** ✅ done in PR #18. The owner picked proposal A, Penitent Iron (D-117).

> *In plain English:* The game has no look and no world yet. This change writes two or three ideas for them, and the owner picks one. The art work of phase 5 then follows that pick.

### 7.3 PR-18: the provenance policy and the Meshy choice

**Scope.**

- The provenance policy for art and audio, in `docs/game/provenance.md` (PR-18). It gives:
  - the fields of a provenance record: the source, the license, the author or the tool, and the date.
  - the rule for each kind of source: original work, a free license, a bought asset, and AI generation (D-24, D-38).
  - the rule for a license that forbids redistribution or asks for attribution (F-16).
  - the place of the manifest in the repository, and the check that phase 5 adds.
- A new check of the Meshy facts, with a source and a date (F-7). The session reads the plans, the terms, and the Unreal plugin again.
- The Meshy choice of the owner as a decision row (OQ-12, D-8). The owner can also defer it again with a date. The session records the deferral in `docs/questions.md`.

**Out of scope.**

- Each spend on Meshy, and each Meshy trial (D-8). Phase 5 holds the trial.
- The manifest file and its import checks. Phase 5 builds them.
- The audio pipeline. Phase 5 holds it (D-38).

**Exit tests.**

1. The `ste-check` job passes, and the `doc-gate` job accepts the Documents section.
2. The policy gives one rule for each kind of source, and each rule cites its decision.
3. Each Meshy fact has a source and a date.
4. OQ-12 has an answer as a decision row, or a dated deferral of the owner.
5. The row of F-16 in `docs/design.md` names the rule of the policy.

**Review focus.** The policy against D-2, D-24, and D-38. Each external fact against its source.

**Questions.** OQ-12, the Meshy choice. It needs the pick of PR-17 first.

**State.** ✅ done in PR #19. The owner picked Meshy on a paid plan, for focal props alone (D-118). The owner also gave the rules of the policy (D-119 to D-121).

> *In plain English:* The game has no rules yet for where its art and sound come from. This change writes those rules, so that each file has a record of its source and its license.

### 7.4 PR-19: the level brief and the gate record

**Scope.**

- The level brief, in `docs/game/level-brief.md` (PR-19). It gives numbers, each with a unit and a label:
  - the range of the first-clear time. The low end is 30 minutes (D-37). M-7 in phase 6 reads this range.
  - the number of combat spaces, and the other spaces.
  - the size of the weapon roster and of the enemy roster.
  - the number of secrets and of checkpoints.
  - the content cost of the length for phases 6 and 7 (F-15): the kit pieces, the props, and the spaces.
- Section 1 of `docs/design.md`: the rows of the rosters, the spaces, and the secrets cite the brief.
- The value of M-8 at the end of phase 2 (D-108).
- The evidence of each line of the gate of phase 2 (section 7.5).

**Out of scope.**

- The beat chart, the flow diagram, and the layout. Phase 6 holds them.
- The tuning values of each weapon and enemy. Phase 4 holds them.

**Exit tests.**

1. The `ste-check` job passes, and the `doc-gate` job accepts the Documents section.
2. Each target of the brief is a number or a range, with a unit.
3. The brief states the content cost of the length (F-15).
4. M-8 has a value at the end of phase 2.
5. Each line of section 7.5 names its evidence.

**Review focus.** The numbers against D-37 and the picks of PR-16 and PR-17. The content cost against F-15. The evidence of each line of the gate.

**Questions.** None open now. The brief needs the decision rows of PR-16 and PR-17. The owner confirms the numbers of the brief during the PR.

**State.** 🔧 planned.

> *In plain English:* The level has no size yet. This change sets its length, its number of fights, and its number of weapons and enemies. It also collects the proof that phase 2 is complete.

### 7.5 The gate of phase 2

**The gate.** The gate of phase 2 passes when every line holds:

1. A decision row records the list of player verbs (PR-16, D-111).
2. A decision row records the pick of the owner for OQ-10 (PR-16, D-36).
3. A decision row records the pick of the owner for OQ-9 (PR-17, D-36).
4. The level brief gives the first-clear time, the combat spaces, and the roster sizes as numbers (PR-19, D-37).
5. The brief states the content cost of the length (PR-19, F-15).
6. M-8 has a value at the end of phase 2 (PR-19, D-108).
7. The five required checks of `main` are green on each PR of the phase (D-61, D-64).

**State.** 🔧 planned. PR-19 records the evidence of each line.

**What the gate does not ask.** No play, no code, and no asset. An answer to OQ-12 is not a line, because the owner can defer it again (PR-18). The provenance policy is complete when PR-18 merges.

> *In plain English:* At this point the game still has no play. But the owner chose how it fights and how it looks, and the level has a size that a test can check.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`. Phase 2 holds this order (D-111, D-112):

1. PR-15: this file.
2. PR-16: the pillars, the core loop, and the recovery mechanic. The owner picks for OQ-10.
3. PR-17: the setting, the tone, and the art direction. The owner picks for OQ-9.
4. PR-18: the provenance policy and the Meshy choice. The owner answers or defers OQ-12.
5. PR-19: the level brief and the gate record.
6. **← GATE of phase 2.** Section 7.5 holds each line.

## 9. Open questions

The register is `docs/questions.md`. These questions block an item of phase 2. Each PR asks its new questions when it starts.

| Question | Subject | Blocks |
|---|---|---|
| OQ-10 | The combat loop with limited resources. Resolved 2026-09-29: D-115 | PR-16, and the gate of phase 2 |
| OQ-9 | Setting, tone, and art direction. Resolved 2026-09-29: D-117 | PR-17, and the gate of phase 2 |
| OQ-12 | Meshy and its plan. Resolved 2026-09-29: D-118 | PR-18 |
