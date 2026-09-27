# The scope of a review, and the read of the PR

The `pr-review` skill names this file at steps 2 to 4. It holds what a review reads, and the boundary of what a review judges.

## Find the review scope

- Follow the read order in `AGENTS.md`. `CLAUDE.md` holds the same text (D-12).
- Load `.claude/skills/one-pr-one-session/SKILL.md`, and bind the session to this PR in the reviewer role (D-5).
- Load `.claude/skills/ste-writing/SKILL.md` before any review text (D-17).
- Read the PR request, its exit tests, the earlier review, and the roadmap entry of the PR.
- Look up each cited D-# and OQ-# with the lookup command of `docs/runbooks/session-context.md`. Do not read the registers in full.
- Read the `Effect` column of each decision. `Superseded by D-N` replaces the whole answer.
- `Revised in part by D-N` changes only the named part, and the rest of that decision stays current.
- Check `docs/questions.md` for open questions that affect this change.
- Read every existing comment on the PR. Take each one into the review as a claim to verify, and never as a finding of your own.
- Export the comments with the command below, and read the output one time.
- Record the PR number, the target branch, the base commit, the merge base, and the head commit.
- Verify that the local checkout and the diff match those commits.
- Keep unrelated local edits. Use an isolated checkout when necessary.
- Inspect the complete diff: deleted files, renamed files, configuration, content, and tests.
- Read a large diff in stages, with the commands below. Read every changed path before the verdict.
- Read each changed file in context. Follow the affected callers and consumers past the diff.
- Continue through the scope after the first finding. Record each area that you did not inspect.

The PR description states intent. The diff and the verified behavior show what the PR does.
Label a review of an uncommitted patch as provisional. It cannot satisfy a review gate, because it names no PR revision.
When the base or the head changes, assess the new diff and the affected evidence before a final verdict.

## The comment export

A PR carries two kinds of comment: the issue comments and the review threads. Export both in one step, and then read the output one time. Replace `<number>` with the PR number.

```
n=<number>
gh pr view "$n" --comments
gh api graphql -F owner=nkramber -F name=iron-absolution -F number="$n" -f query='
  query($owner: String!, $name: String!, $number: Int!) {
    repository(owner: $owner, name: $name) {
      pullRequest(number: $number) {
        reviewThreads(first: 100) {
          nodes { id isResolved path line
            comments(first: 20) { nodes { author { login } body } } }
        }
      }
    }
  }'
```

A reviewer writes no comment. It records each thread under `## PR comments` in the review record.

## The staged read of a diff

A large diff costs more than the review needs in one read. Read it in three stages, and stop at the stage that answers the question.

```
git fetch origin
git diff --stat origin/main...HEAD
git diff --name-status origin/main...HEAD
git diff origin/main...HEAD -- <path>
```

The first command gives the size and the shape. The second command names each added, deleted, renamed, and modified path. The third command reads one path in full.

## Stay inside the pull request

A review judges the change in front of it. It does not design the next one.

Read the roadmap entry for this PR and its exit tests before the first finding. Those two texts set the boundary. This section limits the reach of a review. It never lowers the standard for the code that the PR changes.

A concern is in scope when one of these holds:

- The changed code gives a wrong result under a supported condition.
- The change breaks a caller, a file, or a build that exists today.
- A stated exit test of this PR does not hold.
- A guardrail that the PR names does not hold for the code that this PR adds.

A concern belongs to a later PR when one of these holds:

- It asks a tool that this PR creates to cover a surface that no exit test names.
- It asks for behavior that the roadmap gives to a later PR.
- It repeats a class of defect that this PR corrected, in a surface that this PR does not touch.
- It needs an owner decision about scope, and not a correction.

Write the second kind under `## Out of scope` in the review record. Name the PR or the roadmap item that holds it. Give it no severity. A line in that section never blocks the merge.

A PR that creates a check must pass that check. A new check does not cover the whole repository on the first day. A gap in a new tool is a defect of this PR only when a stated exit test names the missing case.
