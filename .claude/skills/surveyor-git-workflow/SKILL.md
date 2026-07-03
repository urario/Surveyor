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
- For implementation PRs (`feat` / `fix` / `refactor`), include quantitative quality-gate evidence per `CS-01`–`CS-10` and DES-0008: build (warnings-as-errors), unit test counts/results with core-layer coverage (`CS-07` ≥ 80%), architecture tests, and `dotnet format --verify-no-changes` (`CS-09`).

## Workflow

1. Check `git status --short --branch`.
2. If on `main`, create a topic branch before making changes.
3. Review staged changes before proposing a commit message.
4. Suggest a commit subject and body that reflect the actual diff.
5. For PRs, use `.github/pull_request_template.md` and write the body in Japanese: 概要, 種別, 関連要求/成果物 (phase, requirements, artifact IDs), 自己レビュー (crystallized findings), 定量品質ゲート証跡 (required for implementation tasks), OKF/トレーサビリティ, and 残リスク.

See [Git Policy — PR 本文の記載ルール](../../../knowledge/process/git-policy.md) for the authoritative rules.

## Local Guardrails

Use `tools/git/Install-GitHooks.ps1` to configure `.githooks/` locally. Hooks block commits and pushes on `main`, but GitHub branch protection should also be configured.

