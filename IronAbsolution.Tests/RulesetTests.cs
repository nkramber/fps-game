using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The ruleset of `main` in `.github/rulesets/main.json` (D-60, D-61, D-62). A required check
/// that no job reports blocks every merge, and a PR job that the ruleset does not require lets
/// a failure merge in silence. These tests bind the file to the workflows, so a renamed or a
/// new job fails here.
/// </summary>
public sealed class RulesetTests
{
    private const string RulesetPath = ".github/rulesets/main.json";

    private const string WorkflowFolder = ".github/workflows";

    /// <summary>The app id of GitHub Actions. Each required check comes from it, so another app cannot report a name.</summary>
    private const int GitHubActionsAppId = 15368;

    /// <summary>The id of the repository admin role, the one bypass actor (D-60).</summary>
    private const int AdminRoleId = 5;

    /// <summary>A job key: two spaces, the key, and a colon with no value.</summary>
    private static readonly Regex JobKey = new(@"^  ([A-Za-z0-9_-]+):\s*$", RegexOptions.CultureInvariant);

    /// <summary>The name of a job: four spaces, then the key. A step name stands deeper, so it never counts.</summary>
    private static readonly Regex JobName = new(@"^    name:\s*(.+?)\s*$", RegexOptions.CultureInvariant);

    /// <summary>A pull request event in the block form of `on:`.</summary>
    private static readonly Regex PullRequestEventKey = new(@"^  pull_request(_target)?:", RegexOptions.CultureInvariant);

    [Fact]
    public void TheRulesetRequiresEachCheckOnceFromGitHubActions()
    {
        List<(string Context, int AppId)> checks = RequiredChecks();

        Assert.NotEmpty(checks);
        Assert.Equal(checks.Count, checks.Select(check => check.Context).Distinct(StringComparer.Ordinal).Count());
        Assert.All(checks, check => Assert.Equal(GitHubActionsAppId, check.AppId));
    }

    [Fact]
    public void TheRulesetRequiresEachCheckOfTheDecision()
    {
        // D-61 names four checks, and D-64 adds review-gate. A check that leaves the file and the
        // workflows together passes the two binding tests below, so this test holds each name.
        string[] decided = ["ste-check", "build, test, and format", "coverage report", "doc-gate", "review-gate"];
        HashSet<string> required = RequiredChecks().Select(check => check.Context).ToHashSet(StringComparer.Ordinal);
        foreach (string check in decided)
        {
            Assert.True(required.Contains(check), $"The ruleset of main does not require the check '{check}' that D-61 or D-64 names.");
        }
    }

    [Fact]
    public void EachRequiredCheckIsTheNameOfOnePullRequestJob()
    {
        Dictionary<string, List<string>> jobs = PullRequestJobsByCheckName();
        foreach ((string context, _) in RequiredChecks())
        {
            if (!jobs.TryGetValue(context, out List<string>? where))
            {
                Assert.Fail($"The required check '{context}' is the name of no job of a PR workflow, so it never reports, and it blocks every merge (D-61).");
                return;
            }

            Assert.True(where.Count == 1, $"The required check '{context}' is the name of {where.Count} jobs: {string.Join(", ", where)}. A pass of one hides a failure of another (D-61).");
        }
    }

    [Fact]
    public void EachPullRequestJobIsRequired()
    {
        HashSet<string> required = RequiredChecks().Select(check => check.Context).ToHashSet(StringComparer.Ordinal);
        foreach ((string name, List<string> where) in PullRequestJobsByCheckName())
        {
            Assert.True(required.Contains(name), $"The check '{name}' of {string.Join(", ", where)} runs on each PR, and the ruleset of main does not require it (D-61).");
        }
    }

