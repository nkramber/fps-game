## Summary

<!-- What the PR changes, and why. One concern per PR (G-7). Name the roadmap id, for example PR-2. -->

## PR gate

Each line holds before the merge, by auto-merge or by the owner (`AGENTS.md`, PR gate).

- [ ] Tests written and green (T-3). A PR of documents alone needs the ste-check job instead.
- [ ] No silent failure. Every error carries context (T-2).
- [ ] The `ste-check` job is green. PR-2 creates it.
- [ ] The `doc-gate` job is green. PR-4 creates it.
- [ ] The other provider reviewed it, and `docs/reviews/pr-<number>.md` has the verdict `Ready for owner merge` for the effective head (T-4, D-14).
- [ ] The `review-gate` check is green. PR-6 creates it.
- [ ] No review thread stays open, and the ruleset of `main` holds. PR-5 creates the ruleset.
- [ ] The owner confirmed the merge after the merge summary: What, How, CI, and Codex review.
- [ ] `docs/decisions.md` has every new decision.
- [ ] `docs/questions.md` has every new question.
- [ ] `docs/design.md` matches intent.
- [ ] Every check that does not exist yet has a line above with the PR that creates it (G-8).
- [ ] `docs/session-handoff.md` is current, and its newest entry names the branch of this PR.
- [ ] No attribution anywhere (T-6). No commit subject or body names an agent, harness, or model as the source of the work.

## Documents

One line for each category, in this order (D-22). Start the line with `Changed:`, `Reviewed; no change needed:`, or `Not applicable:`. Then give a reason of five words or more that names the part of the document and the cause.

- `docs/design.md`:
- `docs/decisions.md`:
- `docs/questions.md`:
- `docs/roadmaps/`:
- `docs/runbooks/`:
- `docs/session-handoff.md`:
- `CLAUDE.md` and `AGENTS.md`:
- `.claude/skills/`:
