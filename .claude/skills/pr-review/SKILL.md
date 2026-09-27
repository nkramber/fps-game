---
name: pr-review
description: Review a pull request at principal-engineer depth as the reviewing provider. Require the opposite provider, precise evidence, regression checks, and a revision-specific verdict. Use for PR reviews and repeat reviews after fixes. The author answers a review with the review-response skill.
---

# PR review skill

Review the change as the engineer accountable for its effect on the whole system.
Judge correctness, contracts, failure recovery, test quality, and future maintenance.
Apply this standard to code, content, tools, CI, skills, and document PRs.
The author answers a review with the `review-response` skill (D-46).
A green test suite or a persuasive PR description does not establish correctness.

This file holds the procedure. The reference files hold the detail of each step. Load a reference file at the step that needs it, and not before.

| Step | Reference file | What it holds |
|---|---|---|
| 1 | `references/provider-gate.md` | The provider that can review, and the evidence of authorship |
| 2 to 4 | `references/scope-and-read.md` | The read of the PR, and the boundary of a review |
| 5 | `references/review-standard.md` | The depth of the review, and the project contracts |
| 6 | `references/verification.md` | The checks that run, and the evidence of each one |
| 7 | `references/findings.md` | The test for a finding, the severity, and the finding format |
| 8 | `references/review-record.md` | The record skeleton, the verdicts, and the review gate check |
| 9 | `references/commit-and-push.md` | The commit, the push, the session end gate, and the scope limits |
| A repeat pass | `references/repeat-review.md` | The second and later passes, and the PR comments |

## Procedure

1. Run the provider gate. Stop with `Blocked` when the providers match or the evidence conflicts.
2. Find the scope: the read order of `AGENTS.md`, the roadmap entry, and the exit tests.
3. Find each register id of the PR, and read every PR comment.
4. Read the complete diff in stages, and read each changed file in context.
5. Build an independent account of the behavior. Read each project contract that the change touches.
6. Run the checks that can falsify the changed behavior. Record the evidence of each one.
7. Write each finding with its trigger, its contract, its evidence, and its regression check.
8. Write `docs/reviews/pr-<number>.md` with the effective head and one verdict.
9. Commit the record with your handoff entry, push the commit, and run the session end gate.

Steps 2 to 4 use the targeted reads of `docs/runbooks/session-context.md`. The file `references/scope-and-read.md` holds the comment export and the staged read of a diff.

## The three verdicts

A review ends with one of three verdicts: `Blocked`, `Changes required`, or `Ready for owner merge`. The file `references/review-record.md` gives the condition of each one.

Write one verdict name in the `## Verdict` section, exactly as that file spells it. The `make codex-review` command reads that section, and it fails a section that names two verdicts. PR-6 adds the `review-gate` check, which reads the same section. A line under `## Out of scope` never gives the verdict `Changes required`.

An approving record lets the owner merge the PR (D-5). After PR-6, the author can also turn on auto-merge. Approval applies to the recorded revision alone.

## A review that `make codex-review` starts

The author session starts the review with `make codex-review PR=<n>` (D-14). Make installs the newest Codex CLI with npm, then runs the command with `--codex <path>` and `--pr <n>` (D-47). The Codex process is a session of its own, in the reviewer role (D-5). The procedure, the depth, and the record format do not change.

- The checkout is a worktree on the local branch `review/pr-<n>`, which starts at the PR branch on origin (D-48).
- Commit the review record and your own handoff entry as one metadata commit (D-14). Set the handoff author field to `Codex`.
- The pre-commit hook runs ste-check on that commit (D-43). Correct each finding, then commit again.
- Push the commit with `git push origin HEAD:<branch>` (D-48). The branch is the head branch of the PR.
- The local branch has no upstream, so the status line shows no `[ahead N]`. The session end gate compares `git rev-parse HEAD` with the head from `gh pr view`.
- Give each finding its `Open at:` line (`references/findings.md`). The command counts the heads for the three-strike stop (D-14).
- No owner answers during the run. A question that blocks the review gives the verdict `Blocked`, and the record names the question. The author asks the owner.
- Push no change outside the metadata set: `docs/reviews/`, `docs/session-handoff.md`, and `docs/session-handoff-archive.md`.
- The command fails the round when the work head moves during the round, also for a change of documents alone (D-14).
- The command removes `OPENAI_API_KEY`, `CODEX_API_KEY`, and `CODEX_ACCESS_TOKEN` from each Codex process. The review uses the ChatGPT login (D-14, D-53).
- The command stops the review after 90 minutes, and the stop is a fault (D-50). The transcripts go to `artifacts/codex-review/` (D-51).

The command refuses a PR with no commit outside the documents set (D-49). Until PR-6, the owner starts Codex by hand for such a PR. The prompt is in `AGENTS.md`, under "Cross-provider review". After PR-6, the owner can add the `review-override` label to a PR with no code instead (D-35).

## Rules that hold at every step

- A concern is in scope when the changed code breaks a contract that this PR names, or when a stated exit test fails.
- A concern with neither goes under `## Out of scope`, with no severity.
- Continue through the scope after the first finding. Record each area that you did not inspect.
- Do not invent findings to meet a quota. A thorough review can produce no actionable findings.
- Never write a comment on the PR, and never resolve a thread. Take each existing comment into the review as a claim to verify.
- The gitar pass stays out of this repository until the owner answers OQ-16 (D-7).
- Name no provider, agent, harness, or model in the PR description or in a GitHub comment (T-6, D-16).
- The review record and the handoff author field are the two exempt places (T-6).
- When two owner statements conflict, quote both. File the question in `docs/questions.md`, and stop the work that it blocks.
- Load `.claude/skills/one-pr-one-session/SKILL.md`, and bind the session in the reviewer role (D-5).
- Load `.claude/skills/ste-writing/SKILL.md` before any review text (D-17).
