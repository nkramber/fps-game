// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Components/StaticMeshComponent.h"
#include "IronHitFlash.h"
#include "IronHUD.h"
#include "IronMeleeComponent.h"
#include "IronMeleeTuning.h"
#include "IronPlayerCharacter.h"
#include "IronTarget.h"
#include "IronWeaponComponent.h"
#include "IronWeaponTuning.h"
#include "Misc/AutomationTest.h"
#include "PlayerTestWorld.h"

#include <limits>

#if WITH_DEV_AUTOMATION_TESTS

// The melee tests of PR-26 (D-116, D-169 to D-173). The player character looks along +X, so a
// target on the line of the view stands at +X of the eye.
//
// The rule tests give the melee component their own tuning: a copy of the data asset of the
// Blueprint, with fixed numbers. So a change of the numbers in the feel pass of PR-27 does not
// change these tests (D-131). The asset test checks the data asset of the Blueprint.
namespace IronAbsolution::Tests::Melee
{
	// The numbers of the test tuning: the reach of the front of the sphere, the radius of the sphere,
	// the time between two attacks, and the jab.
	constexpr float Range = 250.0f;
	constexpr float SweepRadius = 30.0f;
	constexpr float AttackInterval = 0.8f;
	constexpr float JabDistance = 20.0f;
	constexpr float JabTime = 0.2f;

	/**
	 * Starts the world, and gives the player character the test tuning of the melee attack. Gives
	 * null after a test error for each problem.
	 */
	AIronPlayerCharacter* StartWithMelee(FPlayerTestWorld& World, FAutomationTestBase& Test)
	{
		AIronPlayerCharacter* Character = World.Start(Test);
		if (Character == nullptr || !Test.TestNotNull(TEXT("The melee tuning of the Blueprint"), Character->GetMeleeTuning()))
		{
			return nullptr;
		}
		Test.TestEqual(TEXT("The melee attack took the data asset of the Blueprint when play started"), Character->GetMelee()->GetTuning(), Character->GetMeleeTuning());
		UIronMeleeTuning* Tuning = DuplicateObject<UIronMeleeTuning>(Character->GetMeleeTuning(), GetTransientPackage());
		Tuning->Range = Range;
		Tuning->SweepRadius = SweepRadius;
		Tuning->AttackInterval = AttackInterval;
		Tuning->JabDistance = JabDistance;
		Tuning->JabTime = JabTime;
		if (!Test.TestTrue(TEXT("The melee attack takes the test tuning"), Character->GetMelee()->SetTuning(Tuning)))
		{
			return nullptr;
		}
		return Character;
	}

	/** Places a test target with the face of its board at a distance from the eye, on the line of the view. */
	AIronTarget* SpawnTargetWithFaceAt(FPlayerTestWorld& World, const AIronPlayerCharacter& Character, double FaceDistance)
	{
		return SpawnTarget(World, Character, FaceDistance + TargetSize.X / 2.0);
	}
}

