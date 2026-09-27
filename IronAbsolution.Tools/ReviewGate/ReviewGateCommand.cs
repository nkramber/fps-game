using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace IronAbsolution.Tools.ReviewGate;

/// <summary>
/// The `review-gate` command (D-64). It reads the PR head from git and the labels from a file.
/// Then it applies the rules of <see cref="ReviewGateRules"/>. The workflow `review-gate.yml`
/// runs it from the base branch, and its exit code is the result of the required check.
/// </summary>
public static class ReviewGateCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = ReviewGateRules.CheckName;

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that names the base branch, such as `origin/main`.</summary>
    public const string BaseOption = "--base";

    /// <summary>The option that gives the full hash of the PR head.</summary>
    public const string HeadOption = "--head";

    /// <summary>The option that gives the GitHub number of the PR.</summary>
    public const string PullRequestOption = "--pr";

    /// <summary>The option that names the JSON file of the labels.</summary>
    public const string LabelsOption = "--labels";

    private static readonly string[] RequiredOptions = [BaseOption, HeadOption, PullRequestOption, LabelsOption];

    /// <summary>Reads the options, gathers the facts, and writes the result.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes each line of the result.</param>
    /// <param name="errors">The writer that takes each fault of the command line, of the labels file, or of git.</param>
    /// <returns>0 when the gate passes, or 1 when it fails or the command has a fault (D-40).</returns>
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
        string? pullRequestText = options.Value(PullRequestOption);
        string? labelsPath = options.Value(LabelsOption);
        if (baseRevision is null || head is null || pullRequestText is null || labelsPath is null)
        {
            errors.WriteLine($"Error: {Name} needs each of {string.Join(", ", RequiredOptions)}, each with a value.");
            return Program.FaultExitCode;
        }

        if (!int.TryParse(pullRequestText, NumberStyles.None, CultureInfo.InvariantCulture, out int pullRequest) || pullRequest <= 0)
        {
            errors.WriteLine($"Error: {Name} needs {PullRequestOption} <number>, the GitHub number of the PR. Found '{pullRequestText}'.");
            return Program.FaultExitCode;
        }

        string root = Path.GetFullPath(options.ValueOr(RootOption, "."));
        ReviewGateFacts facts;
        try
        {
            PullRequestLabels labels = PullRequestLabels.Read(labelsPath);
            facts = ReviewGateFacts.Gather(root, baseRevision, head, pullRequest, labels);
        }
        catch (Exception fault) when (fault is InvalidOperationException or IOException or InvalidDataException or UnauthorizedAccessException)
        {
            errors.WriteLine($"{Name}: fault in '{root}'. {fault.Message}");
            return Program.FaultExitCode;
        }

        ReviewGateResult result = ReviewGateRules.Evaluate(facts);
        foreach (string line in result.Details)
        {
            output.WriteLine($"{Name}: {line}");
        }

        output.WriteLine($"{Name}: {(result.Passes ? "pass" : "fail")} for PR #{pullRequest} at {head}, {result.Title}.");
        return result.Passes ? 0 : Program.FaultExitCode;
    }
}
