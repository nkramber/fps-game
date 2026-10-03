// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronHitFlash.h"

#include "Components/StaticMeshComponent.h"
#include "Engine/StaticMesh.h"
#include "Engine/World.h"
#include "Materials/MaterialInstanceDynamic.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronHitFlash, Log, All);

namespace IronAbsolution::HitFlash
{
	// The basic shapes of the engine are 100 cm on each side.
	constexpr float ShapeSize = 100.0f;
}

const FName AIronHitFlash::BrightnessParameter(TEXT("Brightness"));

AIronHitFlash::AIronHitFlash()
{
	PrimaryActorTick.bCanEverTick = true;

	Mesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Mesh"));
	RootComponent = Mesh;
	// A flash must not block a later shot, the player, or a light.
	Mesh->SetCollisionEnabled(ECollisionEnabled::NoCollision);
	Mesh->SetCastShadow(false);
}

void AIronHitFlash::BeginPlay()
{
	Super::BeginPlay();

	// A NaN fails the comparison. An infinite life never ends, so each size and time must also be finite.
	const bool bDiameterValid = FMath::IsFinite(Diameter) && Diameter > 0.0f;
	const bool bLifetimeValid = FMath::IsFinite(Lifetime) && Lifetime > 0.0f;
	if (FlashMesh == nullptr || FlashMaterial == nullptr || !bDiameterValid || !bLifetimeValid)
	{
		UE_LOG(LogIronHitFlash, Error, TEXT("%s has an absent or invalid value: FlashMesh %s, FlashMaterial %s, Diameter %f, Lifetime %f. Each size and time must be finite and above 0. Set each one in the Blueprint subclass (D-154). The flash removes itself."),
			*GetPathName(), *GetPathNameSafe(FlashMesh), *GetPathNameSafe(FlashMaterial), Diameter, Lifetime);
		Destroy();
		return;
	}

	// A static component takes no new mesh after play starts, so the flash is movable.
	Mesh->SetMobility(EComponentMobility::Movable);
	Mesh->SetStaticMesh(FlashMesh);
	Mesh->SetWorldScale3D(FVector(Diameter / IronAbsolution::HitFlash::ShapeSize));
	FlashMaterialInstance = Mesh->CreateDynamicMaterialInstance(0, FlashMaterial);
	FlashMaterialInstance->SetScalarParameterValue(BrightnessParameter, 1.0f);

	SpawnTime = GetWorld()->GetTimeSeconds();
	SetLifeSpan(Lifetime);
}

void AIronHitFlash::Tick(float DeltaSeconds)
{
	Super::Tick(DeltaSeconds);

	if (FlashMaterialInstance != nullptr)
	{
		FlashMaterialInstance->SetScalarParameterValue(BrightnessParameter, GetBrightness());
	}
}

UStaticMeshComponent* AIronHitFlash::GetFlashMesh() const
{
	return Mesh;
}

float AIronHitFlash::GetLifetime() const
{
	return Lifetime;
}

float AIronHitFlash::GetBrightness() const
{
	if (!(Lifetime > 0.0f))
	{
		return 0.0f;
	}
	const double Age = GetWorld()->GetTimeSeconds() - SpawnTime;
	return FMath::Clamp(1.0f - static_cast<float>(Age) / Lifetime, 0.0f, 1.0f);
}
