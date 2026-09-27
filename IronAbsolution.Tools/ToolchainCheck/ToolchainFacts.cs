using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools.CodexReview;

namespace IronAbsolution.Tools.ToolchainCheck;

/// <summary>
/// The text that one source of the toolchain gave, or the reason that it gave none. Exactly one
/// of <see cref="Text"/> and <see cref="Absence"/> is set.
/// </summary>
/// <param name="Source">The command or the file that the text came from, for each message (T-2).</param>
/// <param name="Text">The text of the source, or null when the source is absent.</param>
/// <param name="Absence">The reason that the source gave no text, or null when it gave text.</param>
public sealed record ToolOutput(string Source, string? Text, string? Absence)
{
    /// <summary>Makes the output of a source that gave text.</summary>
    /// <param name="source">The command or the file.</param>
    /// <param name="text">The text that it gave.</param>
    /// <returns>The output.</returns>
    public static ToolOutput Found(string source, string text)
    {
        return new ToolOutput(source, text, null);
    }

    /// <summary>Makes the output of a source that gave no text.</summary>
    /// <param name="source">The command or the file.</param>
    /// <param name="absence">The reason, with its context (T-2).</param>
    /// <returns>The output.</returns>
    public static ToolOutput Absent(string source, string absence)
    {
        return new ToolOutput(source, null, absence);
    }
}

/// <summary>
/// Every fact that the rules of the toolchain check need, read one time from the Mac. The rules
/// do no I/O.
/// </summary>
/// <param name="Xcode">The output of `xcodebuild -version`.</param>
/// <param name="Engine">The text of the version file of the engine.</param>
/// <param name="GitLfs">The output of `git lfs version`.</param>
public sealed record ToolchainFacts(ToolOutput Xcode, ToolOutput Engine, ToolOutput GitLfs)
{
    /// <summary>Runs the two version commands and reads the version file of the engine.</summary>
    /// <param name="engineFolder">The value of the engine variable, or null when it is not set (D-79).</param>
    /// <param name="xcodebuild">The `xcodebuild` program, as a name on the command path or a full path.</param>
    /// <param name="git">The `git` program, as a name on the command path or a full path.</param>
    /// <param name="workingDirectory">The folder in which the two commands run.</param>
    /// <returns>The facts. A command that did not start or failed gives an absent output, not an exception.</returns>
    public static ToolchainFacts Gather(string? engineFolder, string xcodebuild, string git, string workingDirectory)
    {
        return new ToolchainFacts(
            RunTool(xcodebuild, ["-version"], workingDirectory),
            ReadBuildVersion(engineFolder),
            RunTool(git, ["lfs", "version"], workingDirectory));
    }

    private static ToolOutput RunTool(string fileName, IReadOnlyList<string> args, string workingDirectory)
    {
        string command = $"{fileName} {string.Join(' ', args)}";
        ProcessResult result;
        try
        {
            result = ExternalProcess.Run(fileName, args, workingDirectory, []);
        }
        catch (InvalidOperationException fault)
        {
            // An absent program is a failed pin with its reason, not a crash of the check (T-2).
            return ToolOutput.Absent(command, fault.Message);
        }

        if (result.ExitCode != 0)
        {
            return ToolOutput.Absent(command, $"the exit code {result.ExitCode}, stderr: {result.StandardError.Trim()}");
        }

        return ToolOutput.Found(command, result.StandardOutput);
    }

    private static ToolOutput ReadBuildVersion(string? engineFolder)
    {
        if (string.IsNullOrWhiteSpace(engineFolder))
        {
            return ToolOutput.Absent($"${ToolchainPins.EngineVariable}", $"the variable {ToolchainPins.EngineVariable} is not set (D-79)");
        }

        string path = Path.Combine(engineFolder, ToolchainPins.BuildVersionPath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            return ToolOutput.Absent(path, $"no file '{path}'. {ToolchainPins.EngineVariable} names the folder that holds 'Engine'");
        }

        try
        {
            return ToolOutput.Found(path, File.ReadAllText(path));
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            return ToolOutput.Absent(path, $"the file '{path}' did not read: {fault.Message}");
        }
    }
}
