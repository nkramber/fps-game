// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronPlayerController.h"

#include "Engine/LocalPlayer.h"
#include "EnhancedInputSubsystems.h"
#include "InputMappingContext.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronPlayerController, Log, All);

const TArray<TObjectPtr<UInputMappingContext>>& AIronPlayerController::GetMappingContexts() const
{
	return MappingContexts;
}

void AIronPlayerController::SetupInputComponent()
{
	Super::SetupInputComponent();

	// A controller of a remote player has no local input.
	if (!IsLocalPlayerController())
	{
		return;
	}

	UEnhancedInputLocalPlayerSubsystem* InputSubsystem = ULocalPlayer::GetSubsystem<UEnhancedInputLocalPlayerSubsystem>(GetLocalPlayer());
	if (InputSubsystem == nullptr)
	{
		UE_LOG(LogIronPlayerController, Error, TEXT("%s found no Enhanced Input subsystem on its local player, so no key works."), *GetPathName());
		return;
	}

	if (MappingContexts.IsEmpty())
	{
		UE_LOG(LogIronPlayerController, Error, TEXT("%s has no MappingContexts, so no key works. Set them in the Blueprint subclass."), *GetPathName());
		return;
	}

	for (int32 Index = 0; Index < MappingContexts.Num(); ++Index)
	{
		const UInputMappingContext* Context = MappingContexts[Index];
		if (Context == nullptr)
		{
			UE_LOG(LogIronPlayerController, Error, TEXT("%s has no asset in MappingContexts[%d]. Set it in the Blueprint subclass."), *GetPathName(), Index);
			continue;
		}

		InputSubsystem->AddMappingContext(Context, 0);
	}
}
