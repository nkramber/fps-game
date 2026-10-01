// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "Engine/DataAsset.h"

#include "IronMovementTuning.generated.h"

/**
 * The tuning of the movement, the camera, the mantle, and the reach of interact of the player (D-29). The field of view is a setting of
 * the player, not a value of the tuning (D-141). The player character reads each
 * value when play starts, so a new value changes the movement with no change of C++.
 * `docs/game/movement-metrics.md` gives the first values and the reasons.
 *
 * Each default is 0, so C++ holds no tuning value. The asset saves each value that differs from
 * the default, and a value that the asset does not set stays 0. FindInvalidValues reports it (T-2).
 */
UCLASS(BlueprintType)
class IRONABSOLUTION_API UIronMovementTuning : public UPrimaryDataAsset
{
	GENERATED_BODY()

public:
	/** The top speed on the ground. The player moves at this speed with no run key (D-132). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Movement", meta = (ClampMin = "1", Units = "cm/s"))
	float RunSpeed = 0.0f;

	/** The rate at which the speed goes up while the player holds a move key. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Movement", meta = (ClampMin = "1", Units = "cm/s^2"))
	float Acceleration = 0.0f;

	/** The rate at which the speed goes down on the ground after the player releases each move key. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Movement", meta = (ClampMin = "1", Units = "cm/s^2"))
	float BrakingDeceleration = 0.0f;

	/** The friction of the ground. A higher value makes a turn on the ground sharper. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Movement", meta = (ClampMin = "0.1"))
	float GroundFriction = 0.0f;

	/** The height of the feet at the top of a jump, above the ground where the jump started. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Jump", meta = (ClampMin = "1", Units = "cm"))
	float JumpHeight = 0.0f;

	/** The multiplier of the gravity of the world on the player. A higher value makes a shorter jump in time. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Jump", meta = (ClampMin = "0.1"))
	float GravityScale = 0.0f;

	/** The part of the ground control that the player keeps in the air, from 0.01 to 1. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Jump", meta = (ClampMin = "0.01", ClampMax = "1"))
	float AirControl = 0.0f;

	/** The tallest step that the player climbs with no jump. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Level metrics", meta = (ClampMin = "1", Units = "cm"))
	float StepHeight = 0.0f;

	/** The steepest slope that the player walks up. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Level metrics", meta = (ClampMin = "1", ClampMax = "89", Units = "deg"))
	float WalkableSlope = 0.0f;

	/** The height of the camera above the feet. */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Camera", meta = (ClampMin = "1", Units = "cm"))
	float EyeHeight = 0.0f;

	/**
	 * The lowest ledge top that starts a mantle, above the feet at the moment of the check (D-143).
	 * It is above the step height, so a step is never a mantle.
	 */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Mantle", meta = (ClampMin = "1", Units = "cm"))
	float MantleMinHeight = 0.0f;

	/** The highest ledge top that starts a mantle, above the feet at the moment of the check (D-143). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Mantle", meta = (ClampMin = "1", Units = "cm"))
	float MantleMaxHeight = 0.0f;

	/** The time from the start of a mantle to the feet on the ledge. The player has no control in this time (D-143). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Mantle", meta = (ClampMin = "0.01", Units = "s"))
	float MantleTime = 0.0f;

	/** The distance from the eye along the view to a switch or a door that the player can use (D-144). */
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Interact", meta = (ClampMin = "1", Units = "cm"))
	float InteractReach = 0.0f;

	/**
	 * Finds each value that is not finite or is outside its range. The ranges are the same as the
	 * clamps of the editor, so a value of 0 that the asset does not set is outside its range. The
	 * lowest mantle height must also be below the highest.
	 * @return One line for each invalid value, with the name of the value, the value, and the range. Empty when each value is valid.
	 */
	TArray<FString> FindInvalidValues() const;
};
