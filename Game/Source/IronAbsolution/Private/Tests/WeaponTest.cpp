// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Components/StaticMeshComponent.h"
#include "Engine/CollisionProfile.h"
#include "Engine/StaticMeshActor.h"
#include "EngineUtils.h"
#include "IronAmmoStation.h"
#include "IronHitFlash.h"
#include "IronHUD.h"
#include "IronPlayerCharacter.h"
#include "IronTarget.h"
#include "IronWeaponComponent.h"
#include "IronWeaponTuning.h"
#include "Misc/AutomationTest.h"
#include "PlayerTestWorld.h"

#include <limits>

#if WITH_DEV_AUTOMATION_TESTS

// The weapon tests of PR-25 (D-128, D-130, D-151 to D-162). The player character looks along +X,
// so a target on the line of the view stands at +X of the eye.
//
// The rule tests give the weapon component their own tunings: a copy of a data asset of the
// Blueprint, with new numbers. So a change of the numbers in the feel pass of PR-27 does not change
// these tests (D-131). The asset test reads the numbers of the data assets.
namespace IronAbsolution::Tests::Weapon
{
	// The board of a gym target: 10 cm deep, 100 cm wide, and 180 cm high.
	const FVector TargetSize(10.0, 100.0, 180.0);

	// A target this far to the side of the line of the view is out of the line.
	constexpr double SideStep = 300.0;

	/** Gives the point on the line of the view at a distance from the eye. */
	FVector PointOnView(const AIronPlayerCharacter& Character, double Distance)
	{
		return Character.GetPawnViewLocation() + FVector::ForwardVector * Distance;
	}

	/** Places a test target with the center of its board at a distance from the eye, on the line of the view. */
	AIronTarget* SpawnTarget(FPlayerTestWorld& World, const AIronPlayerCharacter& Character, double Distance)
	{
		AIronTarget* Target = World.GetWorld()->SpawnActor<AIronTarget>(PointOnView(Character, Distance), FRotator::ZeroRotator);
		// A static component takes no new mesh after play starts, so the board is movable.
		Target->GetBoard()->SetMobility(EComponentMobility::Movable);
		Target->GetBoard()->SetStaticMesh(World.GetCube());
		Target->GetBoard()->SetRelativeScale3D(TargetSize / CubeSize);
		return Target;
	}

	/** Starts the world, and checks that the player character took the two weapons of its Blueprint. */
	AIronPlayerCharacter* StartWithWeapons(FPlayerTestWorld& World, FAutomationTestBase& Test)
	{
		AIronPlayerCharacter* Character = World.Start(Test);
		if (Character == nullptr
			|| !Test.TestEqual(TEXT("The count of data assets of weapons on the Blueprint"), Character->GetWeapons().Num(), 2)
			|| !Test.TestEqual(TEXT("The count of weapons that the player character took"), Character->GetWeapon()->GetWeaponCount(), 2))
		{
			return nullptr;
		}
		return Character;
	}

	/**
	 * Makes a tuning for a rule test: a copy of a data asset of the Blueprint, for its content, with
	 * new numbers. The range is 100 m.
	 */
	UIronWeaponTuning* MakeTuning(const UIronWeaponTuning& Content, const TCHAR* Name, bool bAutomatic, float ShotsPerSecond, int32 AmmoCapacity, int32 PelletCount, float SpreadAngle, float RaiseTime)
	{
		UIronWeaponTuning* Tuning = DuplicateObject<UIronWeaponTuning>(&Content, GetTransientPackage());
		Tuning->DisplayName = FText::FromString(Name);
		Tuning->bAutomatic = bAutomatic;
		Tuning->ShotsPerSecond = ShotsPerSecond;
		Tuning->AmmoCapacity = AmmoCapacity;
		Tuning->Range = 10000.0f;
		Tuning->PelletCount = PelletCount;
		Tuning->SpreadAngle = SpreadAngle;
		Tuning->RaiseTime = RaiseTime;
		return Tuning;
	}

	/** Fires one press of the fire key: a pull and a release in the same frame. */
	void PressFire(AIronPlayerCharacter& Character)
	{
		Character.GetWeapon()->PullTrigger();
		Character.GetWeapon()->ReleaseTrigger();
	}

	/** Gives each hit flash of the world. */
	TArray<AIronHitFlash*> FindFlashes(UWorld* World)
	{
		TArray<AIronHitFlash*> Flashes;
		for (TActorIterator<AIronHitFlash> It(World); It; ++It)
		{
			Flashes.Add(*It);
		}
		return Flashes;
	}
}

