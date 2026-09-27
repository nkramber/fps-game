# Open questions

This file lists the material choices that belong to the owner. Settled answers move to [decisions.md](decisions.md).

## Conventions

- **Every question here belongs to the owner.** An agent gives options, evidence and a recommendation. An agent never chooses silently.
  - If a question blocks the current PR, ask the owner and stop that part of the work.
  - Keep going on anything the question doesn't block.
- **Append-only.** Use the next `Q-N` number. Never renumber, delete or reuse an ID. Keep answered questions, with their status changed.
- **Status values:**
  - `Open`
  - `Answered → D-N`: set in the same PR that adds the decision.
  - `Withdrawn (reason)`
- **Lookup:** `grep -n '^### Q-7:' docs/questions.md`, or list open ones with `grep -n -B1 'Status: Open' docs/questions.md`.

Entry format:

```
### Q-N: Short title
- Status: Open
- Raised: YYYY-MM-DD, session N
- Blocks: what cannot proceed without an answer
- Needed by: roadmap phase or gate
- Question: one sentence.
- Options: A / B / C with trade-offs.
- Evidence: links.
- Recommendation: which option and why (a recommendation, not a decision).
```

---

### Q-1: Repository license vs the Unreal Engine EULA
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: committing any Unreal project files, C++ modules or engine-derived content to `main`, which is Phase P1 of the roadmap.
- Needed by: before the first Unreal project PR merges.
- Question: The repository is licensed GPL-3.0 (`LICENSE`). How should the game's code and content be licensed, given that the Unreal Engine EULA prohibits combining Unreal "Licensed Technology" with GPL-covered code?
- Options:
  - **A.** Relicense the game code under a permissive license such as MIT or Apache-2.0. The EULA names both as allowed.
  - **B.** Make the project proprietary ("all rights reserved") and keep the repository public or private.
  - **C.** Keep GPL-3.0 only for engine-independent tools, and license the Unreal project separately.
  - **D.** Get legal advice before choosing.
- Evidence:
  - The *Unreal Engine EULA for Creators*, section "Non-Compatible Licenses", says: "Code or content under the following licenses, for example, are prohibited: GNU General Public License (GPL), Lesser GPL (LGPL) (unless you are merely dynamically linking a shared library)…". The allowed examples are BSD, MIT, MS-PL and Apache.
  - This was read from Epic's published EULA PDF on 2026-09-26. The web version at `unrealengine.com/eula/unreal` returned HTTP 403 to automated fetches, so the owner should confirm against the current text.
  - This is not legal advice.
- Recommendation:
  - Resolve this before any Unreal code lands.
  - For the content license, consider a separate license for art and audio.
  - Agents won't edit `LICENSE` (D-10).

### Q-2: What to do with the unreviewed local prototype
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: a clean local `main`. It doesn't block this PR.
- Needed by: before the owner next pushes from their local checkout.
- Question: An earlier local session produced work that was never pushed or reviewed. What should happen to it? The work is:
  - two commits on the owner's local `main`, ahead of `origin/main`:
    - `91246cc`, research that recorded engine, platform and Meshy "decisions";
    - `c101caf`, procedural art, texture and SFX generators;
  - untracked files: `Emberline.uproject`, `Source/` (about 3,700 lines of gameplay C++), `Tests/`;
  - ignored generated output under `SourceArt/Generated/` and `.venv-tools/`.
- Options:
  - **A.** Move the two commits to a local reference branch, for example `local/prototype-2026-09-26`, and reset local `main` to `origin/main`. Then reuse parts only through the roadmap's own PRs, after Q-1.
  - **B.** Discard the work.
  - **C.** Submit it as-is.
- Evidence:
  - `git status` shows `main` "ahead of 'origin/main' by 2 commits".
  - Its commit `91246cc` adds `docs/research/technology-and-art-pipeline.md`, which will conflict with the same path in this PR.
  - It settles owner questions (Q-3, Q-4, Q-6, Q-12) without owner input.
  - It implements gameplay ahead of the feel and design gates.
- Recommendation:
  - **A.** It keeps the work for reference without bypassing review, licensing (Q-1) or phase gates.
  - **C** would conflict with this PR, and would carry GPL-licensed Unreal C++ (see Q-1).

### Q-3: Working title and Unreal project name
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: creating the Unreal project in P1. Project and module names are costly to rename later.
- Needed by: P1 start.
- Question: What is the working title, and what name should the `.uproject` and the primary C++ module use?
- Options:
  - **A.** A neutral code name that is independent of the final title, for example `FpsGame`.
  - **B.** A chosen working title. The local prototype used "Emberline", which the owner never approved.
- Recommendation: **A.** A neutral module name stays correct if the title changes. The display title can change freely later.

