using System;
using System.Collections.Generic;
using System.IO;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.DocGate;
using IronAbsolution.Tools.HandoffRotate;
using IronAbsolution.Tools.ReviewGate;
using IronAbsolution.Tools.SteCheck;
using IronAbsolution.Tools.ToolchainCheck;

namespace IronAbsolution.Tools;

/// <summary>The command line of the tools project. Each command gets its own PR (D-15).</summary>
public static class Program
{
    /// <summary>The exit code of a run that found a fault (T-2, D-40).</summary>
    public const int FaultExitCode = 1;

    /// <summary>
    /// The commands that no PR has written yet, and the PR that adds each one (G-8). PR-8 wrote
    /// the one planned command of phase 1, so the list is empty until a roadmap plans another.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> PlannedCommands =
        new SortedDictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Reads the command name and runs it.</summary>
    /// <param name="args">The command name, then the arguments of that command.</param>
    /// <returns>The exit code of the process.</returns>
    public static int Main(string[] args) => Run(args, Console.Out, Console.Error);

    /// <summary>
    /// Reads the command name, writes each fault to <paramref name="errors"/>, and gives the
    /// exit code. A command that no PR has written yet names that PR (G-8).
    /// </summary>
    /// <param name="args">The command name, then the arguments of that command.</param>
    /// <param name="output">The writer that takes the output of the command.</param>
    /// <param name="errors">The writer that takes each error line.</param>
    /// <returns>The exit code of the run.</returns>
    public static int Run(string[] args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        if (args.Length == 0)
        {
            errors.WriteLine("Error: no command. The first argument names the command.");
            WriteCommands(errors);
            return FaultExitCode;
        }

        string command = args[0];
        if (command == SteCheckCommand.Name)
        {
            return SteCheckCommand.Run(args[1..], output, errors);
        }

        if (command == CodexReviewCommand.Name)
        {
            return CodexReviewCommand.Run(args[1..], output, errors);
        }

        if (command == DocGateCommand.Name)
        {
            return DocGateCommand.Run(args[1..], output, errors);
        }

        if (command == HandoffRotateCommand.Name)
        {
            return HandoffRotateCommand.Run(args[1..], output, errors);
        }

        if (command == ReviewGateCommand.Name)
        {
            return ReviewGateCommand.Run(args[1..], output, errors);
        }

        if (command == ToolchainCheckCommand.Name)
        {
            return ToolchainCheckCommand.Run(args[1..], output, errors);
        }

        if (PlannedCommands.TryGetValue(command, out string? pullRequest))
        {
            errors.WriteLine(
                $"Error: the command '{command}' does not exist yet. {pullRequest} adds it.");
            return FaultExitCode;
        }

        errors.WriteLine($"Error: unknown command '{command}'.");
        WriteCommands(errors);
        return FaultExitCode;
    }

    private static void WriteCommands(TextWriter errors)
    {
        errors.WriteLine("The commands that exist:");
        errors.WriteLine($"  {SteCheckCommand.Name}: ready");
        errors.WriteLine($"  {CodexReviewCommand.Name}: ready");
        errors.WriteLine($"  {DocGateCommand.Name}: ready");
        errors.WriteLine($"  {HandoffRotateCommand.Name}: ready");
        errors.WriteLine($"  {ReviewGateCommand.Name}: ready");
        errors.WriteLine($"  {ToolchainCheckCommand.Name}: ready");
        if (PlannedCommands.Count == 0)
        {
            errors.WriteLine("The planned commands: none.");
            return;
        }

        errors.WriteLine("The planned commands, with the PR that adds each one:");
        foreach (KeyValuePair<string, string> entry in PlannedCommands)
        {
            errors.WriteLine($"  {entry.Key}: {entry.Value}");
        }
    }
}
