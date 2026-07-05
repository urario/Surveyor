---
type: Trace Evidence
title: UT-0014 Domain / Policy Mutation-Focused Coverage Evidence
description: Behavior-test evidence adding surviving/no-coverage mutant killers for the Domain scoring config-validation and classification branches and the Policy confidentiality, sanitizer, fallback-key export, and fallback-key derivation invariants, under the CS-10 mutation-score quality gate.
tags: [trace, unit-test, ut-0014, cs-10, mutation, domain, policy, rq-051, rq-052]
timestamp: 2026-07-05T00:00:00+09:00
---

# UT-0014 Domain / Policy Mutation-Focused Coverage Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `UT-0014`, Domain / Policy mutation-focused重点テスト追加, unit-test phase |
| Upstream | `CS-10` mutation-score quality gate ([coding-standards](../process/coding-standards.md) §CS-10); [stryker workflow / IMP-0016 baseline](#stryker-re-measurement-status) referenced by Issue #97; Issue #97; Issue #95; `RQ-051`, `RQ-052` |
| Downstream | `tests/Surveyor.Domain.Tests/ScoringMutationCoverageTests.cs`; `tests/Surveyor.Policy.Tests/ConfidentialityMutationCoverageTests.cs`; a follow-up Stryker re-measurement once the IMP-0016 tooling is on the default branch |
| Evidence | Added focused behavior tests targeting the surviving / no-coverage mutant concentrations called out for `Surveyor.Domain` (`Scoring/ScoringContracts.cs` `ScoringConfig.Validate`, `Scoring/TestabilityScorer.cs` classification branches) and `Surveyor.Policy` (`Confidentiality/ConfidentialityPolicy.cs`, `Confidentiality/SensitiveValueSanitizer.cs`, `Confidentiality/FallbackKeyExportMapper.cs`, `Sha256FallbackKeyDerivation.cs`). No product code was changed for the sake of mutation score. |
| Verification | `dotnet test eng\Surveyor.Unit.slnf --no-build -v minimal` (CS-07). See [Execution status](#execution-status): the managed remote environment cannot install the .NET 10 SDK (the `builds.dotnet.microsoft.com` egress host is denied by policy), so `dotnet test` and the Stryker re-run were **not** executed in this session; the tests were authored against the existing behavior-test patterns and must be confirmed green by CI. |
| Residual Risk | The Stryker re-measurement (Issue #97 完了条件 3) is deferred — see [Stryker re-measurement status](#stryker-re-measurement-status). Adding tests cannot guarantee the 80% mutation-score target is reached; the intent is to remove the highest-value surviving/no-coverage mutants in the enumerated files. If the target still misses after these additions, the remaining survivor concentration must be recorded for a Human decision, without any score manipulation. |

## Behavior Tests

`Surveyor.Domain.Tests` (`ScoringMutationCoverageTests.cs`):

`ScoringConfigValidationTests` — pins each `ScoringConfig.Validate` invariant branch individually:

- `UT0014DefaultV1ConfigValidates` (always-throw mutant killer)
- `UT0014BlankVersionIsRejected`
- `UT0014BlankCandidateRulesVersionIsRejected`
- `UT0014UnsupportedRoundingIsRejected`
- `UT0014AxisWeightsWithMissingKeyIsRejected`
- `UT0014NegativeAxisWeightIsRejected`
- `UT0014AxisWeightSumOtherThanTenThousandIsRejected`
- `UT0014SignalWeightsWithUndefinedAxisIsRejected`
- `UT0014EmptySignalWeightsIsRejected`
- `UT0014NegativeSignalWeightIsRejected`
- `UT0014SignalWeightSumOtherThanTenThousandIsRejected`

`ScoringClassificationBranchTests` — pins the `TestabilityScorer` classification branches not previously observed:

- `UT0014NoScorableAxesYieldsNotEnoughEvidence` (`UsedWeightBp == 0` → `NoScorableAxes` finding + `NotEnoughEvidence` + `Unknown` confidence)
- `UT0014NonFixableBlockingYieldsNotEnoughEvidence` (`AcquisitionUnavailable` blocking is non-fixable)
- `UT0014DuplicateIdentityYieldsImproveFirstAndUniquenessCandidate` (fixable blocking → `ImproveFirst` + `MakeAutomationIdentityUnique`)

`Surveyor.Policy.Tests` (`ConfidentialityMutationCoverageTests.cs`):

`ConfidentialityPolicyEdgeCaseTests`:

- `UT0014UnknownDecisionSourceIsRejected` (allowlist)
- `UT0014TestFixtureSourceIsAllowed` (allowlist positive member)
- `UT0014NonDefaultSourceOptOutIsAccepted` (`Default`-only guard inversion)
- `UT0014MaskedShareableExportWithOptOutRecordIsRejected`
- `UT0014UnknownRequestedModeIsRejected` (switch default)
- `UT0014BlankSourceAndNullRequestAreRejected`
- `UT0014UnknownMaskingTargetIsRejected` (`RequiresTextMasking` switch default)

`SensitiveValueSanitizerEdgeCaseTests`:

- `UT0014RemainingExceptionKindsAreMapped` (`Argument` / `InvalidOperation` / `Timeout`)
- `UT0014NullMaskValueIsRejected`

`FallbackKeyExportMapperEdgeCaseTests`:

- `UT0014ExportIdIsTruncatedAtEightCharacterBoundary` (`<=` short-id boundary)
- `UT0014NonFallbackKeyWithTokenStillMapsToFallback` (`|| fallbackToken is not null` short-circuit)

`FallbackKeyDerivationMutationTests`:

- `UT0014InvalidInputsAreRejected`
- `UT0014FallbackTokenIsLowercaseHexAndDeterministic` (32-char lowercase hex format)
- `UT0014NormalizeV1TrimsAndCollapsesWhitespace`
- `UT0014ScopeIsTrimmedAndScopeTextAreNotInterchangeable`
- `UT0014EscapeSensitiveCharactersRemainDeterministicAndDistinct`

## Mutation-Killer Rationale

The additions target the mutant classes most likely to survive line-coverage-satisfied tests:

- **Boundary relational mutants** — `ShortExportId` `<=` boundary is pinned by exercising both an 8-character and a 9-character `ExportId`; `ScoringConfig.Validate` sum checks are pinned by off-by-one weight sums.
- **Branch / short-circuit mutants** — `FallbackKeyExportMapper` `elementKey.IsFallback || fallbackToken is not null` is pinned by a non-fallback key that still carries a token; each `Validate` guard is pinned by a single-fault input so a removed guard turns exactly one test red.
- **Switch-default and allowlist mutants** — the `ConfidentialityPolicy` decision-source allowlist (including the positive `TestFixture` member), the `Default`-only opt-out guard, the mode `switch` default, and the `RequiresTextMasking` target `switch` default are each observed.
- **String-normalization mutants** — `NormalizeV1` trim + whitespace-fold and the scope/text label boundary in `Sha256FallbackKeyDerivation` are pinned relationally (equal-when-equivalent, not-equal-when-distinct, non-interchangeable scope/text) so magic-constant fragility is avoided while still killing the deletions.

All new assertions are relational or throw/guard observations rather than hand-computed aggregate magic numbers, keeping them deterministic (`RQ-051`) and free of raw sensitive values (`RQ-052`).

## Execution Status

This session ran inside the managed remote (Linux) environment. The core projects target `net10.0`, which requires the .NET 10 SDK, but the SDK download host `builds.dotnet.microsoft.com` is denied by the environment egress policy (HTTP 403 on `CONNECT`). The SDK could therefore not be installed and neither `dotnet test eng\Surveyor.Unit.slnf` nor the Stryker runner could be executed here. The tests were written against the established behavior-test fixtures (`ScoringFixture`, `ConfidentialityFixture`) and contracts; CI on the default toolchain is the authoritative green/red signal.

## Stryker Re-measurement Status

Issue #97 references `knowledge/process/stryker-workflow.md`, `tools/stryker/Run-StrykerBaseline.ps1`, and `knowledge/traces/imp-0016-stryker-baseline.md` (the `IMP-0016` baseline, Issue #95). Those artifacts are **not present on the current default branch** that this work branches from, so the正規 Stryker re-run (完了条件 3) and the baseline candidate list (範囲限定) cannot be consumed or re-executed as written in this slice. The re-measurement and the new-score recording must be performed once the `IMP-0016` mutation tooling is merged, on a Windows toolchain per the `CS-10` workflow. This trace records the重点テスト additions (完了条件 1) and the deferral so a Human can sequence the re-measurement.
