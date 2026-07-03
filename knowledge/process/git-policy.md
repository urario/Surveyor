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

## PR 本文の記載ルール

PR 本文は [.github/pull_request_template.md](../../.github/pull_request_template.md) の節構成に従う。以下を必須とする。

- **日本語で書く。** GitHub Issue と同じ方針で、PR の概要・自己レビュー・残リスクは日本語で記述する。ただし `RQ-xxx` / `RD-xxx` / `ADR-xxxx` / `DES-xxxx` / `IMP-xxxx` / `UT-xxxx` / `IT-xxxx` / `TRC-xxxx` の識別子、Project フィールド名・選択肢、コマンド、コードは英語・原文のまま残す。
- **各節を必ず埋める。** 該当しない節は空欄にせず `N/A` と明記する。
- **自己レビューを結晶化する。** レビュアに渡す前に作成者自身が差分を通し読みし、その結果を「自己レビュー」節に所見として残す。チェックボックスを埋めるだけでなく、気付いた懸念とその解消、あえて残した判断・妥協点 (トレードオフ) を日本語で書く。最低限、要求 / 設計との突合、レイヤ境界、read-only (`RQ-048`)、決定性 (`RQ-051`)、機密性 (`RQ-052`) の観点を崩していないことを確認する。
- **実装タスクは定量品質ゲート証跡を残す。** 実装系 (`feat` / `fix` / `refactor`) の PR は、実行したコマンドと結果を「定量品質ゲート証跡」節に貼る。ゲートの定義は [Coding Standards](coding-standards.md) の `CS-01`〜`CS-10` と [DES-0008](../design/des-0008-project-structure-and-test-harness.md) に従い、少なくとも次を含める。
  - ビルド (警告=エラー: `CS-01` / `CS-05` / `CS-06` / `CS-08`)。
  - ユニットテスト件数・成否と、コア層 (Domain / Application / Policy / Reports) の行カバレッジ (`CS-07`: 80% 以上)。
  - アーキテクチャテスト (依存方向 / banned API)。
  - `dotnet format --verify-no-changes` (`CS-09`)。
  - 該当する項目が無い場合は、その項目を `N/A` と理由付きで明記する。
- ドキュメント / プロセス / ツールのみの PR (`docs` / `chore` / `test`) は、定量品質ゲート証跡のうち該当する項目 (例: OKF 検証、`test` なら該当テスト) だけを記載し、残りは `N/A` としてよい。
- PR は関連 Issue にリンクし、検証結果と残リスクを PR または Issue に残す。

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

