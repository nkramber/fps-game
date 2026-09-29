# Runbook: the commands of a session

Status: procedure, written 2026-09-27. Written in ASD-STE100 (D-17). The source is the runbook of the same name in the role models (D-12).

A session pays for each byte that it reads and for each model call that it makes. This runbook holds the commands that keep both counts low. Later PRs add the commands of the other tools. PR-3 adds the wait and the review. PR-4 adds the rotation and the local run of the documents gate. `docs/runbooks/main-ruleset.md` holds the commands of the ruleset of `main`.

A session runs on the Windows PC (D-92). `run.ps1` is the entry of each development command (D-99). Run it from any folder of the checkout, because it finds the root from its own folder.

```
.\run.ps1 help
```

## The owner steps

Only the owner installs a program on the Windows PC. A session that finds one of these absent asks the owner, and it does not install it. `run.ps1` names the absent program and the step (T-2).

| Step | Command | Used by |
|---|---|---|
| The .NET SDK of `global.json` | `winget install Microsoft.DotNet.SDK.10` | Each target of the tools project |
| PowerShell 7 (D-101) | `winget install Microsoft.PowerShell` | `run.ps1` and each test of it |
| Node.js with npm | `winget install OpenJS.NodeJS.LTS` | The Codex CLI of `codex-review` (D-47) |
| Git for Windows with Git LFS | `winget install Git.Git` | Each git command, and the binary content (D-30) |
| The GitHub CLI, then `gh auth login` | `winget install GitHub.cli` | `where`, the check wait, and each PR command |
| The Codex login | `codex login` | The cross-provider review (D-14, D-53) |

The `codex-review` target installs the Codex CLI itself with npm on each run (D-47). The login is the owner step.

The pre-commit hook needs the hook path of this checkout. Run it one time (D-43).

```
.\run.ps1 hooks
```

## Targeted reads

Read the newest handoff entry first, and read that entry alone.

```
awk '/^## Session /{n++} n==1' docs/session-handoff.md
```

Read the newest entry that names your branch, when your branch is not new.

```
awk -v b='<branch>' '/^## Session /{n++} n>0 && $0 ~ b {print n; exit}' docs/session-handoff.md
```

Never read `docs/decisions.md` or `docs/questions.md` in full. Look up the ids of the task in one command. Replace the example numbers with every D-# and OQ-# number of the task.

```
d='12|14'; q='1|18'
grep -n -E "^\| D-($d) \|" docs/decisions.md
grep -n -E "\bD-($d)\b" docs/decisions.md | grep -E 'Revis|Supersed' | cut -c1-160
grep -n -E "^[0-9]+\. \*\*OQ-($q)\." docs/questions.md
```

The second line finds each revision of those ids. A `Superseded by D-N` mark replaces the whole answer. A `Revised in part by D-N` mark changes one part, and the rest of that decision stays current.

Find a section of the design doc, and read that section alone.

```
grep -n '^##' docs/design.md
sed -n '<start>,<end>p' docs/design.md
```

## The start and the end of a session

Check the branch and its base at the start.

```
git fetch origin && git status -sb && git log --oneline -1 origin/main
```

Check at the end that the remote holds each commit. The status must show no `[ahead N]`.

```
git push && git fetch origin && git status -sb
```

## The wait and the review

Wait for the checks of a PR with one command after each push. Read the result one time.

```
gh pr checks <number> --watch --interval 60 > /dev/null 2>&1; gh pr checks <number>
```

Start the cross-provider review of a PR from a clean checkout of its branch (D-14). Run it in the background, and read the line `codex-review: <outcome> (exit <code>)`.

```
run.ps1 codex-review -PR <number>
```

The one-pr-one-session skill gives the next step for each exit code.

At the end of a session, add the handoff entry, then rotate the handoff before the commit (D-58, D-59). The command prints the next session number.

```
run.ps1 handoff-rotate
```

Check the Documents section before the push with a local run of the documents gate (D-57). Write the PR description to a file first. The job in CI runs the same command.

```
dotnet run --project IronAbsolution.Tools/IronAbsolution.Tools.csproj -- doc-gate --root . --base origin/main --head HEAD --body <file> --title "<title>" --branch <branch>
```

Read the result of the review gate before the merge summary with a local run (D-64). Write the labels file first, in the form that the workflow writes. The job in CI runs the same command from the tool of `main`.

```
labels=$(mktemp "${TMPDIR:-/tmp}/review-gate-labels.XXXXXX")
printf '{ "labels": [], "overrideLabelEvents": [] }\n' > "$labels"
dotnet run --project IronAbsolution.Tools/IronAbsolution.Tools.csproj -- review-gate --root . --base origin/main --head "$(git rev-parse HEAD)" --pr <number> --labels "$labels"
```

## The toolchain of the engine

Check the pins of the Windows PC before each engine PR (D-28, D-74). The command reads the engine folder from `IRON_ABSOLUTION_ENGINE_DIR` (D-79). `docs/runbooks/engine-setup.md` gives the install.

```
.\run.ps1 toolchain-check
```

The variable is a variable of the user, so a shell of the harness gets it. A shell that was open before the owner set it does not.

## The Unreal project

Build the editor target, then run the automation tests headless. Neither command opens a window.

```
.\run.ps1 editor-build
.\run.ps1 editor-test
```

- `editor-build` writes the log of the build tool to `Game/Saved/Logs/editor-build.log`.
- `editor-test` writes the log of the editor to `Game/Saved/Logs/editor-test.log`, and the test report to `Game/Saved/Automation/editor-test/index.json`.
- The evidence form of the PR takes both logs (D-31, D-80).

The script `Game/Scripts/build_content.py` makes the input actions, the mapping context, the tuning, the player Blueprints, and the gym (D-134). Run it after a change of the script, with no window. Build the editor target first, because the script reads the C++ classes.

```
.un.ps1 content-build
```

- `content-build` writes the log of the editor to `Game/Saved/Logs/content-build.log`. The last line of the script says `build_content: pass.`
- Commit each changed asset with the change of the script. The script is the source of each asset that it makes, so a change in the editor alone goes away at the next run.

Make the packaged Development build, then start the package for its timed run (D-89). The package opens a game window, so ask the owner before `package-run`, and wait for the confirmation (D-96).

```
.\run.ps1 package-build
.\run.ps1 package-run
```

- `package-build` puts the package in `Game/Saved/Packages/Windows`, and it writes its output to `Game/Saved/Logs/package-build.log`.
- `package-run` runs the test map for 10 seconds. The game writes its log to `Game/Saved/Logs/package-run.log`.
