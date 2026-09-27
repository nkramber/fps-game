## Summary

<!-- What changes, and why. One concern per PR (D-5). Name the roadmap work area, for example P0.2. -->

## Validation

<!-- The commands you ran and their real results. Say plainly what was skipped, not available or still running. Engine PRs: include the engine and Xcode versions and log excerpts. -->

## Documents

<!-- One line each: "Changed: <why>", "No change needed: <why>", or "Not applicable: <why>". -->

- `docs/design.md`:
- `docs/decisions.md`:
- `docs/questions.md`:
- `docs/roadmaps/`:
- `docs/workflow.md` / `AGENTS.md`:
- `docs/session-handoff.md`:
- `README.md`:

## Merge gate

- [ ] Branch is based on the current `origin/main`; the diff contains only intended files.
- [ ] Validation above is real and complete for this change.
- [ ] No credentials, account data, generated caches or machine-specific paths (D-9).
- [ ] New decisions are in `decisions.md`, and new questions in `questions.md`; nothing was chosen on the owner's behalf.
- [ ] The newest handoff entry names this branch and ends with a handover prompt.
- [ ] Cross-provider review: `docs/reviews/pr-<n>.md` says `Ready for owner merge` for the current head (D-6).
- [ ] The owner authorized the merge (D-5).
