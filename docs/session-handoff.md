# Session handoff

This file holds the newest entries first, with one entry per session.

- Each session adds one new entry at the top and never edits older entries.
- Keep the 10 newest entries here. When an 11th is added, move the oldest to `docs/session-handoff-archive.md`. Create that file the first time it's needed.

Entry format:

```
## Session N: YYYY-MM-DD, <Claude Code|Codex>
Author: <Claude Code|Codex>
Session: branch `<branch>`, PR #<n> (<open|merged as sha>), role <author|reviewer>, base `<sha>`.

### What this session did, and why
### State of the build (remote head, checks)
### In flight
### Traps and gotchas
### Questions that block progress
### Next concrete action
(ends with the fenced handover prompt from docs/workflow.md)
```

---

## Session 1: 2026-09-27, Claude Code
Author: Claude Code
Session: branch `docs/foundation-roadmap`, PR #1 (open), role author, base `bfb71cf`.

### What this session did, and why
- Studied both role-model repositories and recorded what to adopt, adapt or decline in [role-model-patterns.md](research/role-model-patterns.md).
- Re-checked the engine, Meshy, art-pipeline and level-architecture evidence against primary sources in [technology-and-art-pipeline.md](research/technology-and-art-pipeline.md).
- Set up the documentation system (D-12) and the workflow (D-13):
  - `AGENTS.md`, with `CLAUDE.md` importing it;
  - `README.md`;
  - `docs/design.md`, `decisions.md` (D-1–D-13) and `questions.md` (Q-1–Q-18);
  - `workflow.md`, `reviews/README.md` and the PR template.
- Wrote the **high-level roadmap** P0–P9 ([high-level-roadmap.md](roadmaps/high-level-roadmap.md)). This was the main deliverable.
- No game, Unreal project, asset or CI was created. That was deliberate.

### State of the build (remote head, checks)
- `origin/main` is `bfb71cf`.
- There is no Unreal project, workflow or branch protection.
- The repository setting `allow_auto_merge` is false.
- No CI exists. The docs were checked locally with a temporary link, anchor and ID script that was not committed. P0.2 will add a real one.

### In flight
- This PR is waiting for a cross-provider review (Codex, Q-15) and the owner's merge. It must not be merged without owner authorization.

### Traps and gotchas
- **The owner's local `main` differs from `origin/main`.**
  - Local `main` is 2 commits ahead, `91246cc` and `c101caf`, both unreviewed.
  - It also has untracked `Emberline.uproject`, `Source/` and `Tests/`, plus ignored generated art.
  - This session left all of it untouched and worked in a separate worktree (Q-2).
  - Pushing local `main` would conflict with this PR at `docs/research/technology-and-art-pipeline.md`.
- **License conflict.** `LICENSE` is GPL-3.0, and the Unreal EULA prohibits combining it with GPL code (Q-1). Don't commit Unreal code until the owner resolves this.
- **Xcode.** Xcode 16.2 is installed, and it can't serve UE 5.8 on Tahoe. UE 5.8 needs Xcode 26.1.1 (Q-4).
- **Disk.** The internal disk has 45 GB free. Install the engine on the SSD.
- **Enhanced Input docs.** Epic's Input overview page for 5.8 calls it "experimental". The dedicated page says it is enabled by default. Verify in the editor.

### Questions that block progress
- **Q-1:** license. Blocks P1.
- **Q-2:** local prototype. Blocks a clean local `main`.
- **Q-15:** how the review is started. Blocks the review of this PR.
- **Q-18:** roadmap acceptance. Blocks focused roadmaps.
- Q-3 to Q-8 are needed at the start of P1.

### Next concrete action
- Owner:
  - start the Codex review of this PR with the prompt in `docs/workflow.md`;
  - answer Q-1, Q-2 and Q-18.
- After the merge, the next session implements P0.2, the docs-check CI.

```
Start: P0.2 docs-check CI (link/anchor check, D-/Q- ID checks, handoff format) as one PR.
Repository: nkramber/fps-game. Base: origin/main at <merge sha of PR #1> (PR #1 merged).
Branch to create: chore/p0-docs-check. Role: author. Provider: any (the reviewer must be the other provider).
First: read the newest entry of docs/session-handoff.md, then AGENTS.md.
Roadmap: docs/roadmaps/high-level-roadmap.md, work area P0.2; process in docs/workflow.md § CI/CD strategy.
Open questions that affect this work: Q-14 (auto-merge), Q-17 (attribution); Q-18 must be answered before any focused roadmap.
Owner actions still pending: Q-1 license, Q-2 local prototype, Q-18 roadmap acceptance.
First concrete action: write scripts/check-docs (no heavy dependencies) and run it against the current docs.
```
