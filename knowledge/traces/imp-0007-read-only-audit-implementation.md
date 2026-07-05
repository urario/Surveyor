---
type: Trace Evidence
title: IMP-0007 Read-Only Acquisition Audit Implementation
description: Implementation evidence for the portable read-only acquisition audit that turns UT-0005 green by evaluating recorded UIA/MSAA/Win32 member invocations against the DES-0014 closed read-only allow-list.
tags: [trace, implementation, imp-0007, ut-0005, des-0014, rq-048, rd-032, read-only]
timestamp: 2026-07-05T00:00:00+09:00
---

# IMP-0007 Read-Only Acquisition Audit Implementation

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `IMP-0007`, read-only acquisition audit implementation, implementation phase |
| Upstream | [DES-0014](../design/des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) Read-Only Audit / Concrete read-only allow-list / Unit-Test Intent; [DES-0008](../design/des-0008-project-structure-and-test-harness.md) unit-lane and architecture guardrails; [UT-0005](ut-0005-read-only-audit-evidence.md); Issue #65; `RQ-048`; `RD-032`; guardrails `R-QA-01`, `R-TEST-04` |
| Downstream | `src/Surveyor.Adapters.Uia.Audit`; `tests/Surveyor.Adapters.Uia.Tests/ReadOnlyAuditBehaviorTests.cs`; `eng/Surveyor.Unit.slnf`; `Surveyor.slnx`; future `IMP-0013` real raw-COM reader wiring and `IT-0001` real target state-invariance |
| Evidence | Added a portable `Surveyor.Adapters.Uia.Audit` source project containing `ReadOnlyAcquisitionSpy`, `ReadOnlyAcquisitionAudit`, and `ReadOnlyAuditResult`. The audit treats the DES-0014 read-only member set as a closed positive allow-list, rejects empty recordings as insufficient evidence, preserves deterministic first-seen violation order, and exposes no public API surface. Registered the project and `Surveyor.Adapters.Uia.Tests` in the unit solution filter, and updated architecture-test expectations so the new portable audit assembly is mechanically visible in the fast lane. |
| Verification | RED before implementation: `dotnet build tests\Surveyor.Adapters.Uia.Tests\Surveyor.Adapters.Uia.Tests.csproj -v minimal` failed only with `CS0234`/`CS0246` for missing `Surveyor.Adapters.Uia.Audit` types. GREEN after implementation and PR #94 review response: `dotnet build eng\Surveyor.Unit.slnf -v minimal` succeeded with 0 warnings / 0 errors; `dotnet test eng\Surveyor.Unit.slnf --no-build -v minimal` passed Architecture 8, Domain 26, Policy 19, and Adapters.Uia 52 tests; Domain line coverage 91.43%, Policy line coverage 94.47%; `dotnet format --verify-no-changes --no-restore` exit code 0; `tools/okf/Validate-Okf.ps1` passed for 47 markdown files. |
| Residual Risk | This slice implements the portable invocation audit only. Real raw-COM acquisition still must record actual invocations into the spy in `IMP-0013`; target state-invariance remains an `IT-0001` manual/integration obligation. `RunDiagnostic.SafeArgs` checks remain a `UT-0004`/adapter diagnostic obligation because the audit implementation has no diagnostics surface yet. The `IUIAutomationTextEditPattern.SetValue` fixture token remains a namespace-level sentinel until the real raw-COM reader finalizes its exact vocabulary mapping. |

## Implemented Contract

- `ReadOnlyAcquisitionSpy.RecordInvocation(string memberId)` records the audited member identifier in first-seen order and rejects null, empty, or whitespace identifiers.
- `ReadOnlyAcquisitionSpy.InvokedMembers` exposes the recorded sequence to the audit without giving callers mutation methods beyond the spy itself.
- `ReadOnlyAcquisitionAudit.ReadOnlyAllowList` exposes the DES-0014 closed positive read-only member set for test and review visibility.
- `ReadOnlyAcquisitionAudit.Evaluate(ReadOnlyAcquisitionSpy spy)` returns read-only only when at least one invocation was recorded and every member is allow-listed.
- `ReadOnlyAuditResult` returns `IsReadOnly` plus a deterministic list of distinct offending members in first-seen order.

## Design Notes

- Pattern: no GoF pattern added. A direct spy plus pure policy evaluator is sufficient for the current variation point; Strategy would add indirection without a second policy.
- Public API: none. The audit assembly keeps all types `internal`; `Surveyor.Adapters.Uia.Tests` receives explicit `InternalsVisibleTo`.
- Layering: `Surveyor.Adapters.Uia.Audit` is portable `net10.0` and has no Surveyor project references. It stays outside Windows-facing TFM constraints so `UT-0005` can run in the deterministic unit lane.

## Quality Gate Evidence

```text
> dotnet build eng\Surveyor.Unit.slnf -v minimal
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

```text
> dotnet test eng\Surveyor.Unit.slnf --no-build -v minimal
Passed! - Failed: 0, Passed: 52 - Surveyor.Adapters.Uia.Tests.dll
Passed! - Failed: 0, Passed: 8 - Surveyor.Architecture.Tests.dll
Passed! - Failed: 0, Passed: 19 - Surveyor.Policy.Tests.dll
Passed! - Failed: 0, Passed: 26 - Surveyor.Domain.Tests.dll

| Module          | Line   | Branch | Method |
| Surveyor.Policy | 94.47% | 82.85% | 100%   |
| Surveyor.Domain | 91.43% | 81.26% | 89.53% |
```

```text
> dotnet format --verify-no-changes --no-restore
(exit code 0)
```

```text
> powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\okf\Validate-Okf.ps1
OKF validation passed for 47 markdown files under knowledge.
```

Stryker.NET (`CS-10`): not run in this slice because no repository Stryker configuration or local/global `dotnet-stryker` tool is installed (`rg --files -g '*stryker*' -g '.config/dotnet-tools.json` returned no files; `dotnet stryker --version` returned "command or file was not found").
