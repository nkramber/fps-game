// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/HUD.h"

#include "IronHUD.generated.h"

/**
 * The small HUD of phase 3: a crosshair, the name and the ammo of the weapon in hand, and the hit
 * marker (D-154, the pillar "Every round counts"). The ammo shows in the empty color at 0 (D-158).
 * A shot and a melee attack that hit a gym target show the same hit marker (D-171).
 * Phase 8 holds the final interface.
 *
 * The class holds the rule. A Blueprint subclass holds each size, time, and color (D-29). Each size
 * is in pixels.
 */
UCLASS(Abstract)
class IRONABSOLUTION_API AIronHUD : public AHUD
{
	GENERATED_BODY()

public:
	/**
	 * Tells if the hit marker shows.
	 * @param LastTargetHitTime The time of the world of the last shot or melee attack that hit a target, or no value before the first such hit.
	 * @param Now The time of the world now.
	 * @param MarkerSeconds The time that the marker shows after a hit.
	 * @return True from the hit until the end of the time of the marker.
	 */
	static bool IsHitMarkerShown(const TOptional<double>& LastTargetHitTime, double Now, float MarkerSeconds);

	/**
	 * Gives the later of two times of a hit.
	 * @param ShotHitTime The time of the last shot that hit a target, or no value.
	 * @param MeleeHitTime The time of the last melee attack that hit a target, or no value.
	 * @return The later time, the one time that has a value, or no value when neither has one.
	 */
	static TOptional<double> GetLatestHitTime(const TOptional<double>& ShotHitTime, const TOptional<double>& MeleeHitTime);

	/**
	 * Finds each size and time that is not above 0.
	 * @return One line for each invalid value, with its name and its value. Empty when each value is valid.
	 */
	TArray<FString> FindInvalidValues() const;

	virtual void DrawHUD() override;

protected:
	virtual void BeginPlay() override;

private:
	/** Draws four short lines around the center of the screen, with a gap at the center. */
	void DrawCrosshair(const FVector2D& Center);

	/** Draws an X of four short lines around the crosshair. */
	void DrawHitMarker(const FVector2D& Center);

	/** Draws the name and the rounds of the weapon in hand at the lower right of the screen. */
	void DrawAmmo(const FString& WeaponName, int32 Rounds);

	/** The color of the crosshair, the hit marker, and the ammo. */
	UPROPERTY(EditDefaultsOnly, Category = "HUD", meta = (AllowPrivateAccess = "true"))
	FLinearColor Color = FLinearColor::Transparent;

	/** The color of the ammo of an empty weapon (D-158). */
	UPROPERTY(EditDefaultsOnly, Category = "HUD", meta = (AllowPrivateAccess = "true"))
	FLinearColor EmptyColor = FLinearColor::Transparent;

	/** The length of each line of the crosshair. */
	UPROPERTY(EditDefaultsOnly, Category = "HUD", meta = (AllowPrivateAccess = "true", ClampMin = "1"))
	float CrosshairLength = 0.0f;

	/** The gap from the center of the screen to each line of the crosshair. */
	UPROPERTY(EditDefaultsOnly, Category = "HUD", meta = (AllowPrivateAccess = "true", ClampMin = "1"))
	float CrosshairGap = 0.0f;

	/** The thickness of each line of the crosshair and the hit marker. */
	UPROPERTY(EditDefaultsOnly, Category = "HUD", meta = (AllowPrivateAccess = "true", ClampMin = "1"))
	float LineThickness = 0.0f;

	/** The distance from the center of the screen to the inner end of each line of the hit marker, on each axis. */
	UPROPERTY(EditDefaultsOnly, Category = "HUD", meta = (AllowPrivateAccess = "true", ClampMin = "1"))
	float HitMarkerGap = 0.0f;

	/** The length of each line of the hit marker, on each axis. */
	UPROPERTY(EditDefaultsOnly, Category = "HUD", meta = (AllowPrivateAccess = "true", ClampMin = "1"))
	float HitMarkerLength = 0.0f;

	/** The time that the hit marker shows after a shot hits a target (D-154). */
	UPROPERTY(EditDefaultsOnly, Category = "HUD", meta = (AllowPrivateAccess = "true", ClampMin = "0.01", Units = "s"))
	float HitMarkerSeconds = 0.0f;

	/** The scale of the large font of the engine for the ammo. */
	UPROPERTY(EditDefaultsOnly, Category = "HUD", meta = (AllowPrivateAccess = "true", ClampMin = "0.1"))
	float TextScale = 0.0f;

	/** The distance of the ammo text from the right edge and the lower edge of the screen. */
	UPROPERTY(EditDefaultsOnly, Category = "HUD", meta = (AllowPrivateAccess = "true", ClampMin = "1"))
	float TextMargin = 0.0f;
};
