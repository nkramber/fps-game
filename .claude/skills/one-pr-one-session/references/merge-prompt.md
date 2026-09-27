# The transitional prompt of the merge

A session ends at the hand-over point (D-5). The next PR starts in a new clean session, and that session holds none of this context. This file holds the prompt that carries the work across the gap (D-21).

## The trigger

The bound PR merges in one of three ways:

- The session runs the squash merge after the owner confirms it (D-5).
- The owner merges it and says `Merged PR #x`.
- After PR-6, GitHub auto-merge merges it.

The session reads the state `MERGED` from `gh pr view`. The session then writes one transitional prompt, and it does no other work. The owner asks for no prompt.

Write the prompt for the bound PR of the session alone. A merge message for another PR gets the blocked result of the start gate. The merge of the bound PR is the one exception to step 2 of the start gate.

The prompt names no provider, harness, or model as the source of work (T-6, D-16).

## Procedure: write the prompt

1. Run the session end gate first.
2. Get the merge commit: `git fetch origin && git log --oneline -1 origin/main`.
3. Read section 8 of `docs/design.md`, or the focused roadmap, and name the next PR.
4. The owner can name a different PR.
5. Read the questions register, and name each open question of the next PR.
6. Read the scope and the exit tests of the next roadmap entry.
7. Name each owner answer that the scope or an exit test needs.
8. Name each exit test of the merged PR that needs a run on `main`.
9. Write the block below in the last message, and stop.

Step 7 finds an owner answer that no OQ-# holds. A read of the questions register alone misses it. For example, a roadmap entry can leave a name or a threshold to the session of that PR.

## The block

The prompt is one fenced block, and the owner pastes it into the next clean session. The block copies the template of `AGENTS.md`. When the two differ, `AGENTS.md` wins.

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

The skills of the task include `one-pr-one-session` for all PR work.

## Rules

- The prompt holds one PR of work. A second PR needs a second session, and a second prompt.
- The prompt never asks the new session to record the merge of the old PR. Git holds the merge (D-5).
- An exit test that needs `main` comes before the PR work, because the new PR branch cannot give that result.
- When the merged PR has no exit test for `main`, remove that line.
- The session handoff entry holds the next ids, and the prompt does not copy them.
- The handoff entry of the ending session holds the same next concrete action. The two agree, or the entry wins.
- The session makes no branch and no change for the next PR (D-5).
