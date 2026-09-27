# The repeat review

The `pr-review` skill names this file when the author revises the PR. It also holds the rule for the comments on the PR.

## Repeat review procedure

Do these steps in order after the author revises the PR.

1. Read the response file when one exists.
2. Check the provider gate again. A fix by the reviewer changes eligibility.
3. Read the new head, the new base, and the diff since the reviewed head.
4. Verify each claimed fix against its original trigger and its regression check.
5. Set the `Status:` line of each earlier finding. Keep every id and every piece of evidence.
6. Add the head of this round to the `Open at:` line of each finding that stays open (D-14).
7. Add the head of this round to the `Open at:` line of each finding that opens again.
8. Inspect the new diff for new defects and affected consumers.
9. Add each new finding with the next index in its severity.
10. Update the Identity list to the new effective head.
11. Update the Verification section with the commands that ran on the new head.
12. Move the earlier verdict to `## Earlier verdicts`, with its head.
13. Write the verdict against the new head. Keep one verdict name in the Verdict section.
14. Commit the review record and your handoff entry together, then run the session end gate.

Edit the existing `docs/reviews/pr-<number>.md`. Do not make a second file for the same PR.
Do not delete the earlier verdict. Move it, and keep each finding and its history.

The earlier verdict goes in the `## Earlier verdicts` section, above the Verdict section. A heading there must not be `## Verdict`.
The reader of the record takes the section under the exact heading `## Verdict`, and it fails a section that names two verdicts.
The reader counts the verdict names in the prose too. So write "the earlier findings are fixed", and not "the changes required are done".

Close a finding only when the evidence proves the fix or an owner decision resolves it.
Record each required check that still waits for a result.

### When a finding closes

A finding closes when the correction makes its stated trigger pass and its regression check pass. Set the status to ``fixed in `<sha>`.`` then.

A new trigger for the same class of defect is a new finding with a new id. Assess that new finding against the scope rules of `references/scope-and-read.md`. It is not a reason to keep the old id open.

A P0 to P2 finding that is open in three rounds stops the fix loop with exit 11 (D-14). Write the pattern in the review record. The author asks the owner whether this PR carries the whole surface, or a later PR does. A fourth correction of one finding is a scope question, and not a defect.

## The comments on the PR

The reviewing provider reads the existing PR comments and takes them into its own review context. It never replies to a comment, never resolves a thread, and never writes a comment on the PR.

- A comment is a claim about the code, like any finding. Verify it against the head, and record the result under `## PR comments` in the review record.
- An answer of the author is evidence, and the review checks it: the trigger, the contract, and the commit that it names.
- A comment that the author refuted with evidence is not a finding. A comment that the author fixed is a fix to verify.
- A thread that stays open without an answer blocks the next round. The command refuses to start while a thread is unresolved (D-52).
- The gitar pass stays out of this repository until the owner answers OQ-16 (D-7).
- A comment does not make its writer an author. The provider gate reads the providers of the substantive commits alone.
