// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "FrameTimeCaptureSubsystem.h"

#include "Engine/Engine.h"
#include "Engine/GameViewportClient.h"
#include "Engine/World.h"
#include "EngineUtils.h"
#include "FrameTimeView.h"
#include "GameFramework/PlayerController.h"
#include "HAL/IConsoleManager.h"
#include "Misc/CommandLine.h"
#include "Misc/Parse.h"
#include "Misc/Paths.h"
#include "ProfilingDebugging/CsvProfiler.h"
#include "UnrealClient.h"
#include "UObject/Package.h"
#include "UObject/UObjectGlobals.h"

DEFINE_LOG_CATEGORY_STATIC(LogFrameTimeCapture, Log, All);

namespace IronAbsolution::FrameTimeCapture
{
	// The exit code of a capture that failed. The start command also fails on the absent success
	// line, so the exit code is not the one proof of a fault.
	constexpr uint8 FaultExitCode = 1;

	// The time between two checks of the CSV file after the capture ends. The CSV profiler writes
	// the file on its own thread.
	constexpr float FileCheckSeconds = 0.1f;
}

bool UFrameTimeCaptureSubsystem::ShouldCreateSubsystem(UObject* Outer) const
{
	// A Shipping build has no CSV profiler and no test hook (D-137).
	return !UE_BUILD_SHIPPING && Super::ShouldCreateSubsystem(Outer);
}

void UFrameTimeCaptureSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
	Super::Initialize(Collection);

	FString Path;
	FString Error;
	const EFrameTimeCaptureOption Option = ParseOption(FCommandLine::Get(), Path, Error);
	if (Option == EFrameTimeCaptureOption::Absent)
	{
		return;
	}

	if (Option == EFrameTimeCaptureOption::Invalid)
	{
		Fail(Error);
		return;
	}

#if CSV_PROFILER
	CsvPath = Path;
	UE_LOG(LogFrameTimeCapture, Display, TEXT("Frame-time capture: the capture to %s starts when the first map loads."), *Path);
	PostLoadMapHandle = FCoreUObjectDelegates::PostLoadMapWithWorld.AddUObject(this, &UFrameTimeCaptureSubsystem::HandlePostLoadMap);
#else
	Fail(FString::Printf(TEXT("This build has no CSV profiler, so it cannot write %s. Use a Development package (D-137)."), *Path));
#endif
}

void UFrameTimeCaptureSubsystem::Deinitialize()
{
	FCoreUObjectDelegates::PostLoadMapWithWorld.Remove(PostLoadMapHandle);
	PostLoadMapHandle.Reset();
	if (TickerHandle.IsValid())
	{
		FTSTicker::RemoveTicker(TickerHandle);
		TickerHandle.Reset();
	}

	Super::Deinitialize();
}

EFrameTimeCaptureOption UFrameTimeCaptureSubsystem::ParseOption(const TCHAR* CommandLine, FString& OutPath, FString& OutError)
{
	FString Text;
	if (!FParse::Value(CommandLine, OptionKey, Text))
	{
		return EFrameTimeCaptureOption::Absent;
	}

	// The package runs from its own folder, so a relative path names a file that the start command
	// does not read. The CSV profiler gives each file the extension .csv.
	const bool bFullPath = !Text.IsEmpty() && !FPaths::IsRelative(Text);
	const bool bCsvFile = FPaths::GetExtension(Text).Equals(TEXT("csv"), ESearchCase::IgnoreCase) && !FPaths::GetBaseFilename(Text).IsEmpty();
	if (!bFullPath || !bCsvFile)
	{
		OutError = FString::Printf(TEXT("The option %s has the value '%s'. Give the full path of a file that ends with .csv."), OptionKey, *Text);
		return EFrameTimeCaptureOption::Invalid;
	}

	OutPath = Text;
	return EFrameTimeCaptureOption::Valid;
}

bool UFrameTimeCaptureSubsystem::CheckViewOrders(const TArray<int32>& Orders, FString& OutError)
{
	if (Orders.IsEmpty())
	{
		OutError = TEXT("The map has no AFrameTimeView. The content script places each view of the gym (D-134).");
		return false;
	}

	TSet<int32> Seen;
	bool bValid = true;
	for (const int32 Order : Orders)
	{
		bool bAlreadySeen = false;
		Seen.Add(Order, &bAlreadySeen);
		bValid = bValid && Order >= 1 && !bAlreadySeen;
	}

	if (!bValid)
	{
		TArray<int32> Sorted = Orders;
		Sorted.Sort();
		const FString Values = FString::JoinBy(Sorted, TEXT(", "), [](int32 Order) { return FString::FromInt(Order); });
		OutError = FString::Printf(TEXT("The views have the Order values %s. Each value must be 1 or more, and no value can come two times."), *Values);
		return false;
	}

	return true;
}

