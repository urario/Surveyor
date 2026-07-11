<!--
Surveyor の PR 本文は日本語で書く。各節を必ず埋め、該当しない場合は「N/A」と書く。
識別子 (RQ-xxx / DES-xxxx など) と Project フィールド名・選択肢は英語表記のまま残す。
記載ルールの根拠: knowledge/process/git-policy.md「Pull Request Rules」
-->

## 概要

- 変更の目的と要点を日本語で書く。

## 種別

<!-- 該当するものにチェックする。実装系は「自己レビュー」と「定量品質ゲート証跡」が必須。 -->

- [ ] 実装 (feat / fix / refactor) — 自己レビュー + 定量品質ゲート証跡が必須
- [ ] テスト (test)
- [ ] ドキュメント / プロセス / ツール (docs / chore)

## 関連要求 / 成果物

- Phase: ライフサイクルフェーズ または N/A
- RQ: `RQ-xxx` または N/A
- RD: `RD-xxx` または N/A
- Artifact: `ADR-xxxx` / `DES-xxxx` / `IMP-xxxx` / `UT-xxxx` / `IT-xxxx` / `TRC-xxxx` または N/A
- 関連 Issue: #xxx または N/A

## 自己レビュー (必須)

<!-- 実施したセルフレビューの結果を日本語で「結晶化」して残す。チェックだけでなく所見を書く。 -->

- [ ] 自分の差分を通し読みした
- [ ] 要求 / 設計 (RQ・DES) との突合を確認した
- [ ] レイヤ境界 / read-only (RQ-048) / 決定性 (RQ-051) / 機密性 (RQ-052) を崩していない

所見:

- 気付いた懸念と、その解消または残置理由:
- あえて残した判断・妥協点 (トレードオフ):

## 定量品質ゲート証跡 (実装タスクは必須)

<!--
先にサマリ表で目標と実測を比較し、その後に再現用コマンドと証跡を書く。
行は削除しない。該当しない項目は判定を N/A とし、実測値または補足に理由を書く。
判定は PASS / FAIL / NOT RUN / N/A / BELOW TARGET (CS-10 の非ブロッキング基準値未達のみ) のいずれか。
ゲートの定義は knowledge/process/coding-standards.md (CS-01〜CS-10) と DES-0008 に従う。
-->

### 判定サマリ

| ゲート | 適用条件 | 目標値 | 実測値 | 判定 |
| -- | -- | -- | -- | -- |
| Build / analyzers (`CS-01`, `CS-05`, `CS-06`, `CS-08`) | 実装: 必須 | exit 0、warnings 0、errors 0、メトリクス違反 0、未宣言 public API 0 | <!-- 例: exit 0 / warnings 0 / errors 0 / CA150x 0 / RS0016 0 --> | <!-- PASS 等 --> |
| Unit tests / coverage (`CS-07`) | 実装: 必須、test: 変更範囲 | 全テスト成功、各コア層 line coverage ≥ 80% | <!-- 例: 123 passed / Domain 91.4% / Policy 94.4% --> | <!-- PASS 等 --> |
| Architecture / banned API | 実装: 必須 | 全テスト成功、依存方向違反 0、banned API 違反 0 | <!-- 件数と違反数 --> | <!-- PASS 等 --> |
| Format (`CS-09`) | 実装: 必須 | exit 0、差分 0 | <!-- exit code と差分 --> | <!-- PASS 等 --> |
| Mutation (`CS-10`) | スライス完了時または Issue 指定時 | 対象コア層 mutation score ≥ 80% | <!-- 対象層とスコア。未実施なら理由 --> | <!-- PASS / BELOW TARGET / NOT RUN / N/A --> |

### 実行コマンド / 証跡

| ゲート | 実行コマンド | 証跡 / 補足 |
| -- | -- | -- |
| Build / analyzers | `N/A` | <!-- ログ要約、artifact URL、または N/A 理由 --> |
| Unit tests / coverage | `N/A` | <!-- テスト内訳、coverage report、または N/A 理由 --> |
| Architecture / banned API | `N/A` | <!-- テスト内訳または N/A 理由 --> |
| Format | `N/A` | <!-- 結果または N/A 理由 --> |
| Mutation | `N/A` | <!-- 対象層、score、trace / follow-up Issue、または N/A 理由 --> |
| Other | `N/A` | <!-- その他の定量・手動確認 --> |

## OKF / トレーサビリティ

- OKF 更新: 対象ファイル または N/A
- `tools/okf/Validate-Okf.ps1`: 結果 または N/A

## 残リスク

- 残る手動確認・未決事項。なければ「None known」。
