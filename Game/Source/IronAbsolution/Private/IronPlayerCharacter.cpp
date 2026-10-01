// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronPlayerCharacter.h"

#include "Camera/CameraComponent.h"
#include "Components/CapsuleComponent.h"
#include "Components/MeshComponent.h"
#include "Components/PrimitiveComponent.h"
#include "EnhancedInputComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Engine/World.h"
#include "InputActionValue.h"
#include "IronCharacterMovementComponent.h"
#include "IronGameUserSettings.h"
#include "IronInteractable.h"
#include "IronMovementTuning.h"
#include "Materials/MaterialInterface.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronPlayer, Log, All);

namespace IronAbsolution::PlayerCharacter
{
	// The collision size of the player. The level metrics depend on it: each hall and each door of
	// the level must take this capsule. So it is a rule of the class, not a value of the tuning.
	constexpr float CapsuleRadius = 35.0f;
	constexpr float CapsuleHalfHeight = 90.0f;
}

AIronPlayerCharacter::AIronPlayerCharacter(const FObjectInitializer& ObjectInitializer)
	// The movement component of the player adds the mantle (D-143).
	: Super(ObjectInitializer.SetDefaultSubobjectClass<UIronCharacterMovementComponent>(ACharacter::CharacterMovementComponentName))
{
	GetCapsuleComponent()->InitCapsuleSize(IronAbsolution::PlayerCharacter::CapsuleRadius, IronAbsolution::PlayerCharacter::CapsuleHalfHeight);

	// The body turns with the view on the yaw axis alone. The camera takes the pitch.
	bUseControllerRotationPitch = false;
	bUseControllerRotationYaw = true;
	bUseControllerRotationRoll = false;

	// ApplyMovementTuning sets the height of the camera from the tuning.
	FirstPersonCamera = CreateDefaultSubobject<UCameraComponent>(TEXT("FirstPersonCamera"));
	FirstPersonCamera->SetupAttachment(GetCapsuleComponent());
	FirstPersonCamera->bUsePawnControlRotation = true;
}

bool AIronPlayerCharacter::ApplyMovementTuning(const UIronMovementTuning& Tuning)
{
	const TArray<FString> Errors = Tuning.FindInvalidValues();
	for (const FString& Error : Errors)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s did not take the tuning: %s"), *GetPathName(), *Error);
	}
	if (!Errors.IsEmpty())
	{
		return false;
	}

	if (GetWorld() == nullptr)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s did not take the tuning %s: the character has no world, so the jump has no gravity."), *GetPathName(), *Tuning.GetPathName());
		return false;
	}

	// The check of the gravity comes before each write, so a refused tuning changes nothing. The
	// gravity of the world comes from the base class, because the movement component multiplies
	// it by the scale of the old tuning.
	UCharacterMovementComponent* Movement = GetCharacterMovement();
	const float WorldGravityZ = Movement->UMovementComponent::GetGravityZ();
	const float Gravity = FMath::Abs(WorldGravityZ * Tuning.GravityScale);
	if (Gravity <= UE_KINDA_SMALL_NUMBER)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s did not take the tuning %s: the gravity of the world is %f, so no jump comes down."), *GetPathName(), *Tuning.GetPathName(), WorldGravityZ);
		return false;
	}

	Movement->MaxWalkSpeed = Tuning.RunSpeed;
	Movement->MaxAcceleration = Tuning.Acceleration;
	Movement->BrakingDecelerationWalking = Tuning.BrakingDeceleration;
	Movement->GroundFriction = Tuning.GroundFriction;
	Movement->GravityScale = Tuning.GravityScale;
	Movement->AirControl = Tuning.AirControl;
	Movement->MaxStepHeight = Tuning.StepHeight;
	Movement->SetWalkableFloorAngle(Tuning.WalkableSlope);

	// The tuning gives the height of the jump, so a new gravity keeps the height. The start speed
	// of a jump to the height h under the gravity g is sqrt(2 * g * h).
	Movement->JumpZVelocity = FMath::Sqrt(2.0f * Gravity * Tuning.JumpHeight);
	GetIronMovement()->SetMantleTuning(Tuning.MantleMinHeight, Tuning.MantleMaxHeight, Tuning.MantleTime);
	InteractReach = Tuning.InteractReach;

	// The tuning gives the eye height above the feet. The camera is relative to the center of the capsule.
	const float EyeOffset = Tuning.EyeHeight - GetCapsuleComponent()->GetUnscaledCapsuleHalfHeight();
	BaseEyeHeight = EyeOffset;
	FirstPersonCamera->SetRelativeLocation(FVector(0.0f, 0.0f, EyeOffset));

	return true;
}

