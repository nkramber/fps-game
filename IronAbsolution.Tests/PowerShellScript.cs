using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// Runs one PowerShell script of `scripts/` under `pwsh` (D-72). The hosted Ubuntu runner has
/// PowerShell, so CI runs each test that uses this class, and CI fails without it. The Mac has
/// no PowerShell by default, so a local run skips the test with the reason.
/// </summary>
public static class PowerShellScript
{
    /// <summary>Runs a script to its end, with one change to the environment of the child.</summary>
    /// <param name="scriptPath">The full path of the script.</param>
    /// <param name="workingDirectory">The folder in which the script runs.</param>
    /// <param name="variable">The environment variable that the child gets or loses.</param>
    /// <param name="value">The value of the variable, or null to remove it from the child.</param>
    /// <returns>The exit code, and stdout followed by stderr.</returns>
    public static (int ExitCode, string Output) Run(string scriptPath, string workingDirectory, string variable, string? value)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("pwsh")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (string arg in new List<string> { "-NoProfile", "-NonInteractive", "-File", scriptPath })
        {
            startInfo.ArgumentList.Add(arg);
        }

        startInfo.Environment.Remove(variable);
        if (value is not null)
        {
            startInfo.Environment[variable] = value;
        }

        Process process;
        try
        {
            process = Process.Start(startInfo) ?? throw new InvalidOperationException("'pwsh' gave no process.");
        }
        catch (Win32Exception exception)
        {
            // CI must run these tests. A local Mac with no PowerShell skips them with the reason.
            if (Environment.GetEnvironmentVariable("CI") == "true")
            {
                throw new InvalidOperationException($"'pwsh' did not start on CI: {exception.Message}", exception);
            }

            Assert.Skip($"PowerShell ('pwsh') is not on this machine: {exception.Message}. CI runs this test.");
            throw;
        }

        using (process)
        {
            Task<string> standardError = process.StandardError.ReadToEndAsync();
            string standardOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            string errors = standardError.GetAwaiter().GetResult();
            return (process.ExitCode, standardOutput + errors);
        }
    }
}
