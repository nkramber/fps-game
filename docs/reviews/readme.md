# Review records

Status: the index of the review records. Started 2026-09-27. Written in ASD-STE100 (D-17).

This folder holds one review record for each PR, and the response file of the author when one exists. The format comes from what-you-carry (D-14, D-46). The pr-review skill holds the format from PR-3 onward. The files for PR-1 and PR-2 use the earlier format of this file, and they stay as written, because a dated record is history.

## Names

| File | Holds |
|---|---|
| `pr-<number>.md` | the review record. The number is the GitHub PR number, not the roadmap id. |
| `pr-<number>-response.md` | the answer of the author to each finding |

Provider names are permitted here and in the author field of the session handoff. They are not permitted in a PR description or a GitHub comment (T-6).

## Where the format lives

| Part | File |
|---|---|
| The record skeleton, the machine-read parts, and the verdicts | `.claude/skills/pr-review/references/review-record.md` |
| The finding format, the severities, and the `Open at:` line | `.claude/skills/pr-review/references/findings.md` |
| A repeat review of the same PR | `.claude/skills/pr-review/references/repeat-review.md` |
| The response file of the author | `.claude/skills/review-response/SKILL.md` |

`make codex-review PR=<n>` reads three parts of a record: the `- Head:` line, the `## Findings` section, and the `## Verdict` section (D-14). A part in another form fails the round with the exit code 1.