// Exit test 2 of PR-26: a hit in range, and a miss out of range. The sphere of the sweep hits a
// target a little off the line of the view. A wall between the eye and the target takes the attack.
// A melee hit counts apart from the shots, and shows the flash and the hit marker (D-171, D-172).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionMeleeHitTest,
	"IronAbsolution.Player.Melee.Hit",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionMeleeHitTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Melee;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithMelee(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	UIronMeleeComponent* Melee = Character->GetMelee();
	const float Wait = AttackInterval + 0.05f;

	// In range: one melee hit, no shot hit, one flash at the face of the board, and the time of the hit marker.
	AIronTarget* Target = SpawnTargetWithFaceAt(World, *Character, 200.0);
	World.Tick(FrameSeconds, [] {});
	TestFalse(TEXT("No hit marker before the first melee hit"), Melee->GetLastTargetHitTime().IsSet());
	TestTrue(TEXT("The first attack happens"), Character->MeleeAttack());
	TestEqual(TEXT("An attack at a target in range hits it once"), Target->GetMeleeHitCount(), 1);
	TestEqual(TEXT("A melee hit does not count as a shot"), Target->GetShotHitCount(), 0);
	TestTrue(TEXT("The hit sets the time of the hit marker"), Melee->GetLastTargetHitTime().IsSet());
	const TArray<AIronHitFlash*> Flashes = FindFlashes(World.GetWorld());
	if (TestEqual(TEXT("The hit makes one flash"), Flashes.Num(), 1))
	{
		const double FlashDistance = Flashes[0]->GetActorLocation().X - Character->GetPawnViewLocation().X;
		TestEqual(TEXT("The flash is at the face of the board"), FlashDistance, 200.0, 0.5);
	}

	// At the edge of the range: the front of the sphere reaches the face at 245 cm.
	Target->SetActorLocation(PointOnView(*Character, 245.0 + TargetSize.X / 2.0));
	World.Tick(Wait, [] {});
	TestTrue(TEXT("The second attack happens"), Character->MeleeAttack());
	TestEqual(TEXT("An attack hits a target 5 cm inside the range"), Target->GetMeleeHitCount(), 2);

	// Off the line: the edge of the board is 20 cm to the side of the line of the view. A line
	// misses it, and the sphere of 30 cm hits it.
	Target->SetActorLocation(PointOnView(*Character, 150.0) + FVector(0.0, TargetSize.Y / 2.0 + 20.0, 0.0));
	World.Tick(Wait, [] {});
	TestTrue(TEXT("The third attack happens"), Character->MeleeAttack());
	TestEqual(TEXT("The sphere hits a target 20 cm off the line of the view"), Target->GetMeleeHitCount(), 3);

	// Out of range: the face at 260 cm. The attack happens and misses, and the hit marker does not move.
	const double HitTime = Melee->GetLastTargetHitTime().GetValue();
	Target->SetActorLocation(PointOnView(*Character, 260.0 + TargetSize.X / 2.0));
	World.Tick(Wait, [] {});
	TestTrue(TEXT("An attack that misses still happens"), Character->MeleeAttack());
	TestEqual(TEXT("An attack at a target 10 cm out of range does not hit it"), Target->GetMeleeHitCount(), 3);
	TestEqual(TEXT("A miss does not move the time of the hit marker"), Melee->GetLastTargetHitTime().GetValue(), HitTime);

	// In range, behind a wall: the wall takes the attack.
	Target->SetActorLocation(PointOnView(*Character, 200.0 + TargetSize.X / 2.0));
	World.SpawnBlock(PointOnView(*Character, 100.0), FVector(10.0, 300.0, 300.0));
	World.Tick(Wait, [] {});
	TestTrue(TEXT("The attack at the wall happens"), Character->MeleeAttack());
	TestEqual(TEXT("A wall between the eye and the target takes the attack"), Target->GetMeleeHitCount(), 3);
	TestEqual(TEXT("No attack counted as a shot"), Target->GetShotHitCount(), 0);

	return !HasAnyErrors();
}

// Exit test 2 of PR-26: no second attack before the time between two attacks (D-170).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionMeleeIntervalTest,
	"IronAbsolution.Player.Melee.Interval",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionMeleeIntervalTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Melee;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithMelee(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	AIronTarget* Target = SpawnTargetWithFaceAt(World, *Character, 150.0);
	World.Tick(FrameSeconds, [] {});

	TestTrue(TEXT("The first attack happens"), Character->MeleeAttack());
	TestFalse(TEXT("A second press in the same frame makes no attack"), Character->MeleeAttack());
	World.Tick(0.5f, [] {});
	TestFalse(TEXT("A press 0.5 s after an attack, inside the time of 0.8 s, makes no attack"), Character->MeleeAttack());
	TestEqual(TEXT("The presses inside the time do not hit"), Target->GetMeleeHitCount(), 1);

	World.Tick(AttackInterval - 0.5f - 0.05f, [] {});
	TestFalse(TEXT("A press 0.05 s before the end of the time makes no attack"), Character->MeleeAttack());
	World.Tick(0.05f + FrameSeconds, [] {});
	TestTrue(TEXT("A press after the end of the time makes an attack"), Character->MeleeAttack());
	TestEqual(TEXT("The second attack hits"), Target->GetMeleeHitCount(), 2);

	return !HasAnyErrors();
}

