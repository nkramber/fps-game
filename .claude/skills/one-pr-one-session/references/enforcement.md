# The enforcement of each rule

The lifecycle of a PR has rules, and each rule has an enforcement (D-5, D-12). A machine enforces a rule, or the agent does, or the owner does. Some rules are not observable, and the table says so.

Read this table when a question asks who catches a break of a rule. A row that names a later PR describes the state after that PR merges. Until then, the agent and the owner hold that rule (G-8).

| Rule | Enforcement | Mechanism |
|---|---|---|
| The PR changes `docs/session-handoff.md`, and the newest entry names the PR branch | Machine | `doc-gate` (D-57) |
| The documents matrix gives each category exactly one line, with a disposition and a reason of five words or more | Machine | `doc-gate` (D-57) |
| Each matrix line agrees with the changed paths | Machine | `doc-gate` (D-57) |
| The description and the newest handoff entry put no documents off to later work | Machine, by a fixed list of phrases | `doc-gate` (D-57) |
| No title or branch names a merge record of an earlier PR | Machine | `doc-gate` (D-5, D-57) |
| Each document follows STE, each cited id and path resolves, and no live document cites a superseded decision as current | Machine | The `ste-check` job and the pre-commit hook (D-17, D-43) |
| The agent files, each skill file, and the top handoff entry stay under their byte limits | Machine | The SIZE rules of `ste-check` (D-17) |
| `CLAUDE.md` and `AGENTS.md` stay identical | Machine | The AGENTS 1 rule of `ste-check` (D-12) |
| Each session number is new, and the entries stay newest first | Machine | The HANDOFF rules of `ste-check`. The session fetches first (L-2) |
| The handoff keeps 10 entries, and each older entry moves to the archive with its text intact | Machine, when the session runs the command | HANDOFF 3 of `ste-check` finds the eleventh entry. `make handoff-rotate` moves it and puts an entry out of place back in order (D-58, D-59) |
| No commit lands on `main` or on no branch | Machine | The pre-commit hook (D-43). After PR-5, the ruleset of `main` too |
| The handoff and the review record do not move the work head, and a documents commit does not move the effective head | Machine | `make codex-review` reads the metadata set (D-14) and the documents set (D-49) |
| A session reads the newest handoff entry, looks up register ids in one command, and waits on checks with one command | Agent | The read order of `AGENTS.md`, this skill, and `docs/runbooks/session-context.md` |
| A reviewer loads `pr-review`, and an author who answers findings loads `review-response` | Agent | The skill descriptions and `AGENTS.md` (D-46) |
| A review round starts only when each start condition holds, with no unresolved review thread and an effective head | Machine, when the author runs the command | `make codex-review` refuses with exit 3 (D-14, D-49, D-52) |
| A review round pushes a record of the effective head, and no commit outside the metadata set | Machine | `make codex-review` fails the round with exit 1 (D-14, D-49) |
| A review round stops after 90 minutes | Machine | `make codex-review` fails the round with exit 1 (D-50) |
| A PR with no code merges without a review record only through the owner label | Machine for the paths and the time of the label, owner for the label | `review-gate` reads the `review-override` label and the changed paths (D-35, D-65, D-66). No machine proves that the owner added the label (F-9) |
| A P0 to P2 finding open in three rounds stops the fix loop, and the owner decides | Machine for the stop, owner for the answer | `make codex-review` exits 11 from the `Open at:` lines (D-14) |
| A PR merges only with every required check green, every review thread resolved, and a squash merge | Machine after PR-5 | The ruleset of `main` in `.github/rulesets/main.json` (D-61). `RulesetTests` binds each required check to one job |
| No session uses the admin bypass of the ruleset | Agent and owner | The bypass is for the owner alone, and no session merge passes `--admin` (D-5, D-60) |
| A merge needs an approving record of the effective head, or the owner label | Machine | The required `review-gate` check, from the tool of the base branch (D-64, D-67) |
| No review round uses API pricing | Machine | `make codex-review` removes the API credential variables and uses the ChatGPT login (D-14, D-53) |
| The owner confirms each merge after the merge summary: What, How, CI, and Codex review | Agent and owner | `references/review-and-merge.md` asks with `AskUserQuestion`. No machine reads the summary or the confirmation |
| Auto-merge starts only after the owner confirms the merge | Agent and owner, then machine | The session turns on auto-merge after the confirmation (D-5, D-12). The ruleset then holds the merge until each required check is green |
| No commit or PR carries an attribution line | Machine for the trailers and the generation lines, agent and owner for the text | `.claude/settings.json` sets the attribution to empty text (T-6, D-16). `doc-gate` refuses a co-author trailer, a generation line, or a robot line in the title, the description, or a commit (D-57) |
| A reason is true and specific | Agent and owner | The author writes it, and the cross-provider review checks it |
| The design doc, the registers, and the roadmap agree with the PR | Agent | The author, then the cross-provider review |
| A session starts clean and works on one PR | Agent and owner | The start gate of this skill. The owner starts a new session for each PR |
| A session starts no other PR after the hand-over, and it stops at the merge. It can answer the findings of its own PR after the hand-over | Agent and owner | The closing line of this skill. The owner starts the session of the next PR |
| The provider that wrote a commit | Not observable | Both providers push as one GitHub account (F-9). The handoff author field is the evidence |
| The identity of a session, and whether a context came from a compaction or a fork | Not observable | The harness exposes no session id, and the repository defines none |

An agent rule and an owner rule have no machine that catches a break. A session that skips one costs the next session its time. The cross-provider review reads each agent rule of the PR in front of it (T-4).
