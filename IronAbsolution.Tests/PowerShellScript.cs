using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// Runs one PowerShell script of `scripts/` under `pwsh` (D-72). The hosted Ubuntu runner has
/// PowerShell, so CI runs each test that uses this class, and CI fails without it. The Windows PC
/// takes PowerShell 7 as an owner step of `docs/runbooks/session-context.md` (D-101). A machine
/// without it skips the test with the reason, and no test reads a machine-dependent value.
/// </summary>
public static class PowerShellScript
{
    /// <summary>
    /// Finds a program on the PATH of this process, and gives its full path. The child of a test
    /// can take a PATH with no program on it, and the full path still starts PowerShell.
    /// </summary>
    /// <param name="name">The name of the program, with no file type.</param>
    /// <returns>The full path, or the name when the PATH holds no such program.</returns>
    public static string ProgramPath(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        string[] names = OperatingSystem.IsWindows() ? [name + ".exe", name] : [name];
        foreach (string folder in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator))
        {
            if (folder.Length == 0)
            {
                continue;
            }

            foreach (string candidate in names)
            {
                string path = Path.Combine(folder, candidate);
                if (File.Exists(path))
                {
                    return path;
                }
            }
        }

        // The caller starts the name itself, and the fault of the start names the program.
        return name;
    }

    /// <summary>Runs a script to its end, with a change to the environment of the child.</summary>
    /// <param name="scriptPath">The full path of the script.</param>
    /// <param name="workingDirectory">The folder in which the script runs.</param>
    /// <param name="environment">
    /// The variable of each change, with its value. A null value removes the variable from the
    /// child, so the child does not read the value of this machine.
    /// </param>
    /// <param name="arguments">The arguments after the script path, or none.</param>
    /// <returns>The exit code, and stdout followed by stderr.</returns>
    public static (int ExitCode, string Output) Run(
        string scriptPath,
        string workingDirectory,
        IReadOnlyDictionary<string, string?> environment,
        IReadOnlyList<string>? arguments = null)
    {
        ArgumentNullException.ThrowIfNull(environment);

        ProcessStartInfo startInfo = new ProcessStartInfo(ProgramPath("pwsh"))
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

        foreach (string arg in arguments ?? [])
        {
            startInfo.ArgumentList.Add(arg);
        }

        foreach (KeyValuePair<string, string?> change in environment)
        {
            startInfo.Environment.Remove(change.Key);
            if (change.Value is not null)
            {
                startInfo.Environment[change.Key] = change.Value;
            }
        }

        Process process;
        try
        {
            process = Process.Start(startInfo) ?? throw new InvalidOperationException("'pwsh' gave no process.");
        }
        catch (Win32Exception exception)
        {
            // CI must run these tests. A machine with no PowerShell 7 skips them with the reason.
            if (Environment.GetEnvironmentVariable("CI") == "true")
            {
                throw new InvalidOperationException($"'pwsh' did not start on CI: {exception.Message}", exception);
            }

            Assert.Skip($"PowerShell 7 ('pwsh') is not on this machine: {exception.Message}. Run `winget install Microsoft.PowerShell` (D-101).");
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
