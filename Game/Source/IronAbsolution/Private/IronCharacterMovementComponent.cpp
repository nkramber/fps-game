// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronCharacterMovementComponent.h"

#include "Engine/World.h"
#include "GameFramework/Character.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronMovement, Log, All);

namespace IronAbsolution::Mantle
{
	// The rules of the traces of the mantle. They follow from the size of the capsule, not from the
	// feel, so they are rules of the class and not values of the tuning (D-29).

	// The distance of the sweep that finds the wall in front of the capsule.
	constexpr float WallReach = 30.0f;

	// The input moves the player forward when its direction is within 60 degrees of the view.
	constexpr float MinForwardInput = 0.5f;

	// The wall faces the player when its normal is within 60 degrees of the view, reversed.
	constexpr float MinWallFacing = 0.5f;

	// The capsule on the ledge stands this far past the edge, in addition to its radius.
	constexpr float StandInset = 5.0f;

	// The end of the climb puts the feet this far above the ledge top, so the capsule does not touch it.
	constexpr float TopClearance = 2.0f;

	// The trace of the ledge top starts and ends this far outside the band. A ledge top at a bound
	// is then inside the trace, and the check of the height decides.
	constexpr float BandMargin = 1.0f;

	// The check of the height lets a ledge top this far outside a bound pass, for the float error
	// of a trace. A ledge at a bound is then in the band.
	constexpr float HeightTolerance = 0.1f;

	// The sweeps of the path use a capsule this much thinner, so a wall that touches the capsule
	// at the start does not block a move along it.
	constexpr float PathShrink = 1.0f;
}

void UIronCharacterMovementComponent::SetMantleTuning(float MinHeight, float MaxHeight, float Time)
{
	MantleMinHeight = MinHeight;
	MantleMaxHeight = MaxHeight;
	MantleTime = Time;
}

TOptional<FIronLedge> UIronCharacterMovementComponent::FindLedge() const
{
	using namespace IronAbsolution::Mantle;

	// The character logs an error when it has no valid tuning (T-2). An empty band finds no ledge.
	const UWorld* World = GetWorld();
	if (World == nullptr || UpdatedComponent == nullptr || CharacterOwner == nullptr || !(MantleMinHeight < MantleMaxHeight))
	{
		return {};
	}

	FCollisionQueryParams Params(SCENE_QUERY_STAT(IronMantle), false, CharacterOwner);
	FCollisionResponseParams ResponseParams;
	InitCollisionParams(Params, ResponseParams);
	const ECollisionChannel Channel = UpdatedComponent->GetCollisionObjectType();
	const FCollisionShape Capsule = GetPawnCapsuleCollisionShape(SHRINK_None);
	const FCollisionShape PathCapsule = GetPawnCapsuleCollisionShape(SHRINK_RadiusCustom, PathShrink);

	const FVector Location = UpdatedComponent->GetComponentLocation();
	const double FeetZ = Location.Z - Capsule.GetCapsuleHalfHeight();
	const FVector Forward = CharacterOwner->GetActorForwardVector().GetSafeNormal2D();

	// 1. The wall: the capsule moves forward and hits a face that looks back at the player.
	FHitResult Wall;
	if (!World->SweepSingleByChannel(Wall, Location, Location + Forward * WallReach, FQuat::Identity, Channel, Capsule, Params, ResponseParams)
		|| Wall.bStartPenetrating
		|| FVector::DotProduct(-Wall.ImpactNormal.GetSafeNormal2D(), Forward) < MinWallFacing)
	{
		return {};
	}

	// 2. The ledge top: a line down at the place where the capsule stands on the ledge. A ledge
	// above the band holds the start of the line, so the line finds no top in the band.
	const FVector StandPoint = Wall.ImpactPoint + Forward * (Capsule.GetCapsuleRadius() + StandInset);
	const FVector TopStart(StandPoint.X, StandPoint.Y, FeetZ + MantleMaxHeight + BandMargin);
	const FVector TopEnd(StandPoint.X, StandPoint.Y, FeetZ + MantleMinHeight - BandMargin);
	FHitResult Top;
	if (!World->LineTraceSingleByChannel(Top, TopStart, TopEnd, Channel, Params, ResponseParams) || Top.bStartPenetrating || !IsWalkable(Top))
	{
		return {};
	}
	const float Height = static_cast<float>(Top.ImpactPoint.Z - FeetZ);
	if (Height < MantleMinHeight - HeightTolerance || Height > MantleMaxHeight + HeightTolerance)
	{
		return {};
	}

	// 3. The room: the capsule fits on the ledge, and nothing blocks the path up and then forward.
	FIronLedge Ledge;
	Ledge.Height = Height;
	Ledge.StandLocation = FVector(StandPoint.X, StandPoint.Y, Top.ImpactPoint.Z + Capsule.GetCapsuleHalfHeight() + TopClearance);
	Ledge.RiseLocation = FVector(Location.X, Location.Y, Ledge.StandLocation.Z);
	FHitResult Blocked;
	if (World->OverlapBlockingTestByChannel(Ledge.StandLocation, FQuat::Identity, Channel, Capsule, Params, ResponseParams)
		|| World->SweepSingleByChannel(Blocked, Location, Ledge.RiseLocation, FQuat::Identity, Channel, PathCapsule, Params, ResponseParams)
		|| World->SweepSingleByChannel(Blocked, Ledge.RiseLocation, Ledge.StandLocation, FQuat::Identity, Channel, PathCapsule, Params, ResponseParams))
	{
		return {};
	}

	return Ledge;
}

