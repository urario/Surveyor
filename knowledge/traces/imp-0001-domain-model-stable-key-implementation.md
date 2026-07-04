---
type: Trace Evidence
title: IMP-0001 Domain Model Stable Key Implementation
description: Implementation evidence for the minimal ScreenModel, UiElement, ElementIdentity, ScreenKey, ElementKey, and fallback-key contract slice.
tags: [trace, implementation, imp-0001, des-0009, rq-051, rq-052, rq-053, rq-054]
timestamp: 2026-07-04T00:00:00+09:00
---

# IMP-0001 Domain Model Stable Key Implementation

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `IMP-0001`, minimal domain model and stable-key implementation |
| Upstream | `UT-0001`; `DES-0009`; `DES-0008`; `RQ-051`, `RQ-052`, `RQ-053`, `RQ-054`; Issue #19 |
| Downstream | `src/Surveyor.Domain/Keys/*`; `src/Surveyor.Domain/Model/*`; `src/Surveyor.Application/Ports/IFallbackKeyDerivation.cs`; `src/Surveyor.Policy/Sha256FallbackKeyDerivation.cs`; `Surveyor.slnx`; `Surveyor.Unit.slnf`; `PublicAPI.Unshipped.txt` files |
| Evidence | Added internal-default `ScreenModel`, `UiElement`, `ElementIdentity`, `ScreenIdentity`, `ScreenStateDiscriminator`, `Availability`, `BoundingRect`, `SupportedPatterns`, `ScreenKey`, and `ElementKey`. Added shared internal `KeyDigest` validation to keep `ScreenKey` and `ElementKey` decoupled. Added public `IdentityMaterial` only as the assembly-boundary fallback token carrier. Added `IFallbackKeyDerivation` as the application-owned port and `Sha256FallbackKeyDerivation` as the minimal M09 policy implementation. |
| Verification | Targeted green: `dotnet test tests\Surveyor.Domain.Tests --no-restore` (4 tests); `dotnet test tests\Surveyor.Policy.Tests --no-restore` (2 tests); `dotnet test tests\Surveyor.Architecture.Tests --no-restore` (8 tests). Quality gates: `dotnet build Surveyor.Unit.slnf --no-restore -v minimal` passed with 0 warnings/errors; `dotnet format Surveyor.Unit.slnf --verify-no-changes --no-restore` passed; `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\okf\Validate-Okf.ps1` passed for 39 markdown files. |
| Residual Risk | The Domain model/key types are intentionally `internal` in this first slice to honor CS-02 until a later adapter/application boundary needs promotion. Full confidentiality policy, export/store masking, scoring, serialization, UIA acquisition, and DI wiring remain out of scope per `DES-0009`. |

## Implementation Notes

- Key material uses LF-separated v=1 grammar, UTF-8, SHA-256, first 16 bytes as 32 lowercase hex characters, and `scr:1:` / `elm:1:` canonical strings.
- `KeyDigest` owns canonical digest validation so element keys do not construct throwaway screen keys.
- `DisplayLabel` is not accepted by key derivation APIs and is stored only on `ScreenModel` / `UiElement`.
- fallback raw text is accepted only by `Surveyor.Policy.Sha256FallbackKeyDerivation`; it returns `IdentityMaterial.FallbackKeyToken`, and the Domain never hashes raw target text.
- v=1 fallback normalization trims leading/trailing whitespace and collapses internal `char.IsWhiteSpace` runs to one ASCII space. It does no case folding and no Unicode normalization.
- `Unavailable(reason)` is a distinct value and does not remove the element key.
- No suppressions were added.

## Pattern Record

Port/Adapter: `IFallbackKeyDerivation` keeps raw sensitive fallback text outside the Domain while allowing `Surveyor.Policy` to vary the hashing policy; direct Domain hashing was rejected because it would mix M04 identity modeling with M09 confidentiality policy.
