using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronAbsolution.Tools;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.DocGate;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The documents gate (D-56, D-57): the rules over sample descriptions and diffs, the read of
/// real commits, the bind to the PR template, and the exit codes of the command.
/// </summary>
public sealed class DocGateTests
{
    private const string Branch = "feat/pr-70-sample";
    private const string Title = "feat: the sample change (PR-70)";

    private static readonly string[] CodeAndDocs =
    [
        "IronAbsolution.Tools/Sample.cs",
        "IronAbsolution.Tests/SampleTests.cs",
        "docs/design.md",
        "docs/decisions.md",
        "docs/roadmaps/phase-1-engine-proof.md",
        "docs/session-handoff.md",
    ];

    [Fact]
    public void APullRequestWithCodeAndEachAffectedDocumentPasses()
    {
        DocGateResult result = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs));

        Assert.True(result.Passes, string.Join("\n", result.Problems));
    }

    [Fact]
    public void ASpecificNoChangeReasonPasses()
    {
        string body = Body(("`docs/design.md`", "Reviewed; no change needed: section 6.2 already names the rule that this fix keeps."));
        string[] paths = CodeAndDocs.Where(path => path != "docs/design.md").ToArray();

        DocGateResult result = DocGateRules.Evaluate(Facts(body, paths));

        Assert.True(result.Passes, string.Join("\n", result.Problems));
    }

    [Fact]
    public void AnEmptyDocumentsLineFails()
    {
        // Exit test 1 of PR-4.
        DocGateResult result = DocGateRules.Evaluate(Facts(Body(("`docs/questions.md`", string.Empty)), CodeAndDocs));

        Assert.False(result.Passes);
        Assert.Contains(result.Problems, problem => problem.StartsWith("The line for `docs/questions.md` does not start with", StringComparison.Ordinal));
    }

    [Fact]
    public void TheBlankTemplateFailsOnEachCategory()
    {
        // A PR that keeps the template as it is gives one problem for each category.
        string template = File.ReadAllText(RepositoryRoot.PathTo(".github/pull_request_template.md"));

        DocGateResult result = DocGateRules.Evaluate(Facts(template, CodeAndDocs));

        Assert.Equal(DocGateRules.Categories.Count, result.Problems.Count(problem => problem.Contains("does not start with", StringComparison.Ordinal)));
    }

    [Fact]
    public void TheTemplateNamesEachCategoryInOrder()
    {
        // The template and the rules change together (D-22).
        string template = File.ReadAllText(RepositoryRoot.PathTo(".github/pull_request_template.md"));
        string section = template[template.IndexOf(DocGateRules.SectionHeading, StringComparison.Ordinal)..];
        List<string> lines = section.Split('\n').Where(line => line.StartsWith("- ", StringComparison.Ordinal)).Select(line => line.TrimEnd()).ToList();

        Assert.Equal(DocGateRules.Categories.Select(category => $"- {category.Label}:"), lines);
    }

    [Fact]
    public void AMissingHandoffFails()
    {
        string[] paths = CodeAndDocs.Where(path => path != DocGateRules.HandoffPath).ToArray();
        string body = Body(("`docs/session-handoff.md`", "Reviewed; no change needed: the entry of this session sits in another place."));

        DocGateResult result = DocGateRules.Evaluate(Facts(body, paths, handoffEntry: null));

        Assert.False(result.Passes);
        Assert.Contains(result.Problems, problem => problem.Contains($"does not change {DocGateRules.HandoffPath}", StringComparison.Ordinal));
    }

    [Fact]
    public void AHandoffEntryOfAnotherBranchFails()
    {
        DocGateResult result = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs, Entry("docs/pr-68-notes")));

        Assert.False(result.Passes);
        Assert.Contains(result.Problems, problem => problem.Contains($"does not name Branch: `{Branch}`", StringComparison.Ordinal));
    }

    [Fact]
    public void TheNewestEntryIsTheFirstLineOfAHandoffWithNoTitle()
    {
        // The handoff has no title (D-20). A parse that looks for a line end before the heading
        // would skip the newest entry and read the second one.
        string handoff = Entry(Branch) + "\n" + Entry("feat/pr-69-older").Replace("Session 12", "Session 11", StringComparison.Ordinal);

        string? newest = DocGateRules.NewestHandoffEntry(handoff);

        Assert.NotNull(newest);
        Assert.StartsWith("## Session 12:", newest, StringComparison.Ordinal);
        Assert.Contains(DocGateRules.BranchMark(Branch), newest, StringComparison.Ordinal);
        Assert.DoesNotContain("feat/pr-69-older", newest, StringComparison.Ordinal);
        Assert.Null(DocGateRules.NewestHandoffEntry("# A title alone\n"));
    }

    [Fact]
    public void TheNewestEntryOfTheRepositoryHandoffNamesABranch()
    {
        string handoff = File.ReadAllText(RepositoryRoot.PathTo(DocGateRules.HandoffPath));

        string? newest = DocGateRules.NewestHandoffEntry(handoff);

        Assert.NotNull(newest);
        Assert.Contains("Branch: `", newest, StringComparison.Ordinal);
    }

    [Fact]
    public void DeferredDocumentsInTheDescriptionFail()
    {
        string[] deferrals =
        [
            "The owner merges, and a docs PR records the merge.",
            "A follow-up documentation PR adds the roadmap line.",
            "We will update the design doc next week.",
            "Record the status in the roadmap after the merge.",
            "Roadmap status: TBD.",
            "The roadmap mark follows in a docs PR after the merge.",
        ];
        foreach (string deferral in deferrals)
        {
            DocGateResult result = DocGateRules.Evaluate(Facts(Body() + "\n## Next\n\n" + deferral + "\n", CodeAndDocs));

            Assert.False(result.Passes, deferral);
            Assert.Contains(result.Problems, problem => problem.StartsWith("The PR description defers documents", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void TextAboutTheBehaviorOfAJobIsNoDeferral()
    {
        string body = Body() + "\n## Behavior\n\nThe job will update the check run on each push.\n";

        DocGateResult result = DocGateRules.Evaluate(Facts(body, CodeAndDocs));

        Assert.True(result.Passes, string.Join("\n", result.Problems));
    }

    [Fact]
    public void DeferredDocumentsInTheNewestHandoffEntryFail()
    {
        string entry = Entry(Branch) + "The owner merges, and a docs PR records the merge.\n";

        DocGateResult result = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs, entry));

        Assert.False(result.Passes);
        Assert.Contains(result.Problems, problem => problem.StartsWith("The newest handoff entry defers documents", StringComparison.Ordinal));
    }

    [Fact]
    public void AMergeRecordTitleOrBranchFails()
    {
        DocGateResult byTitle = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs, title: "docs: record the merge of PR-68"));
        DocGateResult byOtherTitle = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs, title: "docs: record the PR-11 merge and the questions"));
        DocGateResult byBranch = DocGateRules.Evaluate(Facts(Body(), CodeAndDocs, Entry("docs/pr-68-merge-record"), branch: "docs/pr-68-merge-record"));

        Assert.All([byTitle, byOtherTitle, byBranch], result => Assert.Contains(result.Problems, problem => problem.Contains("names a merge record", StringComparison.Ordinal)));
    }

    [Fact]
    public void AMissingOrIncompleteLineFails()
    {
        string missing = Body().Replace("- `docs/runbooks/`: Not applicable: the change touches no runbook and no machine setup step.\n", string.Empty, StringComparison.Ordinal);
        string generic = Body(("`docs/questions.md`", "Reviewed; no change needed: no documentation impact."));
        string shortReason = Body(("`docs/questions.md`", "Not applicable: nothing."));

        Assert.Contains(DocGateRules.Evaluate(Facts(missing, CodeAndDocs)).Problems, problem => problem.Contains("no line for `docs/runbooks/`", StringComparison.Ordinal));
        Assert.Contains(DocGateRules.Evaluate(Facts(generic, CodeAndDocs)).Problems, problem => problem.Contains("generic reason", StringComparison.Ordinal));
        Assert.Contains(DocGateRules.Evaluate(Facts(shortReason, CodeAndDocs)).Problems, problem => problem.Contains("generic reason", StringComparison.Ordinal));
    }

    [Fact]
    public void ADescriptionWithNoDocumentsSectionFails()
    {
        DocGateResult result = DocGateRules.Evaluate(Facts("## Summary\n\nThe sample change.\n", CodeAndDocs));

        Assert.Contains(result.Problems, problem => problem.Contains("has no '## Documents' section", StringComparison.Ordinal));
    }

    [Fact]
    public void ASecondLineForACategoryFails()
    {
        // A second line after a valid first line makes the disposition ambiguous.
        string body = Body() + "- `docs/design.md`: Not applicable: no design change reaches this pull request.\n";

        DocGateResult result = DocGateRules.Evaluate(Facts(body, CodeAndDocs));

        Assert.False(result.Passes);
        Assert.Contains(result.Problems, problem => problem.Contains("2 lines for `docs/design.md`", StringComparison.Ordinal));
    }

    [Fact]
    public void ALineThatDisagreesWithTheDiffFails()
    {
        string claimsNoChange = Body(("`docs/design.md`", "Reviewed; no change needed: section 6.2 already names the rule."));
        string claimsChange = Body(("`docs/questions.md`", "Changed: OQ-21 records the question of the sample change."));

        Assert.Contains(DocGateRules.Evaluate(Facts(claimsNoChange, CodeAndDocs)).Problems, problem => problem.Contains("the diff changes a path of that category", StringComparison.Ordinal));
        Assert.Contains(DocGateRules.Evaluate(Facts(claimsChange, CodeAndDocs)).Problems, problem => problem.Contains("the diff changes no path of that category", StringComparison.Ordinal));
    }

    [Fact]
    public void AMoveToTheArchiveCountsAsAHandoffChange()
    {
        string[] paths = [.. CodeAndDocs, "docs/session-handoff-archive.md"];

        DocGateResult result = DocGateRules.Evaluate(Facts(Body(), paths));

        Assert.True(result.Passes, string.Join("\n", result.Problems));
    }

    [Fact]
    public void ALineInsideAnHtmlCommentDoesNotCount()
    {
        string body = Body().Replace("- `docs/questions.md`:", "<!-- - `docs/questions.md`:", StringComparison.Ordinal)
            .Replace("sample change.\n- `docs/roadmaps/`", "sample change. -->\n- `docs/roadmaps/`", StringComparison.Ordinal);

        DocGateResult result = DocGateRules.Evaluate(Facts(body, CodeAndDocs));

        Assert.Contains(result.Problems, problem => problem.Contains("no line for `docs/questions.md`", StringComparison.Ordinal));
    }

    [Fact]
    public void TheHandoffCommitStaysInTheMetadataSet()
    {
        // The gate asks each PR for a handoff commit, and that commit must not move the work head (D-14).
        Assert.Contains(DocGateRules.HandoffPath, ReviewHeads.MetadataPaths);
        Assert.Contains("docs/session-handoff-archive.md", ReviewHeads.MetadataPaths);
    }

    [Fact]
    public void EachAttributionFormFailsAndNamesItsPlace()
    {
        // Each form fails in each place that the gate reads. The problem names the place, the
        // line, and the form. The case of a trailer does not matter (T-6, D-16).
        string robot = char.ConvertFromUtf32(0x1F916);
        (string Title, string BodyPrefix, string Commit, string Expected)[] cases =
        [
            (Title, string.Empty, "feat: sample\n\nThe change.\n\nCo-authored-by: A Person <a@example.invalid>\n", "The message of commit 1a2b3c4, line 5, holds a co-author trailer"),
            (Title, string.Empty, "feat: sample\n\nco-authored-by: a person\n", "The message of commit 1a2b3c4, line 3, holds a co-author trailer"),
            (Title, "CO-AUTHORED-BY: A Person\n", "feat: sample\n", "The PR description, line 1, holds a co-author trailer"),
            (Title, "<!--\nCo-authored-by: A Person\n-->\n", "feat: sample\n", "The PR description, line 2, holds a co-author trailer"),
            (Title, $"{robot} Generated with [Claude Code](https://example.invalid/claude-code)\n", "feat: sample\n", "The PR description, line 1, holds a robot line"),
            (Title, $"{robot} Generated with [Claude Code](https://example.invalid/claude-code)\n", "feat: sample\n", "The PR description, line 1, holds a generation line"),
            ("feat: the sample change, generated by Codex", string.Empty, "feat: sample\n", "The PR title, line 1, holds a generation line"),
            (Title, string.Empty, "feat: sample\n\nGenerated with ChatGPT.\n", "The message of commit 1a2b3c4, line 3, holds a generation line"),
            (Title, string.Empty, "feat: sample\n\nThe text was generated by `Copilot`.\n", "The message of commit 1a2b3c4, line 3, holds a generation line"),
        ];
        foreach ((string title, string bodyPrefix, string commit, string expected) in cases)
        {
            DocGateResult result = DocGateRules.Evaluate(Facts(bodyPrefix + Body(), CodeAndDocs, title: title, commits: [new CommitMessage("1a2b3c4", commit)]));

            Assert.False(result.Passes, expected);
            Assert.Contains(result.Problems, problem => problem.StartsWith(expected, StringComparison.Ordinal) && problem.Contains("(T-6, D-16)", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void ABareProviderNamePasses()
    {
        // The rule is no wider than T-6. The review command, the review record, the merge
        // summary, and the handoff author field name the providers for another reason.
        const string lines = """
            - The other provider reviewed it through `make codex-review PR=5` (T-4, D-14).
            The review record names Codex as the reviewer.
            Author: Claude Code
            The merge summary names What, How, CI, and Codex review.
            The gate fails on a `Co-authored-by:` trailer and on a generation line.
            The prompt is generated by codex-review.
            """;
        CommitMessage commit = new CommitMessage("1a2b3c4", "docs: review record of #5 (PR-4)\n\n" + lines);

        DocGateResult result = DocGateRules.Evaluate(Facts(Body() + "\n## Review\n\n" + lines, CodeAndDocs, commits: [commit]));

        Assert.True(result.Passes, string.Join("\n", result.Problems));
    }

    [Fact]
    public void GatherReadsTheDiffAndTheCommittedHandoff()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        repo.Commit("chore: base", ("README.md", "base"), (DocGateRules.HandoffPath, Entry("feat/pr-69-older")));
        repo.CreateBranch(Branch);
        string head = repo.Commit("feat: sample", ("IronAbsolution.Tools/Sample.cs", "// sample"), (DocGateRules.HandoffPath, Entry(Branch)));
        File.WriteAllText(Path.Combine(repo.Root, "docs", "session-handoff.md"), Entry("feat/not-committed"));
        string bodyPath = Path.Combine(repo.Root, "body.md");
        File.WriteAllText(bodyPath, "body");

        DocGateFacts facts = DocGateFacts.Gather(repo.Root, "main", head, bodyPath, Title, Branch);

        Assert.Equal(["IronAbsolution.Tools/Sample.cs", DocGateRules.HandoffPath], facts.ChangedPaths.Order(StringComparer.Ordinal));
        Assert.NotNull(facts.NewestHandoffEntry);
        Assert.Contains(DocGateRules.BranchMark(Branch), facts.NewestHandoffEntry, StringComparison.Ordinal);
        Assert.Equal("body", facts.Body);
        Assert.Equal([new CommitMessage(head, "feat: sample\n")], facts.CommitMessages);
    }

    [Fact]
    public void AMoveGivesBothPaths()
    {
        // A code file that moves into `docs/` still counts as a code change.
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        string first = repo.Commit("chore: base", ("IronAbsolution.Tools/Sample.cs", "// the same text in both places\n"), ("docs/design.md", "design"));
        repo.Git("mv", "IronAbsolution.Tools/Sample.cs", "docs/Sample.cs");
        string second = repo.Commit("docs: move");

        IReadOnlyList<string> paths = new GitRepository(repo.Root).ChangedPaths(first, second);

        Assert.Equal(["IronAbsolution.Tools/Sample.cs", "docs/Sample.cs"], paths.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheCommandFailsOnATrailerInACommitOfTheRange()
    {
        // The command reads each commit message from the base to the head, and not the base itself.
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        repo.Commit("chore: base\n\nCo-authored-by: Base Person <base@example.invalid>", ("README.md", "base"));
        repo.CreateBranch(Branch);
        string clean = repo.Commit("feat: sample", ChangedFiles());
        string trailer = repo.Commit("fix: sample\n\nThe fix.\n\nCo-authored-by: A Person <a@example.invalid>", ("IronAbsolution.Tools/Sample.cs", "// fixed"));
        string bodyPath = Path.Combine(repo.Root, "body.md");
        File.WriteAllText(bodyPath, Body());

        DocGateFacts facts = DocGateFacts.Gather(repo.Root, "main", "HEAD", bodyPath, Title, Branch);
        (int exitCode, string output, _) = RunCommand("--root", repo.Root, "--base", "main", "--head", "HEAD", "--body", bodyPath, "--title", Title, "--branch", Branch);

        Assert.Equal([trailer, clean], facts.CommitMessages.Select(commit => commit.Sha));
        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"doc-gate: The message of commit {trailer}, line 5, holds a co-author trailer: 'Co-authored-by: A Person <a@example.invalid>'.", output, StringComparison.Ordinal);
        Assert.Contains("doc-gate: fail, 1 problem(s) over 5 changed path(s) and 2 commit(s).", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheExitCodeNamesTheOutcome()
    {
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        repo.Commit("chore: base", ("README.md", "base"));
        repo.CreateBranch(Branch);
        repo.Commit("feat: sample", ChangedFiles());
        string passing = Path.Combine(repo.Root, "passing.md");
        string failing = Path.Combine(repo.Root, "failing.md");
        File.WriteAllText(passing, Body());
        File.WriteAllText(failing, Body() + "\nA docs PR records the merge.\n");

        (int pass, string passOutput, _) = RunCommand("--root", repo.Root, "--base", "main", "--head", "HEAD", "--body", passing, "--title", Title, "--branch", Branch);
        (int fail, string failOutput, _) = RunCommand("--root", repo.Root, "--base", "main", "--head", "HEAD", "--body", failing, "--title", Title, "--branch", Branch);
        (int usage, _, string usageErrors) = RunCommand("--root", repo.Root);
        (int absentBody, _, string absentErrors) = RunCommand("--root", repo.Root, "--base", "main", "--head", "HEAD", "--body", "no-such-file.md", "--title", Title, "--branch", Branch);
        (int badRevision, _, string revisionErrors) = RunCommand("--root", repo.Root, "--base", "no-such-branch", "--head", "HEAD", "--body", passing, "--title", Title, "--branch", Branch);

        Assert.Equal(0, pass);
        Assert.Contains("doc-gate: pass, 0 problem(s)", passOutput, StringComparison.Ordinal);
        Assert.Equal(Program.FaultExitCode, fail);
        Assert.Contains("doc-gate: fail, 1 problem(s)", failOutput, StringComparison.Ordinal);
        Assert.Equal(Program.FaultExitCode, usage);
        Assert.Contains("needs each of --base", usageErrors, StringComparison.Ordinal);
        Assert.Equal(Program.FaultExitCode, absentBody);
        Assert.Contains("'no-such-file.md' does not exist", absentErrors, StringComparison.Ordinal);
        Assert.Equal(Program.FaultExitCode, badRevision);
        Assert.Contains("git merge-base no-such-branch HEAD", revisionErrors, StringComparison.Ordinal);
    }

    [Fact]
    public void ASessionNumberTooLargeForAnIntIsAFaultOfTheCommand()
    {
        // Review P2-1 of PR #5: the parse of the handoff threw an unhandled overflow.
        using TemporaryGitRepository repo = new TemporaryGitRepository();
        repo.Commit("chore: base", ("README.md", "base"));
        repo.CreateBranch(Branch);
        repo.Commit("feat: sample", (DocGateRules.HandoffPath, Entry(Branch).Replace("Session 12", "Session 99999999999999999999", StringComparison.Ordinal)));
        string bodyPath = Path.Combine(repo.Root, "body.md");
        File.WriteAllText(bodyPath, Body());

        (int exitCode, _, string errors) = RunCommand("--root", repo.Root, "--base", "main", "--head", "HEAD", "--body", bodyPath, "--title", Title, "--branch", Branch);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("'docs/session-handoff.md' has the heading 'Session 99999999999999999999'", errors, StringComparison.Ordinal);
    }

    private static (int ExitCode, string Output, string Errors) RunCommand(params string[] args)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();
        int exitCode = Program.Run([DocGateCommand.Name, .. args], output, errors);
        return (exitCode, output.ToString(), errors.ToString());
    }

    /// <summary>Gives the files of a commit that changes each path of <see cref="CodeAndDocs"/> but the tests.</summary>
    private static (string Path, string Text)[] ChangedFiles()
    {
        return
        [
            ("IronAbsolution.Tools/Sample.cs", "// sample"),
            ("docs/design.md", "design"),
            ("docs/decisions.md", "decisions"),
            ("docs/roadmaps/phase-1-engine-proof.md", "roadmap"),
            (DocGateRules.HandoffPath, Entry(Branch)),
        ];
    }

    private static DocGateFacts Facts(string body, IReadOnlyList<string> paths, string? handoffEntry = "", string title = Title, string branch = Branch, IReadOnlyList<CommitMessage>? commits = null)
    {
        return new DocGateFacts
        {
            Title = title,
            Branch = branch,
            Body = body,
            ChangedPaths = paths,
            NewestHandoffEntry = handoffEntry == string.Empty ? Entry(branch) : handoffEntry,
            CommitMessages = commits ?? [],
        };
    }

    private static string Entry(string branch)
    {
        return $"## Session 12: 2026-09-27, Claude Code\n\nAuthor: Claude Code\nSession: author PR-70, round 1. Repository: iron-absolution. Branch: `{branch}`. PR: #70. Role: author.\n\n### What is in flight\n\n- The owner merges this PR.\n";
    }

    /// <summary>Gives a description with a complete Documents section for <see cref="CodeAndDocs"/>, with each given line replaced.</summary>
    private static string Body(params (string Label, string Text)[] replacements)
    {
        Dictionary<string, string> lines = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["`docs/design.md`"] = "Changed: section 7 names the sample rule of this change.",
            ["`docs/decisions.md`"] = "Changed: D-56 records the owner answer on the sample.",
            ["`docs/questions.md`"] = "Reviewed; no change needed: no open question binds the sample change.",
            ["`docs/roadmaps/`"] = "Changed: the phase 1 roadmap marks PR-70 done in this PR.",
            ["`docs/runbooks/`"] = "Not applicable: the change touches no runbook and no machine setup step.",
            ["`docs/session-handoff.md`"] = "Changed: Session 12 describes this PR and names its branch.",
            ["`CLAUDE.md` and `AGENTS.md`"] = "Reviewed; no change needed: no command, gate, or rule of the agent files changes.",
            ["`.claude/skills/`"] = "Reviewed; no change needed: no skill procedure reads the sample change.",
        };
        foreach ((string label, string text) in replacements)
        {
            lines[label] = text;
        }

        string section = string.Concat(DocGateRules.Categories.Select(category => $"- {category.Label}: {lines[category.Label]}\n"));
        return $"## Summary\n\nThe sample change.\n\n{DocGateRules.SectionHeading}\n\n{section}";
    }
}
