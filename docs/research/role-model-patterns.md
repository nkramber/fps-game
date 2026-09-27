# Role-model patterns

- **Date checked:** 2026-09-26.
- **Repositories:** two sibling repositories on the owner's machine:
  - `the-thing-below` (TTB), checked out at branch `feat/pr-108-night-recovery`, head `ef6eae8`;
  - `what-you-carry` (WYC), checked out at `main`, head `4a20b82`.
- **Engines:** both are Godot 4 + C# games.
- **Method:** the entry points were read directly: `AGENTS.md`, the PR template, the ruleset, the handoff prompt, the latest review record and the Gitar wait script. Delegated read-only passes surveyed the rest.

Paths are relative to each repository root. Role-model decision IDs are written `WYC:D-N` or `TTB:D-N`, so they are not confused with this repository's `D-N`.

- **Observed** means seen in the checked-out files.
- **Adopt / Adapt / Decline** are recommendations for this repository. They take effect through [D-12](../decisions.md) and [D-13](../decisions.md) when this PR merges.

Neither repository is treated as correct by default. Both are about two weeks old and already show strain (see § 4).

## 1. Observed patterns

| # | Pattern | Evidence | Why it helps |
|---|---|---|---|
| 1 | **Single agent entry point** with a "first action" and a read order. `CLAUDE.md` and `AGENTS.md` are byte-identical, enforced by a test. | WYC `AGENTS.md` §First action, §Read order (161 lines, 15 KB cap); TTB `CLAUDE.md` (175 lines, 16 KB cap) | A fresh session with no memory knows what to read first and in what order. |
| 2 | **Newest handoff entry first.** The handoff is read before anything else. There are six fixed parts: what was done; build state with the remote head; in flight; traps; blocking questions; next action. | WYC `AGENTS.md` §Session handoff; `docs/session-handoff.md`, with 10 entries kept | Continuity across sessions and providers with no shared memory. |
| 3 | **Handoff rotation and archive.** Entries after the 10th move to `docs/session-handoff-archive.md`. Byte caps per entry. | WYC `tools handoff-rotate`; TTB checks HANDOFF 1–4, SIZE 2 | Keeps the start file small. The archives are still 764–768 KB. |
| 4 | **Handover (transitional) prompt.** A ready-to-paste prompt for the next session names: the merged PR and its SHA, the branch, base, role, open questions and first action. | WYC `.claude/skills/one-pr-one-session/references/merge-prompt.md`; TTB `one-pr-one-session/SKILL.md` step 6 | The next session starts with exact context, not a summary from memory. |
| 5 | **Append-only decision register** with stable IDs. Revisions are recorded as `Superseded by` or `Revised in part by` in the old row. | WYC `docs/decisions.md` (617 rows, 353 KB); TTB (1,209 rows, 756 KB) | Decisions can be cited and traced. Nothing is silently rewritten. |
| 6 | **Open-question register owned by the owner.** Each entry has options, a recommendation and what it blocks. Agents may not pick a default. Answers become `D-N` entries. | WYC `docs/questions.md` §filing rules, `AGENTS.md` §How to work with the owner (WYC:D-124) | Keeps product authority with the owner while work continues. |
| 7 | **Targeted reads** with grep recipes, instead of reading large registers in full. | WYC `AGENTS.md` §Read order; TTB `docs/runbooks/session-context.md` | Controls token cost once the registers grow. |
| 8 | **Design doc tied to decisions.** Claims cite `D-N`. External facts carry a check date. Refuted claims stay with a dated correction. | WYC `docs/design.md` (893 lines); TTB `docs/design.md` (796 lines) | Separates intent from evidence, and keeps a record of what was wrong. |
| 9 | **Roadmap on two levels.** The high-level phases are in `design.md` §7, with focused phase files per PR (Scope, Out of scope, numbered Exit tests, Review focus). TTB adds area files. | WYC `docs/roadmaps/phase-*.md`; TTB `docs/roadmaps/readme.md` | Exit tests make "done" checkable. |
| 10 | **Roadmap PR IDs separate from GitHub numbers.** Commit subjects look like `…(PR-87) (#103)`. | WYC `git log`; TTB `git log` | Plans can name PRs before they exist, but there are two numbering schemes. |
| 11 | **One session = one PR = one role.** The start gate refuses mixed work. | WYC `AGENTS.md` §How to work with the owner (WYC:D-121); TTB TTB:D-576 | Small, reviewable, attributable units. |
| 12 | **PR template with a gate checklist and a "Documents" disposition table.** Each doc category is marked Changed, No change needed or Not applicable, with a reason. A `doc-gate` job parses it. | WYC `.github/pull_request_template.md`; TTB same path | Forces an explicit check that docs stay consistent with the change. |
| 13 | **Cross-provider review record.** The file is `docs/reviews/pr-<n>.md`, with sections Identity (full head SHA), Provider gate, Scope, Findings (P0–P3), Verification and a single bold Verdict. Answers go in `pr-<n>-response.md`. | WYC `docs/reviews/pr-108.md`; `.claude/skills/pr-review/` | Durable, auditable review tied to an exact commit. |
| 14 | **Scripted review.** `make codex-review PR=<n>` runs Codex in a separate worktree, with a three-strike stop for findings that stay open. | WYC `Makefile:58`; TTB `docs/runbooks/merge.md` | Makes the other-provider review repeatable. It is also a large, fragile tool. |
| 15 | **Review-gate check run.** A `pull_request_target` workflow checks the verdict and head SHA of the record. | WYC `.github/workflows/review-gate.yml`; TTB same | A machine-visible merge gate. TTB admits it can't prove which provider wrote the record, because both use the same GitHub account. |
| 16 | **Auto-merge after owner confirmation.** It is armed only after: green CI, a current review verdict, the Gitar pass, and a four-part merge summary that the owner confirms. | WYC `AGENTS.md` §Git rules (WYC:D-516, WYC:D-524, WYC:D-533); TTB `docs/runbooks/merge.md` | Speed without giving up owner control. |
| 17 | **Ruleset as code.** Squash-only; no deletion or force-push; required thread resolution; named required checks; a test binds names to workflow jobs. | WYC `.github/rulesets/main.json`, `docs/runbooks/main-ruleset.md`; TTB `docs/runbooks/branch-protection.json` | Branch protection that can be reviewed and reproduced. |
| 18 | **Gitar automated review** (GitHub App `gitar-bot`). Authors answer every item before the cross-provider review. A wait script polls the check run and proves the dashboard comment is newer than the push. Pause and resume are handled by decision and grep-able markers. | WYC `.github/scripts/gitar-wait.sh` (WYC:D-575), `.claude/skills/gitar-review/`, commit `a3590ba` (WYC:D-574); TTB TTB:D-895/TTB:D-909/TTB:D-945/TTB:D-1073 | Cheap first-pass review. It showed quota and race problems, and was paused twice in days. |
| 19 | **CI cost control.** Docs-only changes skip heavy jobs. Newer pushes cancel older runs. Hosted runners are used; the self-hosted Mac runner was retired. | WYC `actions/ci-skip` (WYC:D-474), WYC:D-356; `docs/runbooks/macos-runner.md` (retired by PR-86: slow nights blocked PR checks, fork-PR code ran on the owner's Mac) | Keeps CI fast and safe on a public repository. |
| 20 | **Nightly checks** that PRs must respect: a "night-gate" requires a green night within 48 h. | WYC/TTB `night*.yml` | Catches slow or random failures. It is specific to their simulation (see § 3). |
| 21 | **Honest status reporting.** The handoff names the remote head. Red checks are named in merge summaries. Runner faults are re-run, not ignored. | WYC `AGENTS.md` §Session handoff (WYC:D-199), WYC:D-585 | Stops "it passed" claims that aren't true. |
| 22 | **Attribution policy.** No AI co-author trailers anywhere. The provider is recorded only in the handoff `Author:` field. | WYC and TTB T-6, `.claude/settings.json` | An owner choice. It conflicts with this harness's default trailers (Q-17). |
| 23 | **Controlled English (ASD-STE100)** for every `.md` file, with a checker and byte caps. | WYC `ste-writing` skill; TTB `ste-check` | Consistent prose for models. It is costly: in TTB, "A call that ran the STE checker alone cost 19% of the input tokens" (`docs/runbooks/session-context.md`). |

## 2. Disposition for this repository

| Pattern | Disposition | How, and why |
|---|---|---|
| 1 Single entry point | **Adapt** | [`AGENTS.md`](../../AGENTS.md) is the only source. `CLAUDE.md` is a one-line import (`@AGENTS.md`), so no parity test is needed. |
| 2 Newest-first handoff, six parts | **Adopt** | [`docs/session-handoff.md`](../session-handoff.md). |
| 3 Rotation and archive | **Adapt, later** | Rule: keep the 10 newest entries and move older ones to an archive file by hand. No tool until there are enough entries to need one. |
| 4 Handover prompt | **Adopt** | Every handoff entry ends with a ready-to-paste prompt ([workflow.md § Session handover prompt](../workflow.md#session-handover-prompt)). |
| 5 Decision register | **Adapt** | Uses `### D-N:` headings instead of wide table rows, so the file is easier to diff and grep. Settled items only. |
| 6 Owner-owned questions | **Adopt** | [`docs/questions.md`](../questions.md). |
| 7 Targeted reads | **Adopt, lightly** | Grep lookups are documented in each register. They become essential once the files grow. |
| 8 Design doc tied to decisions | **Adapt** | [`docs/design.md`](../design.md) splits content into *Approved (D-N)*, *Proposed* and *Open (Q-N)*, instead of emoji registers. |
| 9 Two-level roadmap | **Adapt** | The high-level roadmap is its own file. Focused roadmaps are created per phase only after it is accepted (D-11). No area files until needed. |
| 10 Roadmap PR IDs | **Decline for now** | The high-level roadmap names work areas, such as `P1.2`, not PR numbers. Focused roadmaps can add IDs if there are real cross-references. |
| 11 One session, one PR | **Adopt** | D-5. |
| 12 PR template with Documents table | **Adapt** | [`.github/pull_request_template.md`](../../.github/pull_request_template.md) keeps the Documents disposition. There is no parser yet; a docs-check can read it later if drift appears. |
| 13 Review record | **Adopt** | [`docs/reviews/README.md`](../reviews/README.md) defines a shorter record. |
| 14 Scripted review | **Defer** | Start the review by hand first (Q-15). Script it only after the format settles. |
| 15 Review-gate check | **Defer** | Useful once auto-merge exists (Q-14). The same-account provenance gap applies here too. |
| 16 Auto-merge with owner confirmation | **Adopt, when enabled** | [workflow.md § Auto-merge](../workflow.md#auto-merge). It is off until Q-14 is answered. |
| 17 Ruleset as code | **Adopt in P0** | This repository has no protection today. Commit it as a JSON ruleset plus a runbook. |
| 18 Gitar | **Document only** | D-7. [workflow.md § Gitar](../workflow.md#gitar-planned-not-active) records the design and the lessons: prove the review is current, bound the waits, and use one reversible pause marker. |
| 19 CI cost control | **Adopt** | Hosted Linux for docs and engine-free checks. Docs-only skip. No self-hosted runner on a public repository without an owner decision (Q-8). |
| 20 Nightly gate | **Decline now** | Reconsider as nightly automation or Gauntlet runs once engine tests exist (P3+). |
| 21 Honest status | **Adopt** | [AGENTS.md § Validation](../../AGENTS.md#validation). |
| 22 No-attribution rule | **Owner choice** | Q-17. |
| 23 STE and byte caps | **Decline** | Plain, concise English is enough at this size. Revisit caps only if the start files grow past about 10 KB. |

## 3. Constraints that must not transfer

These belong to those games and engines. Don't copy them into this repository without separate justification:

- **Engine and language:**
  - Godot 4 with .NET/C#;
  - `global.json` SDK pins;
  - `dotnet format` and xUnit;
  - Roslyn `det-lint`;
  - Godot headless smoke flags.
- **Simulation:**
  - deterministic integer-only simulation;
  - cross-OS bit-identity and replay-hash checks;
  - seeded bot "night" sweeps.

  An action FPS in Unreal uses floating-point physics and frame-dependent systems, where bit-identity isn't a goal.
- **Rendering and assets:**
  - texel-per-metre atlases;
  - text-grid sprites and palette ramps;
  - voxel/box model QA;
  - in-repo texture and sound synthesizers.
- **Platform:**
  - the Steam Deck frame and SSH rules;
  - three-OS matrices on every PR (not affordable with Unreal builds);
  - their specific C# tools (DocGate, ReviewGate, HandoffRotate, SteCheck).
- **Game design:** their tenets on game text, world docs and genre rules.

## 4. Gaps and risks for this project

- **Process overhead.** Much of each role model's recent roadmap is process or CI work.
  - In WYC, 13 of the last 22 entries in `docs/roadmaps/phase-2-first-playable.md` are review, Gitar, night or CI items, for example PR-71, PR-73, PR-78 to PR-88.
  - TTB measured 68.4 million context tokens for its mean code PR (`docs/design.md` M-1).
  - This repository adds a gate only when a real failure or risk justifies it, and records that reason.
- **Register growth.** Both decision files passed 350 KB within two weeks.
  - Keep entries short, cite instead of restating, and split by topic only when lookup gets slow.
- **Churn in the review tooling.** Gitar was paused and resumed twice within days in TTB. Every rule change stacked revision chains onto old decisions.
  - Use one reversible switch per external dependency.
- **Engine CI gap.** Neither role model had to build a heavy engine in CI. This project needs its own evidence model for Unreal builds (Q-8).
- **Binary assets.** Neither role model used binary engine assets at scale. LFS quotas, locking and map contention are new risks.
- **Provider provenance.** Both providers commit as one GitHub account, so the provider rule can't be proven by machine. The handoff `Author:` field and the review record are the evidence. Accept this risk, and don't claim more.
- **Licensing.** The role models don't face the GPL vs Unreal EULA conflict. This project does (Q-1).
