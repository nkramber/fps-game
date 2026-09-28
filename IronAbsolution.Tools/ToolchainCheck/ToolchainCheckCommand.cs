using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IronAbsolution.Tools.ToolchainCheck;

/// <summary>
/// The `toolchain-check` command of the Mac (D-15, D-55). It reads the Xcode version, the
/// engine version, and the Git LFS version, and it fails on each pin that does not hold (D-28).
/// Each line names the pin, the expected value, and the found value (T-2). `make
/// toolchain-check` runs it. The Windows PC runs `scripts/toolchain-check.ps1` instead (D-72).
/// </summary>
public static class ToolchainCheckCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "toolchain-check";

    /// <summary>Reads the toolchain of this Mac and reports each pin.</summary>
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

        string? engineFolder = Environment.GetEnvironmentVariable(ToolchainPins.EngineVariable);
        ToolchainFacts facts = ToolchainFacts.Gather(engineFolder, "xcodebuild", "git", Directory.GetCurrentDirectory());
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
