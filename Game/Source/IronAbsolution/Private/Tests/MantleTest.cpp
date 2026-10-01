// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Components/CapsuleComponent.h"
#include "Engine/StaticMeshActor.h"
#include "IronCharacterMovementComponent.h"
#include "IronMovementTuning.h"
#include "IronPlayerCharacter.h"
#include "Misc/AutomationTest.h"
#include "PlayerTestWorld.h"

#if WITH_DEV_AUTOMATION_TESTS

// The mantle tests of PR-24 (D-143). Each test places a ledge in front of the player character,
// who looks along +X, and reads the band and the time from the tuning of the Blueprint.
namespace IronAbsolution::Tests::Mantle
{
	// Each ledge is 200 cm deep and 300 cm wide, with its face across the view.
	constexpr double LedgeDepth = 200.0;
	constexpr double LedgeWidth = 300.0;

	// The face of a ledge for a check in place: inside the reach of the sweep of the wall.
	constexpr double NearGap = 10.0;

	// The face of a ledge for a jump: the player runs toward it for a short time first.
	constexpr double JumpGap = 50.0;

	// The upper limit of a jump from the floor is the jump height plus the top of the band. The
	// top of the jump falls between two frames, so each case stands this far inside or outside it.
	constexpr float JumpLimitMargin = 5.0f;

	// The time of a jump and a climb, with a margin.
	constexpr float ClimbLimitSeconds = 1.5f;

	// The climb time is a count of whole frames, so the pass rule allows two frames of error.
	constexpr float ClimbTimeTolerance = 2.0f * FrameSeconds;

	/** Gives the height of the feet of the character, the bottom of the capsule. */
	double GetFeetZ(const AIronPlayerCharacter& Character)
	{
		return Character.GetActorLocation().Z - Character.GetCapsuleComponent()->GetScaledCapsuleHalfHeight();
	}

	/** Places a ledge on the floor in front of the character, with its face at a gap from the capsule and its top at a height above the feet. */
	AStaticMeshActor* SpawnLedge(FPlayerTestWorld& World, const AIronPlayerCharacter& Character, double Gap, double HeightAboveFeet)
	{
		const double FaceX = Character.GetActorLocation().X + Character.GetCapsuleComponent()->GetScaledCapsuleRadius() + Gap;
		const double TopZ = GetFeetZ(Character) + HeightAboveFeet;
		return World.SpawnBlock(FVector(FaceX + LedgeDepth / 2.0, Character.GetActorLocation().Y, TopZ / 2.0), FVector(LedgeDepth, LedgeWidth, TopZ));
	}
}

// Exit test 3 of PR-24: a ledge at each bound of the band starts a mantle, and a ledge 1 cm outside it does not.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionMantleBandTest,
	"IronAbsolution.Player.Mantle.Band",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionMantleBandTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Mantle;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}
	const UIronMovementTuning* Tuning = Character->GetMovementTuning();

	struct FBandCase
	{
		const TCHAR* Name;
		float Height;
		bool bStartsMantle;
	};
	const FBandCase Cases[] = {
		{TEXT("the lowest bound"), Tuning->MantleMinHeight, true},
		{TEXT("the highest bound"), Tuning->MantleMaxHeight, true},
		{TEXT("1 cm below the band"), Tuning->MantleMinHeight - 1.0f, false},
		{TEXT("1 cm above the band"), Tuning->MantleMaxHeight + 1.0f, false},
	};
	for (const FBandCase& Case : Cases)
	{
		AStaticMeshActor* Ledge = SpawnLedge(World, *Character, NearGap, Case.Height);
		World.Tick(FrameSeconds, [] {});

		const TOptional<FIronLedge> Found = Character->GetIronMovement()->FindLedge();
		TestEqual(FString::Printf(TEXT("A ledge at %s, %.1f cm above the feet, starts a mantle"), Case.Name, Case.Height), Found.IsSet(), Case.bStartsMantle);
		if (Found.IsSet())
		{
			TestNearlyEqual(FString::Printf(TEXT("The height of the ledge at %s"), Case.Name), Found->Height, Case.Height, 0.1f);
		}
		Ledge->Destroy();
	}

	return !HasAnyErrors();
}

