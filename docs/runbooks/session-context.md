# Runbook: the commands of a session

Status: procedure, written 2026-09-27. Written in ASD-STE100 (D-17). The source is the runbook of the same name in the role models (D-12).

A session pays for each byte that it reads and for each model call that it makes. This runbook holds the commands that keep both counts low. Later PRs add the commands of the other tools. PR-3 adds the wait and the review. PR-4 adds the rotation and the local run of the documents gate. `docs/runbooks/main-ruleset.md` holds the commands of the ruleset of `main`.

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
make codex-review PR=<number>
```

The one-pr-one-session skill gives the next step for each exit code.

At the end of a session, add the handoff entry, then rotate the handoff before the commit (D-58, D-59). The command prints the next session number.

```
make handoff-rotate
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

Check the pins of the Mac before each engine PR (D-28). The command reads the engine folder from `IRON_ABSOLUTION_ENGINE_DIR` (D-79). `docs/runbooks/engine-setup.md` gives the install and the Windows script.

```
make toolchain-check
```

A shell of the harness does not read `~/.zshrc`, so it does not get the engine variable. Put the export in front of each engine command of the session.

```
export IRON_ABSOLUTION_ENGINE_DIR="/Volumes/IronAbsolution/Epic Games/UE_5.8"
```

## The Unreal project

Work on the engine from the clone on the `IronAbsolution` volume (D-81). Correction of 2026-09-28: the engine work moves to the Windows PC (D-92), and PR-14 removes the volume (D-97). Build the editor target, then run the automation tests headless.

```
make editor-build
make editor-test
```

- `make editor-build` writes the log of the build tool to `Game/Saved/Logs/editor-build.log`.
- `make editor-test` writes the log of the editor to `Game/Saved/Logs/editor-test.log`, and the test report to `Game/Saved/Automation/editor-test/index.json`.
- The evidence form of the PR takes both logs (D-31, D-80).
- The Windows PC runs `scripts/editor-build.ps1` and `scripts/editor-test.ps1` (D-72).

Make the packaged Development build, then start the package for its timed run (D-89).

```
make package-build
make package-run
```

- `make package-build` puts the app in `Game/Saved/Packages/Mac`, and it writes its output to `Game/Saved/Logs/package-build.log`.
- `make package-run` runs the test map for 10 seconds, and it writes the stdout of the package to `Game/Saved/Logs/package-run.log`.
- The Windows PC runs `scripts/package-build.ps1` and `scripts/package-run.ps1` (D-72).
