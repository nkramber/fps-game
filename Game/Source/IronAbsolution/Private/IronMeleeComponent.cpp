// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronMeleeComponent.h"

#include "Engine/World.h"
#include "IronHitFlash.h"
#include "IronMeleeTuning.h"
#include "IronTarget.h"
#include "IronWeaponComponent.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronMelee, Log, All);

namespace IronAbsolution::Melee
{
	// The world adds the frame times one at a time, so an attack that is due at a frame can be late
	// by a rounding error. This margin lets that frame attack.
	constexpr double TimeMargin = 1.0e-4;
}

UIronMeleeComponent::UIronMeleeComponent()
{
	// Each attack comes from a key press, and the jab reads the time of the world, so the component needs no tick.
	PrimaryComponentTick.bCanEverTick = false;
}

bool UIronMeleeComponent::SetTuning(UIronMeleeTuning* NewTuning)
{
	if (NewTuning == nullptr)
	{
		UE_LOG(LogIronMelee, Error, TEXT("%s did not take the tuning: it is null."), *GetPathName());
		return false;
	}
	const TArray<FString> Errors = NewTuning->FindInvalidValues();
	for (const FString& Error : Errors)
	{
		UE_LOG(LogIronMelee, Error, TEXT("%s did not take the tuning: %s"), *GetPathName(), *Error);
	}
	if (!Errors.IsEmpty())
	{
		return false;
	}

	Tuning = NewTuning;
	LastAttackTime.Reset();
	LastTargetHitTime.Reset();
	return true;
}

bool UIronMeleeComponent::Attack()
{
	if (Tuning == nullptr)
	{
		UE_LOG(LogIronMelee, Error, TEXT("%s did not attack: it has no tuning. The owner gives it with SetTuning."), *GetPathName());
		return false;
	}

	UWorld* World = GetWorld();
	const double Now = World->GetTimeSeconds();
	if (LastAttackTime.IsSet() && Now + IronAbsolution::Melee::TimeMargin < LastAttackTime.GetValue() + Tuning->AttackInterval)
	{
		return false;
	}
	LastAttackTime = Now;

	AActor* Owner = GetOwner();
	FVector EyeLocation;
	FRotator EyeRotation;
	Owner->GetActorEyesViewPoint(EyeLocation, EyeRotation);

	// The center of the sphere stops one radius short of the range, so the front of the sphere
	// reaches the range and no further. A sweep of a sphere uses the simple collision of each mesh.
	const FVector End = EyeLocation + EyeRotation.Vector() * (Tuning->Range - Tuning->SweepRadius);
	const FCollisionQueryParams Params(SCENE_QUERY_STAT(IronMelee), false, Owner);
	FHitResult Hit;
	if (!World->SweepSingleByChannel(Hit, EyeLocation, End, FQuat::Identity, UIronWeaponComponent::TraceChannel, FCollisionShape::MakeSphere(Tuning->SweepRadius), Params))
	{
		return true;
	}

	World->SpawnActor<AIronHitFlash>(Tuning->HitFlashClass, Hit.ImpactPoint, FRotator::ZeroRotator);
	if (AActor* Struck = Hit.GetActor())
	{
		HitActor(*Struck);
	}
	return true;
}

const UIronMeleeTuning* UIronMeleeComponent::GetTuning() const
{
	return Tuning;
}

TOptional<double> UIronMeleeComponent::GetLastTargetHitTime() const
{
	return LastTargetHitTime;
}

float UIronMeleeComponent::GetJabDistance() const
{
	if (Tuning == nullptr || !LastAttackTime.IsSet())
	{
		return 0.0f;
	}
	const double Age = GetWorld()->GetTimeSeconds() - LastAttackTime.GetValue();
	if (Age < 0.0 || Age >= Tuning->JabTime)
	{
		return 0.0f;
	}
	// Half of a sine wave: the weapon goes out and comes back with no jump at either end.
	return Tuning->JabDistance * FMath::Sin(UE_PI * static_cast<float>(Age) / Tuning->JabTime);
}

void UIronMeleeComponent::HitActor(AActor& Actor)
{
	// Phase 4 adds the finish of a stunned enemy here (D-115). The hit has no sound (D-167).
	if (AIronTarget* Target = Cast<AIronTarget>(&Actor))
	{
		Target->RegisterMeleeHit();
		LastTargetHitTime = GetWorld()->GetTimeSeconds();
	}
}
