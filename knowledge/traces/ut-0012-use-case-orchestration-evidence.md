---
okf_version: "0.1"
type: Unit Test Evidence
id: UT-0012
title: Use-Case Orchestration Behavior Evidence
status: verified
tags: [surveyor, unit-test, orchestration, tdd, rq-046, rq-048, rq-054]
---

# UT-0012 Use-Case Orchestration Behavior Evidence

## Trace

| Field | Evidence |
| --- | --- |
| Artifact | `UT-0012`, fake-based `AnalyzeScreenUseCase` behavior tests |
| Issue | #51 |
| Upstream | `DES-0004`, `DES-0005`, `DES-0011`; `IMP-0005`, `IMP-0006` |
| Requirements | `RQ-046`, `RQ-048`, `RQ-054`; `RD-001`, `RD-016`, `RD-025`, `RD-032` |
| Downstream | `IMP-0011` (#69) |
| Test files | `tests/Surveyor.Application.Tests/Orchestration*BehaviorTests.cs`, `OrchestrationAssertions.cs`, recording fake ports |

## Behavior Coverage

- Acquisition, scoring, capture, confidentiality-policy decision, and store stages are asserted in fixed order. The analysis pipeline does not call discovery or report generation: `DES-0011` separates those into `SelectTargetUseCase` and `GenerateReportUseCase`; mixing them into `AnalyzeScreenUseCase` would violate the accepted use-case split.
- Caller cancellation during acquisition produces `Cancelled` and proves that capture, policy, and store are not called.
- Acquisition `PartialResult` plus optional capture `Timeout` aggregates to `SucceededWithPartialResult` while retaining per-stage statuses.
- `RequireCapture == true` plus capture `Timeout` yields `FailedUnexpected`, matching the `DES-0011` stage-criticality rule for required capture.
- The confidentiality decision gate is observed before store and the exact decision is carried into the run result.
- The store port receives a pre-store `StoreRequest` snapshot that excludes the `Store` stage itself; the final returned `AnalysisRunResult` alone reflects the `StoreResult`. This prevents a self-referential persisted snapshot from claiming a store outcome before the save call completes.
- `AcquisitionResult(Status: Cancelled, ScreenModel: null)` now terminates as `RunOutcome.Cancelled` rather than being folded into `FailedUnexpected`.
- User-provided `ScreenSelectionMetadata` is preserved by reference and mapped field-for-field to `PriorityBasis`; absent metadata remains absent and does not fabricate priority (`RD-016`).
- Fixed-clock start/completion timestamps prove ambient local time is not read.

## TDD and Counter-Example Evidence

1. RED before production: the focused test build failed with `CS0234` / `CS0246` for the not-yet-implemented use-case, DTO, capture/store/config ports, and results.
2. GREEN after `IMP-0011`: all five UT-0012 behavior tests passed.
3. `R-QA-01` stage-order counter-example: a temporary implementation reversal produced expected `TreeAcquisition -> Scoring -> Capture -> ConfidentialityPolicy -> Store` versus actual reverse order and failed at position 0.
4. `R-QA-01` metadata counter-example: a temporary replacement with `null` failed `Assert.Same(expected metadata, actual null)`.
5. Both counter-examples were reverted; the follow-up review fix added three more orchestration tests, and the final Application suite passed 31/31.

## Verification

| Command | Result |
| --- | --- |
| `dotnet test tests\Surveyor.Application.Tests\Surveyor.Application.Tests.csproj --no-restore` | PASS: 31 tests; Application line coverage 95.85% |
| `dotnet test eng\Surveyor.Unit.slnf --no-restore` | PASS: Architecture 8, Application 31, Domain 59, Policy 45 |
| `dotnet build Surveyor.slnx --no-restore` | PASS: 0 warnings, 0 errors |
| `dotnet format Surveyor.slnx --verify-no-changes --no-restore` | PASS |

## Second-Pass Smell Review

- Each contract has a separate behavior test rather than one happy-path test.
- Fakes record only observable port calls; tests do not depend on Windows GUI state, wall-clock delay, file I/O, or adapter internals.
- Assertions identify stage-order, cancellation, partial aggregation, policy ordering, or metadata carriage failures directly.
- CA1506 exposed excessive coupling in the initial monolithic candidate; production and test helpers were split by responsibility without suppression.

## Residual Risk

- Real capture, store, and Windows target behavior remains adapter/integration scope (`IMP-0014`, `IMP-0010`, `IT-0001` to `IT-0003`).
- This slice records `ConfidentialityDecision` but does not yet apply `RequiresTextMasking` or produce the protected-store model; text masking / protected persistence remain `IMP-0010` scope and are intentionally not simulated inside `UT-0012`.
- This slice exercises timeout as an explicit port status. Deterministic timeout-controller race arbitration remains a downstream extension when the `IStageTimeoutController` concrete seam is introduced.