FString UFrameTimeCaptureSubsystem::WindowModeName(EWindowMode::Type Mode)
{
	switch (Mode)
	{
	case EWindowMode::Fullscreen:
		return TEXT("Fullscreen");
	case EWindowMode::WindowedFullscreen:
		return TEXT("WindowedFullscreen");
	case EWindowMode::Windowed:
		return TEXT("Windowed");
	default:
		return FString::Printf(TEXT("Unknown(%d)"), static_cast<int32>(Mode));
	}
}

FString UFrameTimeCaptureSubsystem::SuccessLine(const FString& MapPackage, const FString& CsvPath)
{
	return FString::Printf(TEXT("Frame-time capture: pass. The map %s wrote the file %s."), *MapPackage, *CsvPath);
}

void UFrameTimeCaptureSubsystem::HandlePostLoadMap(UWorld* LoadedWorld)
{
	// The first map that loads starts the capture. A later map does not start it again.
	FCoreUObjectDelegates::PostLoadMapWithWorld.Remove(PostLoadMapHandle);
	PostLoadMapHandle.Reset();

	if (LoadedWorld == nullptr)
	{
		Fail(TEXT("The first map did not load, so the capture did not start."));
		return;
	}

	MapPackage = LoadedWorld->GetPackage()->GetName();
	FString Error;
	if (!FindViews(*LoadedWorld, Error))
	{
		Fail(FString::Printf(TEXT("The map %s: %s"), *MapPackage, *Error));
		return;
	}

	// LoadMap spawns the player before it sends this event, so the controller exists now.
	Controller = LoadedWorld->GetFirstPlayerController();
	if (!Controller.IsValid())
	{
		Fail(FString::Printf(TEXT("The map %s has no player controller, so no view can show."), *MapPackage));
		return;
	}

	ViewIndex = 0;
	if (!TurnOffFrameCaps(Error) || !ShowView(Error))
	{
		Fail(Error);
		return;
	}

	UE_LOG(LogFrameTimeCapture, Display, TEXT("Frame-time capture: the map %s loaded with %d views. The capture starts after %.0f seconds."), *MapPackage, Views.Num(), WarmupSeconds);
	TickerHandle = FTSTicker::GetCoreTicker().AddTicker(FTickerDelegate::CreateUObject(this, &UFrameTimeCaptureSubsystem::HandleWarmupElapsed), WarmupSeconds);
}

bool UFrameTimeCaptureSubsystem::HandleWarmupElapsed(float DeltaTime)
{
	TickerHandle.Reset();
#if CSV_PROFILER
	if (FCsvProfiler::IsCapturing())
	{
		Fail(TEXT("Another capture of the CSV profiler runs. Remove each -csv option from the command line."));
		return false;
	}

	FString Error;
	if (!WriteMetadata(Error))
	{
		Fail(Error);
		return false;
	}

	// The profiler starts the capture at the start of the next frame, and adds .csv to the name.
	const FString& Path = CsvPath.GetValue();
	FCsvProfiler::Get()->BeginCapture(-1, FPaths::GetPath(Path), FPaths::GetBaseFilename(Path));
	UE_LOG(LogFrameTimeCapture, Display, TEXT("Frame-time capture: the capture starts. It holds each view for %.0f seconds."), ViewSeconds);
	TickerHandle = FTSTicker::GetCoreTicker().AddTicker(FTickerDelegate::CreateUObject(this, &UFrameTimeCaptureSubsystem::HandleViewElapsed), ViewSeconds);
#endif
	// False removes this ticker, so the warm-up ends one time.
	return false;
}

bool UFrameTimeCaptureSubsystem::HandleViewElapsed(float DeltaTime)
{
	++ViewIndex;
	FString Error;
	if (ViewIndex < Views.Num())
	{
		if (!ShowView(Error))
		{
			TickerHandle.Reset();
			Fail(Error);
			return false;
		}

		// True keeps this ticker, so the next view comes after the same time.
		return true;
	}

	TickerHandle.Reset();
#if CSV_PROFILER
	if (!FCsvProfiler::IsCapturing())
	{
		Fail(FString::Printf(TEXT("The CSV profiler did not start the capture to %s. Read the LogCsvProfiler lines of the log."), *CsvPath.GetValue()));
		return false;
	}

	WrittenFile = FCsvProfiler::Get()->EndCapture();
	if (!WrittenFile->IsValid())
	{
		Fail(FString::Printf(TEXT("The CSV profiler did not end the capture to %s. Read the LogCsvProfiler lines of the log."), *CsvPath.GetValue()));
		return false;
	}

	TickerHandle = FTSTicker::GetCoreTicker().AddTicker(FTickerDelegate::CreateUObject(this, &UFrameTimeCaptureSubsystem::HandleFileCheck), IronAbsolution::FrameTimeCapture::FileCheckSeconds);
#endif
	return false;
}

