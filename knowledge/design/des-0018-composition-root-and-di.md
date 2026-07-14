---
type: Detailed Design
title: DES-0018 Composition Root and DI Detailed Design
description: Detailed design for Surveyor's single composition seam (M13) - the DI mechanism, per-assembly public registration seams, the full provider wiring table with lifetimes and selection keys, the injection invariants (read-only-only adapters, single IClock, single IConfidentialityPolicy, no real clock in test config), fail-fast wiring diagnostics, UT-0013 composition-root invariant tests, and the production-registration smoke.
resource: ../../docs/gui-testability-analyzer-requirements.md
tags: [detailed-design, composition-root, dependency-injection, layering, invariants, rq-054, rq-051, rq-052]
timestamp: 2026-07-14T00:00:00+09:00
---

# DES-0018 Composition Root and DI Detailed Design

This is detailed-design package 11 (the final package) from [DES-0007](des-0007-detailed-design-execution-strategy.md) section 4. It designs `M13`, the composition root: the one seam in Surveyor where abstractions meet concretes (`RQ-054`). It fixes how every application-owned port, use case, presentation port, and shared state object is wired into one object graph at startup, and — more importantly — it fixes the **injection invariants** that keep that wiring honest: only read-only adapters may be registered, exactly one `IClock` and exactly one `IConfidentialityPolicy` exist, and the real system clock can never leak into a test configuration (`R-ARC-01`).

Because the composition root touches all thirteen modules, this package is deliberately written against the *whole* assembled system, not a local slice. Its correctness criterion is a property of the complete graph, so its design decisions (DI mechanism, registration-seam ownership, core/production split, lifetimes) are made once for every port together, never per adapter.

Canonical requirements stay in [gui-testability-analyzer-requirements.md](../../docs/gui-testability-analyzer-requirements.md) (`RQ-xxx`) and derived requirements in [requirements-definition.md](../requirements/requirements-definition.md) (`RD-xxx`).

