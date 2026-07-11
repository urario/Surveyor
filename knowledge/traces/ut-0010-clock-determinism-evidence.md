---
type: Unit Test Evidence
title: UT-0010 Clock Determinism Evidence
description: Behavior-test evidence for fixed-clock UTC timestamp determinism.
tags: [unit-test, ut-0010, rq-051, determinism, clock]
timestamp: 2026-07-11T00:00:00+09:00
---

# UT-0010 Clock Determinism Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `UT-0010`, fixed-clock timestamp determinism, unit test phase |
| Upstream | `RQ-051`, `RD-020`, `DES-0009`, `DES-0011`, `DES-0012`, Issue #49 |
| Downstream | `IMP-0004`; future `IMP-0008`/`IMP-0009` report writers and `IMP-0015` DI wiring |
| Evidence | `tests/Surveyor.Application.Tests/ClockDeterminismBehaviorTests.cs` covers fixed UTC clock behavior, exact DES-0012 UTC timestamp format, and culture independence. |
| Verification | `dotnet test tests/Surveyor.Application.Tests/Surveyor.Application.Tests.csproj` passed: 6 tests, Application line coverage 100%. `dotnet test eng/Surveyor.Unit.slnf` passed for the unit lane. |
| Residual Risk | Report writer integration remains in `IMP-0008`/`IMP-0009`; DI-wide single-clock enforcement remains in `IMP-0015`. |

## Behavior

`FixedClock` normalizes configured instants to UTC and returns the same value for every read. `UtcTimestampFormatter` formats `DateTimeOffset` values as `yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'` with `InvariantCulture`, so ambient culture and local offsets do not affect deterministic report timestamps.
