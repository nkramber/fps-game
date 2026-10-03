// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"

#include "IronMeleeTuning.generated.h"

class AIronHitFlash;

/**
 * The tuning and the content of the melee attack (D-29, D-116). The melee component holds the rule:
 * the sphere sweep along the view, the time between two attacks, and the jab of the weapon in the
 * view. `docs/game/weapon-tuning.md` gives the first values and the reasons.
 *
 * Each number defaults to 0, so C++ holds no tuning value. A value that the asset does not set stays
 * 0, and FindInvalidValues reports it (T-2).
 */
UCLASS(BlueprintType)
class IRONABSOLUTION_API UIronMeleeTuning : public UPrimaryDataAsset
{
	GENERATED_BODY()

public:
	/** The farthest reach of the attack from the eye, along the view. The front of the sphere stops at this distance. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Attack", meta = (ClampMin = "1", Units = "cm"))
	float Range = 0.0f;

	/** The radius of the sphere of the sweep, so a hit needs no exact aim. It must be below the range. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Attack", meta = (ClampMin = "1", Units = "cm"))
	float SweepRadius = 0.0f;

	/** The time from one attack to the next. A press within this time after an attack makes no attack. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Attack", meta = (ClampMin = "0.01", Units = "s"))
	float AttackInterval = 0.0f;

	/** The distance that the weapon in the view moves forward at the middle of the jab. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Jab", meta = (ClampMin = "0.1", Units = "cm"))
	float JabDistance = 0.0f;

	/**
	 * The time of the jab, out and back. The weapon fires no shot in this time. It must not be above
	 * the time between two attacks, so each jab ends before the next one starts.
	 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Jab", meta = (ClampMin = "0.01", Units = "s"))
	float JabTime = 0.0f;

	/** The flash at the point of a hit on a surface, the same as the flash of a shot (D-154). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Feedback")
	TSubclassOf<AIronHitFlash> HitFlashClass;

	/**
	 * Finds each value that is absent, not finite, or outside its range. The ranges are the same as
	 * the clamps of the editor. The radius must be below the range, and the jab must end before the
	 * next attack.
	 * @return One line for each invalid value, with the name of the value, the value, and the rule. Empty when each value is valid.
	 */
	TArray<FString> FindInvalidValues() const;
};
