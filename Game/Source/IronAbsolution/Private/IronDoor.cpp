// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronDoor.h"

#include "Components/SceneComponent.h"
#include "Components/StaticMeshComponent.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronDoor, Log, All);

AIronDoor::AIronDoor()
{
	// The panel moves relative to a fixed root, so the placed door keeps the closed place.
	RootComponent = CreateDefaultSubobject<USceneComponent>(TEXT("Root"));
	Panel = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Panel"));
	Panel->SetupAttachment(RootComponent);
	Panel->SetMobility(EComponentMobility::Movable);
}

void AIronDoor::Interact(AActor& User)
{
	Toggle();
}

bool AIronDoor::Toggle()
{
	// A NaN or an infinity is not near zero, so this check comes first. The door then keeps its
	// state and its panel (T-2).
	if (OpenOffset.ContainsNaN())
	{
		UE_LOG(LogIronDoor, Error, TEXT("%s did not move: its OpenOffset is not finite, %s. Set it on the placed door."), *GetPathName(), *OpenOffset.ToString());
		return false;
	}

	// A door with no open offset does not move, so a use of it must say why (T-2).
	if (OpenOffset.IsNearlyZero())
	{
		UE_LOG(LogIronDoor, Error, TEXT("%s did not move: its OpenOffset is zero. Set it on the placed door."), *GetPathName());
		return false;
	}

	bIsOpen = !bIsOpen;
	Panel->SetRelativeLocation(bIsOpen ? OpenOffset : FVector::ZeroVector);
	return true;
}

bool AIronDoor::IsOpen() const
{
	return bIsOpen;
}

FVector AIronDoor::GetOpenOffset() const
{
	return OpenOffset;
}

void AIronDoor::SetOpenOffset(const FVector& NewOpenOffset)
{
	OpenOffset = NewOpenOffset;
}

UStaticMeshComponent* AIronDoor::GetPanel() const
{
	return Panel;
}