### Q-4: Unreal Engine version, upgrade policy and install location
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: P1.
- Needed by: P1 start.
- Question: Which UE version should be pinned, how are hotfix and minor upgrades taken, and where should the engine be installed?
- Options:
  - **A.** UE **5.8.x** (5.8.3 is the latest hotfix as of 2026-09-26), with Xcode **26.1.1**.
  - **B.** UE 5.7.x. It still needs a new Xcode on this Mac and offers no benefit.
  - Install location:
    - on the external project SSD, which has 923 GB free;
    - on the internal disk, which has 45 GB free.
- Evidence: [technology-and-art-pipeline.md § 1](research/technology-and-art-pipeline.md#1-unreal-engine-5).
- Recommendation:
  - **A**, pinned to the minor version (`5.8`).
  - Each hotfix upgrade gets its own PR with a rebuild and smoke evidence.
  - Install on the SSD.
  - Avoid Xcode 26.4 or later, which Epic says is incompatible.

### Q-5: Target platforms, input devices and performance budget
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: the P3 feel gate criteria and the P5 art budgets.
- Needed by: P3 start.
- Question: Which platforms and input devices must the first level ship on, and what frame-rate and resolution budget must it meet?
- Options:
  - **A.** macOS on Apple Silicon only. Keyboard and mouse first; gamepad optional later. 60 fps at 1080p on the development Mac (M4, 16 GB).
  - **B.** A plus Windows, which needs access to Windows hardware for validation.
  - **C.** A higher frame-rate target, such as 120 fps, which limits lighting and art density.
- Evidence: [technology-and-art-pipeline.md § 1](research/technology-and-art-pipeline.md#1-unreal-engine-5).
- Recommendation:
  - **A** for development and first acceptance.
  - Decide Windows before P8, when packaging and QA cost become real.

### Q-6: C++ and Blueprint responsibilities
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: P1 project template choice and P3 code structure.
- Needed by: P1 exit.
- Question: Should the project be C++ with Blueprint and data-authored content, or Blueprint-only?
- Options:
  - **A.** C++ holds the core rules: movement, weapons, damage, resources, AI decisions and save. Blueprints and Data Assets hold content, tuning and one-off level scripting.
  - **B.** Blueprint-only.
- Evidence: [technology-and-art-pipeline.md § 1](research/technology-and-art-pipeline.md#1-unreal-engine-5).
- Recommendation:
  - **A.** Text source can be diffed, merged, unit tested and reviewed by agents.
  - Binary Blueprint graphs can't be merged, and agents can't review them without the editor.

### Q-7: Binary asset storage
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: the first `.uasset` or `.umap` commit in P1.
- Needed by: P1.
- Question: How are binary assets stored?
- Options:
  - **A.** Git LFS on GitHub. It is free up to 10 GiB of storage and 10 GiB of bandwidth a month.
  - **B.** An Unreal-native VCS with file locking, such as Perforce.
  - **C.** A paid GitHub LFS data plan once the quota is near.
- Evidence: [technology-and-art-pipeline.md § 1](research/technology-and-art-pipeline.md#1-unreal-engine-5).
- Recommendation:
  - **A**, with a size report in CI. Revisit when usage passes about 5 GiB.
  - Use a one-editor-per-map convention in place of locking.

### Q-8: CI for engine builds
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: the P1 exit, which records the CI strategy.
- Needed by: P1 exit.
- Question: How are Unreal compiles, cooks and automation tests validated for PRs?
- Options:
  - **A.** Hosted GitHub runners do docs checks and engine-independent tests only. Engine build and test evidence is produced locally and recorded in the PR and review record.
  - **B.** A self-hosted runner on the owner's Mac. This is a security risk on a public repository with fork PRs, and it ties up the machine.
  - **C.** Paid or private build infrastructure.
- Evidence: [workflow.md § CI/CD](workflow.md#cicd-strategy); [role-model-patterns.md](research/role-model-patterns.md), where one role model retired its self-hosted Mac runner.
- Recommendation:
  - **A** for now. Revisit at the P3 exit, once engine-test volume is known.

### Q-9: Setting, tone and art direction
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: the P2 exit, and every art-pipeline choice in P5.
- Needed by: P2.
- Question: What is the game's original setting, tone and visual style?
- Options: the owner chooses. The session will prepare two or three short, original direction proposals in P2 if asked.
- Recommendation:
  - Choose a style that a small team can produce with modular kits and trim sheets, such as stylized or readable hard-surface.
  - Photoreal organic terrain would multiply content cost.

### Q-10: The resource-limited combat loop
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: P2 exit and the P4 combat foundation.
- Needed by: P2.
- Question: Which resources are limited (ammo, health, armor, cooldowns or others), and by what **original** mechanic does the player get them back mid-fight?
- Options: the owner chooses. P2 may prototype two or three original candidates on paper first.
- Recommendation:
  - Pick one primary recovery mechanic that rewards aggressive play.
  - Test it in the P4 combat sandbox before level production.
  - Don't reuse the reference game's signature mechanics (D-2).

### Q-11: Level scope and the meaning of "replayable"
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: P2 exit, P6 layout, and P8 acceptance criteria.
- Needed by: P2.
- Question: How long is the level, how many combat arenas and enemy or weapon types does it have, and what makes it replayable?
- Options: replayability could come from any of these:
  - difficulty levels;
  - a score, rank or timer;
  - secrets or collectibles;
  - alternate routes;
  - encounter variation.
- Recommendation:
  - Target 15–25 minutes for a first clear.
  - Use four to six combat spaces, a small weapon and enemy roster, and replay value from difficulty plus score or time ranking.
  - These are assumptions to test in P2, not facts.

### Q-12: Meshy use and plan tier
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: any Meshy generation (D-8).
- Needed by: P5, only if generated props are wanted.
- Question: Should Meshy be used at all? If so, on which plan, and for which asset classes?
- Options:
  - **A.** Don't use it.
  - **B.** Free plan. Outputs are licensed CC BY 4.0, Meshy owns them, they are public, and attribution is needed.
  - **C.** Paid plan. The customer owns the output and can keep it private. Non-Enterprise inputs and outputs may still be used for training.
  - Workflow, if used: manual export (FBX) and import, or the plugin. The plugin has Windows binaries only up to UE 5.7, and its Bridge needs Pro.
- Evidence: [technology-and-art-pipeline.md § 2](research/technology-and-art-pipeline.md#2-meshy).
- Recommendation:
  - Decide after the P5 art direction.
  - If used, choose **C** with manual export and import, for focal props only.
  - Run a one- to three-asset trial with a stated budget first.

### Q-13: Audio sourcing
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: P5 audio pipeline proof.
- Needed by: P5.
- Question: Where does audio come from?
- Options:
  - original recordings or synthesis;
  - licensed libraries;
  - commissioned work;
  - AI generation, subject to its terms.
- Recommendation:
  - Original or clearly licensed sources, with a provenance record for every file, in the same way as art.

### Q-14: When to enable auto-merge
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: nothing now. Auto-merge is off at the repository level (`allow_auto_merge: false`).
- Needed by: P0 exit.
- Question: Should agents be allowed to arm GitHub auto-merge? If so, under which conditions?
- Options:
  - **A.** Never. The owner merges every PR by hand.
  - **B.** An agent arms auto-merge only after the owner confirms that specific PR, and only once required checks and the cross-provider verdict are in place.
  - **C.** Auto-merge when all machine gates pass, with no per-PR confirmation.
- Evidence: [workflow.md § Auto-merge](workflow.md#auto-merge).
- Recommendation:
  - **B**, once the docs-check CI and a `main` ruleset exist, which are P0 work.
  - Both role models use per-PR confirmation.

### Q-15: How the cross-provider review is started
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: the reviewer step of each PR. The session 1 PR needs this answered or handled by hand.
- Needed by: now.
- Question: Which second provider reviews, and who starts it?
- Options:
  - **A.** Codex reviews Claude-authored PRs, and Claude Code reviews Codex-authored PRs.
    - The owner starts the review with the prompt in [workflow.md](workflow.md#cross-provider-review).
    - Codex CLI 0.157.1 is installed on the development Mac.
  - **B.** As A, but the author session starts the Codex CLI itself. Script it later.
  - **C.** Another provider.
- Recommendation:
  - **A** now. Move to **B** as a small P0 PR once the review record format has been used a few times.

### Q-16: Gitar integration
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: the Gitar gate. Nothing else waits on it (D-7).
- Needed by: when the owner confirms that Gitar is installed on this repository.
- Question: Has Gitar been integrated, and should its pass become a merge gate?
- Options: keep it documented only (the current state), or activate the gate as described in [workflow.md § Gitar](workflow.md#gitar-planned-not-active).
- Recommendation: No action until the owner explicitly confirms. Then activate it in one dedicated PR.

### Q-17: AI attribution in commits and PRs
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: nothing.
- Needed by: P0 exit.
- Question: Should commits and PRs carry AI co-author trailers or "generated with" lines?
- Options:
  - **A.** Keep them. This is the current default of the authoring tool.
  - **B.** Remove them everywhere, as both role models do. The handoff `Author:` field still identifies the provider for the review gate.
- Recommendation: The owner chooses. Either way, the handoff `Author:` field stays, because the cross-provider rule needs it.

### Q-18: Accept the high-level roadmap
- Status: Open
- Raised: 2026-09-26, session 1
- Blocks: the creation of any focused (low-level) roadmap (D-11).
- Needed by: before P0 closes.
- Question: Does the owner accept the phases, dependencies and gates in [high-level-roadmap.md](roadmaps/high-level-roadmap.md), or ask for changes?
- Recommendation: Accept after the cross-provider review of this PR. Record the answer as a `D-N` entry.
