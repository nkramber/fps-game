---
name: ste-writing
description: Write and review text in ASD-STE100 Simplified Technical English. Load before you write any .md, skill, or agent file in this repo. Holds the process glossary and the checker rules.
---

# STE writing skill

Use this skill before you write text in this repo. The owner requires ASD-STE100 for every doc, skill, and agent file (D-17). This skill is a port of the ste-writing skill of the-thing-below (D-12). Text that the player reads in the game is exempt. It gets its own voice later.

Source: ASD-STE100 Issue 8 (2021-04-30), Part 1, Writing rules. Issue 9 (2025-01) supersedes it with the same 53 rules. The full standard is free at https://www.asd-ste100.org/. This skill gives the 53 rules in short form. It does not copy the dictionary.

## Procedure

1. Write the text.
2. Check each sentence against the checklist below.
3. Correct each sentence that fails.
4. Run `make ste-check` before each commit. The pre-commit hook runs it too (D-43).
5. Read the text again as a reader who does not know the subject.

## Checklist (the rules that fail most often)

- Max 20 words in a procedural sentence. Max 25 words in a descriptive sentence (5.1, 6.3).
- One instruction per sentence (5.2).
- Instructions in the imperative: "Load the file." Not "The file should be loaded." (5.3).
- Active voice in procedures. Active voice as much as possible in descriptions (3.6).
- No "-ing" verb forms. "Sync the data" not "Syncing the data". The rules permit an "-ing" word only in a technical name (3.5).
- No helping verbs for complex tenses: "we did", not "we have been doing" (3.4).
- Tenses allowed: infinitive, imperative, simple present, simple past, past participle as adjective, future (3.2).
- No semicolons (8.1).
- No contractions (4.2).
- Max three words in a noun cluster. Write longer names in full, then use hyphens or a short name (2.1, 2.2).
- Use "the", "a", "this" before nouns (2.3).
- One term per concept. Do not use synonyms for variety (1.11, 9.4).
- Each paragraph: one topic, max six sentences (6.5, 6.6).
- Use vertical lists for complex content (4.3).
- Start a safety note with the risk word: WARNING, CAUTION (7.1).
- Notes give information, not instructions (5.5).
- American English spelling (1.14).
- No phrasal verbs: "remove" not "take out" (9.3).
- Do not use a technical name as a verb (1.7). Write "make a backup", not "backup the data".

## The 53 rules in short form

### Section 1 - Words
- 1.1 Use only approved dictionary words, technical names, and technical verbs.
- 1.2 Use approved words only as the part of speech given.
- 1.3 Use approved words only with their approved meaning.
- 1.4 Use only approved forms of verbs and adjectives.
- 1.5 You can use words that fit a technical name category.
- 1.6 Use an unapproved word only when it is a technical name or part of one.
- 1.7 Do not use technical names as verbs.
- 1.8 Use technical names that agree with approved nomenclature.
- 1.9 Select technical names that are short and easy to understand.
- 1.10 Do not use slang or jargon as technical names.
- 1.11 Do not use different technical names for the same item.
- 1.12 You can use verbs that fit a technical verb category.
- 1.13 Do not use technical verbs as nouns.
- 1.14 Use American English spelling, unless an official directive says otherwise.

### Section 2 - Noun clusters
- 2.1 Write noun clusters of max three words.
- 2.2 Write a long technical name in full, then give a short name or use hyphens.
- 2.3 Use an article or demonstrative adjective before a noun.

### Section 3 - Verbs
- 3.1 Use only verb forms given in the dictionary.
- 3.2 Make only: infinitive, imperative, simple present, simple past, past participle as adjective, future.
- 3.3 Use the past participle only as an adjective.
- 3.4 Do not use helping verbs to make complex verb structures.
- 3.5 Use the "-ing" form only as a technical name or in a technical name.
- 3.6 Use the active voice in procedures. Use it as much as possible in descriptions.
- 3.7 Use an approved verb to describe an action, not a noun.

### Section 4 - Sentences
- 4.1 Write short and clear sentences.
- 4.2 Do not omit words or use contractions to make sentences shorter.
- 4.3 Use a vertical list for complex text.
- 4.4 Use connecting words to connect sentences with related topics.

