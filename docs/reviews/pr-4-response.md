# PR-4 response

The answer of the author to the first review round of PR #4 (PR-3), at the effective head `b0596f5`.

### P2-1
- Disposition: full
- Evidence: The claim reproduces from the package metadata. The `bin` entry of `@openai/codex` is `bin/codex.js`. On macOS, `<npm prefix>/bin/codex` is a link to that script. On Windows, npm writes `codex.cmd` into the prefix, so the Makefile default names no file there. The override that the Makefile comment gave, `CODEX=<prefix>/codex.cmd`, has a second defect: `cmd.exe` runs the shim, and it breaks the review prompt, which has four lines.
- Correction: `2551eb3`. `IronAbsolution.Tools/CodexReview/CodexLauncher.cs` starts `node` with the entry script on each platform, and each Codex call goes through it. The Makefile default is `$(shell npm root --global)/@openai/codex/bin/codex.js`. D-47 notes the change.
- Regression check: `CodexLauncherTests` runs a fake `codex.js` through the launch path. `TheVersionComesFromTheEntryScript` and `AMultiLinePromptReachesTheCliUnchanged` fail on the old command, which started the path itself. `TheMakefileGivesTheEntryScriptOfTheNpmPackage` fails on the old Makefile. `make` passes on macOS with 232 tests.
- Windows: the launch path has no branch for a platform. Only `npm root --global` gives a different folder, as npm documents. The owner runs the Windows builds (D-33), and a Windows run of the tests is open to the owner.

New decision ids: D-54 (the start of each review round without a question to the owner). D-47 has a dated note of this correction.

## Round 2

The answer to the second review round, at the effective head `2551eb3`.

### P2-1
- Disposition: no merit
- Evidence: The round asks for a Windows run of the launcher tests and of `make codex-review`. No contract asks for that. The owner answered on 2026-09-27: all development work runs on the Mac (D-55). G-12 and D-32 bind the game on both platforms. They do not bind the tools project, the Makefile targets, or the review. D-33 has a dated note of this scope. The launch defect of round 1 stays fixed in `2551eb3`, and the Mac run of round 2 passed with 232 tests.
- Correction: none to the code. The documents commit of this round records D-55, and G-12, `CLAUDE.md`, `AGENTS.md`, and the review standard of the pr-review skill state the scope. These are documents alone, so the effective head stays `2551eb3`.
- Regression check: none, because the round found no defect. The status of P2-1 that fits the evidence is ``fixed in `2551eb3`.``
