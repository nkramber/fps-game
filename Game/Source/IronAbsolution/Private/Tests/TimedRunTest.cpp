// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "Misc/AutomationTest.h"
#include "TimedRunSubsystem.h"

#if WITH_DEV_AUTOMATION_TESTS

// The option of the timed run (D-89). The start command of each package gives the option, and a
// value that is not valid must stop the game with the value in the error (T-2).
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionTimedRunOptionTest,
	"IronAbsolution.TimedRun.Option",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionTimedRunOptionTest::RunTest(const FString& Parameters)
{
	const int32 Unchanged = -7;
	int32 Seconds = Unchanged;
	FString Error;
	TestTrue(TEXT("A command line with no option gives Absent"), UTimedRunSubsystem::ParseOption(TEXT("-unattended -windowed"), Seconds, Error) == ETimedRunOption::Absent);
	TestEqual(TEXT("Absent does not change the seconds"), Seconds, Unchanged);
	TestTrue(TEXT("Absent does not change the error"), Error.IsEmpty());

	struct FValidCase
	{
		const TCHAR* CommandLine;
		int32 Seconds;
	};
	const FValidCase ValidCases[] = {
		{TEXT("-TimedRunSeconds=10"), 10},
		{TEXT("-unattended -TimedRunSeconds=1 -windowed"), 1},
		{TEXT("-TimedRunSeconds=3600"), 3600},
	};
	for (const FValidCase& Case : ValidCases)
	{
		Seconds = Unchanged;
		Error.Reset();
		TestTrue(FString::Printf(TEXT("'%s' gives Valid"), Case.CommandLine), UTimedRunSubsystem::ParseOption(Case.CommandLine, Seconds, Error) == ETimedRunOption::Valid);
		TestEqual(FString::Printf(TEXT("The seconds of '%s'"), Case.CommandLine), Seconds, Case.Seconds);
		TestTrue(FString::Printf(TEXT("'%s' gives no error"), Case.CommandLine), Error.IsEmpty());
	}

	const TCHAR* const InvalidValues[] = {TEXT("0"), TEXT("3601"), TEXT("-5"), TEXT("1.5"), TEXT("10s"), TEXT("abc"), TEXT("99999999999")};
	for (const TCHAR* Value : InvalidValues)
	{
		Seconds = Unchanged;
		Error.Reset();
		const FString CommandLine = FString::Printf(TEXT("-unattended -TimedRunSeconds=%s"), Value);
		TestTrue(FString::Printf(TEXT("'%s' gives Invalid"), *CommandLine), UTimedRunSubsystem::ParseOption(*CommandLine, Seconds, Error) == ETimedRunOption::Invalid);
		TestEqual(FString::Printf(TEXT("Invalid does not change the seconds of '%s'"), *CommandLine), Seconds, Unchanged);
		const FString Expected = FString::Printf(TEXT("The option -TimedRunSeconds= has the value '%s'. Give a whole number of seconds from 1 to 3600."), Value);
		TestEqual(FString::Printf(TEXT("The error of '%s'"), *CommandLine), Error, Expected);
	}

	return !HasAnyErrors();
}

// The success line of the timed run. `PackageRunRules.cs` and `scripts/package-run.ps1` look for
// this exact text, and a hosted test compares their copy with the format of this line.
IMPLEMENT_SIMPLE_AUTOMATION_TEST(
	FIronAbsolutionTimedRunSuccessLineTest,
	"IronAbsolution.TimedRun.SuccessLine",
	EAutomationTestFlags::EditorContext | EAutomationTestFlags::ProductFilter)

bool FIronAbsolutionTimedRunSuccessLineTest::RunTest(const FString& Parameters)
{
	TestEqual(
		TEXT("The success line of the test map"),
		UTimedRunSubsystem::SuccessLine(TEXT("/Game/Maps/L_Test"), 10),
		FString(TEXT("Timed run: pass. The map /Game/Maps/L_Test ran for 10 seconds.")));
	return !HasAnyErrors();
}

#endif // WITH_DEV_AUTOMATION_TESTS