    [Fact]
    public void TheRulesetGuardsMainWithSquashAndResolvedThreads()
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(RepositoryRoot.PathTo(RulesetPath)));
        JsonElement ruleset = document.RootElement;

        Assert.Equal("active", ruleset.GetProperty("enforcement").GetString());
        JsonElement refName = ruleset.GetProperty("conditions").GetProperty("ref_name");
        Assert.Equal("refs/heads/main", Assert.Single(refName.GetProperty("include").EnumerateArray()).GetString());
        Assert.Empty(refName.GetProperty("exclude").EnumerateArray());

        // D-62: the file holds no linear history rule, because the squash-only rule keeps the
        // history of main linear.
        string[] expectedTypes = ["deletion", "non_fast_forward", "pull_request", "required_status_checks"];
        string[] types = ruleset.GetProperty("rules").EnumerateArray().Select(RuleType).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(expectedTypes, types);

        JsonElement pullRequest = Parameters(ruleset, "pull_request");
        Assert.Equal("squash", Assert.Single(pullRequest.GetProperty("allowed_merge_methods").EnumerateArray()).GetString());
        Assert.True(pullRequest.GetProperty("required_review_thread_resolution").GetBoolean());
        Assert.Equal(0, pullRequest.GetProperty("required_approving_review_count").GetInt32());

        // GitHub adds these two fields when the file omits them. Its default of true asks for an
        // approval that the one owner cannot give to a commit of an unlinked author, so the merge
        // waits forever. The file declares both, so the comparison of the live ruleset stays empty.
        Assert.False(pullRequest.GetProperty("require_extra_approval_for_unattributed_changes").GetBoolean());
        Assert.Empty(pullRequest.GetProperty("required_reviewers").EnumerateArray());

        // The PRs go one at a time, so a rule that the branch holds the newest main only forces a rebase.
        Assert.False(Parameters(ruleset, "required_status_checks").GetProperty("strict_required_status_checks_policy").GetBoolean());
    }

    [Fact]
    public void TheAdminRoleBypassesThroughAPullRequestAlone()
    {
        // D-60: the admin role bypasses on a PR merge alone, and never by a direct push. No
        // session uses the bypass (D-5).
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(RepositoryRoot.PathTo(RulesetPath)));
        JsonElement actor = Assert.Single(document.RootElement.GetProperty("bypass_actors").EnumerateArray());

        Assert.Equal("RepositoryRole", actor.GetProperty("actor_type").GetString());
        Assert.Equal(AdminRoleId, actor.GetProperty("actor_id").GetInt32());
        Assert.Equal("pull_request", actor.GetProperty("bypass_mode").GetString());
    }

    [Fact]
    public void TheJobReadTakesTheNameOfEachJobOrItsKey()
    {
        const string workflow =
            "on:\n  pull_request:\njobs:\n" +
            "  # A comment is no job.\n" +
            "  named:\n    name: build, test, and format\n    steps:\n      - name: A step name is no check name\n" +
            "  unnamed:\n    runs-on: ubuntu-24.04\n";

        Dictionary<string, string> names = CheckNameByJob(workflow, "fixture.yml");

        Assert.Equal(2, names.Count);
        Assert.Equal("build, test, and format", names["named"]);
        Assert.Equal("unnamed", names["unnamed"]);
    }

    [Theory]
    [InlineData("on:\n  pull_request:\n")]
    [InlineData("on:\n  pull_request:\njobs:\n  # A comment is no job.\n")]
    public void AWorkflowWithNoJobIsAFault(string workflow)
    {
        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => CheckNameByJob(workflow, "fixture.yml"));

        Assert.Contains("fixture.yml", fault.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("on:\n  pull_request:\n    types: [opened]\n", true)]
    [InlineData("on:\n  push:\n  pull_request_target:\n", true)]
    [InlineData("on: [push, pull_request]\n", true)]
    [InlineData("on: pull_request\n", true)]
    [InlineData("on:\n  push:\n    branches: [main]\n", false)]
    [InlineData("on:\n  schedule:\n    - cron: '0 3 * * *'\n# The pull_request event is not a trigger here.\n", false)]
    public void TheTriggerReadFindsEachFormOfAPullRequestEvent(string workflow, bool expected)
    {
        Assert.Equal(expected, RunsOnPullRequest(workflow, "fixture.yml"));
    }

    [Fact]
    public void AWorkflowWithNoTriggerIsAFault()
    {
        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => RunsOnPullRequest("jobs:\n  x:\n", "fixture.yml"));

        Assert.Contains("fixture.yml", fault.Message, StringComparison.Ordinal);
    }

    private static List<(string Context, int AppId)> RequiredChecks()
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(RepositoryRoot.PathTo(RulesetPath)));
        var checks = new List<(string, int)>();
        foreach (JsonElement check in Parameters(document.RootElement, "required_status_checks").GetProperty("required_status_checks").EnumerateArray())
        {
            string context = check.GetProperty("context").GetString()
                ?? throw new InvalidOperationException($"'{RulesetPath}' has a required check with a null context (T-2).");
            checks.Add((context, check.GetProperty("integration_id").GetInt32()));
        }

        return checks;
    }

    private static JsonElement Parameters(JsonElement ruleset, string type)
    {
        foreach (JsonElement rule in ruleset.GetProperty("rules").EnumerateArray())
        {
            if (RuleType(rule) == type)
            {
                return rule.GetProperty("parameters").Clone();
            }
        }

        throw new InvalidOperationException($"'{RulesetPath}' has no rule of the type '{type}' (T-2).");
    }

    private static string RuleType(JsonElement rule)
    {
        return rule.GetProperty("type").GetString()
            ?? throw new InvalidOperationException($"'{RulesetPath}' has a rule with a null type (T-2).");
    }

    /// <summary>
    /// Maps each check name of each job of each PR workflow to the jobs that carry it. Each
    /// entry names the file and the job key.
    /// </summary>
    private static Dictionary<string, List<string>> PullRequestJobsByCheckName()
    {
        var jobs = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        string folder = RepositoryRoot.PathTo(WorkflowFolder);
        IEnumerable<string> paths = Directory.GetFiles(folder, "*.yml").Concat(Directory.GetFiles(folder, "*.yaml")).Order(StringComparer.Ordinal);
        foreach (string path in paths)
        {
            string file = Path.GetFileName(path);
            string workflow = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
            if (!RunsOnPullRequest(workflow, file))
            {
                continue;
            }

            foreach ((string key, string name) in CheckNameByJob(workflow, file))
            {
                if (!jobs.TryGetValue(name, out List<string>? where))
                {
                    where = [];
                    jobs[name] = where;
                }

                where.Add($"{file} job '{key}'");
            }
        }

        return jobs;
    }

    /// <summary>
    /// True when the `on:` key of the workflow names `pull_request` or `pull_request_target`, in
    /// the block form or on the `on:` line. A workflow with no `on:` line is a fault, because a
    /// silent skip hides its jobs from the ruleset tests (T-2).
    /// </summary>
    private static bool RunsOnPullRequest(string workflow, string file)
    {
        string[] lines = workflow.Split('\n');
        string? on = lines.FirstOrDefault(line => line.StartsWith("on:", StringComparison.Ordinal));
        if (on is null)
        {
            throw new InvalidOperationException($"The workflow '{file}' has no 'on:' line, so the ruleset tests cannot read its trigger (T-2).");
        }

        return on.Contains("pull_request", StringComparison.Ordinal) || lines.Any(line => PullRequestEventKey.IsMatch(line));
    }

    /// <summary>
    /// Maps each job key under `jobs:` to its check name: the `name:` line of the job, or the key
    /// when the job has none. The read takes the indent of this repository: jobs at two spaces,
    /// and the keys of a job at four. It is not a YAML parser.
    /// </summary>
    private static Dictionary<string, string> CheckNameByJob(string workflow, string file)
    {
        string[] lines = workflow.Split('\n');
        int start = Array.IndexOf(lines, "jobs:");
        if (start < 0)
        {
            throw new InvalidOperationException($"The workflow '{file}' has no 'jobs:' line, so the ruleset tests cannot read it (T-2).");
        }

        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        string? key = null;
        for (int index = start + 1; index < lines.Length; index += 1)
        {
            Match keyMatch = JobKey.Match(lines[index]);
            if (keyMatch.Success)
            {
                key = keyMatch.Groups[1].Value;
                names[key] = key;
                continue;
            }

            Match nameMatch = JobName.Match(lines[index]);
            if (key is not null && nameMatch.Success)
            {
                names[key] = nameMatch.Groups[1].Value;
            }
        }

        if (names.Count == 0)
        {
            throw new InvalidOperationException($"The workflow '{file}' has no job under a 'jobs:' line, so the ruleset tests cannot read it (T-2).");
        }

        return names;
    }
}
