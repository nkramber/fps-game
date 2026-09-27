# Roadmaps

| File | Level | Status |
|---|---|---|
| [high-level-roadmap.md](high-level-roadmap.md) | High-level: phases P0–P9 from an empty repository to the accepted first level | Draft, awaiting owner acceptance (Q-18) |

There are no focused (low-level) roadmaps yet. By D-11, none are written until the high-level roadmap is accepted.

## How the levels relate

- **The high-level roadmap** owns:
  - the phases, their order and dependencies;
  - each phase's outcome, exit criteria and owner questions;
  - the named work areas, such as `P3.2`.

  It doesn't hold task lists.
- **A focused roadmap** covers one phase, or one work area if a phase is large. It turns work areas into PR-sized entries. It never changes a phase's objective, gate or dependencies. A change to those is made in the high-level roadmap first, with a decision.

## Creating a focused roadmap

1. **Timing.** Create one when its phase is next to start, or when a parallel track opens.
   - Don't write focused roadmaps more than about one phase ahead. Plans made that early go stale.
2. **Scope.** One session and one PR, as a docs-only PR.
3. **File name.** `docs/roadmaps/p<N>-<slug>.md`, for example `p1-engine-proof.md`.
4. **Entry format.** One entry per planned PR:

   ```
   ### P<N>.<area>-<k>: Short title
   - Scope: what the PR changes (one concern).
   - Out of scope: what it must not touch.
   - Depends on: earlier entries, D-N, Q-N.
   - Exit tests: numbered, observable checks and the evidence to attach.
   - Review focus: what the cross-provider reviewer should examine.
   ```

5. **Links.**
   - Add the file to the table above.
   - Link it from its phase in the high-level roadmap.
   - Name it in the session handoff.
6. **Status.** When an entry's PR merges, mark the entry `Done in #<GitHub PR>`. When every entry is done and the phase exit criteria are met with evidence, mark the phase done in the high-level roadmap.

Don't invent GitHub PR numbers. Use the entry IDs until a PR exists.
