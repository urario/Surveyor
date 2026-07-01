---
type: Design Review
title: DES-0007 Multi-Perspective Expert Review
description: Independent multi-perspective expert review of the DES-0007 detailed-design execution strategy across 11 reviewer roles, with structured findings, severity, disposition tracking, and OKF relationship to the reviewed strategy.
resource: ../../docs/gui-testability-analyzer-requirements.md
tags: [design-review, detailed-design, execution-strategy, multi-perspective, traceability, rq-048, rq-051, rq-052, rq-054]
timestamp: 2026-07-01T00:00:00+09:00
---

# DES-0007 Multi-Perspective Expert Review

This is an **independent expert review** of the detailed-design execution strategy
[DES-0007 Detailed Design Phase Execution Strategy](des-0007-detailed-design-execution-strategy.md).
It does not modify the reviewed artifact. It records findings from 11 reviewer perspectives
so the strategy can be corrected (or its residual risks explicitly carried) before the
`DES-0008`–`DES-0017` detailed-design packages and their implementation begin.

Scope of this review is the **execution strategy itself** — package topology, ordering,
trace rules, artifact template, unit-test intent strategy, and residual-risk closure map —
not the downstream packages (which do not yet exist).

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | DES-0007 Multi-Perspective Expert Review, review evidence for the detailed-design phase strategy |
| Upstream (reviewed) | [DES-0007](des-0007-detailed-design-execution-strategy.md) (review target — not modified) |
| Upstream (context) | [DES-0001](../architecture/des-0001-initial-architecture.md); [DES-0002](des-0002-module-responsibility-basic-design.md); [DES-0003](des-0003-module-interface-basic-design.md); [DES-0004](des-0004-analysis-flow-basic-design.md); [DES-0005](des-0005-vmodel-traceability-and-downstream-tests.md); [DES-0006](des-0006-screen-basic-design.md); [ADR-0003](../decisions/adr-0003-review-surface-native-vs-html.md); [Requirement Definition](../requirements/requirements-definition.md); guardrails `RQ-048`, `RQ-051`, `RQ-052`, `RQ-054` |
| Downstream (obligations raised) | Corrections or accepted-with-risk dispositions folded into `DES-0007` and/or the planned `DES-0008`–`DES-0017`; possible new `ADR-0002` (adapter technology), spike notes, and additional `UT`/`IT` obligations recorded in [DES-0005](des-0005-vmodel-traceability-and-downstream-tests.md) |
| Evidence | 11 perspective narratives; 31 structured findings with severity, disposition, and suggested tests; consolidated acceptance-criteria and human-review rollups |
| Verification | `tools/okf/Validate-Okf.ps1`; `git diff --check`; detailed-design gate of [Quality Review Policy](../process/quality-review-policy.md) |
| Residual Risk | This review evaluates the strategy document only. It cannot verify not-yet-written packages, spikes, or code. |

## How To Read This Review

**Statement typing** — every finding separates:

- **事実 (Fact)**: directly supported by a cited passage in the reviewed or upstream OKF documents.
- **推測 (Inference)**: a reasoned consequence not stated in the documents.
- **提案 (Proposal)**: a recommended change; owned by the disposition owner, not asserted as required.
- **未確認 (Unconfirmed)**: a premise this review needs but which the existing documents do not establish.

**Severity** — `Critical` (blocks a guardrail / core correctness), `High` (material risk, correct in-phase),
`Medium` (should fix, can be carried with a named risk), `Low` (polish / hygiene).

**Disposition owner (対応先)** — `Cowork` (this Claude design/review session), `Claude Code` (agent implementation/design drafting), `Codex` (agent implementation), `人間レビュー` (human decision required).

**対応状況 (Disposition)** — set per finding: `未対応` / `対応中` / `対応済` / `却下` / `保留`. Update the value in each finding block and in the summary dashboard as findings are resolved.

## Summary Dashboard

Legend for 対応状況: ☐ 未対応 / ◑ 対応中 / ☑ 対応済 / ✕ 却下 / ⏸ 保留.

| ID | 観点 | 重大度 | 優先 | 対応先 | 対応状況 |
| -- | -- | -- | -- | -- | -- |
| R-ARC-01 | ソフトウェアアーキテクト | High | P1 | 人間レビュー / Cowork | ☑ |
| R-ARC-02 | ソフトウェアアーキテクト | High | P1 | 人間レビュー | ☑ |
| R-ARC-03 | ソフトウェアアーキテクト | Medium | P2 | Cowork | ☑ |
| R-ARC-04 | ソフトウェアアーキテクト | Low | P3 | Cowork | ☑ |
| R-WIN-01 | レガシーWindows/MFC/Win32/C++ | High | P1 | 人間レビュー / Cowork | ☑ |
| R-WIN-02 | レガシーWindows/MFC/Win32/C++ | High | P2 | 人間レビュー | ◑ |
| R-WIN-03 | レガシーWindows/MFC/Win32/C++ | High | P1 | Cowork | ☑ |
| R-WIN-04 | レガシーWindows/MFC/Win32/C++ | Medium | P2 | Cowork | ☑ |
| R-NET-01 | C#/.NET設計者 | Critical | P1 | Cowork / 人間レビュー | ◑ |
| R-NET-02 | C#/.NET設計者 | High | P1 | Cowork | ☑ |
| R-NET-03 | C#/.NET設計者 | Medium | P2 | Cowork | ☑ |
| R-GTA-01 | GUIテスト自動化 | High | P1 | Cowork / 人間レビュー | ☑ |
| R-GTA-02 | GUIテスト自動化 | Medium | P2 | Cowork | ☑ |
| R-GTA-03 | GUIテスト自動化 | Medium | P3 | 人間レビュー | ◑ |
| R-QA-01 | 品質保証・テスト設計 | Medium | P2 | Cowork | ☑ |
| R-QA-02 | 品質保証・テスト設計 | Medium | P2 | Cowork | ☑ |
| R-QA-03 | 品質保証・テスト設計 | Low | P3 | Cowork | ☑ |
| R-OPS-01 | DevOps/CI | High | P1 | 人間レビュー / Cowork | ☑ |
| R-OPS-02 | DevOps/CI | Medium | P2 | Cowork | ☑ |
| R-OPS-03 | DevOps/CI | Medium | P2 | 人間レビュー | ☑ |
| R-MNT-01 | 保守開発リーダー | High | P2 | Cowork | ☑ |
| R-MNT-02 | 保守開発リーダー | Medium | P3 | Cowork | ☑ |
| R-IMP-01 | 実装担当者 | High | P1 | Cowork | ☑ |
| R-IMP-02 | 実装担当者 | Medium | P2 | Cowork | ☑ |
| R-PM-01 | プロジェクトマネージャ | High | P1 | 人間レビュー | ☑ |
| R-PM-02 | プロジェクトマネージャ | Medium | P2 | 人間レビュー | ☑ |
| R-PM-03 | プロジェクトマネージャ | Medium | P3 | 人間レビュー | ☑ |
| R-SEC-01 | セキュリティ・運用 | High | P1 | Cowork | ☑ |
| R-SEC-02 | セキュリティ・運用 | Medium | P2 | 人間レビュー | ☑ |
| R-AI-01 | Claude Code/Codex活用 | Medium | P2 | Cowork | ☑ |
| R-AI-02 | Claude Code/Codex活用 | Medium | P2 | Cowork | ☑ |
| R-AI-03 | Claude Code/Codex活用 | Low | P3 | Cowork | ☑ |

Severity totals: Critical 1 / High 12 / Medium 14 / Low 4 (31 findings).

---

## 1. ソフトウェアアーキテクト

**総評.** 戦略は Clean Architecture の依存方向とポート境界を尊重し、「純粋・決定的なコア設計を Windows アダプタより先行させる」という順序判断が明確で妥当(§4, §5)。ただしパッケージ集合がモジュール `M01`–`M13` を完全には覆っておらず、**Composition Root/DI 詳細設計** と **横断的な診断/ロギング設計** に持ち主がいない。また RSK-RD-001 の解消手段である「spike / 候補 ADR-0002」がプロセスとして定義されていない。

