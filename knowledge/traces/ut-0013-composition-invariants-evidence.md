---
type: Unit Test Evidence
title: UT-0013 Composition Root Invariant Evidence
description: Failing-first behavior-test evidence for read-only positive-proof registrations, single clock and policy injection, test-clock isolation, graph completeness, and sanitized composition diagnostics; tests only, with GREEN handed off to IMP-0015.
tags: [unit-test, ut-0013, des-0018, rq-048, rq-051, rq-052, rq-054, composition, dependency-injection]
timestamp: 2026-07-15T00:00:00+09:00
---

# UT-0013 Composition Root Invariant Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `UT-0013`, composition-root injection invariants, unit-test phase |
| Upstream | [DES-0018](../design/des-0018-composition-root-and-di.md) Composition support types / Injection Invariants / Unit-Test Intent / Downstream Handoff; [DES-0005](../design/des-0005-vmodel-traceability-and-downstream-tests.md) `UT-0013`; Issue #52; merged Human design gate PR #112; `RQ-048`; `RQ-054`; `RQ-051`; `RQ-052`; `RD-025`; `RD-026`; `RD-032`; guardrails `R-ARC-01`, `R-QA-01`, `R-AI-02` |
| Downstream | `tests/Surveyor.Application.Tests/UT0013CompositionFixture.cs`; `UT0013ReadOnlyCompositionTests.cs`; `UT0013ClockAndPolicyCompositionTests.cs`; `UT0013GraphCompositionTests.cs`; `UT0013DiagnosticCompositionTests.cs`; `IMP-0015` #73 turns this tests-only RED spec GREEN after prerequisite `IMP-0018` #113 |
| Evidence | Seventeen deterministic cases specify the valid audited graph and every designed counter-example: each sanctioned target port missing; unaudited, duplicate, forbidden, and uninspectable-factory target registrations; clock/policy missing and duplicate; unmarked clock rejected in Test but accepted in Production; four use cases resolved; all violations collected; and diagnostic shape/token sanitization. The test edge references MEDI only from `Surveyor.Application.Tests`; production/Application DI dependencies remain #73 scope. |
| Verification | RED: `dotnet test tests\Surveyor.Application.Tests\Surveyor.Application.Tests.csproj --no-restore --filter UT0013 -v minimal` fails only because `Surveyor.Application.Composition` and its marker/exception contracts do not exist yet (`CS0234` / `CS0246`). Second pass: a temporary uncommitted contract stub made the test project build with 0 warnings / 0 errors, exposing and then eliminating an initial `CA1506` over-coupling finding by splitting graph and diagnostic tests. Regression isolation: with only `UT0013*.cs` temporarily excluded, the existing Application suite passed 31/31 with 87.56% line coverage. The temporary stub/exclusion were removed. |
| Residual Risk | This artifact intentionally remains RED until `IMP-0015` #73 implements `Surveyor.Application.Composition`, canonical marker inheritance, `AddSurveyorCore`, `AddSurveyorFakeAdapters`, and the three still-missing use cases. #73 also owns GREEN coverage, mutation evidence where applicable, and the Windows production-registration smoke. #73 remains dependent on #113's Discovery/UIA boundary migration. Marker presence is composition-time positive proof, not real-target behavioral proof; `UT-0005` and downstream integration tests retain that obligation. |

## Behavior Inventory

### Invariant A — read-only positive proof (`RQ-048`, `RQ-054`)

- A valid audited fake composition passes.
- Removing each of `ITargetDiscoveryPort`, `IUiTreeAcquisitionPort`, and `IScreenCapturePort` yields `Composition.ReadOnly.MissingTargetAdapter` with the affected short service type name.
- An unmarked acquisition adapter yields `Composition.ReadOnly.UnauditedTargetAdapter`.
- A second audited acquisition adapter yields `Composition.ReadOnly.DuplicateTargetAdapter`; silent last-registration-wins behavior is not accepted.
- An unknown `ITargetControlPort : ITargetFacingPort` yields `Composition.ReadOnly.ForbiddenTargetFacingService`.
- A factory descriptor whose implementation type cannot be inspected fails closed as `Composition.ReadOnly.UnauditedTargetAdapter`.

These cases test positive evidence at the registration boundary. They do not infer safety from a class name or merely inspect whether a port exposes mutation methods.

### Invariants B/C/D — multiplicity and test isolation (`RQ-051`, `RQ-052`, `RQ-054`)

- Missing and duplicate `IClock` registrations yield `Composition.Clock.Missing` / `Composition.Clock.Duplicate`.
- Missing and duplicate `IConfidentialityPolicy` registrations yield `Composition.Policy.Missing` / `Composition.Policy.Duplicate`.
- In `CompositionMode.Test`, a sole clock without `ISurveyorCompositionTestDouble` yields `Composition.Clock.RealClockInTest`.
- The same sole unmarked clock is valid in `CompositionMode.Production`, so the oracle depends on mode and marker rather than a concrete clock type name.

### Graph and diagnostics (`RQ-054`, `RQ-052`)

- The valid headless core + fake graph must resolve `SelectTargetUseCase`, `AnalyzeScreenUseCase`, `GenerateReportUseCase`, and `ExportResultUseCase`; merely building a provider without resolving roots is insufficient.
- Validation reports all detected errors in one `CompositionValidationException`, including three distinct missing target-port diagnostics plus missing clock and policy.
- `CompositionDiagnostic` exposes exactly `Code`, `Severity`, `ServiceTypeName`, and `SafeArgs`. Diagnostic keys/values reject path separators, drive/URI separators, raw exception tokens, and target-data tokens.

## Failing-First And Counter-Example Evidence

The committed spec is tests-only, following the established `UT-0005` → `IMP-0007` pattern. Its first restored run is RED at the absent `Surveyor.Application.Composition` namespace and layer-safe marker contracts. A temporary contract-shape stub was used only to compile-check the tests and was removed; it confirmed that the test code itself is analyzer-clean. Every mis-wired collection above is a first-class `R-QA-01` counter-example that a no-op or partial validator cannot satisfy once #73 supplies the contract.

The second-pass `R-AI-02` review found `CA1506` in an initially combined graph/diagnostic test class. Splitting those responsibilities and centralizing multi-invariant fixture removal produced a 0-warning build under the temporary contract without suppressing production or test code.

## IMP-0015 Handoff

`IMP-0015` #73 must:

1. implement the exact DES-0018 composition types and marker inheritance without weakening the fixed diagnostic codes;
2. implement `AddSurveyorCore` and the TestSupport fake registration seam so all four use cases resolve headlessly;
3. turn all seventeen `UT0013` cases GREEN and record coverage/mutation evidence;
4. consume #113's migrated Discovery/UIA boundary rather than the historical UIA-owned registry; and
5. run the separate Windows production-registration smoke. Functional/manual `IT-0007` remains downstream and is not claimed here.
