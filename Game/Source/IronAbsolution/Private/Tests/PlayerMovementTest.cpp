// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Camera/CameraComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "GameFramework/WorldSettings.h"
#include "IronMovementTuning.h"
#include "IronPlayerCharacter.h"
#include "Misc/AutomationTest.h"
#include "PlayerTestWorld.h"
#include "UObject/Package.h"

#include <limits>

#if WITH_DEV_AUTOMATION_TESTS

// The movement tests of PR-21. Each test starts a game world with a floor and one player
// character, gives the character input values, ticks the world at a fixed rate, and reads the
// position of the character.
namespace IronAbsolution::Tests::Movement
{
	// The time to reach the run speed, the time of the measure, and the longest jump.
	constexpr float RunUpSeconds = 1.0f;
	constexpr float MeasureSeconds = 0.5f;
	constexpr float JumpLimitSeconds = 3.0f;

	// The pass rule: the measured value is within this part of the value of the tuning. The fixed
	// frame rate and the integration of the engine give a small error. A wrong value of the tuning
	// gives an error much larger than this.
	constexpr float Tolerance = 0.02f;

	/** Runs forward until the speed is steady, and gives the speed on the ground over the next half second, in cm/s. */
	float MeasureRunSpeed(FPlayerTestWorld& World, AIronPlayerCharacter& Character)
	{
		const auto RunForward = [&Character] { Character.Move(FVector2D(0.0, 1.0)); };
		World.Tick(RunUpSeconds, RunForward);
		const FVector Start = Character.GetActorLocation();
		World.Tick(MeasureSeconds, RunForward);
		return static_cast<float>(FVector::Dist2D(Start, Character.GetActorLocation()) / MeasureSeconds);
	}

	/** Jumps from the floor, and gives the height of the top of the jump above the start, in cm. */
	float MeasureJumpHeight(FPlayerTestWorld& World, AIronPlayerCharacter& Character)
	{
		const double StartZ = Character.GetActorLocation().Z;
		double TopZ = StartZ;
		Character.Jump();
		World.Tick(JumpLimitSeconds, [&Character, &TopZ] { TopZ = FMath::Max(TopZ, Character.GetActorLocation().Z); });
		return static_cast<float>(TopZ - StartZ);
	}

	/** Describes each value that ApplyMovementTuning writes, so a test can compare the state before and after. */
	FString DescribeTunedState(const AIronPlayerCharacter& Character)
	{
		const UCharacterMovementComponent* Movement = Character.GetCharacterMovement();
		const UCameraComponent* Camera = Character.GetFirstPersonCamera();
		return FString::Printf(
			TEXT("speed %f, acceleration %f, braking %f, friction %f, gravity scale %f, air control %f, step %f, slope %f, jump %f, eye %f, camera Z %f"),
			Movement->MaxWalkSpeed, Movement->MaxAcceleration, Movement->BrakingDecelerationWalking, Movement->GroundFriction,
			Movement->GravityScale, Movement->AirControl, Movement->MaxStepHeight, Movement->GetWalkableFloorAngle(),
			Movement->JumpZVelocity, Character.BaseEyeHeight, Camera->GetRelativeLocation().Z);
	}

	/** Compares a measured value with the value of the tuning, with the tolerance of the pass rule. */
	void TestMeasured(FAutomationTestBase& Test, const TCHAR* What, float Measured, float Expected)
	{
		// The report keeps each measured value, so a pass also gives the evidence of the value.
		Test.AddInfo(FString::Printf(TEXT("%s: measured %.1f, the tuning gives %.1f"), What, Measured, Expected));
		Test.TestNearlyEqual(FString::Printf(TEXT("%s: measured %.1f, the tuning gives %.1f"), What, Measured, Expected), Measured, Expected, Expected * Tolerance);
	}
}

// Exit test 2 of PR-21: the run speed comes from the data asset of the player.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionRunSpeedTest,
	"IronAbsolution.Player.Movement.RunSpeed",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionRunSpeedTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Movement;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}

	TestMeasured(*this, TEXT("The run speed"), MeasureRunSpeed(World, *Character), Character->GetMovementTuning()->RunSpeed);
	return !HasAnyErrors();
}

// Exit test 2 of PR-21: the jump height comes from the data asset of the player.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionJumpHeightTest,
	"IronAbsolution.Player.Movement.JumpHeight",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionJumpHeightTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Movement;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}

	TestMeasured(*this, TEXT("The jump height"), MeasureJumpHeight(World, *Character), Character->GetMovementTuning()->JumpHeight);
	return !HasAnyErrors();
}

