// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronGameUserSettings.h"

#include "Engine/Engine.h"
#include "HAL/IConsoleManager.h"
#include "Misc/DefaultValueHelper.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronSettings, Log, All);

namespace IronAbsolution::Aim
{
	bool FAimSettingBounds::Contains(float Value) const
	{
		// A NaN fails each comparison, so the range check alone lets it pass.
		return FMath::IsFinite(Value) && Value >= Min && Value <= Max;
	}

	/** Writes the value into Setting when it is inside its bounds. Otherwise writes an error line, and changes nothing. */
	bool TrySetAimSetting(const FAimSettingBounds& Bounds, float Value, float& Setting)
	{
		if (!Bounds.Contains(Value))
		{
			UE_LOG(LogIronSettings, Error, TEXT("%s %s is outside the bounds %s to %s. The game keeps %s."), Bounds.Name, *FString::SanitizeFloat(Value), *FString::SanitizeFloat(Bounds.Min), *FString::SanitizeFloat(Bounds.Max), *FString::SanitizeFloat(Setting));
			return false;
		}
		Setting = Value;
		return true;
	}

	/** Gives the loaded value when it is inside its bounds. Otherwise writes an error line, and gives the nearest bound. */
	float BoundLoadedAimSetting(const FAimSettingBounds& Bounds, float Value)
	{
		if (Bounds.Contains(Value))
		{
			return Value;
		}

		// A NaN has no nearest bound, so it takes the lower bound.
		const float Used = FMath::IsFinite(Value) ? FMath::Clamp(Value, Bounds.Min, Bounds.Max) : Bounds.Min;
		UE_LOG(LogIronSettings, Error, TEXT("%s %s in the settings %s is outside the bounds %s to %s, so the game uses %s. The defaults of the project are in Config/DefaultGameUserSettings.ini."), Bounds.Name, *FString::SanitizeFloat(Value), *GGameUserSettingsIni, *FString::SanitizeFloat(Bounds.Min), *FString::SanitizeFloat(Bounds.Max), *FString::SanitizeFloat(Used));
		return Used;
	}

	/**
	 * Runs the console command of one aim setting. With no value, it prints the value and the bounds.
	 * With one number inside the bounds, it sets the value and saves the settings file of the user.
	 */
	void RunAimSettingCommand(const TArray<FString>& Args, const FAimSettingBounds& Bounds, TFunctionRef<float()> GetValue, TFunctionRef<bool(float)> SetValue)
	{
		if (Args.IsEmpty())
		{
			UE_LOG(LogIronSettings, Display, TEXT("%s is %s. The bounds are %s to %s."), Bounds.Name, *FString::SanitizeFloat(GetValue()), *FString::SanitizeFloat(Bounds.Min), *FString::SanitizeFloat(Bounds.Max));
			return;
		}

		float Value = 0.0f;
		if (Args.Num() != 1 || !FDefaultValueHelper::ParseFloat(Args[0], Value))
		{
			UE_LOG(LogIronSettings, Error, TEXT("The command of %s takes one number, and got \"%s\". The game keeps %s."), Bounds.Name, *FString::Join(Args, TEXT(" ")), *FString::SanitizeFloat(GetValue()));
			return;
		}

		// SetValue writes the error line of a value out of bounds.
		if (!SetValue(Value))
		{
			return;
		}

		UIronGameUserSettings::Get().SaveSettings();
		UE_LOG(LogIronSettings, Display, TEXT("%s is now %s, saved in %s."), Bounds.Name, *FString::SanitizeFloat(GetValue()), *GGameUserSettingsIni);
	}

	// The console commands let the owner change each value in the package. The menu of phase 8
	// comes later (D-139, D-141).
	FAutoConsoleCommand MouseSensitivityCommand(
		TEXT("Iron.MouseSensitivity"),
		TEXT("Sets the mouse sensitivity, from 0.1 to 20. Each mouse count turns the view 0.022 degrees times the value (D-139). With no value, prints the value."),
		FConsoleCommandWithArgsDelegate::CreateLambda([](const TArray<FString>& Args)
		{
			UIronGameUserSettings& Settings = UIronGameUserSettings::Get();
			RunAimSettingCommand(Args, MouseSensitivityBounds, [&Settings] { return Settings.GetMouseSensitivity(); }, [&Settings](float Value) { return Settings.SetMouseSensitivity(Value); });
		}));

	FAutoConsoleCommand FieldOfViewCommand(
		TEXT("Iron.FieldOfView"),
		TEXT("Sets the horizontal field of view, from 80 to 120 degrees (D-141). With no value, prints the value."),
		FConsoleCommandWithArgsDelegate::CreateLambda([](const TArray<FString>& Args)
		{
			UIronGameUserSettings& Settings = UIronGameUserSettings::Get();
			RunAimSettingCommand(Args, FieldOfViewBounds, [&Settings] { return Settings.GetFieldOfView(); }, [&Settings](float Value) { return Settings.SetFieldOfView(Value); });
		}));
}

UIronGameUserSettings& UIronGameUserSettings::Get()
{
	checkf(GEngine != nullptr, TEXT("The aim settings need the engine, and GEngine is null."));
	UGameUserSettings* Settings = GEngine->GetGameUserSettings();
	UIronGameUserSettings* IronSettings = Cast<UIronGameUserSettings>(Settings);
	checkf(IronSettings != nullptr, TEXT("The engine made the settings object %s, not a UIronGameUserSettings. GameUserSettingsClassName in DefaultEngine.ini must name /Script/IronAbsolution.IronGameUserSettings."), *GetPathNameSafe(Settings));
	return *IronSettings;
}

void UIronGameUserSettings::LoadSettings(bool bForceReload)
{
	Super::LoadSettings(bForceReload);

	MouseSensitivity = IronAbsolution::Aim::BoundLoadedAimSetting(IronAbsolution::Aim::MouseSensitivityBounds, MouseSensitivity);
	FieldOfView = IronAbsolution::Aim::BoundLoadedAimSetting(IronAbsolution::Aim::FieldOfViewBounds, FieldOfView);
}

float UIronGameUserSettings::GetMouseSensitivity() const
{
	return MouseSensitivity;
}

float UIronGameUserSettings::GetDegreesPerMouseCount() const
{
	return IronAbsolution::Aim::DegreesPerCountAtSensitivityOne * MouseSensitivity;
}

bool UIronGameUserSettings::SetMouseSensitivity(float Value)
{
	if (!IronAbsolution::Aim::TrySetAimSetting(IronAbsolution::Aim::MouseSensitivityBounds, Value, MouseSensitivity))
	{
		return false;
	}
	OnAimSettingsChanged.Broadcast();
	return true;
}

float UIronGameUserSettings::GetFieldOfView() const
{
	return FieldOfView;
}

bool UIronGameUserSettings::SetFieldOfView(float Value)
{
	if (!IronAbsolution::Aim::TrySetAimSetting(IronAbsolution::Aim::FieldOfViewBounds, Value, FieldOfView))
	{
		return false;
	}
	OnAimSettingsChanged.Broadcast();
	return true;
}
