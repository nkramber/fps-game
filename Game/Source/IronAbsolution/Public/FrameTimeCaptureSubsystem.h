// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "Containers/Ticker.h"
#include "GenericPlatform/GenericWindow.h"
#include "Subsystems/GameInstanceSubsystem.h"

#include "FrameTimeCaptureSubsystem.generated.h"

class AFrameTimeView;
class APlayerController;
class UWorld;

/** The state of the frame-time option on a command line. */
enum class EFrameTimeCaptureOption : uint8
{
	/** The command line has no option, so the game makes no capture. */
	Absent,
	/** The option has the full path of a CSV file. */
	Valid,
	/** The option has a value that is not valid. */
	Invalid,
};

/**
 * The frame-time capture of M-3 (D-137). With -FrameTimeCsv=<full path> on the command line, the
 * game waits for the first map to load, and turns off VSync and each frame-rate cap. It shows each
 * AFrameTimeView of the map in order, and the CSV profiler of the engine records the time of each
 * frame. The game then writes the success line in its log, and stops.
 *
 * `run.ps1 frame-capture` starts the package with the option, and reads the CSV file and the log.
 * Each fault writes an error line and stops the game with a nonzero exit code (T-2).
 */
UCLASS()
class IRONABSOLUTION_API UFrameTimeCaptureSubsystem : public UGameInstanceSubsystem
{
	GENERATED_BODY()

public:
	/** The key of the option on the command line, with its equals sign. */
	static constexpr const TCHAR* OptionKey = TEXT("-FrameTimeCsv=");

	/** The time after the map loads and before the capture starts. The first frames load shaders and textures. */
	static constexpr float WarmupSeconds = 5.0f;

	/** The time that the capture holds each view. */
	static constexpr float ViewSeconds = 5.0f;

	/** The metadata key of the window mode. The CSV profiler writes each key in lower case. */
	static constexpr const TCHAR* WindowModeKey = TEXT("IronAbsolution.WindowMode");

	/** The metadata key of the width of the game viewport, in pixels. */
	static constexpr const TCHAR* ViewportWidthKey = TEXT("IronAbsolution.ViewportWidth");

	/** The metadata key of the height of the game viewport, in pixels. */
	static constexpr const TCHAR* ViewportHeightKey = TEXT("IronAbsolution.ViewportHeight");

	/** The metadata key of the value of r.VSync during the capture. */
	static constexpr const TCHAR* VSyncKey = TEXT("IronAbsolution.VSync");

	/** The metadata key of the frame-rate cap of the engine during the capture. 0 is no cap. */
	static constexpr const TCHAR* MaxFpsKey = TEXT("IronAbsolution.MaxFps");

	/** The metadata key of the number of views that the capture showed. */
	static constexpr const TCHAR* ViewCountKey = TEXT("IronAbsolution.ViewCount");

	virtual bool ShouldCreateSubsystem(UObject* Outer) const override;
	virtual void Initialize(FSubsystemCollectionBase& Collection) override;
	virtual void Deinitialize() override;

	/**
	 * Reads the frame-time option from a command line.
	 * @param CommandLine The whole command line.
	 * @param OutPath Takes the full path of the CSV file when the result is Valid. Otherwise it does not change.
	 * @param OutError Takes the reason, with the text of the value, when the result is Invalid. Otherwise it does not change.
	 * @return The state of the option.
	 */
	static EFrameTimeCaptureOption ParseOption(const TCHAR* CommandLine, FString& OutPath, FString& OutError);

	/**
	 * Checks the Order values of the views of a map. The capture needs at least one view, each
	 * value 1 or more, and no value two times.
	 * @param Orders The Order value of each view, in any sequence.
	 * @param OutError Takes the reason, with each value that is wrong, when the result is false. Otherwise it does not change.
	 * @return True when the capture can show the views.
	 */
	static bool CheckViewOrders(const TArray<int32>& Orders, FString& OutError);

	/**
	 * Gives the name of a window mode, as the capture writes it in the metadata.
	 * @param Mode The window mode of the game viewport.
	 * @return The name, such as "WindowedFullscreen".
	 */
	static FString WindowModeName(EWindowMode::Type Mode);

	/**
	 * Gives the success line of a capture. `run.ps1 frame-capture` looks for this exact text.
	 * @param MapPackage The package name of the map of the capture, such as /Game/Maps/L_Gym.
	 * @param CsvPath The full path of the CSV file.
	 * @return The line, such as "Frame-time capture: pass. The map /Game/Maps/L_Gym wrote the file C:/x.csv."
	 */
	static FString SuccessLine(const FString& MapPackage, const FString& CsvPath);

private:
	void HandlePostLoadMap(UWorld* LoadedWorld);
	bool HandleWarmupElapsed(float DeltaTime);
	bool HandleViewElapsed(float DeltaTime);
	bool HandleFileCheck(float DeltaTime);

	/** Finds each view of the world, and puts them in the order of Order. False when a view is wrong. */
	bool FindViews(UWorld& World, FString& OutError);

	/** Turns off VSync, the smooth frame rate, and the frame-rate cap. False when a setting is absent. */
	static bool TurnOffFrameCaps(FString& OutError);

	/** Writes the settings of the capture in the metadata of the CSV file. False when the game has no viewport. */
	bool WriteMetadata(FString& OutError) const;

	/** Makes the view at ViewIndex the view of the player. False when the view or the controller went away. */
	bool ShowView(FString& OutError);

	/** Writes the error line, and stops the game with a nonzero exit code. */
	void Fail(const FString& Reason);

	/** The full path of the CSV file. It has a value only when the command line has a valid option. */
	TOptional<FString> CsvPath;

	/** The package name of the first map that loaded. */
	FString MapPackage;

	/** Each view of the map, in the order of Order. */
	TArray<TWeakObjectPtr<AFrameTimeView>> Views;

	/** The index in Views of the view that the capture shows now. */
	int32 ViewIndex = INDEX_NONE;

	/** The controller of the player. The capture sets its view target. */
	TWeakObjectPtr<APlayerController> Controller;

	/** The file that the CSV profiler wrote. It has a value after the capture ends. */
	TOptional<TSharedFuture<FString>> WrittenFile;

	FDelegateHandle PostLoadMapHandle;
	FTSTicker::FDelegateHandle TickerHandle;
};
