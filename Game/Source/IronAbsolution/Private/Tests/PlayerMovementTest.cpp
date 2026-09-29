// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Camera/CameraComponent.h"
#include "Components/StaticMeshComponent.h"
#include "Engine/StaticMesh.h"
#include "Engine/StaticMeshActor.h"
#include "Engine/World.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "GameFramework/WorldSettings.h"
#include "IronMovementTuning.h"
#include "IronPlayerCharacter.h"
#include "Misc/AutomationTest.h"
#include "Tests/AutomationCommon.h"
#include "UObject/Package.h"

#include <limits>

#if WITH_DEV_AUTOMATION_TESTS

// The movement tests of PR-21. Each test starts a game world with a floor and one player
// character, gives the character input values, ticks the world at a fixed rate, and reads the
// position of the character. The test world has no local player, so each test calls the move and
// jump functions of the character, the functions that the input actions call.
namespace IronAbsolution::Tests::Movement
{
	const TCHAR* const PlayerCharacterClass = TEXT("/Game/Player/BP_PlayerCharacter.BP_PlayerCharacter_C");
	const TCHAR* const CubeMesh = TEXT("/Engine/BasicShapes/Cube.Cube");

	// A fixed rate of 120 frames each second, the frame rate of the budget (D-32).
	constexpr float FrameSeconds = 1.0f / 120.0f;

	// The time from the spawn to a stand on the floor, and the time to reach the run speed.
	constexpr float SettleSeconds = 0.5f;
	constexpr float RunUpSeconds = 1.0f;
	constexpr float MeasureSeconds = 0.5f;
	constexpr float JumpLimitSeconds = 3.0f;

	// The pass rule: the measured value is within this part of the value of the tuning. The fixed
	// frame rate and the integration of the engine give a small error. A wrong value of the tuning
	// gives an error much larger than this.
	constexpr float Tolerance = 0.02f;

	// A game world with a floor of 100 m by 100 m, with its top at Z 0, and one player character on it.
	class FMovementWorld
	{
	public:
		~FMovementWorld()
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

			UWorld* World = Wrapper.GetTestWorld();
			UStaticMesh* Cube = LoadObject<UStaticMesh>(nullptr, CubeMesh);
			UClass* CharacterClass = LoadClass<AIronPlayerCharacter>(nullptr, PlayerCharacterClass);
			if (!Test.TestNotNull(FString::Printf(TEXT("The mesh %s"), CubeMesh), Cube) || !Test.TestNotNull(FString::Printf(TEXT("The class %s"), PlayerCharacterClass), CharacterClass))
			{
				return nullptr;
			}

			AStaticMeshActor* Floor = World->SpawnActor<AStaticMeshActor>(FVector(0.0, 0.0, -50.0), FRotator::ZeroRotator);
			// A static component takes no new mesh after play starts, so the floor is movable.
			Floor->GetStaticMeshComponent()->SetMobility(EComponentMobility::Movable);
			Floor->GetStaticMeshComponent()->SetStaticMesh(Cube);
			Floor->SetActorScale3D(FVector(100.0, 100.0, 1.0));

			AIronPlayerCharacter* Character = World->SpawnActor<AIronPlayerCharacter>(CharacterClass, FVector(0.0, 0.0, 100.0), FRotator::ZeroRotator);
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

	private:
		FTestWorldWrapper Wrapper;
	};

	/** Runs forward until the speed is steady, and gives the speed on the ground over the next half second, in cm/s. */
	float MeasureRunSpeed(FMovementWorld& World, AIronPlayerCharacter& Character)
	{
		const auto RunForward = [&Character] { Character.Move(FVector2D(0.0, 1.0)); };
		World.Tick(RunUpSeconds, RunForward);
		const FVector Start = Character.GetActorLocation();
		World.Tick(MeasureSeconds, RunForward);
		return static_cast<float>(FVector::Dist2D(Start, Character.GetActorLocation()) / MeasureSeconds);
	}

	/** Jumps from the floor, and gives the height of the top of the jump above the start, in cm. */
	float MeasureJumpHeight(FMovementWorld& World, AIronPlayerCharacter& Character)
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
			TEXT("speed %f, acceleration %f, braking %f, friction %f, gravity scale %f, air control %f, step %f, slope %f, jump %f, eye %f, camera Z %f, field of view %f"),
			Movement->MaxWalkSpeed, Movement->MaxAcceleration, Movement->BrakingDecelerationWalking, Movement->GroundFriction,
			Movement->GravityScale, Movement->AirControl, Movement->MaxStepHeight, Movement->GetWalkableFloorAngle(),
			Movement->JumpZVelocity, Character.BaseEyeHeight, Camera->GetRelativeLocation().Z, Camera->FieldOfView);
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
	using namespace IronAbsolution::Tests::Movement;

	FMovementWorld World;
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
	using namespace IronAbsolution::Tests::Movement;

	FMovementWorld World;
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
	using namespace IronAbsolution::Tests::Movement;

	FMovementWorld World;
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
	using namespace IronAbsolution::Tests::Movement;

	// A new tuning has each value at 0, the state of an asset that sets no value.
	const UIronMovementTuning* Empty = NewObject<UIronMovementTuning>(GetTransientPackage());
	const TArray<FString> Errors = Empty->FindInvalidValues();
	TestEqual(TEXT("A new tuning has one error for each of the 11 values"), Errors.Num(), 11);
	if (Errors.Num() > 0)
	{
		TestTrue(FString::Printf(TEXT("The first error names the value and the range: %s"), *Errors[0]), Errors[0].StartsWith(TEXT("RunSpeed of ")) && Errors[0].EndsWith(TEXT(" is 0.0. The range is 1.0 to no maximum.")));
	}

	FMovementWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr || !TestNotNull(TEXT("The movement tuning of the player character"), Character->GetMovementTuning()))
	{
		return false;
	}
	TestTrue(TEXT("The asset of the player has no invalid value"), Character->GetMovementTuning()->FindInvalidValues().IsEmpty());

	const float RunSpeedBefore = Character->GetCharacterMovement()->MaxWalkSpeed;
	AddExpectedMessagePlain(TEXT("did not take the tuning"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 11);
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
	using namespace IronAbsolution::Tests::Movement;

	FMovementWorld World;
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
	Changed->FieldOfView = 110.0f;
	AWorldSettings* Settings = Character->GetWorld()->GetWorldSettings();
	Settings->WorldGravityZ = 0.0f;
	Settings->bWorldGravitySet = true;
	AddExpectedMessagePlain(TEXT("so no jump comes down"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	TestFalse(TEXT("The character refuses a tuning in a world with no gravity"), Character->ApplyMovementTuning(*Changed));
	TestEqual(TEXT("The tuning in a world with no gravity changes nothing"), DescribeTunedState(*Character), Before);

	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
