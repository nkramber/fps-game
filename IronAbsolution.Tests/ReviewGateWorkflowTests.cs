using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IronAbsolution.Tools.ReviewGate;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The workflow of the review gate (D-64). It runs from the base branch with the rights to read
/// alone, and it never checks out the PR head. A PR thus cannot change the code that judges it.
/// The ruleset tests bind its job name to the required check.
/// </summary>
public sealed class ReviewGateWorkflowTests
{
    private const string WorkflowPath = ".github/workflows/review-gate.yml";

    private const string WorkflowFolder = ".github/workflows";

    /// <summary>A `ref:` input of a step. The checkout then takes that ref, and not the base branch.</summary>
    private static readonly Regex RefInput = new(@"^\s+ref:", RegexOptions.CultureInvariant | RegexOptions.Multiline);

    /// <summary>A permission with write rights: a scope and `write`, or `write-all`.</summary>
    private static readonly Regex WritePermission = new(@":\s*write\b|\bwrite-all\b", RegexOptions.CultureInvariant);

    [Fact]
    public void TheWorkflowRunsOnPullRequestTargetAlone()
    {
        string workflow = Workflow();

        Assert.Contains("on:\n  pull_request_target:\n    types: [opened, reopened, synchronize, labeled, unlabeled]\n", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("\n  pull_request:", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("\n  push:", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCheckoutTakesTheBaseBranchAndTheHeadIsDataAlone()
    {
        string workflow = Workflow();

        Assert.False(RefInput.IsMatch(workflow), $"'{WorkflowPath}' gives a ref to a step. On pull_request_target, the checkout must take the base branch (D-64).");
        Assert.Contains("git fetch --no-tags origin \"+refs/pull/${PR_NUMBER}/head:refs/remotes/pull/${PR_NUMBER}/head\"", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("git checkout", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("git switch", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTokenHasNoWriteRight()
    {
        string workflow = Workflow();

        Assert.Contains("permissions:\n  contents: read\n  pull-requests: read\n", workflow, StringComparison.Ordinal);
        Assert.False(WritePermission.IsMatch(workflow), $"'{WorkflowPath}' asks for a write right. The job is the check, so it writes nothing (D-64).");
    }

    [Fact]
    public void TheJobShellStopsOnAFailureInAPipe()
    {
        // With no shell, a run step starts `bash -e` with no pipefail (T-2).
        Assert.Contains("    defaults:\n      run:\n        shell: bash\n", Workflow(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheJobRunsTheCommandOfTheBaseCheckoutWithEachOption()
    {
        string workflow = Workflow();

        Assert.Contains($"  {ReviewGateRules.CheckName}:\n    name: {ReviewGateRules.CheckName}\n", workflow, StringComparison.Ordinal);
        Assert.Contains(
            "review-gate --root \"$GITHUB_WORKSPACE\" --base \"origin/${BASE_REF}\" --head \"$HEAD_SHA\"\n          --pr \"$PR_NUMBER\" --labels \"$RUNNER_TEMP/labels.json\"",
            workflow,
            StringComparison.Ordinal);
        Assert.Contains($".label.name == \"{ReviewGateRules.OverrideLabel}\"", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void NoOtherWorkflowRunsOnPullRequestTarget()
    {
        // The event runs with the secrets of the base. Each other workflow runs the code of the
        // PR head, so it must stay on the event pull_request (D-64).
        string folder = RepositoryRoot.PathTo(WorkflowFolder);
        string[] others = Directory.GetFiles(folder, "*.yml").Concat(Directory.GetFiles(folder, "*.yaml"))
            .Where(path => Path.GetFileName(path) != Path.GetFileName(WorkflowPath))
            .Where(path => File.ReadAllText(path).Contains("pull_request_target", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToArray();

        Assert.True(others.Length == 0, $"These workflows name pull_request_target: {string.Join(", ", others)}. Only {WorkflowPath} can (D-64).");
    }

    private static string Workflow()
    {
        return File.ReadAllText(RepositoryRoot.PathTo(WorkflowPath)).Replace("\r\n", "\n", StringComparison.Ordinal);
    }
}
