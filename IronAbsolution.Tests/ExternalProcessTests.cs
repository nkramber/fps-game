using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools.CodexReview;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The process runner of the review: the removed variables, the two streams, the time limit,
/// and a program that does not start (D-50, D-53).
/// </summary>
public sealed class ExternalProcessTests
{
    [Fact]
    public void TheChildLosesARemovedVariableAndTheParentKeepsIt()
    {
        // A unique name, so no other test sees the variable.
        string name = "IRON_ABSOLUTION_TEST_CREDENTIAL_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(name, "fake-value");
        try
        {
            string stripped = ReadChildEnvironment([name]);
            string kept = ReadChildEnvironment([]);

            Assert.DoesNotContain(name, stripped, StringComparison.Ordinal);
            Assert.Contains(name + "=fake-value", kept, StringComparison.Ordinal);
            Assert.Equal("fake-value", Environment.GetEnvironmentVariable(name));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Fact]
    public void NoApiCredentialVariableReachesTheCodexProcess()
    {
        // Exit test 3 of PR-3 (D-53): a key in the environment of the shell never reaches Codex.
        Dictionary<string, string?> saved = [];
        foreach (string variable in CodexReviewSettings.ApiCredentialVariables)
        {
            saved[variable] = Environment.GetEnvironmentVariable(variable);
            Environment.SetEnvironmentVariable(variable, "sk-test-" + variable);
        }

        try
        {
            string environment = ReadChildEnvironment(CodexReviewSettings.ApiCredentialVariables);

            foreach (string variable in CodexReviewSettings.ApiCredentialVariables)
            {
                Assert.DoesNotContain("sk-test-" + variable, environment, StringComparison.Ordinal);
            }
        }
        finally
        {
            foreach (KeyValuePair<string, string?> entry in saved)
            {
                Environment.SetEnvironmentVariable(entry.Key, entry.Value);
            }
        }
    }

    [Fact]
    public void TheRunReadsStderrApartFromStdout()
    {
        // `codex login status` writes its status line to stderr, and stdout stays empty.
        ProcessResult result = OperatingSystem.IsWindows()
            ? ExternalProcess.Run("cmd.exe", ["/c", "echo Logged in using ChatGPT 1>&2"], Path.GetTempPath(), [])
            : ExternalProcess.Run("sh", ["-c", "echo 'Logged in using ChatGPT' >&2"], Path.GetTempPath(), []);

        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.StartsWith(CodexReviewSettings.ChatGptLoginStatus, CodexReviewSettings.LoginStatusText(result), StringComparison.Ordinal);
    }

    [Fact]
    public void AFailedRunThatMustSucceedNamesTheCommandAndStderr()
    {
        ProcessResult result = OperatingSystem.IsWindows()
            ? ExternalProcess.Run("cmd.exe", ["/c", "echo broken 1>&2 & exit 4"], Path.GetTempPath(), [])
            : ExternalProcess.Run("sh", ["-c", "echo broken >&2; exit 4"], Path.GetTempPath(), []);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(result.RequireSuccess);

        Assert.Contains("exit code 4", error.Message, StringComparison.Ordinal);
        Assert.Contains("broken", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AProgramThatDoesNotStartIsAnErrorThatNamesIt()
    {
        string missing = Path.Combine(Path.GetTempPath(), "no-such-program-" + Guid.NewGuid().ToString("N"));

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => ExternalProcess.Run(missing, [], Path.GetTempPath(), []));

        Assert.Contains(missing, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRunToFilesWritesBothStreams()
    {
        string folder = Path.Combine(Path.GetTempPath(), "external-process-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string outputFile = Path.Combine(folder, "out.txt");
            string errorFile = Path.Combine(folder, "err.txt");

            FileRunResult result = OperatingSystem.IsWindows()
                ? ExternalProcess.RunToFiles("cmd.exe", ["/c", "echo out & echo err 1>&2"], folder, outputFile, errorFile, [], TimeSpan.FromMinutes(1))
                : ExternalProcess.RunToFiles("sh", ["-c", "echo out; echo err >&2"], folder, outputFile, errorFile, [], TimeSpan.FromMinutes(1));

            Assert.Equal(new FileRunResult(0, TimedOut: false), result);
            Assert.Equal("out", File.ReadAllText(outputFile).Trim());
            Assert.Equal("err", File.ReadAllText(errorFile).Trim());
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void TheTimeLimitStopsALongRun()
    {
        // D-50: at the limit, the runner stops the process and says so.
        string folder = Path.Combine(Path.GetTempPath(), "external-process-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string outputFile = Path.Combine(folder, "out.txt");
            string errorFile = Path.Combine(folder, "err.txt");
            DateTime start = DateTime.UtcNow;

            FileRunResult result = OperatingSystem.IsWindows()
                ? ExternalProcess.RunToFiles("cmd.exe", ["/c", "ping -n 60 127.0.0.1 > nul"], folder, outputFile, errorFile, [], TimeSpan.FromSeconds(1))
                : ExternalProcess.RunToFiles("sh", ["-c", "sleep 60"], folder, outputFile, errorFile, [], TimeSpan.FromSeconds(1));

            Assert.True(result.TimedOut);
            Assert.True(DateTime.UtcNow - start < TimeSpan.FromSeconds(30), "The runner waited for the process after the limit.");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    /// <summary>Gives the environment that a child process prints, with the removed variables.</summary>
    private static string ReadChildEnvironment(IReadOnlyList<string> removed)
    {
        ProcessResult result = OperatingSystem.IsWindows()
            ? ExternalProcess.Run("cmd.exe", ["/c", "set"], Path.GetTempPath(), removed)
            : ExternalProcess.Run("env", [], Path.GetTempPath(), removed);
        return result.RequireSuccess();
    }
}