// Exit test 3 of PR-25: a hit on a target in the line, and a miss on a target out of the line. A
// wall between the eye and the target blocks the shot. Each hit makes a flash at the point of the hit.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionWeaponHitTest,
	"IronAbsolution.Player.Weapon.Hit",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionWeaponHitTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Weapon;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithWeapons(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	UIronWeaponComponent* Weapon = Character->GetWeapon();
	UIronWeaponTuning* Single = MakeTuning(*Character->GetWeapons()[0], TEXT("Single"), false, 10.0f, 10, 1, 0.0f, 0.1f);
	if (!TestTrue(TEXT("The weapon takes the test tuning"), Weapon->SetWeapons({Single})))
	{
		return false;
	}

	// In the line: one hit, one flash at the face of the board, and the time of the hit marker.
	const double Distance = 500.0;
	AIronTarget* Target = SpawnTarget(World, *Character, Distance);
	World.Tick(FrameSeconds, [] {});
	TestFalse(TEXT("No hit marker before the first hit"), Weapon->GetLastTargetHitTime().IsSet());
	PressFire(*Character);
	TestEqual(TEXT("A shot at a target in the line hits it once"), Target->GetHitCount(), 1);
	TestTrue(TEXT("The hit sets the time of the hit marker"), Weapon->GetLastTargetHitTime().IsSet());
	const TArray<AIronHitFlash*> Flashes = FindFlashes(World.GetWorld());
	if (TestEqual(TEXT("The hit makes one flash"), Flashes.Num(), 1))
	{
		const FVector Face = PointOnView(*Character, Distance - TargetSize.X / 2.0);
		TestTrue(FString::Printf(TEXT("The flash at %s is at the face of the board at %s"), *Flashes[0]->GetActorLocation().ToString(), *Face.ToString()), Flashes[0]->GetActorLocation().Equals(Face, 0.1));
		TestTrue(TEXT("The flash has no collision, so a later shot goes through it"), Flashes[0]->GetFlashMesh()->GetCollisionEnabled() == ECollisionEnabled::NoCollision);
	}

	// Out of the line: no hit, and the time of the hit marker does not move.
	const double HitTime = Weapon->GetLastTargetHitTime().GetValue();
	Target->SetActorLocation(PointOnView(*Character, Distance) + FVector(0.0, SideStep, 0.0));
	World.Tick(0.2f, [] {});
	PressFire(*Character);
	TestEqual(TEXT("A shot at a target out of the line does not hit it"), Target->GetHitCount(), 1);
	TestEqual(TEXT("A miss does not move the time of the hit marker"), Weapon->GetLastTargetHitTime().GetValue(), HitTime);
	TestEqual(TEXT("The miss spent one round too"), Weapon->GetCurrentAmmo(), 8);

	// In the line, behind a wall: the wall takes the shot.
	Target->SetActorLocation(PointOnView(*Character, Distance));
	World.SpawnBlock(PointOnView(*Character, Distance / 2.0), FVector(10.0, 300.0, 300.0));
	World.Tick(0.2f, [] {});
	PressFire(*Character);
	TestEqual(TEXT("A wall between the eye and the target takes the shot"), Target->GetHitCount(), 1);

	return !HasAnyErrors();
}

// The flash of D-154 fades from full brightness to 0 over its life, and then goes away.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionWeaponHitFlashTest,
	"IronAbsolution.Player.Weapon.HitFlash",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionWeaponHitFlashTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Weapon;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithWeapons(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	SpawnTarget(World, *Character, 500.0);
	World.Tick(FrameSeconds, [] {});
	PressFire(*Character);

	TArray<AIronHitFlash*> Flashes = FindFlashes(World.GetWorld());
	if (!TestTrue(TEXT("The shot of the weapon in hand makes at least one flash"), !Flashes.IsEmpty()))
	{
		return false;
	}
	AIronHitFlash* Flash = Flashes[0];
	const float Lifetime = Flash->GetLifetime();
	TestTrue(FString::Printf(TEXT("The flash has a life, %f s"), Lifetime), Lifetime > 0.0f);
	TestEqual(TEXT("The flash is at full brightness at the spawn"), Flash->GetBrightness(), 1.0f);
	TestNotNull(TEXT("The flash has the mesh of its Blueprint"), Flash->GetFlashMesh()->GetStaticMesh().Get());

	World.Tick(Lifetime / 2.0f, [] {});
	TestTrue(FString::Printf(TEXT("At half of its life, the flash is at half brightness, %f"), Flash->GetBrightness()), FMath::IsNearlyEqual(Flash->GetBrightness(), 0.5f, 0.1f));

	World.Tick(Lifetime, [] {});
	TestEqual(TEXT("After its life, no flash is left"), FindFlashes(World.GetWorld()).Num(), 0);

	return !HasAnyErrors();
}

