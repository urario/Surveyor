---
type: Trace Evidence
title: IMP-0016 Stryker.NET Baseline Implementation
description: Implementation evidence for introducing the pinned local dotnet-stryker tool, Domain/Policy Stryker configuration, and first CS-10 mutation-score baseline.
tags: [trace, implementation, imp-0016, cs-10, stryker, mutation-testing]
timestamp: 2026-07-05T00:00:00+09:00
---

# IMP-0016 Stryker.NET Baseline Implementation

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `IMP-0016`, Stryker.NET baseline implementation, implementation phase |
| Upstream | [Coding Standards](../process/coding-standards.md) `CS-10`; [IMP-0007](imp-0007-read-only-audit-implementation.md) Stryker.NET not-yet-installed handoff; Issue #95 |
| Downstream | `.config/dotnet-tools.json`; `eng/stryker/domain.stryker-config.json`; `eng/stryker/policy.stryker-config.json`; `tools/stryker/Run-StrykerBaseline.ps1`; generated reports under `artifacts/stryker/domain/reports/` and `artifacts/stryker/policy/reports/` |
| Evidence | Added pinned local tool manifest entry `dotnet-stryker` `4.16.0`, selected from `dotnet tool search dotnet-stryker` on SDK `10.0.301`. Added separate Domain and Policy Stryker configs targeting the current core projects with test coverage: `Surveyor.Domain` via `Surveyor.Domain.Tests`, and `Surveyor.Policy` via `Surveyor.Policy.Tests`. Both configs use `reporters` = `progress`, `html`, `markdown`, `mutation-level` = `Standard`, `thresholds.high` = `80`, and `thresholds.break` = `0` so CS-10 is recorded as non-blocking baseline evidence. Added `tools/stryker/Run-StrykerBaseline.ps1` because this SDK/tool-manifest combination restored the local tool but `dotnet tool run dotnet-stryker` still reported "run dotnet tool restore"; the script reads the pinned manifest version and invokes the restored Stryker CLI DLL directly after `dotnet tool restore`. |
| Verification | `dotnet tool search dotnet-stryker --take 10` reported latest `dotnet-stryker` `4.16.0`; `dotnet tool restore` restored `dotnet-stryker` `4.16.0` from `.config/dotnet-tools.json`; `dotnet test tests\Surveyor.Domain.Tests\Surveyor.Domain.Tests.csproj -v minimal` passed 26 tests with `Surveyor.Domain` line coverage 91.43%; direct Domain Stryker run completed in 00:10:10 with final mutation score 70.43% and generated HTML/Markdown reports; direct Policy Stryker run completed in 00:01:31 with final mutation score 68.29% and generated HTML/Markdown reports; `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -Target Policy -SkipVersionCheck` reproduced the Policy baseline and score 68.29%. |
| Residual Risk | Both baseline scores are below the CS-10 target of 80%, but `thresholds.break` remains 0 per Issue #95 so the baseline is non-blocking. `dotnet tool run dotnet-stryker` and `dotnet dotnet-stryker` reported that `dotnet tool restore` was needed even after restore on this machine; the committed script is the reproducible workaround. Stryker 4.16.0 runs on SDK 10.0.301 in this repository when executed from the restored package DLL, but the NuGet package itself ships its tool under `tools/net8.0/any`; future SDK/tool behavior should be rechecked when upgrading. |

## Commands

Restore the pinned local tool:

```powershell
dotnet tool restore
```

Run both configured CS-10 baselines:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -SkipVersionCheck
```

Run one baseline:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -Target Domain -SkipVersionCheck
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -Target Policy -SkipVersionCheck
```

The script writes generated Stryker reports to ignored artifact directories:

- `artifacts/stryker/domain/reports/mutation-report.html`
- `artifacts/stryker/domain/reports/mutation-report.md`
- `artifacts/stryker/policy/reports/mutation-report.html`
- `artifacts/stryker/policy/reports/mutation-report.md`

## Baseline Scores

| Target | Test project | Mutants | Killed | Survived | No coverage | Compile errors | Score |
| -- | -- | --: | --: | --: | --: | --: | --: |
| `Surveyor.Domain` | `Surveyor.Domain.Tests` | 742 | 362 | 122 | 30 | 99 | 70.43% |
| `Surveyor.Policy` | `Surveyor.Policy.Tests` | 120 | 56 | 24 | 2 | 12 | 68.29% |

## Improvement Candidates

Domain survived-mutant concentration:

- `Scoring\TestabilityScorer.cs`: 78 survived mutants and 13 no-coverage mutants. Strengthen tests around scoring rule sensitivity, boundary weights, unavailable-axis aggregation, and candidate/root-cause branch distinctions.
- `Scoring\ScoringContracts.cs`: 9 survived mutants and 7 no-coverage mutants. Add assertions for DTO validation, enum/threshold boundary behavior, and value-object equality/guard semantics that are currently surviving simple mutations.
- Key and model value objects: `KeyMaterial`, `ScreenKey`, `ElementKey`, `BoundingRect`, `ScreenStateDiscriminator`, and `SupportedPatterns` have surviving or no-coverage mutants. Add negative and boundary tests for canonicalization, component ordering, empty/default values, and coordinate/flag edge cases.

Policy survived-mutant concentration:

- `Confidentiality\ConfidentialityPolicy.cs`: 9 survived mutants and 1 no-coverage mutant. Add tests for decision branch boundaries, destination-specific masking differences, and opt-out source handling.
- `Sha256FallbackKeyDerivation.cs`: 6 survived mutants and 1 no-coverage mutant. Add tests for salt/scope/input ordering and malformed or boundary identity material.
- `SensitiveValueSanitizer.cs` and `FallbackKeyExportMapper.cs`: 9 combined survived mutants. Add assertions for path/key masking edge cases, repeated-token behavior, and export pseudonym mapping invariants.

No score-improvement tests were added in this slice because Issue #95 explicitly scopes score improvement to follow-up work.
