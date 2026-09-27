# fps-game

An original, fast first-person shooter with limited resources, planned in Unreal Engine 5. The first goal is one complete, polished, replayable level (D-1). Agents and people start at `AGENTS.md`.

## Status

- The repository holds plans and rules only. It has no Unreal project, no game code, and no assets. Nothing is playable.
- The high-level roadmap is sections 7 and 8 of `docs/design.md`. It waits for the acceptance of the owner (OQ-18).
- An open license question blocks the first engine phase (OQ-1). The engine version and the platforms are also open (OQ-4, OQ-5).

## Where to read

| For | Read |
|---|---|
| Agent rules | `AGENTS.md` (identical to `CLAUDE.md`) |
| The latest state | the newest entry of `docs/session-handoff.md` |
| Intent, guardrails, and the roadmap | `docs/design.md` |
| Settled choices | `docs/decisions.md` |
| Open choices | `docs/questions.md` |
| Focused roadmaps | `docs/roadmaps/readme.md` |
| Evidence | `docs/research/` |

## Setup

No setup exists yet. Phase 1 of the roadmap proves and documents the engine, the toolchain, and Git LFS. PR-2 adds the tools project and its commands.

## Repository conventions

- Commit the Unreal source assets, the `.uproject` file, and the `Config/`, `Content/`, and `Source/` folders when they exist.
- Do not commit generated Unreal folders (`Binaries/`, `DerivedDataCache/`, `Intermediate/`, `Saved/`). Do not commit credentials, account data, or machine-specific paths (D-9).
- Keep each generated or third-party asset traceable to its source, its license, and its import settings.

## License

See `LICENSE`. Its fit with the Unreal Engine EULA is an open question (OQ-1).
