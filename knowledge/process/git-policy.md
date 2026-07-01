---
type: Process
title: Git Policy
description: Branch, commit, pull request, and protection rules for Surveyor development.
tags: [process, git, github, review]
timestamp: 2026-07-01T00:00:00+09:00
---

# Policy

Surveyor uses pull-request based development. Direct commits and direct pushes to `main` are prohibited after project initialization.

# Branch Rules

- `main` is protected as the integration branch.
- Create a topic branch before making project changes.
- Use short kebab-case branch names with a purpose prefix:
  - `feature/<topic>`
  - `fix/<topic>`
  - `chore/<topic>`
  - `docs/<topic>`
  - `test/<topic>`
- Do not force-push `main`.
- Do not rewrite published history unless explicitly agreed.

# Commit Rules

- Use focused commits with one coherent reason for change.
- Prefer Conventional Commit style:
  - `feat: ...`
  - `fix: ...`
  - `test: ...`
  - `docs: ...`
  - `chore: ...`
  - `refactor: ...`
- Mention relevant `RQ-xxx` IDs in the commit body when the change implements or verifies requirement behavior.
- Include verification evidence in the PR, not necessarily in every commit.

# Pull Request Rules

- Use PRs for all changes into `main`.
- Include lifecycle phase, requirement links, artifact IDs where applicable, test evidence, OKF updates, and residual risk in the PR description.
- Request Claude Code review for design/review-heavy changes and use Codex for implementation/test follow-through.
- Merge only after review concerns are resolved or explicitly accepted.

# GitHub Issue / Project Rules

- GitHub Issue は日本語で書く。Project フィールド名と選択肢は英語で固定する。
- 作業開始前に関連 Issue の `RQ`, `RD`, `Artifact`, `Phase`, `Status` を確認する。
- PR は関連 Issue にリンクし、Issue または PR に検証結果と残リスクを残す。
- Issue / Project の詳しい運用は [GitHub Issue and Project Workflow](github-issue-project-workflow.md) に従う。

# Local Guardrails

The repository includes Git hooks under `.githooks/` that block commits and pushes while on `main`.

Install them locally with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\git\Install-GitHooks.ps1
```

These hooks are local guardrails, not a substitute for GitHub branch protection. Configure GitHub branch protection for `main` to require pull requests before merge.

