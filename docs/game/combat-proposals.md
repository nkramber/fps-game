# Combat proposals: the limited resources and the recovery mechanic

Status: the proposals of PR-16 for OQ-10 (D-36, D-111). The owner picked none of them. The combat rules follow Doom (2016) (D-115). This file keeps the four proposals as the record of the choice. Written 2026-09-29 in ASD-STE100 (D-17). The pillars and the core loop are in `docs/game/pillars.md`.

This file gives four original proposals. PR-16 first gave three. The owner then asked for a fourth with the push-forward rhythm of Doom (2016), with original rules. Each one names the limited resources, the action that gives them back in a fight, and a first test for the sandbox of phase 4. The owner picks one during PR-16 (D-113). The section "The pick" records the answer.

Labels: each claim is evidence (with a source), a recommendation, an assumption, or an unknown. No proposal sets a number. PR-19 gives the roster sizes, and phase 4 tunes the values.

## Terms

| Term | Use for | Do not use |
|---|---|---|
| resource | a quantity that the player spends and must get back: health, ammo, or iron | currency, stat |
| round | one unit of ammo that one shot spends | bullet, shell, when the text means this |
| recovery mechanic | the rule that gives resources back to the player in a fight | sustain, economy |
| kill | the final damage that removes an enemy from the fight | frag, takedown |
| combat space | one area of the level that holds one fight | arena, when the text means the level |
| press range | the short distance from the player inside which a kill gives health (proposal D) | melee range, close range |
| sandbox | the test arena of phase 4 | test map, when the text means this |

## Proposal A: Reclaim

**The limited resources.**

- Health. Each hit on the player spends health. Health does not come back by time.
- Ammo, one supply for each weapon. Each shot spends one round or more. Ammo does not come back by time.

**The action that gives resources back.**

- Each round that hits an enemy stays in that enemy. The enemy holds a count of the rounds of each weapon that hit it.
- When the player kills the enemy, each round that it holds flies back to the weapon that fired it.
- A round that misses does not come back.
- When a weapon is full, each round that comes back to it becomes a small quantity of health.

**The tension, and how it rewards attack.**

- A kill is the only source of ammo in a fight. A hit that does not lead to a kill gives nothing back.
- The ammo of the player is inside the enemies. A wounded enemy that escapes, or a switch to a new target, leaves rounds in the field.
- Assumption: this pushes the player to finish each target and to aim well. Both fit the pillar "Attack pays".
- Assumption: health from a full weapon rewards a player who spends little. Such a player gets health for free play.

**The risks.**

- Assumption: a player with low health and low ammo cannot get health back, because no weapon is full. This can make a spiral to death. The first test measures it.
- Unknown: the rule for a weapon with area damage. A blast can hit five enemies with one round. Phase 4 decides whether each enemy holds that round.
- Assumption: an enemy that the player cannot finish holds rounds. This is a risk, and also a tool for the design of an encounter.
- Assumption: the player must see each round fly back, and must hear it. Without a clear cue, the mechanic is invisible.

**The cost.**

- Rule code. Recommendation: one component on each enemy that counts the rounds of each weapon. Also one rule that returns them on the kill, and one that makes health from a full weapon. Assumption: this is a small part of the rule code of phase 4.
- Content. One visual effect for the return, one sound, and one cue on the display for the health from a full weapon.
- Assumption: the mechanic works with one weapon. So the one weapon of phase 3 can give an early check of the feel.

**The first test for the sandbox.**

- Recommendation: one combat space, one weapon, and eight enemies of one kind. The space holds no pickup. The start ammo is less than the ammo that the eight kills need.
- The test records the rounds that the player fires, the rounds that come back, and the rounds that stay in enemies at the end.
- It also records the health at the end, the count of deaths, and the health from a full weapon.
- The owner rates the tension. A second run adds a fast enemy that escapes, to test the rounds that stay in the field.

**Originality (D-2, G-1).**

- It is not like Doom (2016) or its sequel. No melee finish, no chainsaw, and no pickup that a stunned enemy drops.
- It is not like the arrow pickup of a game with a bow. The return is automatic, only on the kill, and only the rounds that hit come back.
- It is not like Ultrakill. Health does not come from blood near the player.
- Unknown: the session knows no shooter that gives back the rounds that hit on the kill. The review checks this claim.

## Proposal B: Relay

**The limited resources.**

- Health. Each hit on the player spends health. Health does not come back by time.
- One magazine for each weapon, with no reserve and no reload. Each shot spends one round or more. Assumption: the player carries three weapons or more.

**The action that gives resources back.**

- A kill with one weapon refills a part of the magazine of each other weapon. It does not refill the weapon that made the kill.
- A kill with the last round of a magazine, a closing shot, gives back health.

**The tension, and how it rewards attack.**

- No weapon can hold a fight alone. Each kill with one weapon gives ammo to the others, so the player changes weapon often.
- The closing shot asks the player to empty a weapon on purpose, on a target that dies. This rewards a player who counts rounds and attacks.
- Assumption: the fight becomes a rotation of weapons with a clear rhythm. This fits the pillars "Attack pays" and "Fast and exact".

