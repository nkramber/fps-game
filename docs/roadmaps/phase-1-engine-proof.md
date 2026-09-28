# Phase roadmap: Phase 1, Engine and toolchain proof

Status: **active focused phase roadmap.** PR-7 adds it. This file gives each item of phase 1 its scope, its exit tests, its review focus, and its questions (D-11, D-18, D-70). It supersedes no earlier file. Written 2026-09-27 in ASD-STE100 (D-17).

The design doc holds the thesis of the game, the system map (section 3), and the cost model (section 4). It also holds the guardrails (section 6) and the global order of every PR (section 8). This file cites each decision by its id and never restates it. The index of this folder is `docs/roadmaps/readme.md`.

External facts: this file states no new external fact. The engine facts come from `docs/research/technology-and-art-pipeline.md`, which the session checked on 2026-09-26. Each PR below checks the facts of its part again, with a date.

Correction of 2026-09-27, PR-8: the owner installs the engine during PR-8 (D-78). The project SSD is case-sensitive, so the engine goes on a case-insensitive volume (F-21, D-81). PR-8 read the pages of Epic again on 2026-09-27. `docs/runbooks/engine-setup.md` holds the links. D-97 later superseded D-81.

Correction of 2026-09-28, PR-12: the game ships on Windows alone (D-91), and the sessions run on the Windows PC (D-92). PR-12 to PR-14 come before PR-11 (D-93, D-94). M-9 goes out of scope (D-95). The entries of PR-8 to PR-10 keep their Mac text as history.

Labels: each claim of a plan is evidence (with a link), a recommendation, an assumption, or an unknown.

## 1. Thesis

Phase 1 proves the engine, the toolchain, the source control, and the tests on the Mac and on the Windows PC. It ships no play. At the end of it, a clean clone builds, tests, and packages on both platforms, and the cost model has its first engine numbers.

The order follows dependency. The toolchain comes first, because no engine work can start before the owner installs the pinned versions (D-28, D-69). The project scaffold and its first headless test come next, because each later engine PR builds and tests that project (D-71). The package follows, because it needs a project that builds. The first TSR measurement comes last, because it runs on the package of the Mac.

Correction of 2026-09-28: phase 1 now proves the engine on the Windows PC alone (D-91). The move to Windows comes before the gate record, because each later session runs on the Windows PC (D-92, D-93). PR-13 moves the development tools first, so that the sessions of PR-14 and PR-11 have their commands (D-94). The gate record has no frame time (D-95).

Hosted runners have no engine (F-8). Each engine PR attaches its Mac logs, and the owner posts the Windows logs (D-31, D-33). The development tools stay on the Mac (D-55). Correction of 2026-09-28: D-92 supersedes D-55. A session on the Windows PC runs the builds and attaches the logs. It asks the owner before a command that opens a game window (D-96).

## 5. Findings that bind this phase

The register in section 5 of `docs/design.md` holds every finding. These rows bind an item of phase 1.

| # | Finding | Binds |
|---|---|---|
| F-3 | Xcode 16.2 is on the Mac, and Unreal Engine 5.8 needs Xcode 26.0 to 26.3 | PR-8: the toolchain check refuses each other Xcode version (D-28). PR-14 removes the Xcode pin (D-97) |
| F-4 | The internal disk has 45 GB free | PR-8: the runbook puts the engine and its cache on the project SSD (D-28). The Windows PC keeps the default cache place (D-100) |
| F-5 | The Mac has 16 GB of memory, the minimum of Epic | PR-9: the first value of M-1. PR-14: a first value on the Windows PC (D-98) |
| F-6 | Two Epic pages disagree on the state of Enhanced Input | PR-9: the editor shows the state of the plugin. Done in PR #10: Enhanced Input 1.0 is on, with no Beta or Experimental label |
| F-8 | Hosted runners have no Unreal Engine | Each engine PR attaches local logs (D-31) |
| F-14 | 60 fps at 4K output on the M4 with 16 GB is a hard target | Out of scope since 2026-09-28 (D-95) |
| F-17 | Unreal cannot build Windows packages on the Mac | PR-9 and PR-10: the owner runs the Windows scripts (D-33, D-72). Out of scope since 2026-09-28 (D-91) |
| F-19 | Section 8 of the design doc put the engine install before its runbook | Section 8 below: the install comes during PR-8, from its runbook (D-69, D-78) |
| F-20 | The text of the review commands says that the owner adds the label | PR-8: the text follows D-76 (D-77, D-82) |
| F-21 | The project SSD is case-sensitive, and Unreal Engine does not start from it | PR-8: the runbook adds a case-insensitive volume (D-81). PR-14 removes it (D-97) |
| F-22 | The name of an MSVC folder does not give the version of its compiler | PR-8: the Windows check reads `cl.exe` (D-74) |
| F-23 | The editor starts its cache server before Editor Preferences can open | PR-9: the owner writes the cache path before the first start (D-85). PR-14 removes it (D-97) |
| F-24 | The toolchain check passed with no Metal Toolchain | PR-9: the check has a Metal Toolchain pin (D-87). PR-14 removes it (D-97) |

