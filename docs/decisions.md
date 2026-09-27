# Decisions

Status: active register. Owner: the owner of `nkramber/fps-game`. Started 2026-09-26. Written in ASD-STE100 (D-17).

This file records each owner decision. Each decision has an id (D-#). The numbers never change. A reversed decision stays in this file with a dated note in the "Effect" column. These decisions are the source of the design doc (`docs/design.md`).

How to read this file:

- The "Decision" column gives the answer.
- The "Effect" column gives what the decision changes, binds, or supersedes.
- A date in the "Effect" column marks a later revision. The note says `Superseded by D-N` or `Revised in part by D-N`, and names the part that changed.
- The owner gave D-1 to D-11 in the brief of session 1 on 2026-09-26.
- The owner gave D-12 to D-23 in answers to questions on 2026-09-27. The session asked each question with options and a recommended option.
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
| D-1 | 2026-09-26 | Goal | The product is one complete, polished, replayable level of a fast, resource-limited first-person shooter. Doom (2016) is a high-level reference for combat intensity and pacing only. | Every phase of the roadmap (`docs/design.md` section 7) serves this goal. More levels stay out of scope until the owner accepts the first level. |
| D-2 | 2026-09-26 | Originality | The setting, characters, weapons, names, art, audio, and level design are all original. The project copies no protected game content. | Reference games inform feel and pacing, not mechanics, assets, names, or layouts. The review checks it. Each asset has a provenance record. |
| D-3 | 2026-09-26 | Engine | The game uses Unreal Engine 5. | The exact version (OQ-4), the platforms (OQ-5), and the split between C++ and Blueprint (OQ-6) stay open. |
| D-4 | 2026-09-26 | Level form | The first level is a compact, hand-authored level. World Partition is not a default. The project adopts large-world tools only when a measurement shows a need. | Binds the level architecture in `docs/research/technology-and-art-pipeline.md` section 4. |
| D-11 | 2026-09-26 | Roadmap order | The high-level roadmap comes first. The owner reviews and accepts it. Focused roadmaps for its phases come after that. | OQ-18 holds the acceptance. `docs/roadmaps/readme.md` gives the rule for focused roadmaps. |

## Process, review, and tools

| Id | Date | Topic | Decision | Effect |
|---|---|---|---|---|
| D-5 | 2026-09-26 | Sessions and merges | Each session delivers one PR with one concern. No agent pushes to `main`, force-pushes, or bypasses a check or a review. No agent merges without explicit owner authorization. | Binds the Git rules in `AGENTS.md`. |
| D-6 | 2026-09-26 | Cross-provider review | A provider other than the author reviews each PR. The author provider never reviews its own PR. A subagent of the author provider is not a cross-provider review. | Tenet T-4. D-14 sets the command. |
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

## Content, assets, and the repository

| Id | Date | Topic | Decision | Effect |
|---|---|---|---|---|
| D-8 | 2026-09-26 | Meshy | No agent spends money on Meshy or makes paid assets without explicit owner approval. The project never assumes that a generated mesh is ready for the game. It never uses one for a whole building or for terrain. | OQ-12 holds the plan and the scope. |
| D-9 | 2026-09-26 | Secrets and caches | No commit holds credentials, Meshy account data, generated caches, or machine-specific paths. | `.gitignore` holds the Unreal caches. The review checks the rest. |
| D-10 | 2026-09-26 | License | No agent edits `LICENSE`. Only the owner changes it. | OQ-1 holds the conflict between GPL-3.0 and the Unreal Engine EULA. |
