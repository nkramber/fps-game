# Review records

Each PR's cross-provider review lives here as `pr-<GitHub PR number>.md`. The author's answers live in `pr-<n>-response.md`. The process is in [workflow.md § Cross-provider review](../workflow.md#cross-provider-review).

## Review record format

```
# Review of PR #<n>: <title>

## Identity
- Reviewer provider: <Claude Code|Codex>. Author provider (from handoff): <the other one>.
- Base: <full sha>. Head reviewed: <full 40-char sha>. Date: YYYY-MM-DD.

## Scope checked
What the PR claims to do, and the paths inspected. Name any path not inspected, and why.

## Findings
### <P0|P1|P2|P3>-<k>: short title
- Where: path:line
- Problem: what is wrong, with the concrete scenario or evidence.
- Expected: what would be correct.
- Status: Open | Fixed in <sha> | Rebutted (see response)

## Verification
Commands run by the reviewer, and their real results. Say plainly what was not run.

## Verdict
**<Ready for owner merge | Changes required | Blocked>** for head <full sha>.
```

- **Severity:**
  - **P0**: breaks `main`, loses data, or exposes secrets.
  - **P1**: wrong behaviour, or a violated decision.
  - **P2**: a real defect or inconsistency that should be fixed before merge.
  - **P3**: optional improvement.

  Any open P0–P2 finding means `Changes required`.
- **Blocked** is for cases where the review can't proceed. Examples: the reviewer is the same provider as the author, or the head changed during the review.

## Response format

```
# Response to review of PR #<n>
### <finding id>
- Disposition: Fixed | Partly fixed | Disagree
- Evidence / change: <sha and explanation, or reasoning with references>
```
