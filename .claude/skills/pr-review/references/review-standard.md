# The review standard

The `pr-review` skill names this file at step 5. It holds the depth of the review and the project contracts that a review reads.

## Principal-engineer review standard

Build an independent account of the behavior before you compare it with the explanation of the author.
For each changed behavior, trace the input, the state transition, the output, the side effects, and the recovery path.
State the invariant that each boundary must keep.

### Correctness and system effects

- Check normal use, boundary values, absent data, invalid data, repeated actions, and interrupted actions where they apply.
- Trace state ownership and lifetime across the tools project, the tests, the game code, and the content.
- Inspect initialization, cancellation, cleanup, and restart when the change affects those paths.
- Check event order, resource disposal, integer bounds, and float edge cases where they affect the result.
- Inspect compatibility with current callers, content, files, and packaged builds.
- Check whether a local fix makes a defect in another consumer of the same contract.
- Verify each exit test against the implementation and the evidence.

Do not expand the review into an unrelated rewrite.
Tell apart the defects that the PR adds, the defects that it exposes, and the independent defects that existed before it.
A defect from before the PR blocks this PR only when it prevents the changed behavior or a required gate.

### Project contracts

Apply each relevant row. Record why an area does not apply when its omission can mislead a reviewer.

| Area | Required examination |
|---|---|
| Tools project | The C# rules of `.claude/skills/csharp-conventions/SKILL.md` (D-44). Each command, its exit codes, and its tests (D-15, D-40). |
| Errors | Required context, visible failure, and no swallowed error. An empty catch or a silent fallback breaks T-2. Unreal asserts follow D-34. |
| Documents | The STE rules, the reference check, and the size limits (D-17). The design doc template (D-19). `CLAUDE.md` and `AGENTS.md` stay identical (D-12). |
| Registers | Each new decision and question has the next id and a date. Each revision of an earlier decision has its mark in the `Effect` column. |
| Input and CI boundaries | Size limits, file paths, and validation at each affected external input. CI permissions, secret access, and untrusted content. Actions pinned by commit SHA, and a time limit on each job (D-42). |
| Credentials and caches | No credentials, account data, generated caches, or machine-specific paths in a commit (G-6, D-9). |
| Unreal code and content | Unreal best practices and the Epic C++ coding standard (G-11, D-34). C++ holds the rules. Data assets and Blueprint subclasses hold tuning and content (D-29). |
| Platforms and budgets | The game keeps working on Windows inside the budget of D-32 (G-12, D-91). The development tools and the review run on the Windows PC (D-92, which supersedes D-55). The session runs the Windows builds and attaches the logs (D-31, D-33). It asks the owner before a command that opens a game window (D-96). |
| Binary assets | Git LFS stores each binary asset (D-30). |
| Originality and licenses | All content is original (G-1, D-2). A third-party asset enters only when its terms allow redistribution (D-24). |
| Dependencies and cost | A decision justifies each dependency. A performance claim has a measurement before and after the change (T-1). No spend without the owner (D-8). |

Do not bring back an earlier contract that a later decision replaces.
For example, D-35 revises D-6 in part: after PR-6, the owner can skip the review of a PR with no code.

### Design, maintainability, and documents

- Confirm one concern per PR and a clear reason for every changed subsystem (G-7).
- Check that helpers go one level deep (T-1).
- Require two concrete cases before an abstraction (T-1).
- Prefer explicit ownership and visible control flow over hidden coupling.
- Explain the concrete maintenance cost of a design objection.
- Do not report personal style preferences as correctness defects.
- Check that the design text, the decisions, the questions, the code, and the exit tests agree.
- Check each roadmap prerequisite against the first gate that needs it.
- Tell apart proposed work, implemented work, measured behavior, and owner approval.
- Verify material external claims against dated primary sources.
- Check each line of the Documents section of the PR description against the diff (D-22). Each reason must be true and specific.
- Confirm that `AGENTS.md` and `CLAUDE.md` stay identical when either one changes (D-12).
- Check the attribution rule in commits, PR text, comments, and deliverables (T-6, D-16).

Documentation and skill PRs need the same provider independence and evidence discipline as code PRs.
For a skill change, examine its trigger, scope, instructions, references, and behavior on a realistic request.
Treat contradictory instructions, and gates that cannot pass, as defects.
