// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "IronPlayerCharacter.h"

#include "Camera/CameraComponent.h"
#include "Components/CapsuleComponent.h"
#include "EnhancedInputComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "InputActionValue.h"
#include "IronGameUserSettings.h"
#include "IronMovementTuning.h"

DEFINE_LOG_CATEGORY_STATIC(LogIronPlayer, Log, All);

namespace IronAbsolution::PlayerCharacter
{
	// The collision size of the player. The level metrics depend on it: each hall and each door of
	// the level must take this capsule. So it is a rule of the class, not a value of the tuning.
	constexpr float CapsuleRadius = 35.0f;
	constexpr float CapsuleHalfHeight = 90.0f;
}

AIronPlayerCharacter::AIronPlayerCharacter()
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

const UIronMovementTuning* AIronPlayerCharacter::GetMovementTuning() const
{
	return MovementTuning;
}

UCameraComponent* AIronPlayerCharacter::GetFirstPersonCamera() const
{
	return FirstPersonCamera;
}

void AIronPlayerCharacter::BeginPlay()
{
	Super::BeginPlay();

	// The field of view comes from the settings of the player, not from the tuning (D-141).
	ApplyFieldOfView();
	UIronGameUserSettings::Get().OnAimSettingsChanged.AddUObject(this, &AIronPlayerCharacter::ApplyFieldOfView);

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
}

void AIronPlayerCharacter::HandleMove(const FInputActionValue& Value)
{
	Move(Value.Get<FVector2D>());
}

void AIronPlayerCharacter::HandleLook(const FInputActionValue& Value)
{
	Look(Value.Get<FVector2D>());
}

void AIronPlayerCharacter::ApplyFieldOfView()
{
	FirstPersonCamera->SetFieldOfView(UIronGameUserSettings::Get().GetFieldOfView());
}
