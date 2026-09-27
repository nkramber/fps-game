# The build entry of the repository (D-41). Run each target from the checkout root.

SOLUTION := IronAbsolution.slnx
TOOLS_PROJECT := IronAbsolution.Tools/IronAbsolution.Tools.csproj

# The Codex CLI of the cross-provider review. The codex-review target installs the newest
# release there first (D-47). Set `CODEX` to give another path, as on Windows.
CODEX ?= $(shell npm prefix --global)/bin/codex

.PHONY: verify where hooks build test format ste-check codex-review clean

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
