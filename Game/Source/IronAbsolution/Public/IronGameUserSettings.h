// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameUserSettings.h"

#include "IronGameUserSettings.generated.h"

namespace IronAbsolution::Aim
{
	/** The name and the bounds of one aim setting (D-139, D-141). */
	struct FAimSettingBounds
	{
		const TCHAR* Name;
		float Min;
		float Max;

		/** True when the value is finite and inside the bounds. */
		bool Contains(float Value) const;
	};

	/** The bounds of the mouse sensitivity (D-139). */
	inline constexpr FAimSettingBounds MouseSensitivityBounds{TEXT("MouseSensitivity"), 0.1f, 20.0f};

	/** The bounds of the horizontal field of view, in degrees (D-141). */
	inline constexpr FAimSettingBounds FieldOfViewBounds{TEXT("FieldOfView"), 80.0f, 120.0f};

	/** The turn of the view for one mouse count at a sensitivity of 1: the scale of Quake and Source games (D-139). */
	inline constexpr float DegreesPerCountAtSensitivityOne = 0.022f;
}

/**
 * The settings of the player, in the settings file of the user (D-34). The engine makes one object
 * of this class, because `DefaultEngine.ini` names it in GameUserSettingsClassName. This class adds
 * the aim settings: the mouse sensitivity and the field of view. The game has no vertical invert (D-140).
 *
 * The defaults of the project are in `Game/Config/DefaultGameUserSettings.ini`, not in C++ (D-29).
 * The file of the user holds each value that the player changed. Each C++ initial value is 0, which
 * is out of bounds, so a default that the project file does not set gives an error at load (T-2).
 */
UCLASS()
class IRONABSOLUTION_API UIronGameUserSettings : public UGameUserSettings
{
	GENERATED_BODY()

public:
	/**
	 * Gives the settings object of the engine. A check stops the game when the engine made an object
	 * of another class, because then `DefaultEngine.ini` does not name this class (D-34).
	 */
	static UIronGameUserSettings& Get();

	/**
	 * Loads the settings of the engine and the aim settings. An aim setting out of its bounds gives
	 * an error line with the name, the value, and the bounds. The game then uses the nearest bound,
	 * because at load it has no earlier good value.
	 */
	virtual void LoadSettings(bool bForceReload = false) override;

	/** Gives the mouse sensitivity on the scale of D-139. */
	float GetMouseSensitivity() const;

	/** Gives the turn of the view for one mouse count, in degrees: 0.022 times the mouse sensitivity (D-139). */
	float GetDegreesPerMouseCount() const;

	/**
	 * Sets the mouse sensitivity, and tells each listener of OnAimSettingsChanged. The value does not
	 * go to the file until SaveSettings.
	 * @return True when the value is inside its bounds. False after an error line, with no change.
	 */
	bool SetMouseSensitivity(float Value);

	/** Gives the horizontal field of view, in degrees (D-141). */
	float GetFieldOfView() const;

	/**
	 * Sets the horizontal field of view, and tells each listener of OnAimSettingsChanged. The value
	 * does not go to the file until SaveSettings.
	 * @return True when the value is inside its bounds. False after an error line, with no change.
	 */
	bool SetFieldOfView(float Value);

	/** Broadcasts after each change of an aim setting. The player character reads the field of view again. */
	FSimpleMulticastDelegate OnAimSettingsChanged;

private:
	/** The mouse sensitivity on the scale of D-139. */
	UPROPERTY(config)
	float MouseSensitivity = 0.0f;

	/** The horizontal field of view, in degrees (D-141). */
	UPROPERTY(config)
	float FieldOfView = 0.0f;
};
