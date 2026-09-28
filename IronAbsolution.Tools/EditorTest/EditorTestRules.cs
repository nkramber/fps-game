using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.ToolchainCheck;

namespace IronAbsolution.Tools.EditorTest;

/// <summary>
/// Every fact of one headless run, read one time after the editor stopped. The rules do no I/O.
/// </summary>
/// <param name="Run">The exit code of the editor, and whether the time limit stopped it.</param>
/// <param name="Limit">The time limit of the run.</param>
/// <param name="Report">The text of the test report, or the reason that it is absent.</param>
/// <param name="Log">The text of the log of the editor, or the reason that it is absent.</param>
public sealed record EditorTestFacts(FileRunResult Run, TimeSpan Limit, ToolOutput Report, ToolOutput Log);

/// <summary>
/// The pass rule of a headless test run (D-71). A pass needs three facts: the exit code 0, a test
/// report with at least one passed test and no other test, and the success line in the log. An
/// exit code of 0 alone is not a pass, because a headless run can end with 0 and run no test.
/// `scripts/editor-test.ps1` applies the same rule on the Windows PC (D-72).
/// </summary>
public static class EditorTestRules
{
    /// <summary>
    /// The line that the automation controller of 5.8.3 writes when the run ends with no failed
    /// test (`AutomationCommandline.cpp`). A failed test gives the exit code -1 in the same line.
    /// </summary>
    public const string SuccessLine = "**** TEST COMPLETE. EXIT CODE: 0 ****";

    /// <summary>The state of a passed test in the test report.</summary>
    public const string PassedState = "Success";

    /// <summary>Applies each check to the facts.</summary>
    /// <param name="facts">The facts of one run.</param>
    /// <returns>One result for the exit code, one for the test report, and one for the log, in that order.</returns>
    public static IReadOnlyList<CheckResult> Evaluate(EditorTestFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        return [CheckExitCode(facts.Run, facts.Limit), CheckReport(facts.Report), CheckLog(facts.Log)];
    }

    private static CheckResult CheckExitCode(FileRunResult run, TimeSpan limit)
    {
        const string check = "Editor exit";
        if (run.TimedOut)
        {
            return new CheckResult(check, false, $"The time limit of {limit.TotalMinutes} minutes stopped the editor");
        }

        return new CheckResult(check, run.ExitCode == 0, $"The editor gave the exit code {run.ExitCode}");
    }

    /// <summary>
    /// The report holds when it names at least one test, each test passed, and the counts show
    /// no failed test, no test that did not run, and no test still in process.
    /// </summary>
    private static CheckResult CheckReport(ToolOutput report)
    {
        const string check = "Test report";
        if (report.Text is null)
        {
            return new CheckResult(check, false, $"No test report: {report.Absence}");
        }

        ReportContent content;
        try
        {
            content = ReadReport(report.Text, report.Source);
        }
        catch (InvalidOperationException fault)
        {
            return new CheckResult(check, false, fault.Message);
        }

        List<string> faults = [];
        if (content.Tests.Count == 0)
        {
            faults.Add("it names no test, so the test filter matched none");
        }

        foreach (string count in new[] { "failed", "notRun", "inProcess" })
        {
            if (content.Counts[count] != 0)
            {
                faults.Add($"'{count}' is {content.Counts[count]}");
            }
        }

        List<string> notPassed = content.Tests
            .Where(test => test.State != PassedState)
            .Select(test => $"{test.Path} ({test.State})")
            .ToList();
        if (notPassed.Count > 0)
        {
            faults.Add($"these tests did not pass: {string.Join(", ", notPassed)}");
        }

        if (faults.Count > 0)
        {
            return new CheckResult(check, false, $"The report '{report.Source}' shows a fault: {string.Join("; and ", faults)}");
        }

        string names = string.Join(", ", content.Tests.Select(test => test.Path));
        return new CheckResult(check, true, $"The report '{report.Source}' shows {content.Tests.Count} passed tests: {names}");
    }

    private static CheckResult CheckLog(ToolOutput log)
    {
        const string check = "Success line";
        if (log.Text is null)
        {
            return new CheckResult(check, false, $"No log: {log.Absence}");
        }

        if (log.Text.Contains(SuccessLine, StringComparison.Ordinal))
        {
            return new CheckResult(check, true, $"The log '{log.Source}' holds the line '{SuccessLine}'");
        }

        return new CheckResult(check, false, $"The log '{log.Source}' has no line '{SuccessLine}'. An exit code of 0 alone is not a pass");
    }

    /// <summary>Reads the counts and each test of the report.</summary>
    /// <exception cref="InvalidOperationException">The report is not JSON, or it has no value of the right type in a field. The message names the file and the field.</exception>
    private static ReportContent ReadReport(string text, string source)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(text);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException($"The report '{source}' is not a JSON object");
            }

            var counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (string field in new[] { "succeeded", "succeededWithWarnings", "failed", "notRun", "inProcess" })
            {
                counts[field] = RequiredNumber(root, field, source);
            }

            if (!root.TryGetProperty("tests", out JsonElement tests) || tests.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException($"The report '{source}' has no array in the field 'tests'");
            }

            List<ReportTest> entries = [];
            foreach (JsonElement test in tests.EnumerateArray())
            {
                entries.Add(new ReportTest(RequiredText(test, "fullTestPath", source), RequiredText(test, "state", source)));
            }

            return new ReportContent(counts, entries);
        }
        catch (JsonException fault)
        {
            throw new InvalidOperationException($"The report '{source}' is not JSON: {fault.Message}", fault);
        }
    }

    private static int RequiredNumber(JsonElement root, string field, string source)
    {
        if (!root.TryGetProperty(field, out JsonElement value)
            || value.ValueKind != JsonValueKind.Number
            || !value.TryGetInt32(out int number))
        {
            throw new InvalidOperationException($"The report '{source}' has no whole number in the field '{field}'");
        }

        return number;
    }

    private static string RequiredText(JsonElement test, string field, string source)
    {
        string? text = null;
        if (test.ValueKind == JsonValueKind.Object
            && test.TryGetProperty(field, out JsonElement value)
            && value.ValueKind == JsonValueKind.String)
        {
            text = value.GetString();
        }

        if (string.IsNullOrEmpty(text))
        {
            throw new InvalidOperationException($"The report '{source}' has a test with no text in the field '{field}'");
        }

        return text;
    }

    private sealed record ReportTest(string Path, string State);

    private sealed record ReportContent(SortedDictionary<string, int> Counts, List<ReportTest> Tests);
}
