---
type: Implementation Evidence
title: IMP-0004 Clock Seam Implementation
description: Implementation evidence for the application-owned clock seam and deterministic timestamp helper.
tags: [implementation, imp-0004, rq-051, determinism, clock]
timestamp: 2026-07-11T00:00:00+09:00
---

# IMP-0004 Clock Seam Implementation

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `IMP-0004`, IClock and fixed-clock seam implementation, implementation phase |
| Upstream | `RQ-051`, `RD-020`, `DES-0009`, `DES-0011`, `DES-0012`, `UT-0010`, Issue #62 |
| Downstream | `IMP-0008`, `IMP-0009`, `IMP-0011`, `IMP-0015` |
| Evidence | Added `Surveyor.Application.Time.IClock`, `SystemClock`, internal `UtcTimestampFormatter`, `Surveyor.TestSupport.FixedClock`, and `Surveyor.Application.Tests` coverage for `UT-0010`. |
| Verification | `dotnet test tests/Surveyor.Application.Tests/Surveyor.Application.Tests.csproj` passed; `dotnet test tests/Surveyor.Architecture.Tests/Surveyor.Architecture.Tests.csproj` passed; `dotnet build Surveyor.slnx` passed with 0 warnings; `dotnet test eng/Surveyor.Unit.slnf` passed; `dotnet format Surveyor.slnx --verify-no-changes` passed; `git diff --check` passed; `tools/okf/Validate-Okf.ps1` passed for 52 markdown files. |
| Residual Risk | `SystemClock` is not wired into DI in this slice; report writers do not yet consume the formatter until `IMP-0008`/`IMP-0009`. |

## Notes

The production seam is intentionally small: only UTC is exposed, local time is unavailable, and serialization formatting uses a fixed invariant UTC representation. `SystemClock` is the single intended ambient-time adapter for application use cases; tests use `FixedClock`.
