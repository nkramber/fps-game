# Provenance policy

Status: the provenance policy for art and audio, and the Meshy choice. PR-18 adds this file (D-118 to D-121). Written 2026-09-29 in ASD-STE100 (D-17).

This file gives the rules for the source and the license of each art file and each audio file. It also gives the Meshy choice of the owner and the Meshy facts behind it. The decisions are in `docs/decisions.md`. The art direction is in `docs/game/art-proposals.md` (D-117).

Labels: each claim is evidence (with a source and a date), a recommendation, an assumption, or an unknown. A rule that cites a D-# id is a decision of the owner.

## 1. Scope

The policy applies to each art file and each audio file that enters the repository or the package. Examples are meshes, textures, images, materials, sounds, music, and fonts. It applies to import sources, such as FBX and WAV files, and to the Unreal assets that come from them.

The policy does not apply to code. The MIT License covers the code (D-24). The policy also does not apply to Epic content that the project uses from the engine folder, because that content is not in the repository.

## 2. The provenance record

Each file in the scope has one provenance record (D-2, D-38). The record has these fields:

| Field | Value | Example |
|---|---|---|
| path | the path of the file from the root of the repository | `Game/Content/Maps/L_Test.umap` |
| kind | `original`, `free-license`, `bought`, or `ai-generated` | `original` |
| source | the web page of the file, or `original` | `original` |
| license | the SPDX id of the license | `MIT`, `CC0-1.0`, `CC-BY-4.0` |
| author | the person who made the file, or the tool that made it | the owner, or Meshy |
| date | the date the file entered the repository, as YYYY-MM-DD | 2026-09-29 |
| attribution | the attribution text of a CC BY file, or empty | see section 4 |
| generation | the prompt or the input, the model version, and the plan of an AI file, or empty | see section 3 |
| cleanup | the changes to the file after the source: the tool and the steps | Blender: new pivot and collision |

An empty field is an error, except in the two fields that the table marks "or empty" (T-2). A later rule can add a field. It cannot remove a field.

## 3. The rule for each kind of source

| Kind | Rule | Decisions |
|---|---|---|
| Original work | The owner or a session makes it for this project. The MIT License covers it. It copies no protected game content. | D-2, D-24, G-1 |
| Free license | The license is CC0 1.0 or CC BY 4.0, and no other. The file keeps its own license. A CC BY file needs the attribution of section 4. | D-24, D-119 |
| Bought asset | The same license rule as a free license: CC0 1.0 or CC BY 4.0 alone. A license that allows a private repository alone does not fit, because the repository is public. | D-24, D-119 |
| AI generation | The terms of the tool give ownership, or a license that allows redistribution. The record holds the generation field. | D-38, D-120 |
| Epic content | The project uses it from the engine folder. A copy into the repository needs a check of the terms first. | D-24 |

Notes on the rules:

