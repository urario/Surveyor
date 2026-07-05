---
type: Trace Evidence
title: UT-0005 Read-Only Acquisition Audit Behavior Evidence
description: Failing-first behavior-test evidence for the read-only acquisition audit spy that fails when any prohibited state-changing UIA pattern is invoked during acquisition, covering the full DES-0014 prohibited-pattern list, the closed positive allow-list, the WM_GETTEXT exception boundary, SafeArgs handoff scope, and the R-QA-01 mutating-fake-provider counter-example. Tests only; the IMP-0007 audit implementation is handed off to another owner.
tags: [trace, unit-test, ut-0005, des-0014, rq-048, rd-032, read-only]
timestamp: 2026-07-05T00:00:00+09:00
---

# UT-0005 Read-Only Acquisition Audit Behavior Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `UT-0005`, read-only acquisition audit spy behavior, unit-test phase |
| Upstream | [DES-0014](../design/des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) Read-Only Audit / Concrete read-only allow-list / Unit-Test Intent; [DES-0007](../design/des-0007-detailed-design-execution-strategy.md) `UT-0005` intent and guardrail failing-first matrix; Issue #44; `RQ-048`; `RD-032`; guardrails `R-QA-01`, `R-TEST-04`, `R-SEC-02` |
| Downstream | `tests/Surveyor.Adapters.Uia.Tests/ReadOnlyAuditBehaviorTests.cs`; `Surveyor.slnx`; drives `IMP-0007` (#65, handed off); complemented by `UT-0004`/adapter SafeArgs diagnostics, `IT-0001` (real target state-invariance), and `IMP-0013` (real COM spy wiring) |
| Evidence | Authored the `UT-0005` behavior spec: an allow-listed read-only acquisition passes; each prohibited state-changing pattern from the DES-0014 map fails the audit (theory over the full list); every documented DES-0014 read-only member is asserted as accepted by the positive allow-list; a member outside the closed positive allow-list fails even if not obviously mutating; `WM_GETTEXT` via `SendMessageTimeout` is allowed while `WM_SETTEXT` and bare `SendMessage` fail; violations enumerate every offending member; an empty recording is not falsely claimed read-only; and a mutating fake acquisition provider turns the spy red (`R-QA-01`). |
| Verification | RED (failing-first) confirmed before review response: `dotnet build tests/Surveyor.Adapters.Uia.Tests` fails only with `CS0234`/`CS0246` for the not-yet-implemented `Surveyor.Adapters.Uia.Audit` types (`ReadOnlyAcquisitionSpy`, `ReadOnlyAcquisitionAudit`, `ReadOnlyAuditResult`); no other/analyzer errors. GREEN is the `IMP-0007` implementer's step once the audit types exist. |
| Residual Risk | This slice authors tests only; `IMP-0007` (audit implementation) is handed off to another owner and must (a) turn this spec green, (b) add `tests/Surveyor.Adapters.Uia.Tests` to `eng/Surveyor.Unit.slnf` once green so the deterministic fast lane sees it permanently, (c) confirm the `R-QA-01` counter-example red by temporarily weakening the audit, and (d) record its own trace. The project is registered in `Surveyor.slnx` now for workspace visibility, but is intentionally not in `eng/Surveyor.Unit.slnf` while it is a failing-first RED spec. The spy audits invocations it can observe; real-target state-invariance is `IT-0001`, and wiring the recording spy to the real raw-COM reader is `IMP-0013`. The audit member vocabulary uses `Interface.Member` string identifiers matching the DES-0014 tables; if `IMP-0007` adopts a strongly-typed member enum, the fixture strings map 1:1. `IUIAutomationTextEditPattern.SetValue` is a namespace-level prohibited-member sentinel for the DES-0014 "Text edit" row; `IMP-0007` must reconcile the final string/enum token with the actual raw-COM reader vocabulary without weakening the prohibited operation. |

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

DES-0014 also states that `UT-0005`/adapter tests must prove emitted `RunDiagnostic.SafeArgs` values match the allow-listed safe shape (`R-SEC-02`). This tests-only RED slice covers the invocation-audit oracle and records the SafeArgs oracle as an `IMP-0007`/adapter completion obligation because the audit result and acquisition diagnostics are not implemented yet. `IMP-0007` must either add SafeArgs checks beside this audit spec when diagnostics are emitted by the audit/acquisition seam, or explicitly link the equivalent `UT-0004`/adapter diagnostic test that rejects raw title, `Name`, path, and raw exception text.

## R-QA-01 Counter-Example

`MutatingFakeProviderTurnsSpyRed` runs a read-only fake provider (audit green) and a mutating fake provider that invokes `IUIAutomationInvokePattern.Invoke` mid-acquisition (audit red, violation listed). Because `IMP-0007` is handed off, the additional "inject a deliberately weakened audit and observe red" confirmation is a named `IMP-0007` completion step, recorded here as a hand-off obligation rather than demonstrated in this tests-only slice.

## Implementation Hand-Off (IMP-0007)

The failing spec fixes the minimal audit contract for the implementer, in namespace `Surveyor.Adapters.Uia.Audit`:

- `ReadOnlyAcquisitionSpy` - records `RecordInvocation(string memberId)` and exposes `IReadOnlyList<string> InvokedMembers`.
- `ReadOnlyAcquisitionAudit` - `ReadOnlyAuditResult Evaluate(ReadOnlyAcquisitionSpy spy)` plus the static closed `IReadOnlyCollection<string> ReadOnlyAllowList`.
- `ReadOnlyAuditResult` - `bool IsReadOnly`, `IReadOnlyList<string> Violations`.

The audit policy is OS-independent pure logic, so it is recommended to implement it in a portable (`net10.0`) unit and reference it from this portable test project, keeping `UT-0005` in the fast unit lane; the real raw-COM reader records into the spy in `IMP-0013`. The final project home is the architect/implementer's call and is flagged here for confirmation.

Before handing off `IMP-0007`, keep `tests/Surveyor.Adapters.Uia.Tests` registered in `Surveyor.slnx` and, after the RED spec turns green, add the same project to `eng/Surveyor.Unit.slnf`. Until then it remains outside `eng/Surveyor.Unit.slnf` by design so the existing green unit lane is not made permanently red by a hand-off PR.
