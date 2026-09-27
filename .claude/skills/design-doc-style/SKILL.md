---
name: design-doc-style
description: The template and the rules of docs/design.md and of each focused roadmap. Load before you edit docs/design.md or a file in docs/roadmaps/.
---

# Design-doc style skill

The design doc follows the template of the design-doc-style skill of the-thing-below (D-19). Write in ASD-STE100. Load `ste-writing` first.

The design doc is one file: `docs/design.md`. Its roadmap section is the high-level roadmap (D-18). Focused roadmaps are separate files in `docs/roadmaps/`. The roadmap section links to them.

## Section template

The file starts with a status header that has no section number. State the doc status, what it supersedes, and the date you verified each external fact. Add a dated line for each correction pass. Never delete a refuted claim. Mark it refuted and keep it.

The numbered sections follow. Each number below is the number in the heading, for example `## 7. Roadmap`.

1. **Thesis.** One paragraph. What the game is for and why the plan has this order.
2. **Lessons learned.** Numbered. Each lesson names the event that taught it. Carry lessons from the two role models when they apply.
3. **System map.** A table of components, what each reads, what each writes, and its sensitivity.
4. **Cost model.** What we pay, what we do not know, and which measurement will answer it.
5. **Defect and finding register.** A numbered table. Findings carry evidence and dates. Findings bind to plan items ("binds PR-3"). The status legend:
   - ✅ done (code merged, or "doc" for a document-only correction)
   - 🔧 planned (item listed)
   - ⚠ constraint (binds a pull request)
   - ❓ needs owner input
   - ⏸ out of scope (a decision parked it)
   - 🅿 parked
6. **Guardrails.** The tenets, quoted in full, then the numbered invariants that every PR must keep.
7. **Roadmap.** Phases. Each entry has an id (PR-#, M-#), a technical paragraph, and a gate. It ends with a plain-English paragraph in a block quote that starts with "*In plain English:*".
8. **Sequence.** A strict ordered list with a single owner. Mark each gate.
9. **Open questions.** A link to `docs/questions.md`. The register there has the numbers. Record the date and the answer there when one arrives.

## Rules

- Every roadmap entry ends with a plain-English paragraph. The paragraph explains the item to a reader who does not know the code.
- Every external fact has a source and a date.
- The numbers continue across revisions. Never renumber.
- "One concern per pull request" applies to the plan items (G-7).
- A refuted premise stays in the doc with a dated correction.
- Ids: F-# findings, PR-# code changes, M-# measurements, D-# owner decisions (in `docs/decisions.md`), OQ-# open questions (in `docs/questions.md`), G-# guardrails, L-# lessons, T-# tenets. A review file uses its own local ids, and they never enter the design doc register.
- The tenets live in the design doc and in the agent files. The agent files quote them in full.
- A roadmap entry cites a D-# id. It never restates the decision.
- Label each claim of a plan as evidence (with a link), recommendation, assumption, or unknown.

## Plain-English paragraph rules

- Max 25 words per sentence (STE rule 6.3).
- No code identifiers unless the reader needs them.
- Say what the game lacks today, what the change does, and why it is safe.

## Focused roadmaps

A focused roadmap covers one phase or one area. It uses the status header and the same sections 1, 5, 7, 8, and 9. It links to the design doc for the system map and the cost model.

Its PR-# ids continue the global sequence. It never restates a decision. It cites the D-# id. `docs/roadmaps/readme.md` gives the rules of the index.

Each PR entry lists:

- its scope, and what is out of scope.
- its exit tests.
- its review focus.
- the questions it needs answered before it starts.
- the area roadmap file that it cites, when one exists.

A phase file follows the phase files of the-thing-below (D-70):

- Each entry of section 7 has a numbered heading, for example `### 7.2 PR-9: the project scaffold`. The reference check reads the PR id from that heading.
- The fields are bold labels: **Scope.**, **Out of scope.**, **Exit tests.**, **Review focus.**, **Questions.**, and **State.** The **State.** field holds a status mark of the legend.
- An owner step and a measurement get an entry with no PR id.
- The last entry of section 7 is the gate of the phase, with one numbered line for each condition.
- Section 8 gives the order inside the phase. Section 9 gives a table of the questions of the phase.
