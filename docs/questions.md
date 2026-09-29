# Open questions

Status: active register. Owner: the owner of `nkramber/iron-absolution`. Started 2026-09-26. Written in ASD-STE100 (D-17).

This file holds every open question for the owner. Each question has an id (OQ-#). The numbers never change. A resolved question stays in this file with its date and the D-# id that resolved it. Section 9 of `docs/design.md` links here.

How to file a question:

- State the question plainly.
- Give the options with their tradeoffs.
- Give a recommendation and its reason.
- Name what the question blocks.
- Ask the owner at once with the options. Stop the work that the question blocks, and continue the other work.

Every open question belongs to the owner. A session never picks a default. When the two role models differ, the session asks the owner (D-13).

Find a question with `grep -n -E '^[0-9]+\. \*\*OQ-(1|18)\.' docs/questions.md`.

## Questions

1. **OQ-1. License and the Unreal Engine EULA.** The repository license is GPL-3.0. The Unreal Engine EULA prohibits a combination with GPL code. Raised 2026-09-26 (PR-1). Blocks each commit of Unreal code or content, and so phase 1. Resolved 2026-09-27: D-24, the MIT License.
   - Evidence: the "Non-Compatible Licenses" part of the Unreal Engine EULA for Creators names GPL and LGPL as prohibited. It names BSD, MIT, and Apache as allowed. The session read the EULA PDF from Epic on 2026-09-26. The web page refused the automated read, so the owner must confirm the current text. This is not legal advice.
   - Option A: put the game code under a permissive license, for example MIT or Apache-2.0.
   - Option B: make the project proprietary, with all rights reserved.
   - Option C: keep GPL-3.0 for tools with no engine code, and license the Unreal project apart.
   - Option D: get legal advice first.
   - Recommendation: decide before the first Unreal commit. Think about a separate license for art and audio. No agent edits `LICENSE` (D-10).
2. **OQ-2. The local prototype that no review saw.** An earlier local session made work that it never pushed. Raised 2026-09-26 (PR-1). Blocks a clean local `main`, but not PR-1. Resolved 2026-09-27: D-25, discard.
   - Evidence: the local `main` of the owner has two commits ahead of `origin/main`. They are `91246cc` (research and some decisions) and `c101caf` (generators of art and sound).
   - Evidence: the checkout also holds untracked `Emberline.uproject`, `Source/`, and `Tests/`. The checkout also holds generated output that git ignores.
   - Evidence: the commit `91246cc` writes `docs/research/technology-and-art-pipeline.md`. PR-1 writes the same path, so the two conflict.
   - Option A: move the two commits to a local reference branch, and reset the local `main` to `origin/main`.
   - Option B: delete the work.
   - Option C: send the work as a PR as it is.
   - Recommendation: option A. Later PRs can use parts of it after OQ-1, at the correct phase gate. Option C adds GPL Unreal code and settles owner questions without the owner.
3. **OQ-3. The working title and the project name.** What is the working title, and what name do the Unreal project and the tools project use? Raised 2026-09-26 (PR-1). Blocks PR-2 and phase 1, because a rename of a module costs much. Resolved 2026-09-27: D-26, Emberline. Superseded 2026-09-27 by D-39, Iron Absolution, because Steam lists a game with the name Emberline.
   - Option A: a neutral code name that does not depend on the title, for example `FpsGame`.
   - Option B: a working title now. The local prototype used "Emberline", but the owner did not approve it.
   - Recommendation: option A. The display title can change at any time.
4. **OQ-4. Engine version and install.** Which Unreal Engine version does the project pin, and where does it go? Raised 2026-09-26 (PR-1). Blocks phase 1. Resolved 2026-09-27: D-28.
   - Option A: Unreal Engine 5.8, at the latest hotfix (5.8.3 on 2026-09-26), with Xcode 26.1.1.
   - Option B: Unreal Engine 5.7. It needs a new Xcode too, and it gives no benefit.
   - Place: the project SSD has 923 GB free. The internal disk has 45 GB free.
   - Recommendation: option A, pinned to 5.8, on the SSD. Each hotfix gets its own PR. Do not use Xcode 26.4 or later.
5. **OQ-5. Platforms, input, and the performance budget.** What platforms, input devices, and frame budget does the first level need? Raised 2026-09-26 (PR-1). Blocks the gate of phase 3. Resolved 2026-09-27: D-32.
   - Option A: macOS on Apple Silicon, keyboard and mouse, 60 fps at 1080p on the M4 with 16 GB.
   - Option B: option A and Windows. Windows needs Windows hardware for each test.
   - Option C: a higher frame target, for example 120 fps. It limits the light and the art.
   - Recommendation: option A for development and acceptance. Decide on Windows before phase 8.
6. **OQ-6. C++ and Blueprint.** Is the project C++ with Blueprint content, or Blueprint only? Raised 2026-09-26 (PR-1). Blocks the template of phase 1. Resolved 2026-09-27: D-29.
   - Option A: C++ holds the rules. Blueprint and data assets hold content, tuning, and one-off scripts of a level.
   - Option B: Blueprint only.
   - Recommendation: option A. Text code can merge, gets unit tests, and a reviewer can read it. Blueprint graphs cannot merge.
7. **OQ-7. Storage of binary assets.** How does the repository keep binary assets? Raised 2026-09-26 (PR-1). Blocks the first Unreal asset commit in phase 1. Resolved 2026-09-27: D-30.
   - Option A: Git LFS on GitHub. The free quota is 10 GiB of storage and 10 GiB of bandwidth each month.
   - Option B: an Unreal version control with file locks, for example Perforce.
   - Option C: a paid GitHub LFS plan near the quota.
   - Recommendation: option A with a size report. Think again near 5 GiB. One person edits a map at a time.
8. **OQ-8. CI for engine builds.** How do PRs prove Unreal builds, cooks, and tests? Raised 2026-09-26 (PR-1). Blocks the gate of phase 1. Resolved 2026-09-27: D-31.
   - Evidence: the role models run CI on hosted runners and retired a self-hosted Mac runner (D-12). Hosted runners have no Unreal Engine.
   - Option A: hosted runners run the checks with no engine. Each engine PR attaches local logs of the build and the tests.
   - Option B: a self-hosted runner on the Mac of the owner. Fork PRs of a public repository then run code there.
   - Option C: paid or private build machines.
   - Recommendation: option A now. Think again at the gate of phase 3.
9. **OQ-9. Setting, tone, and art direction.** What is the original setting, tone, and visual style? Raised 2026-09-26 (PR-1). Blocks the gate of phase 2 and each choice of phase 5. Method set 2026-09-27: D-36. Resolved 2026-09-29: D-117, proposal A, Penitent Iron.
   - Options: the owner chooses. Phase 2 can give two or three short original proposals.
   - Recommendation: a style that a small team can make with modular kits and trim sheets. Photoreal organic terrain costs much more.
   - Plan: PR-17 gives the proposals, and the owner picks during that PR (D-111, D-113).
10. **OQ-10. The combat loop with limited resources.** Which resources are limited, and which original mechanic gives them back in a fight? Raised 2026-09-26 (PR-1). Blocks phase 2 and phase 4. Method set 2026-09-27: D-36. Resolved 2026-09-29: D-115, the combat rules of Doom (2016).
    - Options: the owner chooses. Phase 2 can compare two or three original ideas on paper.
    - Recommendation: one main recovery mechanic that rewards attack. Test it in the combat sandbox of phase 4. Do not copy a mechanic of the reference game (D-2).
    - Plan: PR-16 gives the proposals, and the owner picks during that PR (D-111, D-113).
11. **OQ-11. Level scope and replay value.** How long is the level, how many fights and roster types does it have, and why do players replay it? Raised 2026-09-26 (PR-1). Blocks phase 2, phase 6, and phase 8. Resolved 2026-09-27: D-37, 30 minutes or more, and replay value is not a primary goal.
    - Options for replay value: levels of difficulty, a score or a rank, a timer, secrets, other routes, or changed fights.
    - Recommendation: 15 to 25 minutes for a first clear, and 4 to 6 combat spaces. Use a small roster. Give replay value through difficulty and a score or time rank. These numbers are assumptions for phase 2 to test.
12. **OQ-12. Meshy and its plan.** Does the project use Meshy? If yes, on which plan and for which assets? Raised 2026-09-26 (PR-1). Blocks each Meshy asset (D-8). The owner deferred it on 2026-09-27: decide after the art direction of phase 2. Resolved 2026-09-29: D-118. Option C, for focal props alone.
    - Option A: no Meshy.
    - Option B: the free plan. Meshy owns the output under CC BY 4.0. The models are public, and credits need attribution.
    - Option C: a paid plan. The customer owns the output and can keep it private. Meshy can still use it to train its models.
    - Evidence: the Unreal plugin has Windows builds up to Unreal Engine 5.7 only. Its bridge needs Meshy Pro.
    - Recommendation: decide after the art direction. If yes, use option C with a manual FBX export, for focal props only. Start with a trial of one to three assets and a budget.
    - Plan: PR-18 checks the Meshy facts again and asks the owner, after the pick of PR-17 (D-111, D-112).
13. **OQ-13. Audio sources.** Where does the audio come from? Raised 2026-09-26 (PR-1). Blocks the audio proof of phase 5. Resolved 2026-09-27: D-38.
    - Options: original recordings or synthesis, licensed libraries, commissioned work, or AI generation under its terms.
    - Recommendation: original or clearly licensed sources, with a provenance record for each file.
14. **OQ-14. Auto-merge.** Can an agent arm GitHub auto-merge, and on which conditions? Raised 2026-09-26 (PR-1). Resolved 2026-09-27: D-12. The auto-merge procedure of the role models applies after the owner confirms each PR. PR-6 enables it.
15. **OQ-15. The start of the cross-provider review.** Which provider reviews, and who starts it? Raised 2026-09-26 (PR-1). Resolved 2026-09-27: D-14. Codex reviews through `run.ps1 codex-review`, which PR-3 adds.
16. **OQ-16. Gitar.** Does gitar work on this repository, and does its pass become a gate? Raised 2026-09-26 (PR-1). Blocks the gitar pass only (D-7). The owner said on 2026-09-27 that gitar does not work here yet.
    - Evidence: the role models differ. The-thing-below makes the gitar check run a required check. What-you-carry keeps it out of its ruleset (D-13).
    - Recommendation: no action until the owner confirms that gitar works here. Then ask the divergence question, and add the pass in one PR.
17. **OQ-17. AI attribution.** Do commits and PRs carry AI co-author lines? Raised 2026-09-26 (PR-1). Resolved 2026-09-27: D-16. No attribution (tenet T-6).
18. **OQ-18. Acceptance of the high-level roadmap.** Does the owner accept the phases, the order, and the gates in `docs/design.md` sections 7 and 8? Raised 2026-09-26 (PR-1). Blocks each focused roadmap (D-11). Resolved 2026-09-27: D-27, with one condition.
    - Recommendation: accept after the cross-provider review of PR-1. Record the answer as a new D-# row.
19. **OQ-19. A review skip for PRs with no code.** D-6 asks for a cross-provider review of each PR. Both role models let the owner skip it with a `review-override` label on a PR with no code. Which rule applies? Raised 2026-09-27 (PR-1). Blocks PR-6. Resolved 2026-09-27: D-35.
    - Option A: keep D-6. Each PR gets a Codex review.
    - Option B: add the label rule of the role models. Only the owner adds the label.
    - Recommendation: option B after PR-6. It saves a review round on small document PRs, and the owner still controls it.
20. **OQ-20. Assertions in shipped builds.** Tenet T-2 keeps assertions on in shipped builds. Unreal removes `check` from a build of the Shipping configuration by default. Which rule applies? Raised 2026-09-27 (PR-1). Blocks the project template of phase 1. Resolved 2026-09-27: D-34.
    - Option A: turn on the checks in the target rules of the game for the Shipping configuration. Measure the cost.
    - Option B: keep the Unreal default, and log each failed condition in the Shipping configuration.
    - Recommendation: option A, if the frame budget allows it. Measure it in phase 1.
21. **OQ-21. Gamepad support.** Does the first level support a gamepad? Raised 2026-09-27 (PR-1). Blocks phase 8. D-32 sets keyboard and mouse from the start.
    - Option A: add gamepad support before the release candidate, with aim assist and dead zones.
    - Option B: keyboard and mouse only for the first level.
    - Recommendation: decide before phase 8. Set up Enhanced Input in phase 3 so that a gamepad is a small addition.
22. **OQ-22. The Windows toolchain pin.** Which Visual Studio version and which MSVC toolset does the Windows PC use with Unreal Engine 5.8? Raised 2026-09-27 (PR-7). Blocks PR-8. Resolved 2026-09-27: D-74.
    - Option A: pin the versions that the Windows requirements page of Epic gives for 5.8. PR-8 checks that page with a date.
    - Option B: record the versions that the Windows PC has now, and pin them.
    - Recommendation: option A. The Mac pin follows the page of Epic in the same way (D-28).
23. **OQ-23. The scene of the first M-9 run.** Which scene gives the first value of M-9 on the Mac? Raised 2026-09-27 (PR-7). Blocks PR-11. Resolved 2026-09-27: D-75. Superseded 2026-09-28: D-95 takes M-9 out of scope.
    - Option A: the empty test map of PR-9. The value shows the fixed cost of the frame alone.
    - Option B: a sample scene of Epic on the Mac, outside the repository. Its terms keep it out of the repository (D-24).
    - Option C: a small original room of basic shapes in PR-11, with a set count of lights and meshes.
    - Recommendation: option C. The room is original, the repository holds it, and each later run can use it again.
24. **OQ-24. The kind of the harvest tool.** What original tool takes the role of the chainsaw of D-115? It uses fuel, kills an enemy at once, and gives ammo (D-125). Raised 2026-09-29 (PR-19). Blocks the harvest tool in phase 4.
    - Option A: a tool of the order, for example a rivet driver, that fits a world of riveted iron bodies.
    - Option B: a heavy two-hand weapon, for example a warden's hammer.
    - Option C: another original tool that phase 4 proposes.
    - Recommendation: phase 4 gives two or three original proposals that fit Penitent Iron (D-36, D-117). The owner picks in that PR (D-113).
