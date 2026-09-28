# Phase roadmap: Phase 1, Engine and toolchain proof

Status: **active focused phase roadmap.** PR-7 adds it. This file gives each item of phase 1 its scope, its exit tests, its review focus, and its questions (D-11, D-18, D-70). It supersedes no earlier file. Written 2026-09-27 in ASD-STE100 (D-17).

The design doc holds the thesis of the game, the system map (section 3), and the cost model (section 4). It also holds the guardrails (section 6) and the global order of every PR (section 8). This file cites each decision by its id and never restates it. The index of this folder is `docs/roadmaps/readme.md`.

External facts: this file states no new external fact. The engine facts come from `docs/research/technology-and-art-pipeline.md`, which the session checked on 2026-09-26. Each PR below checks the facts of its part again, with a date.

Correction of 2026-09-27, PR-8: the owner installs the engine during PR-8 (D-78). The project SSD is case-sensitive, so the engine goes on a case-insensitive volume (F-21, D-81). PR-8 read the pages of Epic again on 2026-09-27. `docs/runbooks/engine-setup.md` holds the links.

Labels: each claim of a plan is evidence (with a link), a recommendation, an assumption, or an unknown.

## 1. Thesis

Phase 1 proves the engine, the toolchain, the source control, and the tests on the Mac and on the Windows PC. It ships no play. At the end of it, a clean clone builds, tests, and packages on both platforms, and the cost model has its first engine numbers.

The order follows dependency. The toolchain comes first, because no engine work can start before the owner installs the pinned versions (D-28, D-69). The project scaffold and its first headless test come next, because each later engine PR builds and tests that project (D-71). The package follows, because it needs a project that builds. The first TSR measurement comes last, because it runs on the package of the Mac.

Hosted runners have no engine (F-8). Each engine PR attaches its Mac logs, and the owner posts the Windows logs (D-31, D-33). The development tools stay on the Mac (D-55).

## 5. Findings that bind this phase

The register in section 5 of `docs/design.md` holds every finding. These rows bind an item of phase 1.

| # | Finding | Binds |
|---|---|---|
| F-3 | Xcode 16.2 is on the Mac, and Unreal Engine 5.8 needs Xcode 26.0 to 26.3 | PR-8: the toolchain check refuses each other Xcode version (D-28) |
| F-4 | The internal disk has 45 GB free | PR-8: the runbook puts the engine and its cache on the project SSD (D-28) |
| F-5 | The Mac has 16 GB of memory, the minimum of Epic | PR-9: the first value of M-1 |
| F-6 | Two Epic pages disagree on the state of Enhanced Input | PR-9: the editor shows the state of the plugin |
| F-8 | Hosted runners have no Unreal Engine | Each engine PR attaches local logs (D-31) |
| F-14 | 60 fps at 4K output on the M4 with 16 GB is a hard target | PR-11: the first value of M-9 |
| F-17 | Unreal cannot build Windows packages on the Mac | PR-9 and PR-10: the owner runs the Windows scripts (D-33, D-72) |
| F-19 | Section 8 of the design doc put the engine install before its runbook | Section 8 below: the install comes during PR-8, from its runbook (D-69, D-78) |
| F-20 | The text of the review commands says that the owner adds the label | PR-8: the text follows D-76 (D-77, D-82) |
| F-21 | The project SSD is case-sensitive, and Unreal Engine does not start from it | PR-8: the runbook adds a case-insensitive volume (D-81) |
| F-22 | The name of an MSVC folder does not give the version of its compiler | PR-8: the Windows check reads `cl.exe` (D-74) |

## 7. Roadmap

Each entry below gives one item of phase 1 its scope, its exit tests, its review focus, and its questions (D-70). An owner step and a measurement get an entry with no PR id. Each entry ends with a plain-English paragraph for a reader who does not know the code.

An exit test is a test or a job that the PR adds and that must pass before the merge. The PR gate of `AGENTS.md` still applies to each PR. These tests are the ones that this PR alone can fail.

### 7.1 PR-8: the engine toolchain and the evidence form

**Scope.**

- A setup runbook for the Mac and for the Windows PC. It marks each owner step. It names the pins of D-28 and D-74, and Git LFS.
- The runbook puts the engine and its cache on the project SSD of the Mac (F-4).
- A `toolchain-check` command in the tools project, for the Mac (D-15, D-55). It reads the Xcode version, the engine version, and the Git LFS version. It fails on each pin that does not hold, and it names the pin, the expected value, and the found value (T-2).
- The command reads the engine folder from `IRON_ABSOLUTION_ENGINE_DIR` (D-79). No commit holds a machine-specific path (D-9, G-6).
- D-28 pins one Xcode version, so the command refuses each other version. Its message names the bounds of F-3.
- The runbook adds a case-insensitive volume on the SSD for the engine, its cache, Xcode, and the checkout (F-21, D-81).
- A PowerShell script that prints the versions of the Windows toolchain (D-72). The owner runs it and posts the output (D-33).
- The evidence form of an engine PR (D-31, D-71), in the PR template (D-80). It has one line for each log: the two toolchain checks, the Mac build, the Windows build, the tests, the package, and each measurement.
- A second concern that the owner accepts (D-77): the text of `review-gate`, `make codex-review`, and their test follows D-76. On the instruction of the owner, the session changes the description of the live label (F-20, D-82).