## 7. Roadmap

Each entry below gives one item of phase 1 its scope, its exit tests, its review focus, and its questions (D-70). An owner step and a measurement get an entry with no PR id. Each entry ends with a plain-English paragraph for a reader who does not know the code.

An exit test is a test or a job that the PR adds and that must pass before the merge. The PR gate of `AGENTS.md` still applies to each PR. These tests are the ones that this PR alone can fail.

### 7.1 PR-8: the engine toolchain and the evidence form

**Scope.**

- A setup runbook for the Mac and for the Windows PC. It marks each owner step. It names the pins of D-28 and D-74, and Git LFS.
- The runbook puts the engine and its cache on the project SSD of the Mac (F-4).
- A `toolchain-check` command in the tools project, for the Mac (D-15, D-55, superseded by D-92). It reads the Xcode version, the engine version, and the Git LFS version. It fails on each pin that does not hold, and it names the pin, the expected value, and the found value (T-2).
- The command reads the engine folder from `IRON_ABSOLUTION_ENGINE_DIR` (D-79). No commit holds a machine-specific path (D-9, G-6).
- D-28 pins one Xcode version, so the command refuses each other version. Its message names the bounds of F-3.
- The runbook adds a case-insensitive volume on the SSD for the engine, its cache, Xcode, and the checkout (F-21, D-81, superseded by D-97).
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

- The owner installs Unreal Engine 5.8 at the pinned hotfix and Xcode 26.1.1 on the Mac, from the runbook of PR-8 (D-28). Both go on the case-insensitive volume (D-81, superseded by D-97).
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
- Enhanced Input on. The session confirms the state of the plugin in the editor (F-6). The Android File Server plugin off (D-88).
- One empty test map. It is the default map of the game and of the editor.
- The World Partition choice of the test map, with the reason (D-4). The owner chose no World Partition, because the level loads whole (D-84).
- LFS attributes for each binary type of Unreal, and for the source formats of meshes, images, and sounds (D-30, D-86). Ignore rules for the cache folders of Unreal (D-9).
- One automation test in the module (D-71).
- A Makefile target that builds the editor target on the Mac (D-41). A Makefile target that runs the automation tests headless on the Mac.
- A PowerShell script for each of the two actions on the Windows PC (D-72).
- The test command reads the test report and a success line in the log. An exit code of 0 alone is not a pass. The-thing-below met a headless run that ended with 0 and no success line (finding 64 of its design doc).
- Tests on the hosted runners for the text files: the LFS attributes, the plugin list of the project, and the ignore rules (D-31).
- Unreal best practices govern the C++ and the content (D-34).
- Before the first start of the editor, the owner writes the path of the engine cache on the volume (D-83, D-85, both superseded by D-97). The editor starts its cache server before Editor Preferences can open (F-23).
- A fourth pin of `toolchain-check`: the Metal Toolchain of Xcode is installed (D-87, superseded by D-97). The check passed with no Metal Toolchain, and the editor then compiled no shader (F-24).

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
8. After the first editor build, the engine cache is on the volume. The internal disk holds no cache data (D-83, superseded by D-97, F-4).
9. The hosted tests pass for the LFS attributes, the plugin list, and the ignore rules.
10. M-1 has a value: the peak memory of the editor on the Mac with the test map open (F-5).
11. M-2 has a value for the clean build of the editor target on both platforms.

