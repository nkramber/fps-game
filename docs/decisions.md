# Decisions

Status: active register. Owner: the owner of `nkramber/iron-absolution`. Started 2026-09-26. Written in ASD-STE100 (D-17).

This file records each owner decision. Each decision has an id (D-#). The numbers never change. A reversed decision stays in this file with a dated note in the "Effect" column. These decisions are the source of the design doc (`docs/design.md`).

How to read this file:

- The "Decision" column gives the answer.
- The "Effect" column gives what the decision changes, binds, or supersedes.
- A date in the "Effect" column marks a later revision. The note says `Superseded by D-N` or `Revised in part by D-N`, and names the part that changed.
- The owner gave D-1 to D-11 in the brief of session 1 on 2026-09-26.
- The owner gave D-12 to D-23 in answers to questions on 2026-09-27. The session asked each question with options and a recommended option.
- The owner answered the open questions on 2026-09-27 in the same way. D-24 to D-38 record these answers.
- The owner gave D-39 to D-45 on 2026-09-27 in PR-2. D-40 to D-45 resolve the divergences of the role models that the port of PR-2 met (D-13).
- The owner gave D-46 to D-53 on 2026-09-27 in PR-3. D-46 to D-52 resolve the divergences of the role models that the port of PR-3 met (D-13).
- For D-15, the owner asked for the best fit, and the session recommended it. The owner then chose it.

How to add a decision:

1. Add a row at the end of the table of its topic.
2. Give it the next D-# number.
3. Write "Resolves OQ-N" in the "Effect" column when the decision answers a question.
4. Mark that question "Resolved <date>: D-N" in `docs/questions.md` in the same PR.

Find a row with `grep -n -E '^\| D-(12|14) \|' docs/decisions.md`.

## Product and scope

| Id | Date | Topic | Decision | Effect |
|---|---|---|---|---|
| D-1 | 2026-09-26 | Goal | The product is one complete, polished, replayable level of a fast, resource-limited first-person shooter. Doom (2016) is a high-level reference for combat intensity and pacing only. | Every phase of the roadmap (`docs/design.md` section 7) serves this goal. More levels stay out of scope until the owner accepts the first level. Revised in part by D-37 on 2026-09-27, the meaning of "replayable" alone: the player can play the level again. The rest stands. |
| D-2 | 2026-09-26 | Originality | The setting, characters, weapons, names, art, audio, and level design are all original. The project copies no protected game content. | Reference games inform feel and pacing, not mechanics, assets, names, or layouts. The review checks it. Each asset has a provenance record. |
| D-3 | 2026-09-26 | Engine | The game uses Unreal Engine 5. | The exact version (OQ-4), the platforms (OQ-5), and the split between C++ and Blueprint (OQ-6) stay open. |
| D-4 | 2026-09-26 | Level form | The first level is a compact, hand-authored level. World Partition is not a default. The project adopts large-world tools only when a measurement shows a need. | Binds the level architecture in `docs/research/technology-and-art-pipeline.md` section 4. |
| D-26 | 2026-09-27 | Name | The Unreal project, its primary C++ module, and the tools project use the name Emberline. | Resolves OQ-3. PR-2 creates the tools project with this name. The display title can change at any time. Superseded by D-39 on 2026-09-27: Steam lists an upcoming game with the name Emberline. |
| D-27 | 2026-09-27 | Roadmap acceptance | The owner accepts the high-level roadmap, on one condition. The Codex review of PR-1 must find nothing that changes a phase, its order, or its gate. | Resolves OQ-18. Focused roadmaps can start after the merge of PR-1 (D-11). |
| D-32 | 2026-09-27 | Platforms and budget | The first level ships on macOS and Windows from the start. Windows: 120 fps at 1440p on the owner's PC (Intel Core i9-13900K, NVIDIA RTX 4090, 32 GB DDR5). macOS: 60 fps at 4K output on the M4 with 16 GB, through TSR from a measured internal resolution. Input: keyboard and mouse. | Resolves OQ-5. Gamepad support waits for OQ-21. D-33 sets the Windows validation. |
| D-36 | 2026-09-27 | Direction method | Phase 2 gives two or three original proposals for the setting, tone, and art, and for the mechanic that gives resources back. The owner picks. The sandbox of phase 4 tests the mechanic before level production. | Sets the method of OQ-9 and OQ-10. The choices stay open. |
| D-37 | 2026-09-27 | Level scope | The first clear takes 30 minutes or more, in a fuller level with more spaces and roster variety. Replay-value features are not a primary goal. "Replayable" in D-1 means that the player can start the level again and play it again. | Resolves OQ-11. Revises in part D-1. |
| D-11 | 2026-09-26 | Roadmap order | The high-level roadmap comes first. The owner reviews and accepts it. Focused roadmaps for its phases come after that. | OQ-18 holds the acceptance. `docs/roadmaps/readme.md` gives the rule for focused roadmaps. |
| D-39 | 2026-09-27 | Name | The working title is Iron Absolution. The Unreal project, its primary C++ module, and the tools project use the name `IronAbsolution`. The repository is `nkramber/iron-absolution`, and the local checkout folder is `iron-absolution`. | Supersedes D-26. The owner picked the name from a screen of candidates (`docs/research/name-screen.md`). The session renamed the GitHub repository and the local folder on the instruction of the owner. GitHub redirects the old URL. |

## Process, review, and tools

| Id | Date | Topic | Decision | Effect |
|---|---|---|---|---|
| D-5 | 2026-09-26 | Sessions and merges | Each session delivers one PR with one concern. No agent pushes to `main`, force-pushes, or bypasses a check or a review. No agent merges without explicit owner authorization. | Binds the Git rules in `AGENTS.md`. |
| D-6 | 2026-09-26 | Cross-provider review | A provider other than the author reviews each PR. The author provider never reviews its own PR. A subagent of the author provider is not a cross-provider review. | Tenet T-4. D-14 sets the command. Revised in part by D-35 on 2026-09-27: after PR-6, the owner can skip the review of a PR with no code through a label. The rest stands. |
| D-7 | 2026-09-26 | Gitar | The gitar pass is a documented plan only. No session adds a gitar step, script, wait, or required check until the owner confirms that gitar works on this repository. | OQ-16 holds the confirmation. The plan is in `AGENTS.md` and in `docs/design.md` section 7. |
| D-12 | 2026-09-27 | Infrastructure source | This rule is paramount. Everything about Codex, GitHub, skills, documents, and other infrastructure follows the patterns, procedures, and implementation of the two role-model repositories, what-you-carry and the-thing-below. | Game rules and engine technology of those games do not transfer (D-2, D-3). Resolves OQ-14: the auto-merge procedure of the role models applies. |
| D-13 | 2026-09-27 | Divergence | When the two role models differ, the session asks the owner each time. The answer becomes a decision. | D-14 to D-23 record the answers of 2026-09-27. |
| D-14 | 2026-09-27 | Codex review | Port the `make codex-review PR=<n>` command of what-you-carry. It runs Codex in a separate worktree with the ChatGPT login and no API key. The exit codes are 0 approve, 10 changes, 11 three-strike stop, 3 start fault, and 1 fault. Codex pushes the review record and its own handoff entry as one metadata commit. | The automatic review is required work in phase 0, not optional (PR-3). Resolves OQ-15. |
| D-15 | 2026-09-27 | Tool language | The tools of this repository are one C# .NET project, a port of the role-model tools with their tests. Python in the Unreal Editor is for asset work in the editor only. | The owner asked for the best fit for Unreal C++. The session recommended C#, because Unreal build rules (`Target.cs`, `Build.cs`) are C#. PR-2 creates the project. |
| D-16 | 2026-09-27 | Attribution | Tenet T-6 applies: no attribution. `.claude/settings.json` sets the commit and PR attribution to empty text. | Resolves OQ-17. The PR that replaced GitHub PR #1 has new commits without trailers. |
| D-17 | 2026-09-27 | Text standard | Every `.md`, skill, and agent file follows ASD-STE100, from PR-1. The ste-writing skill and the checker rules of the-thing-below apply, with their size limits. | The ste-writing skill holds the rules. PR-2 ports the checker. |
| D-18 | 2026-09-27 | Roadmap place | Sections 7 and 8 of `docs/design.md` are the high-level roadmap. `docs/roadmaps/readme.md` is the index of the focused roadmaps, as in the-thing-below. | Revises in part the file names of the brief of session 1. The rule of D-11 stands. |
| D-19 | 2026-09-27 | Design doc | `docs/design.md` follows the design-doc-style skill of the-thing-below. | The design-doc-style skill holds the template. |
| D-20 | 2026-09-27 | Handoff format | The session handoff follows the format of the-thing-below. | `AGENTS.md` holds the format. |
| D-21 | 2026-09-27 | Transitional prompt | The prompt that starts the next session follows the template of what-you-carry. A session writes it after the merge of its PR. | `AGENTS.md` holds the template. |
| D-22 | 2026-09-27 | PR template | The PR template follows what-you-carry: Summary, PR gate, and the Documents section with its line prefixes. | The game gate lines of what-you-carry do not transfer (D-12). |
| D-23 | 2026-09-27 | Branch and PR ids | Roadmap entries use global PR-# ids. A branch is named `<prefix>/pr-<n>-<slug>`. A commit subject ends with `(PR-N)`. PR-1 moves to a new branch and a new GitHub PR, and GitHub PR #1 closes. | Binds the Git rules in `AGENTS.md`. |
| D-25 | 2026-09-27 | Local prototype | Discard the local prototype. | Resolves OQ-2. On 2026-09-27 the session reset the local `main` to `origin/main`. It deleted the untracked prototype files and the ignored generated output. |
| D-31 | 2026-09-27 | Engine CI | Hosted runners run each check that needs no engine. Each engine PR attaches local logs of its build, tests, and package. | Resolves OQ-8. Think again at the gate of phase 3. |
| D-33 | 2026-09-27 | Windows validation | The owner runs the Windows builds and tests from documented commands, and posts the logs in the PR. | Phase 1 writes the Windows commands. No session gets remote access to the Windows PC. |
| D-35 | 2026-09-27 | Review override | After PR-6, the owner can add the `review-override` label of the role models to a PR with no code. That PR then merges without a Codex review. Only the owner adds the label. | Resolves OQ-19. Revises in part D-6. PR-6 adds the label to the review gate. |
| D-40 | 2026-09-27 | Tools stack | The tools project follows the-thing-below: the target framework in `Directory.Build.props`, its strict build settings, xunit.v3 on the Microsoft Testing Platform, an `.editorconfig`, and a `dotnet format` check. A usage error exits 1. | Resolves a divergence of D-13 in PR-2. what-you-carry sets the framework in each project and uses xunit 2 on VSTest. |
| D-41 | 2026-09-27 | Makefile | The Makefile follows the-thing-below. `make` alone runs `verify`. A separate `ste-check` target exists. | Resolves a divergence of D-13 in PR-2. what-you-carry prints help by default and runs ste-check inside `lint`. |
| D-42 | 2026-09-27 | CI layout | The CI follows the-thing-below: one `ci.yml` workflow for every job, actions pinned by commit SHA, pinned runner images, and a time limit on each job. | Resolves a divergence of D-13 in PR-2. what-you-carry uses one workflow for each check and actions pinned by tag. |
| D-43 | 2026-09-27 | Pre-commit hook | The repository has the pre-commit hook of the-thing-below. It refuses a commit on `main` or on no branch, and it runs ste-check. `make hooks` installs it. | Resolves a divergence of D-13 in PR-2. what-you-carry has no hook. |
| D-44 | 2026-09-27 | C# skill | A new csharp-conventions skill covers the C# tools alone. It takes the errors, style, tests, and command rules of the-thing-below, and two rules of what-you-carry: no clever one-liners, and tune only on measurement. | Resolves a divergence of D-13 in PR-2. The determinism and Godot rules of both role models do not transfer (D-12). |
| D-45 | 2026-09-27 | Test coverage | The test project writes coverage with coverlet, and a CI job writes a coverage report. No threshold applies. | Resolves a divergence of D-13 in PR-2. what-you-carry has no coverage. |
| D-46 | 2026-09-27 | Review skills | Port the pr-review, review-response, and one-pr-one-session skills of what-you-carry, with their reference files. The parts about gitar stay out (D-7). | Resolves a divergence of D-13 in PR-3. the-thing-below puts the answer of the author into its pr-review skill. |
| D-47 | 2026-09-27 | Codex CLI install | The `codex-review` target of the Makefile installs the newest Codex CLI with npm. It then gives the command the path of the CLI with `--codex` and the PR number with `--pr`. | Resolves a divergence of D-13 in PR-3. the-thing-below installs the CLI inside the command. A test can give the command a fake CLI. |
| D-48 | 2026-09-27 | Review worktree | The review runs in a worktree on the local branch `review/pr-<n>`, which starts at the PR branch on origin. The reviewer pushes with `HEAD:<branch>`. The command removes the branch after a review with no fault. | Resolves a divergence of D-13 in PR-3, as in the-thing-below. The hook of D-43 refuses a commit on no branch, so the detached worktree of what-you-carry does not work here. |
| D-49 | 2026-09-27 | Effective head | The effective head is the newest commit that changes a path outside the documents set: `docs/`, `.claude/skills/`, `CLAUDE.md`, `AGENTS.md`, `README.md`, and `LICENSE`. `make codex-review` refuses a PR with no such commit. Until PR-6, the owner starts Codex by hand for that PR. | Resolves a divergence of D-13 in PR-3, as in what-you-carry. the-thing-below reads the metadata set alone. |
| D-50 | 2026-09-27 | Review time limit | The command stops the Codex review after 90 minutes. The stop is a fault, and the worktree stays for a read. | Resolves a divergence of D-13 in PR-3, as in the-thing-below. what-you-carry has no limit. |
| D-51 | 2026-09-27 | Review transcripts | The command writes the transcript, the error log, and the last message of each review under `artifacts/codex-review/`. Git ignores that folder. | Resolves a divergence of D-13 in PR-3, as in the-thing-below. what-you-carry writes them to the temporary folder of the system. |
| D-52 | 2026-09-27 | Open threads | The command refuses to start while the PR has a review thread that is not resolved. | Resolves a divergence of D-13 in PR-3, as in what-you-carry. the-thing-below reads the threads only in its gitar check. |
| D-53 | 2026-09-27 | API key | The command removes `OPENAI_API_KEY`, `CODEX_API_KEY`, and `CODEX_ACCESS_TOKEN` from each Codex process, as in what-you-carry. It refuses to start when `codex login status` without those variables does not give the ChatGPT login. | Changes exit test 3 of PR-3, which said that a key in the environment refuses the start. A key in the environment of the shell does not stop a review, and it never reaches Codex. |

## Engine and technology

| Id | Date | Topic | Decision | Effect |
|---|---|---|---|---|
| D-28 | 2026-09-27 | Engine version | Pin Unreal Engine 5.8 at its latest hotfix (5.8.3 on 2026-09-26), with Xcode 26.1.1, on the project SSD. Never use Xcode 26.4 or later. Each hotfix upgrade is its own PR with build evidence. | Resolves OQ-4. Binds phase 1. |
| D-29 | 2026-09-27 | C++ and Blueprint | C++ holds the rules: movement, weapons, damage, resources, AI decisions, and save. Data assets and Blueprint subclasses hold tuning, content, and one-off level scripts. | Resolves OQ-6. |
| D-30 | 2026-09-27 | Binary assets | Git LFS on GitHub stores the binary assets, on the free quota. Think again near 5 GiB. One person edits one map at a time. | Resolves OQ-7. The scaffold PR of phase 1 adds the LFS attributes. |
| D-34 | 2026-09-27 | Unreal practice | Unreal best practices govern each rule and each implementation of the engine work, the game code, and the content. A tenet or a role-model rule gives way to them in that scope. Asserts follow the Unreal rules: `check`, `verify`, and `ensure` by purpose, with `USE_CHECKS_IN_SHIPPING` at 0. | Resolves OQ-20. Revises the assert clause of tenet T-2. D-12 still governs the infrastructure. |

## Content, assets, and the repository

| Id | Date | Topic | Decision | Effect |
|---|---|---|---|---|
| D-8 | 2026-09-26 | Meshy | No agent spends money on Meshy or makes paid assets without explicit owner approval. The project never assumes that a generated mesh is ready for the game. It never uses one for a whole building or for terrain. | OQ-12 holds the plan and the scope. |
| D-9 | 2026-09-26 | Secrets and caches | No commit holds credentials, Meshy account data, generated caches, or machine-specific paths. | `.gitignore` holds the Unreal caches. The review checks the rest. |
| D-10 | 2026-09-26 | License | No agent edits `LICENSE`. Only the owner changes it. | Revised in part by D-24 on 2026-09-27: the owner told the session to replace `LICENSE` with the MIT License in PR-1. The rule stands for each later change. |
| D-24 | 2026-09-27 | License | The MIT License covers the whole repository, code and content. The session replaces `LICENSE` in PR-1 on the instruction of the owner. | Resolves OQ-1. Revises in part D-10. A third-party asset keeps its own terms, and it enters the repository only when those terms allow redistribution. Epic content keeps the terms of Epic. |
| D-38 | 2026-09-27 | Audio sources | Audio comes from original work, from free licenses that allow redistribution (for example CC0), or from AI generation with terms that give ownership and allow redistribution. Each file has a provenance record. | Resolves OQ-13. |
