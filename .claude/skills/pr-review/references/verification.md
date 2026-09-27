# Verification

The `pr-review` skill names this file at step 6. It holds the checks that a review runs, and the rules for the evidence of a check.

## Verification

Run the focused checks that can falsify the changed behavior. Complete the project gates that apply.
Use the current build commands in `AGENTS.md`. Do not invent a successful command when no solution or tool exists.

- Run `make` from the checkout root. It runs the build, the tests, the format check, and ste-check (D-41).
- Read the tests as critically as the implementation.
- Verify that each bug fix has a regression test that fails on the old behavior (T-3).
- Run the regression test against the base in an isolated checkout when that is practical.
- Else, explain the causal reason that the old behavior fails the assertion, and state the limit of the run.
- Check test assertions against the contract, not against a copy of the implementation.
- Check test discovery, skipped tests, mocks, fixtures, and assertions that can pass without the intended behavior.
- Tell apart a passed check and a skipped, unavailable, failed, or author-reported check.
- Record the command, the revision, the environment, the result, and the relevant artifact of each required check.
- Verify the CI results against the reviewed revision.
- The CI of each PR runs the `ste-check`, `build, test, and format`, and `coverage report` jobs (D-42, D-45).
- A newer push to a PR cancels the older run. CI on the tip covers the effective head when each later commit changes documents alone (D-49).
- A PR of documents alone needs the ste-check job instead of the tests (the PR gate of `AGENTS.md`).
- A job that ends with a runner infrastructure annotation gives no result for the code. The author runs the failed jobs again.
- Hosted runners have no Unreal Engine. An engine PR attaches the local logs of its build, tests, and package (D-31).
- The owner runs the Windows builds, and posts the logs in the PR (D-33). Read those logs for the reviewed revision.

A check that does not exist yet has a line that names the PR that creates it (G-8).
Name the absent check and the PR that creates it. A PR that creates a check must pass it.
The clause does not excuse a failed check that exists.

Do not repeat broad suites without a new change, failure, or open risk.
Do not weaken a test or a threshold to get a pass.
Absent required evidence blocks approval. Optional evidence gaps belong in the limits of the review.