// The rate of fire of D-151: a held fire key fires an automatic weapon at its rate, and a
// semi-automatic weapon once for each press, never faster than its rate.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionWeaponRateOfFireTest,
	"IronAbsolution.Player.Weapon.RateOfFire",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionWeaponRateOfFireTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Weapon;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithWeapons(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	UIronWeaponComponent* Weapon = Character->GetWeapon();
	const UIronWeaponTuning& Content = *Character->GetWeapons()[0];

	// Automatic at 10 shots each second: one shot at the press, then one each 0.1 s. A hold of
	// 0.95 s ends before the shot at 1.0 s.
	UIronWeaponTuning* Automatic = MakeTuning(Content, TEXT("Automatic"), true, 10.0f, 100, 1, 0.0f, 0.1f);
	if (!TestTrue(TEXT("The weapon takes the automatic tuning"), Weapon->SetWeapons({Automatic})))
	{
		return false;
	}
	Weapon->PullTrigger();
	World.Tick(0.95f, [] {});
	TestEqual(TEXT("A hold of 0.95 s at 10 shots each second fires 10 shots"), 100 - Weapon->GetCurrentAmmo(), 10);
	Weapon->ReleaseTrigger();
	World.Tick(0.5f, [] {});
	TestEqual(TEXT("A released fire key fires no more shots"), Weapon->GetCurrentAmmo(), 90);
	PressFire(*Character);
	TestEqual(TEXT("A new press after a pause fires at once"), Weapon->GetCurrentAmmo(), 89);

	// Semi-automatic at 2 shots each second: a held key fires once, and a press within 0.5 s of a
	// shot does not fire.
	UIronWeaponTuning* SemiAutomatic = MakeTuning(Content, TEXT("Semi-automatic"), false, 2.0f, 10, 1, 0.0f, 0.1f);
	if (!TestTrue(TEXT("The weapon takes the semi-automatic tuning"), Weapon->SetWeapons({SemiAutomatic})))
	{
		return false;
	}
	Weapon->PullTrigger();
	World.Tick(1.0f, [] {});
	TestEqual(TEXT("A held key fires a semi-automatic weapon once"), Weapon->GetCurrentAmmo(), 9);
	Weapon->ReleaseTrigger();
	PressFire(*Character);
	TestEqual(TEXT("A new press 1 s after the shot fires"), Weapon->GetCurrentAmmo(), 8);
	World.Tick(0.2f, [] {});
	PressFire(*Character);
	TestEqual(TEXT("A press 0.2 s after a shot, inside the interval of 0.5 s, does not fire"), Weapon->GetCurrentAmmo(), 8);
	World.Tick(0.35f, [] {});
	PressFire(*Character);
	TestEqual(TEXT("A press 0.55 s after a shot fires"), Weapon->GetCurrentAmmo(), 7);

	return !HasAnyErrors();
}

