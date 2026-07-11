---
okf_version: "0.1"
type: Unit Test Evidence
id: UT-0003
title: Discovery Ordering and Status Mapping Evidence
status: implemented
tags: [surveyor, unit-test, discovery, determinism, rq-049, rq-051]
---

# UT-0003 Discovery Ordering and Status Mapping Evidence

## Trace

| Field | Evidence |
| --- | --- |
| Artifact | `UT-0003`, discovery candidate ordering and status mapping |
| Issues | #42, #63 |
| Upstream | `DES-0005`, `DES-0011`, `DES-0014` |
| Requirements | `RQ-049`, `RQ-051`; supports `RQ-048`, `RQ-052`, `RQ-054` by keeping the seam read-only and sanitized |
| Test files | `tests/Surveyor.Application.Tests/DiscoveryPortBehaviorTests.cs`, `tests/Surveyor.Application.Tests/RunDiagnosticContractTests.cs` |
| Support files | `tests/Surveyor.TestSupport/FakeTargetDiscoveryPort.cs`, `tests/Surveyor.TestSupport/FakeTargetDiscoveryCandidate.cs` |

## Covered Behaviors

- Scrambled discovery inputs are returned in deterministic order by process image name, safe name, then opaque session target id.
- Candidate statuses preserve `Ok`, `IntegrityMismatch`, `Unavailable`, and `PermissionDenied`.
- Process-name filtering uses ordinal comparison.
- Resolve maps known `Ok` targets to a `TargetReference`, known denied targets to their modeled status without a target, and unknown targets to `NotFound`.
- `RunDiagnostic` exposes only safe diagnostic fields and avoids raw HWND, UI text, file path, and exception message carriage.

## Verification

| Command | Result |
| --- | --- |
| `dotnet test tests\Surveyor.Application.Tests\Surveyor.Application.Tests.csproj` | Passed: 5 tests; `Surveyor.Application` line coverage 100% |
| `dotnet test tests\Surveyor.Architecture.Tests\Surveyor.Architecture.Tests.csproj` | Passed: 8 tests |
| `dotnet test eng\Surveyor.Unit.slnf` | Passed: Architecture 8, Adapters.Uia 52, Domain 59, Application 5, Policy 45 |
| `dotnet format Surveyor.slnx --verify-no-changes` | Passed |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\okf\Validate-Okf.ps1` | Passed: 52 markdown files |
| `git diff --check` | Passed |

## Residual Risk

Real process enumeration, HWND ordering, integrity-level detection, and permission probing remain adapter/integration scope for `IMP-0013` and `IT-0005`; this unit evidence covers the deterministic fake seam and Application-layer contract only.
