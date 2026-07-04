---
type: Trace Evidence
title: TRC-0002 DES-0008 Scaffold Implementation Evidence
description: Solution/project scaffold, core unit-lane solution filter, architecture-test behavior, failing-first counter-example, and verification evidence for DES-0008.
tags: [trace, implementation, unit-test, scaffold, des-0008, rq-054, rq-051]
timestamp: 2026-07-04T00:00:00+09:00
---

# TRC-0002 DES-0008 Scaffold Implementation Evidence

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `TRC-0002`, DES-0008 scaffold implementation and architecture-test evidence |
| Upstream | `DES-0008`, `RQ-054`, `RQ-051`, Issue #20 |
| Downstream | `Surveyor.slnx`, `eng/Surveyor.Unit.slnf`, `src/**`, `tests/Surveyor.TestSupport`, `tests/Surveyor.Architecture.Tests`, `tests/fixtures/**`, `tests/integration/**`, `tests/it-fixtures/**` |
| Evidence | Created the DES-0008 project scaffold; used `Surveyor.slnx` as the solution file per human instruction during this slice; centralized determinism and quality settings; added architecture tests for dependency direction, root namespaces, centralized settings, coding-standards quality gates (`CS-01`/`CS-02`/`CS-05`/`CS-06`/`CS-07`/`CS-08`), banned-API analyzer wiring, public API baselines, and the unit solution filter; recorded the required failing-first forbidden-reference counter-example and a banned-API analyzer probe. PR #92 moved the unit solution filter under `eng/` so root `dotnet format --verify-no-changes` resolves `Surveyor.slnx` unambiguously, and added coverlet include filters so each core test project gates its matching production assembly. |
| Verification | RED: `dotnet test tests\Surveyor.Architecture.Tests\Surveyor.Architecture.Tests.csproj --no-restore -v normal` failed after temporarily adding `Surveyor.Domain -> Surveyor.Application`. Banned API probe: `dotnet build src\Surveyor.Domain\Surveyor.Domain.csproj --no-restore -v minimal` failed with `RS0030` for temporary `DateTime.UtcNow`. Public API probe: a temporary documented `public` type in `Surveyor.Domain` failed with `RS0016`. GREEN: `dotnet build eng\Surveyor.Unit.slnf --no-restore -v minimal`, `dotnet test tests\Surveyor.Architecture.Tests\Surveyor.Architecture.Tests.csproj --no-restore -v minimal`, and `dotnet format --verify-no-changes --no-restore` passed with 0 warnings/errors and 8 passing architecture tests. |
| Residual Risk | Windows-facing project build, final Windows TFM / SDK pin, and Windows App SDK package pin remain Human validation items on Windows 11. Integration fixture apps are placement-only README scaffolds in this slice; DES-0014/DES-0015 own fixture content. |

## Created Projects

Core lane (`net10.0` via `Directory.Build.props`):

- `src/Surveyor.Domain/Surveyor.Domain.csproj`
- `src/Surveyor.Application/Surveyor.Application.csproj`
- `src/Surveyor.Policy/Surveyor.Policy.csproj`
- `src/Surveyor.Reports/Surveyor.Reports.csproj`
- `tests/Surveyor.TestSupport/Surveyor.TestSupport.csproj`
- `tests/Surveyor.Architecture.Tests/Surveyor.Architecture.Tests.csproj`

Windows-facing scaffold (`net10.0-windows10.0.19041.0` via `Directory.Build.props`; not built in the local unit lane):

- `src/Surveyor.Adapters.Discovery/Surveyor.Adapters.Discovery.csproj`
- `src/Surveyor.Adapters.Uia/Surveyor.Adapters.Uia.csproj`
- `src/Surveyor.Adapters.Capture/Surveyor.Adapters.Capture.csproj`
- `src/Surveyor.Adapters.Store/Surveyor.Adapters.Store.csproj`
- `src/Surveyor.Presentation/Surveyor.Presentation.csproj`
- `src/Surveyor.App/Surveyor.App.csproj`

Placement-only harness directories:

- `tests/fixtures/uia-trees/`
- `tests/fixtures/golden/`
- `tests/integration/`
- `tests/it-fixtures/Surveyor.ITFixture.WinForms/`
- `tests/it-fixtures/Surveyor.ITFixture.Win32/`

## Architecture-Test Behaviors

`tests/Surveyor.Architecture.Tests/ArchitectureProjectGraphTests.cs` contains:

- `ProjectReferencesFollowDes0008InwardDependencyRule`
- `RootNamespacesMatchProjectModuleMap`
- `DeterminismAndQualitySettingsAreCentralized`
- `BannedApiAnalyzerIsEnabledForDomainAndApplicationCore`
- `CodingStandardsQualityGatesAreMechanicallyConfigured`
- `SourceProjectsDeclarePublicApiBaselines`
- `DomainAndApplicationStayFreeOfWindowsFrameworkReferences`
- `UnitSolutionFilterContainsOnlyCoreLaneProjects`

The dependency-direction test reads the `ProjectReference` graph directly from project XML, so unused forbidden references are caught mechanically rather than depending on emitted assembly metadata.

## RED Counter-Example

Temporary change:

```xml
<ProjectReference Include="..\Surveyor.Application\Surveyor.Application.csproj" />
```

added under `src/Surveyor.Domain/Surveyor.Domain.csproj`.

Observed failure:

```text
失敗 Surveyor.Architecture.Tests.ArchitectureProjectGraphTests.ProjectReferencesFollowDes0008InwardDependencyRule
Assert.Equal() Failure: Collections differ
Expected: []
Actual:   ["Surveyor.Application"]
```

The forbidden reference was then removed and the suite passed.

## Banned-API Probe

Temporary `src/Surveyor.Domain/BannedApiProbe.cs` used `DateTime.UtcNow`.

Observed failure:

```text
error RS0030: シンボル 'DateTime.UtcNow' は、このプロジェクト : Use the application-owned IClock abstraction instead of ambient UTC time (RQ-051). では禁止されています
```

The temporary probe was then removed and the core lane passed.

## Public-API Probe

Temporary `src/Surveyor.Domain/PublicApiProbe.cs` declared a documented `public sealed class PublicApiProbe` without adding it to `PublicAPI.Unshipped.txt`.

Observed failure:

```text
error RS0016: シンボル 'PublicApiProbe' は宣言されたパブリック API の一部ではありません
```

The temporary probe was then removed and the core lane passed.

## Verification Commands

```powershell
dotnet restore tests\Surveyor.Architecture.Tests\Surveyor.Architecture.Tests.csproj --force-evaluate
dotnet test tests\Surveyor.Architecture.Tests\Surveyor.Architecture.Tests.csproj --no-restore -v normal
dotnet build src\Surveyor.Domain\Surveyor.Domain.csproj --no-restore -v minimal
dotnet restore eng\Surveyor.Unit.slnf --force-evaluate
dotnet build eng\Surveyor.Unit.slnf --no-restore -v minimal
dotnet test tests\Surveyor.Architecture.Tests\Surveyor.Architecture.Tests.csproj --no-restore -v minimal
dotnet format --verify-no-changes --no-restore
```

`eng/Surveyor.Unit.slnf` intentionally includes only the core subset and excludes `tests/integration/**` and `tests/it-fixtures/**`.
