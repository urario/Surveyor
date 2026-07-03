# AI 開発ワークフロー

Surveyor では、Claude Code を設計・レビュー寄り、Codex を実装寄りとして使う。
目的は、TDD と成果物トレーサビリティを崩さずに、要求から実装までを小さく進めること。

## 役割

| 役割 | 主担当 | 期待する成果 |
| -- | -- | -- |
| 要求整理 | Claude Code | 関連 RQ、設計論点、未決事項の整理 |
| 設計レビュー | Claude Code | 層分離、非侵襲性、決定性、機密データ扱いの確認 |
| 実装 | Codex | テスト先行の実装、検証、差分説明 |
| コードレビュー | Claude Code | バグ、回帰、テスト不足、要求漏れの指摘 |
| ナレッジ管理 | 両方 | OKF 更新、RQ と成果物のリンク維持 |

## 標準フロー

1. 要求仕様書から関連 `RQ-xxx` を特定する。
2. `main` ではなく topic branch で作業する。
3. 実装前に、必要なら `knowledge/decisions/`、`knowledge/design/`、`knowledge/process/` に判断や設計成果物を残す。
4. Codex は失敗するテストを先に追加する。
5. Codex はテストを通す最小実装を行う。
6. Codex は検証コマンドと結果を報告する。
7. Claude Code は `.claude/agents/surveyor-reviewer.md` の観点でレビューする。
8. OKF の `log.md`、該当概念、必要な `knowledge/traces/` 証跡を更新する。

## 詳細設計レビュー運用

詳細設計 PR は `knowledge/process/ai-design-review-strategy.md` の多層防御に従う。

1. 作成側は `knowledge/process/design-review-patterns.md` の `DRP-xxx` を全件セルフチェックし、PR 本文に Self-Review Evidence を記載する(Codex: `.codex/skills/surveyor-detailed-design`、Claude Code: `surveyor-design-review` のセルフチェックモード)。
2. レビューはロール別プロンプトを都度書かず、`@claude` に `surveyor-design-review` の詳細設計レビュープロトコルを指定する単一トリガーで多視点レビューを一括実行する。
3. 指摘対応では境界を変える修正(型・所有・呼び出し順)に対して `DRP-02`〜`DRP-05` を再チェックし、コントラクト差分を返信してから再レビューを依頼する。
4. 目標は設計 PR あたり再レビュー 1 往復以内。パターン外の Critical/High はカタログへ追記する。

## 工程別トレーサビリティ

工程はウォーターフォールでもアジャイルでも重視する。小さな実装スライスであっても、要件定義、アーキテクチャ設計、基本設計、詳細設計、実装、単体テスト、結合テストのどの証跡に該当するかを明示する。

- 正本ルール: `knowledge/process/lifecycle-traceability.md`
- 設計成果物: `knowledge/design/`
- 実装・単体テスト・結合テスト証跡: `knowledge/traces/`
- 成果物ID: `ADR-xxxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, `TRC-xxxx`

## OKF 参照ルール

要求・設計・開発手順・Git 運用・既存判断で迷った場合は、まず `knowledge/index.md` を読み、リンクされた OKF concept を確認してから変更・レビューを行う。

## Definition of Ready

- 関連 RQ が明示されている。
- 入力、出力、非対象が説明できる。
- UI 非依存でテスト可能な範囲が切り出されている。
- 実 Windows UI 依存の検証が必要な場合、その前提が記録されている。

## Definition of Done

- 対象 RQ と実装・テストの対応が追える。
- 必要な工程別成果物IDまたはPR証跡が残っている。
- 単体テストまたは代替検証が実行済みである。
- OKF ナレッジまたはログが必要に応じて更新され、`tools/okf/Validate-Okf.ps1` が通っている。
- RQ-048、RQ-051、RQ-052 に反する変更がない。

## ローカル資産

- Claude Code agents: `.claude/agents/`
- Claude Code skills: `.claude/skills/`
- Codex skills: `.codex/skills/`
- OKF bundle: `knowledge/`
- OKF policy: `knowledge/process/okf-policy.md`
- Lifecycle traceability: `knowledge/process/lifecycle-traceability.md`
- Git policy: `knowledge/process/git-policy.md`

Claude Code の project subagents は `.claude/agents/` に置くとプロジェクトスコープで読み込まれる。

Codex skills は `.codex/skills/` を正本とする。チームでレビュー・更新する対象は常に repo-local の skill であり、`~/.codex/skills` に置くコピーは個人環境で自動発見しやすくするための派生物として扱う。repo-local の skill を更新した場合、必要に応じて次のコマンドでユーザー環境へ再コピーする。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\codex\Install-ProjectSkills.ps1
```

## Git 運用

`main` への直コミット・直 push は禁止する。通常は `feature/`, `fix/`, `chore/`, `docs/`, `test/` の topic branch を作り、pull request で `main` に統合する。詳細は `knowledge/process/git-policy.md` を参照する。

ローカル hook を有効にする場合は次を実行する。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\git\Install-GitHooks.ps1
```