// Exit test 3 of PR-25, and D-115, D-156, D-158: each shot spends one round, also with more than
// one pellet. An empty weapon does not fire. The ammo station fills each weapon.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionWeaponAmmoTest,
	"IronAbsolution.Player.Weapon.Ammo",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionWeaponAmmoTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Weapon;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithWeapons(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	UIronWeaponComponent* Weapon = Character->GetWeapon();
	const UIronWeaponTuning& Content = *Character->GetWeapons()[0];
	UIronWeaponTuning* Small = MakeTuning(Content, TEXT("Small"), true, 10.0f, 3, 1, 0.0f, 0.1f);
	UIronWeaponTuning* Ring = MakeTuning(Content, TEXT("Ring"), false, 2.0f, 2, 4, 2.0f, 0.1f);
	if (!TestTrue(TEXT("The weapon takes the two test tunings"), Weapon->SetWeapons({Small, Ring})))
	{
		return false;
	}
	TestEqual(TEXT("The first weapon starts full"), Weapon->GetAmmo(0), 3);
	TestEqual(TEXT("The second weapon starts full"), Weapon->GetAmmo(1), 2);

	// A hold of 1 s would fire 10 shots, but the weapon has 3 rounds.
	AIronTarget* Target = SpawnTarget(World, *Character, 300.0);
	Weapon->PullTrigger();
	World.Tick(1.0f, [] {});
	Weapon->ReleaseTrigger();
	TestEqual(TEXT("The automatic weapon stops at 0 rounds"), Weapon->GetAmmo(0), 0);
	TestEqual(TEXT("Each of the 3 rounds was one shot on the target"), Target->GetHitCount(), 3);

	// An empty weapon: no shot, and no change of the ammo.
	World.Tick(0.2f, [] {});
	PressFire(*Character);
	TestEqual(TEXT("An empty weapon stays at 0 rounds"), Weapon->GetAmmo(0), 0);
	TestEqual(TEXT("An empty weapon does not fire"), Target->GetHitCount(), 3);

	// A shot of 4 pellets spends one round.
	TestTrue(TEXT("The change to the weapon of 4 pellets"), Weapon->SelectWeapon(1));
	World.Tick(0.2f, [] {});
	PressFire(*Character);
	TestEqual(TEXT("A shot of 4 pellets spends one round"), Weapon->GetAmmo(1), 1);
	TestEqual(TEXT("Each of the 4 pellets hits the target"), Target->GetHitCount(), 7);
	TestEqual(TEXT("The shot of the second weapon does not change the first"), Weapon->GetAmmo(0), 0);

	// The ammo station in reach fills each weapon (D-156).
	Target->Destroy();
	AIronAmmoStation* Station = World.GetWorld()->SpawnActor<AIronAmmoStation>(PointOnView(*Character, 100.0), FRotator::ZeroRotator);
	Station->GetBody()->SetMobility(EComponentMobility::Movable);
	Station->GetBody()->SetStaticMesh(World.GetCube());
	Station->GetBody()->SetRelativeScale3D(FVector(0.5));
	World.Tick(FrameSeconds, [] {});
	TestEqual(TEXT("The ammo station in reach is the target of interact"), Character->FindInteractTarget(), static_cast<AActor*>(Station));
	TestTrue(TEXT("A use of the ammo station"), Character->Interact());
	TestEqual(TEXT("The station fills the first weapon"), Weapon->GetAmmo(0), 3);
	TestEqual(TEXT("The station fills the second weapon"), Weapon->GetAmmo(1), 2);

	return !HasAnyErrors();
}

// The change weapon test of PR-25 (D-128, D-152, D-157): after a change, the other data asset of the
// Blueprint gives its tuning, after its raise time.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionWeaponChangeTest,
	"IronAbsolution.Player.Weapon.Change",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionWeaponChangeTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Weapon;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithWeapons(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	UIronWeaponComponent* Weapon = Character->GetWeapon();
	const UIronWeaponTuning* First = Character->GetWeapons()[0];
	const UIronWeaponTuning* Second = Character->GetWeapons()[1];
	TestEqual(TEXT("The first weapon is in hand at the start"), Weapon->GetCurrentWeapon(), First);
	TestEqual(TEXT("The view shows the mesh scale of the first weapon"), Character->GetWeaponViewMesh()->GetRelativeScale3D(), First->ViewScale);

	// The change takes the second weapon in hand. It does not fire before the end of its raise time.
	AIronTarget* Target = SpawnTarget(World, *Character, 200.0);
	TestTrue(TEXT("The change to the next weapon"), Weapon->ChangeToNextWeapon());
	TestEqual(TEXT("The second weapon is in hand after the change"), Weapon->GetCurrentWeapon(), Second);
	TestEqual(TEXT("The raise starts at 0"), Weapon->GetRaiseFraction(), 0.0f);
	World.Tick(Second->RaiseTime - 0.05f, [] {});
	TestTrue(FString::Printf(TEXT("The view mesh is below its place during the raise, at Z %f"), Character->GetWeaponViewMesh()->GetRelativeLocation().Z), Character->GetWeaponViewMesh()->GetRelativeLocation().Z < Second->ViewOffset.Z);
	PressFire(*Character);
	TestEqual(TEXT("No shot 0.05 s before the end of the raise"), Weapon->GetAmmo(1), Second->AmmoCapacity);

	// After the raise, a shot uses the tuning of the second data asset: its pellets and its ammo.
	World.Tick(0.1f, [] {});
	TestEqual(TEXT("The raise is complete"), Weapon->GetRaiseFraction(), 1.0f);
	TestEqual(TEXT("The view shows the mesh scale of the second weapon"), Character->GetWeaponViewMesh()->GetRelativeScale3D(), Second->ViewScale);
	TestEqual(TEXT("The view mesh is at the place of the second weapon"), Character->GetWeaponViewMesh()->GetRelativeLocation(), Second->ViewOffset);
	PressFire(*Character);
	TestEqual(TEXT("A shot after the raise spends one round of the second weapon"), Weapon->GetAmmo(1), Second->AmmoCapacity - 1);
	TestEqual(TEXT("The shot does not spend a round of the first weapon"), Weapon->GetAmmo(0), First->AmmoCapacity);
	TestEqual(TEXT("Each pellet of the second data asset hits the target"), Target->GetHitCount(), Second->PelletCount);

	// The slot keys (D-152): slot 0 is the first weapon. A press of the slot in hand changes nothing.
	TestTrue(TEXT("The key of slot 1 takes the first weapon"), Weapon->SelectWeapon(0));
	TestEqual(TEXT("The first weapon is in hand"), Weapon->GetCurrentWeapon(), First);
	TestFalse(TEXT("The key of the slot in hand changes nothing"), Weapon->SelectWeapon(0));
	TestTrue(TEXT("The next weapon after the first is the second"), Weapon->ChangeToNextWeapon());
	TestTrue(TEXT("The next weapon after the last is the first"), Weapon->ChangeToNextWeapon());
	TestEqual(TEXT("The first weapon is in hand after two changes"), Weapon->GetCurrentSlot(), 0);

	return !HasAnyErrors();
}

