// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"

#include "IronMeleeComponent.generated.h"

class UIronMeleeTuning;

/**
 * The melee attack of phase 3: a close attack at short range (D-116). A data asset holds the range,
 * the time between two attacks, and the jab (D-29).
 *
 * An attack is a sphere sweep from the eye of the owner along the view, on the weapon channel, so a
 * hit needs no exact aim. The first blocking hit takes the attack. A hit on a surface makes the flash
 * of a shot, and a hit on a gym target sets the time of the hit marker (D-154). The weapon in the
 * view jabs forward on each attack, a hit or a miss.
 *
 * Phase 4 adds the finish of a stunned enemy to HitActor, the one place that gives an attack to the
 * actor that it hit (D-115, D-116). The sweep, the time between two attacks, and the jab stay.
 *
 * The owner gives the tuning with SetTuning. The player character does this when play starts, from
 * its Blueprint subclass.
 */
UCLASS()
class IRONABSOLUTION_API UIronMeleeComponent : public UActorComponent
{
	GENERATED_BODY()

public:
	UIronMeleeComponent();

	/**
	 * Takes a new tuning. The next attack can start at once.
	 * @param NewTuning The data asset of the attack.
	 * @return True when the tuning is valid. False after an error line for each problem, with no change.
	 */
	bool SetTuning(UIronMeleeTuning* NewTuning);

	/**
	 * Makes one attack, when the time between two attacks is over since the last attack.
	 * @return True when the attack happened, a hit or a miss. False before the end of the time between two attacks, and false after an error line with no tuning.
	 */
	bool Attack();

	/** Gives the tuning, or null before SetTuning. */
	const UIronMeleeTuning* GetTuning() const;

	/** Gives the time of the world of the last attack that hit a gym target, or no value before the first such attack. */
	TOptional<double> GetLastTargetHitTime() const;

	/** Gives the distance forward of the weapon in the view now: 0 at the start and the end of the jab, and the full jab distance at its middle. */
	float GetJabDistance() const;

private:
	/** Gives the attack to the actor that it hit. A gym target counts it as a melee hit. */
	void HitActor(AActor& Actor);

	/** The tuning of the attack. SetTuning sets it. */
	UPROPERTY(Transient)
	TObjectPtr<UIronMeleeTuning> Tuning;

	/** The time of the world of the last attack, or no value before the first attack. */
	TOptional<double> LastAttackTime;

	TOptional<double> LastTargetHitTime;
};