**Out of scope.**

- Each Unreal file. PR-9 creates the project.
- A check that needs the engine on a hosted runner (D-31).

**Exit tests.**

1. `toolchain-check` passes on the Mac of the owner after the install, before the merge (D-78).
2. Tests with fixtures show that the command refuses Xcode 26.4 and later, and each Xcode version before 26.0 (F-3).
3. Tests show a failure that names the pin when the engine, Xcode, or Git LFS is absent (T-2).
4. The owner runs the PowerShell script on the Windows PC and posts its output in the PR.
5. The `doc-gate` job accepts the PR description with the evidence form.
6. The tests of `review-gate` and `make codex-review` assert the label text of D-76 (F-20).

**Review focus.** The pins against D-28, the error messages against T-2, and no machine-specific path in a commit (G-6). The label text against D-76.

**Questions.** None open. D-74 answers OQ-22, the Windows toolchain pin. The owner answered the questions of the PR in D-78 to D-82.

**State.** ✅ done in PR #9.

> *In plain English:* The Mac has no engine and the wrong Xcode today. This change writes the install steps for both computers. It adds a check that shows each wrong version by name.

### 7.2 The engine install

Owner, during PR-8 (D-78). No PR.

**Scope.**

- The owner installs Unreal Engine 5.8 at the pinned hotfix and Xcode 26.1.1 on the Mac, from the runbook of PR-8 (D-28). Both go on the case-insensitive volume (D-81).
- The owner installs Unreal Engine 5.8 at the same hotfix on the Windows PC, with the toolchain of D-74 (D-33).
- The owner installs Git LFS on both machines.

**Exit tests.**

1. `toolchain-check` passes on the Mac.
2. The PowerShell script of PR-8 shows each Windows pin.

**Review focus.** This step has no PR and no review. The session of PR-8 records the result in its handoff entry (D-78).

**Questions.** None.

**State.** ✅ done on 2026-09-27 on both machines. `make toolchain-check` passed on the Mac, and the script passed on the Windows PC (D-78). Before the install, the Mac had no engine, no Xcode app, and no Git LFS (F-3).

> *In plain English:* The owner installs the engine and its tools on both computers. The steps come from the runbook, so the install proves that the runbook is correct.

### 7.3 PR-9: the project scaffold and the first headless test

**Scope.**

- The project `IronAbsolution` in the `Game/` folder, with one C++ module and the Game and Editor targets (D-39, D-73).
- Enhanced Input on. The session confirms the state of the plugin in the editor (F-6).
- One empty test map. It is the default map of the game and of the editor.
- The World Partition choice of the test map, with the reason (D-4). Recommendation: no World Partition, because the level loads whole.
- LFS attributes for each binary type of Unreal (D-30). Ignore rules for the cache folders of Unreal (D-9).
- One automation test in the module (D-71).
- A Makefile target that builds the editor target on the Mac (D-41). A Makefile target that runs the automation tests headless on the Mac.
- A PowerShell script for each of the two actions on the Windows PC (D-72).
- The test command reads the test report and a success line in the log. An exit code of 0 alone is not a pass. The-thing-below met a headless run that ended with 0 and no success line (finding 64 of its design doc).
- Tests on the hosted runners for the text files: the LFS attributes, the plugin list of the project, and the ignore rules (D-31).
- Unreal best practices govern the C++ and the content (D-34).
- At the first start of the editor, the owner sets the path of the engine cache on the volume (D-83).

**Out of scope.**

- The package. PR-10 adds it.
- Game rules and input actions. Phase 3 adds them.

**Exit tests.**

1. A clean clone builds the editor target on the Mac, with the log in the evidence form (D-31).
2. The owner builds the editor target on the Windows PC with the script, and posts the log (D-33).
3. The editor opens the test map with no error, and it shows Enhanced Input on (F-6).
4. The automation test passes headless on the Mac and on the Windows PC, with both logs.
5. A test that fails on purpose gives a nonzero exit code. The session removes that test before the merge.
6. The test command fails with the path when the test report is absent (T-2).
7. A fresh clone restores the test map from LFS, and the map file is not an LFS pointer.
8. After the first editor build, the engine cache is on the volume. The internal disk holds no cache data (D-83, F-4).
8. The hosted tests pass for the LFS attributes, the plugin list, and the ignore rules.
9. M-1 has a value: the peak memory of the editor on the Mac with the test map open (F-5).
10. M-2 has a value for the clean build of the editor target on both platforms.