// Exit test 4 of PR-25 (D-29): a third data asset makes a third tuning with no change of C++.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionWeaponThirdTuningTest,
	"IronAbsolution.Player.Weapon.ThirdTuning",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionWeaponThirdTuningTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Weapon;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithWeapons(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	UIronWeaponComponent* Weapon = Character->GetWeapon();
	UIronWeaponTuning* Third = MakeTuning(*Character->GetWeapons()[0], TEXT("Third"), false, 4.0f, 5, 3, 2.0f, 0.1f);
	Third->Range = 1000.0f;
	TArray<TObjectPtr<UIronWeaponTuning>> Weapons = Character->GetWeapons();
	Weapons.Add(Third);
	if (!TestTrue(TEXT("The weapon takes three data assets"), Weapon->SetWeapons(Weapons)))
	{
		return false;
	}
	TestEqual(TEXT("The third weapon starts with its own capacity"), Weapon->GetAmmo(2), 5);

	AIronTarget* Target = SpawnTarget(World, *Character, 300.0);
	TestTrue(TEXT("The key of slot 3 takes the third weapon"), Weapon->SelectWeapon(2));
	World.Tick(0.2f, [] {});
	PressFire(*Character);
	TestEqual(TEXT("A shot of the third weapon spends one of its 5 rounds"), Weapon->GetAmmo(2), 4);
	TestEqual(TEXT("Each of its 3 pellets hits the target"), Target->GetHitCount(), 3);

	// The range of the third weapon is 10 m, so a target at 11 m is out of reach.
	Target->SetActorLocation(PointOnView(*Character, 1100.0));
	World.Tick(0.3f, [] {});
	PressFire(*Character);
	TestEqual(TEXT("A shot spends a round on a target out of range"), Weapon->GetAmmo(2), 3);
	TestEqual(TEXT("A target out of the range of the third weapon takes no hit"), Target->GetHitCount(), 3);

	return !HasAnyErrors();
}

