// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "TimedRunSubsystem.h"

#include "Engine/World.h"
#include "Misc/CommandLine.h"
#include "Misc/Parse.h"
#include "UObject/Package.h"
#include "UObject/UObjectGlobals.h"

DEFINE_LOG_CATEGORY_STATIC(LogTimedRun, Log, All);

namespace IronAbsolution::TimedRun
{
	// The exit code of a run that did not start. The generic platform code of 5.8.3 ignores it on
	// the Mac, so there the absent success line alone fails the start command.
	constexpr uint8 FaultExitCode = 1;

	// A value longer than this cannot be in the valid range, and Atoi does not read it safely.
	constexpr int32 MaxDigits = 4;
}

bool UTimedRunSubsystem::ShouldCreateSubsystem(UObject* Outer) const
{
	// A Shipping build has no test hook. Phase 8 holds the Shipping configuration.
	return !UE_BUILD_SHIPPING && Super::ShouldCreateSubsystem(Outer);
}

void UTimedRunSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
	Super::Initialize(Collection);

	int32 Seconds = 0;
	FString Error;
	const ETimedRunOption Option = ParseOption(FCommandLine::Get(), Seconds, Error);
	if (Option == ETimedRunOption::Absent)
	{
		return;
	}

	if (Option == ETimedRunOption::Invalid)
	{
		UE_LOG(LogTimedRun, Error, TEXT("Timed run: fail. %s"), *Error);
		FPlatformMisc::RequestExitWithStatus(false, IronAbsolution::TimedRun::FaultExitCode, TEXT("UTimedRunSubsystem::Initialize"));
		return;
	}

	RunSeconds = Seconds;
	UE_LOG(LogTimedRun, Display, TEXT("Timed run: the run of %d seconds starts when the first map loads."), Seconds);
	PostLoadMapHandle = FCoreUObjectDelegates::PostLoadMapWithWorld.AddUObject(this, &UTimedRunSubsystem::HandlePostLoadMap);
}

void UTimedRunSubsystem::Deinitialize()
{
	FCoreUObjectDelegates::PostLoadMapWithWorld.Remove(PostLoadMapHandle);
	PostLoadMapHandle.Reset();
	if (TimeElapsedHandle.IsValid())
	{
		FTSTicker::RemoveTicker(TimeElapsedHandle);
		TimeElapsedHandle.Reset();
	}

	Super::Deinitialize();
}

ETimedRunOption UTimedRunSubsystem::ParseOption(const TCHAR* CommandLine, int32& OutSeconds, FString& OutError)
{
	FString Text;
	if (!FParse::Value(CommandLine, OptionKey, Text))
	{
		return ETimedRunOption::Absent;
	}

	// Each character must be a digit, so a sign, a fraction, or a unit makes the value not valid.
	bool bOnlyDigits = !Text.IsEmpty() && Text.Len() <= IronAbsolution::TimedRun::MaxDigits;
	for (const TCHAR Character : Text)
	{
		bOnlyDigits = bOnlyDigits && FChar::IsDigit(Character);
	}

	const int32 Seconds = bOnlyDigits ? FCString::Atoi(*Text) : 0;
	if (!bOnlyDigits || Seconds < 1 || Seconds > MaxSeconds)
	{
		OutError = FString::Printf(TEXT("The option %s has the value '%s'. Give a whole number of seconds from 1 to %d."), OptionKey, *Text, MaxSeconds);
		return ETimedRunOption::Invalid;
	}

	OutSeconds = Seconds;
	return ETimedRunOption::Valid;
}

FString UTimedRunSubsystem::SuccessLine(const FString& MapPackage, int32 Seconds)
{
	return FString::Printf(TEXT("Timed run: pass. The map %s ran for %d seconds."), *MapPackage, Seconds);
}

void UTimedRunSubsystem::HandlePostLoadMap(UWorld* LoadedWorld)
{
	// The first map that loads starts the run. A later map does not start it again.
	FCoreUObjectDelegates::PostLoadMapWithWorld.Remove(PostLoadMapHandle);
	PostLoadMapHandle.Reset();

	if (LoadedWorld == nullptr)
	{
		UE_LOG(LogTimedRun, Error, TEXT("Timed run: fail. The first map did not load, so the run of %d seconds did not start."), RunSeconds.GetValue());
		FPlatformMisc::RequestExitWithStatus(false, IronAbsolution::TimedRun::FaultExitCode, TEXT("UTimedRunSubsystem::HandlePostLoadMap"));
		return;
	}

	MapPackage = LoadedWorld->GetPackage()->GetName();
	UE_LOG(LogTimedRun, Display, TEXT("Timed run: the map %s loaded. The run lasts %d seconds."), *MapPackage, RunSeconds.GetValue());

	// The core ticker counts the time of the app. The time of the world can slow down or pause.
	const float Delay = static_cast<float>(RunSeconds.GetValue());
	TimeElapsedHandle = FTSTicker::GetCoreTicker().AddTicker(FTickerDelegate::CreateUObject(this, &UTimedRunSubsystem::HandleTimeElapsed), Delay);
}

bool UTimedRunSubsystem::HandleTimeElapsed(float DeltaTime)
{
	TimeElapsedHandle.Reset();
	UE_LOG(LogTimedRun, Display, TEXT("%s"), *SuccessLine(MapPackage, RunSeconds.GetValue()));
	FPlatformMisc::RequestExit(false, TEXT("UTimedRunSubsystem::HandleTimeElapsed"));

	// False removes the ticker, so the success line comes one time.
	return false;
}