bool UIronCharacterMovementComponent::IsMantling() const
{
	return MovementMode == MOVE_Custom && CustomMovementMode == static_cast<uint8>(EIronCustomMovementMode::Mantle);
}

void UIronCharacterMovementComponent::UpdateCharacterStateBeforeMovement(float DeltaSeconds)
{
	Super::UpdateCharacterStateBeforeMovement(DeltaSeconds);

	// The mantle starts in the air alone (D-143). On the ground, a jump starts it.
	if (MovementMode != MOVE_Falling || !WantsToMoveForward())
	{
		return;
	}

	const TOptional<FIronLedge> Ledge = FindLedge();
	if (Ledge.IsSet())
	{
		StartMantle(Ledge.GetValue());
	}
}

void UIronCharacterMovementComponent::PhysCustom(float DeltaTime, int32 Iterations)
{
	if (!IsMantling())
	{
		Super::PhysCustom(DeltaTime, Iterations);
		return;
	}
	if (DeltaTime < MIN_TICK_TIME)
	{
		return;
	}

	// The climb follows the path at a steady speed and ignores the input (D-143).
	MantleElapsed += DeltaTime;
	const float Alpha = FMath::Min(MantleElapsed / MantleTime, 1.0f);
	const FVector OldLocation = UpdatedComponent->GetComponentLocation();
	FHitResult Hit;
	SafeMoveUpdatedComponent(GetMantlePathPoint(Alpha) - OldLocation, UpdatedComponent->GetComponentQuat(), true, Hit);
	Velocity = (UpdatedComponent->GetComponentLocation() - OldLocation) / DeltaTime;

	// A thing that moves into the path after the start, such as a door, stops the climb. It is an
	// event of the game, not an error.
	if (Hit.bBlockingHit)
	{
		UE_LOG(LogIronMovement, Verbose, TEXT("%s stopped the mantle: %s blocks the path."), *GetPathNameSafe(CharacterOwner), *GetNameSafe(Hit.GetActor()));
		EndMantle();
		return;
	}
	if (Alpha >= 1.0f)
	{
		EndMantle();
	}
}

bool UIronCharacterMovementComponent::WantsToMoveForward() const
{
	using namespace IronAbsolution::Mantle;

	const FVector Input = GetCurrentAcceleration().GetSafeNormal2D();
	const FVector Forward = CharacterOwner->GetActorForwardVector().GetSafeNormal2D();
	return FVector::DotProduct(Input, Forward) >= MinForwardInput;
}

void UIronCharacterMovementComponent::StartMantle(const FIronLedge& Ledge)
{
	MantleStart = UpdatedComponent->GetComponentLocation();
	MantleLedge = Ledge;
	MantleElapsed = 0.0f;
	Velocity = FVector::ZeroVector;
	SetMovementMode(MOVE_Custom, static_cast<uint8>(EIronCustomMovementMode::Mantle));
}

FVector UIronCharacterMovementComponent::GetMantlePathPoint(float Alpha) const
{
	// The path has two straight parts: up, then forward. The speed is the same on each part.
	const double RiseLength = FVector::Dist(MantleStart, MantleLedge.RiseLocation);
	const double ForwardLength = FVector::Dist(MantleLedge.RiseLocation, MantleLedge.StandLocation);
	const double Distance = Alpha * (RiseLength + ForwardLength);
	if (Distance < RiseLength)
	{
		return FMath::Lerp(MantleStart, MantleLedge.RiseLocation, Distance / RiseLength);
	}
	// The stand point is past the edge by more than the radius, so the forward part is never empty.
	return FMath::Lerp(MantleLedge.RiseLocation, MantleLedge.StandLocation, (Distance - RiseLength) / ForwardLength);
}

void UIronCharacterMovementComponent::EndMantle()
{
	// The walk mode finds the floor under the capsule. With no floor, the player falls.
	Velocity = FVector::ZeroVector;
	SetMovementMode(MOVE_Walking);
}
