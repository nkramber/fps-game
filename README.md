# iron-absolution

Iron Absolution (working title, D-39) is an original, fast first-person shooter with limited resources, planned in Unreal Engine 5.8 for macOS and Windows. The first goal is one complete, polished level that the player can play again (D-1, D-37). Agents and people start at `AGENTS.md`.

## Status

- The repository holds plans, rules, and a C# tools project that checks the documents. It has no Unreal project, no game code, and no assets. Nothing is playable.
- The high-level roadmap is sections 7 and 8 of `docs/design.md`. The owner accepts it on the condition of D-27.
- The next work is phase 0: the tools, the automatic Codex review, and the gates of the role models.

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

The tools project needs the .NET SDK of `global.json`. Run `make` to build, test, and check the repository, and run `make hooks` one time. Phase 1 of the roadmap proves and documents the engine, the toolchain, and Git LFS.

## Repository conventions

- Commit the Unreal source assets, the `.uproject` file, and the `Config/`, `Content/`, and `Source/` folders when they exist.
- Do not commit generated Unreal folders (`Binaries/`, `DerivedDataCache/`, `Intermediate/`, `Saved/`). Do not commit credentials, account data, or machine-specific paths (D-9).
- Keep each generated or third-party asset traceable to its source, its license, and its import settings.

## License

The MIT License covers the whole repository, code and content (D-24). See `LICENSE`. A third-party asset keeps its own terms, and it enters the repository only when those terms allow redistribution.

This line breaks rule 8.1; the ste-check job must fail.
