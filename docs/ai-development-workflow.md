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
2. 実装前に、必要なら `knowledge/decisions/` または `knowledge/process/` に判断を残す。
3. Codex は失敗するテストを先に追加する。
4. Codex はテストを通す最小実装を行う。
5. Codex は検証コマンドと結果を報告する。
6. Claude Code は `.claude/agents/surveyor-reviewer.md` の観点でレビューする。
7. OKF の `log.md` と該当概念を更新する。

## Definition of Ready

- 関連 RQ が明示されている。
- 入力、出力、非対象が説明できる。
- UI 非依存でテスト可能な範囲が切り出されている。
- 実 Windows UI 依存の検証が必要な場合、その前提が記録されている。

## Definition of Done

- 対象 RQ と実装・テストの対応が追える。
- 単体テストまたは代替検証が実行済みである。
- OKF ナレッジまたはログが必要に応じて更新されている。
- RQ-048、RQ-051、RQ-052 に反する変更がない。

## ローカル資産

- Claude Code agents: `.claude/agents/`
- Claude Code skills: `.claude/skills/`
- Codex skills: `.codex/skills/`
- OKF bundle: `knowledge/`
- OKF policy: `knowledge/process/okf-policy.md`

Claude Code の project subagents は `.claude/agents/` に置くとプロジェクトスコープで読み込まれる。

Codex skills は `.codex/skills/` を正本とする。チームでレビュー・更新する対象は常に repo-local の skill であり、`~/.codex/skills` に置くコピーは個人環境で自動発見しやすくするための派生物として扱う。repo-local の skill を更新した場合、必要に応じて次のコマンドでユーザー環境へ再コピーする。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\codex\Install-ProjectSkills.ps1
```
