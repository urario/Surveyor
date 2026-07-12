---
type: Trace Evidence
title: IMP-0008 Deterministic JSON Report Writer Implementation
description: Implementation evidence for the DES-0012 JSON report writer, Application report contracts, deterministic serializer, and atomic no-partial file writes.
tags: [trace, implementation, imp-0008, des-0012, rq-031, rq-051, rq-052, rq-054, stryker]
timestamp: 2026-07-12T00:00:00+09:00
---

# IMP-0008 Deterministic JSON Report Writer Implementation

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `IMP-0008`, deterministic JSON report writer implementation |
| Upstream | [DES-0012](../design/des-0012-report-schema-and-deterministic-serialization.md); [UT-0006](ut-0006-report-json-evidence.md); [IMP-0004](imp-0004-clock-seam-implementation.md); `RQ-031`, `RQ-051`, `RQ-052`, `RQ-054`; Issue #66 |
| Downstream | `src/Surveyor.Application/Dto/RunId.cs`; `src/Surveyor.Application/Dto/AnalysisRunResult.cs`; `src/Surveyor.Application/Dto/ArtifactContracts.cs`; `src/Surveyor.Application/Dto/ReportContracts.cs`; `src/Surveyor.Application/Ports/IReportGenerationPort.cs`; `src/Surveyor.Application/UseCases/AnalysisRunContext.cs`; `src/Surveyor.Application/UseCases/AnalysisRunResultBuilder.cs`; `src/Surveyor.Application/UseCases/AnalyzeScreenUseCase.cs`; `src/Surveyor.Reports/DeterministicReportWriter.cs`; `src/Surveyor.Reports/ReportJsonDocumentFactory.cs`; `src/Surveyor.Reports/JsonReportSerialization.cs`; `src/Surveyor.Reports/AtomicReportFileWriter.cs`; `eng/stryker/reports.stryker-config.json`; `tools/stryker/Run-StrykerBaseline.ps1`; `tools/reports/Update-GoldenReports.ps1`; `tools/reports/Compare-ReportSemantics.ps1` |
| Evidence | Added Application-owned report DTOs and the report-generation port, then implemented a UI-independent Reports-layer JSON writer. The serializer writes an explicit document shape with invariant UTC text, invariant numeric text, deterministic list traversal, UTF-8 no BOM, and LF newlines. The file writer writes to a temp file and promotes with no overwrite, returning an `IoError` result on collision or unauthorized/not-writable destinations while preserving existing bytes. Multi-format requests are rejected before writing, and later write failure cleans artifacts written by the same command to preserve DES-0012 all-or-none semantics. Reports is also registered as a Stryker target so mutation-score evidence can be reproduced through the canonical runner. |
| Verification | Review-response verification: `dotnet test tests\Surveyor.Reports.Tests\Surveyor.Reports.Tests.csproj --no-restore --filter UT0006 /p:CollectCoverage=false` passed 6 tests; `dotnet test tests\Surveyor.Reports.Tests\Surveyor.Reports.Tests.csproj --no-restore -v minimal` passed 6 tests with `Surveyor.Reports` line coverage 96.94%; `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\reports\Compare-ReportSemantics.ps1 -ExpectedPath .\tests\fixtures\reports\des-0012\golden\report-v1.happy.json -ActualPath .\tests\fixtures\reports\des-0012\golden\report-v1.happy.json` passed; `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\reports\Update-GoldenReports.ps1` passed; Reports Stryker final mutation score 88.99%; OKF validation passed for 64 markdown files. |
| Residual Risk | The implementation slice is intentionally JSON-only. HTML report output (`IMP-0009` / `UT-0007`), cancellation/timeout race injection, and richer projection variants remain outside this slice unless promoted by a follow-up issue. Remaining Reports Stryker survivors are below the 80% gate impact and concentrate in less common projection null/empty variants and durability windows. |

`RunId.New()` uses `Guid.NewGuid()` only to allocate an opaque per-run identifier before report generation. `UT-0006` fixes `RunId` in the fixture and `ReportJsonDocumentFactory` rejects a `ReportRequest.RunId` that differs from `SanitizedRunResult.RunId`, so random ID allocation cannot enter a deterministic report byte comparison for the same run input.

## Mutation Evidence Hook

Reports Stryker execution is available through:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -Target Reports -SkipVersionCheck
```

Generated mutation reports are disposable outputs under `artifacts/stryker/reports/`. The durable score and survivor summary belong in PR evidence and, when material, this trace note or a follow-up `UT` trace.

The first Reports run for this slice produced:

| Target | Mutants | Killed | Survived | No coverage | Ignored | Compile errors | Score |
| -- | --: | --: | --: | --: | --: | --: | --: |
| `Surveyor.Reports` | 575 | 404 | 37 | 13 | 56 | 65 | 88.99% |

Survivors were concentrated in branches not fully exercised by `UT-0006`: `AtomicReportFileWriter.cs` collision/cleanup variants, `DeterministicReportWriter.cs` unsupported/failure paths, `ReportArtifactFactory.cs` alternative result shaping, and projection classes' null/empty variants. These are non-blocking for `IMP-0008` because the configured target is above the `CS-10` 80% threshold and the uncovered behaviors are listed as follow-up report-writer test slices.