void AIronPlayerCharacter::Move(const FVector2D& Axis)
{
	// The body turns with the view on the yaw axis, so the axes of the actor are the axes of the view.
	AddMovementInput(GetActorForwardVector(), Axis.Y);
	AddMovementInput(GetActorRightVector(), Axis.X);
}

void AIronPlayerCharacter::Look(const FVector2D& MouseCounts)
{
	const float DegreesPerCount = UIronGameUserSettings::Get().GetDegreesPerMouseCount();
	AddControllerYawInput(MouseCounts.X * DegreesPerCount);
	AddControllerPitchInput(MouseCounts.Y * DegreesPerCount);
}

AActor* AIronPlayerCharacter::FindInteractTarget() const
{
	UWorld* World = GetWorld();
	if (World == nullptr)
	{
		return nullptr;
	}

	// The eye point of the pawn is the place and the turn of the camera, with or without a controller.
	FVector EyeLocation;
	FRotator EyeRotation;
	GetActorEyesViewPoint(EyeLocation, EyeRotation);
	const FVector End = EyeLocation + EyeRotation.Vector() * InteractReach;

	// A wall between the eye and a switch blocks the use, so the first blocking hit decides.
	const FCollisionQueryParams Params(SCENE_QUERY_STAT(IronInteract), false, this);
	FHitResult Hit;
	if (!World->LineTraceSingleByChannel(Hit, EyeLocation, End, ECC_Visibility, Params))
	{
		return nullptr;
	}

	AActor* HitActor = Hit.GetActor();
	return HitActor != nullptr && HitActor->Implements<UIronInteractable>() ? HitActor : nullptr;
}

bool AIronPlayerCharacter::Interact()
{
	IIronInteractable* Target = Cast<IIronInteractable>(FindInteractTarget());
	if (Target == nullptr)
	{
		return false;
	}

	Target->Interact(*this);
	return true;
}

AActor* AIronPlayerCharacter::GetOutlinedTarget() const
{
	return OutlinedTarget.Get();
}

const UIronMovementTuning* AIronPlayerCharacter::GetMovementTuning() const
{
	return MovementTuning;
}

UCameraComponent* AIronPlayerCharacter::GetFirstPersonCamera() const
{
	return FirstPersonCamera;
}

UIronCharacterMovementComponent* AIronPlayerCharacter::GetIronMovement() const
{
	// The constructor sets the class of the movement component, so the cast cannot fail.
	return CastChecked<UIronCharacterMovementComponent>(GetCharacterMovement());
}

void AIronPlayerCharacter::Tick(float DeltaSeconds)
{
	Super::Tick(DeltaSeconds);
	UpdateInteractCue();
}

void AIronPlayerCharacter::BeginPlay()
{
	Super::BeginPlay();

	// The field of view comes from the settings of the player, not from the tuning (D-141).
	ApplyFieldOfView();
	UIronGameUserSettings::Get().OnAimSettingsChanged.AddUObject(this, &AIronPlayerCharacter::ApplyFieldOfView);

	// The outline material waits on the camera with no weight, so it costs nothing until a target is in reach.
	if (InteractOutlineMaterial == nullptr)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s has no InteractOutlineMaterial, so no cue shows the target in reach. Set it in the Blueprint subclass (D-145)."), *GetPathName());
	}
	else
	{
		FirstPersonCamera->PostProcessSettings.AddBlendable(InteractOutlineMaterial, 0.0f);
	}
	if (InteractGlowMaterial == nullptr)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s has no InteractGlowMaterial, so the target in reach has no glow. Set it in the Blueprint subclass (D-148)."), *GetPathName());
	}

	if (MovementTuning == nullptr)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s has no MovementTuning, so it keeps the defaults of the engine. Set it in the Blueprint subclass (D-29)."), *GetPathName());
		return;
	}

	ApplyMovementTuning(*MovementTuning);
}

