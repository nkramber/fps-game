// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"

#include "IronWeaponComponent.generated.h"

class UIronWeaponTuning;
class USoundBase;

/**
 * The weapon rule of phase 3: the shot, the ammo, and change weapon (D-128). Each weapon is a data
 * asset of the rule, so a new data asset makes a new weapon with no change of C++ (D-29).
 *
 * A shot is hitscan: a line trace from the eye of the owner for each pellet, on the weapon channel
 * (D-130). Each shot spends one round, and an empty weapon does not fire. No weapon needs a reload
 * (D-115). A change of weapon takes the raise time of the new weapon (D-157). Each shot kicks the
 * view up and to a random side, and the kick comes back by itself (D-159, D-161, D-162). The owner
 * adds GetRecoil to its view. The jab of a melee attack holds the fire with HoldFire (D-173).
 *
 * The owner gives the list of weapons with SetWeapons. The player character does this when play
 * starts, from its Blueprint subclass.
 */
UCLASS()
class IRONABSOLUTION_API UIronWeaponComponent : public UActorComponent
{
	GENERATED_BODY()

public:
	/** The trace channel "Weapon" of `Game/Config/DefaultEngine.ini`. Each object blocks it by default. */
	static constexpr ECollisionChannel TraceChannel = ECC_GameTraceChannel1;

	UIronWeaponComponent();

	/**
	 * Takes a new list of weapons, each with a full weapon. The first weapon is in hand, and it can
	 * fire at once.
	 * @param NewWeapons One data asset for each slot, in the order of the slots.
	 * @return True when the list has a weapon and each tuning is valid. False after an error line for each problem, with no change.
	 */
	bool SetWeapons(const TArray<TObjectPtr<UIronWeaponTuning>>& NewWeapons);

	/** Starts a press of the fire key. The weapon in hand fires when it is ready and has ammo. */
	void PullTrigger();

	/** Ends a press of the fire key. An automatic weapon stops its fire. */
	void ReleaseTrigger();

	/**
	 * Takes the weapon of a slot in hand, with the raise time of that weapon.
	 * @param Slot The slot, from 0.
	 * @return True when the weapon in hand changed. False for the slot in hand, and false after an error line for a slot that has no weapon.
	 */
	bool SelectWeapon(int32 Slot);

	/**
	 * Takes the weapon of the next slot in hand, after the last slot the first one.
	 * @return True when the weapon in hand changed. False with one weapon, and false after an error line with no weapon.
	 */
	bool ChangeToNextWeapon();

	/**
	 * Stops the fire of each weapon for a time. A press in this time fires no shot. A held fire key
	 * fires an automatic weapon again at the end of the time. The melee attack calls it for its jab.
	 * @param Seconds The time from now. A shorter hold does not end a longer hold that is in progress.
	 */
	void HoldFire(float Seconds);

	/** Fills each weapon to its capacity (D-156). */
	void RefillAmmo();

	/** Gives the count of weapons. */
	int32 GetWeaponCount() const;

	/** Gives the slot of the weapon in hand, or INDEX_NONE before SetWeapons. */
	int32 GetCurrentSlot() const;

	/** Gives the tuning of the weapon in hand, or null before SetWeapons. */
	const UIronWeaponTuning* GetCurrentWeapon() const;

	/**
	 * Gives the rounds of the weapon of a slot.
	 * @param Slot The slot, from 0. It must have a weapon.
	 */
	int32 GetAmmo(int32 Slot) const;

	/** Gives the rounds of the weapon in hand. There must be a weapon in hand. */
	int32 GetCurrentAmmo() const;

	/** Gives the part of the raise that is complete: 0 at the change, and 1 when the weapon can fire. */
	float GetRaiseFraction() const;

	/** Gives the turn of the view from the recoil now: up in Pitch and to the right in Yaw, in degrees. It is zero when no kick is left. */
	FRotator GetRecoil() const;

	/** Gives the time of the world of the last shot that hit a gym target, or no value before the first such shot. */
	TOptional<double> GetLastTargetHitTime() const;

	/**
	 * Gives the direction of each pellet of one shot: a random point of the cone of the spread
	 * around the view, with an even spread over the cone (D-160). With no spread, each pellet goes
	 * along the view.
	 * @param ViewRotation The rotation of the view.
	 * @param Weapon A tuning with no invalid value.
	 * @param Random The source of the random points.
	 * @return One unit vector for each pellet.
	 */
	static TArray<FVector> GetPelletDirections(const FRotator& ViewRotation, const UIronWeaponTuning& Weapon, const FRandomStream& Random);

	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

protected:
	virtual void BeginPlay() override;

private:
	/**
	 * Fires one shot of the weapon in hand when it is ready and has ammo.
	 * @param bNewPress True for the first frame of a press. Only a new press plays the sound of an empty weapon.
	 * @return True when the weapon fired.
	 */
	bool TryFire(bool bNewPress);

	/** Spends one round, traces each pellet, gives the feedback of each hit, and kicks the view. */
	void FireShot(const UIronWeaponTuning& Weapon);

	/** Moves the kick back toward zero along its own direction, at the rate of return of the last shot. */
	void ReduceRecoil(float DeltaTime);

	/** Plays a sound of the weapon for the player, from a time into the sound. */
	void PlayWeaponSound(USoundBase* Sound, float StartTime) const;

	/** The weapon of each slot. SetWeapons sets them. */
	UPROPERTY(Transient)
	TArray<TObjectPtr<UIronWeaponTuning>> Weapons;

	/** The rounds of the weapon of each slot. */
	TArray<int32> Ammo;

	int32 CurrentSlot = INDEX_NONE;

	bool bTriggerHeld = false;

	/** The time of the world of the next shot at the rate of fire. */
	double NextShotTime = 0.0;

	/** The time of the world at the end of the raise of the weapon in hand. */
	double ReadyTime = 0.0;

	/** The time of the world at the end of the hold of the fire. HoldFire sets it. */
	double FireHeldUntil = 0.0;

	TOptional<double> LastTargetHitTime;

	/** The turn of the view up from the recoil, in degrees (D-159). */
	float RecoilPitch = 0.0f;

	/** The turn of the view to the right from the recoil, in degrees. A negative value turns it to the left (D-161). */
	float RecoilYaw = 0.0f;

	/** The rate at which the kick comes back, in degrees each second. Each shot sets it (D-162). */
	float RecoilReturnRate = 0.0f;

	/** The source of the random points of the spread and of the side of each kick. BeginPlay gives it a new seed. */
	FRandomStream Random;
};
