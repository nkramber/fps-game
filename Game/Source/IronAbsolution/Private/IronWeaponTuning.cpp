// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronWeaponTuning.h"

#include "IronHitFlash.h"
#include "Engine/StaticMesh.h"
#include "Sound/SoundBase.h"

namespace IronAbsolution::WeaponTuning
{
	// One number of the tuning, with its range. The ranges repeat the clamps of the header, because
	// a packaged game has no metadata to read them from.
	struct FValueRange
	{
		const TCHAR* Name;
		double Value;
		double Min;
		double Max;
	};
}

float UIronWeaponTuning::GetShotInterval() const
{
	return 1.0f / ShotsPerSecond;
}

TArray<FString> UIronWeaponTuning::FindInvalidValues() const
{
	using IronAbsolution::WeaponTuning::FValueRange;

	const double NoMax = TNumericLimits<double>::Max();
	const FValueRange Ranges[] = {
		{TEXT("ShotsPerSecond"), ShotsPerSecond, 0.1, NoMax},
		{TEXT("AmmoCapacity"), static_cast<double>(AmmoCapacity), 1.0, NoMax},
		{TEXT("Range"), Range, 1.0, NoMax},
		{TEXT("PelletCount"), static_cast<double>(PelletCount), 1.0, NoMax},
		{TEXT("SpreadAngle"), SpreadAngle, 0.0, 45.0},
		{TEXT("RaiseTime"), RaiseTime, 0.01, NoMax},
		{TEXT("RecoilKick"), RecoilKick, 0.01, 45.0},
		{TEXT("RecoilSideKick"), RecoilSideKick, 0.01, 45.0},
		{TEXT("RecoilRecoveryTime"), RecoilRecoveryTime, 0.01, NoMax},
		{TEXT("ShotSoundStartTime"), ShotSoundStartTime, 0.0, NoMax},
	};

	TArray<FString> Errors;
	for (const FValueRange& Entry : Ranges)
	{
		// A NaN fails each comparison, so the range check alone lets it pass.
		if (!FMath::IsFinite(Entry.Value) || Entry.Value < Entry.Min || Entry.Value > Entry.Max)
		{
			const FString MaxText = Entry.Max == NoMax ? FString(TEXT("no maximum")) : FString::SanitizeFloat(Entry.Max);
			Errors.Add(FString::Printf(TEXT("%s of %s is %s. The range is %s to %s."), Entry.Name, *GetPathName(), *FString::SanitizeFloat(Entry.Value), *FString::SanitizeFloat(Entry.Min), *MaxText));
		}
	}

	// More pellets with no spread go along one line, so each shot counts as many hits.
	if (PelletCount > 1 && !(SpreadAngle > 0.0f))
	{
		Errors.Add(FString::Printf(TEXT("SpreadAngle of %s is %s with %d pellets. More than one pellet needs a spread above 0."), *GetPathName(), *FString::SanitizeFloat(SpreadAngle), PelletCount));
	}

	if (DisplayName.IsEmpty())
	{
		Errors.Add(FString::Printf(TEXT("DisplayName of %s is empty, so the HUD shows no name for the weapon."), *GetPathName()));
	}

	struct FAbsentCase
	{
		const TCHAR* Name;
		bool bAbsent;
	};
	const FAbsentCase Content[] = {
		{TEXT("ShotSound"), ShotSound == nullptr},
		{TEXT("EmptySound"), EmptySound == nullptr},
		{TEXT("HitFlashClass"), HitFlashClass == nullptr},
		{TEXT("ViewMesh"), ViewMesh == nullptr},
	};
	for (const FAbsentCase& Case : Content)
	{
		if (Case.bAbsent)
		{
			Errors.Add(FString::Printf(TEXT("%s of %s is absent. Set it in the data asset."), Case.Name, *GetPathName()));
		}
	}

	// A start at or after the end of the sound plays nothing, so the shot has no sound and no other line tells why.
	if (ShotSound != nullptr && ShotSoundStartTime >= ShotSound->GetDuration())
	{
		Errors.Add(FString::Printf(TEXT("ShotSoundStartTime of %s is %s, and the sound %s lasts %s s. The start must be below the length."), *GetPathName(), *FString::SanitizeFloat(ShotSoundStartTime), *ShotSound->GetPathName(), *FString::SanitizeFloat(ShotSound->GetDuration())));
	}

	if (ViewScale.ContainsNaN() || ViewScale.X <= 0.0 || ViewScale.Y <= 0.0 || ViewScale.Z <= 0.0)
	{
		Errors.Add(FString::Printf(TEXT("ViewScale of %s is %s. Each part must be finite and above 0."), *GetPathName(), *ViewScale.ToString()));
	}
	if (ViewOffset.ContainsNaN())
	{
		Errors.Add(FString::Printf(TEXT("ViewOffset of %s is %s. Each part must be finite."), *GetPathName(), *ViewOffset.ToString()));
	}

	return Errors;
}
