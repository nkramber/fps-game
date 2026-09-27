# fps-game

An original, fast-paced, resource-limited first-person shooter, planned in Unreal Engine 5. The first goal is **one complete, polished, replayable level** ([D-1](docs/decisions.md)).

## Current status

- **Planning only.** There is no Unreal project, game code or asset on `main` yet, and nothing is playable.
- The [high-level roadmap](docs/roadmaps/high-level-roadmap.md) is a draft waiting for owner acceptance. Its first engine step (P1) is blocked by an open licensing question ([Q-1](docs/questions.md)).
- The engine version and supported platforms are not chosen yet ([Q-4, Q-5](docs/questions.md)).

## Start here

| If you are | Read |
|---|---|
| An agent session | [AGENTS.md](AGENTS.md) |
| Catching up | [docs/session-handoff.md](docs/session-handoff.md), newest entry |
| Looking for intent | [docs/design.md](docs/design.md) |
| Looking for the plan | [docs/roadmaps/](docs/roadmaps/README.md) |
| Checking a choice | [docs/decisions.md](docs/decisions.md) (settled) and [docs/questions.md](docs/questions.md) (open) |
| Contributing | [docs/workflow.md](docs/workflow.md) |
| Looking for evidence | [docs/research/](docs/research/) |

## Setup

No setup is possible yet. The engine, toolchain and source-control setup (Unreal Engine, Xcode, Git LFS) will be documented and proven in roadmap phase P1.

## Repository conventions

- Commit source assets, the `.uproject`, `Config/`, `Content/`, `Source/` and the project plugins needed to build the game, once they exist.
- Don't commit generated Unreal folders (`Binaries/`, `DerivedDataCache/`, `Intermediate/`, `Saved/`), credentials, account data or machine-specific paths.
- Keep every generated or third-party asset traceable to its source, license and import settings.

## License

See [LICENSE](LICENSE). Its compatibility with the Unreal Engine EULA is an open owner question ([Q-1](docs/questions.md)).
