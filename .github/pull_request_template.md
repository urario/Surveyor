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
実装系 PR は、実行したコマンドと結果を貼る。該当しない項目は N/A。
ゲートの定義は knowledge/process/coding-standards.md (CS-01〜CS-10) と DES-0008 に従う。
-->

- ビルド (警告=エラー: CS-01 / CS-05 / CS-06 / CS-08): 
- ユニットテスト (件数・成否, コア層カバレッジ % — CS-07 は 80% 以上): 
- アーキテクチャテスト (依存方向 / banned API): 
- `dotnet format --verify-no-changes` (CS-09): 
- その他 (メトリクス CS-06 / 手動確認など): 

## OKF / トレーサビリティ

- OKF 更新: 対象ファイル または N/A
- `tools/okf/Validate-Okf.ps1`: 結果 または N/A

## 残リスク

- 残る手動確認・未決事項。なければ「None known」。
