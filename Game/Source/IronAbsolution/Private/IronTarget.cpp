// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronTarget.h"

#include "Components/SceneComponent.h"
#include "Components/StaticMeshComponent.h"
#include "Components/TextRenderComponent.h"

AIronTarget::AIronTarget()
{
	// The root has no scale, so the size of the board does not change the size of the text.
	RootComponent = CreateDefaultSubobject<USceneComponent>(TEXT("Root"));

	Board = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Board"));
	Board->SetupAttachment(RootComponent);

	CountText = CreateDefaultSubobject<UTextRenderComponent>(TEXT("CountText"));
	CountText->SetupAttachment(RootComponent);
	CountText->SetHorizontalAlignment(EHTA_Center);
}

void AIronTarget::BeginPlay()
{
	Super::BeginPlay();
	ShowCount();
}

void AIronTarget::RegisterShotHit()
{
	++ShotHitCount;
	ShowCount();
}

void AIronTarget::RegisterMeleeHit()
{
	++MeleeHitCount;
	ShowCount();
}

int32 AIronTarget::GetShotHitCount() const
{
	return ShotHitCount;
}

int32 AIronTarget::GetMeleeHitCount() const
{
	return MeleeHitCount;
}

UStaticMeshComponent* AIronTarget::GetBoard() const
{
	return Board;
}

UTextRenderComponent* AIronTarget::GetCountText() const
{
	return CountText;
}

void AIronTarget::ShowCount()
{
	// The text render component of the engine starts a new line at each newline character.
	CountText->SetText(FText::FromString(FString::Printf(TEXT("Shots: %d\nMelee: %d"), ShotHitCount, MeleeHitCount)));
}
