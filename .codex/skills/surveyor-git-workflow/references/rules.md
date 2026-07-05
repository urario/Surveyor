# Surveyor Git Rules

## Hard Rules

- No direct commits on `main`.
- No direct pushes to `main`.
- No force-pushes to `main`.
- No history rewriting of published branches without explicit agreement.

## Normal Flow

```powershell
git switch main
git pull --ff-only
git switch -c chore/example-topic
# edit, test, stage
git commit
git push -u origin chore/example-topic
```

Then open a pull request into `main`.

## GitHub Issue / Project Work

- Issue タイトルと本文は日本語を基本にする。
- Project フィールド名と選択肢は英語固定にする。
- 作業前に関連 Issue の `Status`, `Phase`, `Artifact`, `RQ`, `RD`, `Owner Role` を確認する。
- PR には関連 Issue をリンクし、検証結果と残リスクを残す。
- 詳細は `knowledge/process/github-issue-project-workflow.md` に従う。

## PR Evidence

PR 本文は日本語で書き、`.github/pull_request_template.md` の節を全て埋める。該当しない節は `N/A` と明記する。識別子・Project フィールド名・コマンド・コードは原文のまま残す。

Include:

- 概要: 変更の目的と要点 (日本語)
- 種別: 実装 / テスト / ドキュメント・プロセス
- Phase: lifecycle phase or "N/A"
- Requirements: `RQ-xxx` / `RD-xxx` IDs or "N/A"
- Artifacts: `ADR-xxxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, `TRC-xxxx`, PR-only, or "N/A"
- 自己レビュー: 作成者自身の差分通し読み結果を結晶化する。所見 (見つけた懸念と解消、あえて残したトレードオフ) を日本語で書き、要求/設計との突合、レイヤ境界、read-only (`RQ-048`)、決定性 (`RQ-051`)、機密性 (`RQ-052`) を確認する。
- 定量品質ゲート証跡 (実装タスクは必須): `CS-01`–`CS-10` と DES-0008 に基づく実行コマンドと結果。ビルド (警告=エラー)、ユニットテスト件数・成否とコア層カバレッジ (`CS-07` ≥ 80%)、アーキテクチャテスト (依存方向 / banned API)、`dotnet format --verify-no-changes` (`CS-09`)。`CS-10` を実行した場合は Stryker の実行コマンド、対象レイヤ、スコア、trace へのリンクを入れる。80% 未満でも baseline は非ブロッキングのため、PR では改善候補か follow-up Issue を明記する。該当なしは理由付きで `N/A`。
- OKF: files updated (`tools/okf/Validate-Okf.ps1` の結果) or "N/A"
- Risk: residual manual validation or "None known"

詳細な記載ルールは `knowledge/process/git-policy.md`「PR 本文の記載ルール」に従う。

## Local Hook Installation

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\git\Install-GitHooks.ps1
```

Hooks are local guardrails. GitHub branch protection should also require PRs into `main`.