// Exit test 1 of PR-24, in a test world: from the floor, a jump and a forward move climb a ledge
// inside the upper limit in the time of the tuning, and do not climb a ledge above it.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionMantleClimbTest,
	"IronAbsolution.Player.Mantle.Climb",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionMantleClimbTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Mantle;

	struct FClimbCase
	{
		const TCHAR* Name;
		float Margin;
		bool bClimbs;
	};
	const FClimbCase Cases[] = {
		{TEXT("5 cm below the upper limit"), -JumpLimitMargin, true},
		{TEXT("5 cm above the upper limit"), JumpLimitMargin, false},
	};
	for (const FClimbCase& Case : Cases)
	{
		// Each case starts a new world, because a climb moves the character onto the ledge.
		FPlayerTestWorld World;
		AIronPlayerCharacter* Character = World.Start(*this);
		if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
		{
			return false;
		}
		const UIronMovementTuning* Tuning = Character->GetMovementTuning();
		const UIronCharacterMovementComponent* Movement = Character->GetIronMovement();

		const float Height = Tuning->JumpHeight + Tuning->MantleMaxHeight + Case.Margin;
		const double TopZ = GetFeetZ(*Character) + Height;
		SpawnLedge(World, *Character, JumpGap, Height);
		World.Tick(FrameSeconds, [] {});

		// The player releases the move key at the end of the climb, or walks off the far side of the ledge.
		int32 ClimbFrames = 0;
		bool bClimbEnded = false;
		Character->Jump();
		World.Tick(ClimbLimitSeconds, [Character, Movement, &ClimbFrames, &bClimbEnded]
		{
			if (Movement->IsMantling())
			{
				++ClimbFrames;
			}
			else
			{
				bClimbEnded = ClimbFrames > 0;
			}
			if (!bClimbEnded)
			{
				Character->Move(FVector2D(0.0, 1.0));
			}
		});

		const double FeetZ = GetFeetZ(*Character);
		const bool bOnLedge = Movement->IsMovingOnGround() && FeetZ > TopZ - 1.0;
		AddInfo(FString::Printf(TEXT("A ledge %.1f cm high, %s: %d frames of climb, the feet at %.1f cm, the ledge top at %.1f cm"), Height, Case.Name, ClimbFrames, FeetZ, TopZ));
		TestEqual(FString::Printf(TEXT("The player stands on a ledge %.1f cm high, %s"), Height, Case.Name), bOnLedge, Case.bClimbs);
		if (Case.bClimbs)
		{
			TestNearlyEqual(TEXT("The time of the climb, from the tuning"), ClimbFrames * FrameSeconds, Tuning->MantleTime, ClimbTimeTolerance);
		}
		else
		{
			TestEqual(TEXT("No climb starts at a ledge above the upper limit"), ClimbFrames, 0);
		}
	}

	return !HasAnyErrors();
}

// The mantle starts on a forward move alone (D-143). A jump next to a ledge in the band, with no
// move, does not climb it.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionMantleNeedsForwardMoveTest,
	"IronAbsolution.Player.Mantle.NeedsForwardMove",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionMantleNeedsForwardMoveTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Mantle;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}
	const UIronMovementTuning* Tuning = Character->GetMovementTuning();
	const UIronCharacterMovementComponent* Movement = Character->GetIronMovement();

	// The middle of the band, inside the reach of the wall: each condition of the ledge holds.
	const float Height = (Tuning->MantleMinHeight + Tuning->MantleMaxHeight) / 2.0f;
	SpawnLedge(World, *Character, NearGap, Height);
	World.Tick(FrameSeconds, [] {});
	if (!TestTrue(TEXT("The ledge in the middle of the band has room for a mantle"), Movement->FindLedge().IsSet()))
	{
		return false;
	}

	int32 ClimbFrames = 0;
	Character->Jump();
	World.Tick(ClimbLimitSeconds, [Movement, &ClimbFrames] { ClimbFrames += Movement->IsMantling() ? 1 : 0; });
	TestEqual(TEXT("A jump with no forward move does not climb"), ClimbFrames, 0);
	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
