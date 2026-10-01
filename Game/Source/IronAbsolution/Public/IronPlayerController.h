// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"

#include "IronPlayerController.generated.h"

class UEnhancedInputComponent;
class UInputAction;
class UInputMappingContext;
struct FInputActionValue;

/**
 * The player controller. It adds each input mapping context of its Blueprint subclass for the
 * local player, as the first-person template of the engine does (D-34). A gamepad later adds one
 * more mapping context to the list, with no change of C++ (OQ-21).
 *
 * The quit action closes the game until the menu of phase 8 (D-150).
 */
UCLASS(Abstract)
class IRONABSOLUTION_API AIronPlayerController : public APlayerController
{
	GENERATED_BODY()

public:
	/** Gives the mapping contexts that the Blueprint subclass sets. */
	const TArray<TObjectPtr<UInputMappingContext>>& GetMappingContexts() const;

	/**
	 * Binds the quit action of the Blueprint subclass, so its key closes the game (D-150).
	 * @param EnhancedInput The input component of this controller.
	 * @return True when the action is bound. False after an error line when the Blueprint sets no quit action.
	 */
	bool BindQuitAction(UEnhancedInputComponent& EnhancedInput);

protected:
	virtual void SetupInputComponent() override;

private:
	/** The input mapping contexts of the player, one for each device. The Blueprint subclass sets them. */
	UPROPERTY(EditDefaultsOnly, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TArray<TObjectPtr<UInputMappingContext>> MappingContexts;

	/** The input action that closes the game, with a true or false value. The Blueprint subclass sets it. */
	UPROPERTY(EditDefaultsOnly, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UInputAction> QuitAction;

	void HandleQuit(const FInputActionValue& Value);
};
