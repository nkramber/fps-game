# PR-6 response

The answer of the author to the first review round of PR #6 (PR-5), at the effective head `2d0f013`.

### P2-1
- Disposition: full
- Evidence: The trigger reproduces. With `doc-gate` removed from `.github/rulesets/main.json` and `.github/workflows/doc-gate.yml` removed, the 15 ruleset tests of `2d0f013` passed. D-61 names the four checks, so no test held the decision.
- Correction: `1fe466b`, `IronAbsolution.Tests/RulesetTests.cs`, D-61. The new test `TheRulesetRequiresEachCheckOfTheDecision` asks the ruleset to require each of the four checks of D-61. The test holds the four names as a subset, not as the exact list. A later check can then join the ruleset, and PR-6 adds `review-gate`. The binding tests still require that each PR job is a required check.
- Regression check: the same trigger at `1fe466b` fails `TheRulesetRequiresEachCheckOfTheDecision`, and the other tests pass. With the committed files, `make` passes on macOS with 288 tests, a clean format, and 0 findings.

New decision ids: none. New finding ids: none.