### Section 5 - Procedures
- 5.1 Max 20 words in each sentence.
- 5.2 One instruction in each sentence, unless actions occur at the same time.
- 5.3 Write instructions in the imperative.
- 5.4 Divide a descriptive statement from the command with a comma.
- 5.5 Write notes only to give information, not instructions.

### Section 6 - Descriptions
- 6.1 Give information gradually.
- 6.2 Use key words and phrases to organize the text.
- 6.3 Max 25 words in each sentence.
- 6.4 Use paragraphs to show related information.
- 6.5 Each paragraph has only one topic.
- 6.6 No paragraph has more than six sentences.

### Section 7 - Safety instructions
- 7.1 Use a word such as "WARNING" or "CAUTION" to identify the risk level.
- 7.2 Start a safety instruction with a clear command or condition.
- 7.3 Give an explanation that shows the risk or the possible result.

### Section 8 - Punctuation and word count
- 8.1 Use all standard punctuation except the semicolon.
- 8.2 Use hyphens to connect closely related words.
- 8.3 Use parentheses for references, item identifiers, step identifiers, abbreviations, and singular/plural forms.
- 8.4 In a vertical list, a colon counts as the end of a sentence.
- 8.5 Text in parentheses counts as one word.
- 8.6 Count each number, unit, abbreviation, identifier, quoted text, and title as one word.
- 8.7 A hyphenated word counts as one word.

### Section 9 - Writing practices
- 9.1 Use a different construction when a word-for-word replacement is not enough.
- 9.2 Use each approved word correctly.
- 9.3 Do not make phrasal verbs.
- 9.4 Use a consistent style for terminology and wording.

## Technical names in this project

The rules permit these as written. They are technical names (rule 1.5):

- The working title: Iron Absolution. The project name in code: IronAbsolution (D-39). The repository: iron-absolution.
- The role models: what-you-carry, the-thing-below.
- Tools and platforms: Unreal Engine, Unreal Editor, Blueprint, Enhanced Input, World Partition, Lumen, Nanite, TSR, Temporal Super Resolution, Xcode, macOS, Windows, Apple Silicon, C++, C#, .NET, Python, Git, Git LFS, GitHub, GitHub Actions, Makefile, Meshy, Blender, Houdini, gitar.
- The two harnesses: Claude Code, Codex.
- Process terms: session handoff, decision register, questions register, PR gate, cross-provider review, review record, response file, effective head, transitional prompt, focused roadmap, exit test.
- Content terms: graybox, kit, trim sheet, texel density, pivot, collision, LOD, vertical slice, provenance.
- The standard itself: ASD-STE100, STE.
- Code identifiers in backticks.

## Glossary

One term per concept. Add a row for each term that the owner sets, with the refused synonyms. When a project area gets its own terms, put them in `references/glossary.md` of this skill.

| Term | Use for | Do not use |
|---|---|---|
| owner | the person who owns the repository and answers every question | user, maintainer |
| session | one harness invocation, bound to one PR (D-5) | run, conversation |
| provider | Anthropic or OpenAI, as the source of a harness | vendor, model |
| PR | a GitHub pull request | MR, change request |
| role model | what-you-carry or the-thing-below, as the source of the infrastructure (D-12) | reference repo, template repo |
| reference game | a game that informs feel or pacing only (D-2) | inspiration, clone target |

Process terms:

| Term | Use for | Do not use |
|---|---|---|
| decision register | `docs/decisions.md`, the owner decisions, D-1 onward | decision log |
| questions register | `docs/questions.md`, the open questions, OQ-1 onward | question list |
| session handoff | `docs/session-handoff.md`, the entry of each session, newest first | handover, notes |
| transitional prompt | the prompt that starts the next session after a merge (D-21) | handoff prompt |
| review record | `docs/reviews/pr-<number>.md`, the verdict of the cross-provider review | review file |
| response file | `docs/reviews/pr-<number>-response.md`, the answer of the author | reply |
| effective head | the newest commit that changes a path outside the documents | tip, when the text means this commit |
| Documents section | the part of the PR description with one line for each document category (D-22) | doc checklist |
| focused roadmap | one file under `docs/roadmaps/` with PR entries and exit tests | low-level plan |
| exit test | one numbered check of a roadmap entry | acceptance test, when the text means this |

