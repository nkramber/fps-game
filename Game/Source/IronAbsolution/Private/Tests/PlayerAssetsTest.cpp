// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Engine/World.h"
#include "GameFramework/GameModeBase.h"
#include "GameFramework/WorldSettings.h"
#include "InputAction.h"
#include "InputMappingContext.h"
#include "InputModifiers.h"
#include "IronMovementTuning.h"
#include "IronPlayerCharacter.h"
#include "IronPlayerController.h"
#include "Misc/AutomationTest.h"

#if WITH_DEV_AUTOMATION_TESTS

// The content of the player that `Game/Scripts/build_content.py` makes (D-134). A Blueprint or an
// asset is a binary file, so these tests read each value that the rules of the C++ need.
namespace IronAbsolution::Tests::PlayerAssets
{
	const TCHAR* const CharacterClass = TEXT("/Game/Player/BP_PlayerCharacter.BP_PlayerCharacter_C");
	const TCHAR* const ControllerClass = TEXT("/Game/Player/BP_PlayerController.BP_PlayerController_C");
	const TCHAR* const MappingContext = TEXT("/Game/Input/IMC_KeyboardMouse.IMC_KeyboardMouse");
	const TCHAR* const GymMap = TEXT("/Game/Maps/L_Gym.L_Gym");

	// The protected and private properties of the Blueprint subclasses, read by name. The tests
	// read what the Blueprint saved, and the C++ gives no setter for them.
	template <typename TValue>
	TValue* ReadObjectProperty(const UObject& Owner, const TCHAR* Name)
	{
		const FObjectPropertyBase* Property = CastField<FObjectPropertyBase>(Owner.GetClass()->FindPropertyByName(Name));
		return Property == nullptr ? nullptr : Cast<TValue>(Property->GetObjectPropertyValue_InContainer(&Owner));
	}

	/** Describes one modifier, such as "Negate(Y)" or "Swizzle(YXZ)". */
	FString DescribeModifier(const UInputModifier* Modifier)
	{
		if (const UInputModifierNegate* Negate = Cast<UInputModifierNegate>(Modifier))
		{
			return FString::Printf(TEXT("Negate(%s%s%s)"), Negate->bX ? TEXT("X") : TEXT(""), Negate->bY ? TEXT("Y") : TEXT(""), Negate->bZ ? TEXT("Z") : TEXT(""));
		}
		if (const UInputModifierSwizzleAxis* Swizzle = Cast<UInputModifierSwizzleAxis>(Modifier))
		{
			return FString::Printf(TEXT("Swizzle(%s)"), *StaticEnum<EInputAxisSwizzle>()->GetNameStringByValue(static_cast<int64>(Swizzle->Order)));
		}
		return GetNameSafe(Modifier ? Modifier->GetClass() : nullptr);
	}

	/** Describes one key mapping, such as "S IA_Move Swizzle(YXZ) Negate(XY)". */
	FString DescribeMapping(const FEnhancedActionKeyMapping& Mapping)
	{
		FString Line = FString::Printf(TEXT("%s %s"), *Mapping.Key.GetFName().ToString(), *GetNameSafe(Mapping.Action));
		for (const TObjectPtr<UInputModifier>& Modifier : Mapping.Modifiers)
		{
			Line += TEXT(" ") + DescribeModifier(Modifier);
		}
		return Line;
	}
}