// The jab of D-171 and the hold of the fire of D-173: on each attack, a hit or a miss, the weapon in
// the view moves forward and back in the jab time. The weapon fires no shot in this time, and a held
// fire key fires again after it.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionMeleeJabTest,
	"IronAbsolution.Player.Melee.Jab",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionMeleeJabTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Melee;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithMelee(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	UIronWeaponComponent* Weapon = Character->GetWeapon();
	if (!TestNotNull(TEXT("A weapon in hand"), Weapon->GetCurrentWeapon()))
	{
		return false;
	}
	UIronWeaponTuning* Automatic = DuplicateObject<UIronWeaponTuning>(Weapon->GetCurrentWeapon(), GetTransientPackage());
	Automatic->bAutomatic = true;
	Automatic->ShotsPerSecond = 10.0f;
	Automatic->AmmoCapacity = 100;
	Automatic->PelletCount = 1;
	Automatic->SpreadAngle = 0.0f;
	Automatic->RaiseTime = 0.1f;
	if (!TestTrue(TEXT("The weapon takes the automatic test tuning"), Weapon->SetWeapons({Automatic})))
	{
		return false;
	}
	World.Tick(FrameSeconds, [] {});
	const double RestX = Automatic->ViewOffset.X;
	TestEqual(TEXT("The weapon is at its place before the attack"), Character->GetWeaponViewMesh()->GetRelativeLocation().X, RestX, 0.01);

	// A miss: no target is in range. The jab is at its full distance at the middle of the jab time,
	// and back at its place after it.
	TestTrue(TEXT("The attack happens"), Character->MeleeAttack());
	World.Tick(JabTime / 2.0f, [] {});
	TestEqual(TEXT("The jab is at its full distance at the middle of its time"), Character->GetWeaponViewMesh()->GetRelativeLocation().X, RestX + JabDistance, 0.5);
	World.Tick(JabTime / 2.0f + FrameSeconds, [] {});
	TestEqual(TEXT("The weapon is back at its place after the jab"), Character->GetWeaponViewMesh()->GetRelativeLocation().X, RestX, 0.01);

	// A press of the fire key in the jab fires no shot. A press after the jab fires.
	World.Tick(AttackInterval, [] {});
	TestTrue(TEXT("The second attack happens"), Character->MeleeAttack());
	World.Tick(JabTime / 2.0f, [] {});
	Weapon->PullTrigger();
	Weapon->ReleaseTrigger();
	TestEqual(TEXT("A press of the fire key in the jab fires no shot"), Weapon->GetCurrentAmmo(), 100);
	World.Tick(JabTime / 2.0f + FrameSeconds, [] {});
	Weapon->PullTrigger();
	Weapon->ReleaseTrigger();
	TestEqual(TEXT("A press of the fire key after the jab fires"), Weapon->GetCurrentAmmo(), 99);

	// A held fire key: the attack stops the automatic fire for the jab, and the fire starts again after it.
	World.Tick(AttackInterval, [] {});
	Weapon->PullTrigger();
	TestEqual(TEXT("The press fires at once"), Weapon->GetCurrentAmmo(), 98);
	TestTrue(TEXT("The third attack happens"), Character->MeleeAttack());
	World.Tick(JabTime - 0.02f, [] {});
	TestEqual(TEXT("A held fire key fires no shot in the jab"), Weapon->GetCurrentAmmo(), 98);
	World.Tick(0.15f, [] {});
	Weapon->ReleaseTrigger();
	TestTrue(FString::Printf(TEXT("A held fire key fires again after the jab, %d rounds left"), Weapon->GetCurrentAmmo()), Weapon->GetCurrentAmmo() < 98);
	TestEqual(TEXT("The jab does not drop the weapon as a raise does"), Weapon->GetRaiseFraction(), 1.0f);

	return !HasAnyErrors();
}

