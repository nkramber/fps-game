# Technology and art pipeline research

- **Date checked:** 2026-09-26. Official pages were fetched on that date unless a line says otherwise.
- **Purpose:** to supply evidence for the [high-level roadmap](../roadmaps/high-level-roadmap.md), and to bring up the choices the owner must make. It doesn't design subsystems.
- **Labels:**
  - **Evidence**: observed on the machine or quoted from a primary source.
  - **Recommendation**: this document's advice, which is not a decision.
  - **Assumption**: believed but not checked.
  - **Unknown**: could not be determined.

Settled items live in [decisions.md](../decisions.md). Open items live in [questions.md](../questions.md). Nothing here is a decision.

## 0. Development machine

Evidence, measured locally on 2026-09-26:

| Item | Value | Command |
|---|---|---|
| CPU/GPU | Apple M4, 10 cores, Metal 4 | `sysctl`, `system_profiler SPDisplaysDataType` |
| RAM | 16 GB | `sysctl hw.memsize` |
| OS | macOS 26.5.2 (Tahoe), arm64 | `sw_vers`, `uname -m` |
| Xcode | `Xcode.app` 16.2. The active developer dir is Command Line Tools. | app `Info.plist`, `xcodebuild -version` |
| Unreal Engine / Epic Launcher | Not installed | `/Users/Shared/Epic Games` is absent |
| Blender, Houdini | Not installed | `/Applications` |
| Git LFS | Not installed | `git lfs version` |
| Codex CLI | 0.157.1 | `codex --version` |
| Free disk | 923 GB on the project SSD; **45 GB** on the internal disk | `df -h` |

What this means:

- Every engine-side claim below is unvalidated until UE is installed (Phase P1).
- The 16 GB of RAM is Epic's *minimum* for UE 5.8.
- The internal disk is too small for a comfortable engine install plus the derived-data cache. Install on the SSD (Q-4).

## 1. Unreal Engine 5

### Evidence