// Exit test 3 of PR-21: a new value in a tuning changes the movement, with no change of C++ (D-29).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionTuningChangeTest,
	"IronAbsolution.Player.Movement.TuningChange",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionTuningChangeTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Movement;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}

	// A copy of the asset of the player, with a run speed and a jump height half again as large.
	UIronMovementTuning* Changed = DuplicateObject<UIronMovementTuning>(Character->GetMovementTuning(), GetTransientPackage());
	Changed->RunSpeed *= 1.5f;
	Changed->JumpHeight *= 1.5f;
	if (!TestTrue(TEXT("The character takes the changed tuning"), Character->ApplyMovementTuning(*Changed)))
	{
		return false;
	}

	TestMeasured(*this, TEXT("The run speed of the changed tuning"), MeasureRunSpeed(World, *Character), Changed->RunSpeed);
	// The run ends on the floor, so the jump starts from the ground.
	TestMeasured(*this, TEXT("The jump height of the changed tuning"), MeasureJumpHeight(World, *Character), Changed->JumpHeight);
	return !HasAnyErrors();
}

// A tuning with a value outside its range changes nothing, and the log names each value (T-2).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionInvalidTuningTest,
	"IronAbsolution.Player.Movement.InvalidTuning",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionInvalidTuningTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Movement;

	// A new tuning has each value at 0, the state of an asset that sets no value.
	const UIronMovementTuning* Empty = NewObject<UIronMovementTuning>(GetTransientPackage());
	const TArray<FString> Errors = Empty->FindInvalidValues();
	// 14 values with a range, and the band of the mantle, which is empty.
	TestEqual(TEXT("A new tuning has one error for each of the 14 values, and one for the band of the mantle"), Errors.Num(), 15);
	if (Errors.Num() > 0)
	{
		TestTrue(FString::Printf(TEXT("The first error names the value and the range: %s"), *Errors[0]), Errors[0].StartsWith(TEXT("RunSpeed of ")) && Errors[0].EndsWith(TEXT(" is 0.0. The range is 1.0 to no maximum.")));
	}

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}
	TestTrue(TEXT("The asset of the player has no invalid value"), Character->GetMovementTuning()->FindInvalidValues().IsEmpty());

	const float RunSpeedBefore = Character->GetCharacterMovement()->MaxWalkSpeed;
	AddExpectedMessagePlain(TEXT("did not take the tuning"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 15);
	TestFalse(TEXT("The character refuses the empty tuning"), Character->ApplyMovementTuning(*Empty));
	TestEqual(TEXT("The run speed does not change"), Character->GetCharacterMovement()->MaxWalkSpeed, RunSpeedBefore);
	return !HasAnyErrors();
}

// A refused tuning changes no value of the character: a NaN value, and a world with no gravity (T-2).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionRefusedTuningTest,
	"IronAbsolution.Player.Movement.RefusedTuning",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionRefusedTuningTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Movement;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}
	const FString Before = DescribeTunedState(*Character);

	// A NaN fails each comparison of a range, so the check of the range alone lets it pass.
	UIronMovementTuning* WithNaN = DuplicateObject<UIronMovementTuning>(Character->GetMovementTuning(), GetTransientPackage());
	WithNaN->RunSpeed = std::numeric_limits<float>::quiet_NaN();
	const TArray<FString> Errors = WithNaN->FindInvalidValues();
	TestEqual(TEXT("A tuning with one NaN has one error"), Errors.Num(), 1);
	if (Errors.Num() == 1)
	{
		TestTrue(FString::Printf(TEXT("The error names the value: %s"), *Errors[0]), Errors[0].StartsWith(TEXT("RunSpeed of ")));
	}
	AddExpectedMessagePlain(TEXT("did not take the tuning: RunSpeed of"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	TestFalse(TEXT("The character refuses the tuning with a NaN"), Character->ApplyMovementTuning(*WithNaN));
	TestEqual(TEXT("The tuning with a NaN changes nothing"), DescribeTunedState(*Character), Before);

	// A valid tuning in a world with no gravity: the character refuses it before any write.
	UIronMovementTuning* Changed = DuplicateObject<UIronMovementTuning>(Character->GetMovementTuning(), GetTransientPackage());
	Changed->RunSpeed *= 1.5f;
	AWorldSettings* Settings = Character->GetWorld()->GetWorldSettings();
	Settings->WorldGravityZ = 0.0f;
	Settings->bWorldGravitySet = true;
	AddExpectedMessagePlain(TEXT("so no jump comes down"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	TestFalse(TEXT("The character refuses a tuning in a world with no gravity"), Character->ApplyMovementTuning(*Changed));
	TestEqual(TEXT("The tuning in a world with no gravity changes nothing"), DescribeTunedState(*Character), Before);

	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
