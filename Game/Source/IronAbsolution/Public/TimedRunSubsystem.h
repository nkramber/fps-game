// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "Containers/Ticker.h"
#include "Subsystems/GameInstanceSubsystem.h"

#include "TimedRunSubsystem.generated.h"

class UWorld;

/** The state of the timed-run option on a command line. */
enum class ETimedRunOption : uint8
{
	/** The command line has no option, so the game runs with no set time. */
	Absent,
	/** The option has a whole number of seconds in the valid range. */
	Valid,
	/** The option has a value that is not valid. */
	Invalid,
};

/**
 * The timed run of a package (D-89). With -TimedRunSeconds=<n> on the command line, the game waits
 * for the first map to load, runs it for n seconds, writes the success line in its log, and stops.
 * The start command of the package, `make package-run` or `scripts/package-run.ps1`, reads that line.
 * A value that is not valid writes an error line and stops the game before the map runs (T-2).
 */
UCLASS()
class IRONABSOLUTION_API UTimedRunSubsystem : public UGameInstanceSubsystem
{
	GENERATED_BODY()

public:
	/** The key of the option on the command line, with its equals sign. */
	static constexpr const TCHAR* OptionKey = TEXT("-TimedRunSeconds=");

	/** The largest valid value of the option: one hour. */
	static constexpr int32 MaxSeconds = 3600;

	virtual bool ShouldCreateSubsystem(UObject* Outer) const override;
	virtual void Initialize(FSubsystemCollectionBase& Collection) override;
	virtual void Deinitialize() override;

	/**
	 * Reads the timed-run option from a command line.
	 * @param CommandLine The whole command line.
	 * @param OutSeconds Takes the value when the result is Valid. Otherwise it does not change.
	 * @param OutError Takes the reason, with the text of the value, when the result is Invalid. Otherwise it does not change.
	 * @return The state of the option.
	 */
	static ETimedRunOption ParseOption(const TCHAR* CommandLine, int32& OutSeconds, FString& OutError);

	/**
	 * Gives the success line of a run. The start command of the package looks for this exact text.
	 * @param MapPackage The package name of the map that ran, such as /Game/Maps/L_Test.
	 * @param Seconds The set time of the run.
	 * @return The line, such as "Timed run: pass. The map /Game/Maps/L_Test ran for 10 seconds."
	 */
	static FString SuccessLine(const FString& MapPackage, int32 Seconds);

private:
	void HandlePostLoadMap(UWorld* LoadedWorld);
	bool HandleTimeElapsed(float DeltaTime);

	/** The set time of the run. It has a value only when the command line has a valid option. */
	TOptional<int32> RunSeconds;

	/** The package name of the first map that loaded. */
	FString MapPackage;

	FDelegateHandle PostLoadMapHandle;
	FTSTicker::FDelegateHandle TimeElapsedHandle;
};
