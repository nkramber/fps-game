# The review loop and the merge

The `one-pr-one-session` skill names this file when the PR is ready for review. It holds the author loop of the cross-provider review, the three-strike stop, the merge, and the merge summary (D-14). The `review-response` skill holds the answer to each finding.

## Procedure: the author loop

1. Push the round of changes.
2. Wait until each check of the head is green, with the wait command of the skill.
3. Answer each review thread, and resolve it after the answer (D-52).
4. Run `make codex-review PR=<n>` in the background, and wait for the completion notice (D-14). Ask the owner no question first (D-54).
5. Read the outcome line of the command and its exit code.
6. Do the step that the table below gives for that exit code.

Make installs the newest Codex CLI with npm, then runs the command with `--codex <path>` and `--pr <n>` (D-47). The review uses the ChatGPT login and no API key. The command removes `OPENAI_API_KEY`, `CODEX_API_KEY`, and `CODEX_ACCESS_TOKEN` from each Codex process (D-14).

The command starts Codex in a worktree on the local branch `review/pr-<n>`, which starts at the PR branch on origin (D-48). The author checkout does not change. Codex pushes the review record and its own handoff entry as one metadata commit (D-14). The command removes the branch after a review with no fault.

A review round can take longer than the ten-minute limit of a tool call. Do not poll the round. The command stops the review after 90 minutes, and the stop is a fault that keeps the worktree for a read (D-50). The transcript, the error log, and the last message of each review go to `artifacts/codex-review/` (D-51).

| Exit | Outcome | Next step |
|---|---|---|
| 0 | The review approves the effective head (D-49). | Do the merge below. |
| 10 | `Changes required` or `Blocked`. | Answer the findings with `review-response`, then go to step 1. |
| 11 | The three-strike stop. | Do the three-strike stop below. |
| 3 | A start condition failed: an old CLI, a login that is not ChatGPT, a PR that is not open, no effective head (D-49), an unresolved thread (D-52), or a checkout that is not clean on the PR branch at the origin head. | Correct each condition that the output names, then go to step 4. |
| 1 | A fault: no record, a stale head, no pushed commit, a moved work head, the time limit, a Codex error, or a usage fault (D-40). | Read the transcript, correct the cause, then go to step 4. |

Make exits 2 for a failed target, and it prints the exit code of the command as `Error <code>`. The line `codex-review: <outcome> (exit <code>)` of the command output names the outcome.

The record names the effective head, which skips each documents commit (D-49). A documents commit after an approving round keeps the approval, so no new round is due. A round fails when a commit outside the metadata set arrives during the round, a documents commit included (D-14).

A PR with no commit outside the documents set has no effective head, and the command refuses it (D-49). That PR merges through the `review-override` label alone. The author session adds the label after the last commit outside the metadata set (D-35, D-66, D-76).

## Procedure: the three-strike stop

A P0, P1, or P2 finding that is open in three review rounds stops the fix loop with exit 11 (D-14). The `Open at:` line of each finding holds the count. A P3 finding never blocks, and it never counts.

1. When auto-merge is on, run `gh pr merge <n> --disable-auto`.
2. Stop the fix loop. Make no more commits for the finding.
3. Ask the owner with `AskUserQuestion`.
4. Give the finding, the evidence of the reviewer, the answers of the author, and the options.
5. Record the answer in `docs/reviews/pr-<n>-response.md`.
6. Record a new D-# when the answer sets a rule.

## Procedure: the merge

1. Write the handoff entry of the session, and commit it as a metadata commit.
2. Push the commit, and run the session end gate.
3. Confirm that the record gives `Ready for owner merge` for the effective head.
4. Write the merge summary below.
5. Ask the owner to confirm the merge with `AskUserQuestion`. Ask it as a question, not as a statement.
6. Give two options alone: `Yes, squash-merge it` and `No, not yet`.
7. Stop when the owner picks `No, not yet`. The PR stays open. Do the next step that the owner names.
8. Run `gh pr merge <n> --auto --squash` after the answer `Yes, squash-merge it`. That answer is the explicit authorization of D-5.
9. When the repository has auto-merge off, run `gh pr merge <n> --squash` instead.
10. Never add the `--admin` option. The ruleset bypass is for the owner alone (D-60).
11. Wait on the checks with the wait command of the skill.
12. Run `gh pr view <n> --json state,mergedAt,mergeCommit`.
13. When a job ends with a runner infrastructure annotation, run the failed jobs again, then go to step 11.
14. When the state is `MERGED`, or the owner says `Merged PR #N`, load `merge-prompt.md` and write the prompt.

A PR with the `review-override` label merges the same way, with the owner confirmation (D-35). It needs no review record. The author session adds the label on the standing instruction of the owner (D-76). A commit outside the metadata set after the label needs the label again (D-65).

The ruleset of `main` in `.github/rulesets/main.json` is the machine gate. It requires the checks, `review-gate` included, and resolved conversations, and it permits squash merges alone (D-61, D-64). When every check is green and the state stays `OPEN`, read `gh pr view <n> --json mergeStateStatus,autoMergeRequest`. An unresolved thread or a stale check blocks the merge.

The gitar pass stays out of this loop until the owner answers OQ-16 (D-7).

## The merge summary

Before the owner confirms a merge, write the summary as four questions, each with its answer of a few sentences. Use these questions, in this order:

- **Q: What does this PR change?** A: the change, and the roadmap item that it closes.
- **Q: How does it do it?** A: the method, and the parts of the code or the documents that changed.
- **Q: Is CI green?** A: green or not. Name each red check, and the cause when you know it.
- **Q: What did the Codex review say?** A: the verdict of the record, `Ready for owner merge`, `Blocked`, or `Changes required`, and the effective head that it names.

Put each point that needs the owner as one more question at the end. A PR with the `review-override` label has no review record. Its Codex review answer names the label and the account that added it. The output of `review-gate` gives both (D-65).
