// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"

#include "IronWeaponTuning.generated.h"

class AIronHitFlash;
class USoundBase;
class UStaticMesh;

/**
 * The tuning and the content of one weapon of the weapon rule (D-29, D-128). The weapon component
 * holds the rule: the hitscan shot, the ammo, and change weapon. Each data asset gives one weapon,
 * so a new asset makes a new weapon with no change of C++.
 * `docs/game/weapon-tuning.md` gives the first values and the reasons.
 *
 * Each number defaults to 0, so C++ holds no tuning value. A value that the asset does not set stays
 * 0, and FindInvalidValues reports it (T-2). Two values can be 0: SpreadAngle for a weapon with one
 * pellet that goes along the view, and ShotSoundStartTime for a shot sound with no silence at its start.
 */
UCLASS(BlueprintType)
class IRONABSOLUTION_API UIronWeaponTuning : public UPrimaryDataAsset
{
	GENERATED_BODY()

public:
	/** The name of the weapon on the HUD. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Weapon")
	FText DisplayName;

	/**
	 * True when a held fire key fires at the rate of fire. False when each press of the fire key
	 * fires one shot (D-151).
	 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Fire")
	bool bAutomatic = false;

	/** The highest rate of fire, in shots each second. A press within the interval after a shot does not fire. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Fire", meta = (ClampMin = "0.1"))
	float ShotsPerSecond = 0.0f;

	/** The rounds of a full weapon. Each shot spends one round, for each count of pellets (D-115). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Fire", meta = (ClampMin = "1"))
	int32 AmmoCapacity = 0;

	/** The length of the trace of each pellet from the eye, along the view (D-130). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Fire", meta = (ClampMin = "1", Units = "cm"))
	float Range = 0.0f;

	/** The traces of one shot. Each pellet goes to its own point of the cone of the spread (D-153, D-160). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Fire", meta = (ClampMin = "1"))
	int32 PelletCount = 0;

	/**
	 * The half angle of the cone around the view. Each pellet goes to a random point of the cone,
	 * new for each shot, with an even spread over the cone (D-160). At 0, each pellet goes along the
	 * view. With more than one pellet, it must be above 0.
	 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Fire", meta = (ClampMin = "0", ClampMax = "45", Units = "deg"))
	float SpreadAngle = 0.0f;

	/** The time from a change of weapon to the first shot of this weapon (D-157). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Change weapon", meta = (ClampMin = "0.01", Units = "s"))
	float RaiseTime = 0.0f;

	/** The turn of the view up at each shot (D-159). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Recoil", meta = (ClampMin = "0.01", ClampMax = "45", Units = "deg"))
	float RecoilKick = 0.0f;

	/** The largest turn of the view to the left or the right at each shot. Each shot takes a random turn up to it (D-161). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Recoil", meta = (ClampMin = "0.01", ClampMax = "45", Units = "deg"))
	float RecoilSideKick = 0.0f;

	/**
	 * The time in which the view comes back from the kick, from any height (D-162). Each shot sets
	 * a new rate of return, so the view is back at the aim of the player this time after the last shot.
	 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Recoil", meta = (ClampMin = "0.01", Units = "s"))
	float RecoilRecoveryTime = 0.0f;

	/** The sound of each shot (D-155). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Feedback")
	TObjectPtr<USoundBase> ShotSound;

	/**
	 * The time into the shot sound at which each shot starts it. A file with silence at its start
	 * then plays at the moment of the shot (D-165). It must be below the length of the sound.
	 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Feedback", meta = (ClampMin = "0", Units = "s"))
	float ShotSoundStartTime = 0.0f;

	/** The sound of a press of the fire key on an empty weapon (D-158). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Feedback")
	TObjectPtr<USoundBase> EmptySound;

	/** The flash at the point of each hit on a surface (D-154). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Feedback")
	TSubclassOf<AIronHitFlash> HitFlashClass;

	/** The mesh of the weapon in the view, from the basic shapes of the engine. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "View")
	TObjectPtr<UStaticMesh> ViewMesh;

	/** The scale of the view mesh. Each part must be above 0. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "View")
	FVector ViewScale = FVector::ZeroVector;

	/** The place of the view mesh from the camera: forward in X, to the right in Y, and up in Z. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "View", meta = (Units = "cm"))
	FVector ViewOffset = FVector::ZeroVector;

	/**
	 * Gives the time from one shot to the next shot at the highest rate of fire.
	 * @return 1 divided by ShotsPerSecond. Call it only on a tuning with no invalid value.
	 */
	float GetShotInterval() const;

	/**
	 * Finds each value that is absent, not finite, or outside its range. The ranges are the same as
	 * the clamps of the editor. The spread must agree with the count of pellets.
	 * @return One line for each invalid value, with the name of the value, the value, and the rule. Empty when each value is valid.
	 */
	TArray<FString> FindInvalidValues() const;
};
