// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "EnhancedInputComponent.h"
#include "EnhancedPlayerInput.h"
#include "GameFramework/InputSettings.h"
#include "GameMapsSettings.h"
#include "Misc/AutomationTest.h"
#include "Misc/PackageName.h"

#if WITH_DEV_AUTOMATION_TESTS

namespace IronAbsolution::Tests
{
	// The test map of phase 1. It is a plain level with no World Partition (D-84). The automation
	// tests and the timed run of the package use it (D-89).
	const TCHAR* const TestMapPackage = TEXT("/Game/Maps/L_Test");

	// The gym of phase 3, the default map of the game and of the editor (PR-21, D-133).
	const TCHAR* const GymMapPackage = TEXT("/Game/Maps/L_Gym");
#if WITH_EDITORONLY_DATA
	const TCHAR* const GymMapObject = TEXT("/Game/Maps/L_Gym.L_Gym");
#endif
}

// The first automation test of the project (D-71). The headless run of `run.ps1 editor-test` runs
// each test under the `IronAbsolution` name.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionProjectSettingsTest,
	"IronAbsolution.Project.Settings",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionProjectSettingsTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;

	// The game and the editor both start in the gym. GetGameDefaultMap gives the package name.
	TestEqual(TEXT("The default map of the game"), UGameMapsSettings::GetGameDefaultMap(), FString(GymMapPackage));
	// The game target of a packaged Development build compiles this test too, and that target has
	// no editor-only data. So the check of the editor map has its own guard (F-25).
#if WITH_EDITORONLY_DATA
	const UGameMapsSettings* MapsSettings = GetDefault<UGameMapsSettings>();
	TestEqual(TEXT("The startup map of the editor"), MapsSettings->EditorStartupMap.ToString(), FString(GymMapObject));
#endif
	TestTrue(FString::Printf(TEXT("The package of the test map exists: %s"), TestMapPackage), FPackageName::DoesPackageExist(TestMapPackage));
	TestTrue(FString::Printf(TEXT("The package of the gym exists: %s"), GymMapPackage), FPackageName::DoesPackageExist(GymMapPackage));

	// Enhanced Input is the input system of the project (F-6).
	const UClass* PlayerInputClass = UInputSettings::GetDefaultPlayerInputClass();
	const UClass* InputComponentClass = UInputSettings::GetDefaultInputComponentClass();
	if (!TestNotNull(TEXT("The default player input class"), PlayerInputClass) || !TestNotNull(TEXT("The default input component class"), InputComponentClass))
	{
		return false;
	}
	TestTrue(FString::Printf(TEXT("The default player input class is Enhanced Input, found %s"), *PlayerInputClass->GetPathName()), PlayerInputClass->IsChildOf(UEnhancedPlayerInput::StaticClass()));
	TestTrue(FString::Printf(TEXT("The default input component class is Enhanced Input, found %s"), *InputComponentClass->GetPathName()), InputComponentClass->IsChildOf(UEnhancedInputComponent::StaticClass()));

	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