**The risks.**

- The display must show each magazine at the same time. Assumption: three bars or more are hard to read at speed.
- The mechanic needs three weapons or more. Phase 3 builds one weapon, so the first check of the feel waits for phase 4.
- Each weapon must work against each enemy. A weapon that is weak against one kind breaks the rotation.
- Assumption: a closing shot is hard to aim for with an automatic weapon.

**The cost.**

- Rule code. Recommendation: one event on each kill that names the weapon, one refill of the other magazines, and one check of the last round. Assumption: this is small.
- Content. A display for each magazine, and a cue for the closing shot.
- Assumption: the weapon roster of phase 4 must reach three weapons before the first test. That moves weapon content earlier.

**The first test for the sandbox.**

- Recommendation: one combat space, three weapons, and twelve enemies of two kinds. The space holds no pickup. Each magazine starts at half.
- The test records the changes of weapon for each minute, the kills of each weapon, and the closing shots.
- It also records the health that the closing shots give back.
- It records the count of deaths. The owner rates the tension and the load on the eyes of the display.

**Originality (D-2, G-1).**

- It is not like Doom Eternal. There, the change of weapon serves damage and weak points, and a chainsaw gives ammo. Here, the change of weapon is the source of ammo.
- It is not like the rally of Bloodborne. There, each hit after damage gives health back. Here, only a closing shot gives health.
- Unknown: the session recalls a weapon perk of Destiny 2 that fills one weapon after damage with other weapons. The session did not check the source. The review checks this claim.

## Proposal C: Iron

**The limited resources.**

- One pool, iron, for health and ammo. Each shot spends iron, and each hit on the player spends iron. At zero iron, the player dies.
- Each weapon has its own cost of iron for each shot. Iron does not come back by time.

**The action that gives resources back.**

- Each kill gives iron back. The quantity depends on the kind of enemy.
- Recommendation: an efficient kill gives more iron than it spends. A miss or a hit on the player is the only loss.

**The tension, and how it rewards attack.**

- Each shot costs a little life. A player who kills well grows the pool, and a player who waits or misses loses it.
- The player reads one bar, so the state of the fight is clear at a glance.
- Assumption: the title of the game gets a mechanic of its own. The owner decides whether this matters.

**The risks.**

- Assumption: a player with low iron shoots less, takes more damage, and loses more iron. This is a strong spiral to death.
- Assumption: a player can fear to shoot, because each shot costs life. This works against the pillar "Fast and exact".
- The cost of iron for each shot is the only difference in resource between weapons. Assumption: the weapons of a larger roster then feel alike.
- Assumption: the tuning is sensitive. A small change of one value changes each fight.

**The cost.**

- Rule code. Recommendation: one pool with a cost for each shot, each hit, and each kill. Assumption: this is the smallest of the three.
- Content. One bar on the display, and a cue for the iron from each kill.

**The first test for the sandbox.**

- Recommendation: one combat space, two weapons with different costs of iron, and ten enemies of one kind. The pool starts at half.
- The test records the iron over time, the count of deaths, and the shots for each minute.
- A second run gives the same space with separate health and ammo. Assumption: fewer shots in the first run show the fear to shoot.

**Originality (D-2, G-1).**

- It is not like Bloodborne. There, the player changes health into bullets with an action. Here, health and ammo are one pool, and no action changes one into the other.
- It is not like Doom (2016). No melee finish and no dropped pickup give iron back.
- Unknown: a shooter with one pool for health and ammo can exist. The session did not search. The review checks this claim.

## Proposal D: Press

This proposal aims for the push-forward rhythm of Doom (2016): a hurt player attacks to get health back. Its rules are original (D-2).

**The limited resources.**

- Health. Each hit on the player spends health. Health does not come back by time.
- Ammo, one supply for each weapon. Each shot spends one round or more. Ammo does not come back by time.

**The action that gives resources back.**

- Ammo follows the rule of proposal A. Each round that hits an enemy comes back to its weapon when the player kills that enemy. A miss does not come back.
- Health comes from a kill inside the press range. Any weapon can make that kill with a normal shot.
- The reticle shows when the target is inside the press range. Outside it, a kill gives ammo back but no health.
- Proposal D has no health from a full weapon. The press range replaces that rule.

**The tension, and how it rewards attack.**

- A hurt player must move toward the enemies, not away. Each step forward adds risk and also the only source of health.
- A player with ammo but low health pushes in for the kill. A player with health but low ammo finishes wounded targets from any range.
- Assumption: the fight gets a rhythm of attack from range and a push in to finish. This fits the pillars "Attack pays" and "Fast and exact".

**The risks.**

- Assumption: the damage near an enemy can be more than the health from its kill. Then the push forward loses health. The first test measures it.
- Assumption: an enemy that hits hard at close range makes the press range a trap. Each enemy kind needs a clear window to press in.
- Unknown: the damage of a blast weapon to the player inside the press range. Phase 4 decides whether the player takes damage from a blast of the player.
- Assumption: the size of the press range is the most sensitive value. It needs a check in the sandbox before the other values.

