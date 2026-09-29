// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"

#include "IronPlayerController.generated.h"

class UInputMappingContext;

/**
 * The player controller. It adds each input mapping context of its Blueprint subclass for the
 * local player, as the first-person template of the engine does (D-34). A gamepad later adds one
 * more mapping context to the list, with no change of C++ (OQ-21).
 */
UCLASS(Abstract)
class IRONABSOLUTION_API AIronPlayerController : public APlayerController
{
	GENERATED_BODY()

public:
	/** Gives the mapping contexts that the Blueprint subclass sets. */
	const TArray<TObjectPtr<UInputMappingContext>>& GetMappingContexts() const;

protected:
	virtual void SetupInputComponent() override;

private:
	/** The input mapping contexts of the player, one for each device. The Blueprint subclass sets them. */
	UPROPERTY(EditDefaultsOnly, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TArray<TObjectPtr<UInputMappingContext>> MappingContexts;
};
