// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Engine/Level.h"
#include "Engine/World.h"
#include "FrameTimeCaptureSubsystem.h"
#include "FrameTimeView.h"
#include "Misc/AutomationTest.h"

#if WITH_DEV_AUTOMATION_TESTS

// The option of the frame-time capture (D-137). The start command of the capture gives the option,
// and a value that is not valid must stop the game with the value in the error (T-2).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionFrameTimeCaptureOptionTest,
	"IronAbsolution.FrameTimeCapture.Option",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionFrameTimeCaptureOptionTest::RunTest(const FString& Parameters)
{
	const FString Unchanged = TEXT("unchanged");
	FString Path = Unchanged;
	FString Error;
	TestTrue(TEXT("A command line with no option gives Absent"), UFrameTimeCaptureSubsystem::ParseOption(TEXT("-unattended -TimedRunSeconds=10"), Path, Error) == EFrameTimeCaptureOption::Absent);
	TestEqual(TEXT("Absent does not change the path"), Path, Unchanged);
	TestTrue(TEXT("Absent does not change the error"), Error.IsEmpty());

	struct FValidCase
	{
		const TCHAR* CommandLine;
		const TCHAR* Path;
	};
	const FValidCase ValidCases[] = {
		{TEXT("-FrameTimeCsv=C:/dev/iron-absolution/Game/Saved/Logs/frame-capture.csv"), TEXT("C:/dev/iron-absolution/Game/Saved/Logs/frame-capture.csv")},
		{TEXT("/Game/Maps/L_Gym -FrameTimeCsv=D:\\Captures\\gym.CSV -unattended"), TEXT("D:\\Captures\\gym.CSV")},
		{TEXT("-FrameTimeCsv=\"C:/Folder With Spaces/capture.csv\""), TEXT("C:/Folder With Spaces/capture.csv")},
	};
	for (const FValidCase& Case : ValidCases)
	{
		Path = Unchanged;
		Error.Reset();
		TestTrue(FString::Printf(TEXT("'%s' gives Valid"), Case.CommandLine), UFrameTimeCaptureSubsystem::ParseOption(Case.CommandLine, Path, Error) == EFrameTimeCaptureOption::Valid);
		TestEqual(FString::Printf(TEXT("The path of '%s'"), Case.CommandLine), Path, FString(Case.Path));
		TestTrue(FString::Printf(TEXT("'%s' gives no error"), Case.CommandLine), Error.IsEmpty());
	}

	// A relative path, a file of another type, a path with no file name, and an empty value.
	const TCHAR* const InvalidValues[] = {TEXT("capture.csv"), TEXT("Saved/capture.csv"), TEXT("C:/capture.txt"), TEXT("C:/capture"), TEXT("C:/Captures/.csv"), TEXT("")};
	for (const TCHAR* Value : InvalidValues)
	{
		Path = Unchanged;
		Error.Reset();
		const FString CommandLine = FString::Printf(TEXT("-unattended -FrameTimeCsv=%s"), Value);
		TestTrue(FString::Printf(TEXT("'%s' gives Invalid"), *CommandLine), UFrameTimeCaptureSubsystem::ParseOption(*CommandLine, Path, Error) == EFrameTimeCaptureOption::Invalid);
		TestEqual(FString::Printf(TEXT("Invalid does not change the path of '%s'"), *CommandLine), Path, Unchanged);
		const FString Expected = FString::Printf(TEXT("The option -FrameTimeCsv= has the value '%s'. Give the full path of a file that ends with .csv."), Value);
		TestEqual(FString::Printf(TEXT("The error of '%s'"), *CommandLine), Error, Expected);
	}

	return !HasAnyErrors();
}

