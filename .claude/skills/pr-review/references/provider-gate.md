# The provider gate

The `pr-review` skill names this file at step 1. The gate runs before the substantive review and before any approval (T-4, D-6).

## Mandatory provider gate

**The reviewer MUST NOT come from the provider that wrote the PR.**
This rule applies before the substantive review starts and before any approval (T-4, D-6).

| Provider that wrote the PR | Required reviewer |
|---|---|
| Claude Code, Anthropic | Codex, OpenAI |
| Codex, OpenAI | Claude Code, Anthropic |

A different model, account, session, or subagent from the same provider does not qualify (D-6).
A prompt that assigns the name of the other provider does not change the actual provider.
Author self-checks and automated tests do not satisfy this gate.

Substantive changes alter code, data, configuration, requirements, or executable instructions.
Review findings and test reports alone do not make the reviewer a PR author.

1. Identify the actual reviewer provider from the active environment.
2. Identify every provider that contributed substantive changes or fixes to this PR.
3. Verify authorship from a statement of the owner, or from the handoff entries and review records.
4. Match each source to this PR and its revision.
5. Record the providers, the source, and the result of the gate in the review record.

The `Author:` field of each handoff entry names one provider: `Claude Code` or `Codex`. Read the entries that name the branch of the PR.
The newest handoff entry can describe a review and not the authorship. Read the `Role:` value of the entry too.
Both providers push as one GitHub account, so a Git account does not identify the provider (F-9).
Do not infer authorship from prose style, commit email, or a branch name.

**Stop with `Blocked` when the providers match, when you cannot identify the author, or when the evidence conflicts.**
State the fact or the eligible reviewer that the review needs. Ask the owner to supply that fact or to start a session of the opposite provider.
Do not perform a substitute review with another model from the same provider.

When both providers wrote substantive changes in the PR, neither one qualifies for the whole PR.
Record the conflict, and ask the owner how to separate the changes.
Do not approve through reciprocal review of selected hunks.