> **Version note (2026-07-14, per [DES-0007](des-0007-detailed-design-execution-strategy.md) §5.3, boundary-reshaping clarification before `UT-0013` / `IMP-0015`):** the original Invariant A required rejection of an unknown target-facing service but did not define how `CompositionInvariants` identifies that category. This revision closes the contract with the application-owned `ITargetFacingPort` marker: the three sanctioned target-facing ports inherit it, and any registered service assignable to the marker but outside the sanctioned set is rejected. It also eliminates the App bypass implied by a public raw resolver: Discovery exposes only a methodless `DiscoveryUiaBridge` carrier, while raw registry/resolver/result members stay internal and friend-visible solely to UIA. No raw resolver is a DI service, and architecture tests enforce the exact project/friend/metadata boundary. These are explicit supersede notes, not silent implementation choices.

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `DES-0018`, Composition Root and DI Detailed Design, detailed design phase |
| Upstream | [DES-0002](des-0002-module-responsibility-basic-design.md) `M13` responsibility; [DES-0007](des-0007-detailed-design-execution-strategy.md) §4 package 11 / §4.1 (`R-ARC-01`) / §4.2 DAG / §8.1 resolved decision; [DES-0008](des-0008-project-structure-and-test-harness.md) `Surveyor.App` composition home, project↔module map, inward dependency rule, `Surveyor.Application.Tests` UT-0013 seam; [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md) the nine application-owned ports, four use cases, `IClock`, `IStageTimeoutController`, read-only guardrail; [DES-0013](des-0013-confidentiality-storage-and-export.md) `IConfidentialityPolicy` composition and diagnostic-sanitization rule; [DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) UIA MTA-thread ownership and read-only audit; [DES-0015](des-0015-capture-and-snapshot-correspondence.md) capture adapter resource ownership; [DES-0016](des-0016-operating-ui-detailed-design.md) presentation ports, ViewModel catalog, shared session state; accepted [ADR-0002](../decisions/adr-0002-adapter-technology-selection.md) (concrete provider set); [DES-0005](des-0005-vmodel-traceability-and-downstream-tests.md) `UT-0013` obligation |
| Requirements | `RQ-048` (read-only target access), `RQ-054` (UI-independent core / single seam), `RQ-051` (determinism / single clock), `RQ-052` (confidentiality / single policy); derived `RD-025`, `RD-026`, `RD-032` |
| Downstream | Review gate #40 ("レビュー: DES-0018 の注入不変条件を確認"); prerequisite `IMP-0018` #113 (Discovery/UIA bridge migration); `UT-0013` #52 (composition-root invariants, failing-first); `IMP-0015` #73 (composition root + production-registration smoke after #113); downstream `IT-0007` functional round trip |
| Evidence | DI-mechanism decision, per-assembly public registration seams, core/production composition split, full provider wiring table, injection-invariant rules with mechanical enforcement + counter-examples, exact Discovery/UIA project/friend/type/member boundary, raw-handle diagnostic counter-example, fail-fast wiring-diagnostic shape, Mermaid composition class/sequence diagrams, contract-closure tables, edge-case table, fixture strategy, `UT-0013` intent, end-to-end wiring-smoke assumptions, `IMP-0018` prerequisite and `IMP-0015` handoff |
| Verification | [Validate-Okf.ps1](../../tools/okf/Validate-Okf.ps1); `git diff --check`; author-side `DRP-01`–`DRP-10` + [DES-0007](des-0007-detailed-design-execution-strategy.md) §9 self-review (below); `surveyor-design-review` + `surveyor-quality-review` pre-review evidence, then human owner final approval per [DES-0007](des-0007-detailed-design-execution-strategy.md) §5.2 (review gate #40); future `dotnet test tests/Surveyor.Application.Tests --filter UT0013` once `IMP-0015` source exists |
| Residual Risk | Concrete provider set carries `ADR-0002`'s residual risks unchanged; WinUI 3 hosting may adjust the physical provider-build location while preserving the application-owned core/invariants; `IStageTimeoutController`/`IScoringConfigProvider` and `SystemClock`-home work remain at `IMP-0015`; the current UIA-owned registry conflicts with the accepted Discovery ownership and must migrate in prerequisite `IMP-0018` #113 before #73; the single Discovery→UIA production friend is mechanically allowlisted but remains narrow assembly coupling; semantic target-facing ports can still omit `ITargetFacingPort`, so the architecture sanctioned-set test plus Human review remain required |

## Purpose And Success Criterion

The success criterion is not "the app starts and objects are connected." It is:

- **there is exactly one place** that knows both the concrete adapters and the application ports, and every other assembly stays ignorant of concretes (`RQ-054`);
- **the injection invariants are enforced mechanically at startup and provable in a WinUI-free unit test**, not upheld by reviewer vigilance — a wiring mistake (a second clock, a rogue target-facing adapter, the real clock in a test) fails fast with a sanitized diagnostic, and `UT-0013` catches each violation with a confirmed-red counter-example;
- **the invariant-bearing composition logic is testable headless** so `UT-0013` runs in the unattended unit lane ([DES-0008](des-0008-project-structure-and-test-harness.md) §CI), never needing a live WinUI shell;
- **the wiring is complete for the whole system** — all four use cases, all nine application ports, the domain scorer, the four presentation ports, the ViewModels, and the shared session state each have exactly one obvious registration with a justified lifetime, so `IMP-0015` never has to invent a lifetime or a registration order.

After this package, an implementer wiring `Surveyor.App` copies a table; a reviewer checks four invariants against one test.

## Module Coverage

This package designs **`M13` Composition Root** ([DES-0002](des-0002-module-responsibility-basic-design.md#m13-composition-root)). It designs no other module's internals; it *consumes* every other module's public port/type to wire them. `M11`'s `SystemClock` concrete is *registered* here but *owned* by [DES-0009](des-0009-domain-model-stable-keys-and-availability.md)/[DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md); the presentation ports and ViewModels are *registered* here but *owned* by [DES-0016](des-0016-operating-ui-detailed-design.md); the adapters are *registered* here but *owned* by [DES-0013](des-0013-confidentiality-storage-and-export.md)/[DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md)/[DES-0015](des-0015-capture-and-snapshot-correspondence.md).

## Scope And Non-Goals

In scope, fixed here:

1. The DI mechanism decision (container vs hand-wired) and its justification against the guardrails.
2. The **core/production composition split**: which registrations are adapter-agnostic and application-owned (headless-testable) vs which are Windows/WinUI-bound and `Surveyor.App`-owned.
3. The **per-assembly public registration seam** convention that keeps adapter implementations `internal` (CS-02) while `Surveyor.App` stays the only assembly that composes them.
4. The **full provider wiring table**: every application port and use case → concrete provider, provider-selection key, lifetime, and scope.
5. The **four injection invariants** and their mechanical enforcement, each paired with the `UT-0013` counter-example that proves the enforcement discriminates.
6. The **wiring-diagnostic shape** and the fail-fast-at-startup failure behavior.
7. The `UT-0013` unit-test intent and the end-to-end wiring-smoke integration assumptions.

Non-goals (owned elsewhere):

- Each port's contract → [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md) (application ports), [DES-0016](des-0016-operating-ui-detailed-design.md) (presentation ports).
- Adapter implementations and their internal seams (`IDataProtector`, `IAccessControlService`, the raw UIA reader, the WGC device) → [DES-0013](des-0013-confidentiality-storage-and-export.md)/[DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md)/[DES-0015](des-0015-capture-and-snapshot-correspondence.md). Those seams are composed *inside* each adapter's own registration seam, not by `M13`.
- Project/assembly layout, the inward dependency rule, and its architecture-test enforcement → [DES-0008](des-0008-project-structure-and-test-harness.md). This package assumes that structure and adds one allowed dependency (see [DI mechanism](#di-mechanism-decision)).
- The confidentiality masking/sanitizer *technique* → [DES-0013](des-0013-confidentiality-storage-and-export.md); this package only reuses its diagnostic-sanitization rule for the wiring diagnostics.
- WinUI page/`Frame` navigation mechanics and XAML → [DES-0016](des-0016-operating-ui-detailed-design.md)/`M01`; this package registers the ViewModels/ports, it does not design the visual shell.

## Upstream Decisions (binding)

- **[DES-0007](des-0007-detailed-design-execution-strategy.md) §8.1 (2026-07-01, `R-ARC-01`)**: the composition root is a standalone `DES-0018`, finalized after the adapter spike; its injection invariants and `UT-0013` intent were draftable early, but concrete provider wiring waits on `ADR-0002`.
- **[ADR-0002](../decisions/adr-0002-adapter-technology-selection.md) (Accepted, human-approved 2026-07-03, #30)**: the concrete provider set is fixed — raw-COM UIA acquisition, Windows.Graphics.Capture-primary/PrintWindow-fallback capture, unpackaged/same-integrity default. **The "after `ADR-0002`" gate this Issue names is therefore satisfied**, so this package specifies concrete provider wiring, not only the early invariant draft.
- **[DES-0008](des-0008-project-structure-and-test-harness.md)**: `Surveyor.App` is the composition-root physical home and the only assembly permitted to reference the full concrete-adapter set; the sole narrower exception is the mechanically allow-listed UIA→Discovery methodless-bridge project/friend boundary fixed by `DES-0014`. `Surveyor.Application.Tests` is the `UT-0013` seam home; the inward dependency rule and banned-framework-namespace guard hold.
- **[DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md)**: the nine application-owned ports (`ITargetDiscoveryPort`, `IUiTreeAcquisitionPort`, `IScreenCapturePort`, `IReportGenerationPort`, `IResultStorePort`, `IScoringConfigProvider`, `IConfidentialityPolicy`, `IStageTimeoutController`, `IClock`), the four use cases, the `TestabilityScorer` domain dependency, and the **read-only guardrail** — "no application DTO or use case exposes a target-mutating command" — which is the contract this package's read-only-only invariant rests on.
- **[DES-0013](des-0013-confidentiality-storage-and-export.md)**: `IConfidentialityPolicy` is implemented by `ConfidentialityPolicy` composed with `ISensitiveValueSanitizer`/`IFallbackKeyExportMapper`; the diagnostics-sanitization allowlist (safe codes/enums/counts only — no raw title/`Name`/path/exception message) governs any diagnostic this package emits.
- **[DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md)**: the UIA acquisition adapter runs on a dedicated Surveyor-owned **MTA acquisition thread** and marshals only plain domain values across the wrapper boundary; concrete DI wiring/lifetimes are explicitly delegated to `DES-0018` (this package). This thread-ownership fixes the adapter's lifetime as a long-lived singleton.
- **[DES-0016](des-0016-operating-ui-detailed-design.md)**: the presentation ports (`INavigationService`, `IDialogService`, `IUiDispatcher`, `IHtmlPreviewHost`) are owned by `Surveyor.Presentation` and implemented by `Surveyor.App` (`M01`); ViewModels live in `Surveyor.Presentation`, depend only on the four use cases + presentation ports, and are "constructed by the `DES-0018` composition root"; `RunSessionState`/`FindingSelectionState` are session-scoped shared state.

## Observed Source Deviations

The current scaffold under `src/` differs from prior design assumptions in ways `IMP-0015` must reconcile; recording them here keeps the wiring table honest (`DRP` data-flow closure):

| Observed | Prior assumption | Reconciliation |
| -- | -- | -- |
| `SystemClock` lives in `src/Surveyor.Application/Time/SystemClock.cs` (public) | [DES-0008](des-0008-project-structure-and-test-harness.md) §Module Coverage placed the `IClock` concrete in `Surveyor.App` for the MVP | Its *registration* is production-only regardless of physical home (see [Invariant D](#invariant-d-no-real-clock-in-a-test-configuration)); the core registration never registers it. Whether it physically moves to `Surveyor.App` is an `IMP-0015` cleanup, not a wiring blocker. |
| `src/Surveyor.Adapters.Uia/UiaTargetHandleRegistry.cs` publicly owns token minting (`uia-target-`) and the raw handle table; `IMP-0013` records 57 UIA tests around that historical shape | [DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) fixes Discovery as the token/raw-table owner and UIA as the sole reader | Preserve `IMP-0013` as historical evidence, then migrate the registry, token prefix, public API, UIA constructor/tests, and architecture guards in `IMP-0018` #113. #113 may proceed in parallel with headless `UT-0013` #52; both are prerequisites to `IMP-0015` #73. #73 does not absorb this ownership change. |
| `Surveyor.Adapters.Discovery`/`.Capture`/`.Store` are scaffolds without the complete production port implementations required by this graph | [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md)/[DES-0013](des-0013-confidentiality-storage-and-export.md)/[DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) define their ports/adapters | Their registration seams are designed here; Discovery bridge migration is #113, while remaining capture/store provider prerequisites complete before `IMP-0015` wires the production graph. |
| No DI framework is referenced anywhere in `src/` yet | — | This package chooses the mechanism (below); `IMP-0015` adds the reference. |
| `DeterministicReportWriter` (`IReportGenerationPort`) and the store/policy helpers are `internal` (CS-02 internal-default) | — | Confirms the [per-assembly public registration seam](#per-assembly-public-registration-seams) is *required*, not optional: `Surveyor.App` cannot `new` an internal type. |

## Data And Contract Design

### DI mechanism decision

**Decision: use `Microsoft.Extensions.DependencyInjection` (MEDI) as the container, driven by explicit, per-assembly registration extension methods over `IServiceCollection`. Reject both assembly-scanning/convention auto-registration and a fully hand-written new-expression graph.**

| Axis | Hand-written `new` graph | MEDI + assembly-scanning | MEDI + explicit registrations (chosen) |
| -- | -- | -- | -- |
| Determinism (`RQ-051`) | Deterministic but drifts as the graph grows | Scanning discovers types in a non-guaranteed order; hides what is registered | Every registration is an explicit, reviewable line; resolution order is irrelevant because all lifetimes are singleton/transient with no ordering effect |
| Invariant enforcement (`R-ARC-01`) | No uniform registration model to inspect; each invariant hand-coded per graph | Scanning actively *fights* the read-only-only invariant (it may auto-register an unaudited type) | `IServiceCollection` is a reflectable `ServiceDescriptor` list — the guard inspects it uniformly for "single `IClock`", "single policy", and "exactly one audited adapter per target-facing port" |
| Testability (`RQ-054`) | Core graph not separable from WinUI without discipline | Same | The core registration is a headless method over `IServiceCollection`; `UT-0013` calls it with fake-adapter registrations, no WinUI |
| Internal-default (CS-02) | `App` must reach internals or types go public | Scanning needs types discoverable (often public) | Each assembly's own extension method registers its `internal` type from *inside* that assembly; only the extension method + port are public |
| WinUI 3 hosting | Native fit (manual `App.xaml.cs`) | Works but adds a heavier host | MEDI's `ServiceCollection`/`ServiceProvider` integrates with a manually built provider held on `App`; no Generic Host required |

`Microsoft.Extensions.DependencyInjection.Abstractions` is a pure, OS-agnostic abstraction package (no `Microsoft.UI.*`/`Windows.*` surface), so referencing it from `Surveyor.Application` does not violate the [DES-0008](des-0008-project-structure-and-test-harness.md) inward rule or banned-namespace guard; it is added to the architecture-test's allowed-dependency set alongside the BCL. The concrete `Microsoft.Extensions.DependencyInjection` implementation package is referenced only by `Surveyor.App` and the test projects — never by `Surveyor.Domain`/`Surveyor.Application` — so the container implementation stays at the composition edge.

### Composition topology: core vs production

The composition is split so the invariant-bearing logic is headless and the WinUI/adapter-bound logic is isolated at the edge:

```mermaid
flowchart TB
  subgraph app[Surveyor.App - production composition root, net10.0-windows]
    prod[SurveyorHost.BuildProductionProvider]
    winui[WinUI shell resolves ShellViewModel]
  end
  subgraph appl[Surveyor.Application - portable core composition, net10.0 headless]
    core[SurveyorCoreRegistration.AddSurveyorCore]
    guard[CompositionInvariants.Validate]
  end
  subgraph seams[per-assembly public registration seams]
    disc[AddSurveyorDiscovery]
    uia[AddSurveyorUiaAcquisition]
    cap[AddSurveyorCapture]
    store[AddSurveyorResultStore]
    pol[AddSurveyorConfidentialityPolicy]
    rep[AddSurveyorReports]
    pres[AddSurveyorPresentation]
  end
  subgraph tests[Surveyor.Application.Tests - UT-0013, headless]
    testcompose[AddSurveyorCore + fake adapter registrations + FakeClock]
  end

  prod --> core
  prod --> disc & uia & cap & store & pol & rep & pres
  prod --> guard
  core --> guard
  winui --> prod
  testcompose --> core
  testcompose --> guard
```

- **`SurveyorCoreRegistration.AddSurveyorCore(IServiceCollection)`** (in `Surveyor.Application`, headless) registers only the adapter-agnostic services: the four use cases, the `TestabilityScorer` domain scorer, `IStageTimeoutController`, and `IScoringConfigProvider`. It **does not register any `IClock`, any adapter, or any presentation type** — those are supplied by the caller. This is the single piece `UT-0013` exercises.
- **Per-assembly registration seams** (below) each add their own concrete provider. `Surveyor.App` calls the production set; `Surveyor.Application.Tests` calls fakes.
- **`CompositionInvariants.Validate(IServiceCollection, CompositionMode)`** (in `Surveyor.Application`, headless) runs the four invariant checks over the assembled `ServiceCollection` before `BuildServiceProvider`. Both the production root (`CompositionMode.Production`) and the test composition (`CompositionMode.Test`) call it, so the same guard protects both.
- **`SurveyorHost.BuildProductionProvider()`** (in `Surveyor.App`) is the only WinUI-bound piece: it composes core + production adapter seams + presentation, calls `Validate`, builds the `ServiceProvider`, and hands it to the WinUI shell for first-page resolution.

### Per-assembly public registration seams

Each assembly that owns a concrete port implementation exposes **exactly one** public extension method; the implementation type stays `internal`:

```csharp
// Surveyor.Reports (implementation DeterministicReportWriter stays internal)
namespace Surveyor.Reports;
public static class ReportsRegistration
{
    public static IServiceCollection AddSurveyorReports(this IServiceCollection services);
}
```

| Assembly | Public seam | Registers (service → internal impl) | Notes |
| -- | -- | -- | -- |
| `Surveyor.Adapters.Discovery` | `AddSurveyorDiscovery` | `ITargetDiscoveryPort` → discovery adapter; concrete `DiscoveryUiaBridge` → one session singleton containing the internal registry core | target-facing seam stamps `IReadOnlyAuditedTargetAdapter`; owns token/raw table; bridge delegates internal writer/resolver calls but exposes no public raw member |
| `Surveyor.Adapters.Uia` | `AddSurveyorUiaAcquisition` | `IUiTreeAcquisitionPort` → `UiaTreeAcquisitionAdapter`, receiving the same `DiscoveryUiaBridge` singleton and using its friend-visible internal `TryResolve` delegate; read-only audit is composed internally | owns MTA acquisition thread; target-facing seam stamps `IReadOnlyAuditedTargetAdapter`; sole permitted project consumer/friend of Discovery raw members |
| `Surveyor.Adapters.Capture` | `AddSurveyorCapture` | `IScreenCapturePort` → capture adapter (WGC device + PrintWindow fallback composed internally) | scaffold today; `IMP-0014`; target-facing seam stamps `IReadOnlyAuditedTargetAdapter` |
| `Surveyor.Adapters.Store` | `AddSurveyorResultStore` | `IResultStorePort` → store adapter, composing its `IDataProtector`/`IAccessControlService`/`IStoreFileSystem`/`IExportBundleWriter` internal seams | scaffold today; store slice |
| `Surveyor.Policy` | `AddSurveyorConfidentialityPolicy` | `IConfidentialityPolicy` → `ConfidentialityPolicy`, composing `ISensitiveValueSanitizer`/`IFallbackKeyExportMapper` internally | implemented today |
| `Surveyor.Reports` | `AddSurveyorReports` | `IReportGenerationPort` → `DeterministicReportWriter` | implemented today (internal) |
| `Surveyor.Presentation` | `AddSurveyorPresentation` | ViewModels, `RunSessionState`, `FindingSelectionState` | presentation ports' *implementations* are `Surveyor.App` (WinUI) → registered by `SurveyorHost`, not here |

Rule: **adapter-internal seams are never registered by `M13`.** This includes `IWindowTargetHandleRegistry`, `IWindowTargetHandleResolver`, and `ResolvedWindowTarget`: the container sees only the harmless methodless `DiscoveryUiaBridge` carrier, while Discovery/UIA use its internal roles. `AddSurveyorResultStore` likewise composes `IDataProtector`/`IAccessControlService`/`IStoreFileSystem` inside `Surveyor.Adapters.Store`; the composition root sees only `IResultStorePort`. This keeps the read-only-only and single-policy invariants checkable over a small, application-level service set and prevents `SurveyorHost` from resolving raw-target capability outside a sanctioned port.

### Composition support types

```csharp
namespace Surveyor.Application.Composition;

// Result of the guard — safe by construction (no raw type internals/paths).
public sealed record CompositionDiagnostic(
    string Code,                 // e.g. "Composition.Clock.Duplicate"
    CompositionSeverity Severity,
    string ServiceTypeName,      // short type name only (safe), e.g. "IClock"
    IReadOnlyDictionary<string, string> SafeArgs);

public enum CompositionSeverity { Error, Warning }

// Which composition is being validated. Test tightens Invariant D.
public enum CompositionMode { Production, Test }

public sealed class CompositionValidationException : Exception
{
    public IReadOnlyList<CompositionDiagnostic> Diagnostics { get; }
}

public static class CompositionInvariants
{
    // Collects every Error violation, then throws CompositionValidationException
    // (fail-fast) if any exists, so one build reports all wiring defects at once.
    public static void Validate(IServiceCollection services, CompositionMode mode);
}

public static class SurveyorCoreRegistration
{
    // Adapter-agnostic: use cases, TestabilityScorer, IStageTimeoutController,
    // IScoringConfigProvider. Registers NO IClock, NO adapter, NO presentation type.
    public static IServiceCollection AddSurveyorCore(this IServiceCollection services);
}
```

`CompositionInvariants` and `SurveyorCoreRegistration` live in `Surveyor.Application` and therefore reference **no concrete adapter or clock type** — a hard constraint, since coupling the guard to a concrete (e.g. `SystemClock`, whose physical home is in flux — see [Observed source deviations](#observed-source-deviations)) would break the inward rule the moment that concrete moves outward. The guard reasons only over service *types* on `IServiceCollection` and over the layer-safe markers below. Every assembly that exposes a registration seam (`Surveyor.Application`, `Surveyor.Policy`, `Surveyor.Reports`, `Surveyor.Presentation`, and each `Surveyor.Adapters.*`) references only `Microsoft.Extensions.DependencyInjection.Abstractions` for it — the OS-agnostic abstraction, never the container implementation — so no seam pulls the container into an inner layer.

Three layer-safe marker **interfaces** support the guard without naming any concrete. All are declared in `Surveyor.Application.Composition` so the guard (also in `Surveyor.Application`) can reference them inward-safely; only their *implementations* live in the adapter assemblies (production) and `Surveyor.TestSupport` (fakes), which the guard never references:

```csharp
namespace Surveyor.Application.Composition;

// Interface declared here (Surveyor.Application) so the guard can reference it inward-safely.
// A registered service the composition guard treats as a deliberate test double.
// FakeClock / fake adapters in Surveyor.TestSupport implement it; no src/** type does.
public interface ISurveyorCompositionTestDouble { }

// CATEGORY marker for application ports that inspect a live target. The sanctioned
// v1 ports inherit it. A future target-facing port must inherit it before registration,
// which makes the allow-list comparison mechanical rather than name-based.
public interface ITargetFacingPort { }

// POSITIVE read-only proof, declared here so the guard can require it inward-safely.
// A target-facing adapter registered through a sanctioned seam carries this marker to
// attest it is the DES-0014 read-only-audited adapter (covered by the UT-0005 read-only
// spy). Production audited adapters implement it; TestSupport read-only fakes implement it;
// a rogue/unaudited/mistaken adapter does NOT, so the guard rejects it by DEFAULT
// (Invariant A is a positive check, not a "reject if self-declares mutating" negative one).
public interface IReadOnlyAuditedTargetAdapter { }
```

`ITargetDiscoveryPort`, `IUiTreeAcquisitionPort`, and `IScreenCapturePort` inherit `ITargetFacingPort`. The marker is service-side category metadata; `IReadOnlyAuditedTargetAdapter` is implementation-side positive proof. They are intentionally separate so an unknown target-facing *port* and an unaudited implementation under a known port produce distinct diagnostics.

The marker is a *claim*; the *proof* that the claim is honest is the [DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) `UT-0005` read-only spy at the adapter seam plus code review of where the marker is applied. The composition guard's contribution is to make the **default outcome rejection**: an adapter must positively present the audited marker (and be the sole registration for its port) to pass. Classification still depends on every newly designed target-facing Application port inheriting `ITargetFacingPort`; `Surveyor.Architecture.Tests` enumerates the exact sanctioned set and fails when it changes without the corresponding invariant update. Separately, the Discovery/UIA metadata test rejects raw bridge type/member references from App, Application, Domain, Presentation, Reports, Policy, Capture, and Store. This second check is necessary because App legitimately references the Discovery assembly for registration but must still be unable to use raw capability. Semantic target access hidden behind a new port that omits the marker cannot be inferred mechanically and remains a Human/code-review residual risk recorded below.

`CompositionDiagnostic` carries only a code, a severity, a short service *type name*, and allowlisted `SafeArgs` (counts, expected/actual multiplicities) — it reuses the [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md#diagnostics-model)/[DES-0013](des-0013-confidentiality-storage-and-export.md#diagnostics-and-exception-sanitization) sanitization posture. There is no target, screen, or file in scope at composition time, so there is nothing sensitive to leak beyond assembly/type identifiers, which are safe.

## Provider Wiring Table

The complete v1 wiring. "Selection key" is the mechanism that chooses the provider; in v1 every port has exactly one production provider, so the selection is the *choice of which registration seam `Surveyor.App` composes* (a compile-time set), not a runtime key (see [Provider selection](#provider-selection-and-lifetimes)).

| Service (application/presentation port or type) | Concrete provider (assembly) | Selection key (v1) | Lifetime | Rationale |
| -- | -- | -- | -- | -- |
| `IClock` | `SystemClock` (`Surveyor.Application.Time`) | production seam only | Singleton | stateless; single clock invariant (`RQ-051`); **never** registered by core (Invariant D) |
| `IConfidentialityPolicy` | `ConfidentialityPolicy` (`Surveyor.Policy`) | `AddSurveyorConfidentialityPolicy` | Singleton | stateless deterministic policy; single-policy invariant (`RQ-052`) |
| `TestabilityScorer` (domain type) | `TestabilityScorer` (`Surveyor.Domain`) | `AddSurveyorCore` | Singleton | pure deterministic scorer; no per-run state |
| `IScoringConfigProvider` | application default (`Surveyor.Application`) | `AddSurveyorCore` | Singleton | config resolution; deferred impl per [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md), supplied at `IMP-0015` |
| `IStageTimeoutController` | application default (`Surveyor.Application`) | `AddSurveyorCore` | Singleton | stateless per-call controller; deferred impl per [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md) |
| `ITargetDiscoveryPort` | discovery adapter (`Surveyor.Adapters.Discovery`) | `AddSurveyorDiscovery` | Singleton | target-facing; seam stamps `IReadOnlyAuditedTargetAdapter` (Invariant A positive proof) |
| `IUiTreeAcquisitionPort` | `UiaTreeAcquisitionAdapter` (`Surveyor.Adapters.Uia`) | `AddSurveyorUiaAcquisition` | Singleton | **owns the dedicated MTA acquisition thread** ([DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md)); target-facing → seam stamps `IReadOnlyAuditedTargetAdapter` (Invariant A) |
| `DiscoveryUiaBridge` (methodless public carrier containing internal `WindowTargetHandleRegistry`) | one bridge (`Surveyor.Adapters.Discovery`) | `AddSurveyorDiscovery`; same instance received by `AddSurveyorUiaAcquisition` | Singleton | internal core implements writer/resolver; bridge delegates through internal members; raw interfaces/core/result are not DI services and never reach forbidden consumers |
| `IScreenCapturePort` | capture adapter (`Surveyor.Adapters.Capture`) | `AddSurveyorCapture` | Singleton | owns the WGC device/frame-pool ([DES-0015](des-0015-capture-and-snapshot-correspondence.md)); target-facing → seam stamps `IReadOnlyAuditedTargetAdapter` (Invariant A) |
| `IResultStorePort` | store adapter (`Surveyor.Adapters.Store`) | `AddSurveyorResultStore` | Singleton | stateless over `%LOCALAPPDATA%`; internal DPAPI/ACL/fs seams composed inside |
| `IReportGenerationPort` | `DeterministicReportWriter` (`Surveyor.Reports`) | `AddSurveyorReports` | Singleton | stateless deterministic writer |
| `SelectTargetUseCase` | `Surveyor.Application` | `AddSurveyorCore` | Transient | stateless orchestrator; transient avoids accidental cross-invocation state |
| `AnalyzeScreenUseCase` | `Surveyor.Application` | `AddSurveyorCore` | Transient | same |
| `GenerateReportUseCase` | `Surveyor.Application` | `AddSurveyorCore` | Transient | same |
| `ExportResultUseCase` | `Surveyor.Application` | `AddSurveyorCore` | Transient | same |
| `INavigationService` / `IDialogService` / `IUiDispatcher` / `IHtmlPreviewHost` | WinUI implementations (`Surveyor.App`) | `SurveyorHost` (production only) | Singleton | one shell-bound implementation per app; not headless-testable → never in core |
| `ShellViewModel` | `Surveyor.Presentation` | `AddSurveyorPresentation` | Singleton | one shell; owns `RunUiState`/`RunActivityKind` reducer ([DES-0016](des-0016-operating-ui-detailed-design.md)) |
| other ViewModels (`TargetSelection`/`SelectionMetadata`/`RunProgress`/`ResultOverview`/`ElementFindings`/`SnapshotViewer`/`ReportExport`/`ConfidentialityChoices`) | `Surveyor.Presentation` | `AddSurveyorPresentation` | Transient | one per navigation; read shared singleton state |
| `RunSessionState` / `FindingSelectionState` | `Surveyor.Presentation` | `AddSurveyorPresentation` | Singleton | session-scoped shared state ([DES-0016](des-0016-operating-ui-detailed-design.md)); a single window session in v1 |

### Provider selection and lifetimes

- **v1 uses a single default provider set**, so provider *selection* is not a per-resolve key or runtime switch — it is which registration seams `Surveyor.App` composes. The [DES-0001](../architecture/des-0001-initial-architecture.md) extension strategy (a future alternative provider, e.g. a FlaUI acquisition variant) is accommodated by composing a *different* registration seam, not by adding keyed/named registrations now. Designing a runtime provider-key registry in v1 would be premature (`RD-026`); the seam is the module boundary, and it is already there.
- **No DI "scope" is used in v1.** Surveyor is a single-desktop-session app; a "run" is not a container scope. Run state is held in the singleton `RunSessionState`/`FindingSelectionState`, reset by the [DES-0016](des-0016-operating-ui-detailed-design.md) state-machine rules, not by scope disposal. Introducing request-style scopes would be over-engineering with no consumer. This is recorded so `IMP-0015` does not add a scope factory speculatively.
- **Singleton is the default**; transient is reserved for the cheap, stateless-per-invocation graph nodes (use cases, non-shell ViewModels). The two adapters that own OS resources (UIA MTA thread, WGC device) are singletons deliberately — repeated creation would re-spawn threads/devices and defeat [DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md)'s single-acquisition-thread model.
- **Disposal**: the resource-owning singletons (`IUiTreeAcquisitionPort`, `IScreenCapturePort`) implement `IDisposable`/`IAsyncDisposable` and are disposed by the container when the `ServiceProvider` is disposed at app shutdown (`SurveyorHost` disposes the provider on `App` exit). Disposal semantics of the MTA thread and WGC device are owned by [DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md)/[DES-0015](des-0015-capture-and-snapshot-correspondence.md); `M13` only guarantees the container disposes them exactly once.
- **No captive dependencies.** The lifetime split above is only safe if no singleton captures a shorter-lived service. The singletons here (adapters, scorer, policy, clock, session state, `ShellViewModel`) depend only on other singletons; the transients (use cases, non-shell ViewModels) depend on singletons — the safe direction. In particular `ShellViewModel` (singleton) must **not** hold the transient screen ViewModels (they are reached through `INavigationService`, per [DES-0016](des-0016-operating-ui-detailed-design.md)), or they would be pinned for the app lifetime. `SurveyorHost` builds the provider with `BuildServiceProvider(validateScopes: true, validateOnBuild: true)` so missing registrations and scope captures fail fast at build; the singleton-captures-transient case (which MEDI does not flag) is guarded by the lifetime table above being the single source of truth for `IMP-0015`.

## Injection Invariants (`R-ARC-01`)

Each invariant is stated as (a) what it forbids, (b) how `CompositionInvariants.Validate` detects the violation over `IServiceCollection`, and (c) the `UT-0013` counter-example that must go red. All four map to the [DES-0005](des-0005-vmodel-traceability-and-downstream-tests.md) `UT-0013` obligation and the `RQ-054`/`RQ-051`/`RQ-052` guardrails.

### Invariant A: read-only adapters only (positive proof)

- **Forbids**: registering a target-facing service outside the sanctioned set; registering an *unaudited* implementation under a sanctioned target-facing port; or registering more than one implementation for any target-facing port.
- **Basis, not re-litigated**: [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md#read-only-guardrail) already guarantees the application surface has *no* target-mutating method, and [DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) proves the acquisition adapter's read-only behavior at the adapter seam (`UT-0005` spy). `M13`'s job is to make the *wiring* prove — positively — that the resolved target-facing adapter is that audited one, so production can never resolve a non-audited adapter.
- **Raw-resolution bypass is structurally absent**: `IWindowTargetHandleRegistry`, `IWindowTargetHandleResolver`, and `ResolvedWindowTarget` are Discovery-internal, are not DI service types, and are friend-visible only to UIA. Therefore `SurveyorHost` cannot resolve or call raw capability outside the three sanctioned ports. This is enforced by architecture tests rather than by extending the Application-owned port marker to an outer-layer implementation seam.
- **Detection (positive check)**: the guard holds the canonical **target-facing port set** `{ ITargetDiscoveryPort, IUiTreeAcquisitionPort, IScreenCapturePort }`. It first enumerates every registered `ServiceType` assignable to `ITargetFacingPort`; any such type outside the canonical set yields `Composition.ReadOnly.ForbiddenTargetFacingService`. For **each** sanctioned port it then asserts:
  1. **exactly one** registration exists (zero → `Composition.ReadOnly.MissingTargetAdapter`; more than one → `Composition.ReadOnly.DuplicateTargetAdapter` — this closes the silent second/replacement-registration gap);
  2. that registration's implementation carries the `IReadOnlyAuditedTargetAdapter` marker (absent → `Composition.ReadOnly.UnauditedTargetAdapter`).
  Implementation-type inspection accepts an explicit implementation type or instance; a target-facing factory descriptor whose result type cannot be inspected is rejected as `Composition.ReadOnly.UnauditedTargetAdapter`. Because the default outcome for an unmarked, uninspectable, duplicate, or unknown target-facing registration is **rejection**, a rogue or mistaken adapter cannot pass by omission.
- **`UT-0013` counter-examples** (each confirmed red): (i) register an **unaudited** fake `IUiTreeAcquisitionPort` (no `IReadOnlyAuditedTargetAdapter`) → `Composition.ReadOnly.UnauditedTargetAdapter`; (ii) register **two** audited `IUiTreeAcquisitionPort` implementations → `Composition.ReadOnly.DuplicateTargetAdapter`; (iii) register a fictitious `ITargetControlPort : ITargetFacingPort` → `Composition.ReadOnly.ForbiddenTargetFacingService`; (iv) register a target-facing factory with no inspectable implementation type → `Composition.ReadOnly.UnauditedTargetAdapter`; the valid composition passes.

### Invariant B: single `IClock`

- **Forbids**: zero or more-than-one `IClock` registration.
- **Detection**: `services.Count(d => d.ServiceType == typeof(IClock)) == 1`, else `Composition.Clock.Duplicate` (>1) or `Composition.Clock.Missing` (0) Error.
- **`UT-0013` counter-example**: add a second `IClock` registration on top of the fake clock → `Validate` throws `Composition.Clock.Duplicate`.

### Invariant C: single `IConfidentialityPolicy`

- **Forbids**: zero or more-than-one `IConfidentialityPolicy` registration.
- **Detection**: `services.Count(d => d.ServiceType == typeof(IConfidentialityPolicy)) == 1`, else `Composition.Policy.Duplicate`/`Composition.Policy.Missing` Error.
- **`UT-0013` counter-example**: register two policies → `Validate` throws `Composition.Policy.Duplicate`.

### Invariant D: no real clock in a test configuration

- **Forbids**: a real (non-test-double) clock appearing in any headless/test composition.
- **Structural guarantee (primary)**: `AddSurveyorCore` **never registers an `IClock`**. The clock is a required input supplied by the composer — `SurveyorHost` (production) registers the real clock; `Surveyor.Application.Tests` registers `FakeClock`. Because core never knows any concrete clock, a test built on core + fakes *cannot* pull the real clock. The [DES-0008](des-0008-project-structure-and-test-harness.md) architecture test independently enforces that `Surveyor.App` alone sees the full concrete-adapter set (apart from the exact UIA→Discovery methodless-bridge boundary), and the banned-API analyzer forbids ambient time in `Surveyor.Application`, so the real clock is boxed at the production edge.
- **Detection (defense-in-depth), concrete-free**: in `CompositionMode.Test`, `Validate` asserts the single `IClock` registration's implementation type carries the `ISurveyorCompositionTestDouble` marker; a production clock never carries it, so a real clock in a test config trips `Composition.Clock.RealClockInTest`. The guard names **no** concrete clock type, so it is unaffected by where `SystemClock` physically lives (the finding this design deliberately avoids). In `CompositionMode.Production` the marker is not required, so the real clock is valid.
- **`UT-0013` counter-example**: in `Test` mode, deliberately register a non-marked clock (a stand-in for the real clock, without `ISurveyorCompositionTestDouble`) → `Validate` throws `Composition.Clock.RealClockInTest`; the normal `FakeClock` (marked) composition passes.

## Contract Closure

### Composition input → source, output → consumer (`DRP-03`)

| Composition method | Input → source | Output → consumer |
| -- | -- | -- |
| `AddSurveyorCore` | `IServiceCollection` from the composer | mutated collection with use cases/scorer/config/timeout registered → consumed by `SurveyorHost` and `UT-0013` |
| `AddSurveyor<Adapter>` seams | `IServiceCollection`; the adapter's own internal seams from inside its assembly | collection with `IPort → internal impl` → consumed by the composer |
| internal `IWindowTargetHandleRegistry.Register` on `DiscoveryUiaBridge` | raw window facts from Discovery enumeration | opaque `TargetReference` plus internal registry entry → selection/resolve and later friend-only UIA lookup |
| internal `IWindowTargetHandleResolver.TryResolve` on `DiscoveryUiaBridge` | `TargetReference` selected by the caller | friend-visible `ResolvedWindowTarget` → consumed immediately only by `UiaTreeAcquisitionAdapter`; not public or DI-resolvable |
| `CompositionInvariants.Validate` | the fully assembled `IServiceCollection`; `CompositionMode` from the composer | throws `CompositionValidationException` with sanitized `CompositionDiagnostic`s, or returns → consumed by `SurveyorHost` (fatal dialog) / `UT-0013` (assertion) |
| `SurveyorHost.BuildProductionProvider` | production adapter + presentation seams + core | validated `ServiceProvider` → consumed by the WinUI shell to resolve `ShellViewModel` |

Every input is derivable from the composer or the assembly's own internals; every output has a named consumer.

### Registration ownership (`DRP-05`)

| Registration | Single writer | Write timing | Rule |
| -- | -- | -- | -- |
| `IClock` | production `SurveyorHost` (`SystemClock`) or test composition (`FakeClock`) | at composition | never written by `AddSurveyorCore`; exactly one writer per composition (Invariant B/D) |
| `IConfidentialityPolicy` | `AddSurveyorConfidentialityPolicy` (prod) / policy fake (test) | at composition | exactly one (Invariant C) |
| each adapter port | that adapter's own registration seam | at composition | the seam is the only place its internal impl is named; `M13` never re-registers it |
| discovery target bridge | `AddSurveyorDiscovery` | singleton at composition; entries at session enumeration | one methodless public carrier contains one internal registry core and delegates internal writer/resolver calls; UIA receives the same instance, raw contracts are not DI registrations, and no other production friend exists |
| use cases / scorer / config / timeout | `AddSurveyorCore` | at composition | core is the only writer; production/test never re-register them |
| presentation ports | `SurveyorHost` (WinUI impls) | at composition (production only) | never in core; not headless-registerable |

### Round-trip inventory (`DRP-04`)

The composition root persists nothing and has no serialize/deserialize pair. Its only "round trip" is build→dispose: services created by the `ServiceProvider` are disposed by that same provider at shutdown (the resource-owning UIA/capture singletons especially). The pair is closed by container-owned disposal — `M13` introduces no manual create/dispose asymmetry.

## Class Design (UML)

```mermaid
classDiagram
  direction LR

  class SurveyorCoreRegistration {
    <<static>>
    +AddSurveyorCore(IServiceCollection) IServiceCollection
  }
  class CompositionInvariants {
    <<static>>
    +Validate(IServiceCollection, CompositionMode)
  }
  class CompositionDiagnostic {
    +string Code
    +CompositionSeverity Severity
    +string ServiceTypeName
  }
  class CompositionValidationException {
    +IReadOnlyList~CompositionDiagnostic~ Diagnostics
  }
  class SurveyorHost {
    <<Surveyor.App>>
    +BuildProductionProvider() ServiceProvider
  }

  class IClock { <<interface, Surveyor.Application>> }
  class IConfidentialityPolicy { <<interface, Surveyor.Application>> }
  class ITargetFacingPort { <<methodless interface, Surveyor.Application>> }
  class IReadOnlyAuditedTargetAdapter { <<methodless proof marker>> }
  class ITargetDiscoveryPort { <<interface, inherits target-facing marker>> }
  class IUiTreeAcquisitionPort { <<interface, inherits target-facing marker>> }
  class IScreenCapturePort { <<interface, inherits target-facing marker>> }
  class IResultStorePort { <<interface>> }
  class IReportGenerationPort { <<interface>> }
  class DiscoveryUiaBridge {
    <<Surveyor.Adapters.Discovery; public sealed; no public raw members>>
    -Register(rawWindow) TargetReference
    -TryResolve(TargetReference, out ResolvedWindowTarget) bool
  }
  class IWindowTargetHandleRegistry { <<internal, Discovery>> }
  class IWindowTargetHandleResolver { <<internal, friend-visible only to Uia>> }
  class ResolvedWindowTarget { <<internal, friend-visible result>> }
  class WindowTargetHandleRegistry { <<internal registry core>> }
  class DiscoveryAdapter { <<internal; audited target adapter>> }
  class UiaTreeAcquisitionAdapter { <<internal; audited target adapter>> }

  ITargetFacingPort <|-- ITargetDiscoveryPort
  ITargetFacingPort <|-- IUiTreeAcquisitionPort
  ITargetFacingPort <|-- IScreenCapturePort
  ITargetDiscoveryPort <|.. DiscoveryAdapter
  IReadOnlyAuditedTargetAdapter <|.. DiscoveryAdapter
  IUiTreeAcquisitionPort <|.. UiaTreeAcquisitionAdapter
  IReadOnlyAuditedTargetAdapter <|.. UiaTreeAcquisitionAdapter
  DiscoveryUiaBridge *-- WindowTargetHandleRegistry : contains
  IWindowTargetHandleRegistry <|.. WindowTargetHandleRegistry
  IWindowTargetHandleResolver <|.. WindowTargetHandleRegistry
  IWindowTargetHandleResolver ..> ResolvedWindowTarget
  DiscoveryAdapter --> DiscoveryUiaBridge : internal Register delegate
  UiaTreeAcquisitionAdapter --> DiscoveryUiaBridge : friend-only TryResolve delegate

  SurveyorHost --> SurveyorCoreRegistration : composes
  SurveyorHost --> CompositionInvariants : Validate before build
  SurveyorHost ..> IClock : registers SystemClock (prod)
  SurveyorHost ..> IConfidentialityPolicy : AddSurveyorConfidentialityPolicy
  SurveyorHost ..> ITargetDiscoveryPort
  SurveyorHost ..> IUiTreeAcquisitionPort
  SurveyorHost ..> IScreenCapturePort
  SurveyorHost ..> IResultStorePort
  SurveyorHost ..> IReportGenerationPort
  SurveyorHost ..> DiscoveryUiaBridge : registers/passes methodless carrier only
  CompositionInvariants --> CompositionValidationException : throws
  CompositionValidationException o-- CompositionDiagnostic
```

```mermaid
sequenceDiagram
  participant App as Surveyor.App (WinUI)
  participant Host as SurveyorHost
  participant Core as SurveyorCoreRegistration
  participant Seams as Adapter/Presentation seams
  participant Bridge as DiscoveryUiaBridge singleton
  participant Guard as CompositionInvariants
  participant SP as ServiceProvider
  participant Shell as ShellViewModel

  App->>Host: BuildProductionProvider()
  Host->>Core: AddSurveyorCore(services)
  Host->>Seams: AddSurveyorDiscovery/Uia/Capture/Store/Policy/Reports/Presentation(services)
  Seams->>Bridge: Discovery registers methodless singleton; Uia receives same instance
  Host->>Host: register SystemClock + WinUI presentation ports
  Host->>Guard: Validate(services, Production)
  alt any Error invariant violated
    Guard-->>Host: throw CompositionValidationException(sanitized diagnostics)
    Host-->>App: fatal — show UnexpectedFault dialog, exit (no degraded run)
  else valid
    Guard-->>Host: ok
    Host->>SP: services.BuildServiceProvider()
    Host-->>App: ServiceProvider
    App->>SP: resolve ShellViewModel
    SP-->>Shell: fully wired graph
  end
```

At analysis time, Discovery calls the bridge's internal `Register` delegate and UIA calls its friend-visible internal `TryResolve` delegate; both reach the contained internal registry core before the MTA acquisition loop, as shown in [DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md#class-design-uml). `SurveyorHost` can register/pass the carrier but cannot name any raw member.

## Edge-Case Table

| Case | Required behavior |
| -- | -- |
| Two `IClock` registrations | `Validate` fails fast with `Composition.Clock.Duplicate` (Invariant B); app does not start |
| No `IClock` registered | `Composition.Clock.Missing`; app does not start (a run with no clock would break `RQ-051`) |
| Two `IConfidentialityPolicy` registrations | `Composition.Policy.Duplicate` (Invariant C); app does not start |
| A real (non-test-double) clock in a test composition | `Composition.Clock.RealClockInTest` in `Test` mode (Invariant D, detected via the `ISurveyorCompositionTestDouble` marker — no concrete clock named); structurally prevented because core never registers a clock |
| An unaudited adapter registered under a target-facing port (no `IReadOnlyAuditedTargetAdapter`) | `Composition.ReadOnly.UnauditedTargetAdapter` (Invariant A positive proof); app does not start |
| A second/replacement registration for a target-facing port | `Composition.ReadOnly.DuplicateTargetAdapter` (Invariant A); app does not start — the silent-replacement gap is closed |
| A service type implementing `ITargetFacingPort` outside the sanctioned set | `Composition.ReadOnly.ForbiddenTargetFacingService` (Invariant A); app does not start |
| A target-facing registration uses an uninspectable factory descriptor | `Composition.ReadOnly.UnauditedTargetAdapter`; app does not start |
| App/Application/Domain/Presentation/Reports/Policy/Capture/Store references a Discovery raw bridge type/member | architecture test fails the exact consumer/type/member pair; public carrier reference alone is allowed only where required for registration/injection |
| Discovery exposes a raw registry/resolver/result type or member publicly | public-API/metadata architecture test fails; `SurveyorHost` must never gain a callable raw-resolution surface |
| Discovery grants a second production `InternalsVisibleTo`, or Capture/Store gains a Discovery project edge | architecture test fails the exact friend/project allowlist; design revision + Human review required |
| An application port has no registration | `BuildServiceProvider(validateOnBuild: true)` (plus a required-port check in `Validate`) fails fast, naming the missing port type — never a null-injection at first run |
| A singleton captures a transient/shorter-lived service (e.g. `ShellViewModel` holds a screen ViewModel) | prevented by the lifetime table (single source of truth) + `validateScopes: true`; screen ViewModels are reached via `INavigationService`, never injected into the shell singleton |
| Adapter singleton owns an OS resource (UIA MTA thread, WGC device) | registered `Singleton` so the thread/device is created once; disposed exactly once at provider disposal ([DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md)/[DES-0015](des-0015-capture-and-snapshot-correspondence.md)) |
| WinUI hosting cannot build the provider before the first page | `SurveyorHost` builds and validates the provider in `App` construction, before the shell resolves any ViewModel; if validation throws, the shell is never shown |
| A future alternative provider is added | composed as a different registration seam in `SurveyorHost`; no runtime keying added, no core change |
| Composition diagnostic content | only code/severity/short type name/allowlisted args — no raw path, assembly file location, or target data ([DES-0013](des-0013-confidentiality-storage-and-export.md) sanitization) |

## Diagnostics And Logging

This package emits diagnostics only at **composition time**, before any target is touched, so there is no screen/element/target data in scope. The `CompositionDiagnostic` shape reuses the [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md#diagnostics-model)/[DES-0013](des-0013-confidentiality-storage-and-export.md#diagnostics-and-exception-sanitization) posture: a stable code, a severity, a short service *type name* (safe — no namespace-path or file location), and allowlisted `SafeArgs` (expected/actual registration counts). It never carries an assembly file path, a raw exception message, or reflection dumps. Failure is **fail-fast**: an `Error` invariant throws `CompositionValidationException`; `Surveyor.App` surfaces it through the [DES-0016](des-0016-operating-ui-detailed-design.md) `DialogIntent.UnexpectedFault` fatal path and exits. There is no "degraded" composition — a mis-wired graph is a programmer/config defect, not a recoverable run status, so it must never reach a user-visible analysis run.

## Fixture Strategy

- **Fake adapter registrations** live in `Surveyor.TestSupport` ([DES-0008](des-0008-project-structure-and-test-harness.md)) as the same port fakes `UT-0011`/`UT-0012` reuse, exposed through a `AddSurveyorFakeAdapters(IServiceCollection)` test seam so `UT-0013` composes `AddSurveyorCore` + fakes + `FakeClock` with no WinUI, no Windows, and no live target.
- **Counter-example fixtures** are first-class: each invariant test has a paired "mis-wired" collection (duplicate clock, duplicate policy, real/unmarked clock in test, unaudited target adapter, duplicate target adapter, forbidden target-facing service) that must throw. The valid fixtures require the `Surveyor.TestSupport` read-only fakes to carry both `ISurveyorCompositionTestDouble` and `IReadOnlyAuditedTargetAdapter`; the read-only counter-example fakes deliberately omit the audited marker. A green `UT-0013` is only credited after the counter-example is confirmed red (`R-QA-01`).
- **Architecture counter-examples** accompany `IMP-0018` #113: parameterized fixtures introduce a raw bridge type/member reference from each forbidden consumer (`Surveyor.App`, `.Application`, `.Domain`, `.Presentation`, `.Reports`, `.Policy`, `.Adapters.Capture`, `.Adapters.Store`); publicize `ResolvedWindowTarget`; add a second production friend; and add Capture→Discovery. Each mutation must fail the precise metadata/project/friend oracle, then be reverted. The scan operates on referenced type/member metadata, not merely `ProjectReference`, because App already needs a legitimate Discovery reference to call its registration seam.
- No golden files; composition has no serialized output. The oracle is the thrown/absent `CompositionValidationException` and the resolved graph shape.

## Unit-Test Intent (`UT-0013`)

`UT-0013` lives in `tests/Surveyor.Application.Tests` ([DES-0008](des-0008-project-structure-and-test-harness.md)), runs headless in the unattended unit lane, and protects the composition-root invariants — not the DI framework itself.

| Behavior | Risk guarded | Fixture | Oracle | Counter-example (confirmed red) | Anti-pattern avoided |
| -- | -- | -- | -- | -- | -- |
| Valid core+fake composition builds and resolves the four use cases | wiring completeness (`RQ-054`) | `AddSurveyorCore` + fake adapters + `FakeClock` | `ServiceProvider` resolves each use case with all ports non-null | remove one adapter registration → resolve/validate fails naming the missing port | asserting `BuildServiceProvider()` merely does not throw, without resolving the graph |
| Read-only-only, positive proof (Invariant A) | an unaudited/rogue/duplicate target-facing adapter is resolved in production (`RQ-048`) | valid composition, then four mis-wirings: unaudited fake, duplicate audited adapter, `ITargetControlPort : ITargetFacingPort`, uninspectable factory | `Validate` throws the fixed unaudited/duplicate/forbidden codes; the single-audited-adapter composition passes | relying on naming conventions or a rogue adapter to self-declare as mutating; testing the port's method surface instead of wiring proof |
| Single clock (Invariant B) | two clocks / nondeterministic time source (`RQ-051`) | valid composition + a second `IClock` | `Validate` throws `Composition.Clock.Duplicate` | single clock → passes | asserting a specific clock instance rather than the multiplicity |
| Single policy (Invariant C) | two confidentiality policies (`RQ-052`) | valid composition + a second `IConfidentialityPolicy` | `Validate` throws `Composition.Policy.Duplicate` | single policy → passes | over-asserting policy identity |
| No real clock in test (Invariant D) | a real clock leaks into a test/headless config (`RQ-051`) | `AddSurveyorCore` + fakes, `CompositionMode.Test` | the sole `IClock` impl carries `ISurveyorCompositionTestDouble`; adding a non-marked clock throws `Composition.Clock.RealClockInTest` | production mode with a real (unmarked) clock is valid | proving the fake works but never proving the real clock is excluded; naming a concrete clock type in the guard |
| Diagnostic is sanitized | composition diagnostic leaks a path/internal (`RQ-052`) | a violation composition | `CompositionDiagnostic` contains only code/severity/short type name/safe args | inject a path-bearing arg → assertion catches it | trusting the diagnostic shape without asserting the absence of unsafe content |

Determinism: all `UT-0013` cases are pure over `IServiceCollection` — no time, culture, file system, or process dependence — so they are byte-stable across a fresh process and machine ([DES-0008](des-0008-project-structure-and-test-harness.md) unit lane).

## Integration Assumptions (production-registration smoke)

The composition root has two distinct gates so `IMP-0015` does not depend cyclically on its downstream `IT-0007`:

- **`IMP-0015` production-registration smoke**: on Windows, build the production service collection, validate it, build the provider, and resolve the shell root plus all four use cases. It touches no target and is the completion gate for #73.
- **`IT-0007` functional/manual round trip**: launch the real shell and perform analyze→review→report over the fixture. This remains downstream of `IMP-0015` and is not claimed as #73 evidence.

- **Assumptions**: local Windows 11, unpackaged/same-integrity ([ADR-0002](../decisions/adr-0002-adapter-technology-selection.md)); #73's smoke touches no target. The WinForms fixture target belongs only to downstream `IT-0007` ([DES-0008](des-0008-project-structure-and-test-harness.md)).
- **Smoke**: `SurveyorHost.BuildProductionProvider` builds and validates without throwing; the shell root and all four use cases resolve. A later `IT-0007` launch performs the target round trip.
- **Run mode**: the registration smoke is automated on the Windows lane; the target round trip remains a documented Human gate.

## Downstream Handoff

- **`UT-0013` (#52) — first failing test.** Start red: write `Composition_rejects_duplicate_clock` (or `Composition_rejects_forbidden_target_facing_service`) against a not-yet-existing `CompositionInvariants.Validate`; it fails to compile/throw, then goes green when `AddSurveyorCore` + `CompositionInvariants` land in `Surveyor.Application` and the fake-adapter seam lands in `Surveyor.TestSupport`. Home: `tests/Surveyor.Application.Tests`. Owner: `Codex`.
- **`IMP-0018` (#113) — prerequisite boundary migration.** Move UIA-owned token minting/raw registry into Discovery's methodless bridge; make writer/resolver/result internal with UIA as the only production friend; migrate UIA tests/public API; add exact project/friend/type/member and raw-diagnostic counter-examples. #73 remains blocked until this completes. Owner: `Codex`.
- **`IMP-0015` (#73) — implementation slice after #113.** (1) Add `Microsoft.Extensions.DependencyInjection.Abstractions` to `Surveyor.Application` and the impl package to `Surveyor.App`/tests. (2) Implement `SurveyorCoreRegistration.AddSurveyorCore` + `CompositionInvariants.Validate`. (3) Add the seven public registration seams, with Discovery registering only the methodless bridge carrier and Uia receiving that same instance. (4) Implement `SurveyorHost.BuildProductionProvider` and WinUI presentation-port wiring. (5) Reconcile the `SystemClock` home. Owner: `Codex`.
- **Verification command**: `dotnet test tests/Surveyor.Application.Tests --filter UT0013` on the unit lane; `dotnet build` warnings-as-errors; the automated production-registration smoke on the Windows lane; `Validate-Okf.ps1` for this artifact.
- **Minimal context bundle for the slice**: this package's [Provider Wiring Table](#provider-wiring-table), [Injection Invariants](#injection-invariants-r-arc-01), [Composition support types](#composition-support-types), [per-assembly seam](#per-assembly-public-registration-seams), and [Observed Source Deviations](#observed-source-deviations); [DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md)'s bridge/diagram/diagnostic rules; [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md)'s port list + read-only guardrail; [DES-0016](des-0016-operating-ui-detailed-design.md)'s ViewModel catalog/presentation ports; [DES-0008](des-0008-project-structure-and-test-harness.md)'s architecture-test rules; `RQ-048`/`RQ-054`/`RQ-051`/`RQ-052`.

## Revision Self-Review Evidence (2026-07-14)

This revision is boundary-reshaping. The full affected composition/discovery boundary was re-swept, not only the missing forbidden-port predicate.

| Pattern | Result |
| -- | -- |
| `DRP-01` | finding fixed: added explicit supersede/version notes; the three upstream target-facing ports and four-use-case split are preserved |
| `DRP-02` | finding fixed: canonical marker inheritance is in DES-0011; the methodless bridge, contained internal registry core, registry/resolver/result contracts, owners, visibility, and friend home are defined without inconsistent accessibility |
| `DRP-03` | finding fixed: Discovery write, opaque-reference carriage, friend-only UIA read, raw-result consumption, and production-smoke consumers are closed in tables and diagrams |
| `DRP-04` | checked clean: opaque projection remains one-way; no public/DI raw resolver, raw-handle inward path, or persistence round trip was introduced |
| `DRP-05` | finding fixed: Discovery is the single registry/token writer; UIA is the sole reader; current UIA ownership is explicit #113 migration debt rather than a duplicate accepted owner |
| `DRP-06` | checked clean: unknown service check precedes per-sanctioned-port multiplicity/audit checks and diagnostics are stably ordered |
| `DRP-07` | not applicable: no numeric decision rule was introduced |
| `DRP-08` | checked clean: uninspectable factories fail closed; #73 registration smoke and downstream `IT-0007` have non-cyclic failure gates |
| `DRP-09` | finding fixed: markers are Application-owned; raw contracts are Discovery-internal; only UIA has the project/friend edge; App can register but cannot call the bridge |
| `DRP-10` | checked: the review-triggered boundary reshape re-swept upstream marker definitions, source deviations, tables, both diagrams, architecture/diagnostic counter-examples, prerequisites, handoff, and residual risks |

## Residual Risks

- **Concrete provider set inherits `ADR-0002`'s carried risks** — elevated-target/`uiAccess`/MSIX not calibrated, WGC border/consent UX not visually recorded. Owned by [DES-0014](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md)/[DES-0015](des-0015-capture-and-snapshot-correspondence.md)/`IT-0005`; not reopened here.
- **WinUI 3 hosting adjustment** — *where* the production `ServiceProvider` is built (App constructor vs `OnLaunched`) and how the first page is resolved may need adjustment against Windows App SDK hosting constraints. The application-owned core registration + invariants are hosting-independent, so any such adjustment is confined to `SurveyorHost`. Carried.
- **Deferred application-default impls** — `IStageTimeoutController`/`IScoringConfigProvider` concrete implementations are still deferred by [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md); their registration lines exist here, their bodies land in `IMP-0015`. Carried.
- **`SystemClock` home deviation** — currently in `Surveyor.Application.Time`, not `Surveyor.App` as [DES-0008](des-0008-project-structure-and-test-harness.md) assumed. Neutralized for wiring by Invariant D (core never registers it), reconciled physically at `IMP-0015`. Carried.
- **Scaffold adapters** — discovery/capture/store registration seams are designed but their concrete implementations are created by `IMP-0013`/`IMP-0014`/the store slice; `IMP-0015` wires the full production graph only once those exist. Carried.
- **Marker omission** — a semantically target-facing future Application port can evade runtime categorization if its designer omits `ITargetFacingPort`. The architecture test enumerates the exact sanctioned set and requires an invariant update on change, but semantic classification still needs Human review. Carried.
- **Narrow friend coupling** — Discovery grants UIA the only production `InternalsVisibleTo`; architecture tests reject every second friend and forbidden consumer. Any expansion requires a DES revision and Human review. Carried.
- **Historical registry ownership** — `UiaTargetHandleRegistry` remains UIA-owned until prerequisite `IMP-0018` #113. #73 is explicitly blocked on that migration; no production composition may register the historical resolver shape. Carried until #113 closes.

## Related

- [DES-0007 Detailed Design Phase Execution Strategy](des-0007-detailed-design-execution-strategy.md) — §4 package 11, §4.1/§4.2 DAG, §8.1 `R-ARC-01`
- [DES-0002 Module Responsibility Basic Design](des-0002-module-responsibility-basic-design.md) — `M13`
- [DES-0008 Project Structure and Test Harness Detailed Design](des-0008-project-structure-and-test-harness.md) — `Surveyor.App` home, inward rule, UT-0013 seam
- [DES-0011 Port DTOs, Status Model, and Use-Case Orchestration Detailed Design](des-0011-port-dtos-status-model-and-use-case-orchestration.md) — ports, use cases, read-only guardrail
- [DES-0013 Confidentiality, Storage, and Export Detailed Design](des-0013-confidentiality-storage-and-export.md) — policy composition, diagnostic sanitization
- [DES-0014 Discovery, UIA/MSAA Acquisition, and Read-Only Audit Detailed Design](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) — MTA thread ownership
- [DES-0015 Capture and Snapshot Correspondence Detailed Design](des-0015-capture-and-snapshot-correspondence.md) — capture resource ownership
- [DES-0016 Operating UI Detailed Design](des-0016-operating-ui-detailed-design.md) — presentation ports, ViewModels, session state
- [ADR-0002 Adapter Technology Selection](../decisions/adr-0002-adapter-technology-selection.md) — concrete provider set
- [DES-0005 V-Model Traceability and Downstream Tests](des-0005-vmodel-traceability-and-downstream-tests.md) — `UT-0013` obligation
- [Lifecycle Traceability](../process/lifecycle-traceability.md)
- [Design Review Pattern Catalog](../process/design-review-patterns.md)
