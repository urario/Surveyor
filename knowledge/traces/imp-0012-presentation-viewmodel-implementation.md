---
okf_version: "0.1"
type: Implementation Evidence
id: IMP-0012
title: Presentation ViewModel and Ports Implementation
status: implemented-with-risk
tags: [surveyor, implementation, presentation, viewmodel, rq-046, rq-052, rq-054]
---

# IMP-0012 Presentation ViewModel and Ports Implementation

## Trace

| Field | Evidence |
| --- | --- |
| Artifact | `IMP-0012`, `M02` ViewModel and presentation-port implementation |
| Issue | #70 |
| Upstream | `DES-0016`, `UT-0011`, `IMP-0011`, `DES-0011`, `DES-0013` |
| Requirements | `RQ-046`, `RQ-052`, `RQ-054`; `RD-016`, `RD-022`, `RD-025`, `RD-028`, `RD-030` |
| Production | `src/Surveyor.Presentation/Ports/*`, `src/Surveyor.Presentation/ViewModels/*`, `src/Surveyor.Presentation/PublicAPI.Unshipped.txt` |
| Tests | `UT-0011` Presentation behavior suite |

## Implementation

- Added presentation ports for navigation, dialogs, UI dispatch, and HTML preview. Only these assembly-boundary contracts are public; ViewModel implementations remain internal until concrete App composition requires a wider surface.
- Added a WinUI-independent `ShellViewModel` reducer covering metadata gating, run state/activity transitions, navigation blocking, analysis cancel, post-review report cancel, metadata reset after terminal runs, exception cleanup, and report-result status propagation.
- Added deterministic `FindingSelectionState` using IDs rather than list indexes and preserving incoming result order.
- Added `ReportExportViewModel` for `opt-out-reason-v1` allowlisted session opt-out confirmation and plaintext-preview confirmation before external preview host invocation. `ShellViewModel.GenerateReportAsync` defaults to `ProtectedLocal` without showing an opt-out dialog, applies a previously confirmed session opt-out when present, and no longer fabricates a Unix-epoch protected-local request.
- Registered `Surveyor.Presentation.Tests` in `Surveyor.slnx`, `eng/Surveyor.Unit.slnf`, and architecture project-graph expectations. `Surveyor.Presentation` and its tests stay on `net10.0` so the Ubuntu headless unit lane can restore and run them; WinUI-facing projects remain on the Windows TFM.

## Design and Quality Notes

- Pattern note: the presentation ports are Adapter boundaries, purpose-built to keep ViewModels independent of WinUI and external browser APIs (`RQ-054`); direct WinUI calls were rejected because they would make `UT-0011` require a live UI surface.
- `ShellViewModel` carries a targeted `CA1506` suppression because `DES-0016` makes it the single reducer joining Application DTOs and presentation ports. The suppression is local, justified in source, and the behavior is guarded by focused tests.
- Test fixture classes carry targeted `CA1506` suppressions because they intentionally compose Application and Domain DTOs for deterministic UT-0011 fixtures.
- Public API additions are limited to `Surveyor.Presentation.Ports` and are tracked in `PublicAPI.Unshipped.txt`.

## Verification

| Command | Result |
| --- | --- |
| `dotnet restore Surveyor.slnx` | PASS |
| `dotnet build Surveyor.slnx --no-restore` | PASS: 0 warnings, 0 errors |
| `dotnet test tests\Surveyor.Presentation.Tests\Surveyor.Presentation.Tests.csproj --filter UT0011 --no-restore` | PASS: 13 tests |
| `dotnet test tests\Surveyor.Architecture.Tests\Surveyor.Architecture.Tests.csproj --no-restore` | PASS: 8 tests |
| `dotnet test eng\Surveyor.Unit.slnf --no-build --logger trx --results-directory artifacts\test-results` | PASS: Architecture 8, Presentation 13, Application 31, Domain 59, Policy 45, Reports 7 |
| `git diff --check` | PASS |

## Residual Risk

- Review gate #38 for `DES-0016` remains open; #70 should move at most to review/blocked-with-risk until that gate is closed or explicitly accepted with risks.
- `AnalyzeScreenUseCase` still lacks the DES-0016 progress-parameter public API. `ShellViewModel` therefore depends on an internal `IAnalysisRunner` seam for UT-0011; concrete App composition should bind that seam once the Application use-case contract is updated.
- `ReportCommandRequest.ConfidentialityRequest` remains an internal presentation seam for this slice. PR #109 review identified that the future App composition must align report generation with the actual Application `GenerateReportUseCase` / `GenerateReportRequest` contract rather than treating this seam as an upstream DTO; follow-up is tracked as #110 (`IMP-0017`).
- This slice does not implement XAML pages, resource strings, or real `IHtmlPreviewHost`; those remain App/IT-0007 scope.
- CS-10 Stryker execution was not run for this Presentation slice. Existing Stryker baseline targets Domain/Policy; UT-0011 supplies behavior-level counter-example coverage for this non-core ViewModel layer.