// Exit test 1 of PR-21 needs the Blueprints to hold the tuning, the actions, and the keys.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionPlayerAssetsTest,
	"IronAbsolution.Player.Assets",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionPlayerAssetsTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests::PlayerAssets;

	const UClass* Character = LoadClass<AIronPlayerCharacter>(nullptr, CharacterClass);
	const UClass* Controller = LoadClass<AIronPlayerController>(nullptr, ControllerClass);
	const UInputMappingContext* Context = LoadObject<UInputMappingContext>(nullptr, MappingContext);
	const UWorld* Gym = LoadObject<UWorld>(nullptr, GymMap);
	if (!TestNotNull(FString::Printf(TEXT("The class %s"), CharacterClass), Character)
		|| !TestNotNull(FString::Printf(TEXT("The class %s"), ControllerClass), Controller)
		|| !TestNotNull(FString::Printf(TEXT("The mapping context %s"), MappingContext), Context)
		|| !TestNotNull(FString::Printf(TEXT("The map %s"), GymMap), Gym))
	{
		return false;
	}

	// The character: the tuning of the movement, and one action for each verb with its value type.
	const AIronPlayerCharacter* CharacterDefaults = GetDefault<AIronPlayerCharacter>(Character);
	const UIronMovementTuning* Tuning = CharacterDefaults->GetMovementTuning();
	TestEqual(TEXT("The tuning of the character"), GetPathNameSafe(Tuning), FString(TEXT("/Game/Player/DA_PlayerMovement.DA_PlayerMovement")));
	struct FActionCase
	{
		const TCHAR* Property;
		const TCHAR* Asset;
		EInputActionValueType ValueType;
	};
	const FActionCase Actions[] = {
		{TEXT("MoveAction"), TEXT("/Game/Input/IA_Move.IA_Move"), EInputActionValueType::Axis2D},
		{TEXT("LookAction"), TEXT("/Game/Input/IA_Look.IA_Look"), EInputActionValueType::Axis2D},
		{TEXT("JumpAction"), TEXT("/Game/Input/IA_Jump.IA_Jump"), EInputActionValueType::Boolean},
	};
	for (const FActionCase& Case : Actions)
	{
		const UInputAction* Action = ReadObjectProperty<UInputAction>(*CharacterDefaults, Case.Property);
		TestEqual(FString::Printf(TEXT("The %s of the character"), Case.Property), GetPathNameSafe(Action), FString(Case.Asset));
		if (Action != nullptr)
		{
			TestTrue(FString::Printf(TEXT("The value type of %s"), Case.Asset), Action->ValueType == Case.ValueType);
		}
	}

	// The controller: the mapping context of the keyboard and the mouse, alone (OQ-21).
	const TArray<TObjectPtr<UInputMappingContext>>& Contexts = GetDefault<AIronPlayerController>(Controller)->GetMappingContexts();
	TestEqual(TEXT("The controller has one mapping context"), Contexts.Num(), 1);
	if (Contexts.Num() == 1)
	{
		TestEqual(TEXT("The mapping context of the controller"), GetPathNameSafe(Contexts[0]), FString(MappingContext));
	}

	// The keys of D-136. The move action has X to the right and Y forward.
	TArray<FString> Mappings;
	for (const FEnhancedActionKeyMapping& Mapping : Context->GetMappings())
	{
		Mappings.Add(DescribeMapping(Mapping));
	}
	Mappings.Sort();
	TArray<FString> Expected = {
		TEXT("W IA_Move Swizzle(YXZ)"),
		TEXT("S IA_Move Swizzle(YXZ) Negate(XY)"),
		TEXT("A IA_Move Negate(XY)"),
		TEXT("D IA_Move"),
		TEXT("SpaceBar IA_Jump"),
		TEXT("Mouse2D IA_Look Negate(Y)"),
	};
	Expected.Sort();
	TestEqual(TEXT("The key mappings of the keyboard and the mouse"), FString::Join(Mappings, TEXT(", ")), FString::Join(Expected, TEXT(", ")));

	// The gym starts the player of this module through its game mode.
	const TSubclassOf<AGameModeBase> GameMode = Gym->PersistentLevel->GetWorldSettings()->DefaultGameMode;
	if (TestNotNull(TEXT("The game mode of the gym"), GameMode.Get()))
	{
		const AGameModeBase* GameModeDefaults = GameMode->GetDefaultObject<AGameModeBase>();
		TestEqual(TEXT("The pawn of the game mode of the gym"), GetPathNameSafe(GameModeDefaults->DefaultPawnClass.Get()), FString(CharacterClass));
		TestEqual(TEXT("The controller of the game mode of the gym"), GetPathNameSafe(GameModeDefaults->PlayerControllerClass.Get()), FString(ControllerClass));
	}

	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
