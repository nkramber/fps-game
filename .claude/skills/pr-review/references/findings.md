# Findings

The `pr-review` skill names this file at step 7. It holds the test for a finding, the severity table, the finding format, and the attribution test.

## Precise findings

Investigate each suspected defect before it becomes a finding.
Search for a caller guarantee, a validation layer, a test, or a later decision that can refute the concern.
Use a reproduction, a failed assertion, or a complete causal trace as evidence.
Keep a verified defect apart from an open question or an optional suggestion.

Each finding contains:

- A stable local id, a severity, and a short title that states the defect.
- The reviewed commit and the smallest useful file and line range.
- The input or the state that triggers the defect.
- The expected behavior, with the relevant contract or D-# id.
- The actual behavior, and its result for the player, the data, the build, or the maintainer.
- The evidence: the command, the trace, or the artifact.
- A correction direction, and the regression check that proves the fix.

Put repeated symptoms of one cause under one finding. Name the other affected locations without duplicate findings.
Do not prescribe a broad rewrite when a smaller correction restores the contract.
Do not invent findings to meet a quota. A thorough review can produce no actionable findings.

Answer two questions before a finding goes into the record:

1. Does the changed code break a contract that this PR names?
2. Does a stated exit test of this PR fail?

A finding needs one answer of yes. A concern with two answers of no goes under `## Out of scope`.

| Severity | Meaning |
|---|---|
| P0 | An immediate critical failure, such as broad data loss, a leaked secret, or a build that cannot start. State the demonstrated scope. |
| P1 | A major correctness or recovery failure, or a failed required gate. Resolve it before the merge. |
| P2 | A concrete defect or a material contract gap under a supported condition. Resolve it before the merge, or get an explicit owner disposition. |
| P3 | An optional improvement with no broken required contract. It does not block the merge, and it never counts for the three-strike stop. |

Scope decides whether a concern goes into the table at all. Severity decides how much it blocks. A concern outside the scope of this PR takes no severity.
Severity describes impact and urgency. It does not replace evidence or the project gate.

Do not reduce a severity because the patch is small, or because the author calls the change safe.
When two owner statements conflict, quote both. File the question in `docs/questions.md`, and stop the work that it blocks.

## Finding format

Give each finding a stable id: the letter `P`, the severity number, a hyphen, and an index. `P1-1` is the first P1 finding.
Keep the id for the life of the PR. Never renumber a finding on a repeat review.

```markdown
### P<severity>-<index>: <short title that states the defect>

Status: open.

Open at: `<sha>`, `<sha>`.

File: `<path>:<line range>`, or Commit: `<sha>`.

Trigger: the input or the state that produces the defect.

Expected: the required behavior, with the contract, tenet, guardrail, or D-# id.

Actual: the observed behavior.

Consequence: the result for the player, the data, the build, or the maintainer.

Evidence: the command, the trace, or the file that shows the defect.

Correction: the smallest change that restores the contract.

Regression check: the command or the test that proves the fix, and the result that must appear.
```

The `Status:` line takes exactly one of four forms:

| Form | Use it when |
|---|---|
| `open.` | The finding holds at the head of this round. |
| ``fixed in `<sha>`.`` | A commit corrected the finding, and its regression check passes. The hash has 7 to 40 hexadecimal characters. |
| `accepted risk, D-<n>.` | An owner decision accepts the risk. The decision id names that answer. |
| `withdrawn.` | The evidence refuted the finding. |

Start the `Status:` line and the `Open at:` line with those words. Write no list marker and no bold before them, because the `run.ps1 codex-review` command reads each line by its first word.
A status in another form fails the round (T-2).
A withdrawn finding stays in the file with the evidence that refuted it. Never delete a finding.

## The Open at line

The `Open at:` line lists the effective head of each review round in which the finding is open. Put each head in backticks (D-14). Write each head one time, in the order of the rounds. A round that opens the finding writes its head.

A repeat review adds its head to each finding that stays open or opens again. It never removes a head.

The `run.ps1 codex-review` command counts the heads. A P0 to P2 finding with three heads, in a round that does not approve, stops the fix loop with exit 11 (D-14). The author then asks the owner.
An open finding whose line does not name the head of the round fails the round (T-2).
Each `###` heading of the Findings section is a finding heading with a severity from P0 to P3. Any other line that starts with `###` in that section fails the round.

## Do not raise a tool name as attribution

T-6 and D-16 prohibit text that names an agent, harness, or model **as the source of the work**.
A tool name that identifies a configured file, a schema, or a verified version is not attribution.

| Raise it | Do not raise it |
|---|---|
| A commit body that says an agent wrote the change. | The path `.claude/settings.json`. |
| A co-author trailer or a generation line. | A decision that names the tool that it describes, such as D-14. |
| A PR description that credits a model. | A document that records which tool refused a file. |

Apply the same test to every file before a finding. A reading that condemns the decision register is too broad.
