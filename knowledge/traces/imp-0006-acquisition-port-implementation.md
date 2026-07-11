---
okf_version: "0.1"
type: Implementation Evidence
id: IMP-0006
title: Acquisition Port and Fixture Loader Implementation
status: implemented
tags: [surveyor, implementation, acquisition, port, fixture, rq-017, rq-026]
---

# IMP-0006 Acquisition Port and Fixture Loader Implementation

## Trace

| Field | Evidence |
| --- | --- |
| Artifact | `IMP-0006`, acquisition port + 決定的フィクスチャローダ (`M06`) |
| Issues | #64; closes the implementation side of #43 |
| Upstream | `DES-0011` fixed DTO/status model (`AcquisitionResult` 契約), `DES-0014` エッジ表・confidence rubric・runtime-id 判定・仮想化ハンドリング |
| Requirements | `RQ-017`, `RQ-026`; `RD-003`, `RD-004`; guardrails `RQ-048`, `RQ-051`, `RQ-052` |
| Production files | `src/Surveyor.Application/Ports/IUiTreeAcquisitionPort.cs`, `src/Surveyor.Application/Dto/AcquisitionResult.cs`, `src/Surveyor.Application/Dto/AcquisitionOptions.cs`, `src/Surveyor.Application/Dto/AcquisitionProvenance.cs`, `src/Surveyor.Application/PublicAPI.Unshipped.txt` |
| Test-support files | `tests/Surveyor.TestSupport/FixtureUiTreeAcquisitionPort.cs`, `tests/Surveyor.TestSupport/AcquisitionModelMapper.cs`, `tests/Surveyor.TestSupport/FixtureRuntimeId.cs`, `tests/Surveyor.TestSupport/FixtureFallbackKey.cs`, `tests/Surveyor.TestSupport/AcquisitionBuildState.cs`, `tests/Surveyor.TestSupport/UiaTreeFixtureReader.cs`, `tests/Surveyor.TestSupport/UiaTreeFixture.cs`, `tests/Surveyor.TestSupport/UiaTreeFixtureNode.cs`, `tests/Surveyor.TestSupport/FixtureReadOutcome.cs`, `tests/Surveyor.TestSupport/FixtureRepositoryPaths.cs` |
| Test files | `tests/Surveyor.Application.Tests/AcquisitionFixtureMappingTests.cs`, `tests/Surveyor.Application.Tests/AcquisitionFixtureEdgeTests.cs`, `tests/Surveyor.Application.Tests/AcquisitionFixtureContractTests.cs`, `tests/Surveyor.Application.Tests/AcquisitionScenarios.cs` |

## Implementation Notes

- Application 層に `IUiTreeAcquisitionPort` (`AcquireAsync(TargetReference, AcquisitionOptions, CancellationToken) → AcquisitionResult`) と DTO (`AcquisitionResult` / `AcquisitionOptions` / `AcquisitionProvenance`) を DES-0011 / DES-0014 契約どおり追加した。既存の `OperationStatus` / `RunStage.TreeAcquisition` / `RunDiagnostic` / `TargetReference` とドメインの `Availability` / `ScreenModel` を再利用し、新規ドメイン型は増やしていない。
- 決定的な合成フィクスチャローダを `Surveyor.TestSupport` に置き、`.tree` は confidence / availability を宣言せず rubric 入力 (identity source, provenance, プロパティ完全性, realized/exposed, read outcome) を持つ。confidence と availability はローダが算出する。
- rung 選択・confidence rubric・availability エッジ方針・run レベル rollup / 診断を実装。抽出テキストは `DisplayLabel` にのみ載せキー素材へ渡さない (RQ-052)。既知の取得失敗は status で返し、キャンセルは例外で伝播する (RQ-048)。
- レイヤ/依存方向は変更なし: ポート/DTO は Application、fake seam は `Surveyor.TestSupport`、テストは `Surveyor.Application.Tests` (いずれも portable `net10.0` unit lane)。`Surveyor.slnx` / `eng/Surveyor.Unit.slnf` / `ArchitectureProjectGraphTests` の変更は不要。実 UIA アダプタは `IMP-0013` のスコープ。
- 新規 public API は `PublicAPI.Unshipped.txt` に追記した (RS0016)。class coupling / 複雑度ゲート (CS-06) のため責務を複数クラスへ分離した。

## Verification

| Command | Result |
| --- | --- |
| `dotnet build eng/Surveyor.Unit.slnf` | PR #102 CI unit lane (run 29140761984) 警告0/エラー0 green |
| `dotnet test eng/Surveyor.Unit.slnf` | 全 lane green (`UT-0004` green)、コア層カバレッジ ≥ 80% |
| `dotnet build Surveyor.slnx` + `dotnet format --verify-no-changes` | PR #102 CI windows-build job green |
| `tools/okf/Validate-Okf.ps1` + rq-index freshness | PR #102 CI knowledge job green |

> 注: 実行環境の egress ポリシーにより .NET SDK をローカル取得できないため、ビルド/テストは PR の CI で検証する。RED → GREEN 遷移は commit `7e89f94` (naive, RED) → `553ee5b` (完全実装, GREEN) に残る。

## Residual Risk

- 本スライスは fake / フィクスチャ seam に限定し、実 UIA/MSAA アダプタ・実仮想化・実レガシーエッジ・実 read-only COM 結線は `IMP-0013` / `IT-0002` に残る。
- `AcquisitionOptions.PerNodeReadBudget` の既定値は `DES-0017` で確定するまで暫定 (500ms) であり、フィクスチャでは advisory (read outcome で timeout を明示)。
- confidence rubric / availability エッジ方針のロジックはフィクスチャ fake 内にあり、実アダプタでの再利用整合は `IMP-0013` で確認する。
- ノード単位の `Timeout` / `PermissionDenied` を run レベル `PartialResult` に丸める粒度 (要素単位。DES-0011 §714 の "full Timeout yields FailedUnexpected" はステージ全体で別物) は、`IMP-0013` で実アダプタへ移植する際に設計トレースへ明記する。
