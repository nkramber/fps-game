# The engine targets of the Mac (D-41). Run each target from the checkout root.
#
# `run.ps1` holds each development command now, and it runs on the Windows PC (D-99). PR-14
# removes this file, with each other Mac part (D-97).

TOOLS_PROJECT := IronAbsolution.Tools/IronAbsolution.Tools.csproj
# The Unreal project lives in its own folder, apart from the tools solution (D-73).
UPROJECT := Game/IronAbsolution.uproject

.PHONY: toolchain-check editor-build editor-test package-build package-run

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
