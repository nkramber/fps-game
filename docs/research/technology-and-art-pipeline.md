# Technology and art pipeline research

Status: research, checked 2026-09-26. The link check ran on 2026-09-27. Written in ASD-STE100 (D-17).

- Purpose: give evidence for the roadmap in `docs/design.md`, and find the choices that the owner must make. This file does not design systems.
- Labels: **evidence** comes from the machine or from a primary source. A **recommendation** is advice, not a decision. An **assumption** has no check yet. An **unknown** had no answer.
- The registers hold the settled items (`docs/decisions.md`) and the open items (`docs/questions.md`). Nothing here is a decision.
- On 2026-09-27 the owner decided most choices of this file: D-24 and D-26 to D-38. Where a recommendation below differs from a decision, the decision wins.

## 0. Development machine

Evidence, measured on 2026-09-26:

| Item | Value | Command |
|---|---|---|
| CPU and GPU | Apple M4, 10 cores, Metal 4 | `sysctl`, `system_profiler SPDisplaysDataType` |
| Memory | 16 GB | `sysctl hw.memsize` |
| OS | macOS 26.5.2 (Tahoe), arm64 | `sw_vers`, `uname -m` |
| Xcode | Xcode 16.2. The active developer folder is the Command Line Tools. | app Info.plist, `xcodebuild -version` |
| Unreal Engine, Epic Games Launcher | absent | no Epic Games folder in the shared user folder |
| Blender, Houdini | absent | `/Applications` |
| Git LFS | absent | `git lfs version` |
| .NET SDK | 10.0.400 | `dotnet --version` |
| Codex CLI | 0.157.1, with `codex exec` and `codex review` | `codex --version` |
| Free disk | 923 GB on the project SSD, 45 GB on the internal disk | `df -h` |

Result: each engine claim below stays unproven until phase 1 installs the engine. The Mac has the minimum memory of Epic for Unreal Engine 5.8 (F-5). The internal disk is too small for the engine and its cache (F-4).

## 1. Unreal Engine 5

### Evidence

