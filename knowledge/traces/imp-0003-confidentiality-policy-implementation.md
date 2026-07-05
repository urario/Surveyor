---
type: Trace Evidence
title: IMP-0003 Confidentiality Policy Implementation
description: Implementation evidence for the M09 secure-by-default confidentiality policy, allowlist sensitive-value sanitizer, and fallback-key export pseudonym mapper that turn UT-0008 green with deterministic behavior, zero analyzer or PublicAPI warnings, and no raw sensitive text flowing into keys, paths, ids, logs, diagnostics, or exceptions.
tags: [trace, implementation, imp-0003, des-0013, rq-052, rd-022, confidentiality]
timestamp: 2026-07-04T00:00:00+09:00
---

# IMP-0003 Confidentiality Policy Implementation

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `IMP-0003`, confidentiality policy (`M09`) minimal implementation, implementation phase |
| Upstream | [DES-0013](../design/des-0013-confidentiality-storage-and-export.md) §Public API Definitions / §Masking And Redaction / §Fallback-Key Exposure Decision / §Diagnostics And Exception Sanitization; [DES-0009](../design/des-0009-domain-model-stable-keys-and-availability.md) fallback-key minimal contract; [UT-0008](ut-0008-confidentiality-policy-evidence.md) (#47); Issue #61; `RQ-052`; `RD-022` |
| Downstream | `src/Surveyor.Application/Ports/ConfidentialityContracts.cs`; `src/Surveyor.Policy/Confidentiality/*`; connects to `IMP-0010` store/export and `IT-0004` end-to-end confidential run |
| Evidence | Implemented the application-owned `IConfidentialityPolicy` decision port plus policy-side `ISensitiveValueSanitizer` and `IFallbackKeyExportMapper`. Secure-by-default `ProtectedLocal`, explicit recorded opt-out, mode-independent export/diagnostic masking, deterministic non-reversible text pseudonyms with length buckets, allowlist exception sanitization (kind + HResult only), and export-local fallback pseudonyms. |
| Verification | `dotnet build eng/Surveyor.Unit.slnf` 0 warnings / 0 errors (CS-01/05/06/08); `dotnet test eng/Surveyor.Unit.slnf` Architecture 8, Domain 26, Policy 18 passed; `Surveyor.Policy` line coverage 94.44% (CS-07); `dotnet format --verify-no-changes` exit 0 (CS-09); `tools/okf/Validate-Okf.ps1` passed. |
| Residual Risk | The DES-0013 `IConfidentialityPolicy.Apply` and `CreateShareableExportModel` methods depend on DES-0011 result DTOs (`AnalysisRunResult`, `StoredRunSnapshot`) that do not yet exist and are `IMP-0010` store/export scope; this slice implements the decision, sanitization, and fallback-export-mapping behaviors only. `RunId` / `ExportId` are carried as plain identifiers pending their DES-0011 strong types. At-rest DPAPI/ACL protection and ZIP export remain `IMP-0010` / `UT-0009`. **IMP-0010 hand-off checklist** (from PR #93 multi-role review): (1) `SensitiveValueSanitizer` holds per-run masking-dictionary state, so it must be DI-registered `Transient`/per-run `Scoped` — a lifetime test must prevent an accidental singleton that would silently break determinism (`RQ-051`) and cross-run non-correlation (`RQ-052`); (2) `MaskText` currently masks only `DisplayText` / `WindowTitle` — `FilePath` / `ScreenshotPixels` masking is added with the store/export slice, and `FallbackKeyToken` / `ExceptionMessage` are handled by the mapper / exception sanitizer respectively. |

## Scope And Layering

The slice keeps DES-0008 inward dependencies intact:

- `Surveyor.Application/Ports/ConfidentialityContracts.cs` owns the port (`IConfidentialityPolicy`) and its request/decision DTOs (`ConfidentialityMode`, `ConfidentialityTarget`, `ConfidentialityRequest`, `OptOutRequest`, `ConfidentialityDecision`).
- `Surveyor.Policy/Confidentiality/` implements `M09`: `ConfidentialityPolicy`, `SensitiveValueSanitizer`, `FallbackKeyExportMapper`, and their sanitization DTOs.

No new `ProjectReference` edges were added; `Surveyor.Architecture.Tests` stays green.

## Secure-By-Default Behavior

`ConfidentialityPolicy.Decide` is deterministic and uses only the request (no clock read), stamping `RequestedAtUtc` into `DecidedAtUtc` (`RQ-051`). Default requests resolve to `ProtectedLocal`; `ExplicitLocalOptOut` is accepted only with a non-empty reason code, a non-`Default` decision source, and a recorded `OptOutRequest`, and it is the only mode carrying an `OptOutReasonCode`. `RequiresTextMasking` always returns `true` for `ShareableExport` and `Diagnostics`, so an opt-out cannot weaken egress (`R-SEC-01`).

## Sanitization And Fallback Export

`SensitiveValueSanitizer` assigns per-run first-seen ordinal pseudonyms (`txt-000N`, `win-000N`) with coarse length buckets and never returns raw text; `SanitizeException` maps CLR exception types to an allowlist `ExceptionKind` and keeps only the HResult, dropping message, stack, and path. `FallbackKeyExportMapper` replaces fallback element keys with `exp-<export-id-short>-fk-000N` (`StableAcrossExports = false`) and leaves non-fallback stable keys intact for safe version comparison, matching the DES-0013 fallback-key exposure decision.

## Portability Note

`tests/Surveyor.Architecture.Tests` parsed `ProjectReference` includes with Windows `\` separators only; a one-line normalization to `Path.DirectorySeparatorChar` lets the DES-0008 dependency-graph guardrail run green on non-Windows lanes without changing its semantics. A `tests/**` scoped `CA1515` exclusion was added because xUnit requires public test classes, keeping the `dotnet format` gate green across SDK feature bands while production public-surface discipline is untouched.
