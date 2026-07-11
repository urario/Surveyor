---
okf_version: "0.1"
type: Unit Test Evidence
id: UT-0004
title: Acquisition Fixture to Model Confidence and Unavailable Marker Evidence
status: implemented
tags: [surveyor, unit-test, acquisition, confidence, availability, rq-017, rq-026]
---

# UT-0004 Acquisition Fixture to Model Confidence and Unavailable Marker Evidence

## Trace

| Field | Evidence |
| --- | --- |
| Artifact | `UT-0004`, acquisition fixture tree → domain model 変換 |
| Issues | #43; drives the implementation side #64 |
| Upstream | `DES-0005` slice 6, `DES-0007` §7 UT-0004 intent, `DES-0011` DTO 契約, `DES-0014` エッジ表・confidence rubric・runtime-id 判定 |
| Requirements | `RQ-017`, `RQ-026`; `RD-003`, `RD-004`; guardrails `RQ-048`, `RQ-051`, `RQ-052` |
| Test files | `tests/Surveyor.Application.Tests/AcquisitionFixtureMappingTests.cs`, `tests/Surveyor.Application.Tests/AcquisitionFixtureEdgeTests.cs`, `tests/Surveyor.Application.Tests/AcquisitionFixtureContractTests.cs`, `tests/Surveyor.Application.Tests/AcquisitionScenarios.cs` |
| Support files | `tests/Surveyor.TestSupport/FixtureUiTreeAcquisitionPort.cs`, `tests/Surveyor.TestSupport/AcquisitionModelMapper.cs`, `tests/Surveyor.TestSupport/FixtureRuntimeId.cs`, `tests/Surveyor.TestSupport/FixtureFallbackKey.cs`, `tests/Surveyor.TestSupport/AcquisitionBuildState.cs`, `tests/Surveyor.TestSupport/UiaTreeFixtureReader.cs` |
| Fixtures | `tests/fixtures/uia-trees/acq-*.tree` (happy-path / missing-and-custom / virtualized-vs-absent / legacy-edges / counter-runtimeid / counter-missing-property) |

## Covered Behaviors

- happy path: 全ノードが `High` / `Available` に写像され、`Ok` / rollup 空 / 診断空になる。
- 識別子欠落・カスタムペイン: 安定 ID が無いノードは rung-3 `FallbackHash` または rung-4 `StructuralOrdinal` へ落ち、キーが fallback となり confidence は `Low`。
- 仮想化/遅延サブツリー: `Unavailable(NotRealized)` として写像され、真の不在 `Unavailable(NotExposed)` と区別される (`R-GTA-02`)。ノードはモデルに残り有効なキーを保持する。
- レガシー取得エッジ (`R-WIN-03`): MSAA-only proxy = `Available` / `Medium`、owner-draw / `WM_GETTEXT` テキスト = `Available` / `Low` / fallback キー、`WM_GETTEXT` timeout = `Unavailable(Timeout)`、MDI child のネストした子は通常写像され `High`。
- run レベル: `Unavailable(reason)` rollup を固定走査順で重複排除し、要素数上限で `PartialResult` + `HitElementCap` + `Acquisition.Partial.CapReached` を立てる。診断はすべて `RunStage.TreeAcquisition`。
- 呼び出し側キャンセルは status ではなく `OperationCanceledException` で伝播し、null 入力を拒否する。

## Counter-Example RED (R-QA-01)

- failing-first commit (`7e89f94`) の `FixtureUiTreeAcquisitionPort` は happy-path のみを写像する意図的な naive 実装 (すべて `High` / `Available`、runtime-id 判定・provenance・プロパティ完全性・仮想化・レガシーエッジを無視 — `DES-0007` §7 の忌避スメル)。
- この naive 実装では次のケースが RED になる: 仮想化 (`NotRealized` を期待するが `Available` を返す)、レガシーエッジ (`Timeout` / `Medium` / `Low` を期待)、要素数上限 (`PartialResult` を期待)、反例(b) volatile runtime-id (`FrameworkStableId` を期待するが `AutomationId` rung-1 にする)、反例(e) High-with-missing-property (`Medium` を期待するが `High` を返す)。
- green commit (`553ee5b`) で完全なマッピングへ差し替え、上記が PASS に転じる。
- 第二pass smell チェック (`R-AI-02`): confidence は fixture で宣言せず rubric 入力から算出しているため、反例が意味を持つ。閾値=実装定数コピーやヘルパー文字列コピーのスメルは無い。

## Verification

| Command | Result |
| --- | --- |
| `dotnet test eng/Surveyor.Unit.slnf` | PR #102 CI unit lane (ubuntu-latest, run 29140761984) green |
| `dotnet test tests/Surveyor.Application.Tests/Surveyor.Application.Tests.csproj` | UT-0004 behavior tests (Mapping / Edge / Contract) green |

> 注: 実行環境の egress ポリシーにより .NET SDK をローカル取得できないため、ビルド/テストは PR の CI unit lane (ubuntu) で検証する。RED → GREEN 遷移は commit `7e89f94` (naive, RED) → `553ee5b` (完全実装, GREEN) に残る。

## Residual Risk

- フィクスチャローダは合成 `.tree` を写像する fake seam であり、実 UIA/MSAA からの取得・実仮想化・実レガシーエッジ観察は `IMP-0013` / `IT-0002` のスコープに残る。フィクスチャのレガシーエッジ忠実度は実観察との突合で補正が必要になり得る。
- confidence rubric / availability エッジ方針のロジックは現状フィクスチャ fake 内にあり、実アダプタでの再利用は `IMP-0013` で行う。
