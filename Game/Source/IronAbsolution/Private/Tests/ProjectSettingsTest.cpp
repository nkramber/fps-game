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
	// The test map of phase 1. It is a plain level with no World Partition (D-84).
	const TCHAR* const TestMapPackage = TEXT("/Game/Maps/L_Test");
	const TCHAR* const TestMapObject = TEXT("/Game/Maps/L_Test.L_Test");
}

// The first automation test of the project (D-71). The headless run of `make editor-test` and of
// `scripts/editor-test.ps1` runs each test under the `IronAbsolution` name.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionProjectSettingsTest,
	"IronAbsolution.Project.Settings",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionProjectSettingsTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;

	// The game and the editor both start on the test map. GetGameDefaultMap gives the package name.
	TestEqual(TEXT("The default map of the game"), UGameMapsSettings::GetGameDefaultMap(), FString(TestMapPackage));
	const UGameMapsSettings* MapsSettings = GetDefault<UGameMapsSettings>();
	TestEqual(TEXT("The startup map of the editor"), MapsSettings->EditorStartupMap.ToString(), FString(TestMapObject));
	TestTrue(FString::Printf(TEXT("The package of the test map exists: %s"), TestMapPackage), FPackageName::DoesPackageExist(TestMapPackage));

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
