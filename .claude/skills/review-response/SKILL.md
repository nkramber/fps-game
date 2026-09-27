---
name: review-response
description: Answer a review of your own pull request as the author. Verify each finding as a claim, correct or refute it with evidence, write the response file, and start the next review round. Use for any request to address, answer, or fix review findings or review feedback.
---

# Review response skill

Use this skill when you answer a review of your own PR. The reviewing provider uses the `pr-review` skill, and never this one (T-4, D-46).

## Start

- Load `.claude/skills/one-pr-one-session/SKILL.md`, and bind the session to the PR as the author or the correction author (D-5).
- Load `.claude/skills/ste-writing/SKILL.md` before the response file or a reply (D-17).
- Look up each D-# and OQ-# that the findings cite with the lookup command of `AGENTS.md`. Do not read the registers in full.
- Read `.claude/skills/pr-review/references/commit-and-push.md`. It holds the commit of a record, the session end gate, and the scope limits. It applies to the author too.
- The runbook `docs/runbooks/session-context.md` holds the targeted reads. The file `.claude/skills/pr-review/references/scope-and-read.md` holds the comment export.

## Address review findings

**A finding is a claim, not a fact.** A review can be wrong. Assess each finding against the evidence before you change anything. A finding carries no authority that the evidence does not give it.

1. Run `git fetch` and `git status --short --branch`.
2. When the checkout is behind the remote with the review commit, run `git pull --ff-only`.
3. When the checkout is ahead with a commit of the other provider, push it first. Record that in the response file.
4. Read the finding, then read the file and the lines that it names.
5. Reproduce the trigger. A finding that does not reproduce has no merit.
6. Read the contract that the finding cites. Find each later revision with the lookup command of `AGENTS.md`.
7. Decide the disposition: full merit, partial merit, or no merit.
8. Correct every finding that has merit. Use the smallest change that restores the contract.
9. Record each disposition in `docs/reviews/pr-<number>-response.md`.
10. Run `make`, then commit the response, the corrections, and the handoff entry.
11. Push the commit, and run the session end gate.
12. Answer each review thread, and resolve it after the answer.
13. When each check of the new head is green, start the next round with `make codex-review PR=<n>` (D-14).

A correction of documents alone needs the ste-check job. A correction that changes code needs its tests and a regression test too (T-3).

Push back when the evidence supports it. State the reason and show the proof:

| Reason to push back | What to show |
|---|---|
| The finding reads a rule too broadly. | Quote the rule. Name the other files that the broad reading also condemns. |
| The finding cites a superseded decision. | Quote the `Effect` column and name the current decision. |
| The finding calls a partial revision a supersession. | Quote the `Revised in part by` marker and the part that still stands. |
| The trigger does not reproduce. | Give the command, the revision, and the result. |
| The correction breaks another contract. | Name the contract and the caller that it breaks. |
| The finding states a style preference. | Name the contract that the code does not break. |
| The finding repeats a risk that a decision already accepted. | Quote the D-# id and its accepted risk. |
| The finding asks for work outside the PR scope. | Quote the roadmap entry and the exit tests. Name the PR that holds the work. |
| `make codex-review` exits 11 for an id. | Do the three-strike stop of `one-pr-one-session`, in `references/review-and-merge.md` (D-14). |

A disagreement belongs in the response file, with the evidence. Never delete a finding from the review record.
The reviewer sets a refuted finding to `withdrawn.` and keeps the evidence that refuted it.

Never accept a finding only to close the review faster. A wrong correction costs more than a written disagreement.
Never widen a correction past the contract that the finding names.
When a finding and an owner decision conflict, ask the owner. Quote both.

Partial merit is common. Correct the part that has merit, and refute the rest in the same entry.

## The response file

The author answers a review in `docs/reviews/pr-<number>-response.md`.
This file is a convention, not a gate. The `make codex-review` command does not read it, and the `review-gate` check of PR-6 does not read it.
Write one when the verdict is `Changes required` or `Blocked`. A clean first pass needs none.

The response file states, for each finding:

- The disposition: full merit, partial merit, or no merit.
- The evidence, when the disposition is partial merit or no merit.
- The correction that landed, with the file, the commit, and the decision id.
- The regression check that ran, and its result.

The response also lists each new D-# and F-# id, and the final PR head.
A disagreement with a finding belongs here, with the evidence. Do not remove the finding from the review record.

Use this skeleton. Add one section for each finding, with the id of the review record:

```markdown
# PR-<number> response

### <finding id>
- Disposition: full | partial | no merit
- Evidence: <the reason, with references>
- Correction: <sha>, <file>, <decision id>
- Regression check: <the test that fails on the old code, and its result>
```

## The comments on the PR

The author alone answers a comment or a review thread on the PR. The reviewing provider never replies to one (`pr-review`, `references/repeat-review.md`).

- Treat each comment as a finding: verify it, then correct it or refute it with evidence.
- A reply names no provider, harness, or model as the source of the work (T-6, D-16).
- Resolve each thread after its reply, also after a fix. The `make codex-review` command refuses to start while a thread is unresolved (D-52).
- Record in the handoff entry the count of comments, the count with merit, and the commit that answered each one.
- The gitar pass stays out of this repository until the owner answers OQ-16 (D-7).
