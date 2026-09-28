using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools.CodexReview;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The launch of the Codex CLI through `node` and its entry script, the same path on macOS and
/// on Windows (D-47, PR #4 review P2-1). Each test runs a fake `codex.js`.
/// </summary>
public sealed class CodexLauncherTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "codex-launcher-" + Guid.NewGuid().ToString("N"));

    /// <summary>Makes the folder of the fake CLI.</summary>
    public CodexLauncherTests()
    {
        Directory.CreateDirectory(this.folder);
    }

    [Fact]
    public void NodeTakesTheScriptThenEachArgument()
    {
        Assert.Equal(["/npm/@openai/codex/bin/codex.js", "login", "status"], CodexLauncher.Arguments("/npm/@openai/codex/bin/codex.js", ["login", "status"]));
    }

    [Fact]
    public void TheVersionComesFromTheEntryScript()
    {
        // P2-1: the old command started the path itself. A script with no execute bit, or the
        // `codex.cmd` shim of Windows, did not start that way.
        string script = this.WriteScript("process.stdout.write('codex-cli 0.157.1\\n');");

        Assert.Equal(new CodexVersion(0, 157, 1, string.Empty), CodexLauncher.ReadVersion(script, this.folder));
    }

    [Fact]
    public void AMultiLinePromptReachesTheCliUnchanged()
    {
        // The review prompt has four lines. `cmd.exe` of a Windows shim would break it.
        string script = this.WriteScript("process.stdout.write(process.argv[2]);");
        string prompt = CodexReviewSettings.ReviewPrompt(4, "feat/pr-3-codex-review");

        ProcessResult result = CodexLauncher.Run(script, [prompt], this.folder);

        Assert.Equal(prompt, result.RequireSuccess());
    }

    [Fact]
    public void NoApiCredentialVariableReachesTheCli()
    {
        // Exit test 3 of PR-3 (D-53), through the launch path of each Codex call.
        string script = this.WriteScript("process.stdout.write(['OPENAI_API_KEY','CODEX_API_KEY','CODEX_ACCESS_TOKEN'].map(n => n + '=' + (process.env[n] ?? 'absent')).join('\\n'));");
        Dictionary<string, string?> saved = [];
        foreach (string variable in CodexReviewSettings.ApiCredentialVariables)
        {
            saved[variable] = Environment.GetEnvironmentVariable(variable);
            Environment.SetEnvironmentVariable(variable, "sk-test");
        }

        try
        {
            string output = CodexLauncher.Run(script, [], this.folder).RequireSuccess();

            Assert.Equal("OPENAI_API_KEY=absent\nCODEX_API_KEY=absent\nCODEX_ACCESS_TOKEN=absent", output);
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
    public void TheEntryScriptGivesTheEntryScriptOfTheNpmPackage()
    {
        // P2-1: `<npm prefix>/bin/codex` exists on macOS alone. `npm root` gives the package
        // folder on each platform. The entry script holds the path now (D-99).
        string entry = File.ReadAllText(RepositoryRoot.PathTo("run.ps1"));

        Assert.Contains("npm root --global", entry, StringComparison.Ordinal);
        Assert.Contains("@openai/codex/bin/codex.js", entry, StringComparison.Ordinal);
        Assert.DoesNotContain("/bin/codex'", entry, StringComparison.Ordinal);
    }

    [Fact]
    public void AMissingScriptNamesThePathAndTheTarget()
    {
        string problem = CodexLauncher.MissingProblem("/no/codex.js");

        Assert.Contains("/no/codex.js", problem, StringComparison.Ordinal);
        Assert.Contains("run.ps1 codex-review", problem, StringComparison.Ordinal);
    }

    /// <summary>Removes the folder of the fake CLI.</summary>
    public void Dispose()
    {
        Directory.Delete(this.folder, recursive: true);
    }

    private string WriteScript(string body)
    {
        string script = Path.Combine(this.folder, "codex.js");
        File.WriteAllText(script, body + "\n");
        return script;
    }
}
