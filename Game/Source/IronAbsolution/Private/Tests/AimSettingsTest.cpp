// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Camera/CameraComponent.h"
#include "Engine/Engine.h"
#include "Engine/World.h"
#include "GameFramework/InputSettings.h"
#include "GameFramework/PlayerController.h"
#include "HAL/IConsoleManager.h"
#include "InputCoreTypes.h"
#include "IronGameUserSettings.h"
#include "IronPlayerCharacter.h"
#include "Misc/AutomationTest.h"
#include "Misc/ConfigCacheIni.h"
#include "Misc/Paths.h"
#include "Tests/AutomationCommon.h"
#include "UObject/Package.h"

#include <limits>

#if WITH_DEV_AUTOMATION_TESTS

// The tests of the aim settings of PR-23 (D-139, D-141). The settings object of the engine is the
// one of the editor, so each test puts back the values of the settings file at its end.
namespace IronAbsolution::Tests::Aim
{
	const TCHAR* const PlayerCharacterClass = TEXT("/Game/Player/BP_PlayerCharacter.BP_PlayerCharacter_C");

	// The scale of D-139: each mouse count turns the view by 0.022 degrees times the sensitivity.
	constexpr float DegreesPerCountAtSensitivityOne = 0.022f;

	/** Keeps the aim settings at the start of a test, and puts them back in the settings file at the end. */
	class FKeptAimSettings
	{
	public:
		FKeptAimSettings()
			: MouseSensitivity(UIronGameUserSettings::Get().GetMouseSensitivity())
			, FieldOfView(UIronGameUserSettings::Get().GetFieldOfView())
		{
		}

		~FKeptAimSettings()
		{
			UIronGameUserSettings& Settings = UIronGameUserSettings::Get();
			Settings.SetMouseSensitivity(MouseSensitivity);
			Settings.SetFieldOfView(FieldOfView);
			Settings.SaveSettings();
		}

	private:
		float MouseSensitivity;
		float FieldOfView;
	};

	/** A game world with one player character and a local player controller that possesses it. */
	class FAimWorld
	{
	public:
		~FAimWorld()
		{
			if (Wrapper.GetTestWorld() != nullptr)
			{
				Wrapper.DestroyTestWorld(false);
			}
		}

		/** Starts play, and spawns the character and its controller. Gives null after a test error for each problem. */
		AIronPlayerCharacter* Start(FAutomationTestBase& Test)
		{
			if (!Wrapper.CreateTestWorld(EWorldType::Game) || !Wrapper.BeginPlayInTestWorld())
			{
				Wrapper.ForwardErrorMessages(&Test);
				return nullptr;
			}

			UWorld* World = Wrapper.GetTestWorld();
			UClass* CharacterClass = LoadClass<AIronPlayerCharacter>(nullptr, PlayerCharacterClass);
			if (!Test.TestNotNull(FString::Printf(TEXT("The class %s"), PlayerCharacterClass), CharacterClass))
			{
				return nullptr;
			}

			AIronPlayerCharacter* Character = World->SpawnActor<AIronPlayerCharacter>(CharacterClass, FVector::ZeroVector, FRotator::ZeroRotator);
			APlayerController* Controller = World->SpawnActor<APlayerController>();
			if (!Test.TestNotNull(TEXT("The spawned player character"), Character) || !Test.TestNotNull(TEXT("The spawned player controller"), Controller))
			{
				return nullptr;
			}

			// The test world has no local player. A pawn sends view input only to a local controller,
			// so the test marks the controller as local, as the game mode does for a player.
			Controller->SetAsLocalPlayerController();
			Controller->Possess(Character);
			return Character;
		}

	private:
		FTestWorldWrapper Wrapper;
	};

	/** Runs one console command, as the owner types it in the package. */
	bool RunConsoleCommand(FAutomationTestBase& Test, const TCHAR* Command)
	{
		return Test.TestTrue(FString::Printf(TEXT("The console runs \"%s\""), Command), IConsoleManager::Get().ProcessUserConsoleInput(Command, *GLog, nullptr));
	}
}

