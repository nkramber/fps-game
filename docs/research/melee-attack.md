# Melee attack

Status: research, checked 2026-10-03 for PR-26. Written in ASD-STE100 (D-17).

Section 7.6 of `docs/roadmaps/phase-3-core-feel.md` asks the review to read the melee attack against Unreal practice (D-34). This file records each Epic page and each fact of the engine source that PR-26 uses.

## 1. Epic pages, read 2026-10-03

| Page | Version | Fact that PR-26 uses |
|---|---|---|
| [Traces with Raycasts](https://dev.epicgames.com/documentation/en-us/unreal-engine/traces-with-raycasts-in-unreal-engine) | 5.8 | A trace can be a line, a box, a capsule, or a sphere. A single trace gives one hit result. A trace by channel hits each object that responds to that channel. |
| [Traces Overview](https://dev.epicgames.com/documentation/en-us/unreal-engine/traces-in-unreal-engine---overview) | 5.8 | A shape trace is a sweep from a start point to an end point. A shape trace is the tool when a line trace is not enough. A single trace gives the first thing that matches the criteria. |

## 2. Facts of the engine source, Unreal Engine 5.8.3

- `UWorld::SweepSingleByChannel` sweeps a shape from a start point to an end point on a trace channel. It gives the first blocking hit. The shape can be a box, a sphere, or a capsule (`World.h`).
- `FCollisionQueryParams::bTraceComplex` selects the complex collision. The melee attack sets it to false, so the sweep uses the simple collision of each mesh (`CollisionQueryParams.h`).
- For a sphere sweep, `FHitResult::ImpactPoint` is the point where the surface of the sphere touches the object. When the sweep starts inside an object, `ImpactPoint` is the same as `Location` (`HitResult.h`).
- `UTextRenderComponent` starts a new line at a newline character or at `<br>` (`TextRenderComponent.cpp`). Its default vertical alignment is `EVRTA_TextBottom`, so more lines go up from the place of the text.

## 3. The method of the attack

- A sphere sweep from the eye of the pawn along the view, on the channel "Weapon" (`ECC_GameTraceChannel1`). The shots use the same channel (D-130).
- The center of the sphere stops one radius short of the range. So the front of the sphere reaches the range of the data asset and no further (D-170).
- The sweep ignores the pawn that attacks. The view mesh of the weapon has no collision.
- The first blocking hit takes the attack. A wall between the eye and a gym target takes the attack.
- The time between two attacks uses the time of the world, with a margin of 0.1 ms for the rounding of the frame times. The weapon rule uses the same margin.
- The jab and the hold of the fire read the time of the world, so the melee component needs no tick.

## 4. The place of the finish of phase 4

D-115 and D-116 give the melee attack a second use in phase 4: the finish of a stunned enemy, which gives health. The function `UIronMeleeComponent::HitActor` gives each attack to the actor that it hit. Phase 4 adds the case of a stunned enemy there. The sweep, the time between two attacks, and the jab do not change.

Phase 3 has one case, the gym target, so the rule has no interface for a target of the melee attack (T-1). Phase 4 adds the second case, and can then make the interface.
