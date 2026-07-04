---
type: Trace Evidence
title: UT-0001 Stable Key Behavior Evidence
description: Behavior-test evidence that DisplayLabel changes do not change stable ScreenKey/ElementKey values, with fresh-process determinism, screen-state identity, fallback token contract, and counter-example red evidence.
tags: [trace, unit-test, ut-0001, des-0009, rq-051, rq-052, rq-053]
timestamp: 2026-07-04T00:00:00+09:00
---

# UT-0001 Stable Key Behavior Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `UT-0001`, stable identity, key/label separation, availability, and screen-state behavior |
| Upstream | `RQ-051`, `RQ-052`, `RQ-053`; `RD-004`, `RD-020`, `RD-021`, `RD-022`; `DES-0007`; `DES-0008`; `DES-0009`; Issue #18 |
| Downstream | `tests/Surveyor.Domain.Tests/StableIdentityKeyBehaviorTests.cs`; `tests/Surveyor.Domain.Tests/DomainValueObjectContractTests.cs`; `tests/Surveyor.Domain.Tests/DomainModelFixture.cs`; `tests/Surveyor.Policy.Tests/FallbackKeyDerivationContractTests.cs`; fixtures under `tests/fixtures/uia-trees/` |
| Evidence | Added behavior tests for `DisplayLabel` volatility, fresh-process stable-key equality, screen-state key separation, explicit `Unavailable(PermissionDenied)`, fallback token non-reversibility/cross-process equality, separate fallback whitespace/scope boundary checks, and value-object guard/canonical/equality contracts. Test names use behavior/risk wording via xUnit display names. |
| Verification | RED first: `dotnet test tests\Surveyor.Domain.Tests` failed with missing `Surveyor.Domain.Keys` / `Surveyor.Domain.Model` and missing `ScreenModel`, `ScreenKey`, `ElementKey`, `Availability` types. Policy RED first: `dotnet test tests\Surveyor.Policy.Tests` failed with missing `Surveyor.Application.Ports` / `Surveyor.Domain.Model` / `Surveyor.Policy` types. GREEN after `IMP-0001`, PR #88 review response, rebase onto `origin/main`, and quantitative-gate refresh: `dotnet test Surveyor.Unit.slnf --no-restore --collect "XPlat Code Coverage" --results-directory .\TestResults\coverage-pr88-rebased-final` passed 19 tests; union line coverage was Domain 90.08%, Policy 94.34%, core total 90.61%. `dotnet build Surveyor.Unit.slnf --no-restore -v minimal` passed with 0 warnings/errors, including CS-06 CA1501/CA1502/CA1505/CA1506 metric violations 0. `dotnet format Surveyor.Unit.slnf --verify-no-changes --no-restore` passed. `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\okf\Validate-Okf.ps1` passed for 39 markdown files. PR #88 review response RED checks: temporarily removing `state=` from `KeyMaterial.ForScreen` failed `ScreenKeyChangesWhenStateIdentityChanges`; temporarily disabling fallback whitespace detection failed `FallbackKeyCollapsesWhitespaceAndKeepsScopeBoundary`. |
| Residual Risk | Culture-change verification is structurally covered by `<InvariantGlobalization>true`; non-invariant culture creation is not available in this unit lane. Collision suffix behavior is not asserted in this slice because the requested UT-0001 green path does not include duplicate sibling identities. `IdentityMaterial.StableIdentity` still trusts upstream non-sensitive classification; this is a named handoff risk for `DES-0014` acquisition adapters. Nested fresh-process tests still depend on `dotnet test --no-build` availability and are candidates for a later dedicated IT/harness lane. |

## Behavior Tests