**Review focus.** The C++ against the Epic coding standard (D-34), and the pass rule of the test command. Also the LFS and ignore rules (D-9, D-30), and the Windows scripts (D-72).

**Questions.** None open. The session asks each new question when this PR starts.

**State.** 🔧 planned.

> *In plain English:* The repository gets an empty game project with one test. The change proves that the project builds and that the test runs with no screen, on both computers.

### 7.4 PR-10: the packaged build

**Scope.**

- A Makefile target that makes a packaged Development build on the Mac. A PowerShell script that does the same on the Windows PC (D-33, D-72).
- A start command for each package. The package loads the test map, stops after a set time, and writes a success line in its log.
- The start command fails with the log path when the log has no success line (T-2).

**Out of scope.**

- The Shipping configuration, code signing, notarization, installers, and store builds. Phase 8 holds them.

**Exit tests.**

1. A clean clone makes a packaged Development build on the Mac, with the log.
2. The owner makes a packaged Development build on the Windows PC with the script, and posts the log.
3. Each package starts and stops from the command line, and the start command finds the success line.
4. A package run with no success line fails the start command, and the message names the log.
5. M-2 has a value for the package on both platforms.

**Review focus.** The package settings against Unreal best practice (D-34), the pass rule of the start command, and the Windows script (D-72).

**Questions.** None open. The session asks each new question when this PR starts.

**State.** 🔧 planned.

> *In plain English:* The empty game becomes a program that runs outside the editor on both computers. A script starts it, stops it, and checks its log.

### 7.5 PR-11: the first M-9 and the gate record

**Scope.**

- A small original room of basic shapes, with a set count of lights and meshes (D-75).
- A repeatable procedure that runs the Mac package with that room at 4K output with TSR (D-32). It steps through a set of internal resolutions and reads the frame time.
- The first value of M-9, with the command and the log (F-14).
- The value of M-8 at the end of phase 1.
- The values of M-1, M-2, M-8, and M-9 in section 4 of `docs/design.md`.
- The evidence of each line of the gate of phase 1 (section 7.6).

**Out of scope.**

- The frame budget of the game. Phase 3 measures M-3 and M-9 again in the test gym.
- Art. The room holds basic shapes alone.

**Exit tests.**

1. M-9 has a first value, and the PR holds the command and the log.
2. M-8 has a value.
3. Each line of section 7.6 names its evidence.

**Review focus.** The method of the measurement, and the claim that each value makes. A value from a room of basic shapes is a first value, and not the budget of the level (D-32).

**Questions.** None open. D-75 answers OQ-23, the scene of the first M-9 run.

**State.** 🔧 planned.

> *In plain English:* The Mac draws a smaller image, and the engine scales it up to 4K. This change finds the largest image that still gives 60 frames each second.

### 7.6 The gate of phase 1

**The gate.** The gate of phase 1 passes when every line holds:

1. A clean clone builds the editor target on the Mac and on the Windows PC (PR-9).
2. One automation test passes headless on both platforms, with its log (PR-9).
3. A fresh clone restores the LFS content (PR-9).
4. The project records its World Partition choice with the reason (PR-9, D-4).
5. The editor shows the state of Enhanced Input (PR-9, F-6).
6. A clean clone makes a packaged Development build on both platforms, and the owner posts the Windows logs (PR-10, D-33).
7. Each package starts and stops from the command line (PR-10).
8. M-1 and M-2 have values in section 4 of `docs/design.md` (PR-9, PR-10, PR-11).
9. M-9 has a first value (PR-11).
10. The five required checks of `main` are green on each PR of the phase (D-61, D-64).

**What the gate does not ask.** No play, no feel, and no frame budget. Phase 3 holds those (D-32).

> *In plain English:* At this point the game still does nothing that a player can see. But it builds, tests, and runs on both computers, and we know its first costs.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`. Phase 1 holds this order:

1. PR-7: this file.
2. PR-8: the engine toolchain and the evidence form.
3. Owner: the engine install from the runbook of PR-8, during PR-8 (D-69, D-78).
4. PR-9: the project scaffold and the first headless test.
5. PR-10: the packaged build.
6. PR-11: the first M-9 and the gate record.
7. **← GATE of phase 1.** Section 7.6 holds each line.

Phase 2 runs beside phase 1. Its phase file is a separate documents PR.

## 9. Open questions

The register is `docs/questions.md`. These questions block an item of phase 1. Each PR asks its new questions when it starts.

| Question | Subject | Blocks |
|---|---|---|
| OQ-22 | The Windows toolchain pin | PR-8, resolved by D-74 |
| OQ-23 | The scene of the first M-9 run | PR-11, resolved by D-75 |