- Evidence, 2026-09-29: the [Fab Standard License](https://www.fab.com/eula?lang=en) lets a licensee share content in a private repository with collaborators. The repository of this project is public (`gh repo view`). So an asset under that license cannot enter it.
- Evidence, 2026-09-29: the license of the Meshy free plan is CC BY 4.0, and Meshy owns that output (section 6). D-118 picks a paid plan, so no output of the free plan enters the repository.
- AI generation for art follows the same rule as AI generation for audio (D-38, D-120). A generated mesh still needs a cleanup before it enters the game (D-8, G-3).
- The input of an AI tool is original, or under CC0 1.0 or CC BY 4.0. An example is the image of an image to 3D task. The record names the input.
- Unknown: this session did not read the clauses of the Unreal EULA on the redistribution of Epic content in a public repository. The session that wants the first copy reads them, and records the result in this file.

## 4. Licenses that forbid redistribution or ask for attribution

This section is the rule of F-16.

**A license that forbids redistribution.** The file does not enter the repository, and it does not enter the package (D-24, D-119). The same rule applies to each Creative Commons license with NC (no commercial use), ND (no changes), or SA (share-alike). The game does not use such a file from a folder outside the repository. A clean clone must build the complete game.

**A license that asks for attribution.** CC BY 4.0 is the one such license that the policy allows (D-119). Evidence, 2026-09-29: section 3(a) of the [CC BY 4.0 legal code](https://creativecommons.org/licenses/by/4.0/legalcode.en) asks for these items:

- the name of the creator, and each other name that the licensor gives for attribution.
- the copyright notice, the license notice, and the notice of the disclaimer of warranties.
- a link to the source file, as far as it is practical.
- a statement of each change to the file.
- a statement that the file is under CC BY 4.0, with a link to the license.

The attribution field of the record holds these items. The package carries one credits file that lists the attribution of each CC BY file. Section 3(a)(2) of the legal code lets a credits file do this, if it is reasonable for the medium. Phase 5 sets the path of the credits file and the check of it.

**The MIT License and a CC BY file.** The MIT License covers the original work of the repository (D-24). A CC BY file keeps its CC BY terms, and its record states them. `LICENSE` does not change, because only the owner edits it (D-10, G-9).

## 5. The manifest and its check

The manifest is one JSON file, Game/provenance.json, beside the project file (D-121). It holds one provenance record for each file in the scope, with the path as its key.

Phase 5 builds the manifest and its check (D-121). The focused roadmap of phase 5 gives the PR id of each one (G-8). The check is a command of the tools project (D-15). The recommendation is that it fails with the path and the field in each of these cases (T-2):

- A file in the scope has no record.
- A record names a file that does not exist.
- A record has an empty field that section 2 does not permit.
- A record has a license outside section 3.
- A CC BY record has no attribution, or the credits file does not list it.

Phase 5 sets the file types in the scope of the check. Until the check exists, the author and the reviewer read each record against this policy. Phase 5 adds the records of the files that are already in the repository, for example the test map.

## 6. The Meshy choice

**The choice (D-118).** The project uses Meshy on a paid plan, for focal props alone, as the art direction asks (D-117). Examples are reliquaries, lecterns, candle stands, and chained plates. Meshy makes no kit piece, no structure, no terrain, and no first-person weapon (D-8).

- The path is a manual FBX export from Meshy, and then an import into Unreal Engine. The project does not use the Meshy plugin.
- Phase 5 holds a trial of one to three props. The owner approves the plan and the budget of the trial before any spend (D-8, G-3).
- This choice spends no money. It resolves OQ-12.

**The Meshy facts, checked 2026-09-29.** The first check was on 2026-09-26, in section 2 of `docs/research/technology-and-art-pipeline.md` (F-7).

| Fact | Source |
|---|---|
| Windows binary builds of the plugin exist for Unreal Engine 5.4 to 5.7. The download of 5.7 is plugin version 0.2.0. | [Meshy Unreal page](https://www.meshy.ai/integrations/unreal-engine) |
| The DCC Bridge needs a Meshy Pro account or above. | [Meshy Unreal page](https://www.meshy.ai/integrations/unreal-engine) |
| The project uses Unreal Engine 5.8, so no binary build of the plugin fits it. | `Game/IronAbsolution.uproject` |
| The free plan gives 100 credits each month. Its output is under CC BY 4.0, with attribution to Meshy. | [pricing docs](https://docs.meshy.ai/en/webapp/pricing) |
| The Pro plan gives 1,000 credits each month. The output of Pro and higher plans is private, with full commercial use. | [pricing page](https://www.meshy.ai/pricing), [pricing docs](https://docs.meshy.ai/en/webapp/pricing) |
| Text to 3D costs 20 credits on Meshy 6, and 25 in Ultra Mode. Meshy 7 has no text to 3D. | [pricing docs](https://docs.meshy.ai/en/webapp/pricing) |
| Image to 3D costs 25 credits on Meshy 7 and 20 on Meshy 6. A texture costs 10. A remesh, a rig, and an animation cost 0. | [pricing docs](https://docs.meshy.ai/en/webapp/pricing) |
| A failed generation costs no credits. Monthly credits reset on the first day of each month at 00:00 UTC. | [pricing docs](https://docs.meshy.ai/en/webapp/pricing) |
| On the free plan, Meshy owns the output and licenses it under CC BY 4.0. On a paid plan, the customer owns the output. | [terms of use](https://www.meshy.ai/terms-of-use), updated 2026-09-19 |
| Meshy can use the inputs and the outputs of each customer that is not Enterprise to train its models. | [terms of use](https://www.meshy.ai/terms-of-use) |
| Meshy deletes the API outputs of a customer that is not Enterprise after three days. It can delete the web outputs of an account that stays inactive. | [terms of use](https://www.meshy.ai/terms-of-use) |
| The export formats are GLB, FBX, OBJ, STL, USDZ, and 3MF. FBX has the best native Unreal support. | [export formats](https://docs.meshy.ai/en/webapp/guides/platform/export-formats) |

Changes since 2026-09-26:

- The credit costs now depend on the model version, Meshy 6 or Meshy 7. The first check gave one range for each task.
- The Pro plan now has a number of credits: 1,000 each month.
- The rest of the first check stands.

Unknowns:

- The price of each paid plan. The pricing page shows prices in cards, and the check did not get their text. The owner reads the price before the trial.
- The units, the scale, the axis, the pivot, the LODs, and the collision of an export. No Meshy page gives them. The cleanup and the import settings of phase 5 set each one.

What the facts ask from each Meshy prop:

- Export the output immediately after the generation, and put it in Git LFS (D-30). The API deletes its outputs after three days.
- Record the model version in the generation field, because the costs and the tasks differ between versions.
- Put no private data and no third-party image in a prompt or an input, because Meshy can train on each input.
