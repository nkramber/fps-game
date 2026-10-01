// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronPlayerController.h"

#include "Engine/LocalPlayer.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "InputActionValue.h"
#include "InputMappingContext.h"
#include "Kismet/KismetSystemLibrary.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronPlayerController, Log, All);

const TArray<TObjectPtr<UInputMappingContext>>& AIronPlayerController::GetMappingContexts() const
{
	return MappingContexts;
}

bool AIronPlayerController::BindQuitAction(UEnhancedInputComponent& EnhancedInput)
{
	if (QuitAction == nullptr)
	{
		UE_LOG(LogIronPlayerController, Error, TEXT("%s has no QuitAction, so no key closes the game. Set it in the Blueprint subclass (D-150)."), *GetPathName());
		return false;
	}

	EnhancedInput.BindAction(QuitAction, ETriggerEvent::Started, this, &AIronPlayerController::HandleQuit);
	return true;
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

	UEnhancedInputComponent* EnhancedInput = Cast<UEnhancedInputComponent>(InputComponent);
	if (EnhancedInput == nullptr)
	{
		UE_LOG(LogIronPlayerController, Error, TEXT("%s got the input component %s, not an Enhanced Input component, so no key closes the game (F-6)."), *GetPathName(), *GetPathNameSafe(InputComponent));
		return;
	}
	BindQuitAction(*EnhancedInput);
}

void AIronPlayerController::HandleQuit(const FInputActionValue& Value)
{
	// A quit with no menu, until phase 8 gives one (D-150).
	UKismetSystemLibrary::QuitGame(this, this, EQuitPreference::Quit, false);
}
