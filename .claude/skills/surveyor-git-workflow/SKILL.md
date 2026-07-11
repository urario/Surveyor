---
name: surveyor-git-workflow
description: Enforce Surveyor Git workflow rules including no direct main commits, topic branches, PR evidence, and safe commit messages.
---

# Surveyor Git Workflow

Use this skill for branch creation, staging, commit message proposals, PR preparation, or review of Git operations.

## Rules

- Never commit directly on `main`.
- Never push directly to `main`.
- Create a topic branch before edits: `feature/`, `fix/`, `chore/`, `docs/`, or `test/`.
- Keep commits focused and use Conventional Commit style.
- Preserve `RQ-xxx` traceability in commit bodies or PR descriptions for requirement-bearing work.
- Preserve lifecycle phase and artifact IDs in PR descriptions when durable evidence exists.
- Run relevant tests or validation scripts before proposing a PR.
- Update OKF when a change creates durable project knowledge.
- Write PR descriptions in Japanese, following `.github/pull_request_template.md`; fill every section and mark non-applicable ones as `N/A`. Keep identifiers (`RQ-xxx`, `DES-xxxx`, ...), Project field names, commands, and code in their original form.
- Crystallize a self-review in the "自己レビュー" section: record concerns found and how they were resolved or left, plus deliberate trade-offs — not just checkboxes. Confirm requirement/design alignment, layer boundaries, read-only (`RQ-048`), determinism (`RQ-051`), and confidentiality (`RQ-052`).
- For implementation PRs (`feat` / `fix` / `refactor`), preserve every row in the template's quantitative-gate tables. Fill `適用条件`, `目標値`, `実測値`, and `判定` before the command/evidence table so a human can judge the gate without reading raw logs. Use only `PASS`, `FAIL`, `NOT RUN`, `N/A`, or `BELOW TARGET`; never call missing measurements `PASS`, and give a reason for `N/A` / `NOT RUN`.
- Include build/analyzer counts, unit test counts and per-core-layer coverage (`CS-07` ≥ 80%), architecture/banned API results, format result (`CS-09`), and conditional mutation evidence (`CS-10`) per `CS-01`–`CS-10` and DES-0008.
- When `CS-10` is in scope, use `knowledge/process/stryker-workflow.md` as the canonical restore/run/evidence flow. Include the exact Stryker command, the target layers, the recorded score, and the trace link in the PR. Scores below 80% are non-blocking baseline evidence unless the governing Issue says otherwise, but the PR must still name the surviving-mutant concentration and improvement candidates or follow-up Issue.

## Workflow

1. Check `git status --short --branch`.
2. If on `main`, create a topic branch before making changes.
3. Review staged changes before proposing a commit message.
4. Suggest a commit subject and body that reflect the actual diff.
5. For PRs, use `.github/pull_request_template.md` and write the body in Japanese: 概要, 種別, 関連要求/成果物 (phase, requirements, artifact IDs), 自己レビュー (crystallized findings), 定量品質ゲート証跡 (target/actual/result summary first, reproducible commands second), OKF/トレーサビリティ, and 残リスク.

See [Git Policy — PR 本文の記載ルール](../../../knowledge/process/git-policy.md) for the authoritative rules.

## Local Guardrails

Use `tools/git/Install-GitHooks.ps1` to configure `.githooks/` locally. Hooks block commits and pushes on `main`, but GitHub branch protection should also be configured.
