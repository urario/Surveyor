---
type: Detailed Design
title: DES-0013 Confidentiality, Storage, and Export Detailed Design
description: Detailed design for secure-by-default confidentiality policy, masking and redaction, opt-out recording, local result storage under LOCALAPPDATA with DPAPI CurrentUser and user ACLs, retention, sanitized paths, policy-gated masked exports, diagnostics and exception sanitization, and fallback-key exposure policy.
resource: ../../docs/gui-testability-analyzer-requirements.md
tags: [detailed-design, confidentiality, storage, export, sanitization, dpapi, rq-052, rq-053]
timestamp: 2026-07-03T00:00:00+09:00
---

# DES-0013 Confidentiality, Storage, and Export Detailed Design

This is detailed-design package 6 from [DES-0007](des-0007-detailed-design-execution-strategy.md) section 4. It fixes how Surveyor handles screenshots, extracted text, diagnostics, fallback keys, local persistence, retention, and shareable exports so implementation slice `IMP-0003`, store/export slice `IMP-0010`, unit-test designs `UT-0008`/`UT-0009`, and integration test `IT-0004` can proceed without inventing security policy.

Canonical requirements stay in [gui-testability-analyzer-requirements.md](../../docs/gui-testability-analyzer-requirements.md) (`RQ-xxx`) and derived requirements in [requirements-definition.md](../requirements/requirements-definition.md) (`RD-xxx`).

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `DES-0013`, Confidentiality, Storage, and Export Detailed Design, detailed design phase |
| Upstream | [DES-0002](des-0002-module-responsibility-basic-design.md) `M09`/`M12`; [DES-0003](des-0003-module-interface-basic-design.md) `IConfidentialityPolicy`; [DES-0004](des-0004-analysis-flow-basic-design.md) Stages 5/8; [DES-0005](des-0005-vmodel-traceability-and-downstream-tests.md) `UT-0008`/`UT-0009`/`IT-0004`; [DES-0007](des-0007-detailed-design-execution-strategy.md) package 6, `R-SEC-01`, `R-SEC-02`; [DES-0008](des-0008-project-structure-and-test-harness.md) project homes; [DES-0009](des-0009-domain-model-stable-keys-and-availability.md) fallback-key minimal contract and residual exposure risk; [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md) diagnostics shape |
| Requirements | `RQ-052`, `RQ-053`; derived `RD-012`, `RD-021`, `RD-022` |
| Downstream | Design review issue #35; `UT-0008` issue #47; `UT-0009` issue #48; `IT-0004` issue #56; `IMP-0003` issue #61; `IMP-0010` issue #68; `DES-0012` report schema; `DES-0016` UI consent/notice surfaces |
| Evidence | Confidentiality policy modes, masking rules, opt-out record, fallback-key exposure decision, storage layout, DPAPI CurrentUser + ACL rule, retention, atomic write/export flow, diagnostic and exception sanitizer, Mermaid data-flow, fixture strategy, test intents |
| Verification | `tools/okf/Validate-Okf.ps1`; `git diff --check`; future `dotnet test tests/Surveyor.Policy.Tests --filter UT0008` and `dotnet test tests/Surveyor.Adapters.Store.Tests --filter UT0009` once source exists |
| Residual Risk | DPAPI `CurrentUser` protects against casual cross-user access but not a fully compromised account or local administrator; export-local fallback pseudonyms reduce cross-export comparability for fallback-only elements; WGC capture-border user notice belongs to `DES-0016`; storage cleanup scheduling belongs to implementation/operations policy |

## Module Coverage

Primary modules:

- `M09 Confidentiality Policy` in `Surveyor.Policy`;
- `M12 Result Store / Export` in `Surveyor.Adapters.Store`.

Participating modules:

- `M03` invokes policy and store/export ports;
- `M10` report generation consumes already-sanitized report DTOs;
- `M11` provides `IClock`;
- `M04`/`M08` provide keys, labels, findings, and candidate codes that must be sanitized before logs/exports.

## Security Posture

Surveyor is secure by default:

1. screenshots and extracted UI text are confidential data;
2. logs and diagnostics use safe codes, keys, counts, statuses, and enum values only;
3. local persisted run data lives under `%LOCALAPPDATA%\Surveyor\Runs`;
4. confidential local blobs are encrypted with DPAPI `CurrentUser`;
5. run directories have a user-restricted ACL;
6. shareable export is masked by default and requires an explicit export command;
7. unmasked export is out of v1 scope;
8. any opt-out from masking/protected storage is explicit, scoped, timestamped, and recorded.

## Policy Contracts

Suggested source homes:

- `src/Surveyor.Policy/Confidentiality/ConfidentialityPolicy.cs`
- `src/Surveyor.Policy/Confidentiality/Sanitizer.cs`
- `src/Surveyor.Policy/Confidentiality/FallbackKeyExportMapper.cs`
- `src/Surveyor.Adapters.Store/LocalRunStore.cs`
- `src/Surveyor.Adapters.Store/ExportBundleWriter.cs`