**Review focus.** The C++ against the Epic coding standard (D-34), and the pass rule of the test command. The Metal Toolchain pin (D-87, superseded by D-97). Also the LFS and ignore rules (D-9, D-30), and the Windows scripts (D-72).

**Questions.** None open. The owner answered the questions of the PR in D-84 to D-88.

**State.** ✅ done in PR #10.

> *In plain English:* The repository gets an empty game project with one test. The change proves that the project builds and that the test runs with no screen, on both computers.

### 7.4 PR-10: the packaged build

**Scope.**

- A Makefile target that makes a packaged Development build on the Mac. A PowerShell script that does the same on the Windows PC (D-33, D-72).
- A start command for each package. The package loads the test map, stops after a set time, and writes a success line in its log.
- The start command fails with the log path when the log has no success line (T-2).
- The set time of the run is 10 seconds after the map loads, and the start command stops a package after 5 minutes (D-89). A game subsystem reads the option of the run, and two automation tests cover it.
- The automation test of PR-9 compiles in the game target. It read a field of the editor alone (F-25).
- The `-package` step of the Mac build, so the app holds its libraries (F-26). The Mac start command reads the log from stdout, because the App Sandbox stops `-abslog` (F-27).
- The bundle id of the Mac app (D-90, superseded by D-97).

**Out of scope.**

- The Shipping configuration, code signing, notarization, installers, and store builds. Phase 8 holds them.

**Exit tests.**

1. A clean clone makes a packaged Development build on the Mac, with the log.
2. The owner makes a packaged Development build on the Windows PC with the script, and posts the log.
3. Each package starts and stops from the command line, and the start command finds the success line.
4. A package run with no success line fails the start command, and the message names the log.
5. M-2 has a value for the package on both platforms.

**Review focus.** The package settings against Unreal best practice (D-34), the pass rule of the start command, and the Windows script (D-72).

**Questions.** None open. The owner answered the questions of the PR in D-89 and D-90. D-97 later superseded D-90.

**State.** ✅ done in PR #11.

> *In plain English:* The empty game becomes a program that runs outside the editor on both computers. A script starts it, stops it, and checks its log.

### 7.5 PR-12: the move to Windows alone, with its decisions and its plan

**Scope.**

- The decisions of the move: D-91 to D-100. The notes on each earlier decision that they supersede or revise.
- Each live citation of a superseded decision names the decision that superseded it.
- The design doc: the platform, the frame budget, G-12, the gates of phases 1, 3, 5, 8, and 9, and the order of section 8.
- This file: the entries of PR-12 to PR-14, the new scope of PR-11, the gate, and the order.
- The agent files: the rule of the platforms and the sessions (D-91, D-92, D-96).

**Out of scope.**

- Each change of code, Makefile, script, engine setting, and runbook. PR-13 and PR-14 hold them.

**Exit tests.**

1. The `ste-check` job passes. Its reference check reads each citation of a superseded decision.
2. The `doc-gate` job accepts the Documents section.
3. The entries of PR-13 and PR-14 name each file that holds a Mac part.

**Review focus.** Each decision against the words of the owner. The PR changes documents alone, so the `review-override` label replaces the Codex review (D-35, D-76).

**Questions.** None open. The owner gave D-91 to D-100.

**State.** ✅ done in PR #12.

> *In plain English:* The owner stopped the Mac version of the game. This change records that choice and plans the move of all work to the Windows PC.

### 7.6 PR-13: the development tools on the Windows PC

**Scope.**

- A PowerShell entry script with the development targets of the Makefile: `verify` as the default, `build`, `test`, `format`, `ste-check`, `handoff-rotate`, `codex-review`, `hooks`, `where`, and `clean` (D-99).
- The Makefile keeps the Mac engine targets alone until PR-14 removes it. This split is a recommendation of PR-12.
- The `codex-review` target starts the Codex CLI on the Windows PC (D-14, D-47).
- The pre-commit hook runs on the Windows PC (D-43).
- Each document, skill, tool message, and test that names a `make` target of the development tools names the entry target instead. On 2026-09-28, the agent files, four skills, the runbooks, the pre-commit hook, and the review commands of the tools project held such names.
- `docs/runbooks/session-context.md` gives the commands of a session on the Windows PC. It marks each owner step, such as the install of the .NET SDK of `global.json`, the Codex CLI, the GitHub CLI, and Git LFS.