- `KeysDoNotChangeWhenDisplayLabelsChange`: compares `volatile-label-before.tree` and `volatile-label-after.tree`, which differ only in labels.
- `StableInputKeysAreEqualAcrossFreshProcess`: launches a fresh `dotnet test --no-build` process and compares the recomputed key payload.
- `ScreenKeyChangesWhenStateIdentityChanges`: compares `state-switch-a.tree` and `state-switch-b.tree`.
- `UnavailableReasonIsPreservedWithElementKey`: asserts `PermissionDenied` is preserved and the element still has an `elm:1:` key.
- `FallbackKeyIsNonReversibleAndStableAcrossFreshProcess`: verifies no sentinel substrings in the token and fresh-process equality.
- `FallbackKeyCollapsesWhitespaceAndKeepsScopeBoundary`: verifies v1 whitespace collapse and scope boundary separation.
- `DomainValueObjectContractTests`: verifies key canonical strings, digest guards, value-object guards/equality, stable child copying, and depth-first stable element order.

## Quantitative Quality Gates

| Gate | Result |
| -- | -- |
| Build / analyzers (`CS-01`, `CS-05`, `CS-06`, `CS-08`) | `dotnet build Surveyor.Unit.slnf --no-restore -v minimal` passed with 0 warnings and 0 errors. CS-06 code-metrics diagnostics were 0 for `CA1501` inheritance depth > 5, `CA1502` cyclomatic complexity > 10, `CA1505` maintainability index < 20, and `CA1506` class coupling > 30. |
| Unit tests + coverage (`CS-07`) | `dotnet test Surveyor.Unit.slnf --no-restore --collect "XPlat Code Coverage" --results-directory .\TestResults\coverage-pr88-rebased-final` passed 19 tests. Cobertura union line coverage: `Surveyor.Domain` 336/373 = 90.08%, `Surveyor.Policy` 50/53 = 94.34%, core total 386/426 = 90.61%. Branch coverage is recorded as non-gating evidence: Domain 87/120 = 72.50%, Policy 17/22 = 77.27%, core total 104/142 = 73.24%. |
| Architecture tests | `Surveyor.Architecture.Tests` passed 8 tests in the unit solution run. |
| Format (`CS-09`) | `dotnet format Surveyor.Unit.slnf --verify-no-changes --no-restore` passed. |

## Counter-Example RED

Temporary counter-example fixture change:

```diff
-    "material": "RootWindow",
+    "material": "Orders - draft after",
```

Command:

```powershell
dotnet test tests\Surveyor.Domain.Tests --no-restore --filter FullyQualifiedName~StableIdentityKeyBehaviorTests.KeysDoNotChangeWhenDisplayLabelsChange
```

Observed RED:

```text
Assert.Equal() Failure: Strings differ
Expected: "elm:1:dd1a3bd2e3595c868223cfc402717050"
Actual:   "elm:1:dede0e2cf2f944923f1d96e89b48d700"
```

Temporary state-material omission:

```diff
-        if (state is not null)
+        if (false && state is not null)
```

Command:

```powershell
dotnet test tests\Surveyor.Domain.Tests --no-restore --filter FullyQualifiedName~StableIdentityKeyBehaviorTests.ScreenKeyChangesWhenStateIdentityChanges
```

Observed RED:

```text
Assert.NotEqual() Failure: Strings are equal
Expected: Not "scr:1:91d33544f334ef5adc70b921aa617743"
Actual:       "scr:1:91d33544f334ef5adc70b921aa617743"
```

Temporary fallback whitespace detection removal:

```diff
-            if (char.IsWhiteSpace(character))
+            if (character == '\0')
```

Command:

```powershell
dotnet test tests\Surveyor.Policy.Tests --no-restore --filter FullyQualifiedName~FallbackKeyDerivationContractTests.FallbackKeyCollapsesWhitespaceAndKeepsScopeBoundary
```

Observed RED:

```text
Assert.Equal() Failure: Strings differ
Expected: "085772a96858d7a4c7652d4c29596b12"
Actual:   "c071dba904b7d0409e554886304858d7"
```

All temporary fixture and source changes were reverted before final green verification.
