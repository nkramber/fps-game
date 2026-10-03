// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"

#include "IronHitFlash.generated.h"

class UMaterialInstanceDynamic;
class UMaterialInterface;
class UStaticMesh;
class UStaticMeshComponent;

/**
 * A small bright flash at the point of a hit. It fades out over its life, and then removes itself
 * (D-154). The weapon component spawns one for each pellet that hits a surface.
 *
 * The class holds the rule. A Blueprint subclass holds the mesh, the material, the size, and the
 * life (D-29). The flash has no collision, so a later shot goes through it. The class is not
 * abstract, so a test can spawn it with no values and see the error.
 */
UCLASS()
class IRONABSOLUTION_API AIronHitFlash : public AActor
{
	GENERATED_BODY()

public:
	/** The name of the scalar parameter of the material that the flash sets from 1 down to 0. */
	static const FName BrightnessParameter;

	AIronHitFlash();

	virtual void Tick(float DeltaSeconds) override;

	/** Gives the mesh of the flash. */
	UStaticMeshComponent* GetFlashMesh() const;

	/** Gives the life of the flash in seconds, from the Blueprint subclass. */
	float GetLifetime() const;

	/** Gives the brightness of the flash now: 1 at the spawn, and 0 at the end of the life. */
	float GetBrightness() const;

protected:
	virtual void BeginPlay() override;

private:
	/** The mesh of the flash. BeginPlay sets its mesh, its material, and its size. */
	UPROPERTY(VisibleAnywhere, Category = "Flash")
	TObjectPtr<UStaticMeshComponent> Mesh;

	/** The shape of the flash. The Blueprint subclass sets it. */
	UPROPERTY(EditDefaultsOnly, Category = "Flash", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UStaticMesh> FlashMesh;

	/** The material of the flash, with the scalar parameter BrightnessParameter. The Blueprint subclass sets it. */
	UPROPERTY(EditDefaultsOnly, Category = "Flash", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UMaterialInterface> FlashMaterial;

	/** The size of the flash, from one side to the other. The shape of the mesh is 100 cm. */
	UPROPERTY(EditDefaultsOnly, Category = "Flash", meta = (AllowPrivateAccess = "true", ClampMin = "0.1", Units = "cm"))
	float Diameter = 0.0f;

	/** The time from the spawn to the end of the fade. */
	UPROPERTY(EditDefaultsOnly, Category = "Flash", meta = (AllowPrivateAccess = "true", ClampMin = "0.01", Units = "s"))
	float Lifetime = 0.0f;

	/** The instance of the material of this flash, with its own brightness. */
	UPROPERTY(Transient)
	TObjectPtr<UMaterialInstanceDynamic> FlashMaterialInstance;

	/** The time of the world at the spawn. */
	double SpawnTime = 0.0;
};
