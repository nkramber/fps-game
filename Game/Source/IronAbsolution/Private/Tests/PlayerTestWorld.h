// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "Components/StaticMeshComponent.h"
#include "Engine/StaticMesh.h"
#include "Engine/StaticMeshActor.h"
#include "Engine/World.h"
#include "EngineUtils.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "IronHitFlash.h"
#include "IronPlayerCharacter.h"
#include "IronTarget.h"
#include "Misc/AutomationTest.h"
#include "Tests/AutomationCommon.h"

#if WITH_DEV_AUTOMATION_TESTS

// The game world of the tests of the player: the movement of PR-21, the mantle and interact of
// PR-24, the weapon of PR-25, and the melee attack of PR-26. The test world has no local player, so each test calls the functions of the character
// that the input actions call.
namespace IronAbsolution::Tests
{
	const TCHAR* const PlayerCharacterClass = TEXT("/Game/Player/BP_PlayerCharacter.BP_PlayerCharacter_C");
	const TCHAR* const CubeMesh = TEXT("/Engine/BasicShapes/Cube.Cube");

	// A fixed rate of 120 frames each second, the frame rate of the budget (D-32).
	constexpr float FrameSeconds = 1.0f / 120.0f;

	// The time from the spawn to a stand on the floor.
	constexpr float SettleSeconds = 0.5f;

	// The cube of the engine is 100 cm on each side, with its pivot at the center.
	constexpr double CubeSize = 100.0;

	// A game world with a floor of 100 m by 100 m, with its top at Z 0, and one player character on
	// it at the origin. The character looks along +X.
	class FPlayerTestWorld
	{
	public:
		~FPlayerTestWorld()
		{
			if (Wrapper.GetTestWorld() != nullptr)
			{
				Wrapper.DestroyTestWorld(false);
			}
		}

		/** Starts play, places the floor, and spawns the character. Gives null after a test error for each problem. */
		AIronPlayerCharacter* Start(FAutomationTestBase& Test)
		{
			if (!Wrapper.CreateTestWorld(EWorldType::Game) || !Wrapper.BeginPlayInTestWorld())
			{
				Wrapper.ForwardErrorMessages(&Test);
				return nullptr;
			}

			Cube = LoadObject<UStaticMesh>(nullptr, CubeMesh);
			UClass* CharacterClass = LoadClass<AIronPlayerCharacter>(nullptr, PlayerCharacterClass);
			if (!Test.TestNotNull(FString::Printf(TEXT("The mesh %s"), CubeMesh), Cube) || !Test.TestNotNull(FString::Printf(TEXT("The class %s"), PlayerCharacterClass), CharacterClass))
			{
				return nullptr;
			}

			SpawnBlock(FVector(0.0, 0.0, -50.0), FVector(10000.0, 10000.0, 100.0));

			AIronPlayerCharacter* Character = GetWorld()->SpawnActor<AIronPlayerCharacter>(CharacterClass, FVector(0.0, 0.0, 100.0), FRotator::ZeroRotator);
			if (!Test.TestNotNull(TEXT("The spawned player character"), Character))
			{
				return nullptr;
			}

			// With no controller, the movement component moves the character only with this flag. A
			// character sets its first movement mode at spawn when the flag is on, or when a
			// controller takes it. The flag comes after the spawn, so the test sets the mode.
			UCharacterMovementComponent* Movement = Character->GetCharacterMovement();
			Movement->bRunPhysicsWithNoController = true;
			Movement->SetDefaultMovementMode();
			Tick(SettleSeconds, [] {});
			if (!Test.TestTrue(FString::Printf(TEXT("The character stands on the floor after the spawn, in the mode %s"), *Movement->GetMovementName()), Movement->IsMovingOnGround()))
			{
				return nullptr;
			}
			return Character;
		}

		/** Ticks the world for a time at the fixed rate, and calls BeforeFrame before each frame. */
		void Tick(float Seconds, TFunctionRef<void()> BeforeFrame)
		{
			const int32 Frames = FMath::RoundToInt32(Seconds / FrameSeconds);
			for (int32 Frame = 0; Frame < Frames; ++Frame)
			{
				BeforeFrame();
				Wrapper.TickTestWorld(FrameSeconds);
			}
		}

		/** Places a cube of the engine with its center and its size in centimeters. Start must come first. */
		AStaticMeshActor* SpawnBlock(const FVector& Center, const FVector& Size)
		{
			AStaticMeshActor* Block = GetWorld()->SpawnActor<AStaticMeshActor>(Center, FRotator::ZeroRotator);
			// A static component takes no new mesh after play starts, so the block is movable.
			Block->GetStaticMeshComponent()->SetMobility(EComponentMobility::Movable);
			Block->GetStaticMeshComponent()->SetStaticMesh(Cube);
			Block->SetActorScale3D(Size / CubeSize);
			return Block;
		}

		/** Gives the cube mesh of the engine. Start must come first. */
		UStaticMesh* GetCube() const
		{
			return Cube;
		}

		UWorld* GetWorld() const
		{
			return Wrapper.GetTestWorld();
		}

	private:
		FTestWorldWrapper Wrapper;
		UStaticMesh* Cube = nullptr;
	};

	// The board of a gym target: 10 cm deep, 100 cm wide, and 180 cm high.
	const FVector TargetSize(10.0, 100.0, 180.0);

	/** Gives the point on the line of the view at a distance from the eye. */
	inline FVector PointOnView(const AIronPlayerCharacter& Character, double Distance)
	{
		return Character.GetPawnViewLocation() + FVector::ForwardVector * Distance;
	}

	/** Places a test target with the center of its board at a distance from the eye, on the line of the view. */
	inline AIronTarget* SpawnTarget(FPlayerTestWorld& World, const AIronPlayerCharacter& Character, double Distance)
	{
		AIronTarget* Target = World.GetWorld()->SpawnActor<AIronTarget>(PointOnView(Character, Distance), FRotator::ZeroRotator);
		// A static component takes no new mesh after play starts, so the board is movable.
		Target->GetBoard()->SetMobility(EComponentMobility::Movable);
		Target->GetBoard()->SetStaticMesh(World.GetCube());
		Target->GetBoard()->SetRelativeScale3D(TargetSize / CubeSize);
		return Target;
	}

	/** Gives each hit flash of the world. */
	inline TArray<AIronHitFlash*> FindFlashes(UWorld* World)
	{
		TArray<AIronHitFlash*> Flashes;
		for (TActorIterator<AIronHitFlash> It(World); It; ++It)
		{
			Flashes.Add(*It);
		}
		return Flashes;
	}
}

#endif // WITH_DEV_AUTOMATION_TESTS