## The checker

The `ste-check` command of the tools project is a port of the checker of the-thing-below (D-15, D-17). Run it with `make ste-check`. The command reads every live document that git tracks. It prints one line for each finding: the file, the line, the rule id, and what the rule saw. It exits 1 on any finding.

| Rule id | What the checker flags |
|---|---|
| STE 5.1 | More than 20 words in a sentence of a numbered list item, under any heading |
| STE 6.3 | More than 25 words in any other sentence |
| STE 8.1 | A semicolon |
| STE 4.2 | A contraction: `n't`, or a pronoun with `'s`, `'re`, `'ve`, `'ll`, `'d`, or `'m`. A possessive passes |
| STE 3.6 | Passive voice: is, are, was, were, be, been, or being, then a past participle. Two adverbs can stand between them |
| STE 3.2/3.4 | A helper verb: should, would, could, might, may, shall, ought. Also has, have, or had before a participle |
| STE 3.5 | An -ing form as the first word of a sentence, or after a preposition or a helper word |
| STE 6.6 | More than six sentences in a paragraph |
| MD 1 | An HTML comment across lines |
| REF 1 | A citation of a `D-`, `OQ-`, `F-`, `G-`, `T-`, `L-`, `M-`, or `PR-` id that no register holds |
| REF 2 | A path of this repository in backticks that no file or folder holds |
| REF 3 | A citation of a superseded decision that names no decision which superseded it |
| HANDOFF 1 to 4 | A duplicate session number, an entry out of order, more than 10 entries, or a session heading of another level |
| AGENTS 1 | A difference between `CLAUDE.md` and `AGENTS.md` |
| SIZE 1 to 3 | More than 16 KB in an agent file, 5 KB in the top handoff entry, or 36 KB in a skill file |

Dated records are exempt by path: `docs/reviews/`, `docs/session-handoff.md`, the handoff archive, and the archive folder of `docs/`. A dated record is history, and a rewrite falsifies it.

The passive and participle rules are heuristics. A past participle is an irregular form from a list, or a word that ends in "ed". "is closed" is a finding, and so is "is required". Rewrite the sentence with the actor as the subject: "the build needs the SDK". "must", "can", and "will" pass, because the standard approves them.

## The reference check

- A register defines each id. `docs/decisions.md` defines `D-`, and `docs/questions.md` defines `OQ-`. `docs/design.md` defines `F-`, `G-`, `T-`, `L-`, and `M-`.
- Section 8 of `docs/design.md` and the PR headings of the phase files define `PR-`.
- A line that names a `PR-#` marks each path of that PR. A document can name a file that a later PR creates.
- Write a name that is not a path of this repository without backticks. A branch name and a path of a role model take this rule.
- Do not cite a decision id of a role model. Cite its file instead, because the reference check reads each id as an id of this repository.

## The size rules of the context budget

A session reads the start set in full, so each file of that set has a byte limit.

| File | Limit |
|---|---|
| `CLAUDE.md` and `AGENTS.md` | 16 KB |
| The top entry of `docs/session-handoff.md` | 5 KB |
| Each `.md` file of `.claude/skills/` | 36 KB |

Move text to a skill or a runbook when a file comes near its limit.

## Markdown notes

- Tables, fenced code blocks, and front matter are exempt from every rule. Keep cell text short.
- Headings are titles. They count as one word (8.6). The checker reads no rule on a heading.
- Text in backticks, in double quotes, or in parentheses is one word (8.5, 8.6). The grammar rules do not read inside it.
- An HTML comment on one line is not prose, and the checker removes it. Keep each comment on one line.
- A numbered list item is a procedural step, under any heading. Rule 5.1 applies, max 20 words.
- A bullet list item is one unit. Rule 6.3 applies, max 25 words.
- The "plain-English" paragraphs in the design doc are descriptive text. Rule 6.3 applies.