### ConfidentialityDecision

Fields:

- `ConfidentialityMode Mode`: `ProtectedLocal`, `MaskedShareableExport`, `ExplicitLocalOptOut`
- `string PolicyVersion`: `confidentiality-v1`
- `DateTimeOffset DecidedAtUtc`
- `string DecisionSource`: `Default`, `UserConfirmed`, `TestFixture`
- `string? OptOutReasonCode`
- `IReadOnlyList<string> AppliedTransforms`

Default mode for a normal analysis run is `ProtectedLocal`. Default mode for export is `MaskedShareableExport`.

`ExplicitLocalOptOut` is allowed only for local developer/test scenarios that need plaintext artifacts; it must never be the default and must be visible in the run manifest.

### Sensitive Kinds

| Kind | Examples | Default treatment |
| -- | -- | -- |
| `DisplayText` | UI labels, values, status text, menu item text | Mask in reports/exports/logs. |
| `WindowTitle` | top-level title bar | Mask in reports/exports/logs. |
| `ScreenshotPixels` | captured full window/ROI images | Store encrypted locally; export only masked/redacted image. |
| `FilePath` | local run path, temp path | Never log raw absolute path; expose safe run id only. |
| `FallbackKeyToken` | hash-derived fallback identity from sensitive material | Protected locally; pseudonymized in shareable export. |
| `ExceptionMessage` | external exception message | Drop; map to exception kind/status/HResult only. |

## Masking And Redaction

Text masking uses per-run deterministic pseudonyms, not hashes of raw text. Hashes can be guessed for small UI strings.

For each run, `M09` builds a `MaskingDictionary`:

```text
DisplayText -> txt-0001, txt-0002, ...
WindowTitle -> win-0001, win-0002, ...
FallbackKeyToken -> fk-0001, fk-0002, ...
```

Pseudonym assignment order is deterministic by first safe traversal order from `ScreenModel` and then ordinal raw category order inside the policy implementation. The dictionary is stored only in the protected local store, encrypted. Shareable exports contain pseudonyms but not the reverse mapping.

Length buckets may be included when useful for human review:

- `0`
- `1-4`
- `5-12`
- `13-40`
- `41+`

Do not include raw prefixes, suffixes, or reversible hashes in shareable artifacts.

### Screenshot Redaction

Default export redacts text-bearing and result-bearing regions when their source element has `DisplayText`, `Value`, or unknown sensitivity. V1 supports rectangular redaction:

- fill color: neutral gray;
- optional pseudonym label, e.g. `txt-0007`;
- no raw text drawn into the image.

If bounds are unavailable, export includes the image only when policy can prove it does not contain sensitive text; otherwise the image is replaced with a placeholder metadata record.

## Fallback-Key Exposure Decision

`DES-0009` fixes the fallback-key minimal contract and leaves exposure policy to this package. The decision is:

- protected local run data may store canonical fallback key tokens because the local store is encrypted and ACL-restricted;
- shareable export must not expose canonical fallback key tokens;
- shareable export replaces fallback element keys with export-local pseudonyms (`fk-0001`, `fk-0002`, ...);
- report DTOs mark such ids with `IsFallback = true` and `StableAcrossExports = false`.

Non-fallback stable ids that are not derived from sensitive text may be exported as stable keys. This preserves version comparison where safe, while reducing guess-and-verify exposure for fallback-only elements.

## Local Store Design

Root:

```text
%LOCALAPPDATA%\Surveyor\Runs\<yyyyMMdd>\<run-id>\
```

Path rules:

- `<yyyyMMdd>` comes from `IClock.UtcNow`.
- `<run-id>` is a generated safe id from `M03`; tests may inject a fixed id.
- No path segment uses target title, label, process name, screen name, or raw key.
- File names are fixed: `manifest.json`, `result.protected`, `captures.protected`, `report.protected`, `diagnostics.json`.

Protected blobs:

- serialized with deterministic JSON or byte format decided by `DES-0012` / store implementation;
- encrypted with `ProtectedData.Protect(..., DataProtectionScope.CurrentUser)`;
- include policy/config versions in plaintext manifest and authenticated protected payload metadata.

ACL:

- directory owner: current user;
- full control: current user;
- inherited broad write entries are removed where possible;
- administrators/SYSTEM behavior follows Windows defaults but no additional broad group is granted.

Atomic write:

1. create temp directory under the same root;
2. write protected blobs and plaintext manifest;
3. fsync/flush where supported;
4. move/rename temp directory to final run directory;
5. on failure, mark partial and remove temp best-effort.

Expected store failures return `OperationStatus.IoError` or `Timeout`, not raw exceptions.

## Retention

Default retention for protected local runs is 30 days. The retention job:

- deletes only directories whose manifest identifies Surveyor and whose run age exceeds the configured retention;
- never follows reparse points;
- logs only run ids and counts;
- treats deletion errors as diagnostics with safe codes.

Exports are user-created files and are not automatically deleted by Surveyor.

## Export Bundle Design

Default shareable export is a ZIP bundle:

```text
surveyor-export-<run-id>.zip
  manifest.json
  result.masked.json
  diagnostics.masked.json
  captures/
    roi-0001.png
    roi-0002.png
  README.txt
```

Policy gate:

- export requires an explicit user command;
- export runs `MaskedShareableExport` mode even if the local store is protected;
- export manifest records `PolicyVersion`, `MaskingDictionaryVersion`, `ScoringConfigVersion`, and whether any fallback keys were pseudonymized;
- export never includes the reverse masking dictionary.

ZIP entry order is deterministic by path ordinal. Entry timestamps use a fixed normalized timestamp supplied by the export policy so byte-stable export tests are possible when inputs are fixed.

## Diagnostics And Exception Sanitization

`DES-0011` defines the diagnostic shape. This package defines the sanitizer:

Allowed values:

- diagnostic code;
- stage;
- status;
- severity;
- enum names;
- safe keys or export pseudonyms;
- counts;
- duration milliseconds;
- HRESULT integer;
- exception kind enum, e.g. `UnauthorizedAccess`, `ComError`, `WinRtArgument`, `IOException`.

Disallowed values:

- exception message;
- stack trace in user export;
- absolute file path;
- window title;
- display label;
- raw text/value;
- canonical fallback token in export.

Developer-local debug logs may include stack traces only when explicitly enabled in a local developer configuration and still must pass the raw target text/path sanitizer. Debug mode is out of shareable export.

## Mermaid Data Flow

```mermaid
flowchart TD
  A["AnalysisRunResult"] --> B["ConfidentialityPolicy"]
  B --> C["Protected local model"]
  B --> D["Masked export model"]
  C --> E["DPAPI CurrentUser encryption"]
  E --> F["LOCALAPPDATA run store with user ACL"]
  D --> G["Pseudonymized JSON"]
  D --> H["Redacted captures"]
  G --> I["Policy-gated ZIP export"]
  H --> I
  B --> J["Sanitized diagnostics"]
```

## Edge Cases

| Case | Required behavior |
| -- | -- |
| DPAPI encryption fails | Store returns `IoError`, no plaintext fallback. |
| ACL tightening fails | Store returns `IoError` unless running in a test fixture that explicitly disables ACL checks. |
| Export requested before local protected store completed | Export operates from in-memory sanitized model or fails safely; it does not read partial temp files. |
| Fallback-only element appears in report | Local protected report may contain canonical key; shareable export uses `fk-xxxx` and marks not stable across exports. |
| Exception message contains UI text | Sanitizer drops message; diagnostic keeps exception kind/HResult/status. |
| ROI bounds unavailable for text-bearing element | Export omits or placeholders the screenshot rather than leaking full image. |
| Retention encounters reparse point | Skip and emit safe diagnostic. |

## Unit And Integration Test Handoff

`UT-0008` confidentiality policy tests:

| Test intent | Fixture |
| -- | -- |
| Text masking | raw labels/values replaced by deterministic pseudonyms; no raw text in export model. |
| Fallback key export | canonical fallback key in protected local model, pseudonym in export model. |
| Diagnostic sanitizer | raw exception message/path/title removed; safe code/status/HResult remain. |
| Screenshot redaction decision | text-bearing ROI redacted; unknown bounds replaced by placeholder. |
| Opt-out record | explicit local opt-out requires reason and timestamp; default never opts out. |

`UT-0009` store/export tests:

| Test intent | Fixture |
| -- | -- |
| Store path hygiene | generated paths contain run id/date only, no label/title/raw key. |
| DPAPI wrapper invoked | fake protection service records `CurrentUser`; plaintext blob is not written. |
| ACL service invoked | fake ACL service receives final run directory. |
| Atomic write | temp directory is used; failure leaves no final partial directory. |
| Export determinism | fixed inputs yield stable ZIP entry order and normalized timestamps. |
| Retention safety | old Surveyor run removed; non-Surveyor/reparse entries skipped. |

`IT-0004` end-to-end confidential run:

- analyze a fixture target with sensitive labels and screenshots;
- persist local run;
- export masked bundle;
- assert no known sensitive strings appear in logs, manifest, JSON, ZIP entries, image OCR-safe labels, or paths.

## Implementation Handoff

Implementers should start with:

1. `IConfidentialityPolicy` and immutable policy DTOs;
2. sanitizer allowlist tests;
3. fallback-key export pseudonym mapper;
4. store protection seam (`IDataProtector` wrapping DPAPI);
5. ACL seam;
6. atomic local run writer;
7. masked export writer.

The first implementation slice should make the "safe by default" tests pass before adding optional export refinements.