**良い点(事実).**
- コア先行の根拠が明示(§2「Surveyor's central risk is ... trace drift」/§4 の "Why this order")。
- パッケージごとに upstream・primary tests・順序理由を表化(§4)。
- ガードレール `RQ-048/051/052/054` をレビューチェックリストに固定(§9)。

**懸念点.** パッケージ間依存が線形順序でしか表現されず、並行可能性・ブロッキング関係が不明。M13(合成ルート)と横断診断の設計帰属が空白。

**重大な抜け漏れ.** Composition Root/DI 詳細設計パッケージ(R-ARC-01)、spike/ADR プロセス(R-ARC-02)。

**実装時に詰まりそうな箇所.** どのアダプタ実装が spike 未了でブロックされるかが ADR 化されておらず、着手可否の判断が属人的になる。

**テスト不足になりそうな箇所.** M13 の wiring smoke test(DES-0005 で言及)に対応する詳細設計・受入条件がないため、DI 誤配線(read-only アダプタ・`IClock`・`IConfidentialityPolicy` の注入漏れ)を捕捉するテスト意図が未定義。

**追加すべき設計記述.** パッケージ依存 DAG(並行/直列)、合成ルート詳細設計、診断/ロギング境界。

**追加すべき受入条件.** 「各 `DES-xxxx` は担当モジュール `Mnn` を明記し、M01–M13 のうち詳細設計未割当のモジュールを残さない」。

**Claude Code / Codex に渡せる作業.** M01–M13 と DES-0008–0017 の被覆マトリクス生成、依存 DAG の Mermaid 化(Cowork/Claude Code)。

**人間レビューが必須な論点.** spike をいつ ADR-0002 に昇格させるか(技術選定の権限者)。

### R-ARC-01 — Composition Root / DI 詳細設計パッケージが不在
- **観点**: ソフトウェアアーキテクト
- **指摘内容**: DES-0007 §4 の計画パッケージ `DES-0008`–`DES-0017` は M04〜M12 系を覆うが、**M13 合成ルート(プロバイダ選択・ライフタイム・スコープ・read-only/`IClock`/`IConfidentialityPolicy` の一貫注入)** の詳細設計を明示的に担うパッケージがない。DES-0008 は「solution/project layout, dependency rules」までで DI 構成の詳細設計を宣言していない。
- **根拠**: [事実] DES-0002 M13 は「dependency wiring, provider selection, lifetime/scoping」を負い、DES-0005 は M13 に "wiring smoke test" を割り当てる。[事実] DES-0007 §4 の10パッケージに M13 を主対象とする行がない。[推測] このままだと DI 設計がどこかのパッケージに暗黙吸収され、`RQ-054`(合成ルートが唯一の具象↔抽象の継ぎ目)の担保箇所が曖昧になる。
- **影響**: DI 誤配線(例: 実クロックが混入し `RQ-051` 破壊、ポリシー未注入で `RQ-052` バイパス)が設計レベルで検出されない。
- **重大度**: High / **優先**: P1
- **修正案(提案)**: DES-0008 に「Composition Root / DI 詳細設計」節を追加するか、`DES-0018`(合成ルート)を新設。プロバイダ選択キー、注入不変条件(read-only アダプタのみ・単一 `IClock`・単一ポリシー)を明記。
- **追加確認事項**: M13 を DES-0008 に含めるか独立パッケージにするか(未確認)。
- **追加すべきテスト**: 合成ルート不変条件テスト(禁止: 状態変更アダプタの登録/実クロック注入/ポリシー欠落)。
- **対応先**: 人間レビュー / Cowork ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md` §4, `des-0002-...md` M13, `des-0005-...md`
- **対応状況**: ☐ 未対応

### R-ARC-02 — spike / ADR-0002 昇格プロセスが未定義
- **観点**: ソフトウェアアーキテクト
- **指摘内容**: §8 は RSK-RD-001 の closure を「`DES-0014`, `DES-0015`, possible `ADR-0002`」とするが、spike の入口/出口条件、判断軸の合否基準、ADR-0002 への昇格トリガが定義されていない。§5 末尾も「wait for the relevant spike/decision」と述べるのみ。
- **根拠**: [事実] §8 の closure 列に "possible ADR-0002"、§5 に "Adapter-bound implementation should wait for the relevant spike/decision"。[事実] リポジトリに ADR-0002 は未存在(index に ADR-0001/0003 のみ)。[推測] spike の合否基準がないと、UIA ライブラリ/capture API の選定が「なんとなく決定」になり後戻りリスク。
- **影響**: 技術選定の再現性・説明責任が弱く、アダプタ実装着手のゲートが機能しない。
- **重大度**: High / **優先**: P1
- **修正案(提案)**: §5 or §8 に spike 定義(比較軸: read-only 実現性/決定性/fixture 化容易性/権限・整合性/packaging/性能 は既出、これに測定手順と合否閾値を追加)と「候補が確定したら ADR-0002 を起票」というトリガを明記。
- **追加確認事項**: 技術選定の最終承認者(未確認)。
- **追加すべきテスト**: N/A(プロセス指摘)。ただし spike 成果に対する再現手順の記録を受入条件化。
- **対応先**: 人間レビュー ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md` §5/§8
- **対応状況**: ☐ 未対応

### R-ARC-03 — 横断的な診断/ロギング/観測性の設計帰属が空白
- **観点**: ソフトウェアアーキテクト
- **指摘内容**: DES-0004 は "run-level diagnostics" を各段で生成するが、DES-0007 のパッケージ集合に診断/ロギング/観測性(何を・どの粒度で・どこに残すか)を担う詳細設計がない。これは `RQ-052`(ログ経由の機密漏えい, R-SEC-01 参照)とも直結する。
- **根拠**: [事実] DES-0004 各 Stage が diagnostics を produce。[事実] DES-0007 §4/§6 のテンプレートに diagnostics/logging 節がない。[推測] 診断設計が各パッケージに散逸し一貫性を欠く。
- **影響**: 診断の一貫性欠如、機密サニタイズ漏れの温床。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: DES-0011(orchestration)へ診断モデル節を追加、または横断節を §6 テンプレートに追加。
- **追加すべきテスト**: 診断/ログに raw sensitive text が入らないことを検証する UT(R-SEC-01 と共有)。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §4/§6, `des-0004-...md`
- **対応状況**: ☐ 未対応

### R-ARC-04 — パッケージ依存が線形順序のみで並行性が不明
- **観点**: ソフトウェアアーキテクト
- **指摘内容**: §4 は "Order" 列で1..10の直列順を与えるが、実際は独立に進められるパッケージ(例: DES-0009 純粋コア と DES-0013 機密ポリシー)がある。並行可能/直列必須の区別がない。
- **根拠**: [事実] §4 は単一 Order 列のみ。[推測] PM/実装が不要に直列化し、または誤って依存を跨ぐ。
- **影響**: スケジューリング最適化不能(PM 観点 R-PM-01 と関連)。
- **重大度**: Low / **優先**: P3
- **修正案(提案)**: 依存 DAG を Mermaid 化し「並行可能集合」を注記。
- **追加すべきテスト**: N/A。
- **対応先**: Cowork ／ **優先**: P3 ／ **関連ファイル**: `des-0007-...md` §3/§4
- **対応状況**: ☐ 未対応

---

## 2. レガシーWindows / MFC / Win32 / C++ 有識者

**総評.** 「custom-drawn / non-HWND regions」を残余リスクとして繰り返し保持している点は健全(§8)。しかし対象がレガシー Windows GUI(MFC/Win32)である以上、**アナライザ自身の DPI-awareness**、**UIA COM のアパートメント/スレッド**、**MFC 特有の取得現実(MSAA プロキシ・owner-draw・MDI・windowless コントロール)**、**PrintWindow の黒画面/レイヤードウィンドウ失敗**は「一般的 custom UI」に丸めず、DES-0014/0015 のスコープに個別列挙すべき。

**良い点(事実).** capture/acquisition を live Windows testing 前に fixture 分離する方針(§1, §7)。`Unavailable(reason)` を low score と区別(§7 UT-0004/§8)。

**懸念点.** レガシー固有の取得・座標・キャプチャ失敗様態が抽象化されすぎ。

**重大な抜け漏れ.** アナライザ own DPI manifest(R-WIN-01)、UIA スレッド/アパートメント(R-WIN-02)、MFC 固有取得(R-WIN-03)。

**実装時に詰まりそうな箇所.** PMv2 非対応のまま高 DPI ターゲットへ PrintWindow → 座標/bounds ずれ、UIA 同期 COM を async 化する際のデッドロック。

**テスト不足になりそうな箇所.** レガシー common controls / owner-draw / MDI child の fixture が UT-0004 の "fixture tree" 前提に含まれるか不明。

**追加すべき設計記述.** DES-0014 に「レガシー取得エッジ表」、DES-0015 に「capture 失敗様態表(黒画面/占有/レイヤード/DWM)」。

**追加すべき受入条件.** 「アナライザプロセスは Per-Monitor-V2 DPI aware を明示し、bounds/DPI メタデータは対象の DPI コンテキストで正規化される」。

**Claude Code / Codex に渡せる作業.** レガシー取得エッジ表と capture 失敗様態表のドラフト(Cowork)。

**人間レビューが必須な論点.** UIA ライブラリ(生 COM vs FlaUI)と MSAA フォールバックの実機挙動確認は spike + 人間判断。

### R-WIN-01 — アナライザ自身の DPI-awareness(PMv2)が設計決定として未記載
- **観点**: レガシーWindows/Win32
- **指摘内容**: DES-0015 は「DPI/multi-monitor/occlusion behavior, overlay coordinate mapping」を扱うが、**Surveyor プロセス自身の DPI awareness(Per-Monitor V2)** を明示的な設計決定として挙げていない。アナライザが system-DPI-aware だと、高 DPI/混在 DPI 環境で PrintWindow 出力・`BoundingRect`・オーバレイ座標(SCR-06)が系統的にずれる。
- **根拠**: [事実] DES-0007 §4 DES-0015 行は "DPI/multi-monitor/occlusion" を挙げるが自プロセスの DPI モードに言及なし。[事実] DES-0006 SCR-06 はオーバレイを capture DPI/bounds メタデータ駆動とする。[推測] 自プロセスの DPI コンテキスト未定義だと座標変換の基準が不定になり、`RQ-027`/決定性(`RQ-051`)に影響。
- **影響**: スナップショット対応付けの座標不整合、決定性の環境依存。
- **重大度**: High / **優先**: P1
- **修正案(提案)**: DES-0015 スコープに「アナライザは PMv2 DPI aware(app.manifest/自動化)。全 bounds は対象ウィンドウの DPI コンテキストで正規化し、メタデータに DPI スケールを保持」を追加。
- **追加確認事項**: 対象が DPI 非対応/system-aware の場合の仮想化座標の扱い(未確認)。
- **追加すべきテスト**: IT-0003 に「混在 DPI(例 100%/150%)でのオーバレイ座標一致」を追加。
- **対応先**: 人間レビュー / Cowork ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md` §4(DES-0015), `des-0006-...md` §6
- **対応状況**: ☐ 未対応

### R-WIN-02 — UIA COM のアパートメント/スレッドと同期呼び出しの async 化戦略が未定義
- **観点**: レガシーWindows/Win32(C#/.NET と共有)
- **指摘内容**: DES-0003 は取得ポートを `async` + `CancellationToken` とするが、UIA(COM)/MSAA 呼び出しは同期・ブロッキングで、アパートメント(STA/MTA)とクロスプロセス marshaling の設計が DES-0014 スコープに明示されていない。大規模レガシーツリーでは marshaling コストとキャンセル応答性が問題になる。
- **根拠**: [事実] DES-0003 契約規約で全長時間境界は async。[事実] DES-0007 §4 DES-0014 行は "UIA client choice, MSAA fallback" までで threading/apartment を挙げない。[推測] 同期 COM を `Task.Run` で包むだけだと STA 要件・キャンセル伝播・タイムアウトで詰まる。
- **影響**: デッドロック/フリーズ、キャンセル不能、性能未達(`RQ-050`)。
- **重大度**: High / **優先**: P2
- **修正案(提案)**: DES-0014 に「UIA スレッドモデル(専用 STA/MTA、キャンセル/タイムアウトの協調)」を必須節として追加。
- **追加すべきテスト**: IT-0006(大規模ツリー)にキャンセル応答時間・タイムアウト→`Timeout` status を追加。
- **対応先**: 人間レビュー ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §4(DES-0014), `des-0003-...md`
- **対応状況**: ☐ 未対応

### R-WIN-03 — MFC/Win32 固有の取得現実が DES-0014 スコープに個別列挙されていない
- **観点**: レガシーWindows/MFC
- **指摘内容**: 対象がレガシー GUI にもかかわらず、MSAA/IAccessible プロキシ経由のレガシー common controls、owner-draw/CustomDraw、MDI/SDI 子、windowless(HWND を持たない)コントロール、`WM_GETTEXT` 系テキストなどの取得現実が「custom UI」に丸められている。これらは identifiability/operability 評価軸の入力可用性を大きく左右する。
- **根拠**: [事実] DES-0002 M06 は UIA read + optional MSAA fallback を負う。[事実] DES-0007 §8 は "Custom-drawn/non-HWND regions incomplete" を残余リスクとして保持するが、DES-0014 スコープ列は具体列挙がない。[推測] fixture が近代的 UIA ツリー前提だとレガシー取得の穴を UT が検出しない(R-GTA-02/R-WIN と関連)。
- **影響**: レガシー対象で誤って `Unavailable` 過小/過大、評価軸入力の欠落。
- **重大度**: High / **優先**: P1
- **修正案(提案)**: DES-0014 に「レガシー取得エッジ表(MSAA プロキシ/owner-draw/MDI/windowless/`WM_GETTEXT`)」を追加し、各々の confidence/`Unavailable` 方針を規定。
- **追加すべきテスト**: UT-0004 の fixture に「MSAA のみ/owner-draw/MDI child」ケースを追加。IT-0002 で実 MFC fixture app に対する取得。
- **対応先**: Cowork ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md` §4/§8(DES-0014), `des-0002-...md` M06
- **対応状況**: ☐ 未対応

### R-WIN-04 — PrintWindow の黒画面/レイヤード/DWM 失敗様態が capture エッジに未記載
- **観点**: レガシーWindows/Win32
- **指摘内容**: DES-0015 は "PrintWindow vs Graphics Capture" を open にするが、PrintWindow(`PW_RENDERFULLCONTENT` 無し/DWM/GPU 描画/レイヤードウィンドウ)で黒画面や部分欠落が起きる既知様態が edge-case として明記されていない。これらは `Unavailable(reason)` で明示すべき対象。
- **根拠**: [事実] DES-0007 §4 DES-0015 行は "image format, uncapturable markers" を挙げるが失敗様態の列挙がない。[事実] DES-0006 SCR-06 は uncapturable を「hidden ではなく marked」と規定。[推測] 失敗様態未定義だと黒画面を正常画像として保存し得る。
- **影響**: 機密でない誤キャプチャ/信頼できないスナップショット、ユーザ誤解。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: DES-0015 に capture 失敗様態表を追加し、検出時は `Unavailable(reason)`。
- **追加すべきテスト**: IT-0003 に「レイヤード/GPU 描画ウィンドウ → `Unavailable` マーク」。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §4(DES-0015), `des-0006-...md` §6
- **対応状況**: ☐ 未対応

---

## 3. C# / .NET 設計者

**総評.** 決定性・キャンセル・ポート/DTO の抽象化は .NET の良作法に沿う。ただし決定性を最重要ガードレール(`RQ-051`)に据えながら、**.NET 固有の非決定要因**(プロセスごとにランダム化される `String.GetHashCode()`、カルチャ依存フォーマット、`System.Text.Json` のプロパティ順序無保証)への防御が戦略に明記されていない。DES-0008 が対象フレームワーク/言語設定/決定性ビルド設定を宣言していないのも痛い。

**良い点(事実).** Result vs exception 方針(DES-0003)、`IClock` による時刻決定性(UT-0010)、fake/fixture 前提。

**懸念点.** 決定性の .NET 実装レベルの落とし穴が UT 意図に反映されていない。

**重大な抜け漏れ.** 文字列ハッシュのランダム化(R-NET-01)、プロジェクト/ビルド決定性設定(R-NET-02)。

**実装時に詰まりそうな箇所.** キー導出で既定 `GetHashCode` を使い、実行ごとにキー/順序が変わる。数値/日付の `ToString()` がカルチャ依存で golden が環境ぶれ。

**テスト不足になりそうな箇所.** 「同一入力→同一バイト」を**別プロセス/別カルチャ**で検証する UT が UT-0001/0006 に含意されているか不明。

**追加すべき設計記述.** DES-0008 に TFM・`<Nullable>`・`<InvariantGlobalization>`・`<Deterministic>`・分析器設定。DES-0009/0012 に「安定ハッシュ(例 SHA-256)を使用し `Object.GetHashCode` に依存しない」。

**追加すべき受入条件.** 「キー・順序・シリアライズはプロセス再起動とカルチャ変更に対して不変」。

**Claude Code / Codex に渡せる作業.** 決定性ガード付きプロジェクトテンプレート、`InvariantCulture` シリアライズ規約のドラフト(Cowork/Codex)。

**人間レビューが必須な論点.** ハッシュアルゴリズムと衝突処理の確定(セキュリティと機密の交点、R-SEC と関連)。

### R-NET-01 — `String.GetHashCode()` のプロセス間ランダム化により決定性が崩れる罠が禁止されていない
- **観点**: C#/.NET
- **指摘内容**: `RQ-051`(同一入力→同一出力)を核とするが、.NET Core/5+ の `String.GetHashCode()` は既定でプロセスごとにランダム化される。DES-0009 のキー導出や DES-0012 の順序付けが既定ハッシュ/`Dictionary` 反復順に依存すると、**別実行でキー・並び順・出力バイトが変化**する。UT-0001 の "same stable input gives same key" は同一プロセス内では通っても再起動間で破れ得る。
- **根拠**: [事実] DES-0007 §7 UT-0001/UT-0006 は決定性を主張するが「別プロセスでの不変」を明記しない。[事実] DES-0002 M04 は ScreenKey/ElementKey 導出を負い、DES-0003 は「順序は構造走査→キー、ハッシュ反復順ではない」と規定(方針は正しいが実装罠への警告なし)。[推測] 既定ハッシュ使用は容易に混入し、CI と実行間で golden が不安定化。
- **影響**: 決定性ガードレール `RQ-051` の実質破綻、比較/回帰(`RQ-031`)不能、golden churn。
- **重大度**: Critical / **優先**: P1
- **修正案(提案)**: DES-0009/§7 に「キー材料・順序・タイブレークは安定ハッシュ(SHA-256 等)/序数比較(`StringComparison.Ordinal`)のみを用い、`Object.GetHashCode`/`Dictionary` 反復順に依存しない」を明記。UT-0001/UT-0006 に「別プロセス実行でバイト一致」を追加。
- **追加確認事項**: fallback キー用ハッシュは非可逆性(機密)要件と共通化するか(R-SEC-01/DES-0013 と交点)。
- **追加すべきテスト**: UT-0001「新プロセスでキー再計算 → 同値」、UT-0006「別プロセス+別カルチャで JSON バイト一致」。
- **対応先**: Cowork / 人間レビュー ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md` §4/§7, `des-0002-...md` M04
- **対応状況**: ☐ 未対応

### R-NET-02 — 対象フレームワーク/決定性ビルド/グローバリゼーション設定が DES-0008 に未割当
- **観点**: C#/.NET
- **指摘内容**: DES-0008 は「solution/project layout, namespaces, test projects」までで、TFM、`<Nullable>enable`、`<InvariantGlobalization>`、`<Deterministic>`、分析器/警告レベルといった**決定性・品質に直結するプロジェクト設定**を決定事項として挙げていない。
- **根拠**: [事実] DES-0007 §4 DES-0008 行のスコープに上記設定がない。[推測] カルチャ既定/nullable 無効のまま実装が進むと、数値・日付フォーマットが環境依存化し `RQ-051` を脅かす(R-NET-03 と連動)。
- **影響**: 環境依存の golden ぶれ、null 由来欠陥、決定性ビルド不成立。
- **重大度**: High / **優先**: P1
- **修正案(提案)**: DES-0008 スコープに「TFM/Nullable/InvariantGlobalization/Deterministic/分析器設定」を明記。
- **追加すべきテスト**: ビルド設定検証(smoke)+ カルチャ切替下の UT-0006。
- **対応先**: Cowork ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md` §4(DES-0008)
- **対応状況**: ☐ 未対応

### R-NET-03 — JSON シリアライザ選定とプロパティ順序/カルチャ決定性が未固定
- **観点**: C#/.NET
- **指摘内容**: DES-0012 は "deterministic serialization" を掲げるが、`System.Text.Json` はプロパティ出力順を明示制御しない限り保証せず、数値/浮動小数/日付はカルチャ・丸めで揺れる。シリアライザ選定と順序・数値フォーマット規約が戦略/テンプレートに固定されていない。
- **根拠**: [事実] DES-0007 §4 DES-0012 は "JSON schema/version, stable ordering, timestamp format" を挙げるが、シリアライザ実装制約(順序制御/`InvariantCulture`/丸め)を明記しない。[推測] 実装ごとに順序/フォーマットがぶれ UT-0006 golden が不安定。
- **影響**: バイト非安定、スキーマ検証の揺れ。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: DES-0012 に「明示的プロパティ順序、`InvariantCulture`、固定小数フォーマット、UTF-8 no-BOM、改行正規化」を規定。
- **追加すべきテスト**: UT-0006 に数値/日付/順序の固定化アサート。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §4(DES-0012)
- **対応状況**: ☐ 未対応

---

## 4. GUIテスト自動化 有識者

**総評.** キーと `DisplayLabel` の分離、`Unavailable`≠低スコアという原則は自動化可否判定の勘所を押さえている。ただし本ツールの分析核心である **「UIA プロパティ/パターンの可用性 → 評価軸」への写像** が DES-0010/DES-0014 の必須スコープとして要求されていない。また virtualized/遅延ツリーの過小報告への配慮がない。

**良い点(事実).** AutomationId 的な stable identity をキーに、Name を volatile として分離(DES-0002 M04)。改善候補に「UIA/IAccessible 実装」助言(DES-0002 M08)。

**懸念点.** 評価軸が抽象的で、UIA 実プロパティ(AutomationId 有無・ControlType・対応パターン・IsKeyboardFocusable・bounds 安定性)への具体写像が未要求。

**重大な抜け漏れ.** 軸↔UIA プロパティ写像(R-GTA-01)。

**実装時に詰まりそうな箇所.** virtualized list / lazy load での要素欠落、ツリーのタイミング依存。

**テスト不足になりそうな箇所.** 「即自動化」と判定した画面を実自動化フレームワークで裏取りするクロスチェックがない。

**追加すべき設計記述.** DES-0010/0014 に軸↔UIA プロパティ写像表、virtualization(ItemContainer)方針。

**追加すべき受入条件.** 「各評価軸は具体的 UIA/MSAA プロパティ・パターンの可用性に定義づけられ、欠落は根拠付きで findings 化される」。

**Claude Code / Codex に渡せる作業.** 軸↔プロパティ写像のドラフト(Cowork)。

**人間レビューが必須な論点.** 分類結果の妥当性を実自動化で校正するか(コスト対効果)。

### R-GTA-01 — 評価軸と UIA/MSAA プロパティの写像が必須スコープに含まれていない
- **観点**: GUIテスト自動化
- **指摘内容**: identifiability/operability/result-determinability 等の評価軸は、実務上「AutomationId の有無、ControlType、対応 UIA パターン(Invoke/Value/Toggle 等の**存在**=可読性/自動化容易性の指標)、IsKeyboardFocusable、bounds 安定性」といった具体プロパティに落ちる。DES-0007 は軸を列挙するが、DES-0010(scoring)/DES-0014(acquisition)に「軸↔プロパティ写像を定義せよ」という obligation を課していない。これは分析器の判定妥当性の核心。
- **根拠**: [事実] DES-0007 §4 DES-0010 は "evaluation axes, formulas, thresholds" を挙げるが軸のプロパティ根拠を要求しない。[事実] DES-0002 M08 の軸は `RQ-017`〜`RQ-023` に紐づくが UIA プロパティ写像は未言及。[推測] 写像未定義だと scoring が恣意的になり、`UT-0002` が「閾値=実装定数」型のトートロジー(§7 が避けよと述べる smell)に陥りやすい。
- **影響**: 判定の妥当性・説明可能性が担保されず、専門家校正(DES-0017)の土台が弱い。
- **重大度**: High / **優先**: P1
- **修正案(提案)**: DES-0010 に「各軸の入力=具体 UIA/MSAA プロパティ・パターンの可用性」写像表を必須化。DES-0014 は取得側でその可用性を confidence 付きで供給。
- **追加すべきテスト**: UT-0002 に「特定プロパティ欠落 fixture → 対応軸の finding 発生」ケース群。
- **対応先**: Cowork / 人間レビュー ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md` §4(DES-0010/0014), `des-0002-...md` M08
- **対応状況**: ☐ 未対応

### R-GTA-02 — virtualized/遅延 UIA サブツリーの過小報告がエッジケースに含まれていない
- **観点**: GUIテスト自動化
- **指摘内容**: UIA では仮想化リスト/遅延読み込みで off-screen 要素がツリーに現れないことがある(ItemContainer パターン等)。§6 テンプレートの edge-case 表(DPI/occlusion/cancellation…)に virtualization が挙がっておらず、テスタビリティを過大評価(要素が「無い」=問題なし)しかねない。
- **根拠**: [事実] §6 の edge-case 列挙に virtualization がない。[推測] 仮想化未考慮だと大規模リスト画面の要素被覆が実態より良く見える。
- **影響**: 分析の過小報告、`Unavailable` と「未展開」の混同。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: §6 edge-case 表と DES-0014 に「virtualization / 遅延ツリーの検出と `PartialResult`/`Unavailable(not-realized)` 方針」を追加。
- **追加すべきテスト**: UT-0004 に仮想化 fixture、IT-0002/0006 に大規模仮想リスト。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §6, DES-0014
- **対応状況**: ☐ 未対応

### R-GTA-03 — 「即自動化」分類の実自動化クロスチェックがない
- **観点**: GUIテスト自動化
- **指摘内容**: DES-0017 は専門家との一致率を校正するが、`即自動化` と分類した画面を実際の自動化フレームワーク(WinAppDriver/FlaUI 等)で1件でも駆動して裏取りするクロスチェックがない。誤って「即自動化」とした画面が実は自動化困難なら重大な誤誘導。
- **根拠**: [事実] DES-0007 §4 DES-0017 は "expert review sample size, agreement target" を挙げるが実自動化検証を含まない。[推測] 人間一致のみでは「人も分析器も同じ誤り」を見逃す。
- **影響**: 分類の外的妥当性が未検証。
- **重大度**: Medium / **優先**: P3
- **修正案(提案)**: DES-0017 に「即自動化サンプルの一部を実自動化で裏取り」する任意受入条件を追加(コストと相談)。
- **追加すべきテスト**: IT(手動/半自動): サンプル画面の実自動化スモーク。
- **対応先**: 人間レビュー ／ **優先**: P3 ／ **関連ファイル**: `des-0007-...md` §4(DES-0017)
- **対応状況**: ☐ 未対応

---

## 5. 品質保証・テスト設計者

**総評.** 「このテストはどんな誤実装を捕えるか」という UT 意図の据え方(§7)は非常に良く、coverage-only テストを明確に排している。次段として、テストが本当に誤実装を捕えることの担保(反例 fixture/ミューテーション観点)、golden 運用ガバナンス、ガードレール毎の failing-first 被覆確認を足したい。

**良い点(事実).** §7 の test-smell 明示(「閾値=実装定数」型を排除)、golden は保護意味を宣言した時のみ許容(§7 末尾)。

**懸念点.** テスト自体の有効性(誤実装検出力)を保証する仕組みがない。

**重大な抜け漏れ.** なし(High 級)。中位に反例 fixture・golden 運用。

**実装時に詰まりそうな箇所.** golden 再生成の判断(意味的差分 vs 無害差分)。

**テスト不足になりそうな箇所.** 各 UT が「正しい実装は通し・誤実装は落とす」ことの反例側。

**追加すべき設計記述.** §7 に「各 UT は最低1つの反例(誤実装を模す)fixture を伴う」。

**追加すべき受入条件.** 「golden 変更は保護意味の再確認と承認を伴う」。

**Claude Code / Codex に渡せる作業.** 反例 fixture 雛形、golden 差分の意味的レビュー補助スクリプト(Cowork/Codex)。

**人間レビューが必須な論点.** golden 変更承認者。

### R-QA-01 — テストが誤実装を捕えることの担保(反例/ミューテーション観点)がない
- **観点**: 品質保証
- **指摘内容**: §7 は「どんな誤実装を捕えるか」を問うが、実際にその誤実装を模した**反例 fixture** や最小限のミューテーション観点を要求していない。問いだけでは、書かれたテストが本当に落ちるかは未検証。
- **根拠**: [事実] §7 は意図を問うが反例 fixture を義務化しない。[推測] 反例なしだと green だが無力なテストが混入。
- **影響**: テスト意図と実効性の乖離。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: §7 に「各 UT-xxxx は少なくとも1つの反例(意図的誤実装/破壊 fixture)で赤を確認」を追加。
- **追加すべきテスト**: 反例ケース群(例: UT-0002 に「二重計上する誤 scoring を落とす」)。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §7
- **対応状況**: ☐ 未対応

### R-QA-02 — golden ファイルの運用ガバナンスが未定義
- **観点**: 品質保証
- **指摘内容**: golden は保護意味の宣言時のみ許容(良い)だが、**再生成の承認・意味的差分の識別・無害差分の扱い**というガバナンスがない。決定性出力ほど golden churn の管理が重要。
- **根拠**: [事実] §7 末尾は golden の許容条件のみ規定。[推測] 再生成手順/承認なしだと golden が形骸化。
- **影響**: 回帰検出力の劣化。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: §7 or DES-0012 に golden 運用(再生成コマンド・承認・意味的差分レビュー)を規定。
- **追加すべきテスト**: N/A(プロセス)。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §7, DES-0012
- **対応状況**: ☐ 未対応

### R-QA-03 — ガードレール毎の failing-first 被覆確認マトリクスがない
- **観点**: 品質保証
- **指摘内容**: DES-0005 に RQ→UT/IT トレースはあるが、DES-0007 レベルで「4ガードレールそれぞれに最低1つの failing-first テストがある」ことを確認する明示マトリクスがない。
- **根拠**: [事実] §9 チェックリストは定性的。[推測] 定量マトリクスなしだと被覆漏れに気づきにくい。
- **影響**: ガードレール検証の穴を見落とす。
- **重大度**: Low / **優先**: P3
- **修正案(提案)**: §9 にガードレール×failing-first テストの被覆表を追加。
- **対応先**: Cowork ／ **優先**: P3 ／ **関連ファイル**: `des-0007-...md` §9, `des-0005-...md`
- **対応状況**: ☐ 未対応

---

## 6. DevOps / CI 担当

**総評.** 設計文書の検証コマンド(`Validate-Okf.ps1`, `git diff --check`)は明確だが、**将来のコード**の CI・特に IT の実行環境が未設計。IT-0001–0007 は生きた Windows デスクトップ・fixture app・特定 DPI/マルチモニタ/整合性を要し、通常のヘッドレス CI では走らない。決定性検証にはカルチャ/TZ/改行の固定も要る。

**良い点(事実).** OKF 検証と `git diff --check` を各 DES の Verification に固定。UT はアダプタ非依存で fake 化(DES-0005 の Codex slice 1–12)。

**懸念点.** IT の実行基盤が未決。決定性の CI 前提未固定。fixture app が未スケジュール成果物。

**重大な抜け漏れ.** IT 用の対話的 Windows ランナー方針(R-OPS-01)。

**実装時に詰まりそうな箇所.** UIA/capture の IT を CI で自動化しようとして走らない。

**テスト不足になりそうな箇所.** CI で走らせられない IT の代替(手動ゲート/自己ホストランナー)。

**追加すべき設計記述.** CI トポロジ(UT=ヘッドレス可、IT=対話セッション付き自己ホスト or 手動)、決定性 CI 前提。

**追加すべき受入条件.** 「UT は無人 CI で決定的に緑。IT は環境前提を明記し、対話セッション必須なものは実行手段を指定」。

**Claude Code / Codex に渡せる作業.** CI パイプライン雛形(UT レーン)、カルチャ/TZ 固定設定(Cowork/Codex)。

**人間レビューが必須な論点.** 自己ホスト Windows ランナー(対話セッション/uiAccess 署名)の用意可否。

### R-OPS-01 — IT の実行環境(対話的 Windows デスクトップ/自己ホストランナー)が設計されていない
- **観点**: DevOps/CI
- **指摘内容**: IT-0001–0007 は実 UIA 取得・capture・整合性/uiAccess・手動ウォークスルーを要し、通常のヘッドレス CI エージェントでは実行不能。戦略/DES-0005 は IT の環境前提を記すが、**CI 実行手段**(対話セッション付き自己ホストランナー、あるいは手動ゲート化)を決めていない。
- **根拠**: [事実] DES-0005 IT-0001〜0007 は Windows 版数/DPI/整合性/fixture app/手動手順を前提とする。[事実] DES-0007 の Verification は設計文書検証のみ言及。[推測] 実行手段未定だと IT が「書いたが走らない」状態になる。
- **影響**: IT が実運用されず、`RQ-048` 状態不変等の実機保証が形骸化。
- **重大度**: High / **優先**: P1
- **修正案(提案)**: DES-0008 or 新設 CI 節に「UT=無人 CI、IT=対話セッション付き自己ホスト or 手動ゲート」の実行トポロジを定義。uiAccess 系は署名/インストール前提を明記。
- **追加確認事項**: 自己ホスト Windows ランナーの調達可否(未確認)。
- **追加すべきテスト**: N/A(基盤)。ただし UT レーンの無人緑を受入条件化。
- **対応先**: 人間レビュー / Cowork ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md`, `des-0005-...md` IT 群
- **対応状況**: ☐ 未対応

### R-OPS-02 — 決定性のための CI 前提(カルチャ/TZ/改行)が未固定
- **観点**: DevOps/CI
- **指摘内容**: byte-stable 出力を機械/環境跨ぎで保つには、CI が invariant culture・UTC・改行正規化を固定する必要がある(R-NET-01/03 と連動)。戦略に前提記載がない。
- **根拠**: [事実] §7 UT-0006 は byte-stable を要求。[推測] CI 前提未固定だと環境差で golden がぶれる。
- **影響**: CI 上の偽赤/偽緑。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: CI 節に「`DOTNET_SYSTEM_GLOBALIZATION_INVARIANT`/TZ=UTC/改行正規化」を固定。
- **追加すべきテスト**: 異カルチャ環境での UT-0006 再実行。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §7
- **対応状況**: ☐ 未対応

### R-OPS-03 — IT 用 fixture(レガシー)アプリが未スケジュールの成果物
- **観点**: DevOps/CI
- **指摘内容**: IT-0001–0003/0006/0007 は "fixture app" を前提とするが、その fixture(理想的にはレガシー MFC/Win32 サンプル)を**誰がいつ作るか**がどのパッケージにも割り当てられていない。
- **根拠**: [事実] DES-0005 IT 群が "fixture app" を参照。[事実] DES-0007 §4 のパッケージに fixture app 構築行がない。[推測] 未スケジュールだと IT 着手時に前提が欠落。
- **影響**: IT ブロック、レガシー取得エッジ(R-WIN-03)未検証。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: DES-0008 or DES-0014/0015 に「fixture app(レガシー要素を含む)構築」を成果物として明記。
- **追加すべきテスト**: N/A(成果物)。
- **対応先**: 人間レビュー ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §4, `des-0005-...md` IT 群
- **対応状況**: ☐ 未対応

---

## 7. 保守開発リーダー

**総評.** パッケージ粒度と自己完結方針は保守に有利。ただし **閾値/ルールセットの外部化(コードにハードコードしない)** と **受理済み DES の改訂/supersede 規約** を定めないと、校正ループ(DES-0017)や将来変更が UT と衝突する。

**良い点(事実).** 各 DES を自己完結・索引可能に(§2)。UT が「閾値=実装定数」を避ける方針(§7)。

**懸念点.** 閾値がコード定数化すると校正のたびにコードと UT が揺れる。DES 改訂の版管理規約がない。

**重大な抜け漏れ.** 閾値の設定外部化方針(R-MNT-01)。

**実装時に詰まりそうな箇所.** 校正で閾値変更 → 多数 UT の再ベースライン。

**テスト不足になりそうな箇所.** 閾値変更に強い「順序不変/区別不変」型の性質テスト(値そのものでなく関係を検証)。

**追加すべき設計記述.** 閾値/ルールセットを版付き設定として外部化、DES supersede 規約(log.md 連携)。

**追加すべき受入条件.** 「閾値変更は設定+版数更新で行い、性質テストは値変更に対して安定」。

**Claude Code / Codex に渡せる作業.** 閾値設定スキーマ + 版数のドラフト(Cowork)。

**人間レビューが必須な論点.** 閾値外部化の粒度(設定 vs コード)。

### R-MNT-01 — 閾値/ルールセットの版付き外部化方針がない
- **観点**: 保守開発リーダー
- **指摘内容**: §8 は閾値の adjustment loop を想定し、§7 は「閾値=実装定数」テストを禁じるが、**閾値/ルールセットを版付き設定として外部化する**という保守方針が明示されていない。ハードコードだと校正のたびにコード+UT が揺れる。
- **根拠**: [事実] DES-0007 §8 に閾値 adjustment loop、§7 に閾値定数テスト禁止。[推測] 外部化しないと保守コストと UT churn が増え、どの閾値版数が分類を出したか再現できない。
- **影響**: 校正のたびの回帰コスト増、再現性低下。
- **重大度**: High / **優先**: P2
- **修正案(提案)**: DES-0010 に「閾値/重み/丸めは版付き設定として外部化し、レポートに版数を記録」。UT-0002 は値でなく「順序/区別/単調性」等の性質を検証。
- **追加すべきテスト**: 性質テスト(閾値を変えても `Unavailable`≠低スコア/二重計上なし が保持)。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §4/§7/§8(DES-0010)
- **対応状況**: ☐ 未対応

### R-MNT-02 — 受理済み DES の改訂/supersede 規約がない
- **観点**: 保守開発リーダー
- **指摘内容**: 実装開始後に詳細設計決定を変える必要が生じたとき、`DES-xxxx` をどう改訂/supersede し log.md/index にどう反映するかの規約がない(ADR の supersede に相当する運用)。
- **根拠**: [事実] Lifecycle Traceability は ID 規約を定めるが DES 改訂/supersede 運用を規定しない。[推測] 規約なしだと設計コーパスが陳腐化・不整合化。
- **影響**: 設計知識の信頼性低下。
- **重大度**: Medium / **優先**: P3
- **修正案(提案)**: §5 or Lifecycle Traceability に「DES 改訂は版注記 + log.md 追記 + 影響 UT/IT の見直し」を規定。
- **対応先**: Cowork ／ **優先**: P3 ／ **関連ファイル**: `des-0007-...md` §5, `lifecycle-traceability.md`
- **対応状況**: ☐ 未対応

---

## 8. 実装担当者

**総評.** 「first failing test / candidate module / verification command」を各パッケージ handoff に要求する点は実装者に優しい。一方、**DES-0009(順序2, ドメインキー)が必要とする fallback ハッシュを DES-0013(順序6, 機密ポリシー)が所有**するという順序矛盾が実装をブロックする。per-slice の DoD も薄い。

**良い点(事実).** §5-6 の handoff(最初の失敗テスト・候補領域・検証コマンド)。scaffold を DES-0008 で最優先。

**懸念点.** M04↔M09 の fallback キー seam がパッケージ順序と衝突。

**重大な抜け漏れ.** 上記順序矛盾の解消(R-IMP-01)。

**実装時に詰まりそうな箇所.** 「ドメインキーを作りたいが、その sensitive-fallback ハッシュは6番目のパッケージまで未設計」。

**テスト不足になりそうな箇所.** fallback キー確定段(構築 vs ポリシー適用 vs 結果組立)の決定性・非可逆性テスト(RSK-DES-002)。

**追加すべき設計記述.** DES-0009 と DES-0013 の間に「fallback キー最小契約」を前倒しで固定。

**追加すべき受入条件.** 「fallback キー確定段が単一箇所で定義され、ドメイン内で raw sensitive text をハッシュしない」。

**Claude Code / Codex に渡せる作業.** fallback キー最小インターフェイスのドラフト(Cowork)。

**人間レビューが必須な論点.** 確定段の選択(RSK-DES-002)。

### R-IMP-01 — DES-0009(順序2)が DES-0013(順序6)所有の fallback ハッシュを必要とする順序矛盾
- **観点**: 実装担当者
- **指摘内容**: DES-0002 M04 は「stable identity が無い場合の sensitive-fallback キーの hash を M09 に委譲」する。DES-0007 §4 では DES-0009(ドメイン, 順序2)が先行し、その fallback ハッシュを担う DES-0013(機密, 順序6)は後。RSK-DES-002 も「fallback ScreenKey の確定段が未定」と認めている。実装者は順序2でキーを作る際、順序6まで未設計のハッシュ契約に依存してしまう。
- **根拠**: [事実] DES-0002 M04/M09 に fallback ハッシュ委譲、DES-0005 に RSK-DES-002。[事実] DES-0007 §4 で DES-0009=order2, DES-0013=order6。[推測] 依存が順序を逆行し、DES-0009 実装が DES-0013 を待つか暫定実装で手戻り。
- **影響**: 実装ブロック/手戻り、決定性・機密の一貫性リスク。
- **重大度**: High / **優先**: P1
- **修正案(提案)**: DES-0009 内に「fallback キー最小契約(非可逆・決定的・raw text をドメインで扱わない)」を前倒しで固定し、詳細は DES-0013 で拡張。または §4 で DES-0013 の該当部分を DES-0009 と同時期に着手する依存を明記。RSK-DES-002 の確定段をこの機に決定。
- **追加確認事項**: 確定段(構築/ポリシー/組立)の選択(RSK-DES-002, 未確認)。
- **追加すべきテスト**: UT-0001/UT-0008 に「fallback キーは非可逆・別プロセスで同値(R-NET-01 連動)・raw sensitive text 非露出」。
- **対応先**: Cowork ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md` §4/§8, `des-0002-...md` M04/M09, `des-0005-...md` RSK-DES-002
- **対応状況**: ☐ 未対応

### R-IMP-02 — per-slice の Definition of Done / コーディング規約 / 分析器設定がない
- **観点**: 実装担当者
- **指摘内容**: §5 は handoff 要素を挙げるが、スライスの DoD(テスト緑 + 検証コマンド以外の完了基準: 分析器/警告 0、レイヤ境界検査、機密/決定性チェック)や「accept with risks」の具体条件が未定義。
- **根拠**: [事実] §5 は first failing test/verification command を要求。[推測] DoD 不足だと完了判定がぶれる。
- **影響**: 品質のばらつき、レビュー往復増。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: §5 に per-slice DoD(分析器警告0/レイヤ境界/機密・決定性チェック/トレース更新)を追加。DES-0008 に分析器設定。
- **追加すべきテスト**: レイヤ境界(依存方向)検査(ArchUnitNET 等)。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §5(, DES-0008)
- **対応状況**: ☐ 未対応

---

## 9. プロジェクトマネージャ

**総評.** プロセスとしては完成度が高いが**スケジュール面が無言**。10 の詳細設計 + 実装 + UT + IT を、scaffold もない状態から始める前提で、クリティカルパス・工数・マイルストンがない。特に spike(RSK-RD-001)が約半数のアダプタ系パッケージをゲートするのに未スケジュール・未所有。

**良い点(事実).** パッケージ順序と各ゲート(§5 step 8 のレビュー要求)。

**懸念点.** spike がクリティカルパス上の未管理依存。決定権者/SLA 不明。フェーズ全体の完了基準なし。

**重大な抜け漏れ.** spike のスケジュール化・所有(R-PM-01)。

**実装時に詰まりそうな箇所.** N/A(管理面)。

**テスト不足になりそうな箇所.** N/A。

**追加すべき設計記述.** クリティカルパス、spike をマイルストン化。

**追加すべき受入条件.** 「アダプタ系パッケージ着手前に spike 完了+ADR-0002 起票」。

**Claude Code / Codex に渡せる作業.** 依存 DAG からのクリティカルパス抽出(Cowork)。

**人間レビューが必須な論点.** spike 所有者・決定権者・フェーズ完了基準。

### R-PM-01 — spike が約半数のパッケージをゲートするのに未スケジュール・未所有(クリティカルパス不在)
- **観点**: プロジェクトマネージャ
- **指摘内容**: RSK-RD-001(UIA/capture/packaging)を解く spike が DES-0014/0015 を含むアダプタ系(および UI/性能の一部)をゲートするが、spike の所有者・期日・成果物がどの計画行にもない。クリティカルパスとマイルストンが未定義。
- **根拠**: [事実] §8 は spike/ADR-0002 で closure、§5 は「spike/decision を待て」。[事実] §4 に spike を作業として含む行がない。[推測] 未管理だと着手可否が滞留。
- **影響**: スケジュールリスク、着手判断の属人化。
- **重大度**: High / **優先**: P1
- **修正案(提案)**: §4/§5 に spike を明示作業として追加(所有者・比較軸・期日・成果物=ADR-0002)。依存 DAG からクリティカルパスを図示(R-ARC-04 連動)。
- **追加確認事項**: spike 所有者(未確認)。
- **対応先**: 人間レビュー ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md` §4/§5/§8
- **対応状況**: ☐ 未対応

### R-PM-02 — 設計ゲートの承認者ロール/SLA が未定義
- **観点**: プロジェクトマネージャ
- **指摘内容**: §5 step 8 は「algorithms/schemas/adapter tech/privacy defaults/UI を決めるパッケージは実装前にレビュー要」とするが、**誰が承認するか**(ロール)と応答 SLA が不明。
- **根拠**: [事実] §5 step 8 はレビュー要求のみ。[推測] 承認者未定だとゲートが滞る/形骸化。
- **影響**: ゲートのボトルネック化。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: §5 に承認者ロール(例: 機密=セキュリティ、閾値=品質/ドメイン専門家、アダプタ=アーキ)と SLA を明記。
- **対応先**: 人間レビュー ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §5
- **対応状況**: ☐ 未対応

### R-PM-03 — 詳細設計フェーズ全体の完了/受入基準がない
- **観点**: プロジェクトマネージャ
- **指摘内容**: 各パッケージにゲートはあるが、DES-0007 が定義するフェーズ**全体**の完了基準(どの状態でフェーズを閉じるか)がない。
- **根拠**: [事実] §5/§9 はパッケージ単位。[推測] フェーズ完了の合意点がないと締められない。
- **影響**: フェーズ管理の曖昧化。
- **重大度**: Medium / **優先**: P3
- **修正案(提案)**: §1 or §9 に「フェーズ完了=全計画パッケージが accept/accept-with-risk、残余リスクが明示保持、UT/IT 意図が全ガードレールを被覆」。
- **対応先**: 人間レビュー ／ **優先**: P3 ／ **関連ファイル**: `des-0007-...md` §1/§9
- **対応状況**: ☐ 未対応

---

## 10. セキュリティ・運用担当

**総評.** 出力(レポート/ストア)への機密 secure-by-default は強い。しかし **ログ/診断/例外メッセージという別の流出経路**が `RQ-052` ゲート(§5 の M09 は report/store 前適用)に含まれていない。ローカルストアの at-rest 保護・最小権限(admin/uiAccess を既定要求しない)も未言及。

**良い点(事実).** §5 Stage 5 の機密ゲートを report/store 前に強制、fail-open 禁止(DES-0004)。keys/paths に raw sensitive text を入れない。

**懸念点.** ログ/例外経由の漏えい、store の暗号化/ACL、権限最小化。

**重大な抜け漏れ.** ログ/診断の機密サニタイズ(R-SEC-01)。

**実装時に詰まりそうな箇所.** 例外メッセージにウィンドウタイトル/Name/パスが載る。

**テスト不足になりそうな箇所.** ログ/診断/例外に raw sensitive text が出ないことの検証。

**追加すべき設計記述.** DES-0013 に「ログ/診断/例外のサニタイズ」、store の at-rest 保護と最小権限原則。

**追加すべき受入条件.** 「ログ・診断・例外メッセージに raw な title/Name/パスが出現しない」。

**Claude Code / Codex に渡せる作業.** サニタイズ済みロギング規約と検証 UT のドラフト(Cowork/Codex)。

**人間レビューが必須な論点.** at-rest 暗号化/ACL の要否、uiAccess の運用可否。

### R-SEC-01 — ログ/診断/例外メッセージの機密経路が RQ-052 ゲート/DES-0013 に含まれていない
- **観点**: セキュリティ・運用
- **指摘内容**: `RQ-052` ゲート(M09)は report/store 前の画像/テキストに適用されるが、**run-level diagnostics・例外メッセージ・ログ**は別の egress であり、window title/`Name`/パス等の機密が素通りしうる。DES-0013 のスコープは masking/storage/export に限られ、ログ/診断/例外のサニタイズを含まない。
- **根拠**: [事実] DES-0004 Stage 5 の機密ゲートは report/store 前。各 Stage は diagnostics を生成。[事実] DES-0007 §4 DES-0013 スコープは "masking/redaction, storage paths, retention, export bundle" でログ/例外を含まない。[推測] サニタイズ未規定だと診断/例外/ログから機密漏えい。
- **影響**: `RQ-052` の実質的抜け穴(機密漏えい)。
- **重大度**: High / **優先**: P1
- **修正案(提案)**: DES-0013 スコープに「ログ/診断/例外メッセージのサニタイズ(title/Name/パスの masking)」を追加。診断モデル(R-ARC-03)と統合。
- **追加すべきテスト**: UT: 例外/診断/ログ出力に raw sensitive text が含まれないこと(allow-all/mask-all 両分岐)。IT-0004 に diagnostics 経路を追加。
- **対応先**: Cowork ／ **優先**: P1 ／ **関連ファイル**: `des-0007-...md` §4(DES-0013), `des-0004-...md` Stage5
- **対応状況**: ☐ 未対応

### R-SEC-02 — ローカルストアの at-rest 保護/ACL と最小権限原則が未言及
- **観点**: セキュリティ・運用
- **指摘内容**: RSK-RD-003 の closure(DES-0013)は paths/retention/secure-by-default 値を扱うが、**保存物の at-rest 暗号化/ACL** と、アナライザの**最小権限原則**(既定で admin/uiAccess を要求しない、必要時のみ整合性昇格)への言及がない。スクリーンショットは PHI/PII/資格情報を含みうる。
- **根拠**: [事実] DES-0007 §4/§8 DES-0013 は paths/retention/masking を扱う。[事実] DES-0014 は integrity/uiAccess を扱うが最小権限方針は未記載。[推測] 既定で広権限/平文保存だと漏えい面が拡大。
- **影響**: 保存機密の露出、過剰権限。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: DES-0013 に「store の ACL/at-rest 保護」、DES-0014 に「最小権限(same-integrity 優先、uiAccess は必要時のみ・署名前提)」を追加。
- **追加すべきテスト**: IT-0005 に「same-integrity 既定で動作、昇格は明示時のみ」。
- **対応先**: 人間レビュー ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §4/§8(DES-0013/0014)
- **対応状況**: ☐ 未対応

---

## 11. Claude Code / Codex 活用担当

**総評.** 自己完結 OKF・first-failing-test handoff・DES-0005 の Codex slice 候補など、**エージェント実行を強く意識**した良設計。伸びしろは、spike をエージェントタスクとして枠組み化、生成テストの smell(実装定数アサート)への第2パス点検、パッケージ毎の最小コンテキスト束の3点。

**良い点(事実).** §5-6 の機械可読 handoff、DES-0005 のアダプタ非依存 slice 1–12、決定的 fixture 前提、行動命名テスト方針(§7)。

**懸念点.** spike が「人間待ち」に見えるがエージェント調査タスク化できる。生成テストの smell 点検ゲートがない。

**重大な抜け漏れ.** なし(High 級)。中位に spike のタスク化・テスト smell 点検。

**実装時に詰まりそうな箇所.** エージェントが全上流6文書を読み込まないと1 slice を実装できない(コンテキスト過多)。

**テスト不足になりそうな箇所.** エージェント生成テストが「閾値=実装定数」型 smell を持たないことの点検。

**追加すべき設計記述.** spike のエージェントタスク定義(受入基準付き)、パッケージ毎の最小コンテキスト束、生成テストの第2パス点検ゲート。

**追加すべき受入条件.** 「各 DES-xxxx handoff は、エージェントが上流全読せず着手できる最小コンテキスト束(必要 RQ/RD/DES 抜粋)を含む」。

**Claude Code / Codex に渡せる作業.** slice 毎の最小コンテキスト束生成、smell 点検スクリプト(Cowork/Codex)。

**人間レビューが必須な論点.** spike 成果(技術選定)の最終判断。

### R-AI-01 — spike がエージェント実行タスクとして枠組み化されていない
- **観点**: Claude Code/Codex 活用
- **指摘内容**: DES-0005 は adapter-bound slice を "No (needs spike)" とするが、spike 自体は比較軸(read-only 実現性/決定性/fixture 化/権限/packaging/性能)が既にあり、**受入基準付きのエージェント調査タスク**として定義できる。戦略はそれを人間待ちのように扱い、タスク化していない。
- **根拠**: [事実] DES-0005 Codex slice 13 は spike 依存。[事実] §8 に比較軸あり。[推測] タスク化すればエージェントが候補評価を前進できる。
- **影響**: spike の停滞(R-PM-01/R-ARC-02 連動)。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: §5/§8 に「spike=エージェント調査タスク(比較軸・最小 PoC・測定手順・出力=ADR-0002 草案)」を定義。最終選定のみ人間。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §5/§8, `des-0005-...md`
- **対応状況**: ☐ 未対応

### R-AI-02 — エージェント生成テストの smell(実装定数アサート)第2パス点検ゲートがない
- **観点**: Claude Code/Codex 活用
- **指摘内容**: §7 は smell を明示するが、エージェントが生成した UT がその smell(閾値=実装定数、helper 文字列丸写し等)を持たないことを**第2パス(別エージェント/人間)で点検するゲート**が定義されていない。
- **根拠**: [事実] §7 は smell を列挙。[推測] 生成側が同じ smell を作り込むと自己点検では漏れる(R-QA-01 連動)。
- **影響**: 無力なテストの通過。
- **重大度**: Medium / **優先**: P2
- **修正案(提案)**: §9 チェックリストに「生成テストの smell 第2パス点検(反例で赤確認)」を追加。
- **追加すべきテスト**: 反例 fixture(R-QA-01 と共有)。
- **対応先**: Cowork ／ **優先**: P2 ／ **関連ファイル**: `des-0007-...md` §7/§9
- **対応状況**: ☐ 未対応

### R-AI-03 — パッケージ毎の最小コンテキスト束が未指定
- **観点**: Claude Code/Codex 活用
- **指摘内容**: §2 は「各 DES を自己完結」とするが、パッケージは上流を多数相互参照する。エージェントが1 slice を実装するのに全6文書を読む必要が生じ得る。handoff に「必要 RQ/RD/DES 抜粋の最小束」を含める指定がない。
- **根拠**: [事実] §2 self-contained 方針、各 DES は相互リンク多数。[推測] 最小束がないとエージェントのコンテキスト効率が悪化。
- **影響**: エージェント実行の効率/精度低下。
- **重大度**: Low / **優先**: P3
- **修正案(提案)**: §6 テンプレート handoff 節に「最小コンテキスト束(この slice に必要な RQ/RD/DES 抜粋)」を追加。
- **対応先**: Cowork ／ **優先**: P3 ／ **関連ファイル**: `des-0007-...md` §6
- **対応状況**: ☐ 未対応

---

## Consolidated Rollups

### 追加すべき受入条件(統合)
- キー・順序・シリアライズはプロセス再起動とカルチャ変更に対して不変(R-NET-01/02/03, R-OPS-02)。
- 各 `DES-xxxx` は担当 `Mnn` を明記し、M01–M13 に詳細設計未割当を残さない(R-ARC-01)。
- アナライザは PMv2 DPI aware、bounds は対象 DPI コンテキストで正規化(R-WIN-01)。
- 各評価軸は具体 UIA/MSAA プロパティ・パターン可用性で定義(R-GTA-01)。
- ログ・診断・例外に raw な title/Name/パスが出現しない(R-SEC-01)。
- 閾値/ルールセットを版付き設定として外部化し、レポートに版数を記録して分類を再現可能にする(R-MNT-01)。
- UT は無人 CI で決定的に緑、IT は環境前提と実行手段を明記(R-OPS-01)。
- fallback キー確定段が単一箇所で定義され、ドメイン内で raw sensitive text をハッシュしない(R-IMP-01)。
- アダプタ系パッケージ着手前に spike 完了 + ADR-0002 起票(R-PM-01, R-ARC-02)。

### 人間レビューが必須な論点(統合)
- spike の所有者・技術選定の最終承認・ADR-0002 昇格(R-ARC-02, R-PM-01, R-AI-01)。
- UIA スレッド/アパートメント・MSAA フォールバックの実機挙動(R-WIN-02/03)。
- 自己ホスト Windows ランナー/uiAccess 署名の調達可否(R-OPS-01)。
- at-rest 暗号化/ACL・最小権限運用(R-SEC-02)。
- 設計ゲートの承認者ロールとフェーズ完了基準(R-PM-02/03)。

### Claude Code / Codex に渡せる作業(統合)
- M01–M13 × DES-0008–0017 被覆マトリクスと依存 DAG(R-ARC-01/04)。
- 決定性ガード付きプロジェクトテンプレート・InvariantCulture シリアライズ規約(R-NET-01/02/03, R-OPS-02)。
- レガシー取得エッジ表・capture 失敗様態表・軸↔UIA プロパティ写像(R-WIN-03/04, R-GTA-01)。
- 反例 fixture 雛形・golden 意味的差分レビュー補助・生成テスト smell 点検(R-QA-01/02, R-AI-02)。
- サニタイズ済みロギング規約と検証 UT(R-SEC-01)。
- spike のエージェント調査タスク定義・最小コンテキスト束(R-AI-01/03)。

## Disposition Log — 2026-07-01 DES-0007 Integration

The reviewed strategy [DES-0007](des-0007-detailed-design-execution-strategy.md) was revised on 2026-07-01 to fold in the Cowork-owned findings and to surface human-decision items as explicit carried risks and gates (it decides no technology choice on its own). This log is authoritative for status; the per-finding **対応状況** lines below predate this integration and are reconciled here. `☑` = reflected into DES-0007; `◑` = Cowork/design part reflected, a human decision still carried in DES-0007 §8.1.

| ID | 状況 | DES-0007 反映箇所 |
| -- | -- | -- |
| R-ARC-01 | ☑ | Human decision 2026-07-01 → standalone `DES-0018` (§4 package 11); Modules 列 + module-coverage note; §4.1/§4.2 DAG; `UT-0013` |
| R-ARC-02 | ☑ | §4.2 spike process fully defined (axes/method/exit/output/gate) + hybrid owner + early timing (decided 2026-07-01); only data-driven final pick carried §8.1 |
| R-ARC-03 | ☑ | §4 DES-0011 diagnostics/logging model; §6 template "Diagnostics and logging" row |
| R-ARC-04 | ☑ | §4.2 dependency DAG + parallelizable sets |
| R-WIN-01 | ☑ | §4 DES-0015 scope (PMv2 DPI awareness, bounds normalized to target DPI) |
| R-WIN-02 | ◑ | §4 DES-0014 scope (UIA threading/apartment + cooperative cancel/timeout); live behavior verification carried to spike |
| R-WIN-03 | ☑ | §4 DES-0014 legacy acquisition edge table |
| R-WIN-04 | ☑ | §4 DES-0015 capture failure-mode table |
| R-NET-01 | ◑ | §4.1/§7 stable-hash/ordinal rule + cross-process UT-0001/UT-0006; hash algorithm/collision final choice carried (security intersection) |
| R-NET-02 | ☑ | §4 DES-0008 determinism/quality project settings |
| R-NET-03 | ☑ | §4 DES-0012 serializer determinism contract |
| R-GTA-01 | ☑ | §4 DES-0010 axis↔UIA/MSAA property-and-pattern mapping obligation |
| R-GTA-02 | ☑ | §4 DES-0014 virtualized/lazy-tree handling; §6 edge-case row |
| R-GTA-03 | ◑ | §4 DES-0017 optional real-automation cross-check; cost decision human |
| R-QA-01 | ☑ | §6 template + §7 counter-example fixture rule |
| R-QA-02 | ☑ | §4 DES-0012 + §7 golden-file governance |
| R-QA-03 | ☑ | §9 guardrail failing-first coverage matrix |
| R-OPS-01 | ☑ | Human decision 2026-07-01 → documented manual gate now, self-hosted automation revisited after fixture app/adapters (§8.2) |
| R-OPS-02 | ☑ | §8.2 invariant culture / TZ=UTC / newline normalization |
| R-OPS-03 | ☑ | Human decision 2026-07-01 → DES-0008 owns harness, DES-0014/0015 specify legacy content, incremental, mixed real-MFC + lighter surface (§4 package 1) |
| R-MNT-01 | ☑ | §4 DES-0010 versioned externalized thresholds + property-style tests |
| R-MNT-02 | ☑ | §5.3 design revision/supersede convention |
| R-IMP-01 | ☑ | §4 DES-0009 fallback-key minimal contract front-loaded; §5.3; §8 RSK-DES-002 updated |
| R-IMP-02 | ☑ | §5.1 per-slice Definition of Done |
| R-PM-01 | ☑ | §4.2 spike as scheduled/owned work item (hybrid owner, early parallel timing) + critical path; decided 2026-07-01 |
| R-PM-02 | ☑ | Human decision 2026-07-01 → AI pre-clears + human final approval on every gate (§5.2); §8.1 resolved log |
| R-PM-03 | ☑ | §8.2 phase-completion criteria + human close sign-off (decided 2026-07-01) |
| R-SEC-01 | ☑ | §4 DES-0013 log/diagnostics/exception sanitization; §9 confidentiality check extended |
| R-SEC-02 | ☑ | Human decision 2026-07-01 → DPAPI CurrentUser encryption by default + ACL (DES-0013); same-integrity default, uiAccess opt-in signed (DES-0014); §8.1 resolved log |
| R-AI-01 | ☑ | §4.2 spike as agent investigation task |
| R-AI-02 | ☑ | §7 + §9 second-pass smell check for generated tests |
| R-AI-03 | ☑ | §6 template minimal context bundle |

New/strengthened `UT`/`IT` obligations from these dispositions are recorded in [DES-0005](des-0005-vmodel-traceability-and-downstream-tests.md).

## Disposition Workflow

1. 各 finding の **対応状況** を更新(☐→◑→☑/✕/⏸)し、Summary Dashboard の同 ID 行も同期。
2. `却下`/`保留` は理由を finding 直下に1行追記。
3. 反映は原則 `DES-0007`(修正)または該当 `DES-0008`–`DES-0017`(前倒し記述)へ。`DES-0007` を改訂したら `knowledge/log.md` に追記。
4. 新規 `UT`/`IT` obligation は [DES-0005](des-0005-vmodel-traceability-and-downstream-tests.md) に反映。
5. すべて `対応済`/`却下`/`保留(理由付き)` になった時点で本レビューを close とし log に記録。

## Related

- [DES-0007 Detailed Design Phase Execution Strategy](des-0007-detailed-design-execution-strategy.md)(review target)
- [DES-0002 Module Responsibility Basic Design](des-0002-module-responsibility-basic-design.md)
- [DES-0003 Module Interface Basic Design](des-0003-module-interface-basic-design.md)
- [DES-0004 Analysis Flow Basic Design](des-0004-analysis-flow-basic-design.md)
- [DES-0005 V-Model Traceability and Downstream Tests](des-0005-vmodel-traceability-and-downstream-tests.md)
- [DES-0006 Screen (Operating UI) Basic Design](des-0006-screen-basic-design.md)
- [Lifecycle Traceability](../process/lifecycle-traceability.md)
- [Quality Review Policy](../process/quality-review-policy.md)
