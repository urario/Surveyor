---
type: Trace Evidence
title: UT-0005 Read-Only Acquisition Audit Behavior Evidence
description: Behavior-test evidence for the read-only acquisition audit spy that fails when any prohibited state-changing UIA pattern is invoked during acquisition, covering the full DES-0014 prohibited-pattern list, the closed positive allow-list, the WM_GETTEXT exception boundary, SafeArgs handoff scope, and the R-QA-01 mutating-fake-provider counter-example. Failing-first RED evidence is closed by IMP-0007.
tags: [trace, unit-test, ut-0005, des-0014, rq-048, rd-032, read-only]
timestamp: 2026-07-05T00:00:00+09:00
---

# UT-0005 Read-Only Acquisition Audit Behavior Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `UT-0005`, read-only acquisition audit spy behavior, unit-test phase |
| Upstream | [DES-0014](../design/des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) Read-Only Audit / Concrete read-only allow-list / Unit-Test Intent; [DES-0007](../design/des-0007-detailed-design-execution-strategy.md) `UT-0005` intent and guardrail failing-first matrix; Issue #44; `RQ-048`; `RD-032`; guardrails `R-QA-01`, `R-TEST-04`, `R-SEC-02` |
| Downstream | `tests/Surveyor.Adapters.Uia.Tests/ReadOnlyAuditBehaviorTests.cs`; `Surveyor.slnx`; [IMP-0007](imp-0007-read-only-audit-implementation.md); complemented by `UT-0004`/adapter SafeArgs diagnostics, `IT-0001` (real target state-invariance), and `IMP-0013` (real COM spy wiring) |
| Evidence | Authored the `UT-0005` behavior spec: an allow-listed read-only acquisition passes; each prohibited state-changing pattern from the DES-0014 map fails the audit (theory over the full list); every documented DES-0014 read-only member is asserted as accepted by the positive allow-list; a member outside the closed positive allow-list fails even if not obviously mutating; `WM_GETTEXT` via `SendMessageTimeout` is allowed while `WM_SETTEXT` and bare `SendMessage` fail; violations enumerate every offending member; an empty recording is not falsely claimed read-only; and a mutating fake acquisition provider turns the spy red (`R-QA-01`). |
| Verification | RED (failing-first) confirmed before implementation: `dotnet build tests\Surveyor.Adapters.Uia.Tests\Surveyor.Adapters.Uia.Tests.csproj -v minimal` failed only with `CS0234`/`CS0246` for missing `Surveyor.Adapters.Uia.Audit` types. GREEN closed by `IMP-0007`: `dotnet test tests\Surveyor.Adapters.Uia.Tests\Surveyor.Adapters.Uia.Tests.csproj --no-build -v minimal` passed 51 tests; `dotnet test eng\Surveyor.Unit.slnf --no-build -v minimal` passed Architecture 8, Domain 26, Policy 19, and Adapters.Uia 51 tests. |
| Residual Risk | The behavior spec is green and registered in `eng/Surveyor.Unit.slnf`. The spy audits invocations it can observe; real-target state-invariance is `IT-0001`, and wiring the recording spy to the real raw-COM reader is `IMP-0013`. `RunDiagnostic.SafeArgs` remains a `UT-0004`/adapter diagnostic obligation because the audit result has no diagnostic surface yet. The audit member vocabulary uses `Interface.Member` string identifiers matching the DES-0014 tables; if the real adapter adopts a strongly-typed member enum, the fixture strings map 1:1. `IUIAutomationTextEditPattern.SetValue` is a namespace-level prohibited-member sentinel for the DES-0014 "Text edit" row; `IMP-0013` must reconcile the final string/enum token with the actual raw-COM reader vocabulary without weakening the prohibited operation. |

## Behavior Tests

`ReadOnlyAuditBehaviorTests`:

- `ReadOnlyAcquisitionPassesAudit`
- `ProhibitedPatternFailsAudit` (theory over the full DES-0014 prohibited-pattern list)
- `MutatingFakeProviderTurnsSpyRed` (`R-QA-01` counter-example)
- `MemberOutsideAllowListFailsEvenIfNotObviouslyMutating` (closed positive list, `R-TEST-04`)
- `WmGetTextIsAllowedButWmSetTextAndBareSendMessageFail`
- `DocumentedReadOnlyMemberPassesAudit` (theory over the DES-0014 concrete read-only allow-list)
- `ViolationsEnumerateEveryOffendingMember`
- `EmptyRecordingIsReportedHonestly`
- `AllowListContainsDocumentedReadOnlyMembers`

## Meaningful Oracle And Anti-Pattern Avoided

Per DES-0014 Unit-Test Intent, the oracle is "the invoked COM member set over a full acquisition is a subset of the concrete allow-list; any state-changing pattern, bare `SendMessage`, or `WM_SETTEXT` fails." The tests exercise the audit's decision logic against realistic recorded invocation sequences produced by fake acquisition providers, not the anti-pattern of "merely checking the port type exposes no mutation method" from Issue #44. The allow-list is treated as closed/positive: a member not explicitly listed fails even if it is read-only in practice (`R-TEST-04`), and each DES-0014 documented read-only member is asserted to pass individually so an accidentally incomplete allow-list turns the spec red.

## SafeArgs Scope

DES-0014 also states that `UT-0005`/adapter tests must prove emitted `RunDiagnostic.SafeArgs` values match the allow-listed safe shape (`R-SEC-02`). This evidence covers the invocation-audit oracle. The SafeArgs oracle remains an adapter diagnostic obligation because the portable audit result has no diagnostics surface yet. The adapter slice must either add SafeArgs checks beside this audit spec when diagnostics are emitted by the audit/acquisition seam, or explicitly link the equivalent `UT-0004`/adapter diagnostic test that rejects raw title, `Name`, path, and raw exception text.

## R-QA-01 Counter-Example

`MutatingFakeProviderTurnsSpyRed` runs a read-only fake provider (audit green) and a mutating fake provider that invokes `IUIAutomationInvokePattern.Invoke` mid-acquisition (audit red, violation listed). The pre-implementation RED run also confirmed that the spec fails when the audit contract is missing. A future mutation-test run can still weaken the allow-list check deliberately, but the current behavior suite already contains the `R-QA-01` counter-example.

## Implementation Closure (IMP-0007)

The spec fixes the minimal audit contract implemented by `IMP-0007`, in namespace `Surveyor.Adapters.Uia.Audit`:

- `ReadOnlyAcquisitionSpy` - records `RecordInvocation(string memberId)` and exposes `IReadOnlyList<string> InvokedMembers`.
- `ReadOnlyAcquisitionAudit` - `ReadOnlyAuditResult Evaluate(ReadOnlyAcquisitionSpy spy)` plus the static closed `IReadOnlyCollection<string> ReadOnlyAllowList`.
- `ReadOnlyAuditResult` - `bool IsReadOnly`, `IReadOnlyList<string> Violations`.

The audit policy is OS-independent pure logic, so `IMP-0007` implemented it in portable `src/Surveyor.Adapters.Uia.Audit` and referenced it from this portable test project, keeping `UT-0005` in the fast unit lane. The real raw-COM reader records into the spy in `IMP-0013`.

`tests/Surveyor.Adapters.Uia.Tests` is registered in both `Surveyor.slnx` and `eng/Surveyor.Unit.slnf` now that the RED spec is green.
