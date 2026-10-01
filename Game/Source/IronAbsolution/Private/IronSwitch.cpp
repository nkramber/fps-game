// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronSwitch.h"

#include "Components/StaticMeshComponent.h"
#include "IronDoor.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronSwitch, Log, All);

AIronSwitch::AIronSwitch()
{
	Button = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Button"));
	RootComponent = Button;
}

void AIronSwitch::Interact(AActor& User)
{
	if (Door == nullptr)
	{
		UE_LOG(LogIronSwitch, Error, TEXT("%s did nothing for %s: it names no door. Set Door on the placed switch."), *GetPathName(), *User.GetPathName());
		return;
	}

	Door->Toggle();
}

AIronDoor* AIronSwitch::GetDoor() const
{
	return Door;
}

void AIronSwitch::SetDoor(AIronDoor* NewDoor)
{
	Door = NewDoor;
}

UStaticMeshComponent* AIronSwitch::GetButton() const
{
	return Button;
}