- **Current line.** UE 5.8 was [released](https://forums.unrealengine.com/t/unreal-engine-5-8-released/2729274) in 2026. The hotfixes are:
  - [5.8.1](https://forums.unrealengine.com/t/5-8-1-hotfix-released/2738864), 2026-07-28;
  - [5.8.2](https://forums.unrealengine.com/t/5-8-2-hotfix-released/2746335), 2026-08-25;
  - [5.8.3](https://forums.unrealengine.com/t/5-8-3-hotfix-released/2833315), about 2026-09-22, the latest one found.

  The release dates come from forum metadata in search results.
- **macOS requirements for 5.8** ([Epic](https://dev.epicgames.com/documentation/unreal-engine/macos-development-requirements-for-unreal-engine)):
  - macOS: minimum Sonoma 14.5; recommended "Latest macOS Sequoia 15". Tahoe isn't named.
  - Xcode: minimum 26.0; recommended 26.1.1. "Xcode 26.4 is not compatible with Unreal Engine".
  - RAM: minimum 16 GB; recommended 32 GB or more. CPU: recommended Apple Silicon M3.
  - Rendering on Apple Silicon:
    - Lumen with software ray tracing: M1+.
    - Hardware ray tracing and MegaLights: M2+, *Experimental*.
    - Nanite and Virtual Shadow Maps: M2+, *Beta*.
- **Xcode on Tahoe** ([Apple](https://developer.apple.com/support/xcode/)):
  - Xcode 26.0–26.3 need macOS 15.6 or later.
  - Apple lists Xcode 16.x only up to Sequoia 15.x.
  - So the installed Xcode 16.2 can't serve UE 5.8, and 26.1.1 is the target.
- **Input.**
  - The [Enhanced Input](https://dev.epicgames.com/documentation/unreal-engine/enhanced-input-in-unreal-engine) page for 5.8 says "Enhanced Input is enabled by default". It supports runtime mapping contexts and Player Mappable Input Configs for remapping.
  - The [Input overview](https://dev.epicgames.com/documentation/unreal-engine/input-overview-in-unreal-engine) page, also labelled 5.8, still calls Enhanced Input "experimental". This is an Epic documentation inconsistency. Follow the dedicated page, and confirm in the editor during P1.
- **Revision control.**
  - Epic's [source control page](https://dev.epicgames.com/documentation/unreal-engine/source-control-in-unreal-engine) says "Perforce and SVN are supported by default". It doesn't cover Git or Git LFS.
  - [One File Per Actor](https://dev.epicgames.com/documentation/unreal-engine/one-file-per-actor-in-unreal-engine) ("Use External Actors") also works on levels without World Partition. It cuts map-file contention, but readable changelists need Perforce.
  - GitHub Free includes [10 GiB of LFS storage and 10 GiB a month of bandwidth](https://docs.github.com/en/billing/concepts/product-billing/git-lfs). Beyond that, and with no payment method on file, pushes of LFS objects are blocked.
- **Testing.** The [Automation Test Framework](https://dev.epicgames.com/documentation/unreal-engine/automation-test-framework-in-unreal-engine) for 5.8 covers:
  - unit, feature, smoke, content-stress and screenshot tests;
  - Automation Spec, Functional Testing (level tests), Automation Driver (input), Gauntlet (session runs), Low-Level Tests and CQTest.

  How to run them headless from the command line must be proven in P1 (**Unknown** from this page).
- **Packaging and profiling.** [Packaging](https://dev.epicgames.com/documentation/unreal-engine/packaging-your-project) uses BuildCookRun. Profiling uses [stat commands](https://dev.epicgames.com/documentation/unreal-engine/stat-commands-in-unreal-engine) and [Unreal Insights](https://dev.epicgames.com/documentation/unreal-engine/unreal-insights-in-unreal-engine). These links resolve (HTTP 200 on 2026-09-27), but their content wasn't re-read in this session. The earlier local research cited them (see § 6).
- **License.** The *Unreal Engine EULA for Creators* prohibits combining the Licensed Technology with GPL code, among other licenses. This repository is GPL-3.0. See **Q-1**, which blocks P1.

### Alternatives considered

- **UE 5.8.x vs UE 5.7.x.**
  - 5.7 gains nothing on this Mac: it would still need a new Xcode.
  - 5.8 is the current line with hotfixes.
  - Meshy's plugin supports only up to 5.7, but Meshy isn't a reason to downgrade (see § 2).
- **C++ with Blueprint/data content vs Blueprint-only.**
  - C++ text is diffable, mergeable, unit-testable and reviewable by agents without the editor.
  - Blueprint graphs are binary assets that can't be merged or reviewed as text.
  - Blueprint-only is faster to start, but it fits badly with PR-based cross-provider review.
- **Git LFS vs Perforce.**
  - Git LFS keeps GitHub PRs, CI and review in one place, but has no Unreal-integrated locking.
  - Perforce is Epic's default and has locking, but it breaks the GitHub PR workflow that D-5 and D-6 rely on.

### Recommendations (owner choices: Q-3 to Q-8)

- **Engine and toolchain.**
  - Pin UE **5.8.x** at the latest hotfix, with **Xcode 26.1.1**, installed on the SSD.
  - Take each hotfix in its own PR, with rebuild evidence.
- **Target.**
  - Develop and accept on **macOS, Apple Silicon**, with keyboard and mouse.
  - Use a starting budget of **60 fps at 1080p on the M4 16 GB**. This is a budget, not a measurement.
  - Decide Windows or gamepad before packaging work (P8).
- **Code split.**
  - C++ holds the core rules.
  - Data Assets and Blueprint subclasses hold tuning, content and one-off level scripting.
  - Blueprints never hold rules that tests must cover.
- **Input.** Use Enhanced Input, with remapping via Player Mappable configs, proven in P3.
- **Source control.** Use Git with LFS for `.uasset`, `.umap` and binary source art.
  - Add a `.gitattributes` in the P1 scaffold PR, not before, because there is nothing to track yet.
  - Follow a one-editor-per-map convention.
  - Evaluate One File Per Actor in P6, when several people or sessions touch one map.
- **Level transitions.** For one level, only menu → level → results → restart matter. Plain map loads are enough. Don't design streaming.
- **Rendering defaults.** Leave the lighting choice (dynamic, Lumen or baked) open until the P5 vertical-slice room measures it. Nanite and VSM are Beta on Mac, and the machine is at minimum RAM.
- **Must prove in P1:**
  - a C++ project compiles and opens;
  - a Development build packages and runs;
  - headless automation tests run;
  - LFS round-trips;
  - editor memory and compile times are recorded.

## 2. Meshy

### Evidence

- **Plugin support.**
  - The [Meshy Unreal integration page](https://www.meshy.ai/integrations/unreal-engine) says: "Windows binary builds are available for UE 5.4, 5.5, 5.6 and 5.7". A UE 5.7 source package exists, and "Other targets require building and verifying the source package."
  - There is **no macOS binary and no 5.8 build**.
  - The [plugin introduction](https://docs.meshy.ai/en/webapp/plugins/unreal/introduction) shows a Mac install path (`/Users/Shared/Epic Games/Unreal_5.5/…`) but lists no plan requirement.
- **Account.** The integration page says: "A Meshy Pro account or above is required to use the DCC Bridge". The Bridge is the path the plugin uses to receive models.
- **Plans** ([pricing docs](https://docs.meshy.ai/en/webapp/pricing)):
  - Free: 100 credits a month, CC BY 4.0, "commercial with attribution".
  - Paid: private, "full commercial".
  - Text-to-3D (Meshy 6): 20 credits. Image-to-3D: 20–25. AI texturing: 10. Remesh and rigging: 0.
  - Paid-tier prices and download limits weren't on the page (**Unknown**).
- **Terms** ([terms of use](https://www.meshy.ai/terms-of-use), last updated 2026-09-19):
  - Free plan: Meshy owns the output and licenses it CC BY 4.0.
  - Paid plans: "Customers on a paid Meshy plan own their Customer Output", and may keep content private.
  - Non-Enterprise inputs and outputs may be used for training.
  - Non-Enterprise API outputs are deleted after 3 days.
  - Web-app outputs of inactive accounts may be deleted.
- **Formats** ([export formats](https://docs.meshy.ai/en/webapp/guides/platform/export-formats)): GLB, FBX, OBJ, STL, USDZ, 3MF. FBX is recommended for Unreal. GLB embeds textures; FBX and OBJ need their texture files kept alongside.
- **Not documented** on those pages: units, scale, axis, pivots, LODs and collision. All of these must be fixed in a cleanup or import step.

### Plugin vs manual path

| | Plugin + DCC Bridge | Manual export → Unreal import |
|---|---|---|
| Works on this Mac with UE 5.8 | No verified build. It would mean compiling a 5.7 source package. | Yes. This uses the standard FBX/glTF import. |
| Plan needed | Pro or above | Any, but the licence differs by plan |
| Control of scale, pivot, collision, LOD, materials | Less visible | Full, through fixed import settings |
| Repeatability and review | Tied to the editor session | The exported source file is committed (LFS) with a provenance note |

### Recommendations (owner choice: Q-12)

- Use the **manual export → FBX → import** path, if Meshy is used at all.
- Use it only for **focal or set-dressing props**.
- Don't use it for walkable geometry, buildings, terrain, grid-snapped kit pieces, or first-person weapons without a DCC cleanup pass.
- Record provenance for every asset: prompt or input image, model version, plan and licence, date, remesh settings and cleanup steps.
- Spend nothing until the owner approves a plan and a trial budget (D-8).
- Know that the licence and privacy terms change with the plan.

## 3. World and building art pipeline

| Approach | Strengths | Costs and risks | Recommendation |
|---|---|---|---|
| **Modular architectural kit + trim sheets and tiling materials** | Fast layout revision, consistent texel density, reusable collision, cheap to render, good fit for arenas and corridors | Needs a grid, pivot and naming standard up front. Repetition needs dressing. | **Primary**, for all walkable structure |
| **Hand-authored meshes in a DCC (Blender)** | Highest quality control for hero props and bespoke pieces; standard FBX/glTF export | Blender isn't installed. Skill and time cost per asset. | **Primary for bespoke art**, once proven in P5 |
| **Unreal Modeling Mode** | In-editor blockout, quick fixes and collision tweaks | Less suitable for final hero art; changes live in binary assets | **Graybox and fixes** |
| **Graybox with editor primitives or a simple kit** | Layout and pacing tested before art spend | Throwaway by design | **Required** in P3, P4 and P6 |
| **Procedural tools (Houdini, PCG)** | They pay off with many variants or large terrain | Tool and licence cost; overkill for one compact level | **Decline** for level 1 unless P5 shows a specific need |
| **Code-generated placeholder art** | Deterministic and diffable | Low fidelity; a tool to maintain | **Optional** for placeholders only (see Q-2) |
| **AI-generated props (Meshy)** | Quick concept-to-prop | Licensing, topology, scale, consistency | **Selective**, after Q-12 |

### Topics the pipeline must standardize before production (P5)

- **Terrain.**
  - **Assumption:** a compact FPS level is mostly interior or built space, with small exterior areas.
  - Choose kit floors and sculpted meshes over the Landscape tool unless the level brief (Q-11) needs open ground.
- **Scale and grid.**
  - 1 Unreal unit = 1 cm.
  - Pick one kit grid (commonly 100, 200 or 400 cm) and player metrics (jump height, step height, corridor width) *after* the P3 feel gate. Movement numbers decide layout metrics.
- **Pivots, collision, UVs and LODs.**
  - Pivots sit at grid corners.
  - Use simple collision for walkable surfaces and custom collision for stairs.
  - Keep one texel-density target.
  - Add lightmap UVs only if baked lighting is chosen.
  - Use LODs or HLOD only where profiling shows a need.
- **Materials.** Use a small set of master materials with instances. Trim sheets carry detail.
- **Lighting.** Choose among dynamic, Lumen (software RT) and baked by measuring the vertical-slice room against Q-5. This is **Unknown** until measured.
- **Performance.** Set per-space budgets (draw calls, triangles, lights, texture memory) from P5 measurements, not guesses.
- **Provenance.** Record every asset's source, licence, author or tool, and date in a manifest that is checked in review (D-2).
- **Source control.**
  - DCC source files (`.blend`) and exports (`.fbx`) go in LFS.
  - Unreal assets are named with prefixes such as `SM_` and `M_`.
  - Folders are organized by kit, not by level.
- **Revision cost.** Measure how long it takes to change one kit piece and see it everywhere. This is the key reason to choose kits.

### Likely first-level art needs

These are **assumptions** to be confirmed by the P2 level brief:

- one architectural kit and its trims;
- 10–30 props;
- a first-person weapon set;
- a small enemy roster with animation;
- pickups;
- VFX for firing, impacts and deaths;
- a sky or backdrop;
- UI;
- sound for every combat event.

**Animation sourcing** for enemies and first-person arms is an unresearched risk. Address it in the P5 focused roadmap.

## 4. Level and gameplay architecture

- **World Partition.**
  - Epic's [World Partition](https://dev.epicgames.com/documentation/unreal-engine/world-partition-in-unreal-engine) page (5.8) describes "a complete solution for large world management". It targets "large worlds where it is impossible to load the entire map in the Editor".
  - World Partition itself is on in the Blank and First Person templates, with streaming disabled by default.
  - **Recommendation:** a compact, authored level loads whole. Don't adopt World Partition streaming (D-4).
  - Decide in P1 whether to create the level from a non-World-Partition template, or keep World Partition with streaming off. Record the choice with its evidence.
- **Collaboration.** One map per level, and one editor of a map at a time. Sublevels or [Level Instances](https://dev.epicgames.com/documentation/unreal-engine/level-instancing-in-unreal-engine) split the work only if contention shows up.
- **Likely reusable boundaries** (a recommendation; detailed design belongs to focused roadmaps):
  - **Global game rules** (C++, tested): player movement and camera, weapons and damage, the resource economy, enemy behaviours, encounter and spawn framework, checkpoint and restart, settings and save, HUD shell, and scoring.
  - **Level content** (data and placement): layout, encounter definitions and triggers, pickup placement, scripted beats, art, lighting, audio ambience, and par times or scoring thresholds.
  - The test of the boundary: **a second level should need new content and data, not new rule code.**
- **Leave to later:** AI framework (StateTree, Behavior Tree or custom), save format, UI framework, and animation system. Each belongs to its phase's focused roadmap.

## 5. Risks

| Risk | Impact | Mitigation |
|---|---|---|
| GPL-3.0 repository vs Unreal EULA (Q-1) | Can't legally combine and distribute | Owner resolves before any Unreal code merges |
| 16 GB RAM, Epic's minimum | Slow builds, editor pressure | Modest content, measure in P1, avoid Beta renderers by default |
| Tahoe not named by Epic; Xcode 16.2 unusable | Toolchain faults | Pin Xcode 26.1.1. Record faults in the P1 evidence. |
| No hosted CI with the engine (Q-8) | Engine regressions caught late | Local build and test evidence in every engine PR; engine-free tests in CI |
| Binary assets can't be merged | Lost work on conflicts | LFS, one-editor-per-map, small PRs |
| Feel or scope found late | Expensive rework of content | P3 and P4 gates before P6 and P7 |
| AI or third-party asset licensing | Legal or ownership problems | Provenance manifest; no spend without approval |
| Process overhead (see [role-model-patterns.md](role-model-patterns.md)) | Slow delivery | Keep gates proportional. Add a check only when a real failure justifies it. |

## 6. Relation to the earlier local research

An earlier, unpushed local commit (`91246cc`, Q-2) holds a file at this same path. Its engine, Xcode, Meshy and licence-term facts agree with this check. However:

- it recorded owner choices (engine pin, the Meshy decision, the code split, lighting) as *decisions*;
- it missed the GPL/EULA conflict and the internal-disk limit;
- it described a C++ implementation that no review has seen.

This document replaces it for the purposes of this branch. Its choices are carried as recommendations and open questions instead.
