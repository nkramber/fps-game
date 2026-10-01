// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Camera/CameraComponent.h"
#include "Engine/PostProcessVolume.h"
#include "Engine/RendererSettings.h"
#include "Engine/World.h"
#include "HAL/IConsoleManager.h"
#include "IronPlayerCharacter.h"
#include "Misc/AutomationTest.h"

#if WITH_DEV_AUTOMATION_TESTS

namespace IronAbsolution::Tests::Rendering
{
	const TCHAR* const PlayerCharacterClass = TEXT("/Game/Player/BP_PlayerCharacter.BP_PlayerCharacter_C");
	const TCHAR* const GymMap = TEXT("/Game/Maps/L_Gym.L_Gym");

	/** Gives the value of an integer console variable, or a test error and -1 when the engine has no such variable. */
	int32 ReadConsoleInt(FAutomationTestBase& Test, const TCHAR* Name)
	{
		const IConsoleVariable* Variable = IConsoleManager::Get().FindConsoleVariable(Name);
		if (!Test.TestNotNull(FString::Printf(TEXT("The console variable %s"), Name), Variable))
		{
			return -1;
		}
		return Variable->GetInt();
	}
}

// The game has no motion blur (D-149). The project setting turns off the default, and no camera of
// the player and no volume of the gym turns it on again.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionNoMotionBlurTest,
	"IronAbsolution.Project.NoMotionBlur",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionNoMotionBlurTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests::Rendering;

	TestFalse(TEXT("The project setting of the default motion blur is off"), static_cast<bool>(GetDefault<URendererSettings>()->bDefaultFeatureMotionBlur));
	TestEqual(TEXT("The console variable r.DefaultFeature.MotionBlur"), ReadConsoleInt(*this, TEXT("r.DefaultFeature.MotionBlur")), 0);

	// The camera of the player takes the default unless its settings override the amount.
	const UClass* Character = LoadClass<AIronPlayerCharacter>(nullptr, PlayerCharacterClass);
	if (TestNotNull(FString::Printf(TEXT("The class %s"), PlayerCharacterClass), Character))
	{
		const UCameraComponent* Camera = GetDefault<AIronPlayerCharacter>(Character)->GetFirstPersonCamera();
		TestFalse(TEXT("The camera of the player does not override the motion blur amount"), static_cast<bool>(Camera->PostProcessSettings.bOverride_MotionBlurAmount));
	}

	const UWorld* Gym = LoadObject<UWorld>(nullptr, GymMap);
	if (TestNotNull(FString::Printf(TEXT("The map %s"), GymMap), Gym))
	{
		for (const AActor* Actor : Gym->PersistentLevel->Actors)
		{
			if (const APostProcessVolume* Volume = Cast<APostProcessVolume>(Actor))
			{
				TestFalse(FString::Printf(TEXT("The volume %s does not override the motion blur amount"), *Volume->GetName()), static_cast<bool>(Volume->Settings.bOverride_MotionBlurAmount));
			}
		}
	}

	return !HasAnyErrors();
}

// The outline of D-145 reads the custom depth after the temporal upscale, so the custom depth has
// no jitter. The tooltip of the engine setting gives this rule.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionCustomDepthJitterTest,
	"IronAbsolution.Project.CustomDepthJitter",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionCustomDepthJitterTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests::Rendering;

	TestEqual(TEXT("The console variable r.CustomDepthTemporalAAJitter"), ReadConsoleInt(*this, TEXT("r.CustomDepthTemporalAAJitter")), 0);
	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
