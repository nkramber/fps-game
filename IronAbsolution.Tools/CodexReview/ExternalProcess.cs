using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>The exit code and the output of one finished process.</summary>
/// <param name="Command">The program and its arguments, for the message of a fault.</param>
/// <param name="WorkingDirectory">The folder in which the process ran.</param>
/// <param name="ExitCode">The exit code of the process.</param>
/// <param name="StandardOutput">The full text of stdout.</param>
/// <param name="StandardError">The full text of stderr.</param>
public sealed record ProcessResult(string Command, string WorkingDirectory, int ExitCode, string StandardOutput, string StandardError)
{
    /// <summary>Gives stdout when the exit code is 0.</summary>
    /// <returns>The full text of stdout.</returns>
    /// <exception cref="InvalidOperationException">
    /// The exit code is not 0. The message names the command, the folder, the exit code, and stderr (T-2).
    /// </exception>
    public string RequireSuccess()
    {
        if (this.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"`{this.Command}` gave the exit code {this.ExitCode} in '{this.WorkingDirectory}'. stderr: {this.StandardError.Trim()}");
        }

        return this.StandardOutput;
    }
}

/// <summary>The result of a long process that wrote its output to files.</summary>
/// <param name="ExitCode">The exit code of the process, or -1 when the time limit stopped it.</param>
/// <param name="TimedOut">True when the time limit stopped the process.</param>
public sealed record FileRunResult(int ExitCode, bool TimedOut);

/// <summary>
/// Runs `gh` and the Codex CLI. Stdin closes at the start, because `codex exec` reads a piped
/// stdin into the prompt. Each variable in the removed list is absent from the environment of
/// the child, and the environment of this process does not change (D-53).
/// </summary>
public static class ExternalProcess
{
    /// <summary>Runs a short process to its end and gives its output.</summary>
    /// <param name="fileName">The program, as a name on the command path or a full path.</param>
    /// <param name="args">Each argument, in order.</param>
    /// <param name="workingDirectory">The folder in which the process runs.</param>
    /// <param name="removedVariables">Each environment variable that the child does not get.</param>
    /// <returns>The exit code, stdout, and stderr.</returns>
    /// <exception cref="InvalidOperationException">The program did not start. The message names it and the folder.</exception>
    public static ProcessResult Run(string fileName, IReadOnlyList<string> args, string workingDirectory, IReadOnlyList<string> removedVariables)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(removedVariables);

        using Process process = Start(fileName, args, workingDirectory, removedVariables);

        // The two streams drain at the same time, or a full stderr pipe stops the child.
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        string standardOutput = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return new ProcessResult(Describe(fileName, args), workingDirectory, process.ExitCode, standardOutput, standardError.GetAwaiter().GetResult());
    }

    /// <summary>
    /// Runs a long process, and writes stdout and stderr to two files as they arrive. At the
    /// time limit, the method stops the process and each child of it (D-50).
    /// </summary>
    /// <param name="fileName">The program, as a name on the command path or a full path.</param>
    /// <param name="args">Each argument, in order.</param>
    /// <param name="workingDirectory">The folder in which the process runs.</param>
    /// <param name="standardOutputPath">The file that takes stdout.</param>
    /// <param name="standardErrorPath">The file that takes stderr.</param>
    /// <param name="removedVariables">Each environment variable that the child does not get.</param>
    /// <param name="limit">The time limit of the process.</param>
    /// <returns>The exit code, and whether the time limit stopped the process.</returns>
    /// <exception cref="InvalidOperationException">The program did not start. The message names it and the folder.</exception>
    public static FileRunResult RunToFiles(
        string fileName,
        IReadOnlyList<string> args,
        string workingDirectory,
        string standardOutputPath,
        string standardErrorPath,
        IReadOnlyList<string> removedVariables,
        TimeSpan limit)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(removedVariables);

        using Process process = Start(fileName, args, workingDirectory, removedVariables);
        using FileStream standardOutputFile = File.Create(standardOutputPath);
        using FileStream standardErrorFile = File.Create(standardErrorPath);
        Task standardOutput = process.StandardOutput.BaseStream.CopyToAsync(standardOutputFile);
        Task standardError = process.StandardError.BaseStream.CopyToAsync(standardErrorFile);
        if (!process.WaitForExit(limit))
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
            Task.WaitAll(standardOutput, standardError);
            return new FileRunResult(-1, TimedOut: true);
        }

        Task.WaitAll(standardOutput, standardError);
        return new FileRunResult(process.ExitCode, TimedOut: false);
    }

    private static Process Start(string fileName, IReadOnlyList<string> args, string workingDirectory, IReadOnlyList<string> removedVariables)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (string arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        foreach (string variable in removedVariables)
        {
            startInfo.Environment.Remove(variable);
        }

        Process process;
        try
        {
            process = Process.Start(startInfo)
                ?? throw new InvalidOperationException($"'{fileName}' gave no process in '{workingDirectory}'.");
        }
        catch (Win32Exception exception)
        {
            throw new InvalidOperationException($"'{fileName}' did not start in '{workingDirectory}': {exception.Message}", exception);
        }

        process.StandardInput.Close();
        return process;
    }

    private static string Describe(string fileName, IReadOnlyList<string> args)
    {
        return $"{fileName} {string.Join(' ', args)}";
    }
}
