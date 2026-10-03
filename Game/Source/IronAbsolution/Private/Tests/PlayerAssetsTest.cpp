// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Engine/World.h"
#include "GameFramework/GameModeBase.h"
#include "GameFramework/WorldSettings.h"
#include "InputAction.h"
#include "InputMappingContext.h"
#include "InputModifiers.h"
#include "IronAmmoStation.h"
#include "IronDoor.h"
#include "IronHUD.h"
#include "IronMovementTuning.h"
#include "IronPlayerCharacter.h"
#include "IronPlayerController.h"
#include "IronSwitch.h"
#include "IronTarget.h"
#include "IronWeaponTuning.h"
#include "Materials/Material.h"
#include "Sound/SoundWave.h"
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
		if (const UInputModifierScalar* Scalar = Cast<UInputModifierScalar>(Modifier))
		{
			return FString::Printf(TEXT("Scalar(%s)"), *FString::SanitizeFloat(Scalar->Scalar.X));
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
		{TEXT("InteractAction"), TEXT("/Game/Input/IA_Interact.IA_Interact"), EInputActionValueType::Boolean},
		{TEXT("FireAction"), TEXT("/Game/Input/IA_Fire.IA_Fire"), EInputActionValueType::Boolean},
		{TEXT("ChangeWeaponAction"), TEXT("/Game/Input/IA_ChangeWeapon.IA_ChangeWeapon"), EInputActionValueType::Boolean},
		{TEXT("SelectWeaponAction"), TEXT("/Game/Input/IA_SelectWeapon.IA_SelectWeapon"), EInputActionValueType::Axis1D},
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

	// The outline of the cue of interact: a post-process material before the bloom (D-145, D-148).
	const UMaterialInterface* Outline = ReadObjectProperty<UMaterialInterface>(*CharacterDefaults, TEXT("InteractOutlineMaterial"));
	TestEqual(TEXT("The outline material of the character"), GetPathNameSafe(Outline), FString(TEXT("/Game/Player/M_InteractOutline.M_InteractOutline")));
	if (Outline != nullptr)
	{
		const UMaterial* OutlineBase = Outline->GetMaterial();
		TestTrue(TEXT("The outline material is a post-process material"), OutlineBase->MaterialDomain == MD_PostProcess);
		TestTrue(TEXT("The outline material runs before the bloom, so the bloom makes it glow"), OutlineBase->BlendableLocation == BL_SceneColorBeforeBloom);
#if WITH_EDITORONLY_DATA
		// The graph of a material is editor-only data. The game target builds this test with no graph.
		TestTrue(TEXT("The outline material writes the emissive color"), OutlineBase->GetEditorOnlyData()->EmissiveColor.IsConnected());
#endif
	}

	// The fill glow of the cue: an additive unlit overlay (D-148).
	const UMaterialInterface* Glow = ReadObjectProperty<UMaterialInterface>(*CharacterDefaults, TEXT("InteractGlowMaterial"));
	TestEqual(TEXT("The glow material of the character"), GetPathNameSafe(Glow), FString(TEXT("/Game/Player/M_InteractGlow.M_InteractGlow")));
	if (Glow != nullptr)
	{
		const UMaterial* GlowBase = Glow->GetMaterial();
		TestTrue(TEXT("The glow material is a surface material"), GlowBase->MaterialDomain == MD_Surface);
		TestTrue(TEXT("The glow material adds its color"), GlowBase->BlendMode == BLEND_Additive);
		TestTrue(TEXT("The glow material is unlit"), GlowBase->GetShadingModels().HasOnlyShadingModel(MSM_Unlit));
	}

	// The two data assets of the weapon rule, each valid, with different tuning (D-128, D-153).
	const TArray<TObjectPtr<UIronWeaponTuning>>& Weapons = CharacterDefaults->GetWeapons();
	if (TestEqual(TEXT("The character has two weapons"), Weapons.Num(), 2))
	{
		TestEqual(TEXT("The weapon of slot 1"), GetPathNameSafe(Weapons[0]), FString(TEXT("/Game/Weapons/DA_WeaponRifle.DA_WeaponRifle")));
		TestEqual(TEXT("The weapon of slot 2"), GetPathNameSafe(Weapons[1]), FString(TEXT("/Game/Weapons/DA_WeaponScatter.DA_WeaponScatter")));
		for (const TObjectPtr<UIronWeaponTuning>& Weapon : Weapons)
		{
			if (Weapon != nullptr)
			{
				TestEqual(FString::Printf(TEXT("The invalid values of %s: %s"), *Weapon->GetPathName(), *FString::Join(Weapon->FindInvalidValues(), TEXT(" | "))), Weapon->FindInvalidValues().Num(), 0);
			}
		}
		if (Weapons[0] != nullptr && Weapons[1] != nullptr)
		{
			TestTrue(TEXT("One weapon is automatic and the other is not"), Weapons[0]->bAutomatic != Weapons[1]->bAutomatic);
			TestTrue(TEXT("The two weapons have different counts of pellets"), Weapons[0]->PelletCount != Weapons[1]->PelletCount);

			// The mix of D-168: the rifle fires 10 shots each second and its sounds overlap, so each
			// shot of the rifle plays quieter than a shot of the scatter gun.
			const USoundWave* RifleShot = Cast<USoundWave>(Weapons[0]->ShotSound);
			const USoundWave* ScatterShot = Cast<USoundWave>(Weapons[1]->ShotSound);
			if (TestNotNull(TEXT("The shot sound of the rifle is a sound wave"), RifleShot) && TestNotNull(TEXT("The shot sound of the scatter gun is a sound wave"), ScatterShot))
			{
				TestTrue(FString::Printf(TEXT("The rifle shot, at volume %f, is quieter than the scatter shot, at volume %f"), RifleShot->Volume, ScatterShot->Volume), RifleShot->Volume < ScatterShot->Volume);
			}
		}
	}

	// The controller: the mapping context of the keyboard and the mouse, alone (OQ-21).
	const TArray<TObjectPtr<UInputMappingContext>>& Contexts = GetDefault<AIronPlayerController>(Controller)->GetMappingContexts();
	TestEqual(TEXT("The controller has one mapping context"), Contexts.Num(), 1);
	if (Contexts.Num() == 1)
	{
		TestEqual(TEXT("The mapping context of the controller"), GetPathNameSafe(Contexts[0]), FString(MappingContext));
	}

	// The quit action of the controller (D-150).
	const UInputAction* Quit = ReadObjectProperty<UInputAction>(*GetDefault<AIronPlayerController>(Controller), TEXT("QuitAction"));
	TestEqual(TEXT("The QuitAction of the controller"), GetPathNameSafe(Quit), FString(TEXT("/Game/Input/IA_Quit.IA_Quit")));

	// The keys of D-136. The move action has X to the right and Y forward. The mouse has no
	// modifier, because a mouse count reaches the character as it is (D-139).
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
		TEXT("E IA_Interact"),
		TEXT("Escape IA_Quit"),
		TEXT("Mouse2D IA_Look"),
		// The weapon keys of D-151 and D-152. The scalar gives the key 2 the value of slot 2.
		TEXT("LeftMouseButton IA_Fire"),
		TEXT("MouseScrollUp IA_ChangeWeapon"),
		TEXT("MouseScrollDown IA_ChangeWeapon"),
		TEXT("One IA_SelectWeapon"),
		TEXT("Two IA_SelectWeapon Scalar(2.0)"),
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
		TestEqual(TEXT("The HUD of the game mode of the gym"), GetPathNameSafe(GameModeDefaults->HUDClass.Get()), FString(TEXT("/Game/Player/BP_PlayerHUD.BP_PlayerHUD_C")));
		if (GameModeDefaults->HUDClass != nullptr && GameModeDefaults->HUDClass->IsChildOf<AIronHUD>())
		{
			const TArray<FString> HudErrors = GetDefault<AIronHUD>(GameModeDefaults->HUDClass)->FindInvalidValues();
			TestEqual(FString::Printf(TEXT("The invalid values of the HUD: %s"), *FString::Join(HudErrors, TEXT(" | "))), HudErrors.Num(), 0);
		}
	}

	// The gym has one test switch that names the one test door, and the door moves when it opens (D-146).
	TArray<const AIronSwitch*> Switches;
	TArray<const AIronDoor*> Doors;
	TArray<const AIronTarget*> Targets;
	TArray<const AIronAmmoStation*> Stations;
	for (const AActor* Actor : Gym->PersistentLevel->Actors)
	{
		if (const AIronTarget* Target = Cast<AIronTarget>(Actor))
		{
			Targets.Add(Target);
		}
		if (const AIronAmmoStation* Station = Cast<AIronAmmoStation>(Actor))
		{
			Stations.Add(Station);
		}
		if (const AIronSwitch* Switch = Cast<AIronSwitch>(Actor))
		{
			Switches.Add(Switch);
		}
		if (const AIronDoor* Door = Cast<AIronDoor>(Actor))
		{
			Doors.Add(Door);
		}
	}
	if (TestEqual(TEXT("The gym has one test switch"), Switches.Num(), 1) && TestEqual(TEXT("The gym has one test door"), Doors.Num(), 1))
	{
		TestEqual(TEXT("The test switch names the test door"), static_cast<const AIronDoor*>(Switches[0]->GetDoor()), Doors[0]);
		TestFalse(TEXT("The test door has an open offset"), Doors[0]->GetOpenOffset().IsNearlyZero());
		TestNotNull(TEXT("The panel of the test door has a mesh"), Doors[0]->GetPanel()->GetStaticMesh().Get());
		TestNotNull(TEXT("The button of the test switch has a mesh"), Switches[0]->GetButton()->GetStaticMesh().Get());
	}

	// The targets of the weapon at 10 m, 25 m, and 50 m, and one ammo station (D-156).
	if (TestEqual(TEXT("The gym has three targets"), Targets.Num(), 3))
	{
		for (const AIronTarget* Target : Targets)
		{
			TestNotNull(FString::Printf(TEXT("The board of %s has a mesh"), *Target->GetName()), Target->GetBoard()->GetStaticMesh().Get());
		}
	}
	if (TestEqual(TEXT("The gym has one ammo station"), Stations.Num(), 1))
	{
		TestNotNull(TEXT("The body of the ammo station has a mesh"), Stations[0]->GetBody()->GetStaticMesh().Get());
	}

	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
