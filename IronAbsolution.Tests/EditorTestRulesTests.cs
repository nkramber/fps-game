using System;
using System.Collections.Generic;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.EditorTest;
using IronAbsolution.Tools.ToolchainCheck;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The pass rule of the headless test run against fixture reports and logs (D-71). An exit code
/// of 0 alone is not a pass, and an absent report fails with its path (exit test 6 of PR-9, T-2).
/// </summary>
public sealed class EditorTestRulesTests
{
    private const string ReportPath = "/checkout/Game/Saved/Automation/editor-test/index.json";
    private const string LogPath = "/checkout/Game/Saved/Logs/editor-test.log";
    private const string PassedLog = "LogAutomationCommandLine: Display: **** TEST COMPLETE. EXIT CODE: 0 ****\n";
    private const string FailedLog = "LogAutomationCommandLine: Display: **** TEST COMPLETE. EXIT CODE: -1 ****\n";

    private static readonly TimeSpan Limit = TimeSpan.FromMinutes(30);

    [Fact]
    public void EachCheckPassesOnACleanRun()
    {
        IReadOnlyList<CheckResult> results = EditorTestRules.Evaluate(Facts(0, Report(1, 0, ("IronAbsolution.Project.Settings", "Success")), PassedLog));

        Assert.Equal(["Editor exit", "Test report", "Success line"], [results[0].Check, results[1].Check, results[2].Check]);
        Assert.All(results, result => Assert.True(result.Holds, result.Line()));
        Assert.Equal("Editor exit: pass. The editor gave the exit code 0.", results[0].Line());
        Assert.Equal($"Test report: pass. The report '{ReportPath}' shows 1 passed tests: IronAbsolution.Project.Settings.", results[1].Line());
    }

    [Fact]
    public void AnExitCodeOfZeroWithNoSuccessLineFails()
    {
        // The false pass of a headless run that ends with 0 and writes no success line.
        IReadOnlyList<CheckResult> results = EditorTestRules.Evaluate(Facts(0, Report(1, 0, ("IronAbsolution.Project.Settings", "Success")), "LogInit: Display: Engine is initialized.\n"));

        Assert.True(results[0].Holds);
        Assert.False(results[2].Holds);
        Assert.Equal(
            $"Success line: fail. The log '{LogPath}' has no line '**** TEST COMPLETE. EXIT CODE: 0 ****'. An exit code of 0 alone is not a pass.",
            results[2].Line());
    }

    [Fact]
    public void AnAbsentReportFailsWithItsPath()
    {
        EditorTestFacts facts = new EditorTestFacts(
            new FileRunResult(0, TimedOut: false),
            Limit,
            ToolOutput.Absent(ReportPath, $"no file '{ReportPath}'"),
            ToolOutput.Found(LogPath, PassedLog));

        CheckResult report = EditorTestRules.Evaluate(facts)[1];

        Assert.False(report.Holds);
        Assert.Equal($"Test report: fail. No test report: no file '{ReportPath}'.", report.Line());
    }

    [Fact]
    public void AnAbsentLogFailsWithItsPath()
    {
        EditorTestFacts facts = new EditorTestFacts(
            new FileRunResult(0, TimedOut: false),
            Limit,
            ToolOutput.Found(ReportPath, Report(1, 0, ("IronAbsolution.Project.Settings", "Success"))),
            ToolOutput.Absent(LogPath, $"no file '{LogPath}'"));

        CheckResult log = EditorTestRules.Evaluate(facts)[2];

        Assert.False(log.Holds);
        Assert.Equal($"Success line: fail. No log: no file '{LogPath}'.", log.Line());
    }

