# Role-model patterns

Status: research, checked 2026-09-26 and revised 2026-09-27 after D-12. Written in ASD-STE100 (D-17).

- The role models are two sibling repositories on the machine of the owner. Both are Godot 4 games in C#.
  - the-thing-below: the session read its branch `feat/pr-108-night-recovery` at `ef6eae8`. Its `main` was at `a8ba710`.
  - what-you-carry: the session read its `main` at `4a20b82`.
- Method: the session read the entry points directly. These were the agent files, the PR templates, the ruleset, the transitional prompts, a review record, the gitar wait script, and the skills. Two read-only passes surveyed the rest.
- Paths start with the name of the role model, for example what-you-carry/AGENTS.md. This text does not cite a decision id of a role model, because the reference check reads each id as an id of this repository.

The owner made the role models paramount for all infrastructure on 2026-09-27 (D-12). When the two differ, the owner decides each time (D-13). So the dispositions below follow D-12 to D-23, not the first proposal of session 1.

## 1. Observed patterns

| # | Pattern | Evidence | Why it helps |
|---|---|---|---|
| 1 | One agent entry point. `CLAUDE.md` and `AGENTS.md` are byte-identical, and a check enforces it. It has a first action, a read order, tenets, and a PR gate. | what-you-carry/AGENTS.md (161 lines), the-thing-below/AGENTS.md (175 lines) | A fresh session with no memory knows what to read and in which order. |
| 2 | Newest handoff entry first, in six parts. A session reads it before any other file. | what-you-carry/docs/session-handoff.md, the-thing-below/docs/session-handoff.md | Continuity across sessions and providers without shared memory. |
| 3 | Rotation of the handoff. Entries after the tenth move to an archive. Size limits apply to the top entry. | what-you-carry handoff-rotate command, the-thing-below HANDOFF rules | The start file stays small. The archives still reached about 765 KB. |
| 4 | Transitional prompt after the merge. It names the merge sha, branch, base, role, open questions, and first action. | what-you-carry/.claude/skills/one-pr-one-session/references/merge-prompt.md | The next session starts from exact facts, not from a summary. |
| 5 | Append-only decision register with ids. A revision marks the old row as superseded or revised in part. | what-you-carry/docs/decisions.md (617 rows), the-thing-below/docs/decisions.md (1,209 rows) | Decisions stay citable and traceable. |
| 6 | Questions register that the owner owns. Each entry has options, a recommendation, and what it blocks. No agent picks a default. | what-you-carry/docs/questions.md, the-thing-below/docs/questions.md | Authority over the product stays with the owner. |
| 7 | Targeted reads with grep commands, not full reads of large registers. | the-thing-below/docs/runbooks/session-context.md | Controls the token cost as the registers grow. |
| 8 | One design doc template: thesis, lessons, system map, cost model, findings, guardrails, roadmap, sequence, and questions. | what-you-carry and the-thing-below design-doc-style skills | Intent, evidence, and plan stay in one place with ids. |
| 9 | Two levels of roadmap. Section 7 of the design doc holds the phases. Focused files hold PR entries with exit tests. | what-you-carry/docs/roadmaps/, the-thing-below/docs/roadmaps/readme.md | Exit tests make "done" checkable. |
| 10 | Global PR-# ids apart from GitHub numbers, as in a subject that ends with `(PR-<n>) (#<m>)`. | git log of both role models | A plan can name a PR before it exists. |
| 11 | One session, one PR, one role. A start gate refuses mixed work. | what-you-carry/AGENTS.md, one-pr-one-session skill | Small, reviewable units of work. |
| 12 | PR template with a PR gate and a Documents section. The doc-gate job parses it. | what-you-carry/.github/pull_request_template.md | Each PR states what it did to each document. |
| 13 | Review record tied to a full head sha, with one verdict in bold. A response file holds the answers of the author. | what-you-carry/docs/reviews/pr-108.md, pr-review skill | The review is durable and tied to an exact commit. |
| 14 | Scripted Codex review: `make codex-review PR=<n>` in a separate worktree, with exit codes and a three-strike stop. | what-you-carry/Makefile, what-you-carry CodexReview tool | The review of the other provider is repeatable. |
| 15 | Review-gate check run. A `pull_request_target` workflow reads the record of the head as data. | what-you-carry/.github/workflows/review-gate.yml | A machine gate for the verdict. |
| 16 | Auto-merge after owner confirmation of a four-part merge summary. | what-you-carry/AGENTS.md, Git rules | Speed, with owner control. |
| 17 | Ruleset of `main` as code, with a test that binds the check names to the jobs. | what-you-carry/.github/rulesets/main.json | Branch protection that a review can read. |
| 18 | Gitar pass: a wait script proves that the review comment is newer than the push. Pause and resume use one marker. | what-you-carry/.github/scripts/gitar-wait.sh | A cheap first review. Its quota stopped it more than one time. |
| 19 | CI cost control: a skip set for documents, cancel of older runs, hosted runners only. | what-you-carry ci-skip action, what-you-carry/docs/runbooks/macos-runner.md | Fast and safe CI on a public repository. |
| 20 | Night gate: a green night within 48 hours. | night workflows of both role models | Finds slow or random failures of their simulation. |
| 21 | Honest status: the handoff names the remote head, and a merge summary names each red check. | what-you-carry/AGENTS.md, Session handoff | No false "it passed" claims. |
| 22 | No attribution. `.claude/settings.json` sets empty attribution. | `.claude/settings.json` of both | An owner choice about provenance in history. |
| 23 | ASD-STE100 for every document, with a checker and size limits. | ste-writing skills of both | Consistent text for models. In the-thing-below, one checker call used 19% of the input tokens of a session. |

