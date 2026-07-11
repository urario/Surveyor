---
type: Trace Evidence
title: IMP-0013 Real UIA Acquisition Adapter Implementation
description: Implementation evidence for the real UIA acquisition adapter skeleton, raw-reader seam, read-only audit wiring, target-handle registry, and adapter contract tests under DES-0014.
tags: [trace, implementation, imp-0013, des-0014, rq-017, rq-026, rq-048, rq-049, read-only, uia]
timestamp: 2026-07-11T00:00:00+09:00
---

# IMP-0013 Real UIA Acquisition Adapter Implementation

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `IMP-0013`, real UIA acquisition adapter, implementation phase |
| Upstream | [DES-0014](../design/des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md); [ADR-0002](../decisions/adr-0002-adapter-technology-selection.md); [IMP-0006](imp-0006-acquisition-port-implementation.md); [IMP-0007](imp-0007-read-only-audit-implementation.md); Issue #71; `RQ-017`, `RQ-026`, `RQ-048`, `RQ-049`; `RD-003`, `RD-004`, `RD-023`, `RD-032` |
| Downstream | `IT-0001` target state-invariance, `IT-0002` live UIA legacy-edge comparison, `IT-0005` integrity/permission validation, `IT-0006` cancellation/timeout calibration, future `DES-0018` / `IMP-0015` composition wiring |
| Evidence | Added `Surveyor.Adapters.Uia.UiaTreeAcquisitionAdapter` implementing `IUiTreeAcquisitionPort`; `UiaTargetHandleRegistry` for opaque `TargetReference` token to HWND/process-image mapping inside the adapter boundary; internal `IRawUiaReader` / `RawUiaNode` / `RawUiaReadResult` seam; `DynamicRawUiaReader` that activates the Windows UIA COM client, attempts `IUIAutomation6` call-budget configuration, and records read-only member invocations; adapter-side mapper that reuses the DES-0014 identity rung, confidence, availability, and run-level rollup semantics; read-only audit wiring using `ReadOnlyAcquisitionAudit`; and adapter tests covering mapping, audit violation, unresolved target, caller cancellation, and registry process metadata propagation. |
| Verification | `dotnet test tests\Surveyor.Adapters.Uia.Tests\Surveyor.Adapters.Uia.Tests.csproj --no-restore -v minimal` passed 57 tests. `dotnet test eng\Surveyor.Unit.slnf --no-restore -v minimal` passed Architecture 8, Domain 59, Application 23, Policy 45; Domain line coverage 97.01%, Application 100%, Policy 100%. `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -SkipVersionCheck` passed with Domain mutation score 80.54% and Policy mutation score 89.02%. |
| Residual Risk | The raw COM path is compiled and structurally wired, but this slice did not run a live Windows target smoke acquisition. `DynamicRawUiaReader` uses the installed UIA COM type information through dynamic COM dispatch; real legacy target behavior, MSAA/`WM_GETTEXT` fallback breadth, COM-level timeout calibration, and state-invariance remain `IT-0001`/`IT-0002`/`IT-0005`/`IT-0006` obligations. Discovery-owned registry population is not implemented here; `UiaTargetHandleRegistry` is the adapter-side mechanism for future `DES-0018`/`IMP-0015` wiring and now carries process image metadata when available. |

## Implemented Contract

- `UiaTreeAcquisitionAdapter.AcquireAsync` resolves only registry-issued opaque tokens, checks cancellation before raw reads, maps known target failures to `AcquisitionResult.Status`, and never throws for expected target unavailability.
- `UiaTargetHandleRegistry` keeps HWND values inside the adapter assembly boundary and returns `TargetReference` values with opaque `SessionTargetId` tokens. Process image names stay in the same adapter-side registry entry so `ScreenIdentity` does not collapse unrelated target processes to a hard-coded value.
- `IRawUiaReader` is the fakeable COM-read boundary required by `DES-0014`; adapter tests substitute it without bypassing mapper, audit, or status behavior.
- `DynamicRawUiaReader` records DES-0014 read-only calls into `ReadOnlyAcquisitionSpy` and builds `RawUiaNode` trees from UIA current properties and raw-view traversal. It attempts `IUIAutomation6` `ConnectionTimeout` / `TransactionTimeout` configuration before target reads and emits `Acquisition.UiaCallBudget.Fallback` if that capability is unavailable.
- Offscreen status is intentionally not mapped to `UnavailableReason.NotExposed`; DES-0014 treats offscreen elements as existing nodes, with bounds handling deferred to DES-0015.
- `UiaAcquisitionModelMapper` converts raw nodes into `ScreenModel` / `UiElement` with stable rung selection, fallback token derivation through `IFallbackKeyDerivation`, `Unavailable(NotRealized)` rollup, and top-down confidence classification.

## Design Notes

- Pattern: Adapter. Purpose: keep Windows UIA COM and HWND handling behind the Application-owned `IUiTreeAcquisitionPort`. Rejected simpler alternative: direct UIA calls from use cases would violate `RQ-054` layering and make `RQ-048` audit wiring untestable.
- Public API: `UiaTreeAcquisitionAdapter` and `UiaTargetHandleRegistry` are public because composition/discovery wiring will cross the adapter assembly boundary. All raw-reader and mapper seams remain internal and test-visible through `InternalsVisibleTo`.
- Read-only guardrail: the real adapter evaluates the `ReadOnlyAcquisitionSpy` after every raw read. A prohibited or unlisted invocation returns `OperationStatus.Unavailable` with a safe diagnostic instead of exposing raw target data.

## Quality Gate Evidence

```text
> dotnet test tests\Surveyor.Adapters.Uia.Tests\Surveyor.Adapters.Uia.Tests.csproj --no-restore -v minimal
Passed! - Failed: 0, Passed: 57, Skipped: 0, Total: 57
```

```text
> dotnet test eng\Surveyor.Unit.slnf --no-restore -v minimal
Passed! - Failed: 0, Passed: 8  - Surveyor.Architecture.Tests.dll
Passed! - Failed: 0, Passed: 59 - Surveyor.Domain.Tests.dll
Passed! - Failed: 0, Passed: 23 - Surveyor.Application.Tests.dll
Passed! - Failed: 0, Passed: 45 - Surveyor.Policy.Tests.dll

Coverage:
Surveyor.Domain      97.01% line
Surveyor.Application 100% line
Surveyor.Policy      100% line
```

```text
> powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -SkipVersionCheck
Domain: final mutation score 80.54% (414 killed / 93 survived / 7 no coverage / 99 compile errors / 742 total)
Policy: final mutation score 89.02% (73 killed / 9 survived / 0 no coverage / 12 compile errors / 120 total)
```

Stryker.NET (`CS-10`) was run through the canonical `IMP-0016` runner. Both configured core-layer targets met the `>= 80%` score target, so no mutation-score remediation was required.
