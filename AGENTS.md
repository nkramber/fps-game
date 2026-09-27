# Agent instructions

This is the entry point for every agent session in this repository, whether Claude Code, Codex or another tool. `CLAUDE.md` only imports this file. Edit this file, not that one.

The project is an original, fast-paced, resource-limited first-person shooter in Unreal Engine 5. The goal is one complete, replayable level (D-1). See [README.md](README.md) for the current status.

## First actions

1. `git fetch origin`, then `git status -sb`. Confirm which branch you are on and that it's based on the current `origin/main`.
2. Read the **newest entry** of [docs/session-handoff.md](docs/session-handoff.md). Print it alone with:
   `awk '/^## Session /{n++} n==1' docs/session-handoff.md`
3. Then read the rest of this file.

## Read order

1. The newest handoff entry, plus any older entry it points to.
2. This file.
3. [docs/design.md](docs/design.md): the vision and guardrails.
4. [docs/decisions.md](docs/decisions.md) and [docs/questions.md](docs/questions.md). Look up the `D-N` and `Q-N` of your task by grep, as each file's conventions describe.
5. [docs/roadmaps/](docs/roadmaps/README.md): your phase and work area.
6. [docs/workflow.md](docs/workflow.md): the PR, review, merge and CI rules.
7. [docs/research/](docs/research/) as needed. Research is dated evidence, not a decision.

## Where each kind of truth lives

| Kind | File | Rule |
|---|---|---|
| Intended experience and guardrails | `docs/design.md` | Mark each item Approved (D-N), Proposed, or Open (Q-N). |
| Settled choices | `docs/decisions.md` | Append-only `D-N`. Settled only. |
| Open owner choices | `docs/questions.md` | Append-only `Q-N`, each with options and a recommendation. |
| Plans | `docs/roadmaps/` | High-level first; focused roadmaps derived from it (D-11). |
| Evidence | `docs/research/` | Dated, with sources, labelled evidence, recommendation, assumption or unknown. |
| Continuity | `docs/session-handoff.md` | Newest entry first, one per session. |
| Process | `docs/workflow.md`, this file | Link to details; don't duplicate them. |

**Update together.** A change that answers a question changes three files in the same PR:

- `questions.md`: the status;
- `decisions.md`: the new `D-N`;
- `design.md` or the roadmap: wherever the answer applies.

A change of plan updates the roadmap and cites its `D-N`.

## Working with the owner

- **Product choices belong to the owner.** Don't settle them silently.
  - If a material choice can't be inferred from `decisions.md`, ask the owner a concise question with options and a recommendation, and file it in `questions.md`.
  - Keep working on anything the question doesn't block.
- **Push back** with evidence when a request rests on a wrong premise. If two owner statements conflict, quote both.
- **Record answers** as `D-N` entries with the date and source.
- **Owner-only actions:**
  - editing `LICENSE` (D-10);
  - spending money, for example on Meshy (D-8);
  - changing repository settings or rulesets;
  - merging (D-5);
  - activating Gitar (D-7).

## PR and session scope

- One session delivers one PR with one concern (D-5). Use a short branch and conventional commit subjects.
- Never push to `main`, force-push, bypass checks or reviews, or merge without explicit owner authorization.
- Every PR gets a cross-provider review. The authoring provider never reviews its own PR (D-6). See [docs/workflow.md](docs/workflow.md).
- Don't implement ahead of the roadmap's gates. For example: no level content before the P6 layout lock, and no Unreal code before Q-1 is answered.
- Never commit credentials, Meshy or other account data, generated caches, or machine-specific paths (D-9).
- Leave work you didn't create untouched, such as the owner's local commits or untracked files. Report it.

## Validation

- Run every check that exists for what you changed, and report its real result.
- Never say a check passed if it was skipped, unavailable, interrupted or still running.
- **Docs PRs:** check that every relative link and anchor resolves, that `D-N` and `Q-N` IDs are unique and every cited ID exists, and that the diff holds only the intended files.
  - There is no committed checker yet. Adding one is roadmap work area P0.2.
- **Engine PRs (from P1):** give the engine and Xcode versions, the exact build, test and package commands, and log excerpts. Hosted CI can't build Unreal (Q-8).
- Before you finish, review the full diff against `origin/main`.

## Session handoff

At the end of every session:

1. Fetch the remote.
2. Add **one new entry at the top** of `docs/session-handoff.md`, numbered one above the newest. Never edit an older entry.
3. Commit the entry on the PR branch.

The entry uses the format at the top of that file, and it ends with the handover prompt from [docs/workflow.md § Session handover prompt](docs/workflow.md#session-handover-prompt).

After the push, confirm that `git status -sb` shows no `ahead` count.