// The spread of D-160: each pellet goes to a random point of the cone around the view, new for each
// shot, and the points fill the cone. With no spread, the pellet goes along the view.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionWeaponPelletsTest,
	"IronAbsolution.Player.Weapon.Pellets",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionWeaponPelletsTest::RunTest(const FString& Parameters)
{
	UIronWeaponTuning* Tuning = NewObject<UIronWeaponTuning>();
	const FRotator View(10.0, 30.0, 0.0);
	const FVector Forward = View.Vector();
	// A fixed seed, so each run of the test reads the same points.
	const FRandomStream Random(25);

	Tuning->PelletCount = 1;
	Tuning->SpreadAngle = 0.0f;
	const TArray<FVector> One = UIronWeaponComponent::GetPelletDirections(View, *Tuning, Random);
	if (TestEqual(TEXT("One pellet gives one direction"), One.Num(), 1))
	{
		TestTrue(TEXT("One pellet with no spread goes along the view"), One[0].Equals(Forward, 1.0e-6));
	}

	// 100 shots of 8 pellets in a cone of 2 degrees.
	Tuning->PelletCount = 8;
	Tuning->SpreadAngle = 2.0f;
	double MaxAngle = 0.0;
	double MinAngle = 180.0;
	int32 InnerHalf = 0;
	int32 Count = 0;
	FVector Sum = FVector::ZeroVector;
	TArray<FVector> FirstShot;
	for (int32 Shot = 0; Shot < 100; ++Shot)
	{
		const TArray<FVector> Pellets = UIronWeaponComponent::GetPelletDirections(View, *Tuning, Random);
		if (!TestEqual(TEXT("Eight pellets give eight directions"), Pellets.Num(), 8))
		{
			return false;
		}
		if (Shot == 0)
		{
			FirstShot = Pellets;
		}
		else if (Shot == 1)
		{
			TestFalse(TEXT("The second shot has new points"), Pellets[0].Equals(FirstShot[0], 1.0e-9));
		}
		for (const FVector& Pellet : Pellets)
		{
			const double Angle = FMath::RadiansToDegrees(FMath::Acos(FMath::Clamp(FVector::DotProduct(Pellet, Forward), -1.0, 1.0)));
			TestTrue(TEXT("Each pellet is a unit vector"), Pellet.IsUnit(1.0e-6));
			MaxAngle = FMath::Max(MaxAngle, Angle);
			MinAngle = FMath::Min(MinAngle, Angle);
			// An even spread puts a quarter of the points inside half the angle, because the area
			// of a small cap grows with the square of its angle.
			InnerHalf += Angle < 1.0 ? 1 : 0;
			Sum += Pellet;
			++Count;
		}
	}
	TestTrue(FString::Printf(TEXT("No pellet is outside the cone of 2 degrees. The largest angle is %f"), MaxAngle), MaxAngle <= 2.0 + 1.0e-4);
	TestTrue(FString::Printf(TEXT("The pellets reach the edge of the cone. The largest angle is %f"), MaxAngle), MaxAngle > 1.9);
	TestTrue(FString::Printf(TEXT("The pellets reach the center of the cone. The smallest angle is %f"), MinAngle), MinAngle < 0.3);
	const double InnerPart = static_cast<double>(InnerHalf) / Count;
	TestTrue(FString::Printf(TEXT("About a quarter of the pellets are inside 1 degree, %f"), InnerPart), InnerPart > 0.18 && InnerPart < 0.32);
	const double MeanAngle = FMath::RadiansToDegrees(FMath::Acos(FMath::Clamp(FVector::DotProduct(Sum.GetSafeNormal(), Forward), -1.0, 1.0)));
	TestTrue(FString::Printf(TEXT("The mean of the pellets is on the view, %f degrees off"), MeanAngle), MeanAngle < 0.2);

	return !HasAnyErrors();
}

// The recoil of D-159, D-161, and D-162: each shot kicks the view up and to a random side, and the
// whole kick comes back in the recovery time after the last shot, also after a long burst. The
// control rotation does not change, and the view keeps to the pitch limits.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionWeaponRecoilTest,
	"IronAbsolution.Player.Weapon.Recoil",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionWeaponRecoilTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Weapon;

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithWeapons(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	UIronWeaponComponent* Weapon = Character->GetWeapon();
	UIronWeaponTuning* Kick = MakeTuning(*Character->GetWeapons()[0], TEXT("Kick"), true, 10.0f, 100, 1, 0.0f, 0.1f);
	Kick->RecoilKick = 1.0f;
	Kick->RecoilSideKick = 0.5f;
	Kick->RecoilRecoveryTime = 0.2f;
	if (!TestTrue(TEXT("The weapon takes the test tuning"), Weapon->SetWeapons({Kick})))
	{
		return false;
	}
	TestEqual(TEXT("No kick before the first shot"), Character->GetViewRotation().Pitch, 0.0);

	// Single shots: a kick of 1 degree up and a random side of up to 0.5 degree, half of it back in
	// 0.1 s, and all of it back in 0.2 s.
	int32 Left = 0;
	int32 Right = 0;
	for (int32 Shot = 0; Shot < 20; ++Shot)
	{
		PressFire(*Character);
		const FRotator View = Character->GetViewRotation();
		TestEqual(TEXT("A shot kicks the view up by the kick of the tuning"), View.Pitch, 1.0, 1.0e-4);
		TestTrue(FString::Printf(TEXT("The side of the kick is inside 0.5 degree, at %f"), View.Yaw), FMath::Abs(View.Yaw) <= 0.5 + 1.0e-4);
		Left += View.Yaw < 0.0 ? 1 : 0;
		Right += View.Yaw > 0.0 ? 1 : 0;
		if (Shot == 0)
		{
			FVector EyeLocation;
			FRotator EyeRotation;
			Character->GetActorEyesViewPoint(EyeLocation, EyeRotation);
			TestEqual(TEXT("The eye of the next shot has the kick"), EyeRotation.Pitch, 1.0, 1.0e-4);
			World.Tick(0.1f, [] {});
			TestEqual(TEXT("Half of the kick is back after half of the recovery time"), Character->GetViewRotation().Pitch, 0.5, 0.01);
			World.Tick(0.1f + FrameSeconds, [] {});
		}
		else
		{
			World.Tick(0.2f + FrameSeconds, [] {});
		}
		TestEqual(TEXT("The kick is back up and down after the recovery time"), Character->GetViewRotation().Pitch, 0.0);
		TestEqual(TEXT("The kick is back to the side after the recovery time"), FRotator::NormalizeAxis(Character->GetViewRotation().Yaw), 0.0, 1.0e-6);
	}
	TestTrue(FString::Printf(TEXT("The kicks go to each side: %d to the left and %d to the right"), Left, Right), Left > 0 && Right > 0);
	TestEqual(TEXT("The recoil does not turn the body"), Character->GetActorRotation().Pitch, 0.0);

	// A held key for 3 s: each shot kicks by 1 degree, and 0.1 s takes back half of the kick of the
	// moment. The view settles near 2 degrees, and all of it is back 0.2 s after the last shot.
	Weapon->PullTrigger();
	World.Tick(0.3f, [] {});
	TestTrue(FString::Printf(TEXT("A held key climbs above one kick, to %f degrees"), Character->GetViewRotation().Pitch), Character->GetViewRotation().Pitch > 1.0);
	World.Tick(2.7f, [] {});
	TestTrue(FString::Printf(TEXT("After 3 s, the climb settles at or below 2 degrees, at %f"), Character->GetViewRotation().Pitch), Character->GetViewRotation().Pitch <= 2.0 + 0.1);
	Weapon->ReleaseTrigger();
	World.Tick(0.2f + FrameSeconds, [] {});
	TestEqual(TEXT("The view is back at the aim 0.2 s after the end of a 3 s burst"), Character->GetViewRotation().Pitch, 0.0);

	// A large kick keeps to the pitch limit of the camera manager.
	Kick->RecoilKick = 45.0f;
	Kick->RecoilRecoveryTime = 10.0f;
	for (int32 Shot = 0; Shot < 3; ++Shot)
	{
		PressFire(*Character);
		World.Tick(0.11f, [] {});
	}
	TestTrue(FString::Printf(TEXT("A kick of more than 90 degrees keeps the view at the pitch limit, %f"), Character->GetViewRotation().Pitch), Character->GetViewRotation().Pitch <= 89.9 + 1.0e-4);

	return !HasAnyErrors();
}

