// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Camera/CameraComponent.h"
#include "Components/StaticMeshComponent.h"
#include "Engine/StaticMeshActor.h"
#include "IronDoor.h"
#include "IronMovementTuning.h"
#include "IronPlayerCharacter.h"
#include "IronSwitch.h"
#include "Misc/AutomationTest.h"
#include "PlayerTestWorld.h"

#if WITH_DEV_AUTOMATION_TESTS

// The interact tests of PR-24 (D-144 to D-146). Each test places a switch on the line of the view
// of the player character, who looks along +X, and a door to the side.
namespace IronAbsolution::Tests::Interact
{
	// The switch is a cube 20 cm on each side, as in the gym.
	constexpr double SwitchSize = 20.0;

	// A switch this far inside or outside the reach, from the eye to the face of the switch.
	constexpr double ReachMargin = 10.0;

	// The door stands to the side of the view, so the line of the view never hits it.
	const FVector DoorLocation(0.0, 500.0, 125.0);
	const FVector DoorSize(200.0, 20.0, 250.0);
	const FVector DoorOpenOffset(0.0, 0.0, -260.0);

	/** Gives the point on the line of the view at a distance from the eye. */
	FVector PointOnView(const AIronPlayerCharacter& Character, double Distance)
	{
		return Character.GetPawnViewLocation() + FVector::ForwardVector * Distance;
	}

	/** Places a test door with a panel and an open offset. */
	AIronDoor* SpawnDoor(FPlayerTestWorld& World)
	{
		AIronDoor* Door = World.GetWorld()->SpawnActor<AIronDoor>(DoorLocation, FRotator::ZeroRotator);
		Door->GetPanel()->SetStaticMesh(World.GetCube());
		Door->GetPanel()->SetRelativeScale3D(DoorSize / CubeSize);
		Door->SetOpenOffset(DoorOpenOffset);
		return Door;
	}

	/** Places a test switch with its face at a distance from the eye, on the line of the view. */
	AIronSwitch* SpawnSwitch(FPlayerTestWorld& World, const AIronPlayerCharacter& Character, double Distance)
	{
		AIronSwitch* Switch = World.GetWorld()->SpawnActor<AIronSwitch>(PointOnView(Character, Distance + SwitchSize / 2.0), FRotator::ZeroRotator);
		// A static component takes no new mesh after play starts, so the button is movable.
		Switch->GetButton()->SetMobility(EComponentMobility::Movable);
		Switch->GetButton()->SetStaticMesh(World.GetCube());
		Switch->SetActorScale3D(FVector(SwitchSize / CubeSize));
		return Switch;
	}

	/** Gives the weight of each blendable on the camera, for example "M_InteractOutline 1.0". */
	FString DescribeBlendables(const AIronPlayerCharacter& Character)
	{
		TArray<FString> Lines;
		for (const FWeightedBlendable& Blendable : Character.GetFirstPersonCamera()->PostProcessSettings.WeightedBlendables.Array)
		{
			Lines.Add(FString::Printf(TEXT("%s %.1f"), *GetNameSafe(Blendable.Object), Blendable.Weight));
		}
		return FString::Join(Lines, TEXT(", "));
	}
}

// Exit test 3 of PR-24: a switch in reach opens and closes the door. A switch out of reach, or
// behind a wall, is not a target.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionInteractReachTest,
	"IronAbsolution.Player.Interact.Reach",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionInteractReachTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Interact;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}
	const float Reach = Character->GetMovementTuning()->InteractReach;

	AIronDoor* Door = SpawnDoor(World);
	AIronSwitch* Switch = SpawnSwitch(World, *Character, Reach - ReachMargin);
	Switch->SetDoor(Door);
	World.Tick(FrameSeconds, [] {});

	// In reach: each use toggles the door.
	TestEqual(TEXT("The switch 10 cm inside the reach is the target"), Character->FindInteractTarget(), static_cast<AActor*>(Switch));
	TestTrue(TEXT("The first use of the switch in reach"), Character->Interact());
	TestTrue(TEXT("The first use opens the door"), Door->IsOpen());
	TestEqual(TEXT("The open panel moves by the open offset"), Door->GetPanel()->GetRelativeLocation(), DoorOpenOffset);
	TestTrue(TEXT("The second use of the switch in reach"), Character->Interact());
	TestFalse(TEXT("The second use closes the door"), Door->IsOpen());
	TestEqual(TEXT("The closed panel is back at the door"), Door->GetPanel()->GetRelativeLocation(), FVector::ZeroVector);

	// Out of reach: no target, and no use.
	Switch->SetActorLocation(PointOnView(*Character, Reach + ReachMargin + SwitchSize / 2.0));
	World.Tick(FrameSeconds, [] {});
	TestNull(TEXT("The switch 10 cm outside the reach is not a target"), Character->FindInteractTarget());
	TestFalse(TEXT("A use with no target in reach"), Character->Interact());
	TestFalse(TEXT("The door stays closed"), Door->IsOpen());

	// In reach, but behind a wall: the wall blocks the use.
	Switch->SetActorLocation(PointOnView(*Character, Reach - ReachMargin + SwitchSize / 2.0));
	World.SpawnBlock(PointOnView(*Character, Reach / 2.0), FVector(10.0, 300.0, 300.0));
	World.Tick(FrameSeconds, [] {});
	TestNull(TEXT("A switch behind a wall is not a target"), Character->FindInteractTarget());

	return !HasAnyErrors();
}

