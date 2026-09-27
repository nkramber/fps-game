# Decisions

This file lists the decisions that are settled. Open choices go in [questions.md](questions.md), not here.

## Conventions

- **Append-only.** Add each new entry at the bottom with the next `D-N` number. Never renumber, delete or reuse an ID.
- **Settled only.** An entry needs a source that settles it:
  - the owner's words, with the date and where they were given; or
  - a convention the owner delegated to a session, which takes effect when its PR merges. The owner can revise it at any time.

  A recommendation is not a decision.
- **Revisions.** Record a change as a new entry. Then add one line to the old entry: `- Revised by D-N (YYYY-MM-DD): <what changed; what still stands>` or `- Superseded by D-N (YYYY-MM-DD)`. Don't edit the old decision text. If you cite a superseded entry, also cite the entry that replaced it.
- **From question to decision.** When the owner answers a `Q-N`:
  1. Add a `D-N` entry here that says `Resolves Q-N`.
  2. Set that question's status to `Answered → D-N`.

  Do both in the same PR.
- **Lookup.** Find an entry with `grep -n '^### D-12:' docs/decisions.md`. Once the file grows, don't read it end to end.

Entry format:

```
### D-N: Short title
- Date: YYYY-MM-DD
- Source: owner (where) | delegated convention (PR / session)
- Decision: one or two sentences.
- Effect: what this constrains or enables; links.
```

---

### D-1: Product goal
- Date: 2026-09-26
- Source: owner, session 1 brief.
- Decision:
  - The long-term product is **one complete, polished, replayable level** of a fast-paced, resource-limited first-person shooter.
  - *Doom (2016)* is a high-level reference for **combat intensity and pacing only**.
- Effect:
  - Every phase of the [high-level roadmap](roadmaps/high-level-roadmap.md) serves this goal.
  - Further levels are out of scope until the owner accepts the first one.

### D-2: Original content only
- Date: 2026-09-26
- Source: owner, session 1 brief.
- Decision:
  - The setting, characters, weapons, names, art, audio and level design are all original.
  - No protected game content is copied.
- Effect:
  - Reference games inform feel and pacing, not specific mechanics, assets, names or layouts.
  - Reviews check this. Asset provenance is recorded (see D-8, Q-12, Q-13).

### D-3: Engine family is Unreal Engine 5
- Date: 2026-09-26
- Source: owner, session 1 brief.
- Decision: The game is built with Unreal Engine 5.
- Effect: The exact version, the platforms and the split between C++ and Blueprint are still open: Q-4, Q-5 and Q-6.

### D-4: Compact authored level; World Partition is not a default
- Date: 2026-09-26
- Source: owner, session 1 brief ("Do not select World Partition by default").
- Decision:
  - The first level is planned as a compact, hand-authored space.
  - World Partition and other large-world tools are adopted only when measured evidence justifies them.
- Effect: See the level architecture section of [technology-and-art-pipeline.md](research/technology-and-art-pipeline.md#4-level-and-gameplay-architecture).

### D-5: One session, one scoped PR; the owner controls merges
- Date: 2026-09-26
- Source: owner, session 1 brief.
- Decision:
  - Each agent session delivers one PR with one concern, on a short branch, with conventional commit subjects.
  - Agents don't push to `main` or force-push.
  - Agents don't bypass checks or review gates.
  - Agents don't merge without explicit owner authorization.
- Effect: [workflow.md](workflow.md) describes the loop. Auto-merge stays off until Q-14 is answered.

### D-6: Cross-provider review
- Date: 2026-09-26
- Source: owner, session 1 brief.
- Decision:
  - Every PR is reviewed by an AI provider other than the one that wrote it.
  - The authoring provider never self-reviews. A subagent of the same provider doesn't count as a cross-provider review.
- Effect: See [workflow.md § Cross-provider review](workflow.md#cross-provider-review). How the review is started is still open: Q-15.

### D-7: Gitar is documented, not implemented
- Date: 2026-09-26
- Source: owner, session 1 brief.
- Decision:
  - The Gitar automated review pass is documented as a planned gate.
  - No Gitar step, script, required check or wait is added until the owner explicitly confirms that Gitar is integrated with this repository.
- Effect: See [workflow.md § Gitar](workflow.md#gitar-planned-not-active) and Q-16.

### D-8: Meshy spending and asset readiness
- Date: 2026-09-26
- Source: owner, session 1 brief.
- Decision:
  - Agents don't spend money on Meshy or generate paid assets without explicit owner approval.
  - A generated mesh is never assumed game-ready.
  - A generated mesh is never assumed suitable for whole buildings or terrain.
- Effect:
  - Q-12 holds the plan and scope choice.
  - Any Meshy asset needs a provenance record and a cleanup pass (see [technology-and-art-pipeline.md § 2](research/technology-and-art-pipeline.md#2-meshy)).

### D-9: Never commit secrets or machine state
- Date: 2026-09-26
- Source: owner, session 1 brief.
- Decision: Never commit:
  - credentials or tokens;
  - Meshy account data;
  - generated caches (`Binaries/`, `Intermediate/`, `DerivedDataCache/`, `Saved/`);
  - machine-specific absolute paths.
- Effect: `.gitignore` covers the Unreal caches. Reviews check the rest.

### D-10: The license file is owner-controlled
- Date: 2026-09-26
- Source: owner, session 1 brief ("Preserve the license verbatim").
- Decision: Agents never edit `LICENSE`. Only the owner changes it.
- Effect: The compatibility problem between GPL-3.0 and the Unreal Engine EULA is raised as Q-1. Agents don't resolve it.

### D-11: High-level roadmap first; focused roadmaps later
- Date: 2026-09-26
- Source: owner, session 1 brief.
- Decision:
  - The high-level roadmap is written, reviewed and accepted first.
  - Focused, low-level roadmaps for its phases are derived only after that.
- Effect: See the completion rule in [roadmaps/README.md](roadmaps/README.md). Acceptance is Q-18.

### D-12: Documentation system
- Date: 2026-09-26
- Source: convention delegated by the owner's session 1 brief ("propose and establish a minimal, coherent documentation and contribution system"). It takes effect when the session 1 PR merges.
- Decision:
  - The sources of truth are:
    - `docs/design.md`: intent;
    - `docs/decisions.md`: settled choices;
    - `docs/questions.md`: open choices;
    - `docs/roadmaps/`: plans;
    - `docs/research/`: dated evidence;
    - `docs/session-handoff.md`: continuity.
  - IDs are `D-N` and `Q-N`, append-only. Roadmap phases are `P0`–`P9`.
  - `AGENTS.md` is the single agent entry point. `CLAUDE.md` only imports it.
  - Each file is kept to its own job. The rationale is in [role-model-patterns.md](research/role-model-patterns.md).
- Effect: [AGENTS.md](../AGENTS.md) holds the update-together rules.

### D-13: Contribution workflow
- Date: 2026-09-26
- Source: convention delegated by the owner's session 1 brief. It takes effect when the session 1 PR merges.
- Decision: Contributions use:
  - the PR template in `.github/pull_request_template.md`;
  - review records in `docs/reviews/pr-<number>.md`;
  - newest-first handoff entries, each ending with a ready-to-paste handover prompt;
  - squash merges.

  The owner can revise any of these.
- Effect:
  - See [workflow.md](workflow.md).
  - CI and branch rules don't exist yet. Phase P0 of the roadmap adds them.