// Exit test 1 of PR-23: the mouse sensitivity changes the aim, on the scale of D-139.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionMouseSensitivityTest,
	"IronAbsolution.Player.Aim.MouseSensitivity",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionMouseSensitivityTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests::Aim;

	FKeptAimSettings Kept;
	FAimWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr)
	{
		return false;
	}
	APlayerController* Controller = Character->GetController<APlayerController>();

	// Two sensitivities, so the test shows that the setting changes the turn. The mouse moves 100
	// counts to the right and 50 counts forward. A forward move turns the view up.
	UIronGameUserSettings& Settings = UIronGameUserSettings::Get();
	for (const float Sensitivity : {2.5f, 7.0f})
	{
		if (!TestTrue(FString::Printf(TEXT("The settings take the sensitivity %.1f"), Sensitivity), Settings.SetMouseSensitivity(Sensitivity)))
		{
			return false;
		}
		Controller->RotationInput = FRotator::ZeroRotator;
		Character->Look(FVector2D(100.0, 50.0));

		const float DegreesPerCount = DegreesPerCountAtSensitivityOne * Sensitivity;
		TestNearlyEqual(FString::Printf(TEXT("The yaw of 100 counts at the sensitivity %.1f"), Sensitivity), Controller->RotationInput.Yaw, 100.0 * DegreesPerCount, 1.0e-4);
		TestNearlyEqual(FString::Printf(TEXT("The pitch of 50 counts at the sensitivity %.1f"), Sensitivity), Controller->RotationInput.Pitch, 50.0 * DegreesPerCount, 1.0e-4);
	}

	return !HasAnyErrors();
}

// Exit test 1 of PR-23: the field of view changes the view, at the start and after a change (D-141).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionFieldOfViewTest,
	"IronAbsolution.Player.Aim.FieldOfView",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionFieldOfViewTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests::Aim;

	FKeptAimSettings Kept;
	FAimWorld World;
	AIronPlayerCharacter* Character = World.Start(*this);
	if (Character == nullptr)
	{
		return false;
	}

	UIronGameUserSettings& Settings = UIronGameUserSettings::Get();
	const UCameraComponent* Camera = Character->GetFirstPersonCamera();
	TestEqual(TEXT("The camera takes the field of view of the settings at the start"), Camera->FieldOfView, Settings.GetFieldOfView());

	TestTrue(TEXT("The settings take the field of view 110"), Settings.SetFieldOfView(110.0f));
	TestEqual(TEXT("The camera takes a new field of view"), Camera->FieldOfView, 110.0f);

	if (RunConsoleCommand(*this, TEXT("Iron.FieldOfView 90")))
	{
		TestEqual(TEXT("The camera takes the field of view of the console command"), Camera->FieldOfView, 90.0f);
	}

	return !HasAnyErrors();
}

// Exit test 3 of PR-23: a value out of bounds gives the error, and the game keeps the last good value (T-2).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionAimBoundsTest,
	"IronAbsolution.Player.Aim.Bounds",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionAimBoundsTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests::Aim;

	FKeptAimSettings Kept;
	UIronGameUserSettings& Settings = UIronGameUserSettings::Get();
	const float NaN = std::numeric_limits<float>::quiet_NaN();

	// Three values through the setter and one through the console, for each setting.
	AddExpectedMessagePlain(TEXT("is outside the bounds 0.1 to 20.0. The game keeps 3.0."), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 4);
	AddExpectedMessagePlain(TEXT("is outside the bounds 80.0 to 120.0. The game keeps 95.0."), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 4);
	AddExpectedMessagePlain(TEXT("The command of MouseSensitivity takes one number, and got \"fast\". The game keeps 3.0."), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);

	// The last good values of this test.
	if (!TestTrue(TEXT("The settings take the sensitivity 3"), Settings.SetMouseSensitivity(3.0f)) || !TestTrue(TEXT("The settings take the field of view 95"), Settings.SetFieldOfView(95.0f)))
	{
		return false;
	}

	for (const float Value : {25.0f, 0.05f, NaN})
	{
		TestFalse(FString::Printf(TEXT("The settings refuse the sensitivity %s"), *FString::SanitizeFloat(Value)), Settings.SetMouseSensitivity(Value));
	}
	for (const float Value : {79.0f, 121.0f, NaN})
	{
		TestFalse(FString::Printf(TEXT("The settings refuse the field of view %s"), *FString::SanitizeFloat(Value)), Settings.SetFieldOfView(Value));
	}
	RunConsoleCommand(*this, TEXT("Iron.MouseSensitivity 25"));
	RunConsoleCommand(*this, TEXT("Iron.MouseSensitivity fast"));
	RunConsoleCommand(*this, TEXT("Iron.FieldOfView 130"));
	TestEqual(TEXT("The sensitivity keeps the last good value"), Settings.GetMouseSensitivity(), 3.0f);
	TestEqual(TEXT("The field of view keeps the last good value"), Settings.GetFieldOfView(), 95.0f);

	// A value inside the bounds through the console.
	RunConsoleCommand(*this, TEXT("Iron.MouseSensitivity 3.5"));
	TestEqual(TEXT("The console command sets the sensitivity"), Settings.GetMouseSensitivity(), 3.5f);

	return !HasAnyErrors();
}

