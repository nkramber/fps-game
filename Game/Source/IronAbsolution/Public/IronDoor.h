// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "IronInteractable.h"

#include "IronDoor.generated.h"

class UStaticMeshComponent;

/**
 * A door with one panel. A use of the door, or of a switch that names it, opens a closed door and
 * closes an open door (D-146). The open panel moves by the open offset of the placed door.
 *
 * The class holds the rule. The level gives the mesh of the panel and the open offset (D-29).
 */
UCLASS()
class IRONABSOLUTION_API AIronDoor : public AActor, public IIronInteractable
{
	GENERATED_BODY()

public:
	AIronDoor();

	/** Opens a closed door, or closes an open door. */
	virtual void Interact(AActor& User) override;

	/**
	 * Opens a closed door, or closes an open door.
	 * @return True when the door moved. False after an error line when the door has no open offset.
	 */
	bool Toggle();

	/** Gives true when the door is open. */
	bool IsOpen() const;

	/** Gives the move of the panel from the closed place to the open place, relative to the door. */
	FVector GetOpenOffset() const;

	/** Sets the move of the panel from the closed place to the open place. It applies at the next toggle. */
	void SetOpenOffset(const FVector& NewOpenOffset);

	/** Gives the panel, the part that moves and blocks the path. */
	UStaticMeshComponent* GetPanel() const;

private:
	/** The panel of the door. The level sets its mesh and its size. */
	UPROPERTY(VisibleAnywhere, Category = "Door")
	TObjectPtr<UStaticMeshComponent> Panel;

	/** The move of the panel from the closed place to the open place, relative to the door. */
	UPROPERTY(EditAnywhere, Category = "Door", meta = (AllowPrivateAccess = "true", Units = "cm"))
	FVector OpenOffset = FVector::ZeroVector;

	bool bIsOpen = false;
};