**The cost.**

- Rule code. Recommendation: the count of rounds from proposal A, plus one check of distance on each kill. Assumption: this is small.
- Content. The return effect and the sound of proposal A. Also one cue on the reticle for the press range, and one cue for the health from a kill.
- Assumption: the mechanic works with one weapon. So the gym of phase 3 can give an early check.

**The first test for the sandbox.**

- Recommendation: one combat space, one weapon, and eight enemies of two kinds. One kind attacks at close range, and one attacks from far away.
- The space holds no pickup. The player starts with half health and less ammo than the eight kills need.
- The test records the time inside the press range, the health from each kill, and the damage inside the press range.
- It also records the count of deaths.
- A second run doubles the press range. The owner rates the tension of the two runs.

**Originality (D-2, G-1).**

- It is not like Doom (2016). No stun state, no melee finish, no animation of a finish, no time without damage, and no chainsaw. The kill is a normal shot.
- It is not like Ultrakill. There, blood from damage near the player heals. Here, only a kill heals, and each kill gives a set quantity.
- It is not like Doom Eternal. No fire gives armor, and no chainsaw gives ammo.
- Unknown: a shooter can give health for each kill at a short distance. The session did not search. The review checks this claim.

## Comparison

| Part | A: Reclaim | B: Relay | C: Iron | D: Press |
|---|---|---|---|---|
| Resources | health, ammo for each weapon | health, one magazine for each weapon | one pool for both | health, ammo for each weapon |
| Recovery | the rounds that hit come back on the kill | a kill refills the other weapons, a closing shot gives health | each kill gives iron | the rounds that hit come back on the kill, a kill in the press range gives health |
| Reward for attack | finish each target, aim well | change weapon, count rounds | kill well | finish each target, push in when hurt |
| Main risk | spiral with low health and low ammo | the display, and the need for three weapons | spiral, and fear to shoot | the press range as a trap |
| Rule code | small | small | smallest | small |
| First check of the feel | phase 3, with one weapon | phase 4, with three weapons | phase 3, with one weapon | phase 3, with one weapon |

## Recommendation

Recommendation: proposal D, Press.

- It gives the push-forward rhythm that the owner asked for, with original rules.
- A kill is the only source of ammo, and a kill in the press range is the only source of health in a fight. Both reward attack.
- It removes the main risk of proposal A. A hurt player with low ammo can still get health, because a kill in the press range needs no full weapon.
- It works with one weapon, so the gym of phase 3 can give an early check.
- Proposal B asks for three weapons before any test, and a heavy display. Proposal C works against the pillar "Fast and exact".

## The pick

The owner picked during PR-16 (D-113). The pick is none of the four proposals. The combat rules follow Doom (2016) (D-115). D-115 resolves OQ-10.

Correction of 2026-09-29: an original harvest tool replaces the chainsaw, with the same rule (PR-19, D-125). This section keeps the word chainsaw as history. The first test for the sandbox uses the harvest tool. Phase 4 proposes its kind (OQ-24).

The owner gave three answers in turn:

1. "Same rules as Doom 2016". The session then showed the conflict with D-2 and G-1.
2. "Keep D-2: similar feel". The session then wrote proposal D.
3. "Nah, back to Doom 2016. Use those mechanics." The owner then confirmed the text of D-115.

### The rules of D-115

Evidence: the Gameplay section of the article "Doom (2016 video game)" of Wikipedia, read on 2026-09-29 (https://en.wikipedia.org/wiki/Doom_(2016_video_game)).

- An enemy with enough damage goes into a stunned state. The player can then kill it with a short melee finish. The finish gives health.
- A chainsaw kills an enemy at once when it has enough fuel. The kill gives ammo.
- The player also gets resources from pickups and from kills.
- No weapon needs a reload.

Unknown: the article gives no numbers. It does not give the fuel cost of each enemy, the quantity of each drop, or the time of the stunned state. Phase 4 sets each value in the sandbox. The session did not read a second source, because two wiki sites refused the request.

### What stays original

D-115 revises D-2 and G-1 for the combat rules alone. These parts stay original:

- The names in the game. The project does not use the names of Doom (2016) for the finish, the chainsaw, the resources, or the enemies.
- The art and the audio of each weapon, enemy, pickup, and animation of the finish.
- The look and the sound of each cue, for example the mark of a stunned enemy.
- The layouts of the level.

Recommendation: the review of each PR of phase 4 checks these four parts. The rules can match Doom (2016). The expression of the rules must not.

### The first test for the sandbox

Recommendation: one combat space, one weapon, the chainsaw with fuel for two kills, and eight enemies of two kinds. The space holds no pickup. The player starts with half health and less ammo than the eight kills need.

- The test records the finishes, the health from each finish, the chainsaw kills, and the ammo from each one.
- It also records the count of deaths and the time of the fight.
- The owner rates the push forward, and compares it with the pace of Doom (2016).