**Out of scope.**

- The engine targets and the Mac removal. PR-14 holds them.
- A change of the hosted jobs. They call `dotnet` and no Makefile target (D-42).

**Exit tests.**

1. The `verify` target of the entry passes on the Windows PC: build, test, format, and ste-check.
2. `codex-review` gives the review of PR-13 from the Windows PC, and the review record approves the effective head (D-14).
3. On the Windows PC, the pre-commit hook refuses a commit on `main` (D-43).
4. Tests show that each target of the entry fails with its context when a start condition fails (T-2).
5. No live document names a development target of the Makefile.
6. The five required checks of `main` stay green (D-61, D-64).

**Review focus.** The targets of the entry against the targets of D-41. The error messages against T-2. The start of Codex on Windows (D-47).

**Questions.** None open. The session asks each new question at its start.

**State.** 🔧 planned.

> *In plain English:* The tools that check, test, and review each change move from the Mac to the Windows PC. After this change, every session runs on that PC.

### 7.7 PR-14: the engine commands on Windows alone

**Scope.**

- The entry script gets the engine targets: `toolchain-check`, `editor-build`, `editor-test`, `package-build`, and `package-run`. Each target runs the Windows command of D-72 (D-99).
- `toolchain-check` reads the pins of the Windows PC: the engine of D-28, the toolchain of D-74, and Git LFS (T-2).
- The editor test command and the start command use the program paths of Windows.
- A session runs each engine command on the Windows PC and attaches the log (D-31, D-92). It asks the owner before each command that opens a game window (D-96).
- The engine cache stays in the default place of the engine (D-100). The runbook says so.
- A first value of M-1 on the Windows PC, with the test map open (D-98).
- The removal of each Mac part (D-97). The inventory below lists the files.

**The inventory of Mac parts.** PR-12 read each tracked file on 2026-09-28. These files hold a Mac part:

- `Makefile`: the Mac engine targets, then the whole file.
- `IronAbsolution.Tools/ToolchainCheck/`: the Xcode pin, the Metal Toolchain pin, and the Mac facts. `IronAbsolution.Tests/ToolchainRulesTests.cs` and `IronAbsolution.Tests/ToolchainCheckCommandTests.cs` test them.
- `IronAbsolution.Tools/EditorTest/EditorTestCommand.cs` and `IronAbsolution.Tools/PackageRun/PackageRunCommand.cs`: the Mac program paths. Their tests read the paths.
- `IronAbsolution.Tools/CodexReview/CodexReviewCommand.cs` and `IronAbsolution.Tests/CodexLauncherTests.cs`: comments on the folders of macOS.
- `Game/Config/DefaultEngine.ini`: the Metal shader formats, the Xcode project settings, and the bundle id of D-90, superseded by D-97. `IronAbsolution.Tests/GameFilesTests.cs` reads them.
- `.gitignore`: the rule for Xcode project files.
- `scripts/`: the five scripts become targets of the entry (D-99).
- `docs/runbooks/engine-setup.md`: the volume, Xcode, the Metal Toolchain, the cache path, and the start of the Mac editor. `docs/runbooks/session-context.md`: the volume.
- `CLAUDE.md`, `AGENTS.md`, and `README.md`: the text of `toolchain-check` and the platform.
- `.claude/skills/pr-review/references/review-standard.md`: the platform row.

**Out of scope.**

- The gate record. PR-11 holds it.
- The Shipping configuration, code signing, installers, and store builds. Phase 8 holds them.

**Exit tests.**

1. A clean clone builds the editor target on the Windows PC from the entry, with the log.
2. The automation tests pass headless on the Windows PC from the entry, with the log.
3. A fresh clone on the Windows PC restores the test map from LFS. The map file is not an LFS pointer.
4. A clean clone makes a packaged Development build on the Windows PC. The start command finds the success line. The session asks the owner before the run (D-96).
5. `toolchain-check` passes on the Windows PC. Tests show a failure that names each pin when that pin does not hold (T-2).
6. No tracked file holds a Mac target, a Mac pin, a Mac program path, or a Mac engine setting. A hosted test reads `DefaultEngine.ini` for the Mac sections.
7. M-1 has a value on the Windows PC (D-98).

