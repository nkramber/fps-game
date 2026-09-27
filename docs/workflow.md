# Workflow: PRs, review, merge, CI/CD and handoff

This file explains how work moves from a session to `main`.

- The rules come from D-5, D-6, D-7 and D-13 in [decisions.md](decisions.md).
- Anything not yet enabled is labelled **Now**, **Planned (P0.x)**, or **Owner-gated (Q-N)**.

## PR loop

**Now:**

1. **Start.**
   - Read the newest [session-handoff](session-handoff.md) entry and [AGENTS.md](../AGENTS.md).
   - Run `git fetch origin`.
   - Branch from the current `origin/main`. Name the branch `<type>/<short-slug>`, for example `docs/p1-engine-proof` or `feat/p3-movement`.
2. **One concern.** One session delivers one PR (D-5). If a second concern appears, file it in the handoff or `questions.md`. Don't widen the PR.
3. **Commits.**
   - Use conventional subjects: `feat:`, `fix:`, `docs:`, `test:`, `chore:`, `refactor:`.
   - Keep commits small and do no history rewriting after review starts.
   - For the attribution policy, see Q-17.
4. **Validate** as [AGENTS.md § Validation](../AGENTS.md#validation) describes. Put the real results in the PR body.
5. **Hand off.** Add a new handoff entry at the top of `session-handoff.md`, ending with the handover prompt. Commit it on the PR branch.
6. **Push and open the PR** with `.github/pull_request_template.md`.
   - Push with `git push -u origin <branch>`.
   - Never push to `main`, force-push a shared branch, or bypass checks.
7. **Review** by the other provider. See [Cross-provider review](#cross-provider-review).
8. **Merge** only by the owner, or by auto-merge once Q-14 allows it. Squash merge. An agent never merges without explicit owner authorization (D-5).

## Cross-provider review

**Now (D-6). How the review starts is open (Q-15).**

- **Who.** The reviewer is a provider other than the author: Claude Code reviews Codex work, and Codex reviews Claude Code work. The author's provider is the `Author:` field of the PR's handoff entry. A subagent of the authoring provider is **not** a cross-provider review.
- **How to start it, for now.** The owner, or the author with owner consent, opens a fresh session of the other provider in a clean checkout of the PR branch, with this prompt:

  ```
  Review PR #<n> of nkramber/fps-game as the cross-provider reviewer.
  Author provider: <Claude Code|Codex>. You must be the other provider; if not, stop with "Blocked".
  Check out the PR head, read AGENTS.md, the newest handoff entry, and docs/reviews/README.md.
  Review the whole diff against the PR's stated scope, the exit criteria it claims, docs consistency,
  and D-/Q- rules. Write docs/reviews/pr-<n>.md in the format of docs/reviews/README.md, commit it on
  the PR branch with subject "docs: review record for #<n>", push, and stop. Do not merge.
  ```

- **Record.** The review lives in `docs/reviews/pr-<n>.md` ([format](reviews/README.md)). The verdict is exactly one of `Ready for owner merge`, `Changes required` or `Blocked`, and it names the full head SHA it reviewed.
- **Findings.**
  - The author answers each finding in `docs/reviews/pr-<n>-response.md` with a fix commit or a reasoned rebuttal.
  - The reviewer then re-reviews the new head.
  - If a P0–P2 finding is still open after three rounds, stop and ask the owner.
- **Known limit.** Both providers push as the same GitHub account, so provider identity can't be proven by machine. The handoff `Author:` field and the record are the evidence. Don't claim more than that.
- **Planned (P0.5):** a script that starts the Codex CLI review in a separate worktree. Codex CLI 0.157.1 is installed on the development Mac.

## Auto-merge

- **Now:** off. The repository has `allow_auto_merge: false`, and `main` has no ruleset.
- **Owner-gated (Q-14).** The recommended policy is shown below. An agent may arm `gh pr merge <n> --auto --squash` only when **all** of these hold:
  1. The owner has recorded a decision enabling auto-merge.
  2. The `main` ruleset (P0.3) exists, and every required check exists and is green on the head. A check that was skipped, cancelled or is still running doesn't count as green.
  3. The review record says `Ready for owner merge` for the current head. A later commit that only adds the review record or the handoff doesn't invalidate it.
  4. No review thread is unresolved.
  5. Once Q-16 activates it, the Gitar pass is complete for the head.
  6. The owner confirmed this PR after a short merge summary: what changed, how it was validated, CI status naming any red or skipped check, and the review verdict.
- **Revoke.** If a new commit changes code after approval, or a check turns red, disable auto-merge (`gh pr merge <n> --disable-auto`) and restart review.

## CI/CD strategy

- **Now:** there are no workflows. Validation is local, and the PR body reports it.
- **Planned (P0.2): docs check** on hosted `ubuntu-latest`, for every PR and push to `main`. It checks:
  - relative links and anchors resolve;
  - `D-N` and `Q-N` headings are unique and sequential;
  - every cited `D-N` and `Q-N` exists;
  - the newest handoff entry has the required parts and names the PR branch.

  Use a small script under `scripts/` with no heavy dependencies. Newer pushes cancel older runs.
- **Planned (P0.3): `main` ruleset as code.** It requires:
  - a PR;
  - squash-only merges;
  - no force-push or deletion;
  - resolved conversations;
  - the docs check as a required check.

  Commit the JSON and a runbook. The owner applies or approves the live change.
- **Engine builds (P1, Owner-gated Q-8).** Hosted runners have no Unreal Engine. The recommended model is:
  - engine compiles, automation tests and packaging run locally;
  - each engine PR attaches the exact commands, versions and a log excerpt;
  - hosted CI keeps the engine-independent checks.

  A self-hosted runner on the owner's Mac needs an explicit owner decision, because the repository is public and fork PRs would run on that machine.
- **Asset checks (P5).** Add import and provenance validation when assets begin.
- **Delivery (P8).** Packaged builds are produced by a recorded command. Where they are published is decided in P8.

## Gitar (planned, not active)

- **State:** documented only (D-7). **Don't** add Gitar steps, waits, scripts, required checks or PR-template gates until the owner explicitly confirms that Gitar is integrated with this repository (Q-16).
- **What it is:** Gitar is a third-party GitHub App (`gitar-bot`). It posts an automated review dashboard comment and review threads on each PR, and reports a `Gitar` check run. Both role models use it ([role-model-patterns.md](research/role-model-patterns.md), pattern 18).
- **Intended use, once confirmed:**
  1. After each push, wait for the Gitar check run on the head. Poll at intervals and stop at a fixed limit, then tell the owner.
  2. Prove the review is current: the dashboard comment must be updated *after* the push, because a completed check alone isn't proof.
  3. The author answers every Gitar item (fix, or reasoned rebuttal and resolve) **before** asking for the cross-provider review. The cross-provider reviewer reads the Gitar threads but doesn't reply to Gitar.
  4. The Gitar pass becomes a condition of auto-merge (step 5 above).
- **Activation:** one dedicated PR adds the wait script, the template line and the required-check change, all citing the owner's confirmation `D-N`. If Gitar later fails or its quota runs out, pause it with one reversible decision and one grep-able marker. Don't scatter edits.

## Session handover prompt

**Now:**

- Every handoff entry ends with a fenced, ready-to-paste prompt for the next session, so a fresh session with no memory can start correctly.
- Write it after the PR is opened. Update it if the PR merges before the session ends.
- Template:

```
Start: <one concern for the next PR, e.g. "P0.2 docs-check CI">
Repository: nkramber/fps-game. Base: origin/main at <sha> (<PR #n merged | PR #n still open>).
Branch to create: <type>/<slug>. Role: <author|reviewer>. Provider: <the provider that must run this>.
First: read the newest entry of docs/session-handoff.md, then AGENTS.md.
Roadmap: docs/roadmaps/high-level-roadmap.md, work area <Px.y>.
Open questions that affect this work: <Q-N list or "none">.
Owner actions still pending: <list or "none">.
First concrete action: <command or file>.
```