// Exit test 2 of PR-24, in a test world: the player uses the door itself, as well as the switch.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionInteractDoorTest,
	"IronAbsolution.Player.Interact.Door",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionInteractDoorTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Interact;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}

	// The door across the view, 100 cm in front of the eye.
	AIronDoor* Door = SpawnDoor(World);
	Door->SetActorLocation(FVector(Character->GetPawnViewLocation().X + 100.0 + DoorSize.Y / 2.0, 0.0, DoorLocation.Z));
	Door->SetActorRotation(FRotator(0.0, 90.0, 0.0));
	World.Tick(FrameSeconds, [] {});

	TestEqual(TEXT("The door in reach is the target"), Character->FindInteractTarget(), static_cast<AActor*>(Door));
	TestTrue(TEXT("A use of the door in reach"), Character->Interact());
	TestTrue(TEXT("A use of the closed door opens it"), Door->IsOpen());
	return !HasAnyErrors();
}

// The cue of D-145 and D-148: the target in reach has the custom depth on and the glow overlay,
// and the camera uses the outline material. With no target, each is off, so the cue costs no pass.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionInteractCueTest,
	"IronAbsolution.Player.Interact.Cue",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionInteractCueTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Interact;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}
	const float Reach = Character->GetMovementTuning()->InteractReach;
	TestEqual(TEXT("With no target, the outline has no weight"), DescribeBlendables(*Character), FString(TEXT("M_InteractOutline 0.0")));

	AIronSwitch* Switch = SpawnSwitch(World, *Character, Reach - ReachMargin);
	World.Tick(FrameSeconds, [] {});
	TestEqual(TEXT("The switch in reach has the outline"), Character->GetOutlinedTarget(), static_cast<AActor*>(Switch));
	TestTrue(TEXT("The button of the switch in reach draws to the custom depth"), Switch->GetButton()->bRenderCustomDepth);
	TestEqual(TEXT("The button of the switch in reach has the glow overlay"), GetNameSafe(Switch->GetButton()->GetOverlayMaterial()), FString(TEXT("M_InteractGlow")));
	TestEqual(TEXT("With a target, the outline has full weight"), DescribeBlendables(*Character), FString(TEXT("M_InteractOutline 1.0")));

	Switch->SetActorLocation(PointOnView(*Character, Reach + ReachMargin + SwitchSize / 2.0));
	World.Tick(FrameSeconds, [] {});
	TestNull(TEXT("The switch out of reach has no outline"), Character->GetOutlinedTarget());
	TestFalse(TEXT("The button of the switch out of reach does not draw to the custom depth"), Switch->GetButton()->bRenderCustomDepth);
	TestNull(TEXT("The button of the switch out of reach has no overlay"), Switch->GetButton()->GetOverlayMaterial());
	TestEqual(TEXT("With no target again, the outline has no weight"), DescribeBlendables(*Character), FString(TEXT("M_InteractOutline 0.0")));

	return !HasAnyErrors();
}

// A switch with no door and a door with no open offset each write an error line and change nothing (T-2).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionInteractErrorsTest,
	"IronAbsolution.Player.Interact.Errors",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionInteractErrorsTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Interact;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}

	AIronSwitch* Switch = SpawnSwitch(World, *Character, Character->GetMovementTuning()->InteractReach - ReachMargin);
	World.Tick(FrameSeconds, [] {});
	AddExpectedMessagePlain(TEXT("did nothing for"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	TestTrue(TEXT("The switch with no door takes the use"), Character->Interact());

	AIronDoor* Door = SpawnDoor(World);
	Door->SetOpenOffset(FVector::ZeroVector);
	Switch->SetDoor(Door);
	AddExpectedMessagePlain(TEXT("OpenOffset is zero"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	TestTrue(TEXT("The switch with a door takes the use"), Character->Interact());
	TestFalse(TEXT("A door with no open offset stays closed"), Door->IsOpen());

	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
