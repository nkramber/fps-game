// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "IronInteractable.h"

#include "IronAmmoStation.generated.h"

class UStaticMeshComponent;

/**
 * A gym station that fills each weapon of the player to its capacity. The player uses it with the
 * verb "interact" (D-156). The ammo pickups of phase 4 are a different rule.
 *
 * The class holds the rule. The level gives the mesh and its size (D-29).
 */
UCLASS()
class IRONABSOLUTION_API AIronAmmoStation : public AActor, public IIronInteractable
{
	GENERATED_BODY()

public:
	AIronAmmoStation();

	/** Fills each weapon of the user. A user that is not the player character gets an error line and no change. */
	virtual void Interact(AActor& User) override;

	/** Gives the body, the part that the player looks at to use the station. */
	UStaticMeshComponent* GetBody() const;

private:
	/** The body of the station. The level sets its mesh and its size. */
	UPROPERTY(VisibleAnywhere, Category = "Station")
	TObjectPtr<UStaticMeshComponent> Body;
};
