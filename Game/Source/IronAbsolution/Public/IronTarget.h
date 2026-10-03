// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"

#include "IronTarget.generated.h"

class UStaticMeshComponent;
class UTextRenderComponent;

/**
 * A gym target that counts the hits of shots and of melee attacks apart, and shows the two counts
 * above itself on two lines. The target has no health, because phase 4 builds damage and health
 * (sections 7.5 and 7.6 of `docs/roadmaps/phase-3-core-feel.md`).
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

	/** Adds one hit to the count of shots, and shows the new counts. The weapon calls it for each pellet that hits the board. */
	void RegisterShotHit();

	/** Adds one hit to the count of melee attacks, and shows the new counts. The melee component calls it for each attack that hits the target. */
	void RegisterMeleeHit();

	/** Gives the count of pellets that hit the target since the start of play. */
	int32 GetShotHitCount() const;

	/** Gives the count of melee attacks that hit the target since the start of play. */
	int32 GetMeleeHitCount() const;

	/** Gives the board, the part that a shot hits. */
	UStaticMeshComponent* GetBoard() const;

	/** Gives the text that shows the two counts. */
	UTextRenderComponent* GetCountText() const;

protected:
	virtual void BeginPlay() override;

private:
	/** Writes the two counts into the text, one on each line. */
	void ShowCount();

	/** The board of the target. The level sets its mesh and its size. */
	UPROPERTY(VisibleAnywhere, Category = "Target")
	TObjectPtr<UStaticMeshComponent> Board;

	/** The two counts, above the board. The level sets its place, its size, and its color. */
	UPROPERTY(VisibleAnywhere, Category = "Target")
	TObjectPtr<UTextRenderComponent> CountText;

	int32 ShotHitCount = 0;

	int32 MeleeHitCount = 0;
};
