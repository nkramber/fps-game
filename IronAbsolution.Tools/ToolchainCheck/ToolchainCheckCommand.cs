using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IronAbsolution.Tools.ToolchainCheck;

/// <summary>
/// The `toolchain-check` command of the Windows PC (D-15, D-74). It reads the Visual Studio
/// version, the MSVC toolsets, the Windows SDK, the engine version, and the Git LFS version, and
/// it fails on each pin that does not hold (D-28, D-30).
/// Each line names the pin, the expected value, and the found value (T-2). `run.ps1
/// toolchain-check` runs it (D-99).
/// </summary>
public static class ToolchainCheckCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "toolchain-check";

    /// <summary>Reads the toolchain of this Windows PC and reports each pin.</summary>
    /// <param name="args">The arguments after the command name. The command takes none.</param>
    /// <param name="output">The writer that takes each line of a pin that holds.</param>
    /// <param name="errors">The writer that takes each line of a pin that fails.</param>
    /// <returns>0 when each pin holds, or 1 when a pin fails or the command line holds a fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        if (OptionParser.Read(Name, args, [], [], errors) is null)
        {
            return Program.FaultExitCode;
        }

        // The Windows version is information for the evidence form, not a pin (D-31).
        output.WriteLine($"{Name}: Windows: {Environment.OSVersion.Version} (information, not a pin).");
        ToolchainFacts facts = ToolchainFacts.Gather(
            Environment.GetEnvironmentVariable(ToolchainPins.EngineVariable),
            Environment.GetEnvironmentVariable(ToolchainPins.ProgramFilesX86Variable),
            "git",
            Directory.GetCurrentDirectory());
        return Report(ToolchainRules.Evaluate(facts), output, errors);
    }

    /// <summary>Writes one line for each pin and the total.</summary>
    /// <param name="results">The result of each pin.</param>
    /// <param name="output">The writer that takes each line of a pin that holds, and the total when each pin holds.</param>
    /// <param name="errors">The writer that takes each line of a pin that fails, and the total when a pin fails.</param>
    /// <returns>0 when each pin holds, or 1.</returns>
    public static int Report(IReadOnlyList<PinResult> results, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        foreach (PinResult result in results)
        {
            TextWriter writer = result.Holds ? output : errors;
            writer.WriteLine($"{Name}: {result.Line()}");
        }

        int failed = results.Count(result => !result.Holds);
        if (failed == 0)
        {
            output.WriteLine($"{Name}: each of the {results.Count} pins holds.");
            return 0;
        }

        errors.WriteLine($"{Name}: {failed} of the {results.Count} pins fail.");
        return Program.FaultExitCode;
    }
}
