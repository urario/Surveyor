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
| Downstream | `src/Surveyor.Application/Dto/ReportContracts.cs`; `src/Surveyor.Application/Dto/ArtifactContracts.cs`; `src/Surveyor.Application/Ports/IReportGenerationPort.cs`; `src/Surveyor.Reports/DeterministicReportWriter.cs`; `src/Surveyor.Reports/ReportJsonDocumentFactory.cs`; `src/Surveyor.Reports/JsonReportSerialization.cs`; `src/Surveyor.Reports/AtomicReportFileWriter.cs`; `eng/stryker/reports.stryker-config.json`; `tools/stryker/Run-StrykerBaseline.ps1` |
| Evidence | Added Application-owned report DTOs and the report-generation port, then implemented a UI-independent Reports-layer JSON writer. The serializer writes an explicit document shape with invariant UTC text, invariant numeric text, deterministic list traversal, UTF-8 no BOM, and LF newlines. The file writer writes to a temp file and promotes with no overwrite, returning an `IoError` result on collision while preserving the pre-existing destination bytes. Reports is also registered as a Stryker target so mutation-score evidence can be reproduced through the canonical runner. |
| Verification | `dotnet test tests\Surveyor.Reports.Tests\Surveyor.Reports.Tests.csproj --no-restore --filter UT0006 /p:CollectCoverage=false` passed 2 tests. `dotnet test tests\Surveyor.Reports.Tests\Surveyor.Reports.Tests.csproj --no-restore -v minimal` passed 2 tests with `Surveyor.Reports` line coverage 95.55%. `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -Target Reports -SkipVersionCheck` completed with final mutation score 88.18%. `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\okf\Validate-Okf.ps1` passed for 64 markdown files after the trace update. |
| Residual Risk | The implementation slice is intentionally JSON-only. HTML report output (`IMP-0009` / `UT-0007`), cancellation/timeout race coverage, schema-invalid failure injection, and multi-format all-or-none cleanup remain outside this slice unless promoted by a follow-up issue. Remaining Reports Stryker survivors are below the 80% gate impact and concentrate in unimplemented branch/failure surfaces such as atomic-write edge cases, unsupported-artifact/factory branches, and projection null/empty variants. |

## Mutation Evidence Hook

Reports Stryker execution is available through:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -Target Reports -SkipVersionCheck
```

Generated mutation reports are disposable outputs under `artifacts/stryker/reports/`. The durable score and survivor summary belong in PR evidence and, when material, this trace note or a follow-up `UT` trace.

The first Reports run for this slice produced:

| Target | Mutants | Killed | Survived | No coverage | Ignored | Compile errors | Score |
| -- | --: | --: | --: | --: | --: | --: | --: |
| `Surveyor.Reports` | 555 | 388 | 38 | 14 | 50 | 65 | 88.18% |

Survivors were concentrated in branches not fully exercised by `UT-0006`: `AtomicReportFileWriter.cs` collision/cleanup variants, `DeterministicReportWriter.cs` unsupported/failure paths, `ReportArtifactFactory.cs` alternative result shaping, and projection classes' null/empty variants. These are non-blocking for `IMP-0008` because the configured target is above the `CS-10` 80% threshold and the uncovered behaviors are listed as follow-up report-writer test slices.