// The hit marker of D-171 shows for the later hit, of a shot or of a melee attack.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionMeleeHitMarkerTest,
	"IronAbsolution.Player.Melee.HitMarker",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionMeleeHitMarkerTest::RunTest(const FString& Parameters)
{
	const TOptional<double> None;
	TestFalse(TEXT("No time before the first hit"), AIronHUD::GetLatestHitTime(None, None).IsSet());
	TestEqual(TEXT("A shot alone gives its time"), AIronHUD::GetLatestHitTime(TOptional<double>(4.0), None).Get(0.0), 4.0);
	TestEqual(TEXT("A melee hit alone gives its time"), AIronHUD::GetLatestHitTime(None, TOptional<double>(5.0)).Get(0.0), 5.0);
	TestEqual(TEXT("A later melee hit wins"), AIronHUD::GetLatestHitTime(TOptional<double>(4.0), TOptional<double>(5.0)).Get(0.0), 5.0);
	TestEqual(TEXT("A later shot wins"), AIronHUD::GetLatestHitTime(TOptional<double>(6.0), TOptional<double>(5.0)).Get(0.0), 6.0);
	return !HasAnyErrors();
}

// T-2: an invalid tuning and an absent tuning each write an error line and change nothing.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionMeleeErrorsTest,
	"IronAbsolution.Player.Melee.Errors",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionMeleeErrorsTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Melee;

	// A tuning with no value set: five numbers, the radius against the range, and the flash.
	UIronMeleeTuning* Empty = NewObject<UIronMeleeTuning>();
	const TArray<FString> EmptyErrors = Empty->FindInvalidValues();
	TestEqual(FString::Printf(TEXT("The errors of an empty tuning: %s"), *FString::Join(EmptyErrors, TEXT(" | "))), EmptyErrors.Num(), 7);

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithMelee(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	UIronMeleeComponent* Melee = Character->GetMelee();
	const UIronMeleeTuning* Valid = Melee->GetTuning();

	// One rule broken in each copy. A NaN is not a value.
	UIronMeleeTuning* NotFinite = DuplicateObject<UIronMeleeTuning>(Valid, GetTransientPackage());
	NotFinite->Range = std::numeric_limits<float>::quiet_NaN();
	TestEqual(TEXT("A NaN range is one error"), NotFinite->FindInvalidValues().Num(), 1);
	UIronMeleeTuning* WideSphere = DuplicateObject<UIronMeleeTuning>(Valid, GetTransientPackage());
	WideSphere->SweepRadius = WideSphere->Range;
	TestEqual(TEXT("A radius as large as the range is one error"), WideSphere->FindInvalidValues().Num(), 1);
	UIronMeleeTuning* LongJab = DuplicateObject<UIronMeleeTuning>(Valid, GetTransientPackage());
	LongJab->JabTime = LongJab->AttackInterval + 0.1f;
	const TArray<FString> LongJabErrors = LongJab->FindInvalidValues();
	if (TestEqual(TEXT("A jab longer than the time between two attacks is one error"), LongJabErrors.Num(), 1))
	{
		TestTrue(FString::Printf(TEXT("The error names the value and the tuning: %s"), *LongJabErrors[0]), LongJabErrors[0].Contains(TEXT("JabTime")) && LongJabErrors[0].Contains(LongJab->GetPathName()));
	}

	// A refused tuning changes nothing: the test tuning stays.
	AddExpectedMessagePlain(Empty->GetPathName(), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, EmptyErrors.Num());
	TestFalse(TEXT("The melee attack refuses an empty tuning"), Melee->SetTuning(Empty));
	AddExpectedMessagePlain(TEXT("did not take the tuning: it is null"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	TestFalse(TEXT("The melee attack refuses a null tuning"), Melee->SetTuning(nullptr));
	TestEqual(TEXT("The test tuning stays after each refusal"), Melee->GetTuning(), Valid);

	// A melee component with no tuning makes no attack and has no jab.
	UIronMeleeComponent* NoTuning = NewObject<UIronMeleeComponent>(Character);
	AddExpectedMessagePlain(TEXT("did not attack: it has no tuning"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	TestFalse(TEXT("A melee component with no tuning makes no attack"), NoTuning->Attack());
	TestEqual(TEXT("A component with no attack has no jab"), NoTuning->GetJabDistance(), 0.0f);

	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
