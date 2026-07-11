---
okf_version: "0.1"
type: Implementation Evidence
id: IMP-0011
title: Use-Case Orchestration Implementation
status: implemented
tags: [surveyor, implementation, application, orchestration, rq-046, rq-048, rq-054]
---

# IMP-0011 Use-Case Orchestration Implementation

## Trace

| Field | Evidence |
| --- | --- |
| Artifact | `IMP-0011`, `M03` analysis use-case orchestration |
| Issue | #69 |
| Upstream | `DES-0011`, `UT-0012`, `IMP-0004`, `IMP-0005`, `IMP-0006` |
| Requirements | `RQ-046`, `RQ-048`, `RQ-054`; `RD-001`, `RD-016`, `RD-025`, `RD-032` |
| Production | `src/Surveyor.Application/Dto/*Run*.cs`, capture/store DTOs and ports, `UseCases/AnalyzeScreenUseCase.cs`, stage runners and run context |
| Tests | `UT-0012` fake-based behavior suite |

## Implementation

- Added immutable Application-owned request/result, stage, capture, store, and screen-selection DTOs plus capture/store/scoring-config ports. All boundary APIs carry Japanese XML documentation and are tracked in `PublicAPI.Unshipped.txt`.
- `AnalyzeScreenUseCase` executes a UI-independent pipeline over inward-facing ports. The Application assembly contains no WinUI, UIA, capture API, or file-system implementation dependency (`RQ-054`).
- Acquisition cancellation stops all later work; recoverable acquisition/capture/store statuses aggregate to a partial run; required-capture failure maps to unexpected failure.
- `ScreenSelectionMetadata` is copied unchanged into `AnalysisRunResult` and mapped without ranking/recomputation to Domain `PriorityBasis` (`RD-016`).
- The existing `IConfidentialityPolicy.Decide` contract is the pre-store egress gate. Full sanitize/protected-store transformation stays with `IMP-0010` rather than being duplicated here.
- Diagnostics are sorted by stage, severity, code, and element key. Timestamps come only from injected `IClock`.

## Design and Quality Notes

- No GoF catalog pattern was introduced. A direct monolithic method was initially the simplest candidate, but CA1506 measured excessive coupling; the code was refactored into single-purpose internal stage runners plus `AnalysisRunContext` and `AnalysisPipeline`.
- No analyzer, metrics, coverage, or formatting suppression was added.
- `CS-10` mutation execution is N/A for this slice: the repository's canonical Stryker baseline currently targets Domain and Policy, while this change is confined to Application. UT-0012 counter-example runs provide mutation-style evidence for the two Issue-mandated faults.

## Verification

| Command | Result |
| --- | --- |
| `dotnet build Surveyor.slnx --no-restore` | PASS: 0 warnings, 0 errors; Public API, CA and metrics gates included |
| `dotnet test eng\Surveyor.Unit.slnf --no-restore` | PASS: Architecture 8, Application 28, Domain 59, Policy 45 |
| Application coverage | PASS: line 94.71%, branch 65%, method 96.66%; line target >= 80% |
| `dotnet format Surveyor.slnx --verify-no-changes --no-restore` | PASS |

## Residual Risk

- Capture/store ports in this slice are contracts exercised by fakes; real Windows capture and protected persistence are downstream adapter work.
- Full `IConfidentialityPolicy.Apply` / protected model consistency depends on `IMP-0010`; this slice intentionally does not invent its DTOs.
- Live read-only state invariance remains the human/Windows integration gate (`IT-0001`).