// The views of a map. A map with no view, a view with no Order, or two views with one Order must
// stop the capture with each value in the error (T-2).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionFrameTimeCaptureViewOrderTest,
	"IronAbsolution.FrameTimeCapture.ViewOrder",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionFrameTimeCaptureViewOrderTest::RunTest(const FString& Parameters)
{
	FString Error;
	TestTrue(TEXT("Views 1 to 4 in any sequence pass"), UFrameTimeCaptureSubsystem::CheckViewOrders({3, 1, 4, 2}, Error));
	TestTrue(TEXT("A pass gives no error"), Error.IsEmpty());
	TestTrue(TEXT("Views with gaps between the values pass"), UFrameTimeCaptureSubsystem::CheckViewOrders({10, 20}, Error));

	TestFalse(TEXT("No view fails"), UFrameTimeCaptureSubsystem::CheckViewOrders({}, Error));
	TestEqual(TEXT("The error of no view"), Error, FString(TEXT("The map has no AFrameTimeView. The content script places each view of the gym (D-134).")));

	Error.Reset();
	TestFalse(TEXT("Two views with one value fail"), UFrameTimeCaptureSubsystem::CheckViewOrders({2, 1, 2}, Error));
	TestEqual(TEXT("The error of two views with one value"), Error, FString(TEXT("The views have the Order values 1, 2, 2. Each value must be 1 or more, and no value can come two times.")));

	Error.Reset();
	TestFalse(TEXT("A view with the default value 0 fails"), UFrameTimeCaptureSubsystem::CheckViewOrders({1, 0}, Error));
	TestEqual(TEXT("The error of a view with the value 0"), Error, FString(TEXT("The views have the Order values 0, 1. Each value must be 1 or more, and no value can come two times.")));

	return !HasAnyErrors();
}

// The success line and the names of the window modes. `FrameCaptureRules.cs` looks for this exact
// text, and a hosted test compares its copy with the format of these lines.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionFrameTimeCaptureTextTest,
	"IronAbsolution.FrameTimeCapture.Text",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionFrameTimeCaptureTextTest::RunTest(const FString& Parameters)
{
	TestEqual(
		TEXT("The success line of the gym"),
		UFrameTimeCaptureSubsystem::SuccessLine(TEXT("/Game/Maps/L_Gym"), TEXT("C:/x/frame-capture.csv")),
		FString(TEXT("Frame-time capture: pass. The map /Game/Maps/L_Gym wrote the file C:/x/frame-capture.csv.")));
	TestEqual(TEXT("The name of borderless fullscreen (D-138)"), UFrameTimeCaptureSubsystem::WindowModeName(EWindowMode::WindowedFullscreen), FString(TEXT("WindowedFullscreen")));
	TestEqual(TEXT("The name of fullscreen"), UFrameTimeCaptureSubsystem::WindowModeName(EWindowMode::Fullscreen), FString(TEXT("Fullscreen")));
	TestEqual(TEXT("The name of a window"), UFrameTimeCaptureSubsystem::WindowModeName(EWindowMode::Windowed), FString(TEXT("Windowed")));
	return !HasAnyErrors();
}

// The gym holds the views of the capture, from the content script (D-134, D-137).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionFrameTimeCaptureGymViewsTest,
	"IronAbsolution.FrameTimeCapture.GymViews",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionFrameTimeCaptureGymViewsTest::RunTest(const FString& Parameters)
{
	const TCHAR* const GymMap = TEXT("/Game/Maps/L_Gym.L_Gym");
	const UWorld* Gym = LoadObject<UWorld>(nullptr, GymMap);
	if (!TestNotNull(FString::Printf(TEXT("The map %s"), GymMap), Gym))
	{
		return false;
	}

	TArray<int32> Orders;
	for (const AActor* Actor : Gym->PersistentLevel->Actors)
	{
		if (const AFrameTimeView* View = Cast<AFrameTimeView>(Actor))
		{
			Orders.Add(View->Order);
		}
	}

	// The check runs first, so the message of the test holds its error.
	FString Error;
	const bool bViewsValid = UFrameTimeCaptureSubsystem::CheckViewOrders(Orders, Error);
	TestTrue(FString::Printf(TEXT("The views of the gym can make a capture. %s"), *Error), bViewsValid);
	Orders.Sort();
	const FString Values = FString::JoinBy(Orders, TEXT(", "), [](int32 Order) { return FString::FromInt(Order); });
	TestEqual(TEXT("The Order values of the views of the gym"), Values, FString(TEXT("1, 2, 3, 4")));
	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
