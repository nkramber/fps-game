using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IronAbsolution.Tools.CodexReview;
using IronAbsolution.Tools.HandoffRotate;

namespace IronAbsolution.Tools.DocGate;

/// <summary>The outcome of the documents gate: whether the PR passes, and one line for each problem (T-2).</summary>
/// <param name="Passes">True when the PR has no problem.</param>
/// <param name="Problems">Each problem, with the place and the rule.</param>
public sealed record DocGateResult(bool Passes, IReadOnlyList<string> Problems);

/// <summary>One required line of the Documents section: the label of the PR template, and the paths of that category.</summary>
/// <param name="Label">The label, as the PR template writes it.</param>
/// <param name="Paths">Each path of the category. A path that ends with a slash covers each path under it.</param>
public sealed record DocumentCategory(string Label, IReadOnlyList<string> Paths);

/// <summary>
/// The rules of the documents gate (D-5, D-22, D-57). A PR carries its own handoff entry, and
/// a Documents section that gives each category one line that agrees with the diff. No text
/// of the PR puts documents off to later work, and no PR only records an earlier merge. No
/// title, description, or commit message names an agent, a harness, or a model as the source
/// of the work (T-6, D-16). The rules read text and paths alone, and they do no I/O.
/// </summary>
public static class DocGateRules
{
    /// <summary>The name of the command and of the job.</summary>
    public const string JobName = "doc-gate";

    /// <summary>The handoff file. It is in the metadata set, so its commit never moves the work head (D-14).</summary>
    public const string HandoffPath = HandoffRotateRules.HandoffPath;

    /// <summary>The heading of the Documents section of the PR description (D-22).</summary>
    public const string SectionHeading = "## Documents";

    /// <summary>The disposition of a category that the PR changes.</summary>
    public const string Changed = "Changed:";

    /// <summary>The disposition of a category that the PR can affect and does not change.</summary>
    public const string NoChangeNeeded = "Reviewed; no change needed:";

    /// <summary>The disposition of a category that the PR cannot reach.</summary>
    public const string NotApplicable = "Not applicable:";

    /// <summary>The fewest words of a reason. A shorter reason cannot name a document and a cause.</summary>
    public const int MinimumReasonWords = 5;

    /// <summary>The required categories, in the order of the PR template (D-22).</summary>
    public static readonly IReadOnlyList<DocumentCategory> Categories =
    [
        new DocumentCategory("`docs/design.md`", ["docs/design.md"]),
        new DocumentCategory("`docs/decisions.md`", ["docs/decisions.md"]),
        new DocumentCategory("`docs/questions.md`", ["docs/questions.md"]),
        new DocumentCategory("`docs/roadmaps/`", ["docs/roadmaps/"]),
        new DocumentCategory("`docs/runbooks/`", ["docs/runbooks/"]),
        new DocumentCategory("`docs/session-handoff.md`", [HandoffPath, HandoffRotateRules.ArchivePath]),
        new DocumentCategory("`CLAUDE.md` and `AGENTS.md`", ["CLAUDE.md", "AGENTS.md"]),
        new DocumentCategory("`.claude/skills/`", [".claude/skills/"]),
    ];

