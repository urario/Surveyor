---
type: Trace Evidence
title: UT-0008 Confidentiality Policy Behavior Evidence
description: Behavior-test evidence for secure-by-default confidentiality decisions, explicit opt-out recording across both policy branches, deterministic non-reversible text masking, allowlist-based diagnostic and exception sanitization, fallback-key export pseudonymization, and R-QA-01 counter-example red evidence under DES-0013.
tags: [trace, unit-test, ut-0008, des-0013, rq-052, rd-022, confidentiality]
timestamp: 2026-07-04T00:00:00+09:00
---

# UT-0008 Confidentiality Policy Behavior Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `UT-0008`, confidentiality policy secure-by-default and sanitization behavior, unit-test phase |
| Upstream | [DES-0013](../design/des-0013-confidentiality-storage-and-export.md) §Policy Contracts / §Diagnostics And Exception Sanitization / §Unit And Integration Test Handoff; [DES-0009](../design/des-0009-domain-model-stable-keys-and-availability.md) fallback-key minimal contract; Issue #47; `RQ-052`; `RD-022`; guardrails `R-SEC-01`, `R-QA-01` |
| Downstream | `tests/Surveyor.Policy.Tests/ConfidentialityPolicyBehaviorTests.cs`; drives `IMP-0003` (#61); connects to `IMP-0010` store/export and `IT-0004` end-to-end egress proof |
| Evidence | Added behavior tests covering default `ProtectedLocal`, explicit opt-out recording with reason and UTC timestamp, both mask-all / allow-all branches, export/diagnostic egress that always masks even under opt-out, deterministic non-reversible text pseudonyms with length buckets, allowlist exception sanitization keeping only kind and HResult, and fallback-key export-local pseudonymization scoped by `ExportId`. |
| Verification | RED first: `dotnet build tests/Surveyor.Policy.Tests` failed with missing `Surveyor.Policy.Confidentiality` / `Surveyor.Application.Ports` confidentiality types. Counter-example RED (`R-QA-01`): mutating `Decide` to default `ExplicitLocalOptOut` and leaking `ShareableExport` masking under opt-out failed 3 decision tests. GREEN after `IMP-0003`: `dotnet test tests/Surveyor.Policy.Tests` passed 18 tests with `Surveyor.Policy` line coverage 94.44% (CS-07 ≥ 80%). |
| Residual Risk | `UT-0008` proves the M09 policy unit in isolation; the full-run egress proof (logs/manifest/JSON/ZIP/redacted-image labels) is deferred to `IT-0004`. At-rest store protection and export ZIP determinism are `UT-0009` / `IMP-0010` scope. Screenshot redaction and the DES-0011-coupled `Apply` / `CreateShareableExportModel` surfaces are out of this slice. |

## Behavior Tests

`ConfidentialityPolicyDecisionTests`:

- `DefaultDecisionIsProtectedLocalAndNeverOptsOut`
- `ExplicitOptOutIsRecordedWithReasonAndTimestamp`
- `OptOutWithoutReasonOrRecordIsRejected`
- `OptOutCannotBeDefaultSourced`
- `ProtectedModeWithOptOutRecordIsRejected`
- `ExportAndDiagnosticsAlwaysMaskEvenUnderOptOut`
- `MaskedShareableExportModeMasks`

`SensitiveValueSanitizerTests`:

- `TextMaskingIsDeterministicAndNonReversible`
- `UnsupportedKindIsRejected`
- `LengthBucketBoundariesAreStable`
- `ExceptionSanitizationKeepsKindAndDropsMessage`
- `UnknownExceptionMapsToUnknownKind`

`FallbackKeyExportMapperTests`:

- `FallbackExportKeyIsPseudonymizedAndNonReversible`
- `NonFallbackExportKeyStaysStable`
- `FallbackExportPseudonymIsScopedByExportId`
- `InvalidContextIsRejected`

## Secure-By-Default And Both-Branch Evidence

The decision tests exercise both policy branches so the easy one-sided test is avoided (Issue #47 §範囲). The mask-all branch (`ProtectedLocal`, source `Default`) requires local masking; the allow-all branch (`ExplicitLocalOptOut`, source `UserConfirmed`, recorded reason) permits plaintext local artifacts. Independent of branch, `RequiresTextMasking` returns `true` for `ShareableExport` and `Diagnostics`, proving egress is never weakened by a local opt-out (`R-SEC-01`).

## Counter-Example Red Evidence (`R-QA-01`)

Two representative wrong implementations were injected and observed to fail before reverting:

- default `Decide` returning `ExplicitLocalOptOut` (allow-all default) → `DefaultDecisionIsProtectedLocalAndNeverOptsOut` red.
- `RequiresTextMasking(ShareableExport)` following the local opt-out rule → `ExportAndDiagnosticsAlwaysMaskEvenUnderOptOut` red.

The exception-sanitization test additionally fixes representative `UnauthorizedAccessException`, `IOException`, and `COMException` (obtained via `Marshal.GetExceptionForHR`) instances whose messages embed a `SENTINEL` path; the assertions fail for any implementation that echoes the message, path, or raw text instead of keeping only kind and HResult.

## Determinism Evidence

Text masking assigns first-seen ordinal pseudonyms per kind (`txt-0001`, `win-0001`) and returns the same pseudonym for a repeated raw value, so masking is deterministic for a fixed traversal (`RQ-051`). Fallback export keys are `exp-<export-id-short>-fk-000N`; the same `ExportId` and ordinal are reproducible while a different `ExportId` changes the export key, matching the DES-0013 export-local pseudonym rule.
