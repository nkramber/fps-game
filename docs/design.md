# Design: vision and guardrails

This file states the intended player experience and the rules that protect it.

- Each item is marked **Approved** (with its `D-N` in [decisions.md](decisions.md)), **Proposed** (a recommendation the owner hasn't settled), or **Open** (with its `Q-N` in [questions.md](questions.md)).
- Only Approved items constrain work. Proposed items guide drafts and prototypes.
- The plan for reaching this vision is in the [high-level roadmap](roadmaps/high-level-roadmap.md).

## 1. Vision

**Approved (D-1):**

- The product is one complete, polished, replayable level of a **fast-paced, resource-limited first-person shooter**.
- *Doom (2016)* is a high-level reference for **combat intensity and pacing only**:
  - relentless forward pressure;
  - short, intense arena fights;
  - movement as defence;
  - a rhythm of fight → breath → fight.

**Proposed:** the player should feel powerful but never comfortable. Scarce resources push them *toward* danger rather than away from it.

## 2. Guardrails

| Guardrail | Status |
|---|---|
| Everything is original: setting, characters, weapons, names, art, audio and level design. Reference games inform feel and pacing, never specific mechanics, assets, names or layouts. | Approved (D-2) |
| Unreal Engine 5. | Approved (D-3); version open (Q-4) |
| A compact, hand-authored level. No large-world tooling (World Partition streaming) without measured need. | Approved (D-4) |
| Generated assets (Meshy) are never assumed game-ready, and are never used for whole buildings or terrain. No spending without approval. | Approved (D-8) |
| Scope is one level until the owner accepts it. New levels or systems not in the brief become questions, not plans. | Approved (D-1) |
| Feel before content: movement, weapons and the combat loop pass owner gates before level art is produced. | Proposed (roadmap P3/P4 gates) |
| Readability over detail: enemies, pickups and hazards are readable at speed. Art density must not hide threats. | Proposed |
| The performance budget is a design constraint, not a late optimisation. | Proposed; the budget is open (Q-5) |

## 3. Core experience

| Topic | Current state |
|---|---|
| Player verbs: move, jump, aim, shoot, switch weapon, and possibly dash or melee | Proposed; settled in P2 |
| Which resources are limited, and the **original** mechanic for regaining them mid-fight | Open (Q-10) |
| Weapon roster size and roles | Open (Q-11) |
| Enemy roster size and roles | Open (Q-11) |
| Level length, number of combat spaces, secrets | Open (Q-11). Placeholder assumption: a 15–25 minute first clear, with 4–6 combat spaces. |
| What "replayable" means: difficulty, score, time or rank, secrets, routes | Open (Q-11) |
| Setting, tone and art direction | Open (Q-9) |
| Platforms, input devices and frame-rate budget | Open (Q-5) |
| Working title | Open (Q-3) |

## 4. How this file changes

- When the owner answers a question, the matching row becomes **Approved** and cites the new `D-N`, in the same PR as the decision.
- A **Proposed** item may be edited in any PR that explains why.
- If an Approved item seems wrong, don't edit it. File a question.
- Keep this file short. Detailed system design belongs in focused roadmaps or a later `docs/systems/` file, linked from here.
