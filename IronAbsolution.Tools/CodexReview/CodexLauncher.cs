using System;
using System.Collections.Generic;
using System.Globalization;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>
/// Starts the Codex CLI. The CLI is the entry script `bin/codex.js` of the npm package, and the
/// launcher starts it with `node` on each platform (D-47). npm gives Windows a `codex.cmd` shim
/// in place of the script, and `cmd.exe` breaks a prompt of more than one line, so no call goes
/// through a shim or a shell. Each call removes the API credential variables (D-53).
/// </summary>
public static class CodexLauncher
{
    /// <summary>The program that runs the entry script. npm installs the CLI, so `node` is on the command path.</summary>
    public const string NodeProgram = "node";

    /// <summary>Gives the arguments of `node`: the entry script, then each argument of the CLI.</summary>
    /// <param name="script">The full path of `bin/codex.js`.</param>
    /// <param name="args">Each argument of the CLI, in order.</param>
    /// <returns>The arguments of `node`.</returns>
    public static IReadOnlyList<string> Arguments(string script, IReadOnlyList<string> args)
    {
        ArgumentException.ThrowIfNullOrEmpty(script);
        ArgumentNullException.ThrowIfNull(args);
        return [script, .. args];
    }

    /// <summary>Runs a short call of the CLI to its end.</summary>
    /// <param name="script">The full path of `bin/codex.js`.</param>
    /// <param name="args">Each argument of the CLI, in order.</param>
    /// <param name="workingDirectory">The folder in which the CLI runs.</param>
    /// <returns>The exit code, stdout, and stderr.</returns>
    /// <exception cref="InvalidOperationException">`node` did not start. The message names it and the folder.</exception>
    public static ProcessResult Run(string script, IReadOnlyList<string> args, string workingDirectory)
    {
        return ExternalProcess.Run(NodeProgram, Arguments(script, args), workingDirectory, CodexReviewSettings.ApiCredentialVariables);
    }

    /// <summary>Runs a long call of the CLI, with stdout and stderr to two files and a time limit (D-50).</summary>
    /// <param name="script">The full path of `bin/codex.js`.</param>
    /// <param name="args">Each argument of the CLI, in order.</param>
    /// <param name="workingDirectory">The folder in which the CLI runs.</param>
    /// <param name="standardOutputPath">The file that takes stdout.</param>
    /// <param name="standardErrorPath">The file that takes stderr.</param>
    /// <param name="limit">The time limit of the call.</param>
    /// <returns>The exit code, and whether the time limit stopped the call.</returns>
    /// <exception cref="InvalidOperationException">`node` did not start. The message names it and the folder.</exception>
    public static FileRunResult RunToFiles(string script, IReadOnlyList<string> args, string workingDirectory, string standardOutputPath, string standardErrorPath, TimeSpan limit)
    {
        return ExternalProcess.RunToFiles(NodeProgram, Arguments(script, args), workingDirectory, standardOutputPath, standardErrorPath, CodexReviewSettings.ApiCredentialVariables, limit);
    }

    /// <summary>Reads the version of the CLI.</summary>
    /// <param name="script">The full path of `bin/codex.js`.</param>
    /// <param name="workingDirectory">The folder in which the CLI runs.</param>
    /// <returns>The version.</returns>
    /// <exception cref="InvalidOperationException">`node` did not start, or the CLI gave an exit code other than 0.</exception>
    /// <exception cref="FormatException">The output has another form.</exception>
    public static CodexVersion ReadVersion(string script, string workingDirectory)
    {
        ProcessResult result = Run(script, ["--version"], workingDirectory);
        return CodexVersion.Parse(result.RequireSuccess());
    }

    /// <summary>Gives the problem of an entry script that does not exist, for the refusal of the start.</summary>
    /// <param name="script">The path that `--codex` gave.</param>
    /// <returns>The text of the problem.</returns>
    public static string MissingProblem(string script)
    {
        return string.Format(CultureInfo.InvariantCulture, "The Codex CLI is missing: no file at '{0}'. Run `make codex-review`, which installs it and gives the path of `bin/codex.js` (D-47).", script);
    }
}
