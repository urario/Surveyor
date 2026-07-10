---
okf_version: "0.1"
type: Implementation Evidence
id: IMP-0005
title: Discovery Port and Fake Implementation
status: implemented
tags: [surveyor, implementation, discovery, port, fake, rq-049, rq-051]
---

# IMP-0005 Discovery Port and Fake Implementation

## Trace

| Field | Evidence |
| --- | --- |
| Artifact | `IMP-0005`, discovery port and deterministic fake |
| Issues | #63; closes the implementation side of #42 |
| Upstream | `DES-0011` fixed DTO/status model; `DES-0014` discovery field details |
| Requirements | `RQ-049`, `RQ-051`; guardrails `RQ-048`, `RQ-052`, `RQ-054` |
| Production files | `src/Surveyor.Application/Dto/*`, `src/Surveyor.Application/Ports/ITargetDiscoveryPort.cs` |
| Test-support files | `tests/Surveyor.TestSupport/FakeTargetDiscoveryPort.cs`, `tests/Surveyor.TestSupport/FakeTargetDiscoveryCandidate.cs` |
| Test files | `tests/Surveyor.Application.Tests/DiscoveryPortBehaviorTests.cs`, `tests/Surveyor.Application.Tests/RunDiagnosticContractTests.cs` |

## Implementation Notes

- Added Application-owned DTOs for discovery query, target references, target candidates, process info, discovery/resolve results, operation status, stages, diagnostic severity, and safe diagnostics.
- Added `ITargetDiscoveryPort` with `ListTargetsAsync` and `ResolveAsync` methods matching the DES-0011 contract.
- Added a deterministic fake discovery port in `Surveyor.TestSupport` so unit tests can cover discovery ordering and status mapping without real Windows UI targets.
- Kept Application DTOs UI-independent and sanitized; raw handles, paths, UI text, and adapter internals do not cross the port boundary.
- Registered `Surveyor.Application.Tests` in the root solution and unit solution filter and updated architecture project-graph expectations.

## Verification

| Command | Result |
| --- | --- |
| `dotnet test tests\Surveyor.Application.Tests\Surveyor.Application.Tests.csproj` | Passed: 5 tests; `Surveyor.Application` line coverage 100% |
| `dotnet test tests\Surveyor.Architecture.Tests\Surveyor.Architecture.Tests.csproj` | Passed: 8 tests |
| `dotnet build Surveyor.slnx` | Passed with 0 warnings and 0 errors |
| `dotnet test eng\Surveyor.Unit.slnf` | Passed: Architecture 8, Adapters.Uia 52, Domain 59, Application 5, Policy 45 |
| `dotnet format Surveyor.slnx --verify-no-changes` | Passed |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\okf\Validate-Okf.ps1` | Passed: 52 markdown files |
| `git diff --check` | Passed |

## Residual Risk

This slice intentionally does not implement live UIA/MSAA discovery. Real Windows discovery ordering, same-integrity probing, `uiAccess` behavior, and live permission failures remain downstream adapter and integration-test work.
