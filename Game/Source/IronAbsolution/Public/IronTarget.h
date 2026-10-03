// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"

#include "IronTarget.generated.h"

class UStaticMeshComponent;
class UTextRenderComponent;

/**
 * A gym target that counts each hit and shows the count above itself. The target has no health,
 * because phase 4 builds damage and health (section 7.5 of `docs/roadmaps/phase-3-core-feel.md`).
 *
 * The class holds the rule. The level gives the mesh of the board, its size, and the look of the
 * count (D-29).
 */
UCLASS()
class IRONABSOLUTION_API AIronTarget : public AActor
{
	GENERATED_BODY()

public:
	AIronTarget();

	/** Adds one hit to the count, and shows the new count. The weapon calls it for each pellet that hits the board. */
	void RegisterHit();

	/** Gives the count of hits since the start of play. */
	int32 GetHitCount() const;

	/** Gives the board, the part that a shot hits. */
	UStaticMeshComponent* GetBoard() const;

	/** Gives the text that shows the count. */
	UTextRenderComponent* GetCountText() const;

protected:
	virtual void BeginPlay() override;

private:
	/** Writes the count into the text. */
	void ShowCount();

	/** The board of the target. The level sets its mesh and its size. */
	UPROPERTY(VisibleAnywhere, Category = "Target")
	TObjectPtr<UStaticMeshComponent> Board;

	/** The count of hits, above the board. The level sets its place, its size, and its color. */
	UPROPERTY(VisibleAnywhere, Category = "Target")
	TObjectPtr<UTextRenderComponent> CountText;

	int32 HitCount = 0;
};
