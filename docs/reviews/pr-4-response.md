# PR-4 response

The answer of the author to the first review round of PR #4 (PR-3), at the effective head `b0596f5`.

### P2-1
- Disposition: full
- Evidence: The claim reproduces from the package metadata. The `bin` entry of `@openai/codex` is `bin/codex.js`. On macOS, `<npm prefix>/bin/codex` is a link to that script. On Windows, npm writes `codex.cmd` into the prefix, so the Makefile default names no file there. The override that the Makefile comment gave, `CODEX=<prefix>/codex.cmd`, has a second defect: `cmd.exe` runs the shim, and it breaks the review prompt, which has four lines.
- Correction: `2551eb3`. `IronAbsolution.Tools/CodexReview/CodexLauncher.cs` starts `node` with the entry script on each platform, and each Codex call goes through it. The Makefile default is `$(shell npm root --global)/@openai/codex/bin/codex.js`. D-47 notes the change.
- Regression check: `CodexLauncherTests` runs a fake `codex.js` through the launch path. `TheVersionComesFromTheEntryScript` and `AMultiLinePromptReachesTheCliUnchanged` fail on the old command, which started the path itself. `TheMakefileGivesTheEntryScriptOfTheNpmPackage` fails on the old Makefile. `make` passes on macOS with 232 tests.
- Windows: the launch path has no branch for a platform. Only `npm root --global` gives a different folder, as npm documents. The owner runs the Windows builds (D-33), and a Windows run of the tests is open to the owner.

New decision ids: D-54 (the start of each review round without a question to the owner). D-47 has a dated note of this correction.
