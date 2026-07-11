# Surveyor Git Rules

## Hard Rules

- No direct commits on `main`.
- No direct pushes to `main`.
- No force-pushes to `main`.
- No history rewriting of published branches without explicit agreement.

## Normal Flow

```powershell
git switch main
git fetch origin
git pull --ff-only
git switch -c chore/example-topic
# edit, test, stage
git commit
git push -u origin chore/example-topic
```

Then open a pull request into `main`.

If the topic branch edits high-churn coordination files such as `knowledge/log.md`, refresh it from `origin/main` again before the final push or merge-readiness handoff.

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
- 定量品質ゲート証跡 (実装タスクは必須): テンプレートの標準行を削除せず、最初の表で `適用条件` / `目標値` / `実測値` / `判定` を比較可能にし、次の表で再現コマンドと evidence / trace を示す。`判定` は `PASS` / `FAIL` / `NOT RUN` / `N/A` / `BELOW TARGET` のみ。未計測を `PASS` にせず、`N/A` / `NOT RUN` には理由を書く。
- ゲート内容: `CS-01`–`CS-10` と DES-0008 に基づくビルド (警告・エラー・メトリクス・公開 API)、ユニットテスト件数・成否と各コア層カバレッジ (`CS-07` ≥ 80%)、アーキテクチャテスト (依存方向 / banned API)、`dotnet format --verify-no-changes` (`CS-09`)。`CS-10` を実行した場合は Stryker の実行コマンド、対象レイヤ、スコア、trace へのリンクを入れる。80% 未満は `BELOW TARGET` (非ブロッキング) とし、改善候補か follow-up Issue を明記する。
- OKF: files updated (`tools/okf/Validate-Okf.ps1` の結果) or "N/A"
- Risk: residual manual validation or "None known"

詳細な記載ルールは `knowledge/process/git-policy.md`「PR 本文の記載ルール」に従う。

## Local Hook Installation

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\git\Install-GitHooks.ps1
```

Hooks are local guardrails. GitHub branch protection should also require PRs into `main`.
