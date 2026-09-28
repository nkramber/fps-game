# iron-absolution

Iron Absolution (working title, D-39) is an original, fast first-person shooter with limited resources, planned in Unreal Engine 5.8 for Windows (D-91). The first goal is one complete, polished level that the player can play again (D-1, D-37). Agents and people start at `AGENTS.md`.

## Status

- The repository holds plans, rules, a C# tools project, and an empty Unreal project in `Game/` (D-73). The project has one test map and its automation tests. It has no game rules and no assets. Nothing is playable.
- The high-level roadmap is sections 7 and 8 of `docs/design.md`. The owner accepts it on the condition of D-27.
- The current work is phase 1: the proof of the engine and the toolchain on the Windows PC (D-91).

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

The tools project needs the .NET SDK of `global.json`. Run `run.ps1 verify` to build, test, and check the repository, and run `run.ps1 hooks` one time. Run `run.ps1 help` for each target.

The engine work runs on Windows alone (D-91). `docs/runbooks/engine-setup.md` installs the toolchain. `run.ps1 toolchain-check` checks its pins, and `run.ps1 editor-build` and `run.ps1 editor-test` build and test the project.

## Repository conventions

- Commit the Unreal source assets, the `.uproject` file, and the `Config/`, `Content/`, and `Source/` folders when they exist.
- Do not commit generated Unreal folders (`Binaries/`, `DerivedDataCache/`, `Intermediate/`, `Saved/`). Do not commit credentials, account data, or machine-specific paths (D-9).
- Keep each generated or third-party asset traceable to its source, its license, and its import settings.

## License

The MIT License covers the whole repository, code and content (D-24). See `LICENSE`. A third-party asset keeps its own terms, and it enters the repository only when those terms allow redistribution.
