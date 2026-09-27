using System;
using System.Collections.Generic;
using System.IO;

namespace IronAbsolution.Tools.DocGate;

/// <summary>
/// The `doc-gate` command (D-56, D-57). It reads the diff, the handoff, and the commit messages
/// of the PR from git, and the description from a file. Then it applies the rules of
/// <see cref="DocGateRules"/>. The workflow `doc-gate.yml` runs it on each push and on each
/// edit of the description.
/// </summary>
public static class DocGateCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = DocGateRules.JobName;

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that names the base branch, such as `origin/main`.</summary>
    public const string BaseOption = "--base";

    /// <summary>The option that names the head of the PR.</summary>
    public const string HeadOption = "--head";

    /// <summary>The option that names the file of the PR description.</summary>
    public const string BodyOption = "--body";

    /// <summary>The option that gives the PR title.</summary>
    public const string TitleOption = "--title";

    /// <summary>The option that gives the PR branch.</summary>
    public const string BranchOption = "--branch";

    private static readonly string[] RequiredOptions = [BaseOption, HeadOption, BodyOption, TitleOption, BranchOption];

    /// <summary>Reads the options, gathers the facts, and writes one line for each problem.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes each problem and the result line.</param>
    /// <param name="errors">The writer that takes each fault of the command line or of git.</param>
    /// <returns>0 when the PR passes, or 1 when it fails or the command has a fault (D-40).</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, .. RequiredOptions], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? baseRevision = options.Value(BaseOption);
        string? head = options.Value(HeadOption);
        string? bodyPath = options.Value(BodyOption);
        string? title = options.Value(TitleOption);
        string? branch = options.Value(BranchOption);
        if (baseRevision is null || head is null || bodyPath is null || title is null || branch is null)
        {
            errors.WriteLine($"Error: {Name} needs each of {string.Join(", ", RequiredOptions)}, each with a value.");
            return Program.FaultExitCode;
        }

        string root = Path.GetFullPath(options.ValueOr(RootOption, "."));
        DocGateFacts facts;
        try
        {
            facts = DocGateFacts.Gather(root, baseRevision, head, bodyPath, title, branch);
        }
        catch (Exception fault) when (fault is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            errors.WriteLine($"{Name}: fault in '{root}'. {fault.Message}");
            return Program.FaultExitCode;
        }

        DocGateResult result = DocGateRules.Evaluate(facts);
        foreach (string problem in result.Problems)
        {
            output.WriteLine($"{Name}: {problem}");
        }

        output.WriteLine($"{Name}: {(result.Passes ? "pass" : "fail")}, {result.Problems.Count} problem(s) over {facts.ChangedPaths.Count} changed path(s) and {facts.CommitMessages.Count} commit(s).");
        return result.Passes ? 0 : Program.FaultExitCode;
    }
}