void AIronPlayerCharacter::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
	UIronGameUserSettings::Get().OnAimSettingsChanged.RemoveAll(this);
	ShowInteractCue(OutlinedTarget.Get(), false);
	OutlinedTarget.Reset();
	Super::EndPlay(EndPlayReason);
}

void AIronPlayerCharacter::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	UEnhancedInputComponent* EnhancedInput = Cast<UEnhancedInputComponent>(PlayerInputComponent);
	if (EnhancedInput == nullptr)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s got the input component %s, not an Enhanced Input component, so no action binds. The project settings name Enhanced Input (F-6)."), *GetPathName(), *GetPathNameSafe(PlayerInputComponent));
		return;
	}

	// Each absent action is an error of its own, so one log names each property to set.
	if (MoveAction == nullptr)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s has no MoveAction, so the player cannot move. Set it in the Blueprint subclass."), *GetPathName());
	}
	else
	{
		EnhancedInput->BindAction(MoveAction, ETriggerEvent::Triggered, this, &AIronPlayerCharacter::HandleMove);
	}

	if (LookAction == nullptr)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s has no LookAction, so the player cannot aim. Set it in the Blueprint subclass."), *GetPathName());
	}
	else
	{
		EnhancedInput->BindAction(LookAction, ETriggerEvent::Triggered, this, &AIronPlayerCharacter::HandleLook);
	}

	if (JumpAction == nullptr)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s has no JumpAction, so the player cannot jump. Set it in the Blueprint subclass."), *GetPathName());
	}
	else
	{
		EnhancedInput->BindAction(JumpAction, ETriggerEvent::Started, this, &ACharacter::Jump);
		EnhancedInput->BindAction(JumpAction, ETriggerEvent::Completed, this, &ACharacter::StopJumping);
	}

	if (InteractAction == nullptr)
	{
		UE_LOG(LogIronPlayer, Error, TEXT("%s has no InteractAction, so the player cannot use a switch or a door. Set it in the Blueprint subclass."), *GetPathName());
	}
	else
	{
		EnhancedInput->BindAction(InteractAction, ETriggerEvent::Started, this, &AIronPlayerCharacter::HandleInteract);
	}
}

void AIronPlayerCharacter::HandleMove(const FInputActionValue& Value)
{
	Move(Value.Get<FVector2D>());
}

void AIronPlayerCharacter::HandleLook(const FInputActionValue& Value)
{
	Look(Value.Get<FVector2D>());
}

void AIronPlayerCharacter::HandleInteract(const FInputActionValue& Value)
{
	Interact();
}

void AIronPlayerCharacter::UpdateInteractCue()
{
	AActor* Target = FindInteractTarget();
	if (Target == OutlinedTarget.Get())
	{
		return;
	}

	ShowInteractCue(OutlinedTarget.Get(), false);
	ShowInteractCue(Target, true);
	OutlinedTarget = Target;
	if (InteractOutlineMaterial != nullptr)
	{
		// A blendable with no weight adds no pass to the frame.
		FirstPersonCamera->PostProcessSettings.AddBlendable(InteractOutlineMaterial, Target != nullptr ? 1.0f : 0.0f);
	}
}

void AIronPlayerCharacter::ShowInteractCue(AActor* Target, bool bShown) const
{
	if (Target == nullptr)
	{
		return;
	}

	// The glow takes the overlay slot of each mesh. No target of phase 3 has an overlay of its own.
	UMaterialInterface* Glow = bShown ? InteractGlowMaterial.Get() : nullptr;
	Target->ForEachComponent<UPrimitiveComponent>(false, [bShown, Glow](UPrimitiveComponent* Component)
	{
		Component->SetRenderCustomDepth(bShown);
		if (UMeshComponent* Mesh = Cast<UMeshComponent>(Component))
		{
			Mesh->SetOverlayMaterial(Glow);
		}
	});
}

void AIronPlayerCharacter::ApplyFieldOfView()
{
	FirstPersonCamera->SetFieldOfView(UIronGameUserSettings::Get().GetFieldOfView());
}
