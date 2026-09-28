## Summary

<!-- What the PR changes, and why. One concern per PR (G-7). Name the roadmap id, for example PR-2. -->

## PR gate

Each line holds before the merge, by auto-merge or by the owner (`AGENTS.md`, PR gate).

- [ ] Tests written and green (T-3). A PR of documents alone needs the ste-check job instead.
- [ ] No silent failure. Every error carries context (T-2).
- [ ] The `ste-check` job is green.
- [ ] The `doc-gate` job is green (D-56, D-57). No part of this PR waits for a later PR.
- [ ] The other provider reviewed it, and `docs/reviews/pr-<number>.md` has the verdict `Ready for owner merge` for the effective head (T-4, D-14). On a PR with no code, the label `review-override` that the author session adds can replace the record (D-35, D-66, D-76).
- [ ] The `review-gate` check is green (D-64).
- [ ] No review thread stays open, and the ruleset of `main` holds (D-60 to D-62).
- [ ] The owner confirmed the merge after the merge summary: What, How, CI, and Codex review.
- [ ] `docs/decisions.md` has every new decision.
- [ ] `docs/questions.md` has every new question.
- [ ] `docs/design.md` matches intent.
- [ ] Every check that does not exist yet has a line above with the PR that creates it (G-8).
- [ ] `docs/session-handoff.md` is current, and its newest entry names the branch of this PR.
- [ ] No attribution anywhere (T-6). No commit subject or body names an agent, harness, or model as the source of the work.

## Evidence

One line for each log of an engine PR (D-31, D-80). Start the line with `Attached:`, and name the place of the log: a PR comment or a file. A PR with no engine work writes `Not applicable:` and a reason on each line. The engine PR of each log is in `docs/roadmaps/phase-1-engine-proof.md`.

- Mac toolchain, `make toolchain-check`:
- Windows toolchain, `scripts/toolchain-check.ps1`:
- Mac build:
- Windows build:
- Tests:
- Package:
- Measurements:

## Documents

One line for each category, in this order (D-22). Start the line with `Changed:`, `Reviewed; no change needed:`, or `Not applicable:`. Then give a reason of five words or more that names the part of the document and the cause. The `doc-gate` job reads this section.

- `docs/design.md`:
- `docs/decisions.md`:
- `docs/questions.md`:
- `docs/roadmaps/`:
- `docs/runbooks/`:
- `docs/session-handoff.md`:
- `CLAUDE.md` and `AGENTS.md`:
- `.claude/skills/`:
