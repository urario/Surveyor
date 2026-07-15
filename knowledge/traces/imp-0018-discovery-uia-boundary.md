---
type: Trace Evidence
title: IMP-0018 Discovery-Owned UIA Target Boundary
description: Implementation evidence for moving opaque target token minting and raw HWND ownership from UIA to Discovery while retaining UIA as the sole production reader.
tags: [trace, implementation, imp-0018, des-0014, des-0018, rq-048, rq-052, rq-054, layering, discovery, uia]
timestamp: 2026-07-16T00:00:00+09:00
---

# IMP-0018 Discovery-Owned UIA Target Boundary

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `IMP-0018`, Discovery-owned target handle bridge, implementation phase |
| Upstream | [DES-0014](../design/des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md); [DES-0018](../design/des-0018-composition-root-and-di.md); [IMP-0013](imp-0013-uia-acquisition-adapter.md); Issue #113; `RQ-048`, `RQ-052`, `RQ-054`; `RD-025`, `RD-032` |
| Downstream | `UT-0013` #52 and this artifact are prerequisites to `IMP-0015` #73 production composition; live target state-invariance, permission, and timeout behavior remain `IT-0001`, `IT-0005`, and `IT-0006` |
| Evidence | Added public methodless `DiscoveryUiaBridge` with one internal `WindowTargetHandleRegistry`; internal writer/resolver/result/raw types; Discovery-owned `tgt-<counter>` token minting; UIA-only production friend and UIA→Discovery project edge; migrated `UiaTreeAcquisitionAdapter`; removed public `UiaTargetHandleRegistry`; closed public API baselines; added per-consumer, friend, edge, public-result, raw-member, sink, and raw-HWND diagnostic counter-examples. |
| Verification | Failing-first architecture run reproduced four boundary violations before production changes. `dotnet build Surveyor.slnx -c Release --no-restore` passed with 0 warnings / 0 errors. UIA adapter tests passed 61; Architecture tests passed 25; full headless unit lane passed with core coverage gates; formatting, OKF validation (68 files), and `git diff --check` passed. |
| Residual Risk | The deliberately narrow Discovery→UIA production friend remains assembly coupling; any additional production friend or consumer requires a DES revision and Human review. Live HWND behavior and target state-invariance remain assigned to the existing IT gates. No additional implementation risk is known within this unit/architecture slice. |

## Supersede Relationship

`IMP-0013` remains the historical evidence for the real UIA acquisition adapter and its original test gate. This artifact supersedes only its registry ownership and public surface: `UiaTargetHandleRegistry`, the `uia-target-` prefix, and the UIA-owned raw table are removed. The mapper, raw reader, read-only audit, and acquisition behavior remain `IMP-0013` evidence.

## Implemented Boundary

- Discovery is the sole token/raw-table writer. A bridge instance contains exactly one locked registry core, preserving deterministic session-local `tgt-1`, `tgt-2`, ... issuance without using HWND, title, path, process image, or caller ordinal as token material.
- `IWindowTargetHandleRegistry`, `IWindowTargetHandleResolver`, `WindowTargetHandleRegistry`, `Win32TargetHandle`, `ResolvedWindowTarget`, and all raw operations are `internal`.
- `DiscoveryUiaBridge` exposes only its public constructor. Its raw delegates are internal; the only production `InternalsVisibleTo` target is `Surveyor.Adapters.Uia`. Test-only friends remain distinguishable and do not expand production capability.
- UIA consumes `ResolvedWindowTarget` immediately and retains no enumeration or mutation operation. App/Application/Domain/Presentation/Reports/Policy/Capture/Store cannot compile against raw types or bridge members.
- A seeded `HWND = 0x1234` (`4660`) reaches the fake raw reader but neither decimal nor hexadecimal text reaches the opaque token or `RunDiagnostic` output. Architecture tests also keep the raw bridge free of logging and persistence sinks.

## Failing-First And Counter-Example Evidence

Before production implementation, Architecture tests failed because UIA lacked the Discovery edge, Discovery lacked its production friend, the methodless bridge public API was absent, and the expected graph differed from source. The production change was then made without weakening those oracles.

The green suite retains synthetic mutations that prove rejection of:

- a raw boundary reference from each of App, Application, Domain, Presentation, Reports, Policy, Capture, and Store;
- a second production friend;
- Capture→Discovery;
- a public `ResolvedWindowTarget`;
- a forbidden `DiscoveryUiaBridge.TryResolve` call;
- a raw bridge logging/persistence sink marker.

## Quality Gate Evidence

```text
dotnet build Surveyor.slnx -c Release --no-restore
Build succeeded. 0 Warning(s), 0 Error(s)

dotnet test tests\Surveyor.Adapters.Uia.Tests\Surveyor.Adapters.Uia.Tests.csproj --no-build -c Release -v minimal
Passed: 61, Failed: 0

dotnet test tests\Surveyor.Architecture.Tests\Surveyor.Architecture.Tests.csproj --no-restore -v minimal
Passed: 25, Failed: 0

dotnet test eng\Surveyor.Unit.slnf --no-restore -v minimal
Architecture 25; Domain 59; Application 31; Policy 45; Reports 7; Presentation 16; all passed
Domain 97.01% line; Application 87.56%; Policy 100%; Reports 96.94%

dotnet format Surveyor.slnx --verify-no-changes --no-restore
Passed

powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\okf\Validate-Okf.ps1
OKF validation passed for 68 markdown files under knowledge.

git diff --check
Passed
```
