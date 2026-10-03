// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronAmmoStation.h"

#include "Components/StaticMeshComponent.h"
#include "IronPlayerCharacter.h"
#include "IronWeaponComponent.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronAmmoStation, Log, All);

AIronAmmoStation::AIronAmmoStation()
{
	Body = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Body"));
	RootComponent = Body;
}

void AIronAmmoStation::Interact(AActor& User)
{
	AIronPlayerCharacter* Character = Cast<AIronPlayerCharacter>(&User);
	if (Character == nullptr)
	{
		UE_LOG(LogIronAmmoStation, Error, TEXT("%s did nothing for %s: only the player character has weapons to fill."), *GetPathName(), *User.GetPathName());
		return;
	}

	Character->GetWeapon()->RefillAmmo();
}

UStaticMeshComponent* AIronAmmoStation::GetBody() const
{
	return Body;
}
