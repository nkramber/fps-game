// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronHUD.h"

#include "Engine/Canvas.h"
#include "Engine/Engine.h"
#include "Engine/Font.h"
#include "Engine/World.h"
#include "IronPlayerCharacter.h"
#include "IronWeaponComponent.h"
#include "IronWeaponTuning.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronHUD, Log, All);

bool AIronHUD::IsHitMarkerShown(const TOptional<double>& LastTargetHitTime, double Now, float MarkerSeconds)
{
	if (!LastTargetHitTime.IsSet())
	{
		return false;
	}
	const double Age = Now - LastTargetHitTime.GetValue();
	return Age >= 0.0 && Age < MarkerSeconds;
}

TArray<FString> AIronHUD::FindInvalidValues() const
{
	struct FValueCase
	{
		const TCHAR* Name;
		float Value;
	};
	const FValueCase Values[] = {
		{TEXT("CrosshairLength"), CrosshairLength},
		{TEXT("CrosshairGap"), CrosshairGap},
		{TEXT("LineThickness"), LineThickness},
		{TEXT("HitMarkerGap"), HitMarkerGap},
		{TEXT("HitMarkerLength"), HitMarkerLength},
		{TEXT("HitMarkerSeconds"), HitMarkerSeconds},
		{TEXT("TextScale"), TextScale},
		{TEXT("TextMargin"), TextMargin},
		{TEXT("the alpha of Color"), Color.A},
		{TEXT("the alpha of EmptyColor"), EmptyColor.A},
	};

	TArray<FString> Errors;
	for (const FValueCase& Case : Values)
	{
		// A NaN fails the comparison too.
		if (!(Case.Value > 0.0f))
		{
			Errors.Add(FString::Printf(TEXT("%s of %s is %s. It must be above 0."), Case.Name, *GetPathName(), *FString::SanitizeFloat(Case.Value)));
		}
	}
	return Errors;
}

void AIronHUD::BeginPlay()
{
	Super::BeginPlay();

	for (const FString& Error : FindInvalidValues())
	{
		UE_LOG(LogIronHUD, Error, TEXT("%s Set it in the Blueprint subclass (D-29)."), *Error);
	}
}

void AIronHUD::DrawHUD()
{
	Super::DrawHUD();

	// A frame with no player character, for example at the start of a map, has nothing to show.
	const AIronPlayerCharacter* Character = Cast<AIronPlayerCharacter>(GetOwningPawn());
	if (Character == nullptr || Canvas == nullptr)
	{
		return;
	}

	const FVector2D Center(Canvas->ClipX / 2.0, Canvas->ClipY / 2.0);
	DrawCrosshair(Center);

	const UIronWeaponComponent* Weapon = Character->GetWeapon();
	const UIronWeaponTuning* Tuning = Weapon->GetCurrentWeapon();
	if (Tuning == nullptr)
	{
		return;
	}
	if (IsHitMarkerShown(Weapon->GetLastTargetHitTime(), GetWorld()->GetTimeSeconds(), HitMarkerSeconds))
	{
		DrawHitMarker(Center);
	}
	DrawAmmo(Tuning->DisplayName.ToString(), Weapon->GetCurrentAmmo());
}

void AIronHUD::DrawCrosshair(const FVector2D& Center)
{
	const float Near = CrosshairGap;
	const float Far = CrosshairGap + CrosshairLength;
	DrawLine(Center.X - Far, Center.Y, Center.X - Near, Center.Y, Color, LineThickness);
	DrawLine(Center.X + Near, Center.Y, Center.X + Far, Center.Y, Color, LineThickness);
	DrawLine(Center.X, Center.Y - Far, Center.X, Center.Y - Near, Color, LineThickness);
	DrawLine(Center.X, Center.Y + Near, Center.X, Center.Y + Far, Color, LineThickness);
}

void AIronHUD::DrawHitMarker(const FVector2D& Center)
{
	const float Near = HitMarkerGap;
	const float Far = HitMarkerGap + HitMarkerLength;
	DrawLine(Center.X - Far, Center.Y - Far, Center.X - Near, Center.Y - Near, Color, LineThickness);
	DrawLine(Center.X + Near, Center.Y - Near, Center.X + Far, Center.Y - Far, Color, LineThickness);
	DrawLine(Center.X - Far, Center.Y + Far, Center.X - Near, Center.Y + Near, Color, LineThickness);
	DrawLine(Center.X + Near, Center.Y + Near, Center.X + Far, Center.Y + Far, Color, LineThickness);
}

void AIronHUD::DrawAmmo(const FString& WeaponName, int32 Rounds)
{
	UFont* Font = GEngine->GetLargeFont();
	const FString Text = FString::Printf(TEXT("%s  %d"), *WeaponName, Rounds);
	float Width = 0.0f;
	float Height = 0.0f;
	GetTextSize(Text, Width, Height, Font, TextScale);
	const FLinearColor TextColor = Rounds > 0 ? Color : EmptyColor;
	DrawText(Text, TextColor, Canvas->ClipX - TextMargin - Width, Canvas->ClipY - TextMargin - Height, Font, TextScale);
}