    [Fact]
    public void AFailedTestFailsEachCheckAndNamesTheTest()
    {
        // Exit test 5 of PR-9: a test that fails on purpose gives a nonzero exit code.
        IReadOnlyList<CheckResult> results = EditorTestRules.Evaluate(Facts(
            255,
            Report(1, 1, ("IronAbsolution.Project.Settings", "Success"), ("IronAbsolution.Project.FailOnPurpose", "Fail")),
            FailedLog));

        Assert.All(results, result => Assert.False(result.Holds, result.Line()));
        Assert.Equal("Editor exit: fail. The editor gave the exit code 255.", results[0].Line());
        Assert.Contains("'failed' is 1", results[1].Detail, StringComparison.Ordinal);
        Assert.Contains("these tests did not pass: IronAbsolution.Project.FailOnPurpose (Fail)", results[1].Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AReportWithNoTestFails()
    {
        CheckResult report = EditorTestRules.Evaluate(Facts(0, Report(0, 0), PassedLog))[1];

        Assert.False(report.Holds);
        Assert.Contains("it names no test, so the test filter matched none", report.Detail, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("notRun")]
    [InlineData("inProcess")]
    public void ATestThatDidNotFinishFailsTheReport(string count)
    {
        string text = Report(1, 0, ("IronAbsolution.Project.Settings", "Success")).Replace($"\"{count}\": 0", $"\"{count}\": 1", StringComparison.Ordinal);

        CheckResult report = EditorTestRules.Evaluate(Facts(0, text, PassedLog))[1];

        Assert.False(report.Holds);
        Assert.Contains($"'{count}' is 1", report.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ATimeLimitStopFailsTheExitCheck()
    {
        EditorTestFacts facts = new EditorTestFacts(
            new FileRunResult(-1, TimedOut: true),
            Limit,
            ToolOutput.Absent(ReportPath, $"no file '{ReportPath}'"),
            ToolOutput.Found(LogPath, "LogInit: Display: Engine is initialized.\n"));

        CheckResult exit = EditorTestRules.Evaluate(facts)[0];

        Assert.Equal("Editor exit: fail. The time limit of 30 minutes stopped the editor.", exit.Line());
    }

    [Theory]
    [InlineData("not json", "is not JSON")]
    [InlineData("[]", "is not a JSON object")]
    [InlineData("""{ "succeeded": 1, "succeededWithWarnings": 0, "failed": 0, "notRun": 0, "inProcess": 0 }""", "has no array in the field 'tests'")]
    [InlineData("""{ "succeeded": "1", "succeededWithWarnings": 0, "failed": 0, "notRun": 0, "inProcess": 0, "tests": [] }""", "has no whole number in the field 'succeeded'")]
    [InlineData("""{ "succeeded": 1, "succeededWithWarnings": 0, "notRun": 0, "inProcess": 0, "tests": [] }""", "has no whole number in the field 'failed'")]
    [InlineData("""{ "succeeded": 1, "succeededWithWarnings": 0, "failed": 0, "notRun": 0, "inProcess": 0, "tests": [ { "state": "Success" } ] }""", "has a test with no text in the field 'fullTestPath'")]
    [InlineData("""{ "succeeded": 1, "succeededWithWarnings": 0, "failed": 0, "notRun": 0, "inProcess": 0, "tests": [ { "fullTestPath": "A.B", "state": "" } ] }""", "has a test with no text in the field 'state'")]
    public void AnInvalidReportFailsWithThePathAndTheField(string text, string fault)
    {
        CheckResult report = EditorTestRules.Evaluate(Facts(0, text, PassedLog))[1];

        Assert.False(report.Holds);
        Assert.StartsWith($"The report '{ReportPath}' {fault}", report.Detail, StringComparison.Ordinal);
    }

    private static EditorTestFacts Facts(int exitCode, string report, string log)
    {
        return new EditorTestFacts(
            new FileRunResult(exitCode, TimedOut: false),
            Limit,
            ToolOutput.Found(ReportPath, report),
            ToolOutput.Found(LogPath, log));
    }

    /// <summary>Writes a report in the form of the automation controller of 5.8.3.</summary>
    private static string Report(int succeeded, int failed, params (string Path, string State)[] tests)
    {
        List<string> entries = [];
        foreach ((string path, string state) in tests)
        {
            entries.Add($$"""{ "testDisplayName": "{{path}}", "fullTestPath": "{{path}}", "state": "{{state}}", "warnings": 0, "errors": 0 }""");
        }

        return $$"""
            {
                "devices": [],
                "succeeded": {{succeeded}},
                "succeededWithWarnings": 0,
                "failed": {{failed}},
                "notRun": 0,
                "inProcess": 0,
                "totalDuration": 0.1,
                "tests": [ {{string.Join(", ", entries)}} ]
            }
            """;
    }
}