    /// <summary>Text that puts documents off to later work, or that names a PR which only records an earlier one (D-5).</summary>
    private static readonly Regex[] DeferralPatterns =
    [
        new Regex(@"\b(follow-?up|later|next|second|separate|another) (docs|documentation) PR\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
        new Regex(@"\b(docs|documentation) PR (records|will record|to record|that records|after)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
        new Regex(@"\b(follows|comes|lands) in a (docs|documentation) PR\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
        new Regex(@"\bwill update\b[^.]{0,40}\b(docs?|documents?|documentation|design|roadmaps?|handoff|decisions|questions|register|skills?)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
        new Regex(@"\b(update|record|write)\b[^.]{0,40}\b(after the merge|after merge|in a later PR)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
        new Regex(@"\b(TBD|TODO)\b", RegexOptions.CultureInvariant),
    ];

    /// <summary>A title or a branch of a PR that records the merge of an earlier PR (D-5).</summary>
    private static readonly Regex MergeRecordPattern = new Regex(
        @"merge[- ]record|\brecord the (PR-\d+ )?merge\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// The forms of attribution (T-6, D-16): a co-author trailer, a generation line that names a
    /// tool of a provider, and the robot line that some tools add. A bare name passes, because
    /// the review command, the review record, the merge summary, and the handoff author field
    /// name the providers for another reason. The lookahead keeps the command name
    /// `codex-review` out of the generation line. The robot is U+1F916, as a UTF-16 pair.
    /// </summary>
    private static readonly (string Form, Regex Pattern)[] AttributionPatterns =
    [
        ("a co-author trailer", new Regex(@"^\s*co-authored-by:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
        ("a generation line", new Regex(@"\bgenerated\s+(with|by)\s+[\[`*_]*(Claude|Codex|ChatGPT|GPT|OpenAI|Anthropic|Copilot|Gemini|Cursor|Aider|Devin)\b(?!-review)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
        ("a robot line", new Regex(@"^\s*🤖", RegexOptions.CultureInvariant)),
    ];

    private static readonly Regex HtmlComment = new Regex(@"<!--.*?-->", RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private static readonly string[] GenericReasons = ["no documentation impact", "no docs impact"];

    /// <summary>Applies every rule to the facts of one PR.</summary>
    /// <param name="facts">The facts of the PR.</param>
    /// <returns>The outcome, with one line for each problem.</returns>
    public static DocGateResult Evaluate(DocGateFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        List<string> problems = [];
        if (MergeRecordPattern.IsMatch(facts.Title) || MergeRecordPattern.IsMatch(facts.Branch))
        {
            problems.Add($"The title '{facts.Title}' or the branch '{facts.Branch}' names a merge record. A PR carries its own documents, and no PR records an earlier merge (D-5).");
        }

        CheckHandoff(facts, problems);

        // A hidden comment of the template is no line of the Documents section.
        string body = HtmlComment.Replace(facts.Body, string.Empty);
        CheckSection(body, facts.ChangedPaths, problems);
        CheckDeferral("the PR description", body, problems);
        if (facts.NewestHandoffEntry is not null)
        {
            CheckDeferral("the newest handoff entry", facts.NewestHandoffEntry, problems);
        }

        // The attribution rule reads the raw description, because a hidden comment is still text of the PR.
        CheckAttribution("The PR title", facts.Title, problems);
        CheckAttribution("The PR description", facts.Body, problems);
        foreach (CommitMessage commit in facts.CommitMessages)
        {
            CheckAttribution($"The message of commit {commit.Sha}", commit.Message, problems);
        }

        return new DocGateResult(problems.Count == 0, problems);
    }

    /// <summary>Gives the first session entry of the handoff text, or null when the text holds none.</summary>
    /// <param name="handoffText">The text of the handoff.</param>
    /// <returns>The text of the first entry, from its heading to the next heading.</returns>
    /// <exception cref="InvalidOperationException">A session number is too large for an int.</exception>
    public static string? NewestHandoffEntry(string handoffText)
    {
        ArgumentNullException.ThrowIfNull(handoffText);

        // The handoff has no title, so its first line is the newest heading (D-20).
        IReadOnlyList<HandoffEntry> entries = HandoffRotateRules.Parse(handoffText, HandoffPath).Entries;
        return entries.Count == 0 ? null : entries[0].Text;
    }

    /// <summary>Gives the mark of the branch in the session line of a handoff entry (D-20).</summary>
    /// <param name="branch">The branch of the PR.</param>
    /// <returns>The text `Branch: `, then the branch in backticks.</returns>
    public static string BranchMark(string branch)
    {
        return $"Branch: `{branch}`";
    }

    private static void CheckHandoff(DocGateFacts facts, List<string> problems)
    {
        if (!facts.ChangedPaths.Contains(HandoffPath))
        {
            problems.Add($"The PR does not change {HandoffPath}. Each PR carries its own handoff entry (D-5).");
            return;
        }

        if (facts.NewestHandoffEntry is null)
        {
            problems.Add($"{HandoffPath} at the head holds no '## Session <number>:' entry (D-20).");
            return;
        }

        string branchMark = BranchMark(facts.Branch);
        if (!facts.NewestHandoffEntry.Contains(branchMark, StringComparison.Ordinal))
        {
            problems.Add($"The newest handoff entry does not name {branchMark}. The entry must describe the work of this PR (D-5, D-20).");
        }
    }

    private static void CheckSection(string body, IReadOnlyList<string> changedPaths, List<string> problems)
    {
        List<string> lines = SectionLines(body);
        if (lines.Count == 0)
        {
            problems.Add($"The PR description has no '{SectionHeading}' section with lines. The section gives one line for each category (D-22).");
            return;
        }

        foreach (DocumentCategory category in Categories)
        {
            string prefix = $"- {category.Label}:";
            List<string> matches = lines.FindAll(candidate => candidate.StartsWith(prefix, StringComparison.Ordinal));
            if (matches.Count == 0)
            {
                problems.Add($"The Documents section has no line for {category.Label} (D-22).");
                continue;
            }

            if (matches.Count > 1)
            {
                problems.Add($"The Documents section has {matches.Count} lines for {category.Label}. A category has one line, because two lines make its disposition ambiguous (D-22).");
                continue;
            }

            string text = matches[0][prefix.Length..].Trim();
            string? disposition = Array.Find([Changed, NoChangeNeeded, NotApplicable], value => text.StartsWith(value, StringComparison.Ordinal));
            if (disposition is null)
            {
                problems.Add($"The line for {category.Label} does not start with '{Changed}', '{NoChangeNeeded}', or '{NotApplicable}': '{text}' (D-22).");
                continue;
            }

            string reason = text[disposition.Length..].Trim();
            int words = reason.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            if (words < MinimumReasonWords || Array.Exists(GenericReasons, generic => reason.Contains(generic, StringComparison.OrdinalIgnoreCase)))
            {
                problems.Add($"The line for {category.Label} gives a generic reason, or fewer than {MinimumReasonWords} words: '{reason}'. Name the part of the document and the cause (D-22).");
            }

            bool changed = HasChangedPath(category, changedPaths);
            if (disposition == Changed && !changed)
            {
                problems.Add($"The line for {category.Label} says '{Changed}', and the diff changes no path of that category.");
            }

            if (disposition != Changed && changed)
            {
                problems.Add($"The line for {category.Label} says '{disposition}', and the diff changes a path of that category.");
            }
        }
    }

    /// <summary>Gives each line that starts with "- " between the Documents heading and the next "## " heading.</summary>
    private static List<string> SectionLines(string body)
    {
        List<string> result = [];
        bool inSection = false;
        foreach (string rawLine in body.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                inSection = line.Trim() == SectionHeading;
                continue;
            }

            if (inSection && line.StartsWith("- ", StringComparison.Ordinal))
            {
                result.Add(line);
            }
        }

        return result;
    }

    private static bool HasChangedPath(DocumentCategory category, IReadOnlyList<string> changedPaths)
    {
        foreach (string changedPath in changedPaths)
        {
            foreach (string path in category.Paths)
            {
                bool matches = path.EndsWith('/') ? changedPath.StartsWith(path, StringComparison.Ordinal) : changedPath == path;
                if (matches)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Adds one problem for each line of the text that holds a form of attribution, with the source and the line number.</summary>
    private static void CheckAttribution(string source, string text, List<string> problems)
    {
        string[] lines = text.Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index].TrimEnd('\r');
            foreach ((string form, Regex pattern) in AttributionPatterns)
            {
                if (pattern.IsMatch(line))
                {
                    problems.Add($"{source}, line {index + 1}, holds {form}: '{line.Trim()}'. No title, description, or commit names an agent, a harness, or a model as the source of the work (T-6, D-16).");
                }
            }
        }
    }

    private static void CheckDeferral(string source, string text, List<string> problems)
    {
        foreach (Regex pattern in DeferralPatterns)
        {
            Match match = pattern.Match(text);
            if (match.Success)
            {
                problems.Add($"{char.ToUpperInvariant(source[0])}{source[1..]} defers documents to later work: '{match.Value}'. The PR carries all of its documents (D-5).");
            }
        }
    }
}
