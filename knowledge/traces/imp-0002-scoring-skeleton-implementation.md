---
type: Trace Evidence
title: IMP-0002 Scoring Skeleton Implementation
description: Implementation evidence for the M08 deterministic scoring, classification, and improvement candidate skeleton.
tags: [trace, implementation, imp-0002, des-0010, rq-005, rq-017, rq-018, rq-019, rq-020, rq-021, rq-022, rq-023, rq-029, rq-034, rq-051]
timestamp: 2026-07-04T00:00:00+09:00
---

# IMP-0002 Scoring Skeleton Implementation

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `IMP-0002`, M08 scoring skeleton implementation |
| Upstream | `DES-0010`; `UT-0002`; `IMP-0001`; Issue #60; `RQ-005`, `RQ-017`, `RQ-018`, `RQ-019`, `RQ-020`, `RQ-021`, `RQ-022`, `RQ-023`, `RQ-029`, `RQ-034`, `RQ-051`; `RD-005`, `RD-006`, `RD-007`, `RD-008`, `RD-009`, `RD-010`, `RD-011`, `RD-014`, `RD-015`, `RD-016`, `RD-020` |
| Downstream | `src/Surveyor.Domain/Scoring/ScoringContracts.cs`; `src/Surveyor.Domain/Scoring/TestabilityScorer.cs`; `src/Surveyor.Domain/Model/SupportedPatterns.cs`; `tests/Surveyor.Domain.Tests/ScoringSkeletonBehaviorTests.cs`; `knowledge/traces/ut-0002-scoring-determinism-evidence.md` |
| Evidence | Added `ScoringConfig.DefaultV1()`, score/class/candidate records and enums, deterministic `TestabilityScorer.Score`, axis-specific basis-point scoring, weighted aggregation, confidence caps, ordered classification, root-cause de-duplication, and candidate generation that copies `PriorityBasis` without computing priority. Domain code reads no clock, files, UIA objects, capture data, or localization resources. `UT-0002` now asserts every v1 evidence-code vector emitted by M08 from the neutral model, so later UIA/MSAA adapter population is a downstream input-mapping task rather than an unresolved M08 behavior gap. |
| Verification | Initial RED: `dotnet test tests\Surveyor.Domain.Tests --filter UT0002` failed because `Surveyor.Domain.Scoring` did not exist. GREEN after implementation and PR #92 residual-risk closure: `dotnet test tests\Surveyor.Domain.Tests --no-restore --filter UT0002 -v minimal` passed 15 tests with `Surveyor.Domain` line coverage 80.16%; `dotnet test -v minimal` passed `Architecture` 8, `Domain` 24, and `Policy` 2 tests with Domain 90.23% and Policy 94.33% line coverage. `dotnet build -v minimal` passed with 0 warnings / 0 errors, and `dotnet format --verify-no-changes --no-restore` passed from the repository root. |
| Residual Risk | None known for the `IMP-0002` M08 scoring skeleton behavior. The concrete UIA/MSAA adapter population of neutral observations remains owned by downstream acquisition slices (`DES-0014`), not by this pure-domain implementation. One scoped `CA1506` suppression remains on the internal scoring pipeline because the slice coordinates the full M08 contract surface; method-level complexity was split and no behavioral suppressions were added. |

## Implementation Notes

- `Unavailable` axes use `UnknownDueToUnavailable` with null `ScoreBp`; they are excluded from aggregate numerator and denominator.
- Aggregation and formula math use integer basis points and one half-away-from-zero rounding point.
- Sorting uses enum order and `StringComparer.Ordinal`; output ids are derived from enum codes and canonical keys, not `GetHashCode()` or dictionary iteration.
- `PriorityBasis` is copied to `ScoreResult` and candidate records only when supplied by the caller.
- Candidate ordering is code, scope, target key with null last, and id.

## Pattern Record

Strategy-like versioned configuration: `ScoringConfig` carries v1 weights, thresholds, rounding, and candidate rule version so later calibrated scoring can replace the configuration without changing the pure scorer entry point; hard-coded hidden thresholds were rejected because they would make DES-0017 calibration and report provenance opaque.
