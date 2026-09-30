// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"

#include "IronPlayerCharacter.generated.h"

class UCameraComponent;
class UInputAction;
class UInputComponent;
class UIronMovementTuning;
struct FInputActionValue;

/**
 * The player of the game: a first-person character with the movement component of the engine
 * (D-29, D-34). The player moves at full speed with no run key (D-132), jumps, and turns the view
 * with the mouse for the verb "aim" (D-116).
 *
 * The class holds the rules. A Blueprint subclass holds the content: the movement tuning and the
 * input actions. No C++ names a key, so a new device needs a new mapping context alone (OQ-21).
 */
UCLASS(Abstract)
class IRONABSOLUTION_API AIronPlayerCharacter : public ACharacter
{
	GENERATED_BODY()

public:
	AIronPlayerCharacter();

	/**
	 * Writes each value of a tuning into the movement component and the camera. The character
	 * calls it with its own tuning when play starts. It needs a world, because the jump speed
	 * comes from the gravity of the world.
	 * @param Tuning The values to write.
	 * @return True when each value is valid and the character took it. False after an error line for each problem, with no change.
	 */
	bool ApplyMovementTuning(const UIronMovementTuning& Tuning);

	/**
	 * Adds one frame of move input, relative to the view.
	 * @param Axis X moves to the right, and Y moves forward. Each part runs from -1 to 1.
	 */
	void Move(const FVector2D& Axis);

	/**
	 * Turns the view by one frame of mouse movement. Each mouse count turns the view by the degrees
	 * of the mouse sensitivity (D-139). The project turns off the input scales of the engine, so the
	 * controller takes the degrees as they are.
	 * @param MouseCounts The mouse movement in counts: to the right in X, and forward in Y. A positive Y turns the view up.
	 */
	void Look(const FVector2D& MouseCounts);

	/** Gives the tuning that the Blueprint subclass sets, or null when it sets none. */
	const UIronMovementTuning* GetMovementTuning() const;

	/** Gives the first-person camera. */
	UCameraComponent* GetFirstPersonCamera() const;

protected:
	virtual void BeginPlay() override;
	virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;
	virtual void SetupPlayerInputComponent(UInputComponent* PlayerInputComponent) override;

private:
	void HandleMove(const FInputActionValue& Value);
	void HandleLook(const FInputActionValue& Value);

	/** Sets the field of view of the camera from the settings of the player (D-141). */
	void ApplyFieldOfView();

	/** The camera at the eye height of the tuning. The view follows the control rotation. */
	UPROPERTY(VisibleAnywhere, Category = "Camera")
	TObjectPtr<UCameraComponent> FirstPersonCamera;

	/** The tuning of the movement and the camera. The Blueprint subclass sets it (D-29). */
	UPROPERTY(EditDefaultsOnly, Category = "Movement", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UIronMovementTuning> MovementTuning;

	/** The input action of the verb "move", with a 2D value. */
	UPROPERTY(EditDefaultsOnly, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UInputAction> MoveAction;

	/** The input action of the verb "aim", with a 2D value. */
	UPROPERTY(EditDefaultsOnly, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UInputAction> LookAction;

	/** The input action of the verb "jump", with a true or false value. */
	UPROPERTY(EditDefaultsOnly, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UInputAction> JumpAction;
};
