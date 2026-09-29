# Art proposals: the setting, the tone, and the visual style

Status: the proposals of PR-17 for OQ-9 (D-36, D-111). Written 2026-09-29 in ASD-STE100 (D-17). The owner picked proposal A, Penitent Iron (D-117). The section "The pick" records the answer.

This file gives three original proposals. Each one gives the setting, the tone, and the visual style. It also gives the content cost, the fit with the frame budget, the use of Meshy, and a note on originality. The pillars and the core loop are in `docs/game/pillars.md`. The combat rules are in D-115.

The proposals use words alone. No image of another game or artist goes into the repository (D-2, D-24). The hex values of each palette are a start for phase 5, not final values.

Labels: the setting, the tone, and the style of each proposal are design proposals. Each claim on cost, budget, Meshy, or originality is evidence (with a source), a recommendation, an assumption, or an unknown.

Evidence for each note on originality: Doom (2016) takes place in a facility on Mars and in Hell, with demons as enemies. The facility has a foundry. Source: the article "Doom (2016 video game)" of Wikipedia, read on 2026-09-29 (https://en.wikipedia.org/wiki/Doom_(2016_video_game)).

Assumption: a stunned enemy in Doom (2016) glows blue, then orange. The article does not state the color, and the session found no other source.

## Terms

| Term | Use for | Do not use |
|---|---|---|
| setting | the place and the world of the level | lore, universe |
| tone | the mood that the level gives the player | vibe, feel, when the text means this |
| visual style | the shapes, the materials, the palette, and the light mood | art style, aesthetic |
| palette | the small set of colors that the level uses | color scheme |
| signal color | a color that only a combat cue, a pickup, or a threat uses | accent, highlight |
| cue | a sign in the game that tells the player about a state, for example a stunned enemy | tell, indicator |
| kit | the set of modular meshes that builds the structure of the level | tileset |
| trim sheet | one texture that holds many strips of detail for many meshes | atlas, when the text means this |

## What the pillars ask from the art

Each proposal answers these four needs. They come from the pillars of `docs/game/pillars.md` and from the combat rules of D-115.

- "Fast and exact": each enemy has a clear silhouette against the level at speed. A threat off the screen has a cue.
- "Every round counts": the player sees each pickup and each resource at a glance. Only pickups and cues use the signal colors.
- The stunned state of D-115 needs a cue that the player reads in less than one second. Its look and its sound stay original (D-115).
- "One level, complete": one level of 30 minutes or more needs variety of spaces (D-37). The kit must give that variety at a low cost (F-15).

## Proposal A: Penitent Iron

**The setting and the tone.**

A monastery of cold iron stands on a sea cliff at dawn. An old order sealed the confessions of the penitent into cast iron plates, and stacked them in vaults under the cliff. The iron woke, and each plate became a body. The player is the last warden of the vaults, and descends to silence them. The tone is solemn and severe. It is quiet and cold between fights, and loud in each fight.

Fit with the pillars: the enemies are hollow iron shells, so a stunned enemy shows a clear crack. The chainsaw cuts iron, and the fiction supports ammo from the kill. The descent gives one direction for the level, and each floor gives a new space.

**The visual style.**

- Shapes: tall narrow arches, ribbed vaults of riveted iron beams, and stacked walls of iron plates with stamped script. Vertical lines dominate.
- Materials: dark cast iron, pale limestone, old wax, chain, and salt crust near the sea. No rust red, no molten metal, and no fire as a theme.
- Palette: cold blue-gray stone and iron. Pale gold is the signal color for pickups. A white-violet crack is the cue of a stunned enemy.
- Light mood: shafts of cold daylight through high slit windows at the top floors. Candle clusters and lamps of wax deeper down. The dawn light fades with each floor.

| Role | Color | Hex |
|---|---|---|
| Base stone | pale blue-gray limestone | `#9BA3AB` |
| Base iron | cold dark iron | `#2E3338` |
| Shadow | deep slate | `#14181C` |
| Signal: pickup | pale gold | `#E8C66A` |
| Signal: stunned enemy | white-violet | `#C9B8FF` |
| Signal: danger | cold cyan | `#5FE0E6` |

**The content cost.**

- Assumption: one architecture kit of arches, pillars, vault modules, stairs, and plate walls, with two trim sheets. Arches and vaults need more pieces than straight walls.
- Assumption: the variety of spaces comes from three zones on the same kit. They are the cliff cloister in daylight, the scriptorium in candle light, and the deep vaults in dark.
- Assumption: the plate walls with stamped script repeat well, because the script is a decal on a trim sheet.
- Assumption: the content cost is medium. Curved pieces and ornament cost more than the plain blocks of proposal B.

**The fit with the budget (D-32).**

- Assumption: the kit, a few master materials, and small props fit 120 fps at 1440p on the RTX 4090 with room to spare.
- Unknown: many candle lights in one space cost more with dynamic light. Phase 5 chooses the light method by a measurement (M-5).
- Assumption: the fog in the light shafts is a volumetric cost. Phase 5 measures it in the room of M-5.

**What it asks from Meshy (OQ-12, D-8).**

- Recommendation: focal props alone, for example reliquaries, lecterns, candle stands, and chained plates. No kit piece and no structure (D-8).
- Assumption: a style guide with the palette and the materials keeps generated props close to the kit. Each prop still needs a cleanup.

**Originality (D-2, G-1).**

- It is not like Doom (2016). No Mars, no research base, no hell, no demons, no fire and rock, no rune of a pentagram, and no heavy metal look.
- It has no foundry and no molten metal, although Doom (2016) has a foundry.
- The cue of a stunned enemy is a white-violet crack in an iron shell. It is not a glow on a demon.
- Risk: a gothic monastery is common in dark fantasy games. The iron plate bodies and the cold sea light are the original parts. The review checks this claim.
- Unknown: the session did not search for a game with iron plate bodies that hold confessions.

## Proposal B: Spillway

**The setting and the tone.**

A vast flood barrier of concrete stands across a river mouth at night, in a storm. The operators drained its basins for repair, and something climbed up from the deep sluices. The player is an engineer of the night shift who must reach the control hall and close the barrier. The tone is tense, cold, and lonely. The storm is loud outside, and the concrete halls are silent inside.

Fit with the pillars: the barrier gives many different spaces from one material: basins, turbine halls, gantries, tunnels, and control rooms. The route goes along the barrier and down into it, so the level has one clear goal.

**The visual style.**

- Shapes: massive blocks of concrete, repeated ribs, large round pipes, steel gantries, and grates. Horizontal lines dominate.
- Materials: wet concrete with form marks, galvanized steel, painted stencils, rubber seals, and pools of shallow water.
- Palette: cool concrete grays. Sodium amber is the light of the work areas. Signal green marks pickups. A pulse of white-pink marks a stunned enemy.
- Light mood: night, with sodium lamps in rows and white floodlights on the gantries. Lightning outside gives short bright moments. The inner halls are dark between the lamps.

| Role | Color | Hex |
|---|---|---|
| Base concrete | cool wet gray | `#7D8285` |
| Base steel | galvanized blue-gray | `#5A6770` |
| Shadow | near black | `#101314` |
| Work light | sodium amber | `#F2A541` |
| Signal: pickup | signal green | `#7CFF6B` |
| Signal: stunned enemy | white-pink pulse | `#FFB8D9` |

**The content cost.**

- Assumption: one concrete kit of blocks, ribs, stairs, and tunnels, and one steel kit of gantries, rails, and grates. Each kit has one trim sheet. Straight pieces cost less than curved pieces.
- Assumption: the variety of spaces comes from the scale of each space and from its lamps, not from new kit pieces.
- Assumption: the structure cost is the lowest of the three proposals. The storm adds a cost of effects: rain, splashes, lightning, and wet surfaces.
- Assumption: the total content cost is low to medium.

**The fit with the budget (D-32).**

- Assumption: the kit fits 120 fps at 1440p on the RTX 4090.
- Unknown: rain particles, water surfaces, and the reflections of wet surfaces cost frame time with Lumen. Phase 5 measures them (M-5).
- Recommendation: keep the rain outside and in a few open spaces, and keep the water inside shallow and still.

**What it asks from Meshy (OQ-12, D-8).**

- Recommendation: focal props alone, for example valves, control panels, pumps, and tool carts. No kit piece and no structure (D-8).
- Assumption: industrial props are a common subject, so generated props need less cleanup of the style.

**Originality (D-2, G-1).**

- It is not like Doom (2016). No research base on Mars, no hell, no demons, and no red rock. The industry is civil work on Earth, not a science base.
- The cue of a stunned enemy is a white-pink pulse, not a blue or orange glow (see the assumption on the cue color).
- Risk: large brutalist concrete halls are common in some recent games. The flood barrier, the storm, and the drained basins are the original parts. The review checks this claim.
- Unknown: the session did not search for a shooter set on a flood barrier.

## Proposal C: Tribunal

**The setting and the tone.**

A court of black iron hangs in a white void. Its tiers rise like a stair of judgment, and each tier holds one sentence. The player is a condemned figure of iron who fights up through the tiers to earn absolution at the top. The tone is mythic and stark. The world has few details, and each detail has a meaning.

Fit with the pillars: a stylized look gives the best read of silhouettes and cues at speed. Each tier is one combat space, so the structure of the level is clear to the player.

**The visual style.**

- Shapes: large geometric forms, stepped tiers, thin columns, and oversized symbols of a court: scales, chains, and seats of judges. The silhouette of each shape is simple.
- Materials: flat black iron with a hard white edge light, and white stone with almost no texture. Detail comes from shape and edge, not from texture.
- Palette: two base values, black and bone white. Red is the signal color of enemies and danger. Gold marks pickups. Blue marks a stunned enemy.
- Light mood: flat bright light from the void, and a hard edge light on each form. Each tier changes the angle of the light and the color of the void.

| Role | Color | Hex |
|---|---|---|
| Base form | flat black iron | `#161616` |
| Base void | bone white | `#EDE8DC` |
| Edge light | pure white | `#FFFFFF` |
| Signal: enemy and danger | court red | `#D63A2F` |
| Signal: pickup | gold | `#F2C230` |
| Signal: stunned enemy | cold blue | `#3F8CFF` |

**The content cost.**

- Assumption: one kit of simple forms with no trim sheet. Most surfaces need a flat material and an edge effect alone.
- Assumption: the cost of each asset is the lowest of the three proposals. But a stylized look needs a strict style guide, and each off-style asset breaks the whole look.
- Assumption: the variety of spaces comes from the shape of each tier and from the color of the void.
- Assumption: the content cost is low, and the cost of the art direction is high.

**The fit with the budget (D-32).**

- Assumption: flat materials and simple forms give the most room in the frame budget.
- Unknown: the edge effect needs a post-process material or a custom shader. Phase 5 measures its cost.

**What it asks from Meshy (OQ-12, D-8).**

- Assumption: Meshy output has texture detail that does not fit a flat style. Each generated prop needs a new flat material and a cleanup of its shape.
- Recommendation: do not use Meshy for this proposal, or use it for rough shapes alone.

**Originality (D-2, G-1).**

- It is not like Doom (2016). No realistic look, no hell, no demons, and no science base.
- Blue is the cue of a stunned enemy here. If the assumption on the cue color is true, Doom (2016) also uses blue. Recommendation: phase 4 gives the cue an original shape and sound, not only a color (D-115).
- Risk: a stylized shooter in black, white, and red resembles some recent games. The court of iron in a void is the original part. The review checks this claim.
- Unknown: the session did not search for a shooter set in a court of judgment.

## Comparison

| Part | A: Penitent Iron | B: Spillway | C: Tribunal |
|---|---|---|---|
| Setting | iron monastery on a sea cliff | concrete flood barrier in a storm | iron court in a white void |
| Tone | solemn, severe | tense, lonely | mythic, stark |
| Look | realistic, gothic, cold | realistic, brutalist, wet | stylized, flat, graphic |
| Kits | one kit, two trim sheets | two kits, one trim sheet each | one kit of simple forms |
| Content cost | medium | low to medium, plus storm effects | low, with a high cost of style |
| Main budget risk | many small lights, fog | rain, water, wet reflections | the edge effect |
| Meshy | focal props | focal props | little or none |
| Read at speed | good, with the signal colors | good, with the lamps | best |
| Fit with the title | strong | weak | strong |

## Recommendation

Recommendation: proposal A, Penitent Iron.

- It fits the working title and the combat rules. Iron bodies crack when stunned, and the chainsaw cuts iron (D-115).
- One kit with trim sheets makes the whole level, as OQ-9 recommends. The three zones give variety of spaces at a low cost (F-15).
- Its look is far from Doom (2016). It has no fire, no hell, and no red.
- Proposal B costs less in structure, but it adds storm effects and fits the title less. Proposal C costs least in assets, but one off-style asset breaks it.

## The pick

The owner picked proposal A, Penitent Iron, during PR-17 (D-113). D-117 records the pick and resolves OQ-9. The owner took the recommendation with no change.

The pick binds these later items:

- PR-18 gives the Meshy choice for focal props of this style (OQ-12, D-8).
- PR-19 sizes the level brief for one kit and three zones: the cloister, the scriptorium, and the vaults.
- Phase 5 sets the kit grid, the light method (M-5), and the final palette values.
- Phase 4 gives the stunned cue an original look and sound: the white-violet crack (D-115).

Correction of 2026-09-29: an original harvest tool replaces the chainsaw, with the same rule (PR-19, D-125). The proposal text above keeps the word chainsaw as history. The fit with the fiction stays: a kill of an iron body gives ammo.