// Exit test 2 of PR-23: each value comes back after a restart, from the settings file of the user.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionAimRestartTest,
	"IronAbsolution.Player.Aim.Restart",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionAimRestartTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests::Aim;

	FKeptAimSettings Kept;
	UIronGameUserSettings& Settings = UIronGameUserSettings::Get();
	if (!TestTrue(TEXT("The settings take the sensitivity 7"), Settings.SetMouseSensitivity(7.0f)) || !TestTrue(TEXT("The settings take the field of view 110"), Settings.SetFieldOfView(110.0f)))
	{
		return false;
	}
	Settings.SaveSettings();

	// A new object that loads the file again from the disk, as the game does at its start.
	UIronGameUserSettings* Restarted = NewObject<UIronGameUserSettings>(GetTransientPackage());
	Restarted->LoadSettings(true);
	TestEqual(FString::Printf(TEXT("The sensitivity comes back from %s"), *GGameUserSettingsIni), Restarted->GetMouseSensitivity(), 7.0f);
	TestEqual(FString::Printf(TEXT("The field of view comes back from %s"), *GGameUserSettingsIni), Restarted->GetFieldOfView(), 110.0f);

	return !HasAnyErrors();
}

// A settings file with a value out of bounds gives the error at load, and the game uses the nearest bound (T-2).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionAimLoadBoundsTest,
	"IronAbsolution.Player.Aim.LoadBounds",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionAimLoadBoundsTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests::Aim;

	// The kept values go back to the file at the end, over the values of this test.
	FKeptAimSettings Kept;
	const FString Section = UIronGameUserSettings::StaticClass()->GetPathName();
	GConfig->SetFloat(*Section, TEXT("MouseSensitivity"), 50.0f, GGameUserSettingsIni);
	GConfig->SetFloat(*Section, TEXT("FieldOfView"), 10.0f, GGameUserSettingsIni);

	AddExpectedMessagePlain(TEXT("MouseSensitivity 50.0 in the settings"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	AddExpectedMessagePlain(TEXT("FieldOfView 10.0 in the settings"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	UIronGameUserSettings* Loaded = NewObject<UIronGameUserSettings>(GetTransientPackage());
	Loaded->LoadSettings(false);
	TestEqual(TEXT("A sensitivity above the bounds loads as the upper bound"), Loaded->GetMouseSensitivity(), 20.0f);
	TestEqual(TEXT("A field of view below the bounds loads as the lower bound"), Loaded->GetFieldOfView(), 80.0f);

	return !HasAnyErrors();
}

// The project config that the aim settings need: the settings class, the input scales, and the defaults.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionAimProjectConfigTest,
	"IronAbsolution.Player.Aim.ProjectConfig",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionAimProjectConfigTest::RunTest(const FString& Parameters)
{
	const UGameUserSettings* Settings = GEngine->GetGameUserSettings();
	TestTrue(FString::Printf(TEXT("The engine makes the settings object of the project, found %s"), *GetPathNameSafe(Settings)), Settings != nullptr && Settings->IsA<UIronGameUserSettings>());

	// The engine scales the mouse by 0.07 and the view input by 2.5 by default. The scale of D-139
	// needs one mouse count to reach the character as 1.
	const UInputSettings* InputSettings = GetDefault<UInputSettings>();
	TestFalse(TEXT("The input scales of the engine are off"), InputSettings->bEnableLegacyInputScales);
	const FInputAxisConfigEntry* MouseAxis = InputSettings->AxisConfig.FindByPredicate([](const FInputAxisConfigEntry& Entry) { return Entry.AxisKeyName == EKeys::Mouse2D.GetFName(); });
	if (TestNotNull(TEXT("The axis properties of Mouse2D"), MouseAxis))
	{
		TestEqual(TEXT("The sensitivity of the axis Mouse2D"), MouseAxis->AxisProperties.Sensitivity, 1.0f);
	}

	// Each default of the project is present and inside its bounds (D-139, D-141).
	const FString DefaultsPath = FPaths::ProjectConfigDir() / TEXT("DefaultGameUserSettings.ini");
	FConfigFile Defaults;
	Defaults.Read(DefaultsPath);
	const FString Section = UIronGameUserSettings::StaticClass()->GetPathName();
	for (const IronAbsolution::Aim::FAimSettingBounds& Bounds : {IronAbsolution::Aim::MouseSensitivityBounds, IronAbsolution::Aim::FieldOfViewBounds})
	{
		float Value = 0.0f;
		if (TestTrue(FString::Printf(TEXT("%s holds %s in [%s]"), *DefaultsPath, Bounds.Name, *Section), Defaults.GetFloat(*Section, Bounds.Name, Value)))
		{
			TestTrue(FString::Printf(TEXT("The default %s %s is inside the bounds %s to %s"), Bounds.Name, *FString::SanitizeFloat(Value), *FString::SanitizeFloat(Bounds.Min), *FString::SanitizeFloat(Bounds.Max)), Bounds.Contains(Value));
		}
	}

	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
