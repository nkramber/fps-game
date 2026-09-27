# The build entry of the repository (D-41). Run each target from the checkout root.

SOLUTION := IronAbsolution.slnx
TOOLS_PROJECT := IronAbsolution.Tools/IronAbsolution.Tools.csproj

.PHONY: verify where hooks build test format ste-check clean

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