**Review focus.** The Windows commands against Unreal best practice (D-34). The pass rules of the test command and the start command. The removal against the inventory above.

**Questions.** None open. The session asks each new question at its start.

**State.** 🔧 planned.

> *In plain English:* The engine commands run on the Windows PC alone, and the Mac parts go. After this change, one command builds, tests, and runs the game there.

### 7.8 PR-11: the gate record

**Scope.**

- The value of M-8 at the end of phase 1.
- The values of M-1, M-2, and M-8 in section 4 of `docs/design.md`.
- The evidence of each line of the gate of phase 1 (section 7.9).

Correction of 2026-09-28: the scope held a room of basic shapes, a TSR procedure on the Mac package, and the first value of M-9. They went out of scope with M-9 (D-75, superseded by D-95).

**Out of scope.**

- The frame budget of the game. Phase 3 measures M-3 in the test gym.
- A frame time. M-9 is out of scope (D-95).

**Exit tests.**

1. M-8 has a value.
2. Each line of section 7.9 names its evidence.

**Review focus.** The claim that each value makes, and the evidence of each line of the gate.

**Questions.** None open. D-75 answered OQ-23, and D-95 superseded D-75.

**State.** 🔧 planned.

> *In plain English:* This change collects the proof of each line of the gate. It measures the storage of the large files, and it records the first costs of the engine.

### 7.9 The gate of phase 1

Correction of 2026-09-28: the gate reads the Windows PC alone (D-91). The Mac evidence of PR-9 and PR-10 stays as history.

**The gate.** The gate of phase 1 passes when every line holds:

1. A clean clone builds the editor target on the Windows PC (PR-9, PR-14).
2. One automation test passes headless on the Windows PC, with its log (PR-9, PR-14).
3. A fresh clone restores the LFS content (PR-9, PR-14).
4. The project records its World Partition choice with the reason (PR-9, D-4).
5. The editor shows the state of Enhanced Input (PR-9, F-6).
6. A clean clone makes a packaged Development build on the Windows PC, with its log (PR-10, PR-14).
7. The package starts and stops from the command line (PR-10, PR-14).
8. M-1 and M-2 have Windows values in section 4 of `docs/design.md` (PR-9, PR-10, PR-14, D-98).
9. ⏸ M-9 has a first value. Out of scope since 2026-09-28 (D-95).
10. The five required checks of `main` are green on each PR of the phase (D-61, D-64).
11. No tracked file holds a Mac part (PR-14, D-97).

**What the gate does not ask.** No play, no feel, and no frame budget. Phase 3 holds those (D-32).

> *In plain English:* At this point the game still does nothing that a player can see. But it builds, tests, and runs on the Windows PC, and we know its first costs.

## 8. Sequence

The global order lives in section 8 of `docs/design.md`. Phase 1 holds this order:

1. PR-7: this file.
2. PR-8: the engine toolchain and the evidence form.
3. Owner: the engine install from the runbook of PR-8, during PR-8 (D-69, D-78).
4. PR-9: the project scaffold and the first headless test.
5. PR-10: the packaged build.
6. PR-12: the move to Windows alone, with its decisions and its plan (D-93).
7. PR-13: the development tools on the Windows PC (D-94).
8. PR-14: the engine commands on Windows alone (D-94).
9. PR-11: the gate record (D-95).
10. **← GATE of phase 1.** Section 7.9 holds each line.

Correction of 2026-09-28: PR-11 was step 6, before the move to Windows (D-93, D-94).

Phase 2 runs beside phase 1. Its phase file is a separate documents PR.

## 9. Open questions

The register is `docs/questions.md`. These questions block an item of phase 1. Each PR asks its new questions when it starts.

| Question | Subject | Blocks |
|---|---|---|
| OQ-22 | The Windows toolchain pin | PR-8, resolved by D-74 |
| OQ-23 | The scene of the first M-9 run | PR-11, resolved by D-75, superseded by D-95 |
