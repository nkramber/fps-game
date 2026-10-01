// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "EnhancedInputComponent.h"
#include "InputAction.h"
#include "IronPlayerController.h"
#include "Misc/AutomationTest.h"
#include "PlayerTestWorld.h"

#if WITH_DEV_AUTOMATION_TESTS

namespace IronAbsolution::Tests::Quit
{
	const TCHAR* const ControllerClass = TEXT("/Game/Player/BP_PlayerController.BP_PlayerController_C");
}

// The quit key closes the game until the menu of phase 8 (D-150). The test reads the binding. It
// does not press the key, because the quit would close the editor that runs the test.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionQuitBindingTest,
	"IronAbsolution.Player.Quit.Binding",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionQuitBindingTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Quit;

	FPlayerTestWorld World;
	if (World.Start(*this) == nullptr)
	{
		return false;
	}
	UClass* Class = LoadClass<AIronPlayerController>(nullptr, ControllerClass);
	if (!TestNotNull(FString::Printf(TEXT("The class %s"), ControllerClass), Class))
	{
		return false;
	}

	// A controller with no local player gets no input component from the engine, so the test makes one.
	AIronPlayerController* Controller = World.GetWorld()->SpawnActor<AIronPlayerController>(Class);
	UEnhancedInputComponent* EnhancedInput = NewObject<UEnhancedInputComponent>(Controller);
	if (!TestNotNull(TEXT("The spawned player controller"), Controller) || !TestTrue(TEXT("The controller binds its quit action"), Controller->BindQuitAction(*EnhancedInput)))
	{
		return false;
	}

	TArray<FString> Bindings;
	for (const TUniquePtr<FEnhancedInputActionEventBinding>& Binding : EnhancedInput->GetActionEventBindings())
	{
		Bindings.Add(FString::Printf(TEXT("%s %s"), *GetNameSafe(Binding->GetAction()), *UEnum::GetValueAsString(Binding->GetTriggerEvent())));
	}
	TestEqual(TEXT("The bindings of the input component"), FString::Join(Bindings, TEXT(", ")), FString(TEXT("IA_Quit ETriggerEvent::Started")));
	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
