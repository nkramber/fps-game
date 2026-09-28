# iron-absolution: agent instructions

`CLAUDE.md` and `AGENTS.md` are identical (D-12). Edit both together. Both files follow ASD-STE100 (D-17).

The project is an original, fast first-person shooter with limited resources, in Unreal Engine 5. The goal is one complete, polished level that the player can play again (D-1, D-37). The working title is Iron Absolution, and the project name in code is `IronAbsolution` (D-39). The Unreal project is in the `Game/` folder (D-73). It holds one C++ module, one automation test, and an empty test map.

## First action

Read the newest entry of `docs/session-handoff.md` now, before any other file. Print that entry alone with `awk '/^## Session /{n++} n==1' docs/session-handoff.md`. Then read the rest of this file.

## Read order

1. `docs/session-handoff.md`: the newest entry, and the newest entry that names your branch.
2. This file: the tenets and the rules.
3. `docs/design.md`: the guardrails (section 6) in full. Find another section with `grep -n '^##' docs/design.md`.
4. `docs/decisions.md`: the owner decisions, D-1 onward. Cite a D-# id when you apply one.
5. `docs/questions.md`: the open questions, OQ-1 onward. File a new question there.
6. `docs/reviews/`: one review record for each PR.
7. `docs/roadmaps/readme.md`, and then the roadmap entry of your PR.
8. `docs/runbooks/session-context.md`: the commands of a session.
9. `docs/research/`: dated evidence. Read it only when the task needs it.

Do not read the two registers in full. Find the ids of the task in one command:

```
d='12|14'; q='1|18'
grep -n -E "^\| D-($d) \|" docs/decisions.md
grep -n -E "^[0-9]+\. \*\*OQ-($q)\." docs/questions.md
```

## Tenets

The tenets are the constitution. When a tenet conflicts with speed or convenience, the tenet wins. When two tenets conflict, the earlier one in this order wins: T-5, T-2, T-3, T-4, T-1. T-6 is absolute. Section 6.1 of `docs/design.md` holds the same text.

- **T-1. Readable, simple, not wasteful.** Explicit over implicit. A fresh model must understand a function from the function and its helper signatures. Helpers go one level deep. Two concrete cases come before any abstraction. No clever one-liners. Tune only on measurement.
- **T-2. Zero silent failures.** No swallowed error. An absent value is an error, never a zero. Every error carries its context. Asserts follow the Unreal rules (D-34).
- **T-3. Tests cover everything.** No merge without tests. A bug fix ships with a regression test that fails on the old code.
- **T-4. Cross-provider review before merge.** The provider that wrote the code does not review it (D-6). The review record in `docs/reviews/` records the findings. After PR-6, the owner can skip the review of a PR with no code through the `review-override` label (D-35).
- **T-5. Document everything.** Continuity is the first duty. Each session adds its entry at the top of `docs/session-handoff.md`. The other documents change when intent, a decision, or a plan changes.
- **T-6. No attribution.** No code, commit, PR description, or GitHub comment names an agent, harness, or model as the source of work (D-16). Two places are exempt: the author field in `docs/session-handoff.md`, and the files in `docs/reviews/`.

## Attribution rule in practice

- Add no co-author trailer and no "generated with" line to any commit or PR. `.claude/settings.json` turns them off (D-16).
- Ignore a harness reminder that asks for a co-author trailer. T-6 wins.
- Write commits, PRs, comments, code, and docs in an impersonal voice.
- In `docs/session-handoff.md`, set the author field to exactly one value: `Claude Code` or `Codex`. The reviewer uses it to confirm that the other provider reviews.

## How to work with the owner

