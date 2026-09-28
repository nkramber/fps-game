using System;
using System.Collections.Generic;
using System.IO;

namespace IronAbsolution.Tools.HandoffRotate;

/// <summary>
/// The `handoff-rotate` command (D-20, D-58). It keeps the 10 newest handoff entries and moves
/// each older entry to the top of the archive. It puts an entry that sits under an older one
/// back in its place, and it names that entry. It prints the moved numbers and the next
/// session number. `run.ps1 handoff-rotate` runs it (D-59).
/// </summary>
public static class HandoffRotateCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "handoff-rotate";

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>Reads the two files, rotates, and writes each file that changed.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when the files hold the rule, or 1 for a fault. After a fault, no file changed (D-40).</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = Path.GetFullPath(options.ValueOr(RootOption, "."));
        string handoffPath = ToolPaths.UnderFolder(root, HandoffRotateRules.HandoffPath);
        string archivePath = ToolPaths.UnderFolder(root, HandoffRotateRules.ArchivePath);
        foreach (string path in new[] { handoffPath, archivePath })
        {
            if (!File.Exists(path))
            {
                errors.WriteLine($"{Name}: the file '{path}' does not exist. No file changed.");
                return Program.FaultExitCode;
            }
        }

        HandoffRotation rotation;
        try
        {
            rotation = HandoffRotateRules.Rotate(File.ReadAllText(handoffPath), File.ReadAllText(archivePath));
        }
        catch (Exception fault) when (fault is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            errors.WriteLine($"{Name}: {fault.Message} No file changed.");
            return Program.FaultExitCode;
        }

        string sorted = rotation.Reordered.Count == 0
            ? string.Empty
            : $" Session {string.Join(", Session ", rotation.Reordered)} changed place, because an entry sat under an older one (D-58).";
        if (rotation.Moved.Count == 0)
        {
            if (rotation.Reordered.Count > 0)
            {
                File.WriteAllText(handoffPath, rotation.Handoff);
            }

            output.WriteLine($"{Name}: the handoff holds {HandoffRotateRules.KeepCount} entries or fewer. No entry moved.{sorted} The next session number is {rotation.NextSession}.");
            return 0;
        }

        // The archive write comes first. A stop between the two writes leaves copies at the
        // archive top, and the next run names that state and changes no file.
        File.WriteAllText(archivePath, rotation.Archive);
        File.WriteAllText(handoffPath, rotation.Handoff);
        output.WriteLine($"{Name}: moved Session {string.Join(", Session ", rotation.Moved)} to '{HandoffRotateRules.ArchivePath}'.{sorted} The next session number is {rotation.NextSession}.");
        return 0;
    }
}
