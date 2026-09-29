// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronMovementTuning.h"

namespace IronAbsolution::MovementTuning
{
	// One value of the tuning, with its range. The ranges repeat the clamps of the header, because
	// a packaged game has no metadata to read them from.
	struct FValueRange
	{
		const TCHAR* Name;
		float Value;
		float Min;
		float Max;
	};
}

TArray<FString> UIronMovementTuning::FindInvalidValues() const
{
	using IronAbsolution::MovementTuning::FValueRange;

	const float NoMax = TNumericLimits<float>::Max();
	const FValueRange Ranges[] = {
		{TEXT("RunSpeed"), RunSpeed, 1.0f, NoMax},
		{TEXT("Acceleration"), Acceleration, 1.0f, NoMax},
		{TEXT("BrakingDeceleration"), BrakingDeceleration, 1.0f, NoMax},
		{TEXT("GroundFriction"), GroundFriction, 0.1f, NoMax},
		{TEXT("JumpHeight"), JumpHeight, 1.0f, NoMax},
		{TEXT("GravityScale"), GravityScale, 0.1f, NoMax},
		{TEXT("AirControl"), AirControl, 0.01f, 1.0f},
		{TEXT("StepHeight"), StepHeight, 1.0f, NoMax},
		{TEXT("WalkableSlope"), WalkableSlope, 1.0f, 89.0f},
		{TEXT("EyeHeight"), EyeHeight, 1.0f, NoMax},
		{TEXT("FieldOfView"), FieldOfView, 60.0f, 130.0f},
	};

	TArray<FString> Errors;
	for (const FValueRange& Range : Ranges)
	{
		// A NaN fails each comparison, so the range check alone lets it pass.
		if (!FMath::IsFinite(Range.Value) || Range.Value < Range.Min || Range.Value > Range.Max)
		{
			const FString MaxText = Range.Max == NoMax ? FString(TEXT("no maximum")) : FString::SanitizeFloat(Range.Max);
			Errors.Add(FString::Printf(TEXT("%s of %s is %s. The range is %s to %s."), Range.Name, *GetPathName(), *FString::SanitizeFloat(Range.Value), *FString::SanitizeFloat(Range.Min), *MaxText));
		}
	}

	return Errors;
}
