// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronWeaponComponent.h"

#include "Engine/World.h"
#include "IronHitFlash.h"
#include "IronTarget.h"
#include "IronWeaponTuning.h"
#include "Kismet/GameplayStatics.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronWeapon, Log, All);

namespace IronAbsolution::Weapon
{
	// The world adds the frame times one at a time, so a shot that is due at a frame can be late by
	// a rounding error. This margin lets that frame fire.
	constexpr double TimeMargin = 1.0e-4;
}

UIronWeaponComponent::UIronWeaponComponent()
{
	// The tick fires an automatic weapon while the fire key stays down.
	PrimaryComponentTick.bCanEverTick = true;
}

bool UIronWeaponComponent::SetWeapons(const TArray<TObjectPtr<UIronWeaponTuning>>& NewWeapons)
{
	TArray<FString> Errors;
	if (NewWeapons.IsEmpty())
	{
		Errors.Add(TEXT("the list of weapons is empty"));
	}
	for (int32 Slot = 0; Slot < NewWeapons.Num(); ++Slot)
	{
		if (NewWeapons[Slot] == nullptr)
		{
			Errors.Add(FString::Printf(TEXT("slot %d has no data asset"), Slot));
			continue;
		}
		Errors.Append(NewWeapons[Slot]->FindInvalidValues());
	}
	for (const FString& Error : Errors)
	{
		UE_LOG(LogIronWeapon, Error, TEXT("%s did not take the weapons: %s"), *GetPathName(), *Error);
	}
	if (!Errors.IsEmpty())
	{
		return false;
	}

	Weapons.Reset();
	Ammo.Reset();
	for (const TObjectPtr<UIronWeaponTuning>& Weapon : NewWeapons)
	{
		Weapons.Add(Weapon);
		Ammo.Add(Weapon->AmmoCapacity);
	}
	CurrentSlot = 0;
	bTriggerHeld = false;
	NextShotTime = 0.0;
	ReadyTime = 0.0;
	LastTargetHitTime.Reset();
	RecoilPitch = 0.0f;
	RecoilYaw = 0.0f;
	RecoilReturnRate = 0.0f;
	return true;
}

void UIronWeaponComponent::PullTrigger()
{
	bTriggerHeld = true;
	TryFire(true);
}

void UIronWeaponComponent::ReleaseTrigger()
{
	bTriggerHeld = false;
}

bool UIronWeaponComponent::SelectWeapon(int32 Slot)
{
	if (!Weapons.IsValidIndex(Slot))
	{
		UE_LOG(LogIronWeapon, Error, TEXT("%s did not change weapon: slot %d has no weapon. The weapons fill slots 0 to %d."), *GetPathName(), Slot, Weapons.Num() - 1);
		return false;
	}
	if (Slot == CurrentSlot)
	{
		return false;
	}

	CurrentSlot = Slot;
	// The raise time is the wait after a change, so the new weapon does not wait for the rate of
	// fire of the old weapon too.
	ReadyTime = GetWorld()->GetTimeSeconds() + Weapons[Slot]->RaiseTime;
	NextShotTime = 0.0;
	return true;
}

bool UIronWeaponComponent::ChangeToNextWeapon()
{
	if (Weapons.IsEmpty())
	{
		UE_LOG(LogIronWeapon, Error, TEXT("%s did not change weapon: it has no weapons. The owner gives them with SetWeapons."), *GetPathName());
		return false;
	}
	return SelectWeapon((CurrentSlot + 1) % Weapons.Num());
}

void UIronWeaponComponent::RefillAmmo()
{
	for (int32 Slot = 0; Slot < Weapons.Num(); ++Slot)
	{
		Ammo[Slot] = Weapons[Slot]->AmmoCapacity;
	}
}

int32 UIronWeaponComponent::GetWeaponCount() const
{
	return Weapons.Num();
}

int32 UIronWeaponComponent::GetCurrentSlot() const
{
	return CurrentSlot;
}

const UIronWeaponTuning* UIronWeaponComponent::GetCurrentWeapon() const
{
	return Weapons.IsValidIndex(CurrentSlot) ? Weapons[CurrentSlot].Get() : nullptr;
}

int32 UIronWeaponComponent::GetAmmo(int32 Slot) const
{
	checkf(Ammo.IsValidIndex(Slot), TEXT("%s has no weapon in slot %d. The weapons fill slots 0 to %d."), *GetPathName(), Slot, Ammo.Num() - 1);
	return Ammo[Slot];
}

int32 UIronWeaponComponent::GetCurrentAmmo() const
{
	return GetAmmo(CurrentSlot);
}

float UIronWeaponComponent::GetRaiseFraction() const
{
	const UIronWeaponTuning* Weapon = GetCurrentWeapon();
	if (Weapon == nullptr)
	{
		return 1.0f;
	}
	const double Remaining = ReadyTime - GetWorld()->GetTimeSeconds();
	return 1.0f - FMath::Clamp(static_cast<float>(Remaining) / Weapon->RaiseTime, 0.0f, 1.0f);
}

FRotator UIronWeaponComponent::GetRecoil() const
{
	return FRotator(RecoilPitch, RecoilYaw, 0.0f);
}

TOptional<double> UIronWeaponComponent::GetLastTargetHitTime() const
{
	return LastTargetHitTime;
}