- **Current line.** Epic released [Unreal Engine 5.8](https://forums.unrealengine.com/t/unreal-engine-5-8-released/2729274) in 2026. The hotfixes are [5.8.1](https://forums.unrealengine.com/t/5-8-1-hotfix-released/2738864) (2026-07-28), [5.8.2](https://forums.unrealengine.com/t/5-8-2-hotfix-released/2746335) (2026-08-25), and [5.8.3](https://forums.unrealengine.com/t/5-8-3-hotfix-released/2833315) (about 2026-09-22). The dates come from forum data in search results.
- **macOS requirements of 5.8** ([Epic](https://dev.epicgames.com/documentation/unreal-engine/macos-development-requirements-for-unreal-engine)):
  - macOS: the minimum is Sonoma 14.5, and the recommendation is "Latest macOS Sequoia 15". The page does not name Tahoe.
  - Xcode: the minimum is 26.0, and the recommendation is 26.1.1. The page says "Xcode 26.4 is not compatible with Unreal Engine".
  - Memory: the minimum is 16 GB, and the recommendation is 32 GB or more. The recommended CPU is Apple Silicon M3.
  - Apple Silicon: Lumen with software ray tracing needs M1 or later. Hardware ray tracing and MegaLights are experimental on M2 or later. Nanite and Virtual Shadow Maps are beta on M2 or later.
- **Xcode on Tahoe** ([Apple](https://developer.apple.com/support/xcode/)): Xcode 26.0 to 26.3 need macOS 15.6 or later. Apple lists Xcode 16 only up to Sequoia 15. So Xcode 16.2 cannot serve Unreal Engine 5.8, and 26.1.1 is the target (F-3).
- **Input.** The [Enhanced Input](https://dev.epicgames.com/documentation/unreal-engine/enhanced-input-in-unreal-engine) page of 5.8 says "Enhanced Input is enabled by default". It supports mapping contexts at runtime and player mappable configs. The [input overview](https://dev.epicgames.com/documentation/unreal-engine/input-overview-in-unreal-engine) page of 5.8 still calls it experimental. Phase 1 checks it in the editor (F-6).
- **Version control.** The [source control](https://dev.epicgames.com/documentation/unreal-engine/source-control-in-unreal-engine) page says "Perforce and SVN are supported by default". It does not cover Git or Git LFS. [One File Per Actor](https://dev.epicgames.com/documentation/unreal-engine/one-file-per-actor-in-unreal-engine) also works on a level without World Partition. It lowers map conflicts, but readable change lists need Perforce.
- **LFS quota.** GitHub Free includes [10 GiB of LFS storage and 10 GiB of bandwidth each month](https://docs.github.com/en/billing/concepts/product-billing/git-lfs). Past the quota, with no payment method, GitHub blocks new LFS pushes.
- **Tests.** The [Automation Test Framework](https://dev.epicgames.com/documentation/unreal-engine/automation-test-framework-in-unreal-engine) of 5.8 has unit, feature, smoke, content stress, and screenshot tests. It also names Automation Spec, Functional Testing, Automation Driver, Gauntlet, Low-Level Tests, and CQTest. That page does not show a headless command line, so phase 1 must prove one (unknown).
- **Packages and profiles.** A [package build](https://dev.epicgames.com/documentation/unreal-engine/packaging-your-project) uses BuildCookRun. Profiles use [stat commands](https://dev.epicgames.com/documentation/unreal-engine/stat-commands-in-unreal-engine) and [Unreal Insights](https://dev.epicgames.com/documentation/unreal-engine/unreal-insights-in-unreal-engine). These links gave HTTP 200 on 2026-09-27, but this session did not read their content again.
- **Tool languages.** Unreal build rules are C# classes in Target.cs and Build.cs files ([modules](https://dev.epicgames.com/documentation/en-us/unreal-engine/unreal-engine-modules)). [Python in the editor](https://dev.epicgames.com/documentation/unreal-engine/scripting-the-unreal-editor-using-python) runs only in the Unreal Editor, also headless through a commandlet. It never runs in a packaged game. These facts support D-15.
- **License.** The Unreal Engine EULA for Creators prohibits a combination of Unreal technology with GPL code. This repository is GPL-3.0 (F-1, OQ-1).

### Alternatives

- **Unreal Engine 5.8 or 5.7.** Version 5.7 gives no gain on this Mac, because it also needs a new Xcode. Version 5.8 is the current line with hotfixes. The Meshy plugin stops at 5.7, but Meshy is not a reason to go back (section 2).
- **C++ with Blueprint content, or Blueprint only.** C++ text can merge, gets unit tests, and a reviewer can read it without the editor. Blueprint graphs are binary assets. They cannot merge, and a text review cannot read them.
- **Git LFS or Perforce.** Git LFS keeps the PR, CI, and review flow of the role models on GitHub (D-12). It has no file lock inside Unreal. Perforce is the default of Epic and has locks, but it breaks the GitHub flow.

### Recommendations

The owner decided these on 2026-09-27 (D-28 to D-34). The owner chose macOS and Windows with higher budgets than this list (D-32):

- Pin Unreal Engine 5.8 at the latest hotfix, with Xcode 26.1.1, on the SSD. Each hotfix gets its own PR with build evidence.
- Develop and accept on macOS with Apple Silicon, keyboard, and mouse. Start with a budget of 60 fps at 1080p on the M4 with 16 GB. This budget is not a measurement.
- Put the rules in C++. Put tuning, content, and one-off level scripts in data assets and Blueprint subclasses.
- Use Enhanced Input, with remap through player mappable configs. Phase 3 proves it.
- Use Git LFS for `.uasset`, `.umap`, and binary source art. Add the LFS attributes in the scaffold PR of phase 1, when content exists. One person edits one map at a time.
- For one level, only the flow from menu to level to results to restart matters. Plain map loads are enough.
- Leave the light method open until the room of phase 5 measures it. Nanite and Virtual Shadow Maps are beta on the Mac, and the memory is at the minimum.
- Phase 1 must prove five things. A C++ project builds and opens. A package runs. A headless test runs. LFS works both ways. M-1 and M-2 have values.

## 2. Meshy

### Evidence

- **Plugin.** The [Meshy Unreal page](https://www.meshy.ai/integrations/unreal-engine) says: "Windows binary builds are available for UE 5.4, 5.5, 5.6 and 5.7". It also says that "Other targets require building and verifying the source package". No macOS build and no 5.8 build exist (F-7).
- **Account.** The same page says "A Meshy Pro account or above is required to use the DCC Bridge". The plugin gets models through that bridge. The [plugin introduction](https://docs.meshy.ai/en/webapp/plugins/unreal/introduction) shows a Mac install path, but it names no plan.
- **Plans** ([pricing docs](https://docs.meshy.ai/en/webapp/pricing)): the free plan gives 100 credits each month under CC BY 4.0, with commercial use and attribution. Paid plans are private, with full commercial use. Text to 3D costs 20 credits, image to 3D 20 to 25, texture 10, and remesh and rig 0. The page gives no paid prices and no download limits (unknown).
- **Terms** ([terms of use](https://www.meshy.ai/terms-of-use), last updated 2026-09-19):
  - On the free plan, Meshy owns the output and licenses it under CC BY 4.0.
  - On a paid plan, the customer owns the output and can keep it private.
  - Meshy can use inputs and outputs of non-Enterprise customers to train its models.
  - Meshy deletes API outputs of non-Enterprise customers after 3 days. It can delete web outputs of accounts that stay inactive.
- **Formats** ([export formats](https://docs.meshy.ai/en/webapp/guides/platform/export-formats)): GLB, FBX, OBJ, STL, USDZ, and 3MF. FBX is the recommendation for Unreal. GLB holds its textures, and FBX and OBJ need their texture files beside them.
- **Gaps.** Those pages give no units, scale, axis, pivots, LODs, or collision. A cleanup or import step must set each of them.

### Plugin or manual path

| | Plugin and bridge | Manual export and Unreal import |
|---|---|---|
| Works on this Mac with 5.8 | No proven build. It needs a compile of a 5.7 source package. | Yes, through the normal FBX or glTF import |
| Plan | Pro or above | Any, but the license changes with the plan |
| Control of scale, pivot, collision, LOD, and materials | Less | Full, through fixed import settings |
| Repeat and review | Tied to one editor session | The export goes into LFS with a provenance note |

### Recommendations (OQ-12)

- If the project uses Meshy, use the manual path: export FBX, then import.
- Use it for focal props and dressing only. Never use it for floors, walls, buildings, terrain, grid kit pieces, or first-person weapons without a DCC cleanup.
- Keep a provenance record for each asset. It holds the prompt or image, the model version, the plan, the license, the date, and the cleanup.
- Spend nothing before the owner approves a plan and a trial budget (D-8).

## 3. World and building art pipeline

| Method | Strength | Cost and risk | Recommendation |
|---|---|---|---|
| Modular kit with trim sheets and tile materials | Fast layout change, one texel density, collision that repeats, low render cost | Needs grid, pivot, and name rules first. Repeats need dressing. | Main method for all walkable structure |
| Meshes made in a DCC (Blender) | Best control for hero props and special pieces | Not installed. Time and skill for each asset. | Main method for special art, after phase 5 |
| Unreal Modeling Mode | Blockout and quick fixes in the editor | Weak for final art. The result lives in binary assets. | Graybox and fixes |
| Graybox from editor shapes or a simple kit | Tests layout and pacing before art spend | Thrown away by design | Required in phases 3, 4, and 6 |
| Houdini or PCG | Pays off with many variants or large terrain | Tool cost. Too much for one compact level. | Not for level 1, unless phase 5 finds a need |
| Generated placeholder art from code | Repeatable and readable in a diff | Low detail, and a tool to keep | Placeholders only (see OQ-2) |
| Generated props (Meshy) | Fast from idea to prop | License, topology, scale, and style | Selective, after OQ-12 |

### What phase 5 must set before production

- **Terrain.** Assumption: a compact FPS level is mostly interior or built space, with small outdoor parts. Use kit floors and sculpted meshes, not the Landscape tool, unless the brief needs open ground (OQ-11).
- **Scale and grid.** One Unreal unit is one centimeter. Choose one kit grid, for example 100, 200, or 400 cm. Choose it after the gate of phase 3, because the movement metrics set the layout metrics.
- **Pivots, collision, UVs, and LODs.** Pivots sit on grid corners. Walkable surfaces get simple collision, and stairs get custom collision. Keep one texel density. Add lightmap UVs only for baked light. Add LODs only where a profile shows a need.
- **Materials.** A few master materials with instances. Trim sheets carry the detail.
- **Light.** Choose dynamic light, Lumen with software ray tracing, or baked light by a measurement in the room of phase 5 (M-5). This choice is unknown until then.
- **Performance.** Set budgets for each space from the measurements of phase 5, not from guesses.
- **Provenance.** Each asset has its source, license, author or tool, and date in a manifest that the review checks (D-2).
- **Storage.** DCC files and exports go into LFS. Unreal asset names use a prefix, for example `SM_` for a static mesh. Folders follow the kit, not the level.
- **Revision cost.** Measure the time to change one kit piece and see it in each space (M-6). This cost is the main reason for kits.

### Likely art needs of the first level

These are assumptions for the brief of phase 2 to confirm:

- one architecture kit with its trims, and 10 to 30 props.
- a set of first-person weapons, a small enemy roster with animation, and pickups.
- effects for fire, impacts, and deaths, a sky or backdrop, a UI, and a sound for each combat event.

The sources of animation for enemies and first-person arms are a risk with no research yet. The focused roadmap of phase 5 holds it.

## 4. Level and gameplay architecture

- **World Partition.** The [World Partition](https://dev.epicgames.com/documentation/unreal-engine/world-partition-in-unreal-engine) page of 5.8 calls it "a complete solution for large world management". It aims at worlds "where it is impossible to load the entire map in the Editor". The Blank and First Person templates keep World Partition on, with streaming off by default. Recommendation: a compact level loads whole, so do not use its streaming (D-4). Phase 1 records whether the level uses World Partition at all, with the reason.
- **Collaboration.** One map for the level, and one editor of a map at a time. Sublevels or [level instances](https://dev.epicgames.com/documentation/unreal-engine/level-instancing-in-unreal-engine) split the work only when conflicts start.
- **Boundaries that the next levels can use again.** This is a recommendation, and the focused roadmaps hold the design:
  - Global rules, in C++ with tests: movement and camera, weapons and damage, resources, and enemy behavior. Also encounters and spawns, checkpoints and restart, settings and save, the HUD frame, and the score.
  - Level content, as data and placement: layout, encounter data and triggers, pickups, scripted beats, art, light, ambient sound, and par times.
  - The test of the boundary: a second level needs new content and data, but no new rule code.
- **Later decisions.** The AI framework, the save format, the UI framework, and the animation system each belong to the focused roadmap of their phase.

## 5. Risks

| Risk | Result | Mitigation |
|---|---|---|
| GPL-3.0 and the Unreal EULA | No legal combination and distribution | Closed. The license is MIT (D-24). |
| 60 fps at 4K output on the base M4 (F-14) | The budget fails, or the art gets thin | TSR from a measured internal resolution (M-9), from phase 1 |
| 16 GB memory, the minimum of Epic | Slow builds, editor pressure | Small content, M-1 in phase 1, no beta renderer by default |
| Tahoe not named by Epic, Xcode 16.2 not usable | Toolchain faults | Pin Xcode 26.1.1. Record each fault in phase 1. |
| No engine on hosted CI (OQ-8) | Late discovery of engine faults | Local evidence in each engine PR, hosted checks for the rest |
| Binary assets cannot merge | Lost work after a conflict | LFS, one editor for each map, small PRs |
| Late discovery of bad feel or scope | Costly rework of content | Gates of phases 3 and 4 before phases 6 and 7 |
| Licenses of generated or bought assets | Legal or ownership problems | Provenance manifest, no spend without approval |
| Process overhead | Slow delivery | Phase 0 ports only the infrastructure of the role models (D-12) |

## 6. The earlier local research

An earlier local commit (`91246cc`) held a file at this path. The owner discarded it on 2026-09-27 (D-25).

Its engine, Xcode, Meshy, and license-term facts agree with this check. But it wrote owner choices as decisions: the engine pin, the Meshy choice, the code split, and the light method. It missed the GPL conflict and the small internal disk. It also described C++ code that no review saw. This file replaced it, and it kept those choices as recommendations and questions for the owner.
