// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronMeleeTuning.h"

#include "IronHitFlash.h"

TArray<FString> UIronMeleeTuning::FindInvalidValues() const
{
	// One number of the tuning, with its lowest value. The values repeat the clamps of the header,
	// because a packaged game has no metadata to read them from.
	struct FValueMin
	{
		const TCHAR* Name;
		float Value;
		float Min;
	};
	const FValueMin Values[] = {
		{TEXT("Range"), Range, 1.0f},
		{TEXT("SweepRadius"), SweepRadius, 1.0f},
		{TEXT("AttackInterval"), AttackInterval, 0.01f},
		{TEXT("JabDistance"), JabDistance, 0.1f},
		{TEXT("JabTime"), JabTime, 0.01f},
	};

	TArray<FString> Errors;
	for (const FValueMin& Entry : Values)
	{
		// A NaN fails each comparison, so the minimum check alone lets it pass.
		if (!FMath::IsFinite(Entry.Value) || Entry.Value < Entry.Min)
		{
			Errors.Add(FString::Printf(TEXT("%s of %s is %s. It must be finite and at least %s."), Entry.Name, *GetPathName(), *FString::SanitizeFloat(Entry.Value), *FString::SanitizeFloat(Entry.Min)));
		}
	}

	// A sphere as large as the range starts past the end of the sweep, so the attack has no direction.
	if (SweepRadius >= Range)
	{
		Errors.Add(FString::Printf(TEXT("SweepRadius of %s is %s, and Range is %s. The radius must be below the range."), *GetPathName(), *FString::SanitizeFloat(SweepRadius), *FString::SanitizeFloat(Range)));
	}

	// A jab longer than the time between two attacks starts again before it comes back, so the weapon jumps in the view.
	if (JabTime > AttackInterval)
	{
		Errors.Add(FString::Printf(TEXT("JabTime of %s is %s s, and AttackInterval is %s s. The jab must not be longer than the time between two attacks."), *GetPathName(), *FString::SanitizeFloat(JabTime), *FString::SanitizeFloat(AttackInterval)));
	}

	if (HitFlashClass == nullptr)
	{
		Errors.Add(FString::Printf(TEXT("HitFlashClass of %s is absent. Set it in the data asset."), *GetPathName()));
	}

	return Errors;
}
