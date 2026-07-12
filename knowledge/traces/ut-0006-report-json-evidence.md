---
type: Trace Evidence
title: UT-0006 Report JSON Determinism and Atomicity Evidence
description: Unit-test evidence for DES-0012 JSON byte stability, LF/no-BOM golden governance, and no-partial destination-collision behavior.
tags: [trace, unit-test, ut-0006, des-0012, rq-031, rq-051, rq-052, rq-054, golden]
timestamp: 2026-07-12T00:00:00+09:00
---

# UT-0006 Report JSON Determinism and Atomicity Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `UT-0006`, report JSON unit-test evidence |
| Upstream | [DES-0012](../design/des-0012-report-schema-and-deterministic-serialization.md); [UT-0010](ut-0010-clock-determinism-evidence.md); `RQ-031`, `RQ-051`, `RQ-052`, `RQ-054`; Issue #45 |
| Downstream | `tests/Surveyor.Reports.Tests/ReportJsonBehaviorTests.cs`; `tests/Surveyor.Reports.Tests/ReportFixture.cs`; `tests/fixtures/reports/des-0012/golden/report-v1.happy.json`; [IMP-0008](imp-0008-report-writer-implementation.md) |
| Evidence | The UT-0006 suite pins the v1 JSON bytes against the governed DES-0012 golden, verifies UTF-8 without BOM, LF-only newlines, exact byte equality after repeat generation, byte-stable output under `tr-TR`, and byte-stable output from a fresh `dotnet test` process. It also verifies destination collision preserves existing bytes, multi-format requests leave no partial JSON artifact, later JSON write failure cleans already written artifacts, DES-0012 invariant mismatches return `SchemaInvalid`, not-writable-as-file destinations return `IoError` instead of leaking an exception, and caller cancellation leaves no artifact. |
| Verification | Review-response verification: `dotnet test tests\Surveyor.Reports.Tests\Surveyor.Reports.Tests.csproj --no-restore -v minimal` passed 7 tests with `Surveyor.Reports` line coverage 96.94% and branch coverage 78.57%; golden semantic compare and golden regeneration scripts both passed. |
| Residual Risk | `UT-0006` now covers the JSON writer happy path, collision/no-partial, multi-format no-partial, invariant mismatch, cancellation, and representative IO failure behavior. Timeout race injection and richer projection variants remain follow-up hardening scope. |

## Protected Golden Meaning

`tests/fixtures/reports/des-0012/golden/report-v1.happy.json` is not an arbitrary snapshot. It protects:

- explicit v1 property order and required section shape;
- invariant UTC timestamp text and basis-point/percent formatting;
- safe identifier-only output, without raw display labels, absolute paths, screenshots, or raw exception messages;
- `Unavailable(PermissionDenied)` preservation as unavailable state, not a low score;
- LF newline normalization and UTF-8 no-BOM bytes.

Golden regeneration must follow the DES-0012 governance rule: review the semantic diff, confirm the upstream contract change, then update this trace or the PR evidence.

## Golden Governance Commands

Regenerate the governed JSON golden from the production writer:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\reports\Update-GoldenReports.ps1
```

Compare semantic equivalence between two report JSON files before reviewing byte-level golden changes:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\reports\Compare-ReportSemantics.ps1 -ExpectedPath .\tests\fixtures\reports\des-0012\golden\report-v1.happy.json -ActualPath <candidate.json>
```