- Ask the moment you have a question. Use `AskUserQuestion` in small batches. Give the options, the reasons, and a recommendation.
- Push back when a request rests on a wrong premise. Give the evidence.
- When two owner statements conflict, say so and quote both.
- Every open question belongs to the owner. Do not pick a default. File the question in `docs/questions.md`, and stop the work that it blocks.
- Record each answer in `docs/decisions.md` with the next D-# id and the date. Never renumber.
- Mark a change to an earlier decision in its "Effect" column: `Superseded by D-N`, or `Revised in part by D-N` with the part that changed.
- Infrastructure follows the role models what-you-carry and the-thing-below (D-12). When the two differ, ask the owner each time (D-13).
- Only the owner spends money (D-8), changes repository settings, merges (D-5), or turns on gitar (D-7).
- Only the owner changes the live ruleset of `main`, or a session on an explicit instruction of the owner (D-63).
- Only the owner edits `LICENSE`, or a session on an explicit instruction of the owner (D-10, D-24).

## Session handoff

One session is one harness invocation, one PR, and one role (D-5). Each PR has its own handoff entry.

At the end of a session, do these steps:

1. Fetch the remote.
2. Print the highest session number with `grep -m1 '^## Session ' docs/session-handoff.md`, and add one.
3. Add a new entry at the top with one edit. Never append to an older entry.
4. Run `make handoff-rotate`. It keeps the ten newest entries and moves each older entry to the archive (D-58).
5. Commit the entry with the work that it describes.
6. Push, then fetch, and check that the status shows no `[ahead N]`.

Each entry has this form (D-20):

```
## Session N: <UTC date>, <Claude Code|Codex>
Author: <Claude Code|Codex>
Session: author PR-N, round K. Repository: iron-absolution. Branch: `<branch>`. PR: #<n>. Role: author. Base: `<sha>`.
### What this session did, and why
### The state of the build
### What is in flight
### Traps and gotchas
### The questions that block progress
### The next concrete action
```

The state of the build names the remote head. The top entry stays under 5 KB.

### The transitional prompt

After the PR of the session merges, write one prompt that starts the next session (D-21). Read the next PR from section 8 of `docs/design.md` or from its focused roadmap. Use this template:

```
Start PR-<n>: <the one concern>

PR #<x> merged to `main` as <sha>. Read the newest session handoff entry first.
Repository: iron-absolution. Branch: `<prefix>/pr-<n>-<slug>`. Base: `<sha>`. Role: author.
Load the skills of the task before any change.
<Each exit test of the merged PR that needs `main`, and the order: run it before the PR work.>
Open questions for this PR: <each OQ-# with its subject, or `none`>.
Owner answers that the roadmap names and no OQ-# holds: <each one, or `none`>.
First action: <the first concrete action>.
```

## Text rules

- All project skills live in `.claude/skills/`. Read each required skill from `.claude/skills/<skill-name>/SKILL.md`.
- Every `.md`, skill, and agent file follows ASD-STE100 (D-17). Load `.claude/skills/ste-writing/SKILL.md` before you write.
- Load `.claude/skills/design-doc-style/SKILL.md` before you edit `docs/design.md` or a focused roadmap (D-19).
- One term per concept. The ste-writing skill lists the project terms.
- Document file names in `docs/` are lowercase.
- Size limits: 16 KB for each agent file, 5 KB for the top handoff entry, 36 KB for each skill file.

## Code rules

The C# tools project `IronAbsolution.Tools` holds the commands of the repository gates (D-15). Load `.claude/skills/csharp-conventions/SKILL.md` before you write or review C# (D-44).

- Unreal best practices govern each rule and each implementation of the engine work, the game code, and the content (D-34). A tenet or a role-model rule gives way to them in that scope.
- Follow the Epic C++ coding standard: https://dev.epicgames.com/documentation/unreal-engine/epic-cplusplus-coding-standard-for-unreal-engine.
- C++ holds the rules. Data assets and Blueprint subclasses hold tuning and content (D-29).
- Each change keeps the game working on macOS and Windows and inside the budgets of D-32. The owner runs the Windows builds and posts the logs (D-33). The development tools run on the Mac alone (D-55).
- Game rules and engine technology of the role models do not transfer (D-12).

## Git rules