## 2. Disposition for this repository

| # | Disposition | How |
|---|---|---|
| 1 | Adopt (D-12) | `AGENTS.md` and `CLAUDE.md` are identical. PR-2 adds the parity check. |
| 2 | Adopt, format of the-thing-below (D-20) | `docs/session-handoff.md` |
| 3 | Adopt (D-12) | PR-4 ports the rotation. `docs/session-handoff-archive.md` exists now. |
| 4 | Adopt, template of what-you-carry (D-21) | `AGENTS.md`, section "The transitional prompt" |
| 5 | Adopt (D-12) | `docs/decisions.md` |
| 6 | Adopt (D-12) | `docs/questions.md` |
| 7 | Adopt (D-12) | `docs/runbooks/session-context.md` |
| 8 | Adopt, template of the-thing-below (D-19) | `docs/design.md`, the design-doc-style skill |
| 9 | Adopt, with the index of the-thing-below (D-18) | `docs/roadmaps/readme.md` |
| 10 | Adopt (D-23) | PR-1 to PR-6 in section 8 of `docs/design.md` |
| 11 | Adopt (D-5, D-12) | PR-3 ports the one-pr-one-session skill. |
| 12 | Adopt, template of what-you-carry (D-22) | `.github/pull_request_template.md`. PR-4 ports doc-gate. |
| 13 | Adopt, format of what-you-carry (D-14) | `docs/reviews/readme.md` |
| 14 | Adopt, implementation of what-you-carry (D-14) | PR-3 |
| 15 | Adopt (D-12) | PR-6 |
| 16 | Adopt (D-12) | PR-6. The owner turns on the setting. |
| 17 | Adopt (D-12) | PR-5 |
| 18 | Plan only (D-7) | After OQ-16. The two role models differ on the required check. |
| 19 | Adopt (D-12) | PR-2 and later workflows |
| 20 | Not now | Their night runs their simulation. A night of engine tests can come after phase 3, as a new question. |
| 21 | Adopt (D-12) | `AGENTS.md` |
| 22 | Adopt (D-16) | `.claude/settings.json` |
| 23 | Adopt, rules of the-thing-below (D-17) | the ste-writing skill. PR-2 ports the checker. |

## 3. What must not transfer

D-12 covers infrastructure. The game rules and the engine technology of the role models stay out (D-2, D-3):

- Godot, and the Godot smoke session flags.
- The determinism tenet of the-thing-below, bit identity across platforms, replay hashes, and seed sweeps.
- Their game art rules: texel atlases, sprites, palettes, and voxel model checks.
- The Steam Deck rules, and the three-platform matrix on each PR. Unreal builds cost too much for that.
- The game text style, the world docs, and the genre rules.

Their C# tools do transfer as a port (D-15). The session removes the parts that serve their game, for example the smoke filter rule of the ste-check.

## 4. Gaps and risks for this project

- **Process overhead.** In what-you-carry, 13 of the last 22 entries of its phase 2 roadmap are review, gitar, night, or CI items. The-thing-below measured 68.4 million context tokens for its mean code PR. Phase 0 ports six PRs of infrastructure before any engine work. The owner accepts that cost through D-12.
- **Growth of the registers.** Both decision registers grew past 350 KB in two weeks. Keep rows short, cite ids, and use targeted reads.
- **Churn of external services.** The-thing-below paused and resumed gitar two times in a few days. Keep one reversible switch for each service (L-5).
- **Engine CI.** Neither role model builds a heavy engine in CI. This project needs its own evidence model (OQ-8).
- **Binary assets.** Neither role model uses binary engine assets at scale. LFS quotas, locks, and map conflicts are new risks (OQ-7).
- **Provider proof.** Both providers push as one GitHub account, so no machine check proves the provider (F-9).
- **License.** The role models do not use Unreal Engine. This project has the GPL conflict (OQ-1).
- **Divergence.** The role models differ in many small ways. Each difference costs an owner question (D-13). On 2026-09-27 the session asked 14 questions, and 8 of them were about differences.
