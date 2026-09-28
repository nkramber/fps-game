---
name: csharp-conventions
description: The C# rules of the tools project. Error context, no silent failure, style, test shape, and the commands. Load before you write or review C#.
---

# C# conventions skill

Load this skill before you write or review C# in this repo (D-44). The C# of this repo is the tools project alone (D-15). The game code is Unreal C++, and Unreal best practices govern it (D-34). This skill does not apply to that code.

The skill takes the errors, style, tests, and command rules of the-thing-below, and two rules of what-you-carry (D-44). The determinism rules and the Godot rules of the role models do not transfer (D-12).

## Projects

- `IronAbsolution.Tools` is one command-line program. Each command has its own folder and its own PR (D-15).
- `IronAbsolution.Tests` holds the tests of each command. It references the tools project.
- `Directory.Build.props` holds the target framework and the strict build settings of each project (D-40).
- Each new package reference needs a reason in a comment next to it.

## Errors (T-2)

- No empty `catch`. No `catch` that writes a log and continues with no decision that names it.
- Every exception carries its context: the file path, the line, and the field.
- An absent value is an error, never a default. `GetValueOrDefault` on an input field is a finding.
- Assertions stay on in each configuration. Do not use `Debug.Assert`.
- Nullable reference types are on, and warnings are errors.
- A command writes each fault to its error writer and gives the exit code 1 (D-40).

## Style (T-1)

- Keep each file clean for `dotnet format`. Suppress a warning only with a comment next to the pragma that names the reason.
- Do not put a comment between the arrow of an expression body and its expression. `dotnet format` then writes the line ending of the machine. Give the member a body with braces, and put the comment inside it.
- Explicit over implicit. No interface for a single implementation. Two concrete cases come before an abstraction.
- Helpers go one level deep. A reader understands a method from the method and the signatures of its helpers.
- No clever one-liners. Tune only on measurement (D-44).
- Name a method for what it does. Name a type for what it is. Do not use a name with `Manager`, `Handler`, or `Util`.
- Each public member has a doc comment that states the contract, not the implementation.
- A comment explains a decision or a trap. It cites a D-# id when one applies. It never repeats the code.
- Cite only the ids of this repository. Do not cite a decision id of a role model.

## Tests (T-3)

- The tests use `xunit.v3` as the one test package, under Microsoft.Testing.Platform (D-40). Each type under test has its own test class.
- A bug fix ships with a regression test that fails on the old code. The PR description names the test.
- A test asserts the contract, not a copy of the implementation.
- A test that reads a committed file finds the root through `RepositoryRoot`, so it runs from any folder.
- A test that needs a checkout builds one in a temporary folder and removes it at the end.
- The coverage report shows the lines that no test runs (D-45). No coverage number fails the build.

## Commands

`run.ps1` is the entry point (D-99). These are the raw commands:

```
dotnet build IronAbsolution.slnx
dotnet test --solution IronAbsolution.slnx --no-build
dotnet format IronAbsolution.slnx --verify-no-changes
dotnet run --project IronAbsolution.Tools/IronAbsolution.Tools.csproj -- ste-check --root .
```

Put each option of the test application after `--`, because `dotnet test` reads the options before it (D-40).

`global.json` pins the SDK and sets the test runner (D-40).
