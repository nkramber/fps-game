# The build entry of the repository (D-41). Run each target from the checkout root.

SOLUTION := IronAbsolution.slnx
TOOLS_PROJECT := IronAbsolution.Tools/IronAbsolution.Tools.csproj
# The Unreal project lives in its own folder, apart from the tools solution (D-73).
UPROJECT := Game/IronAbsolution.uproject

# The entry script of the Codex CLI of the cross-provider review. The codex-review target
# installs the newest release first, and the command starts the script with `node` (D-47).
# `npm root` gives the global package folder on macOS and on Windows. The `bin/codex` link of
# macOS is absent on Windows, where npm writes a `codex.cmd` shim.
CODEX ?= $(shell npm root --global)/@openai/codex/bin/codex.js

.PHONY: verify where hooks build test format ste-check handoff-rotate codex-review toolchain-check editor-build editor-test package-build package-run clean

## verify: every check that this machine can run.
verify: build test format ste-check

## build: build every project of the solution.
build:
	dotnet build $(SOLUTION)

## test: run every test of the solution. Run the `build` target first.
test:
	dotnet test --solution $(SOLUTION) --no-build

## format: fail when a file needs a format change (D-40).
format:
	dotnet format $(SOLUTION) --verify-no-changes

## ste-check: the STE checker, the reference check, and the session number check (D-17).
#
# The command reads every live document that git tracks. The four dated records stay out of
# the writing rules, and the command holds their paths itself. The run builds the Tools
# project itself, as the pre-commit hook does, so the target runs alone too.
ste-check:
	dotnet run --project $(TOOLS_PROJECT) -- ste-check --root .

## handoff-rotate: keep the 10 newest handoff entries, and move each older entry to the archive (D-58, D-59).
#
# Run it after you add the handoff entry, and before the commit. The command also puts an
# entry that sits under an older one back in its place, and it names that entry. It prints the
# next session number. The doc-gate command has no target. The doc-gate workflow runs it on
# each PR (D-56). The review-gate command has no target either. The review-gate workflow runs
# it from the base branch (D-64).
handoff-rotate:
	dotnet run --project $(TOOLS_PROJECT) -- handoff-rotate --root .

## codex-review: the cross-provider review of one PR through the Codex CLI (D-14, D-47).
#
# `PR=<n>` names the GitHub number of the PR. The target installs the newest CLI with npm, then
# the command checks the start conditions, runs the review in a worktree, and reads the verdict
# from the review record on origin. The command gives 0 for an approval, 10 for changes, 11 for
# the three-strike stop, 3 for a refused start, and 1 for a fault. Make gives 2 for each code
# other than 0, so read the line `codex-review: <outcome> (exit <code>)` of the output.
codex-review:
	@test -n "$(PR)" || { echo "codex-review: set PR=<number>, such as make codex-review PR=4 (T-2)." >&2; exit 1; }
	npm install --global @openai/codex@latest
	dotnet run --project $(TOOLS_PROJECT) -- codex-review --root . --pr $(PR) --codex "$(CODEX)"

## toolchain-check: the pins of the Mac toolchain: Xcode, the engine, Git LFS, and Metal (D-28, D-79, D-87).
#
# The command reads the engine folder from IRON_ABSOLUTION_ENGINE_DIR, so no commit holds a
# path of one machine (D-9). It prints one line for each pin, with the expected value and the
# found value, and it fails on each pin that does not hold. `verify` does not run it, because
# the hosted runners have no engine (D-31). `docs/runbooks/engine-setup.md` gives the install.
# The Windows PC runs `scripts/toolchain-check.ps1` instead (D-72).
toolchain-check:
	dotnet run --project $(TOOLS_PROJECT) -- toolchain-check

## editor-build: build the editor target of the Unreal project on the Mac (D-41, D-73).
#
# The engine folder comes from IRON_ABSOLUTION_ENGINE_DIR (D-79). The build tool writes its log
# to Game/Saved/Logs/editor-build.log, and the evidence form of the PR takes that log (D-31).
# Build.sh of the engine gives 0 for the result "up to date", and so does this target. The
# Windows PC runs `scripts/editor-build.ps1` instead (D-72).
editor-build:
	@test -n "$$IRON_ABSOLUTION_ENGINE_DIR" || { echo "editor-build: set IRON_ABSOLUTION_ENGINE_DIR to the folder that holds Engine (D-79)." >&2; exit 1; }
	"$$IRON_ABSOLUTION_ENGINE_DIR/Engine/Build/BatchFiles/Mac/Build.sh" IronAbsolutionEditor Mac Development -Project="$(CURDIR)/$(UPROJECT)" -WaitMutex -Log="$(CURDIR)/Game/Saved/Logs/editor-build.log"

## editor-test: run each automation test of the project headless on the Mac (D-71).
#
# The command starts the editor with no window, and a pass needs three facts: the exit code 0,
# a test report with no failed test, and the success line of the log. An exit code of 0 alone
# is not a pass. Run the `editor-build` target first. The Windows PC runs
# `scripts/editor-test.ps1` instead (D-72).
editor-test:
	dotnet run --project $(TOOLS_PROJECT) -- editor-test --root .

## package-build: make a packaged Development build of the game on the Mac (D-72, D-89).
#
# The engine folder comes from IRON_ABSOLUTION_ENGINE_DIR (D-79). RunUAT builds the game target,
# cooks the test map, and puts the app in Game/Saved/Packages/Mac. The `-package` step puts the
# libraries and the content in the app. Without it, the app of the archive does not start (F-26).
# The target first removes the last package, so a failed build leaves no old package for
# `package-run`. The output goes to Game/Saved/Logs/package-build.log too, and the evidence form
# of the PR takes that log (D-31). `pipefail` keeps the exit code of RunUAT through `tee`.
# The Windows PC runs `scripts/package-build.ps1` instead (D-72).
package-build:
	@test -n "$$IRON_ABSOLUTION_ENGINE_DIR" || { echo "package-build: set IRON_ABSOLUTION_ENGINE_DIR to the folder that holds Engine (D-79)." >&2; exit 1; }
	rm -rf "$(CURDIR)/Game/Saved/Packages/Mac"
	mkdir -p "$(CURDIR)/Game/Saved/Logs"
	set -o pipefail; "$$IRON_ABSOLUTION_ENGINE_DIR/Engine/Build/BatchFiles/RunUAT.sh" BuildCookRun -project="$(CURDIR)/$(UPROJECT)" -platform=Mac -clientconfig=Development -build -cook -stage -package -pak -archive -archivedirectory="$(CURDIR)/Game/Saved/Packages" -unattended -utf8output -nop4 2>&1 | tee "$(CURDIR)/Game/Saved/Logs/package-build.log"

## package-run: the start command of the Mac package (D-89).
#
# The command starts the package of `package-build` with the timed-run option. The package runs
# the test map for 10 seconds, writes the success line, and stops. A pass needs the exit code 0
# and the success line in Game/Saved/Logs/package-run.log. The command stops a package that runs
# for 5 minutes, and each failure names the log (T-2). The Windows PC runs
# `scripts/package-run.ps1` instead (D-72).
package-run:
	dotnet run --project $(TOOLS_PROJECT) -- package-run --root .

## where: the branch, the tree, and the PR state.
where:
	git status --short --branch
	gh pr status

## hooks: install the pre-commit hook in this checkout (D-43).
hooks:
	git config core.hooksPath .githooks
	@echo "hooks: the hook path is .githooks."

## clean: remove the build output of every project.
clean:
	dotnet clean $(SOLUTION)
