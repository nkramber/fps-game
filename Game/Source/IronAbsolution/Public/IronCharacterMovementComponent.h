// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/CharacterMovementComponent.h"

#include "IronCharacterMovementComponent.generated.h"

/** The custom movement modes of the player, for `MOVE_Custom` of the movement component. */
UENUM()
enum class EIronCustomMovementMode : uint8
{
	/** The player climbs onto a ledge, first up, then forward (D-143). */
	Mantle = 0,
};

/** A ledge that the player can climb, and the path of the climb in world space. */
struct FIronLedge
{
	/** The height of the ledge top above the feet at the moment of the check, in cm. */
	float Height = 0.0f;

	/** The center of the capsule at the top of the vertical part of the climb. */
	FVector RiseLocation = FVector::ZeroVector;

	/** The center of the capsule on the ledge, at the end of the climb. */
	FVector StandLocation = FVector::ZeroVector;
};

/**
 * The movement component of the player. It adds the mantle to the movement of the engine (D-116,
 * D-143). While the player is in the air and moves forward into a ledge in the band of the
 * tuning, the component climbs onto the ledge in a custom movement mode. Epic names a custom mode
 * with an override of `PhysCustom` as the way to add a movement to this component (D-34).
 *
 * The player character writes the band and the time from the movement tuning (D-29).
 */
UCLASS()
class IRONABSOLUTION_API UIronCharacterMovementComponent : public UCharacterMovementComponent
{
	GENERATED_BODY()

public:
	/**
	 * Sets the band and the time of the mantle. The movement tuning checks each value first.
	 * @param MinHeight The lowest ledge top above the feet, in cm.
	 * @param MaxHeight The highest ledge top above the feet, in cm.
	 * @param Time The time of the climb, in seconds.
	 */
	void SetMantleTuning(float MinHeight, float MaxHeight, float Time);

	/**
	 * Looks for a ledge in front of the player, with the traces of the mantle. The check does not
	 * read the input or the movement mode.
	 * @return The ledge and the path of the climb, or no value when no ledge in the band has room for the player.
	 */
	TOptional<FIronLedge> FindLedge() const;

	/** Gives true while the player climbs a ledge. */
	bool IsMantling() const;

protected:
	virtual void UpdateCharacterStateBeforeMovement(float DeltaSeconds) override;
	virtual void PhysCustom(float DeltaTime, int32 Iterations) override;

private:
	/** Gives true when the input of this frame moves the player forward, toward the view. */
	bool WantsToMoveForward() const;

	/** Starts the climb onto the ledge. */
	void StartMantle(const FIronLedge& Ledge);

	/** Gives the point of the path of the climb at a part of the time, from 0 to 1. */
	FVector GetMantlePathPoint(float Alpha) const;

	/** Ends the climb. The player walks on the ledge, or falls when no floor holds the player. */
	void EndMantle();

	float MantleMinHeight = 0.0f;
	float MantleMaxHeight = 0.0f;
	float MantleTime = 0.0f;

	/** The path of the current climb: the start, the top of the vertical part, and the end. */
	FVector MantleStart = FVector::ZeroVector;
	FIronLedge MantleLedge;

	/** The time since the start of the current climb, in seconds. */
	float MantleElapsed = 0.0f;
};
