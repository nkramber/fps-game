# The review record

The `pr-review` skill names this file at step 8. It holds the record skeleton, the verdicts, the corrections that a reviewer makes to the PR description, and the review gate check. It replaces the format in `docs/reviews/readme.md`.

## Review record

Use one file for each PR in `docs/reviews/` (T-4). Keep its file name and its finding ids on a repeat review.
For a new record, use `docs/reviews/pr-<number>.md` with the actual GitHub PR number, not the roadmap id.
Write provider names only in the review record and in the handoff author field (T-6, D-16).
Keep those names out of each PR description and each GitHub comment.

The `run.ps1 codex-review` command reads the review record (D-14). The `review-gate` check reads the same parts (D-64). Four parts of the record have a machine reader. Keep their format exact:

| Part | Exact form | Rule |
|---|---|---|
| The file name | `docs/reviews/pr-<number>.md` | The number is the GitHub PR number, not the roadmap id. |
| The head field | `- Head: ` and the hash in backticks, in the Identity list | The hash is the effective head. A short hash is permitted. |
| The findings | The `## Findings` section, with the finding format of `references/findings.md` | Each `###` heading in the section is a finding heading. |
| The verdict | One of the three verdict names, in the `## Verdict` section | Start the first non-empty line of the section with the name in bold and a period, as the skeleton shows. |

The reader reads the verdict from that first line alone, outside each fenced block. Do not reword the name.

The effective head is the newest commit that changes a path outside the documents set (D-49).
The documents set is `docs/`, `.claude/skills/`, `CLAUDE.md`, `AGENTS.md`, `README.md`, and `LICENSE`.
A commit that changes only those paths does not change the effective head. A later documents commit keeps the approval.
Read the effective head with this command, and take the first line:

```
git log --format='%H %s' origin/main..HEAD -- . ':!docs/' ':!.claude/skills/' ':!CLAUDE.md' ':!AGENTS.md' ':!README.md' ':!LICENSE'
```

The metadata set is `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md` (D-14).
The review commit holds the review record and the handoff entry, so it is always a metadata commit.
The work head is the newest commit outside the metadata set. The command checks that the work head does not move during a round.
Record the effective head, not the tip.

A PR with no commit outside the documents set has no effective head, and the command refuses it (D-49). The author session adds the `review-override` label to such a PR, and no record approves it (D-35, D-66, D-76).

Use this skeleton. Keep the heading text and the order.

```markdown
# PR-<number> review

Date: <YYYY-MM-DD>

## Identity

- PR: <number>
- Target: `main`
- Base: `<sha>`
- Merge base: `<sha>`
- Head: `<effective head sha>`
- Branch: `<branch>`

## Provider gate

State the author provider, the source of that fact, and the reviewer provider.
State the result of the gate against T-4 and D-6.

## Intended behavior and scope

State the intent, what the review inspected, and every affected contract.
Name each area that the review did not inspect.

## Findings

One subsection for each finding, in severity order. Use the finding format of references/findings.md.
Write "No finding." when the review found none.

## Out of scope

One line for each concern that a later PR holds. Name that PR or roadmap item.
Give no severity here. Write "None." when the review found none.

## PR comments

One line for each comment thread on the PR: the claim, the answer of the author, and what the review verified.
Write "None." when the PR has no comment.

## Description edits

One line for each correction that this review made to the PR description.
Give the old value and the new one. Write "None." when the review changed nothing.

## Verification

One line for each command or check, with its result.
Name each check that did not run, and the reason.
End with the push line: `- Push: <sha> is the head of origin/<branch>, verified with gh pr view.`

## Open questions and accepted risks

Name each open OQ-# and each accepted risk with its D-# id.

## Earlier verdicts

The verdict of each earlier round, with its head. Write "None." on the first round.

## Verdict

**<Blocked | Changes required | Ready for owner merge>.** This verdict applies to head `<sha>`.
Give the reason in one or two sentences.
```

The `## Earlier verdicts` section comes before the `## Verdict` section. A heading in that section must not be `## Verdict`, because the reader takes the first section with that exact heading. The reason after the verdict names no other verdict name, because a section that names two verdicts fails.

## Correct the PR description

The PR description follows the template of D-22, and the owner reads it at the merge. A description that names a stale head, an old count, or a replaced correction misleads the owner.

The reviewer corrects such a description directly. It needs no finding, and the author needs no extra pass for it.

The reviewer changes only a fact that the review verified:

- the effective head, the base, or the merge base.
- a count that the review ran, such as the test total or the finding total.
- a check result that the review read.
- a sentence that names a correction that a later commit replaced.

The reviewer never changes:

- what the author says the PR does, or why.
- a decision, a tradeoff, or a recommendation.
- a gate line that the owner ticks.

Name no provider, agent, harness, or model in the description (T-6, D-16).

Write one line for each edit in the review record, under `## Description edits`. Give the old value and the new one. The owner then reads every change in one place.

A claim that is wrong in substance stays a finding. The reviewer corrects a stale fact, and the author corrects a wrong statement.

## Verdicts

| Verdict | Required condition |
|---|---|
| Blocked | Provider independence, the review target, a necessary owner decision, or required evidence stays open. Record the verified defects too. |
| Changes required | The eligible review found defects or contract violations in scope that need correction. List the required changes. |
| Ready for owner merge | The provider gate passes, the review covers the complete scope, all required checks pass, and no blocking finding stays open. |

A line under `## Out of scope` never gives the verdict `Changes required`.
A record that gives `Ready for owner merge` with an open P0 to P2 finding fails the round with exit 1. An owner disposition sets the status of such a finding to `accepted risk, D-<n>.`.

No findings does not mean no risk. State the material limits without a claim of zero regressions.
Approval applies only to the recorded revision. A new base or head needs an assessment of the changed scope and evidence.
An approving record lets the author turn on auto-merge after the owner confirms the merge (D-12). The owner can also merge the PR (D-5).

When the review record goes into the PR, keep the assessed implementation head in that file.
Check each later documents commit before the final verdict.
Do not require the review record to contain its own commit hash.
A documents commit cannot hide code, content, configuration, or test changes.

## The review gate check

The `review-gate` workflow runs the tool of the base branch on each push and on each label change (D-64). The tool reads the record at the PR head as data. Without the `review-override` label, the check applies three rules:

1. `docs/reviews/pr-<number>.md` exists for the PR number.
2. The verdict is `Ready for owner merge`.
3. The head in the Identity list is the effective head.

With the label, the check applies the label rules of D-65 in place of these three. The check has two states. Read the color before you start:

| Color | Meaning | What to do |
|---|---|---|
| Red | No review record exists, or the record does not approve this head. | Write the record, or read the findings. The author corrects them. |
| Green | An approved review covers the effective head, or the `review-override` label covers a PR with no code. | Auto-merge merges after the owner confirms, or the owner merges. |

Rule 3 fails when the author pushes code after the approval. That result is correct.
Assess the new diff again, then update the head field and the verdict together.
Rule 3 does not fail when the later commits change only the documents set (D-49).
