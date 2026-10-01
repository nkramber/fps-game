// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "IronInteractable.h"

#include "IronSwitch.generated.h"

class AIronDoor;
class UStaticMeshComponent;

/**
 * A switch that opens and closes one door. Each use toggles the door (D-146).
 *
 * The class holds the rule. The level gives the mesh of the button and names the door (D-29).
 */
UCLASS()
class IRONABSOLUTION_API AIronSwitch : public AActor, public IIronInteractable
{
	GENERATED_BODY()

public:
	AIronSwitch();

	/** Toggles the door of the switch. With no door, it writes an error line and does nothing. */
	virtual void Interact(AActor& User) override;

	/** Gives the door that the switch opens and closes, or null when the level sets none. */
	AIronDoor* GetDoor() const;

	/** Sets the door that the switch opens and closes. */
	void SetDoor(AIronDoor* NewDoor);

	/** Gives the button, the part that the player looks at to use the switch. */
	UStaticMeshComponent* GetButton() const;

private:
	/** The button of the switch. The level sets its mesh and its size. */
	UPROPERTY(VisibleAnywhere, Category = "Switch")
	TObjectPtr<UStaticMeshComponent> Button;

	/** The door that the switch opens and closes. The level sets it. */
	UPROPERTY(EditInstanceOnly, Category = "Switch", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<AIronDoor> Door;
};
