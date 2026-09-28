# Runbook: the ruleset of main

Status: procedure, written 2026-09-27. Written in ASD-STE100 (D-17). The source is the runbook of the same name in what-you-carry (D-12).

The file `.github/rulesets/main.json` holds the ruleset of `main`. The live ruleset on GitHub matches the file. `RulesetTests` binds the file to the workflows. Each job of a PR workflow is a required check, and each required check is the name of one job (D-61).

## What the ruleset holds

- The branch `refs/heads/main`, with the enforcement `active`.
- Squash merges alone, and resolved conversations.
- No required approval, because one person owns the repository.
- No extra approval for a commit of an unlinked author, and no required reviewers. GitHub adds both fields with other values when the file omits them.
- Five required checks from the GitHub Actions app (id 15368): `ste-check`, `build, test, and format`, `coverage report`, and `doc-gate` (D-61), and `review-gate` (D-64).
- No rule for the newest `main` on the branch, because the PRs go one at a time.
- No linear history rule, because the squash-only rule keeps the history linear (D-62).
- One bypass: the repository admin role, through a PR merge alone (D-60).
- No deletion of `main`, and no force push to it.

An extra approval for a commit of an unlinked author blocks each merge, because the one owner cannot give it. The file declares that field as false, so the comparison stays empty.

No workflow has a path filter, so each PR job reports on a documents head and on a code head. A required check that never reports blocks every merge.

The `review-gate` job runs on the event `pull_request_target`, from the file on `main` (D-64). The PR that adds or changes that file gets no check from its own version of the file.

## The bypass

The bypass is for the owner alone (D-60). No session uses it (D-5).

- A session never runs `gh pr merge` with the `--admin` option.
- A session merges only when each required check is green and no review thread stays open.

## Procedure: the first setup

The author session of PR-5 does these steps on the explicit instruction of the owner (D-63). It uses the file of the PR head, before the merge.

1. Confirm that the review record approves the effective head.
2. Confirm that the checks of the PR head are green.
3. Create the ruleset from the file of the PR head.
4. Compare the live ruleset with the file. Set `ref` to the PR branch.

```
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
ref=origin/feat/pr-5-main-ruleset
git fetch origin
ruleset=$(mktemp "${TMPDIR:-/tmp}/main-ruleset.XXXXXX")
git show "$ref:.github/rulesets/main.json" > "$ruleset"
gh api --method POST "repos/$repo/rulesets" --input "$ruleset" --jq '{id, name, enforcement}'
```

Each procedure sets its own variables and writes its own temporary files with `mktemp`. A fixed path can hold a file of another session, and the upload then sends that file.

## Procedure: a change of the ruleset

1. Change `.github/rulesets/main.json` and the workflow jobs in one PR.
2. Run `make`. It runs `RulesetTests`.
3. Get the approval of the owner for the update of the live ruleset.
4. Update the live ruleset from the file on `main` after the merge.
5. Compare the live ruleset with the file.

Only the owner changes the live ruleset, or a session on an explicit instruction of the owner (D-63).

```
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
git fetch origin main
ruleset=$(mktemp "${TMPDIR:-/tmp}/main-ruleset.XXXXXX")
git show origin/main:.github/rulesets/main.json > "$ruleset"
id=$(gh api "repos/$repo/rulesets" --jq '.[] | select(.name == "main") | .id')
gh api --method PUT "repos/$repo/rulesets/$id" --input "$ruleset" --jq '{id, name, enforcement}'
```

CAUTION: A PR that renames or removes a required job cannot merge under the old ruleset. The old check never reports on its head. Update the live ruleset from the file of the PR head before that merge, with `ref` set to the PR branch.

A PR that adds a required check updates the live ruleset after the merge. A check that no workflow of `main` has yet can block the merge of the PR that adds it. PR-6 took this order for `review-gate` (D-67).

## Procedure: the review gate and auto-merge (D-67)

PR-6 adds `review-gate` to the file. The live ruleset takes it after the merge of PR-6.

1. Merge PR-6 under the four checks of D-61.
2. Get the explicit instruction of the owner for the update of the live ruleset (D-63).
3. Update the live ruleset from the file on `main`, with the procedure above.
4. Compare the live ruleset with the file. Set `ref` to `origin/main`.
5. The owner makes the label `review-override` one time.
6. The owner turns on auto-merge in the repository settings.

Steps 5 and 6 change the repository, so the owner does them, or a session on the explicit instruction of the owner. These are the commands:

```
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
gh label create review-override --repo "$repo" --color B60205 --description "A PR with no code merges with no review record. The author session adds it (D-35, D-76)"
gh api --method PATCH "repos/$repo" -F allow_auto_merge=true --jq '{allow_auto_merge}'
```

The first PR after PR-6 proves the gate on GitHub. Its `review-gate` check is red before its review record, and green after an approving record.

## Compare the live ruleset with the file

The API adds fields that the file does not hold, such as the id and the dates. Compare the parts that the file holds. Set `ref` to `origin/main`, or to the PR branch in the first setup.

```
repo=$(gh repo view --json nameWithOwner --jq .nameWithOwner)
ref=origin/main
git fetch origin
work=$(mktemp -d "${TMPDIR:-/tmp}/main-ruleset.XXXXXX")
git show "$ref:.github/rulesets/main.json" > "$work/main-ruleset.json"
id=$(gh api "repos/$repo/rulesets" --jq '.[] | select(.name == "main") | .id')
keys='{name, target, enforcement, conditions, bypass_actors, rules: (.rules | sort_by(.type))}'
gh api "repos/$repo/rulesets/$id" | jq -S "$keys" > "$work/live.json"
jq -S "$keys" "$work/main-ruleset.json" > "$work/file.json"
diff "$work/file.json" "$work/live.json" && echo "The live ruleset matches the file."
```

An empty diff proves the match. A field that GitHub adds with a default value shows in the diff. Put that field and its value in the file, so the next comparison is empty.
