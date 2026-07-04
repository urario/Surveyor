---
type: Trace Evidence
title: UT-0002 Scoring Determinism Evidence
description: Behavior-test evidence for deterministic scoring, axis mapping, unavailable semantics, de-duplication, rounding, confidence, classification boundaries, config validation, dictionary-order independence, and no fabricated priority.
tags: [trace, unit-test, ut-0002, des-0010, rq-034, rq-051]
timestamp: 2026-07-04T00:00:00+09:00
---

# UT-0002 Scoring Determinism Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `UT-0002`, scoring determinism and classification behavior |
| Upstream | `DES-0010`; Issue #60; `RQ-005`, `RQ-017`, `RQ-018`, `RQ-019`, `RQ-020`, `RQ-021`, `RQ-022`, `RQ-023`, `RQ-029`, `RQ-034`, `RQ-051`; `RD-005` through `RD-011`, `RD-014`, `RD-015`, `RD-016`, `RD-020` |
| Downstream | `tests/Surveyor.Domain.Tests/ScoringSkeletonBehaviorTests.cs`; `src/Surveyor.Domain/Scoring/ScoringContracts.cs`; `src/Surveyor.Domain/Scoring/TestabilityScorer.cs` |
| Evidence | Added tests covering deterministic result equality under element/config dictionary reordering, fresh-process scoring payload stability under invariant/culture environment settings, exact v1 evidence-code vectors for all seven axes, `Unavailable` as unknown rather than zero, root-cause candidate de-duplication without hiding distinct improvement perspectives, precondition state metadata / finding / candidate emission, basis-point rounding, overall confidence caps, classification threshold boundaries, config validation failure, priority-basis copy-only behavior, and public value-object equality operators introduced by the DES-0010 public API alignment. |
| Verification | RED first: `dotnet test tests\Surveyor.Domain.Tests --filter UT0002` failed with missing `Surveyor.Domain.Scoring` types after NuGet restore was allowed. PR #92 review RED: `UT0002RootCauseDeduplicationKeepsDistinctImprovementCandidates` and `UT0002PreconditionAxisUsesStateMetadataAndEmitsImprovementFindings` failed against the prior implementation. GREEN after review closure: `dotnet test tests\Surveyor.Domain.Tests --no-restore --filter UT0002 -v minimal /p:CollectCoverage=false` passed 16 tests; `dotnet test tests\Surveyor.Domain.Tests --no-restore -v minimal` passed 26 tests with `Surveyor.Domain` line coverage 91.43%, satisfying the CS-07 threshold. Full gate: `dotnet test -v minimal` passed `Architecture` 8, `Domain` 26, and `Policy` 2 tests with Domain 91.43% and Policy 94.33% line coverage. |
| Residual Risk | Serializer byte identity is not asserted in this slice because report serialization belongs to `DES-0012`; the deterministic tests compare ordered domain payload values directly. Culture-sensitive formatting is avoided in domain code; the fresh-process probe sets invariant globalization and a non-default `LANG` value to catch environment-dependent payload drift. |

## Behavior Tests

- `UT0002DeterministicAggregateClassAndCandidateOrderIgnoreElementOrder`
- `UT0002ScoringPayloadIsStableAcrossFreshProcess`
- `UT0002AxisMappingEmitsStableScoresAndEvidenceForEveryV1Axis`
- `UT0002UnavailableDoesNotBecomeNumericZero`
- `UT0002RootCauseDeduplicationKeepsDistinctImprovementCandidates`
- `UT0002PreconditionAxisUsesStateMetadataAndEmitsImprovementFindings`
- `UT0002BasisPointMidpointRoundingIsHalfAwayFromZero`
- `UT0002OverallConfidenceFollowsParticipatingAxesAndUnknownWeightCaps`
- `UT0002ClassificationBoundaryOrderIsStable`
- `UT0002ConfigValidationFailsBeforeScoring`
- `UT0002SuppliedPriorityBasisIsCopiedButNotComputed`

## Determinism Evidence

The deterministic-order test scores the same synthetic screen twice with child element order reversed and `AxisWeights`, `SignalWeights`, and `SignalThresholds` dictionaries reversed. It asserts equal aggregate score, class, finding id order, and candidate id order, proving the implementation does not depend on input enumeration order for the covered behavior.

The fresh-process test spawns a child `dotnet test` process with coverage disabled for the child probe only, invariant globalization enabled, and a non-default `LANG` value. It compares aggregate score, percent, class, confidence, ordered axis payload, finding ids, and candidate ids against the parent process payload.
