# The commit and the push of a record

The `pr-review` skill names this file at step 9. The `review-response` skill names it too, because the author commits a response file the same way.

## Commit the record

Always commit the review record and the session handoff entry, then push them to the PR branch (D-14). Do it in the session that writes them.

| After | Commit these files | Who commits |
|---|---|---|
| A review or a repeat review | `docs/reviews/pr-<number>.md` and `docs/session-handoff.md` | The reviewer |
| Work that answers a review | `docs/reviews/pr-<number>-response.md`, each corrected file, and `docs/session-handoff.md` | The author |

Make one commit that holds the record and its handoff entry. Never leave either file uncommitted or unpushed.
A push is the only way for a reader of the PR head to see the record. The `run.ps1 codex-review` command reads the branch on origin, and so does the `review-gate` check (D-64).
A review is complete only when the remote holds the record. The session end gate below proves it.

An uncommitted review record has three effects:

- The next commit of the other provider absorbs it, and the history no longer shows who wrote what.
- An author can commit an approval that the author never read, and then report the wrong verdict.
- No reader of the PR head can read the record, because the record is not on the PR head.

Write the commit subject with a conventional prefix, and end it with the roadmap id of the PR. The GitHub number and the roadmap id differ: for example, "docs: review record of #4 (PR-3)".
Write the commit message in an impersonal voice. Name no provider, agent, harness, or model (T-6, D-16).

Fetch the remote and read the handoff again before you write the entry. Take the highest session number and add one (L-2).
Add the handoff entry at the top of the file, as a new entry, in the format of `AGENTS.md` (D-20). Name the remote head in the state of the build.

Set the author field to your own provider, and the role to `reviewer` or `author`.
Another provider can add an entry above yours while you work. Add your own entry. Never append to an older one, and never edit theirs.

## Session end gate

Run these four commands after the commit, in this order. The evidence comes from the remote, not from the local checkout.

```
git push origin <branch>
git fetch origin
git status --short --branch
gh pr view <number> --json headRefOid --jq .headRefOid
```

The status line must show no `[ahead N]`. The hash from `gh pr view` must equal `git rev-parse HEAD`.

A review that `run.ps1 codex-review` starts runs on the local branch `review/pr-<n>`, and it pushes with `git push origin HEAD:<branch>` (D-48). That branch has no upstream, so its status line shows no count. The hash comparison alone is the proof there.

Write the push line in the Verification section of the review record, and name the remote head in the handoff entry.
A record with no push line is incomplete, and the next session treats it as unpushed.

When the remote refuses the push, the review is not complete. Do not end the session.
Ask the owner to approve the push, and say in the handoff that the record has a commit and no push.
A sandbox that blocks the network can refuse the push without a clear message from git. Read the status line and the hash, and not the push output alone.

At the start of a review or a repeat review, run `git fetch` and `git status --short --branch` too.
When the checkout is ahead of the remote with a commit of the other provider, push that commit first. Record that in the review record.

## Scope limits

A review request permits inspection, verification, the review record, and the handoff entry.
It requires a commit of those two files, and a push of that commit to the PR branch (D-14).
It also permits a correction of a stale fact in the PR description, under "Correct the PR description" in `references/review-record.md`.
It does not by itself permit a code fix, a merge, or another external message.

A reviewer never pushes to `main`, never force-pushes, and never merges (D-5).
Honor an explicit permission of the owner that the session already holds.
When the reviewer writes a substantive fix, assess the provider eligibility again. The reviewer cannot approve its own contribution.
Do not disguise a fix as review metadata to pass the provider gate.
