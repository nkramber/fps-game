// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"

#include "IronPlayerCharacter.generated.h"

class UCameraComponent;
class UInputAction;
class UInputComponent;
class UIronCharacterMovementComponent;
class UIronMovementTuning;
class UIronWeaponComponent;
class UIronWeaponTuning;
class UMaterialInterface;
class UStaticMeshComponent;
struct FInputActionValue;

/**
 * The player of the game: a first-person character with the movement component of the engine
 * (D-29, D-34). The player moves at full speed with no run key (D-132), jumps, and turns the view
 * with the mouse for the verb "aim" (D-116). In the air, a forward move into a ledge starts a mantle
 * (D-143). The interact key uses the switch or the door in reach, and an outline and a glow show
 * the target in reach (D-144, D-145, D-148). The fire key fires the weapon in hand, and the verb
 * "change weapon" takes another weapon in hand (D-128, D-151, D-152).
 *
 * The class holds the rules. A Blueprint subclass holds the content: the movement tuning, the data
 * assets of the weapons, and the input actions. No C++ names a key, so a new device needs a new
 * mapping context alone (OQ-21).
 */
UCLASS(Abstract)
class IRONABSOLUTION_API AIronPlayerCharacter : public ACharacter
{
	GENERATED_BODY()

public:
	explicit AIronPlayerCharacter(const FObjectInitializer& ObjectInitializer);

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

	/**
	 * Finds the target of the verb "interact": the first thing on the line of the view, in the
	 * reach of the tuning, when it takes the verb (D-144).
	 * @return The actor in reach, or null when no target is in reach.
	 */
	AActor* FindInteractTarget() const;

	/**
	 * Uses the target in reach, as the interact key does.
	 * @return True when a target was in reach and took the use.
	 */
	bool Interact();

	/** Gives the actor with the cue, or null when no target is in reach (D-145, D-148). */
	AActor* GetOutlinedTarget() const;

	/** Gives the tuning that the Blueprint subclass sets, or null when it sets none. */
	const UIronMovementTuning* GetMovementTuning() const;

	/** Gives the first-person camera. */
	UCameraComponent* GetFirstPersonCamera() const;

	/** Gives the movement component of the player, with the mantle. */
	UIronCharacterMovementComponent* GetIronMovement() const;

	/** Gives the weapon rule of the player: the shot, the ammo, and change weapon. */
	UIronWeaponComponent* GetWeapon() const;

	/** Gives the data assets of the weapons that the Blueprint subclass sets, one for each slot. */
	const TArray<TObjectPtr<UIronWeaponTuning>>& GetWeapons() const;

	/** Gives the mesh of the weapon in hand in the view. */
	UStaticMeshComponent* GetWeaponViewMesh() const;

	/**
	 * Gives the rotation of the view: the control rotation, with the recoil of the weapon added to
	 * the pitch and the yaw (D-159, D-161). The camera, the shot, and interact read it. The control
	 * rotation does not change, so the view comes back to the aim of the player when the recoil is gone.
	 */
	virtual FRotator GetViewRotation() const override;

	virtual void Tick(float DeltaSeconds) override;

protected:
	virtual void BeginPlay() override;
	virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;
	virtual void SetupPlayerInputComponent(UInputComponent* PlayerInputComponent) override;

private:
	void HandleMove(const FInputActionValue& Value);
	void HandleLook(const FInputActionValue& Value);
	void HandleInteract(const FInputActionValue& Value);
	void HandleFireStarted(const FInputActionValue& Value);
	void HandleFireCompleted(const FInputActionValue& Value);
	void HandleChangeWeapon(const FInputActionValue& Value);

	/** Takes the weapon of a slot in hand. The value of the action is the number of the slot, from 1. */
	void HandleSelectWeapon(const FInputActionValue& Value);

	/** Shows the mesh of the weapon in hand in the view. A new weapon comes up from below during its raise. */
	void UpdateWeaponView();

	/** Moves the cue to the target in reach, or removes it (D-145, D-148). */
	void UpdateInteractCue();

	/**
	 * Shows or hides the cue on an actor. The cue turns on the custom depth of each primitive, for
	 * the outline material, and puts the glow material on each mesh as its overlay.
	 */
	void ShowInteractCue(AActor* Target, bool bShown) const;

	/** Sets the field of view of the camera from the settings of the player (D-141). */
	void ApplyFieldOfView();

	/** The camera at the eye height of the tuning. The view follows the control rotation. */
	UPROPERTY(VisibleAnywhere, Category = "Camera")
	TObjectPtr<UCameraComponent> FirstPersonCamera;

	/** The weapon rule of the player. BeginPlay gives it the data assets of the weapons. */
	UPROPERTY(VisibleAnywhere, Category = "Weapon")
	TObjectPtr<UIronWeaponComponent> Weapon;

	/** The mesh of the weapon in hand, on the camera. The data asset of the weapon gives its mesh, scale, and place. */
	UPROPERTY(VisibleAnywhere, Category = "Weapon")
	TObjectPtr<UStaticMeshComponent> WeaponViewMesh;

	/** The data asset of each weapon, one for each slot, in the order of the slots. The Blueprint subclass sets them (D-128). */
	UPROPERTY(EditDefaultsOnly, Category = "Weapon", meta = (AllowPrivateAccess = "true"))
	TArray<TObjectPtr<UIronWeaponTuning>> Weapons;

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

	/** The input action of the verb "interact", with a true or false value. */
	UPROPERTY(EditDefaultsOnly, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UInputAction> InteractAction;

	/** The input action of the verb "shoot", with a true or false value. */
	UPROPERTY(EditDefaultsOnly, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UInputAction> FireAction;

	/** The input action that takes the next weapon in hand, with a true or false value (D-152). */
	UPROPERTY(EditDefaultsOnly, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UInputAction> ChangeWeaponAction;

	/** The input action that takes the weapon of one slot in hand. Its 1D value is the number of the slot, from 1 (D-152). */
	UPROPERTY(EditDefaultsOnly, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UInputAction> SelectWeaponAction;

	/**
	 * The post-process material that draws the outline of the target in reach, from the custom
	 * depth (D-145). The camera uses it only while a target is in reach.
	 */
	UPROPERTY(EditDefaultsOnly, Category = "Interact", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UMaterialInterface> InteractOutlineMaterial;

	/** The overlay material that fills the target in reach with a glow (D-148). */
	UPROPERTY(EditDefaultsOnly, Category = "Interact", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UMaterialInterface> InteractGlowMaterial;

	/** The reach of interact from the tuning, in cm. ApplyMovementTuning writes it. */
	float InteractReach = 0.0f;

	/** The actor with the outline of the cue. A weak pointer, because a level can remove the actor. */
	TWeakObjectPtr<AActor> OutlinedTarget;
};
