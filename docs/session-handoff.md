## Session 1: 2026-09-27, Claude Code

Author: Claude Code
Session: author PR-1, round 1. Repository: fps-game. Branch: `docs/pr-1-foundation`. PR: #2. Role: author. Base: `bfb71cf`.

### What this session did, and why

- Studied the two role models and re-checked the engine, Meshy, pipeline, and level research against primary sources (`docs/research/`).
- Wrote the registers (D-1 to D-23, OQ-1 to OQ-20), the design doc with the high-level roadmap (sections 7 and 8), the agent files, the PR template, and the review format.
- The owner then made the role models paramount for all infrastructure (D-12), with a question for each divergence (D-13). The answers are D-14 to D-23: the automatic Codex review, C# tools, no attribution, STE now, and the formats of the-thing-below and what-you-carry.
- PR-1 moved to this new branch with new commits, and GitHub PR #1 closed (D-23). The old commits carried AI co-author lines (F-10).
- Ported the ste-writing and design-doc-style skills, and added `.claude/settings.json`.

### The state of the build

- `origin/main` is `bfb71cf`. No CI, ruleset, or Unreal project exists. The repository setting for auto-merge is off.
- The ste-check binary of the-thing-below, run by hand against this branch, gives no finding that applies here. PR-2 ports the checker.

### What is in flight

- PR-1 waits for a Codex review by hand (the prompt is in `AGENTS.md`) and for the merge by the owner.

### Traps and gotchas

- The local `main` of the owner is two commits ahead, with untracked `Source/`, `Tests/`, and `Emberline.uproject` (OQ-2). The session did not touch them. A push of that `main` conflicts with PR-1.
- `LICENSE` is GPL-3.0, and the Unreal Engine EULA prohibits GPL code (OQ-1). Commit no Unreal code before an answer.
- Xcode 16.2 cannot build Unreal Engine 5.8. It needs Xcode 26.1.1. The internal disk has 45 GB free, so install the engine on the SSD.
- The ste-check of the-thing-below needs `docs/session-handoff-archive.md` and section 8 of `docs/design.md`.
- A harness reminder asks for co-author lines. T-6 wins (D-16).

### The questions that block progress

- OQ-1 blocks phase 1. OQ-3 blocks PR-2 (the name of the tools project). OQ-18 blocks each focused roadmap.
- OQ-2 blocks a clean local `main`.

### The next concrete action

- The owner starts the Codex review of PR-1, answers OQ-1, OQ-2, OQ-3, and OQ-18, and merges PR-1.
- After the merge, write the transitional prompt for PR-2 (tools project and ste-check).
