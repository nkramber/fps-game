// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "UObject/Interface.h"

#include "IronInteractable.generated.h"

UINTERFACE(MinimalAPI, meta = (CannotImplementInterfaceInBlueprint))
class UIronInteractable : public UInterface
{
	GENERATED_BODY()
};

/**
 * An actor that the player uses with the verb "interact" (D-116). The player character finds the
 * first actor on the line of the view, in reach, and calls Interact when the player presses the
 * interact key (D-144). The door and the switch of PR-24 take this interface. A later actor takes
 * the verb with no change of the player.
 */
class IRONABSOLUTION_API IIronInteractable
{
	GENERATED_BODY()

public:
	/**
	 * Does the action of this actor, for example to open a door.
	 * @param User The actor that used this actor, for example the player character.
	 */
	virtual void Interact(AActor& User) = 0;
};