// The hit marker of D-154 shows from the hit until the end of its time, and not before the first hit.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionWeaponHitMarkerTest,
	"IronAbsolution.Player.Weapon.HitMarker",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionWeaponHitMarkerTest::RunTest(const FString& Parameters)
{
	const float MarkerSeconds = 0.15f;
	TestFalse(TEXT("No marker before the first hit"), AIronHUD::IsHitMarkerShown(TOptional<double>(), 10.0, MarkerSeconds));
	TestTrue(TEXT("The marker shows at the hit"), AIronHUD::IsHitMarkerShown(TOptional<double>(10.0), 10.0, MarkerSeconds));
	TestTrue(TEXT("The marker shows 0.1 s after the hit"), AIronHUD::IsHitMarkerShown(TOptional<double>(10.0), 10.1, MarkerSeconds));
	TestFalse(TEXT("The marker does not show 0.2 s after the hit"), AIronHUD::IsHitMarkerShown(TOptional<double>(10.0), 10.2, MarkerSeconds));
	return !HasAnyErrors();
}

// T-2: an invalid tuning, an empty list, a slot with no weapon, and an ammo station for a user that
// is not the player each write an error line and change nothing. The trace channel has its name.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionWeaponErrorsTest,
	"IronAbsolution.Player.Weapon.Errors",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionWeaponErrorsTest::RunTest(const FString& Parameters)
{
	using namespace IronAbsolution::Tests;
	using namespace IronAbsolution::Tests::Weapon;

	TestEqual(TEXT("The name of the weapon trace channel"), UCollisionProfile::Get()->ReturnChannelNameFromContainerIndex(UIronWeaponComponent::TraceChannel).ToString(), FString(TEXT("Weapon")));

	// A tuning with no value set: each number, the name, and each part of the content.
	UIronWeaponTuning* Empty = NewObject<UIronWeaponTuning>();
	const TArray<FString> EmptyErrors = Empty->FindInvalidValues();
	TestEqual(FString::Printf(TEXT("The errors of an empty tuning: %s"), *FString::Join(EmptyErrors, TEXT(" | "))), EmptyErrors.Num(), 14);

	FPlayerTestWorld World;
	AIronPlayerCharacter* Character = StartWithWeapons(World, *this);
	if (Character == nullptr)
	{
		return false;
	}
	UIronWeaponComponent* Weapon = Character->GetWeapon();
	const UIronWeaponTuning& Content = *Character->GetWeapons()[0];
	TestEqual(TEXT("The first data asset of the Blueprint has no invalid value"), Content.FindInvalidValues().Num(), 0);

	// More than one pellet needs a spread. One pellet can have a spread. A NaN is not a value.
	UIronWeaponTuning* SpreadOne = MakeTuning(Content, TEXT("Spread one"), false, 2.0f, 5, 1, 2.0f, 0.1f);
	UIronWeaponTuning* PelletsFlat = MakeTuning(Content, TEXT("Pellets flat"), false, 2.0f, 5, 4, 0.0f, 0.1f);
	UIronWeaponTuning* NotFinite = MakeTuning(Content, TEXT("Not finite"), false, std::numeric_limits<float>::quiet_NaN(), 5, 1, 0.0f, 0.1f);
	TestEqual(TEXT("One pellet with a spread is valid"), SpreadOne->FindInvalidValues().Num(), 0);
	TestEqual(TEXT("Four pellets with no spread is one error"), PelletsFlat->FindInvalidValues().Num(), 1);
	TestEqual(TEXT("A NaN rate of fire is one error"), NotFinite->FindInvalidValues().Num(), 1);

	// A start of the shot sound at or after its end plays nothing (D-165).
	UIronWeaponTuning* LateStart = MakeTuning(Content, TEXT("Late start"), false, 2.0f, 5, 1, 0.0f, 0.1f);
	LateStart->ShotSoundStartTime = LateStart->ShotSound->GetDuration();
	TestEqual(TEXT("A start of the shot sound at its end is one error"), LateStart->FindInvalidValues().Num(), 1);

	// A refused list changes nothing: the weapons of the Blueprint stay. The test gives each log
	// line to the first expected message that matches it, so each pattern matches its own lines
	// alone. Each error line of the empty tuning names the path of that tuning.
	AddExpectedMessagePlain(Empty->GetPathName(), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, EmptyErrors.Num());
	TestFalse(TEXT("The weapon refuses an empty tuning"), Weapon->SetWeapons({Empty}));
	AddExpectedMessagePlain(TEXT("the list of weapons is empty"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	TestFalse(TEXT("The weapon refuses an empty list"), Weapon->SetWeapons({}));
	AddExpectedMessagePlain(TEXT("slot 1 has no data asset"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	const TArray<TObjectPtr<UIronWeaponTuning>> WithGap = {Character->GetWeapons()[0], nullptr};
	TestFalse(TEXT("The weapon refuses a slot with no data asset"), Weapon->SetWeapons(WithGap));
	TestEqual(TEXT("The weapons of the Blueprint stay after each refusal"), Weapon->GetWeaponCount(), 2);
	TestEqual(TEXT("The first weapon of the Blueprint stays in hand"), Weapon->GetCurrentWeapon(), &Content);

	AddExpectedMessagePlain(TEXT("slot 2 has no weapon"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	TestFalse(TEXT("A slot with no weapon is no change"), Weapon->SelectWeapon(2));
	TestEqual(TEXT("The weapon in hand stays"), Weapon->GetCurrentSlot(), 0);

	// The ammo station refuses a user that is not the player, and fills nothing.
	PressFire(*Character);
	const int32 Rounds = Weapon->GetCurrentAmmo();
	AIronAmmoStation* Station = World.GetWorld()->SpawnActor<AIronAmmoStation>(PointOnView(*Character, 1000.0), FRotator::ZeroRotator);
	AStaticMeshActor* Other = World.SpawnBlock(PointOnView(*Character, 2000.0), FVector(10.0));
	AddExpectedMessagePlain(TEXT("only the player character has weapons to fill"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	Station->Interact(*Other);
	TestEqual(TEXT("A use by another actor fills nothing"), Weapon->GetCurrentAmmo(), Rounds);

	// A hit flash with no Blueprint values writes an error line and removes itself.
	AddExpectedMessagePlain(TEXT("has an absent or invalid value"), ELogVerbosity::Error, EAutomationExpectedMessageFlags::Contains, 1);
	World.GetWorld()->SpawnActor<AIronHitFlash>(PointOnView(*Character, 3000.0), FRotator::ZeroRotator);
	TestEqual(TEXT("A hit flash with no values removes itself"), FindFlashes(World.GetWorld()).Num(), 0);

	// The base HUD has no Blueprint values, so each size, time, and color is an error.
	TestEqual(TEXT("The errors of the base HUD"), GetDefault<AIronHUD>()->FindInvalidValues().Num(), 10);

	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