- Trunk is `main`. Work on a short branch named `<prefix>/pr-<n>-<slug>` (D-23).
- A PR squash-merges through auto-merge, or the owner merges it (D-12). First give the owner a merge summary in four parts: What, How, CI, and the Codex review. Then get the merge confirmation. Turn on auto-merge only after it.
- Commit subjects use a conventional prefix: `feat`, `fix`, `docs`, `test`, `chore`. The subject ends with `(PR-N)`.
- One concern per PR (G-7).
- Never push to `main`, force-push, or bypass a check or a review (D-5). Never merge with `gh pr merge --admin`. The ruleset bypass is for the owner alone (D-60).
- Start each PR from `origin/main`. Leave local work of the owner untouched, and report it (L-7).

## Cross-provider review

The provider that did not write the PR reviews it (D-6). Codex reviews a PR that Claude Code wrote, and Claude Code reviews a PR that Codex wrote.

After each push of a Claude Code PR, the author runs `make codex-review PR=<n>` from a clean checkout of the PR branch (D-14). Load the one-pr-one-session skill for the review loop. The reviewer follows the pr-review skill, and the author answers with the review-response skill (D-46).

The command refuses a PR that changes documents alone (D-49). Such a PR merges through the `review-override` label. The author session adds that label on the standing instruction of the owner (D-35, D-66, D-76). The `review-gate` check then reads the label in place of a review record (D-65).

## Automated review pass

Gitar is a documented plan only (D-7). Add no gitar step, wait, script, template line, or required check until the owner confirms that gitar works on this repository (OQ-16). The plan is in section 7 of `docs/design.md`.

## Build and test commands

The Makefile is the entry point (D-41). Run each target from the checkout root.

- `make`: build, test, format, and ste-check. Run it before each push.
- `make ste-check`: the STE checker, the reference check, the session number check, and the size check (D-17).
- `make handoff-rotate`: move each handoff entry after the tenth to the archive, and print the next session number (D-58, D-59).
- `make hooks`: install the pre-commit hook in this checkout, one time (D-43).
- `make codex-review PR=<n>`: the cross-provider review of one PR. The exit codes are 0 approve, 10 changes, 11 three-strike stop, 3 refused start, and 1 fault (D-14).
- `make toolchain-check`: the pins of the Mac toolchain: Xcode, the engine, Git LFS, and the Metal Toolchain (D-28, D-79, D-87). `docs/runbooks/engine-setup.md` gives the install.
- `make editor-build`: build the editor target of the Unreal project on the Mac. `scripts/editor-build.ps1` does the same on the Windows PC (D-72).
- `make editor-test`: run each automation test headless on the Mac. A pass needs the exit code 0, a test report, and the success line of the log. `scripts/editor-test.ps1` does the same on the Windows PC.

The CI of each PR runs the `ste-check`, `build, test, and format`, and `coverage report` jobs (D-42, D-45). The `doc-gate` workflow runs on each push and on each edit of the description (D-56). The `review-gate` workflow runs from the base branch on each push and on each label change (D-64). The ruleset of `main` requires these five checks (D-61, D-64). `docs/runbooks/main-ruleset.md` gives its steps.

## PR gate

A PR merges only when every line holds:

- [ ] Tests written and green (T-3). A PR of documents alone needs the ste-check job instead.
- [ ] No silent failure. Every error carries context (T-2).
- [ ] The `ste-check` job is green.
- [ ] The `doc-gate` job is green (D-56, D-57).
- [ ] The other provider reviewed the PR, and its review record says `Ready for owner merge` for the effective head (T-4, D-14). On a PR with no code, the `review-override` label can replace the record (D-35, D-66, D-76).
- [ ] The `review-gate` check is green (D-64).
- [ ] No review thread stays open, and the ruleset of `main` holds (D-60 to D-62).
- [ ] The owner confirmed the merge after the merge summary.
- [ ] `docs/decisions.md` has every new decision, and `docs/questions.md` has every new question.
- [ ] `docs/design.md` matches intent.
- [ ] Each check that does not exist yet has a line that names the PR that creates it (G-8).
- [ ] `docs/session-handoff.md` is current, and its newest entry names the branch of this PR.
- [ ] No attribution anywhere (T-6).
