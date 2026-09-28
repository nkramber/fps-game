# Roadmaps

Status: the index of the focused roadmaps. Started 2026-09-27 (D-18). Written in ASD-STE100 (D-17).

This folder expands the roadmap of `docs/design.md`. It never replaces it. The global order of every PR lives in section 8 of that file. Each file here cites a decision and never restates it.

## Where to start

1. Section 8 of `docs/design.md`: the strict order of every PR and gate. Read it first.
2. Section 7 of `docs/design.md`: the phase that holds a PR, and the gate of that phase.
3. A phase file below: the scope, exit tests, review focus, and questions of one PR.
4. An area file below: how one area works, and which PR builds each part.

## When a focused roadmap starts

A focused roadmap starts only after the owner accepts the high-level roadmap (D-11, OQ-18). Phase 0 has no phase file, because section 7 of `docs/design.md` holds its PR entries. PR-7 adds the first phase file, for phase 1.

Write a phase file just before its phase starts. Do not write one more than one phase ahead, because such a plan goes stale. A new phase file is one docs PR.

## The two axes

A phase file and an area file cut the same plan two ways. Each PR appears in one phase file, and in one or more area files.

| Kind | Axis | Answers |
|---|---|---|
| Phase file | time | What does this PR build, and what must it pass? |
| Area file | subject | How does this area work, and which PR builds each part? |

Add an area file only when one area spans more than one phase. Two links carry an area back to the time axis:

- Each entry in section 7 of an area file names its phase file.
- Section 8 of an area file gives the order inside that area. It points to section 8 of `docs/design.md` for the global order.

## The phase files

| File | Phase | Gate |
|---|---|---|
| `docs/roadmaps/phase-1-engine-proof.md` | Phase 1: engine and toolchain proof | A clean clone builds, packages, and runs one headless test on Windows. M-1 and M-2 have values (D-91, D-95). |

Name a phase file `phase-<n>-<slug>.md`, for example phase-1-engine-proof.md. Name an area file area-<slug>.md.

## The area files

| File | What it holds |
|---|---|
| none yet | |

## The shape of a file

Each file here keeps the status header and the sections 1, 5, 7, 8, and 9 of the design doc template. The design-doc-style skill holds that template. Load it before you write or edit a file here.

A PR entry in a phase file uses a global PR-# id that continues section 8 of `docs/design.md`. Each entry gives the scope, what is out of scope, the exit tests, the review focus, the questions, and the area file that it cites. A phase file follows the phase files of the-thing-below (D-70).
