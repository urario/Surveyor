---
type: Trace Evidence
title: UT-0014 Domain / Policy Mutation-Focused Coverage Evidence
description: Behavior-test evidence for Domain and Policy Stryker.NET mutation score recovery under the CS-10 quality gate.
tags: [trace, unit-test, ut-0014, cs-10, mutation, domain, policy, rq-051, rq-052]
timestamp: 2026-07-05T19:40:00+09:00
---

# UT-0014 Domain / Policy Mutation-Focused Coverage Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `UT-0014`, Domain / Policy mutation-focused unit-test evidence |
| Upstream | `CS-10` mutation-score quality gate ([coding-standards](../process/coding-standards.md)); Issue #97; Issue #95; `RQ-051`, `RQ-052` |
| Downstream | `tests/Surveyor.Domain.Tests/ScoringMutationCoverageTests.cs`; `tests/Surveyor.Domain.Tests/ScoringMutationCoverageAdditionalTests.cs`; `tests/Surveyor.Domain.Tests/DomainValueObjectContractTests.cs`; `tests/Surveyor.Policy.Tests/ConfidentialityMutationCoverageTests.cs` |
| Evidence | Added focused behavior tests for Domain scoring validation/classification branches, Domain key/value-object determinism contracts, Policy confidentiality decisions, sanitizer boundaries, fallback-key export, and fallback-key derivation invariants. No production code was changed for the sake of mutation score. |
| Verification | `dotnet test eng\Surveyor.Unit.slnf -v minimal --no-restore`; local Windows Stryker re-measurement using the IMP-0016 baseline tooling overlay |
| Result | Domain mutation score: 80.54% (414 killed / 93 survived / 7 no coverage / 99 compile errors / 742 total). Policy mutation score: 89.02% (73 killed / 9 survived / 0 no coverage / 12 compile errors / 120 total). Both targets are at or above the 80% CS-10 threshold. |
| Residual Risk | Remaining Domain survivors concentrate in `Scoring/TestabilityScorer.cs` and value-object edge cases; Policy survivors concentrate in `Sha256FallbackKeyDerivation.cs` and isolated confidentiality branches. These are below the threshold but should remain visible for the next hardening slice rather than being suppressed. |

## Behavior Tests

`Surveyor.Domain.Tests`:

- `ScoringMutationCoverageTests.cs` pins `ScoringConfig.Validate` positive/negative guard branches, axis/signal weight boundaries, zero signal weights, partial applicability for action/result elements, and distinct fallback/unstable identity finding codes.
- `ScoringMutationCoverageAdditionalTests.cs` pins element-vs-screen improvement-candidate scope, target element key retention, and screen-level no-target-key behavior.
- `DomainValueObjectContractTests.cs` pins stable digest output, key equality components, key material separator escaping, fallback propagation, single-field equality differences, and hash-code participation for equality components.

`Surveyor.Policy.Tests`:

- `ConfidentialityMutationCoverageTests.cs` pins decision-source allowlist behavior, opt-out transform boundaries, masking/export rejections, exception and text sanitizer boundaries, export-id truncation boundaries, fallback-token determinism, whitespace normalization, scope/text separation, and escaped separator material.

## Mutation-Killer Rationale

The added assertions target survivor classes that line coverage alone did not constrain:

- Boundary mutants: export-id truncation, length buckets, axis/signal sums, zero weights, and allowlisted decision sources.
- Branch and short-circuit mutants: fallback-token export, partial element applicability, screen/element candidate scopes, and confidentiality mode/target switches.
- Determinism and privacy contracts: stable digests, fallback flags, key material escaping, and lower-case fallback token format.
- Value-object contracts: equality components and hash-code components for keys, labels, bounds, availability, identities, state discriminators, and supported patterns.

The tests are relational or contract-based where possible, preserving `RQ-051` deterministic behavior and avoiding raw sensitive material exposure under `RQ-052`.

## Execution Evidence

`dotnet test eng\Surveyor.Unit.slnf -v minimal --no-restore`:

- `Surveyor.Architecture.Tests`: 8 passed.
- `Surveyor.Adapters.Uia.Tests`: 52 passed.
- `Surveyor.Domain.Tests`: 59 passed; line coverage 97.01%, branch coverage 91.93%, method coverage 93.3%.
- `Surveyor.Policy.Tests`: 45 passed; line/branch/method coverage 100%.

Stryker.NET re-measurement:

- Domain: final mutation score 80.54%.
- Policy: final mutation score 89.02%.

The Stryker re-measurement was executed on the PR branch with a local IMP-0016 tooling overlay applied for measurement only. The overlay itself is not part of this PR; this trace records the measured scores and the test additions that produced them.