bool UFrameTimeCaptureSubsystem::HandleFileCheck(float DeltaTime)
{
	if (!WrittenFile->IsReady())
	{
		// True keeps this ticker until the profiler wrote the whole file.
		return true;
	}

	TickerHandle.Reset();
	const FString& Written = WrittenFile->Get();
	const FString& Path = CsvPath.GetValue();
	if (Written.IsEmpty() || !FPaths::IsSamePath(Written, Path))
	{
		Fail(FString::Printf(TEXT("The CSV profiler wrote the file '%s', not %s."), *Written, *Path));
		return false;
	}

	UE_LOG(LogFrameTimeCapture, Display, TEXT("%s"), *SuccessLine(MapPackage, Path));
	FPlatformMisc::RequestExit(false, TEXT("UFrameTimeCaptureSubsystem::HandleFileCheck"));
	return false;
}

bool UFrameTimeCaptureSubsystem::FindViews(UWorld& World, FString& OutError)
{
	TArray<AFrameTimeView*> Found;
	TArray<int32> Orders;
	for (TActorIterator<AFrameTimeView> It(&World); It; ++It)
	{
		Found.Add(*It);
		Orders.Add(It->Order);
	}

	if (!CheckViewOrders(Orders, OutError))
	{
		return false;
	}

	Found.Sort([](const AFrameTimeView& A, const AFrameTimeView& B) { return A.Order < B.Order; });
	Views.Reset();
	for (AFrameTimeView* View : Found)
	{
		Views.Add(View);
	}

	return true;
}

bool UFrameTimeCaptureSubsystem::TurnOffFrameCaps(FString& OutError)
{
	IConsoleVariable* VSync = IConsoleManager::Get().FindConsoleVariable(TEXT("r.VSync"));
	if (GEngine == nullptr || VSync == nullptr)
	{
		OutError = TEXT("The engine or the console variable r.VSync is absent, so the capture cannot turn off the frame caps.");
		return false;
	}

	// D-137: the frame time is the true cost of a frame. VSync rounds each frame up to the refresh
	// of the display, and a cap or the smooth frame rate adds a wait.
	GEngine->bSmoothFrameRate = false;
	GEngine->SetMaxFPS(0.0f);
	VSync->Set(0, ECVF_SetByCode);
	return true;
}

bool UFrameTimeCaptureSubsystem::WriteMetadata(FString& OutError) const
{
#if CSV_PROFILER
	const UGameViewportClient* Client = GEngine != nullptr ? GEngine->GameViewport.Get() : nullptr;
	FViewport* Viewport = Client != nullptr ? Client->Viewport : nullptr;
	const IConsoleVariable* VSync = IConsoleManager::Get().FindConsoleVariable(TEXT("r.VSync"));
	if (Viewport == nullptr || VSync == nullptr)
	{
		OutError = TEXT("The game has no viewport or no console variable r.VSync, so the capture cannot record its settings.");
		return false;
	}

	// The start command checks each value against D-137, so a capture with other settings fails.
	const FIntPoint Size = Viewport->GetSizeXY();
	const FString WindowMode = WindowModeName(Viewport->GetWindowMode());
	// %g writes 0 as "0" and 120 as "120", the form that the start command compares.
	const FString MaxFps = FString::Printf(TEXT("%g"), GEngine->GetMaxFPS());
	FCsvProfiler::SetMetadata(WindowModeKey, *WindowMode);
	FCsvProfiler::SetMetadata(ViewportWidthKey, *FString::FromInt(Size.X));
	FCsvProfiler::SetMetadata(ViewportHeightKey, *FString::FromInt(Size.Y));
	FCsvProfiler::SetMetadata(VSyncKey, *FString::FromInt(VSync->GetInt()));
	FCsvProfiler::SetMetadata(MaxFpsKey, *MaxFps);
	FCsvProfiler::SetMetadata(ViewCountKey, *FString::FromInt(Views.Num()));
	UE_LOG(LogFrameTimeCapture, Display, TEXT("Frame-time capture: the window mode is %s at %dx%d. r.VSync is %d, and the frame-rate cap is %s."), *WindowMode, Size.X, Size.Y, VSync->GetInt(), *MaxFps);
#endif
	return true;
}

bool UFrameTimeCaptureSubsystem::ShowView(FString& OutError)
{
	AFrameTimeView* View = Views[ViewIndex].Get();
	APlayerController* Player = Controller.Get();
	if (View == nullptr || Player == nullptr)
	{
		OutError = FString::Printf(TEXT("View %d of %d or the player controller went away during the capture."), ViewIndex + 1, Views.Num());
		return false;
	}

	// No blend time, so each frame of the capture shows one of the views.
	Player->SetViewTarget(View);
	UE_LOG(LogFrameTimeCapture, Display, TEXT("Frame-time capture: view %d of %d, with the Order %d."), ViewIndex + 1, Views.Num(), View->Order);
	return true;
}

void UFrameTimeCaptureSubsystem::Fail(const FString& Reason)
{
	// The engine can write a partial CSV file when it stops. The start command fails on the exit
	// code and on the absent success line, so a partial file does not pass.
	UE_LOG(LogFrameTimeCapture, Error, TEXT("Frame-time capture: fail. %s"), *Reason);
	FPlatformMisc::RequestExitWithStatus(false, IronAbsolution::FrameTimeCapture::FaultExitCode, TEXT("UFrameTimeCaptureSubsystem::Fail"));
}
