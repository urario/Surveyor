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
| Downstream | `tests/Surveyor.Domain.Tests/StableIdentityKeyBehaviorTests.cs`; `tests/Surveyor.Domain.Tests/DomainModelFixture.cs`; `tests/Surveyor.Policy.Tests/FallbackKeyDerivationContractTests.cs`; fixtures under `tests/fixtures/uia-trees/` |
| Evidence | Added behavior tests for `DisplayLabel` volatility, fresh-process stable-key equality, screen-state key separation, explicit `Unavailable(PermissionDenied)`, and fallback token non-reversibility/cross-process equality. Test names use behavior/risk wording via xUnit display names. |
| Verification | RED first: `dotnet test tests\Surveyor.Domain.Tests` failed with missing `Surveyor.Domain.Keys` / `Surveyor.Domain.Model` and missing `ScreenModel`, `ScreenKey`, `ElementKey`, `Availability` types. Policy RED first: `dotnet test tests\Surveyor.Policy.Tests` failed with missing `Surveyor.Application.Ports` / `Surveyor.Domain.Model` / `Surveyor.Policy` types. GREEN after `IMP-0001`: `dotnet test tests\Surveyor.Domain.Tests --no-restore` passed 4 tests; `dotnet test tests\Surveyor.Policy.Tests --no-restore` passed 1 test; `dotnet test tests\Surveyor.Architecture.Tests --no-restore` passed 8 tests; `dotnet build Surveyor.Unit.slnf --no-restore -v minimal` passed with 0 warnings/errors; `dotnet format Surveyor.Unit.slnf --verify-no-changes --no-restore` passed; `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\okf\Validate-Okf.ps1` passed for 39 markdown files. |
| Residual Risk | Culture-change verification is structurally covered by `<InvariantGlobalization>true`; non-invariant culture creation is not available in this unit lane. Collision suffix behavior is not asserted in this slice because the requested UT-0001 green path does not include duplicate sibling identities. |

## Behavior Tests

- `表示名変更で ElementKey と ScreenKey が変わらない (RQ-051/RQ-053)`: compares `volatile-label-before.tree` and `volatile-label-after.tree`, which differ only in labels.
- `同一安定入力のキーは fresh process でも同値になる (R-NET-01)`: launches a fresh `dotnet test --no-build` process and compares the recomputed key payload.
- `スクリーン状態が異なると ScreenKey が変わる (RD-002/RQ-053)`: compares `state-switch-a.tree` / `state-switch-b.tree`.
- `Unavailable は reason を保持し有効な ElementKey を持つ (RD-020)`: asserts `PermissionDenied` is preserved and the element still has an `elm:1:` key.
- `fallback-key は非可逆で fresh process でも同値になる (RQ-051/RQ-052)`: verifies v=1 whitespace collapse, no sentinel substrings in the token, and fresh-process equality.

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

The temporary fixture change was reverted before green verification.