TArray<FVector> UIronWeaponComponent::GetPelletDirections(const FRotator& ViewRotation, const UIronWeaponTuning& Weapon, const FRandomStream& Random)
{
	const FRotationMatrix Axes(ViewRotation);
	const FVector Forward = Axes.GetUnitAxis(EAxis::X);
	const FVector Right = Axes.GetUnitAxis(EAxis::Y);
	const FVector Up = Axes.GetUnitAxis(EAxis::Z);
	const double CosSpread = FMath::Cos(FMath::DegreesToRadians(static_cast<double>(Weapon.SpreadAngle)));

	// An even spread over the cap of the cone: the cosine of the angle from the view is even from 1
	// down to the cosine of the spread, and the turn around the view is even. The engine function
	// FRandomStream::VRandCone is not even over the cone, so the rule does not use it.
	TArray<FVector> Directions;
	for (int32 Pellet = 0; Pellet < Weapon.PelletCount; ++Pellet)
	{
		const double CosAngle = 1.0 - Random.FRand() * (1.0 - CosSpread);
		const double SinAngle = FMath::Sqrt(FMath::Max(0.0, 1.0 - CosAngle * CosAngle));
		const double Turn = UE_DOUBLE_TWO_PI * Random.FRand();
		const FVector Side = Right * FMath::Cos(Turn) + Up * FMath::Sin(Turn);
		Directions.Add(Forward * CosAngle + Side * SinAngle);
	}
	return Directions;
}

void UIronWeaponComponent::BeginPlay()
{
	Super::BeginPlay();
	Random.GenerateNewSeed();
}

void UIronWeaponComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	const UIronWeaponTuning* Weapon = GetCurrentWeapon();
	if (Weapon == nullptr)
	{
		return;
	}

	// The kick comes back before the shot, so a shot of this frame adds its full kick.
	ReduceRecoil(DeltaTime);

	if (bTriggerHeld && Weapon->bAutomatic)
	{
		TryFire(false);
	}
}

bool UIronWeaponComponent::TryFire(bool bNewPress)
{
	const UIronWeaponTuning* Weapon = GetCurrentWeapon();
	if (Weapon == nullptr)
	{
		UE_LOG(LogIronWeapon, Error, TEXT("%s did not fire: it has no weapons. The owner gives them with SetWeapons."), *GetPathName());
		return false;
	}

	const double Now = GetWorld()->GetTimeSeconds();
	if (Now + IronAbsolution::Weapon::TimeMargin < ReadyTime || Now + IronAbsolution::Weapon::TimeMargin < NextShotTime)
	{
		return false;
	}

	if (Ammo[CurrentSlot] <= 0)
	{
		// No shot and no change of ammo. The HUD shows the empty count (D-158).
		if (bNewPress)
		{
			PlayWeaponSound(Weapon->EmptySound, 0.0f);
		}
		return false;
	}

	// A held trigger keeps the rate of fire: when the shot is due within one interval, the next
	// interval starts at the planned time, so the frame rate does not slow the fire. After a pause,
	// the next interval starts now.
	const double Interval = Weapon->GetShotInterval();
	const double Base = (Now - NextShotTime) < Interval ? NextShotTime : Now;
	NextShotTime = Base + Interval;

	FireShot(*Weapon);
	return true;
}

void UIronWeaponComponent::FireShot(const UIronWeaponTuning& Weapon)
{
	--Ammo[CurrentSlot];
	PlayWeaponSound(Weapon.ShotSound, Weapon.ShotSoundStartTime);

	AActor* Owner = GetOwner();
	UWorld* World = GetWorld();
	FVector EyeLocation;
	FRotator EyeRotation;
	Owner->GetActorEyesViewPoint(EyeLocation, EyeRotation);

	// A complex trace hits the triangles of a mesh, not its simple collision, so the hit is exact.
	const FCollisionQueryParams Params(SCENE_QUERY_STAT(IronWeapon), true, Owner);
	bool bHitTarget = false;
	for (const FVector& Direction : GetPelletDirections(EyeRotation, Weapon, Random))
	{
		FHitResult Hit;
		if (!World->LineTraceSingleByChannel(Hit, EyeLocation, EyeLocation + Direction * Weapon.Range, TraceChannel, Params))
		{
			continue;
		}

		World->SpawnActor<AIronHitFlash>(Weapon.HitFlashClass, Hit.ImpactPoint, FRotator::ZeroRotator);
		if (AIronTarget* Target = Cast<AIronTarget>(Hit.GetActor()))
		{
			Target->RegisterHit();
			bHitTarget = true;
		}
	}

	// One hit marker for each shot, for one pellet on the target or for each pellet. The hit has no
	// sound (D-167).
	if (bHitTarget)
	{
		LastTargetHitTime = World->GetTimeSeconds();
	}

	// The kick comes after the traces, so the shot goes where the player aimed. The new rate brings
	// the whole kick back in the recovery time, so a long burst settles at a small height and has no
	// slow return at its end (D-162).
	RecoilPitch += Weapon.RecoilKick;
	RecoilYaw += Random.FRandRange(-Weapon.RecoilSideKick, Weapon.RecoilSideKick);
	RecoilReturnRate = FMath::Sqrt(RecoilPitch * RecoilPitch + RecoilYaw * RecoilYaw) / Weapon.RecoilRecoveryTime;
}

void UIronWeaponComponent::PlayWeaponSound(USoundBase* Sound, float StartTime) const
{
	// A sound of the game, not of the interface, so a pause of the game stops it. The volume of
	// each sound asset sets the mix (D-168).
	UGameplayStatics::PlaySound2D(this, Sound, 1.0f, 1.0f, StartTime, nullptr, GetOwner(), false);
}

void UIronWeaponComponent::ReduceRecoil(float DeltaTime)
{
	const float Length = FMath::Sqrt(RecoilPitch * RecoilPitch + RecoilYaw * RecoilYaw);
	if (Length <= 0.0f)
	{
		return;
	}
	const float Step = FMath::Min(Length, RecoilReturnRate * DeltaTime);
	const float Scale = (Length - Step) / Length;
	RecoilPitch *= Scale;
	RecoilYaw *= Scale;
}
