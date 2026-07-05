---
type: Detailed Design
title: DES-0014 Discovery, UIA/MSAA Acquisition, and Read-Only Audit Detailed Design
description: Detailed design for read-only target discovery, UIA raw-COM tree acquisition with MSAA/WM_GETTEXT legacy fallbacks, UIA apartment/threading and cooperative cancellation, the RD-032 prohibited-pattern read-only audit, the identity-source ladder rung-1 runtime-id detection, the acquisition confidence rubric, virtualized-tree handling, and the minimal-privilege policy.
resource: ../../docs/gui-testability-analyzer-requirements.md
tags: [detailed-design, discovery, uia, msaa, read-only, threading, legacy-edges, rq-048, rq-049, rq-050]
timestamp: 2026-07-04T00:00:00+09:00
---

# DES-0014 Discovery, UIA/MSAA Acquisition, and Read-Only Audit Detailed Design

This is detailed-design package 7 from [DES-0007](des-0007-detailed-design-execution-strategy.md) section 4. It fixes how Surveyor discovers a target, reads its UIA/MSAA tree into the `DES-0009` domain model, proves it never mutates the target, and marks legacy edges honestly, so discovery slice `IMP-0005`, acquisition slice `IMP-0006`, read-only audit slice `IMP-0007`, and the real UIA adapter slice `IMP-0013` can proceed without inventing acquisition behavior. It is the first adapter-bound package and is unblocked by the accepted [ADR-0002](../decisions/adr-0002-adapter-technology-selection.md) (raw-COM UIA client, same-integrity unpackaged default).

Canonical requirements stay in [gui-testability-analyzer-requirements.md](../../docs/gui-testability-analyzer-requirements.md) (`RQ-xxx`) and derived requirements in [requirements-definition.md](../requirements/requirements-definition.md) (`RD-xxx`).

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `DES-0014`, Discovery, UIA/MSAA Acquisition, and Read-Only Audit Detailed Design, detailed design phase |
| Upstream | [ADR-0002](../decisions/adr-0002-adapter-technology-selection.md) (UIA raw COM, capture, packaging, minimal-privilege); [DES-0002](des-0002-module-responsibility-basic-design.md) `M05`/`M06`; [DES-0003](des-0003-module-interface-basic-design.md) `ITargetDiscoveryPort`/`IUiTreeAcquisitionPort`; [DES-0004](des-0004-analysis-flow-basic-design.md) Stages 1/2; [DES-0005](des-0005-vmodel-traceability-and-downstream-tests.md) `UT-0003`/`UT-0004`/`UT-0005`/`IT-0001`/`IT-0002`/`IT-0005`/`IT-0006`; [DES-0007](des-0007-detailed-design-execution-strategy.md) package 7, `R-WIN-02`, `R-WIN-03`, `R-GTA-02`, `R-SEC-02`; [DES-0008](des-0008-project-structure-and-test-harness.md) project homes and banned-API guards; [DES-0009](des-0009-domain-model-stable-keys-and-availability.md) identity-source ladder (rung-1 detection delegated here), `Availability`/`AcquisitionConfidence` semantics, `IdentityMaterial` and `IFallbackKeyDerivation` seam; **[DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md) (binding, not merely referenced): `ITargetDiscoveryPort.ListTargetsAsync`/`ResolveAsync` and `IUiTreeAcquisitionPort.AcquireAsync` method signatures, `TargetReference`/`TargetCandidate`/`TargetDiscoveryResult`/`TargetResolveResult`/`AcquisitionResult`/`OperationStatus`/`RunDiagnostic` shapes, and `IStageTimeoutController`-owned stage timeout/cancellation precedence — this package fixes only the field detail DES-0011 left open and adapter-internal mechanics, and does not rename methods or redefine already-fixed DTOs** |
| Requirements | `RQ-048`, `RQ-049`, `RQ-050`; `RQ-017`, `RQ-026`, `RQ-054`; derived `RD-001`, `RD-003`, `RD-004`, `RD-023`, `RD-024`, `RD-026`, `RD-032` |
| Downstream | Design review issue #36; `UT-0003` issue #42; `UT-0004` issue #43; `UT-0005` issue #44; `IMP-0005` issue #63; `IMP-0006` issue #64; `IMP-0007` issue #65; `IMP-0013` issue #71; `IT-0001` issue #53; `IT-0002` issue #54; `IT-0005` issue #57; `IT-0006` (UIA cancellation/timeout on a large legacy tree, `R-WIN-02`; no issue tracked yet in this bundle); `DES-0015` (capture shares `TargetReference`/DPI context); `DES-0017` (perf caps calibration); `DES-0018` (adapter provider wiring) |
| Evidence | Discovery/acquisition field detail conforming to the DES-0011-fixed port/DTO shapes, the adapter-internal `Win32TargetHandle` opaque-boundary mechanism, within-session ordering key, UIA apartment/threading model with COM-level timeout enforcement and cooperative cancellation, `RD-032` prohibited-pattern→COM-method audit mapping with a concrete read-only allow-list and spy, legacy acquisition edge table, virtualized-tree `NotRealized` handling, confidence rubric with a misclassification counter-example, rung-1 runtime-id detection rules, minimal-privilege policy, Mermaid class/sequence diagrams, contract-closure tables, edge cases, fixture strategy with counter-examples, `UT-0003`/`UT-0004`/`UT-0005` intents, integration assumptions |
| Verification | `tools/okf/Validate-Okf.ps1`; `git diff --check`; author-side `DRP-01`–`DRP-10` + DES-0007 §9 self-review (below), re-swept after the `R-ARC-01` boundary-reshaping fix; future `dotnet test tests/Surveyor.Adapters.Discovery.Tests --filter UT0003`, `dotnet test tests/Surveyor.Adapters.Uia.Tests --filter "UT0004|UT0005"` once source exists; live `IT-0001`/`IT-0002`/`IT-0005`/`IT-0006` on the manual Windows gate |
| Residual Risk | `RSK-RD-001` closes for UIA client selection but live legacy-edge coverage stays incremental (`DES-0015` capture edges + IT fixture app built out over time); rung-1 runtime-id pattern set is versioned and may need tuning against real frameworks; `WM_GETTEXT` is a documented read-only exception (query message, no state change) carried as a named risk with both reactive and proactive mitigation; cross-run `TargetReference.SessionTargetId` identity is within-session only by design; the read-only guarantee is defense-in-depth (structural + build-time + test-time) but not a cryptographic proof against a determined caller using unsafe interop against the raw COM objects the wrapper never exposes |

## Module Coverage

Primary modules ([DES-0002](des-0002-module-responsibility-basic-design.md)):

- **`M05` Target Discovery** — read-only enumeration of candidate windows/processes, `HWND` resolution, permission/integrity classification, within-session stable ordering. Home: `Surveyor.Adapters.Discovery` (adapter) implementing the application-owned `ITargetDiscoveryPort`.
- **`M06` UIA/MSAA Acquisition** — reads the target tree into the `DES-0009` model with confidence/availability markers, owns the strongest `RQ-048` read-only guarantee, populates `IdentitySource` (including rung-1 detection), and calls the `M09` `IFallbackKeyDerivation` seam when no non-sensitive identity exists. Home: `Surveyor.Adapters.Uia` implementing `IUiTreeAcquisitionPort`; the OS-independent invocation audit policy lives in portable `Surveyor.Adapters.Uia.Audit` so `UT-0005` remains in the fast unit lane.

Not covered here: capture/DPI (`M07` → `DES-0015`); scoring of the acquired model (`M08` → `DES-0010`); confidentiality masking/storage of extracted text (`M09`/`M12` → `DES-0013`); concrete DI wiring of these adapters (`M13` → `DES-0018`). The `IClock` and DTO/diagnostics shapes are consumed as fixed by `DES-0009`/`DES-0011`.

## Scope And Non-Goals

In scope:

1. Discovery DTO field detail not already fixed by `DES-0011` (`DiscoveryQuery`, `TargetProcessInfo`, `TargetIntegrityHint`), the adapter-internal `Win32TargetHandle` mechanism behind the `DES-0011`-fixed `TargetReference`, and the within-session ordering key (`RQ-051` scoped to live selection, not report determinism).
2. UIA client concretization on ADR-0002's raw COM, the thin Surveyor-owned wrapper boundary, and the MSAA / `WM_GETTEXT` fallback rules.
3. UIA apartment/threading model, cooperative cancellation, a COM-level per-call timeout budget, and the reentrancy/thread-abandon fallback (`R-WIN-01`, `R-WIN-02`, `R-WIN-03`, `RQ-050`).
4. The `RD-032` prohibited-pattern read-only audit: pattern→COM-method mapping, banned-API enforcement, and the acquisition spy (`RQ-048`, `UT-0005`).
5. Legacy acquisition edge table (MSAA proxy / owner-draw / MDI / windowless / `WM_GETTEXT`) with per-case confidence and `Unavailable` policy (`R-WIN-03`).
6. Virtualized/lazy-tree detection → `Unavailable(NotRealized)` / `PartialResult`, distinct from genuine absence (`R-GTA-02`).
7. The acquisition confidence rubric (how `M06` assigns `High`/`Medium`/`Low`).
8. Identity-source ladder rung-1 "runtime-generated id" detection rules (delegated here by `DES-0009`).
9. Minimal-privilege policy: same-integrity default; `uiAccess`/elevation as signed opt-in (`R-SEC-02`, ADR-0002).

Non-goals (owned elsewhere):

- Capture API, DPI normalization, occlusion/offscreen imaging → `DES-0015`.
- Domain key derivation, `IdentityMaterial` construction rules, fallback-key minimal contract → `DES-0009` (fixed; consumed here).
- Fallback-key masking/storage/export/log policy → `DES-0013`.
- Scoring/classification of acquired signals → `DES-0010`.
- Caps *default values* calibration and large-tree performance targets → `DES-0017` (this package fixes cap *semantics* and where caps apply).
- Concrete provider registration / lifetimes → `DES-0018`.

## Upstream Decisions (binding)

- **ADR-0002 §Decision**: UIA client is **raw COM (`Interop.UIAutomationClient` PIA)** behind a thin internal wrapper in `Surveyor.Adapters.Uia`; decisive reason is the 1:1 auditable mapping between the `RD-032` prohibited-pattern list and COM methods that are simply never referenced. Packaging is **unpackaged, same-integrity default**, with a signed classic-manifest `uiAccess` escape hatch. Capture is WGC-primary (owned by `DES-0015`).
- **DES-0003 `ITargetDiscoveryPort`**: read-only enumeration; per-candidate status is an `OperationStatus` value (`Ok`/`PermissionDenied`/`IntegrityMismatch`/`Unavailable`/`Timeout`); `TargetReference` is an opaque domain-safe handle (no `HWND`/process types inward, `RQ-054`); within-session stable order, not z-order.
- **DES-0003 `IUiTreeAcquisitionPort`**: strongest `RQ-048` owner; only read patterns; state-changing patterns (`RD-032`) absent from the surface; per-node `Availability.Unavailable(reason)`, run-level `PermissionDenied`/`PartialResult`/`Timeout` via `OperationStatus`; `Name`/text as `DisplayLabel` only, never in keys, handed downstream via `M09`.
- **DES-0004 Stages 1/2**: run does not start until a `TargetReference` resolves; per-node unavailable and `PartialResult`/`Timeout` are recorded, never turned into scores.
- **DES-0009**: the identity-source ladder (rung 1 AutomationId → rung 2 framework-stable id → rung 3 `M09` fallback token → rung 4 structural ordinal) is fixed, including the `ElementIdentity`/`IdentityMaterial` union (`StableIdentity` | `FallbackKeyToken`, no third constructor); **rung-1 runtime-generated-id detection is explicitly delegated to this package**; `Availability`/`AcquisitionConfidence` are distinct from low scores; `IdentityMaterial.StableIdentity` trusts its caller to pass non-sensitive text — **this package is that caller and owns the classification**.
- **DES-0011 (binding, fixed there — not redefined here)**: `ITargetDiscoveryPort.ListTargetsAsync(DiscoveryQuery, ct) -> TargetDiscoveryResult` and `.ResolveAsync(TargetReference, ct) -> TargetResolveResult`; `IUiTreeAcquisitionPort.AcquireAsync(TargetReference, AcquisitionOptions, ct) -> AcquisitionResult`; the `TargetReference`, `TargetCandidate`, `TargetDiscoveryResult`, `TargetResolveResult`, and `AcquisitionResult` field sets; the `OperationStatus` and `RunDiagnostic` shapes; the `IStageTimeoutController` race rule (caller cancellation wins over stage timeout) and `IClock` threading. This package's field detail (`DiscoveryQuery`, `TargetProcessInfo`, `TargetIntegrityHint`, `AcquisitionOptions`) and adapter-internal mechanics fit inside those fixed shapes without renaming a method or adding/removing a DTO field.

## Data And Contract Design

`ITargetDiscoveryPort`/`IUiTreeAcquisitionPort` method signatures and the `TargetReference`/`TargetCandidate`/`TargetDiscoveryResult`/`TargetResolveResult`/`AcquisitionResult`/`OperationStatus` shapes are fixed by `DES-0011` and are **consumed as-is below, not redefined**. This package fixes only the field detail `DES-0011` left open (`DiscoveryQuery`, `TargetProcessInfo`, `TargetIntegrityHint`, `AcquisitionOptions`) and the adapter-internal mechanics behind those fixed shapes.

### Discovery

- `DiscoveryQuery` (field detail not fixed elsewhere) — `DiscoveryScope Scope` (`TopLevelWindows` | `ProcessScoped`), `string? ProcessNameFilter` (non-sensitive, ordinal match), `bool IncludeInvisible = false`. Caller input only.
- `TargetReference` (`DES-0011`, consumed as-is) — `string SessionTargetId`, `TargetKind Kind` (`ProcessWindow` | `TopLevelWindow` | `Fixture`), `string? SafeDisplayHint`, `TargetIntegrityHint IntegrityHint`. `TargetIntegrityHint` (field detail fixed here, enum) — `SameOrLower`, `HigherRequiresElevation`, `Unknown`.
- `TargetProcessInfo` (field detail not fixed elsewhere, referenced by `TargetCandidate.Process`) — `string ProcessImageName` (file name only, non-sensitive), `int ProcessId` (diagnostics-lane only, never key material).
- `TargetCandidate` (`DES-0011`, consumed as-is) — `TargetReference Reference`, `string SafeName`, `TargetProcessInfo Process`, `bool IsLikelyLegacyGui`, `OperationStatus Status`, `IReadOnlyList<RunDiagnostic> Diagnostics`. Single writer: the discovery adapter. `Status` is the `DES-0011` `OperationStatus` value directly (`Ok`, `Unavailable`, `PermissionDenied`, `IntegrityMismatch`, `Timeout`); this package does not define a parallel discovery-only status enum.

**Adapter-internal handle (never crosses the port, closes `R-ARC-02`).** Inside `Surveyor.Adapters.Discovery`, each live candidate is tracked by an `internal`-only `Win32TargetHandle`: `nint WindowHandle`, `int ProcessId`, `string ProcessImageName`, `string WindowClass`, `int WithinSessionOrdinal`. `Win32TargetHandle` is never returned by, or accepted as a parameter of, `ITargetDiscoveryPort` or `IUiTreeAcquisitionPort` — it is the *only* place a raw `HWND` lives. The adapter keeps a process-local registry (`Dictionary<string, Win32TargetHandle>`) keyed by an adapter-minted opaque token, and `TargetReference.SessionTargetId` **is** that token (e.g. `"tgt-" + <session-scoped ulong counter>` — no title, path, or `HWND` bits encoded). `ResolveAsync`/`AcquireAsync` look the token up in this registry to reach the live `HWND`; there is no reverse mapping exposed inward, and no inward-facing method accepts or returns a `Win32TargetHandle`. This is the concrete, type-level mechanism enforcing `RQ-054` opacity — a compile error, not just a review convention, would result from any attempt to thread `Win32TargetHandle` across the port.

**Within-session ordering key** (`RQ-051`, live-selection scope): before returning `TargetDiscoveryResult.Candidates` (which `DES-0011` requires the producer to pre-sort), the discovery adapter orders candidates by the ordinal tuple `(ProcessImageName, WindowClass, WithinSessionOrdinal)`, where `WithinSessionOrdinal` is the ascending index in the deterministic top-level enumeration of that (process, class) group. Raw `HWND` and z-order are never ordering inputs, and `WithinSessionOrdinal` itself never leaves the adapter (`Win32TargetHandle`-only). This ordering is *not* report/`ScreenKey` material — report determinism is owned by `M04` keys (`DES-0009`).

### Acquisition

`IUiTreeAcquisitionPort.AcquireAsync(TargetReference, AcquisitionOptions, ct) -> AcquisitionResult` and `AcquisitionResult`'s field set (`Status`, `ScreenModel`, `ElementCount`, `HitElementCap`, `Availability`, `Diagnostics`) are fixed by `DES-0011` and consumed as-is; this package does not introduce a parallel run-status enum or a `PartialReason` DTO field.

- `AcquisitionOptions` (field detail not fixed elsewhere) — `int MaxElementCount` (the same cap `AnalysisRunOptions.MaxElementCount` threads through per `DES-0011`; default 20000 per `DES-0011`, not redefined here), `TimeSpan PerNodeReadBudget` (adapter-level fine-grained budget enforced *within* the `IStageTimeoutController`-owned `TreeAcquisition` stage timeout; cap *semantics* fixed here, default *value* → `DES-0017`). The overall per-run timeout for the `TreeAcquisition` stage is owned by `AnalyzeScreenUseCase` calling `IStageTimeoutController.RunAsync` (`DES-0011`) with `AnalysisRunOptions.AcquisitionTimeout`; this package does not add a second, competing run-level timeout field.
- `AcquisitionResult.Status` uses `OperationStatus` directly. A `PartialResult` status's specific reason (`CapReached` / `VirtualizedSubtree` / `NodeErrors`) is carried as a `RunDiagnostic.Code` (e.g. `Acquisition.Partial.CapReached`) plus `SafeArgs`, per the `DES-0011` diagnostics model, not as a new DTO field. `HitElementCap = true` together with a `PartialResult` status is the cap-reached case; `ElementCount` is the count of nodes actually read.
- `AcquisitionResult.Availability` (field detail fixed here — `DES-0011` fixes the field's presence and type, `IReadOnlyList<Availability>`, but not its population rule) is a **run-level rollup, not a mirror of the tree**: the distinct `Unavailable(reason)` values encountered among this run's nodes, each appearing at most once, ordered by first-encountered node in fixed traversal order. `Available` is not included (it is the non-notable default, already implicit in `ElementCount`). This list exists so a consumer (e.g. `DES-0012`'s report writer, or a diagnostics dashboard) can learn *which* unavailability categories occurred in a run without walking `ScreenModel`; it is a derived summary populated once, at run completion, from the same node reads that write `UiElement.Availability` inside `ScreenModel` — `UiElement.Availability` remains the single per-node source of truth, and no consumer may treat the rollup as authoritative for, or use it to answer, any single node's state.
- Per-node `Availability` and `AcquisitionConfidence` are the `DES-0009` types; this package fixes **how their values are assigned** (rubric and legacy table below).
- `AcquisitionProvenance` (enum, recorded per node only in `RunDiagnostic.SafeArgs`, never as a `UiElement` field or key material) — `UiaNative`, `MsaaProxy`, `WmGetText`, `Synthesized`. Used by the confidence rubric and honest edge marking; `DES-0009`'s `UiElement` has no provenance field, so provenance never becomes domain/model state.

### Identity population (rung selection)

`M06` selects the ladder rung per element in fixed order and hands the result to `DES-0009`'s fixed `ElementIdentity`/`IdentityMaterial` types:

1. **Rung 1 — AutomationId**, only if it passes the runtime-id detection below; source `AutomationId`; `IdentityMaterial.StableIdentity(automationId)`.
2. **Rung 2 — framework-stable id** (Win32 control id / `FrameworkStableId`) when present and stable; `IdentityMaterial.StableIdentity(controlId)`.
3. **Rung 3 — `M09` fallback token** of the normalized `Name`/title via `IFallbackKeyDerivation.DeriveFallbackToken(scope, rawName)`; `IdentitySource.FallbackHash`, marks `IsFallback`. Raw `Name` never touches the domain directly (`RQ-052`).
4. **Rung 4 — structural ordinal** among same-`ControlKind` siblings in fixed traversal order; recorded as `ElementIdentity.SiblingOrdinal` with `IdentitySource.StructuralOrdinal`, per the `DES-0009`-fixed shape — this package does not add a third `IdentityMaterial` constructor beyond the fixed `StableIdentity`/`FallbackKeyToken` union; never fallback (no sensitive input).

**Rung-1 runtime-generated-id detection (owned here, versioned `runtime-id-rules v=1`).** An AutomationId is treated as runtime-generated (and rung 1 is skipped, falling through to rung 2+) if it matches any deny rule, evaluated with `StringComparison.Ordinal`:

- GUID-shaped: matches `^\{?[0-9a-fA-F]{8}-...\}?$` (with/without braces).
- Pure ephemeral integer: all-digits **and** length ≥ 6 (heuristic for HWND/handle-like ids); short numeric control ids remain rung 2 framework-stable, not rung 1.
- **Embedded long digit run** (`R-SEC-04`): contains any substring of ≥ 8 consecutive digits *anywhere* in the value (not only whole-string matches), e.g. `customer_48213099` or `row-2024011500391`. This is broader than the whole-string integer rule above so an AutomationId that embeds an account/customer/order id alongside a stable prefix is still excluded from rung 1, reducing (not eliminating — see Residual Risks) the chance sensitive-shaped content reaches `IdentityMaterial.StableIdentity` unflagged.
- Known auto-id prefixes: ordinal-prefixed by a versioned set (e.g. framework auto names). The set is externalized alongside the `DES-0010` config-version discipline so a change bumps `runtime-id-rules` version and is recorded in the report.

Rationale: a too-permissive rung-1 rule silently destabilizes keys across runs (`RQ-051`/`RQ-053`); a too-strict rule over-uses fallback and degrades comparability (`RD-021`). The deny-list is conservative and testable (counter-example fixtures below). The embedded-digit-run addition trades a small amount of extra rung-2/3 fallthrough for materially lower risk of a customer/account identifier landing in a stable key (`RQ-052`); it is a syntactic heuristic, not content classification, so it is carried as a residual risk, not a closed guarantee.

## Contract Closure

### Port-method I/O derivation

| Method | Input → source | Output → consumer |
| -- | -- | -- |
| `ITargetDiscoveryPort.ListTargetsAsync(DiscoveryQuery, ct)` | `DiscoveryQuery` = caller (`SelectTargetUseCase`) input; window/process facts = outward OS enumeration | `TargetDiscoveryResult.Candidates` → `SelectTargetUseCase` for user choice; each candidate's `TargetReference` → `ResolveAsync` |
| `ITargetDiscoveryPort.ResolveAsync(TargetReference, ct)` | `TargetReference` = a candidate chosen by the caller; `SessionTargetId` resolves via the adapter's internal `Win32TargetHandle` registry | `TargetResolveResult.Target` (+ status) → `AnalyzeScreenUseCase` as Stage-2 input |
| `IUiTreeAcquisitionPort.AcquireAsync(TargetReference, AcquisitionOptions, ct)` | `TargetReference` = Stage-1 output; `AcquisitionOptions` caps = caller input; tree facts = outward UIA/MSAA reads; fallback token = `IFallbackKeyDerivation` (`M09`) | `AcquisitionResult` → `AnalyzeScreenUseCase` → `M08` scoring (`ScreenModel`) and `DES-0011` diagnostics (status/provenance via `SafeArgs`) |

Every input is derivable from caller input, a prior-stage output, or an outward read through a defined contract; every output has a named inward consumer. No inward method needs a value it cannot obtain (guards `DRP-03`).

### DTO field ownership

| Field | Single writer | Write timing | Sync / fabrication rule |
| -- | -- | -- | -- |
| `TargetCandidate.Status` | discovery adapter | during enumeration | consumers never fabricate a status; `OperationStatus.Unavailable`/other non-`Ok` values are explicit, never silently upgraded |
| `TargetReference.SessionTargetId` | discovery adapter | at candidate creation | opaque adapter-minted token; the `Win32TargetHandle` it maps to (incl. `WindowHandle`) never crosses the port, is never persisted, never key material |
| `Win32TargetHandle.WithinSessionOrdinal` | discovery adapter | at candidate creation | adapter-internal ordering only; never inward, never report/`ScreenKey` input |
| `UiElement.Availability` / `.Confidence` | acquisition adapter | during node read | per rubric/legacy table; `Unavailable` never rewritten to a score by consumers |
| `AcquisitionResult.Status` / `.HitElementCap` | acquisition adapter | at run completion | recorded, never converted to a score (`DES-0004` Stage 2) |
| `AcquisitionResult.Availability` | acquisition adapter | at run completion, derived from the same node reads as `UiElement.Availability` | deduplicated `Unavailable(reason)` rollup only; never a second source of truth for a specific node's state, never fabricated independently of the tree |
| `IdentitySource` / `IdentityMaterial` | acquisition adapter (via ladder) | at node model construction | domain keys become final here; downstream never re-derives keys |
| `AcquisitionProvenance` | acquisition adapter | per node | `RunDiagnostic.SafeArgs` only; never a `UiElement` field, never key material |

### Round-trip inventory

- **Fixture tree ⇄ model**: the UT fixture serializer and `IUiTreeAcquisitionPort` fake read the same `.tree` schema into the same `ScreenModel`/`UiElement` types the live adapter produces — symmetric types, so a fixture-passing test exercises the real model shape (shared with `DES-0009` `IMP-0001` reader).
- **`TargetReference` opaque projection**: outward (adapter holds `Win32TargetHandle`, including `HWND`/pid) vs inward (`TargetReference.SessionTargetId` opaque token plus optional `SafeDisplayHint`) are two distinct types, not one type with a visibility split — there is no field on `TargetReference` a bug could accidentally populate with `WindowHandle`. There is no inward→`Win32TargetHandle` direction (asymmetry is intentional and enforced by type separation, not an omission).
- No persistence round-trip is introduced here (store/export symmetry is `DES-0013`).

## UIA Threading And Apartment Model (`R-WIN-01`, `R-WIN-02`, `R-WIN-03`, `RQ-050`)

- **Apartment**: the raw-COM UIA client runs on a dedicated Surveyor-owned **MTA** acquisition thread, never on the WinUI UI/dispatcher thread. The thin wrapper marshals results to the caller as plain domain values (no COM objects cross the wrapper boundary), so `RQ-054` holds and UI responsiveness is preserved.
- **Node-boundary cooperative cancellation**: UIA COM reads are blocking, so cancellation is checked at each node boundary and before each subtree descent (`ct.ThrowIfCancellationRequested()` between reads). An external cancel surfaces as `OperationCanceledException` from `AcquireAsync`; per `DES-0011`'s `IStageTimeoutController` race rule, caller cancellation wins any race with the stage timeout and the outcome is `CancelledByCaller`, never an `AcquisitionResult` status.
- **COM-level call budget, not cooperative-only (`R-WIN-01`)**: node-boundary checks alone cannot interrupt a *single* blocking COM call against a hung target — there is no yield point inside `GetCurrentPropertyValue`/pattern-getter calls for `ct.ThrowIfCancellationRequested()` to run. `RawComUiaClient` therefore sets `ConnectionTimeout`/`TransactionTimeout` on construction to a value derived from `AcquisitionOptions.PerNodeReadBudget`, so an individual COM call that would hang instead fails with a COM timeout `HRESULT`, which the wrapper maps to that node's `Unavailable(Timeout)` (see legacy edge table) rather than blocking indefinitely. These two properties live on **`IUIAutomation6`** (Windows 10 1809+), not the base `IUIAutomation` interface the wrapper otherwise programs against — `RawComUiaClient` obtains it via `QueryInterface`/an `as`-style COM interface cast on the `CUIAutomation8` instance at construction, and this is a named `IMP-0013` prerequisite (targeting an older Windows 10 build without `IUIAutomation6` degrades this mechanism to cooperative-only cancellation for that machine, itself a residual-risk-worthy fact, not assumed away). `PerNodeReadBudget` is enforced by this COM-level mechanism, not only by a stopwatch around cooperative checks.
- **Timeout vs cancellation precedence**: the `TreeAcquisition` stage runs through `IStageTimeoutController.RunAsync(RunStage.TreeAcquisition, AnalysisRunOptions.AcquisitionTimeout, AcquireAsync, callerToken)` per `DES-0011`; that controller's race rule (caller cancellation wins) is consumed as-is, not redefined here. `MaxElementCount` is a separate, expected bound → cap reached produces `OperationStatus.PartialResult` with `HitElementCap = true` (a result, recorded). `PerNodeReadBudget` exhaustion (via the COM-level timeout above) marks that node `Unavailable(Timeout)` and the run continues (partial), rather than aborting.
- **No reentrancy, with a documented abandon fallback (`R-WIN-03`)**: one acquisition per `TargetReference` at a time; the wrapper owns the COM lifetime and disposes it deterministically on completion, cancel, or the `TreeAcquisition` stage timeout (`AnalysisRunOptions.AcquisitionTimeout` via `IStageTimeoutController`). If the underlying blocking call does not return even after its COM-level timeout fires (a genuinely wedged target/driver, not merely a slow one), the acquisition thread is **abandoned, not joined or `Thread.Abort`-ed** (`Thread.Abort` is unsupported on modern .NET and COM apartment state makes forced termination unsafe): the run is completed with `OperationStatus.Timeout`/`PartialResult` from the caller's perspective, the abandoned thread's COM objects are left to finalize, and the *next* acquisition on that `TargetReference` uses a fresh MTA thread and a fresh `RawComUiaClient` rather than reusing the wedged one. This is a named residual risk (see below), not a solved problem — `IT-0006` exercises cancellation/timeout response time against a large legacy tree to keep it honest.

## Read-Only Audit (`RQ-048`, `RD-032`, `UT-0005`)

Enforcement is layered so no single layer is a single point of failure: **type-surface absence** (the port exposes no mutation), a **structural runtime guarantee** (no mutation-capable object ever reaches calling code), a **build-time banned-symbol guard**, and a **test-time call-audit spy**.

### Structural runtime guarantee (`R-SEC-01`)

The build/test-time layers below prove the wrapper's *own* code never references a mutating COM member, but a static banned-symbol list cannot by itself stop reflection- or `dynamic`-based invocation of a pattern object that *did* leak out of the wrapper. `RawComUiaClient` closes that gap structurally, not just procedurally: `IUIAutomationElement.GetCurrentPattern`/`GetCachedPattern` results are read for their property values **inside** `RawComUiaClient` and are never returned to `UiaTreeAcquisitionAdapter` or any caller — the wrapper's `IUiaComReader` seam returns only plain data (`UiaNodeSnapshot`: `ControlKind`, `BoundingRect?`, `string? AutomationId`, `bool[] SupportedPatternFlags`, etc.), never a COM interface pointer or `object`. Because no COM pattern object crosses the wrapper boundary at runtime, there is no reachable value on which a caller could invoke a banned member via reflection or `dynamic`, regardless of build-time guard coverage. The banned-symbol list below is additionally extended to ban `dynamic` typing and `Type.InvokeMember`/late-bound COM invocation anywhere in `Surveyor.Adapters.Uia`, closing the remaining path of the wrapper's *own* code reaching a banned member indirectly.

### Prohibited pattern → COM method map (never referenced by the wrapper)

| `RD-032` prohibited pattern | UIA COM interface / method that must be absent |
| -- | -- |
| Invoke | `IUIAutomationInvokePattern.Invoke` |
| SetValue | `IUIAutomationValuePattern.SetValue`, `IUIAutomationRangeValuePattern.SetValue` |
| Select | `IUIAutomationSelectionItemPattern.Select` / `AddToSelection` / `RemoveFromSelection` |
| Toggle | `IUIAutomationTogglePattern.Toggle` |
| Expand/Collapse | `IUIAutomationExpandCollapsePattern.Expand` / `Collapse` |
| Scroll | `IUIAutomationScrollPattern.Scroll` / `SetScrollPercent`, `ScrollItemPattern.ScrollIntoView` |
| Dock | `IUIAutomationDockPattern.SetDockPosition` |
| Transform | `IUIAutomationTransformPattern.Move` / `Resize` / `Rotate` |
| Text edit | `IUIAutomationTextEditPattern` / `TextPattern` range set operations |
| Window/focus | `SetFocus`, `IUIAutomationWindowPattern.Close` / `SetWindowVisualState` |

The wrapper references **only** read interfaces (`IUIAutomationElement` property reads, `TreeWalker`, `CacheRequest`, read-only pattern getters). ADR-0002 chose raw COM precisely so this is a 1:1 reviewable absence.

### Concrete read-only allow-list (`R-TEST-04`)

`UT-0005`'s spy asserts the invoked-member set is a **subset of this explicit allow-list** (a positive list, not a deny-list — any member not named here fails the assertion even if it is not in the prohibited-pattern table above):

- `IUIAutomationElement`: `GetCurrentPropertyValue`, `GetCachedPropertyValue`, `GetCurrentPattern`, `GetCachedPattern`, `FindAll`/`FindFirst` (read-only search), `BuildUpdatedCache`.
- `IUIAutomationTreeWalker`: `GetFirstChildElement`, `GetNextSiblingElement`, `GetParentElement`, `Normalize` (all navigation-only).
- `IUIAutomationCacheRequest`: `AddProperty`, `AddPattern`, `Push`/property setters that configure *what to cache*, not target state.
- Read-only pattern getters, invoked but never their write counterparts: `IUIAutomationValuePattern.CurrentValue`, `IUIAutomationRangeValuePattern.CurrentValue`/`CurrentMinimum`/`CurrentMaximum`, `IUIAutomationTogglePattern.CurrentToggleState`, `IUIAutomationSelectionItemPattern.CurrentIsSelected`, `IUIAutomationExpandCollapsePattern.CurrentExpandCollapseState`, `IUIAutomationTextPattern.DocumentRange`/`GetVisibleRanges`/`ITextRangeProvider.GetText` (read range text only, never `SetValue`/`Select`/`ScrollIntoView` write operations on the range).
- Win32 legacy fallback: `SendMessageTimeout(..., WM_GETTEXT, ...)` only (see below) — never `SendMessage`/`PostMessage` for any other message, and never `WM_SETTEXT`.

A member invoked during the spied acquisition that is not in this list fails the test, whether or not it happens to also be read-only in practice — the allow-list is closed, not "closed except for anything not proven dangerous."

### Enforcement layers

1. **Banned-API architecture test** (`DES-0008` guard, extended here): the prohibited COM member set (table above) plus `dynamic`/`Type.InvokeMember` usage in `Surveyor.Adapters.Uia` is a banned-symbol list; referencing any is a build failure — a static, always-on guard.
2. **Structural runtime guarantee** (above, `R-SEC-01`): no COM pattern object ever leaves `RawComUiaClient`, so there is no reachable value for a caller to invoke reflectively even if the build-time guard were bypassed.
3. **Acquisition read-only spy** (`UT-0005`, `IMP-0007`): a spy `IUiaComReader` implementation records every method invoked during a full acquisition over a fixture; the test asserts the invoked-set is a subset of the concrete allow-list above and fails if any state-changing method (or `WM_SETTEXT`/input send) appears. Counter-example: a deliberately mutating fake provider must turn the spy test red.
4. **`WM_GETTEXT` exception, explicitly bounded**: text retrieval for controls lacking a UIA `Name` uses `SendMessageTimeout(hwnd, WM_GETTEXT, ..., SMTO_ABORTIFHUNG | SMTO_BLOCK, PerNodeReadBudget.Milliseconds, out _)`, never a bare `SendMessage`, so a non-responding window fails fast into `Unavailable(Timeout)` instead of blocking the acquisition thread indefinitely (closes `R-WIN-02`). `WM_GETTEXT` is a query message that does not change target state; it is allow-listed as read-only with a written justification, downgrades confidence (below), and is recorded as `AcquisitionProvenance.WmGetText`. `WM_SETTEXT` and any input-injection message remain banned. A proactive, versioned deny-list of window classes known to override `WM_GETTEXT` with non-idempotent handlers (populated as such classes are identified, e.g. via the IT fixture app) skips the attempt entirely for those classes rather than only reacting after the fact (`R-SEC-03`); this is additive to, not a replacement for, the existing reactive downgrade-to-`Unavailable(NotExposed)` rule when a control is later found to have side effects.

## Legacy Acquisition Edge Table (`R-WIN-03`)

| Edge | Detection | Technique | Provenance | Confidence | Availability policy |
| -- | -- | -- | -- | -- | -- |
| MSAA-only proxy (no native UIA) | UIA element backed by MSAA proxy (`IAccessible`) | read MSAA role/name via UIA-over-MSAA bridge | `MsaaProxy` | ≤ `Medium` | `Available` with degraded confidence; missing role → `Unavailable(NotExposed)` |
| Owner-draw control (no exposed text) | UIA `Name` empty and control is custom-drawn | attempt `SendMessageTimeout(WM_GETTEXT)`; else structural only | `WmGetText` or `Synthesized` | `Low` | `Available(Low)` if any text; else `Unavailable(NotExposed)` |
| MDI child windows | MDI client/child window class | enumerate child frames as nested screens/subtrees | `UiaNative`/`MsaaProxy` | per-node | children mapped normally; unreachable child → `Unavailable(NotExposed)` |
| Windowless / ActiveX controls | no `HWND`, MSAA-only object | read via MSAA `IAccessible` only | `MsaaProxy` | ≤ `Medium` | `Available` degraded; no accessible interface → `Unavailable(NotExposed)` |
| `WM_GETTEXT`-only text | UIA `Name` absent but `WM_GETTEXT` returns text | read-only `SendMessageTimeout(WM_GETTEXT, SMTO_ABORTIFHUNG)`, skipped up front for classes on the proactive deny-list (`R-SEC-03`) | `WmGetText` | `Low` | `Available(Low)`; timeout/abort on the message → `Unavailable(Timeout)` |

All edges: extracted text is `DisplayLabel` only, never key material; when no non-sensitive identity exists the rung-3 fallback token is used for the key.

## Virtualized / Lazy-Tree Handling (`R-GTA-02`)

- A subtree that a provider advertises but has not realized (virtualized list/grid items, `ItemContainerPattern`/`VirtualizedItem`, lazy tree nodes) is marked `Unavailable(NotRealized)` on the placeholder node and contributes `OperationStatus.PartialResult` with `RunDiagnostic.Code = Acquisition.Partial.VirtualizedSubtree` to the run result.
- `NotRealized` is **distinct from `NotExposed`** (genuine absence): the model records that content exists but was not materialized read-only, so scoring/reporting never conflate "hidden by virtualization" with "not present." Surveyor does **not** scroll or expand to force realization (that would violate `RQ-048`).

## Acquisition Confidence Rubric

`AcquisitionConfidence` is assigned per node from identity strength × property completeness × provenance (ordinal decision list, first match wins):

1. `High` — rung-1/2 stable identity **and** `UiaNative` provenance **and** required properties (`ControlType`, bounds, `Name`-or-empty) present.
2. `Medium` — rung-1/2 identity with `MsaaProxy` provenance, **or** `UiaNative` with partial properties (e.g. missing bounds but present role).
3. `Low` — rung-3 fallback identity, **or** `WmGetText`/`Synthesized` provenance, **or** owner-draw text.
4. Confidence is orthogonal to `Availability`: an `Unavailable(reason)` node carries no confidence claim; a `Low`-confidence node is still `Available`. Confidence never becomes a score (that mapping is `DES-0010`).
5. **Ordering is strict, first-match-wins, and evaluated top-down** (`R-TEST-03`): a node with rung-1/2 identity and `UiaNative` provenance but *missing* a required property (e.g. no bounds) must fall out of rule 1 into rule 2 (`Medium`), never stay `High` on identity/provenance alone. The counter-example fixture in Fixture Strategy below exists specifically to catch an implementation that checks identity+provenance and forgets the property-completeness conjunction in rule 1.

## Minimal-Privilege Policy (`R-SEC-02`, ADR-0002)

- Default: run **same integrity** as the analyzer session, unpackaged, no `uiAccess`, no elevation, no prompts. Discovery reports `IntegrityMismatch`/`HigherRequiresElevation` for targets it cannot read at this privilege instead of silently escalating.
- Opt-in: a signed classic-manifest build may enable `uiAccess`/elevation **only** when a specific target requires it; this is an explicit user action, never automatic. MSIX is incompatible with `uiAccess` (ADR-0002), so the opt-in path is the unpackaged signed build.
- No privileged operation is a mutation: elevation only widens *read* reach; the `RD-032` read-only audit still applies unchanged.

## Class Design (UML)

```mermaid
classDiagram
    class ITargetDiscoveryPort {
      <<application port, DES-0011>>
      +ListTargetsAsync(DiscoveryQuery, ct) TargetDiscoveryResult
      +ResolveAsync(TargetReference, ct) TargetResolveResult
    }
    class IUiTreeAcquisitionPort {
      <<application port, DES-0011>>
      +AcquireAsync(TargetReference, AcquisitionOptions, ct) AcquisitionResult
    }
    class Win32WindowDiscoveryAdapter {
      <<Surveyor.Adapters.Discovery / M05>>
      -EnumerateTopLevel()
      -ClassifyIntegrity(handle) TargetIntegrityHint
      -Register(handle) TargetReference
    }
    class Win32TargetHandle {
      <<internal, never crosses the port>>
      +nint WindowHandle
      +int ProcessId
      +string ProcessImageName
      +string WindowClass
      +int WithinSessionOrdinal
    }
    class UiaTreeAcquisitionAdapter {
      <<Surveyor.Adapters.Uia / M06>>
      -RunOnMtaThread(work)
      -ReadNode(elt) UiElement
      -SelectRung(elt) ElementIdentity
      -AssignConfidence(elt, provenance)
    }
    class IUiaComReader {
      <<internal seam interface, UT-0004/UT-0005 fake boundary>>
      +ReadProperties(elt) UiaNodeSnapshot
      +Walk(elt, cache) IReadOnlyList~UiaNodeHandle~
      +TryGetLegacyText(hwnd, budget) string?
    }
    class RawComUiaClient {
      <<internal thin wrapper, implements IUiaComReader>>
      note "references read-only COM members only (RD-032 absent); sets ConnectionTimeout/TransactionTimeout"
    }
    class IFallbackKeyDerivation {
      <<application port / M09>>
      +DeriveFallbackToken(scope, rawText) IdentityMaterial
    }
    ITargetDiscoveryPort <|.. Win32WindowDiscoveryAdapter
    Win32WindowDiscoveryAdapter --> Win32TargetHandle
    IUiTreeAcquisitionPort <|.. UiaTreeAcquisitionAdapter
    IUiaComReader <|.. RawComUiaClient
    UiaTreeAcquisitionAdapter --> IUiaComReader
    UiaTreeAcquisitionAdapter ..> IFallbackKeyDerivation : rung-3 only
```

`UiaTreeAcquisitionAdapter` depends on the `IUiaComReader` seam, not directly on `RawComUiaClient`; `IMP-0013` wires the real implementation, while `UT-0004`/`UT-0005` substitute a fake/spy `IUiaComReader` so rung-selection, confidence-assignment, and read-only-audit logic in `UiaTreeAcquisitionAdapter` itself is exercised by the tests, not bypassed by them (closes `R-TEST-02`).

```mermaid
sequenceDiagram
    participant UC as AnalyzeScreenUseCase
    participant STC as IStageTimeoutController
    participant ACQ as UiaTreeAcquisitionAdapter (MTA)
    participant COM as IUiaComReader
    participant M09 as IFallbackKeyDerivation
    UC->>STC: RunAsync(TreeAcquisition, AcquisitionTimeout, AcquireAsync, callerToken)
    STC->>ACQ: AcquireAsync(TargetReference, options, ct)
    ACQ->>ACQ: dispatch to MTA acquisition thread
    loop per node (fixed traversal order)
        ACQ->>ACQ: ct.ThrowIfCancellationRequested()
        ACQ->>COM: read properties/patterns (COM-level ConnectionTimeout/TransactionTimeout bounds the call itself)
        COM-->>ACQ: role, name?, bounds?, patterns
        alt no non-sensitive identity
            ACQ->>M09: DeriveFallbackToken("elm", rawName)
            M09-->>ACQ: IdentityMaterial (IsFallback)
        end
        ACQ->>ACQ: assign Availability + Confidence + provenance (SafeArgs only)
    end
    ACQ-->>STC: AcquisitionResult(ScreenModel, Status, ElementCount, HitElementCap, Diagnostics)
    STC-->>UC: StageCallResult(Value, TimedOut, CancelledByCaller, Diagnostics) per DES-0011 race rule
```

## Edge Cases

| Case | Behavior |
| -- | -- |
| Target closed mid-acquisition | disposed-target COM fault → `OperationStatus.PartialResult` with `RunDiagnostic.Code = Acquisition.Partial.NodeErrors` and a sanitized diagnostic; no throw of raw COM fault to the caller |
| Permission denied on subtree | node `Unavailable(PermissionDenied)`; run continues; run-level `OperationStatus.PermissionDenied` if the root is unreadable |
| Element count cap reached | stop descent, `OperationStatus.PartialResult` with `HitElementCap = true` and `RunDiagnostic.Code = Acquisition.Partial.CapReached`; already-read nodes returned in stable order |
| Per-run timeout | `IStageTimeoutController` reports `TimedOut` (caller token not canceled); `AnalyzeScreenUseCase` maps the `TreeAcquisition` stage to `OperationStatus.Timeout`; partial tree returned if one was assembled before the budget fired |
| External cancel | `IStageTimeoutController` reports `CancelledByCaller` (wins any race with a concurrent timeout, per `DES-0011`); `OperationCanceledException` surfaces from `AcquireAsync` itself (not an `AcquisitionResult` status); no partial artifact promised beyond what the caller already holds |
| Duplicate sibling AutomationId | keys stay unique via `DES-0009` collision rule; also a `DES-0010` testability finding (not resolved here) |
| Empty/whitespace `Name` | not sensitive; rung falls through; no fallback token derived for empty text |
| Offscreen element | `Availability` may be `Available` with `Offscreen`-derived bounds handling deferred to `DES-0015`; existence is still modeled |
| Runtime-generated AutomationId | rung-1 skipped per detection rules; rung-2+ used; provenance/confidence reflect the downgrade |

## Diagnostics And Logging

This package **emits** run-level diagnostics as `DES-0011`-owned `RunDiagnostic` entries (`Code`, `Stage = TreeAcquisition`/`TargetDiscovery`, `Severity`, `Status`, `ScreenKey?`/`ElementKey?`, `MessageTemplateId`, `SafeArgs`): counts of nodes by provenance/confidence, cap/timeout hits, per-subtree `Unavailable` reasons, and the `runtime-id-rules` version applied. **Sanitizer call-site (`R-SEC-02`)**: `Win32WindowDiscoveryAdapter`/`UiaTreeAcquisitionAdapter` never call `ISensitiveValueSanitizer`/`DES-0013`'s sanitizer directly and never need to — every `RunDiagnostic.SafeArgs` value this package writes is already restricted, by construction, to the `DES-0011`-fixed allow-listed shape (enum names, counts, durations in milliseconds, config/`runtime-id-rules` versions, and stable keys already non-reversible); raw window title, `Name`, or path are never assigned to a diagnostic field in the first place, so there is nothing for a sanitizer to intercept at this layer. Sanitizing the *emitted* `DisplayLabel`/report content for the end user remains `DES-0013`'s Stage-5 job, downstream of this package; `UT-0004`/`UT-0005` assert that every `SafeArgs` value produced by this adapter matches the allow-listed shape (regex/enum-membership check), so an accidental raw-text diagnostic fails a test rather than only a code review. COM `HRESULT`s map to modeled statuses; raw fault text is never surfaced to logs or exceptions crossing inward.

## Fixture Strategy

- **UT (synthetic, deterministic)** under `tests/fixtures/uia-trees/` (schema shared with `DES-0009` `IMP-0001`): trees seeded with each `IdentitySource` rung, `Unavailable` reasons, virtualized placeholders, MSAA-proxy/owner-draw/`WM_GETTEXT` provenance markers, and duplicate ids.
- **Fake `IUiaComReader`** (`R-TEST-01`/`R-TEST-02`): a test double implementing the seam interface with a configurable per-call delay and an explicit cancellation-injection point, so `UT-0004`/a dedicated cancel-vs-timeout test can deterministically drive "cancel fires mid-node-read," "COM-level timeout fires mid-node-read," and "cap reached before either" without a real Windows target or real-time sleeps — mirroring the `IStageTimeoutController` fake pattern DES-0011 already uses for `UT-0012`. This same fake is the substitution point for `UT-0004`/`UT-0005`, so tests exercise `UiaTreeAcquisitionAdapter`'s real rung-selection/confidence logic (not a re-implementation of it) against controlled reader behavior.
- **Counter-example fixtures** (per behavior, `R-QA-01`): (a) a mutating fake UIA provider that calls a prohibited pattern → must turn the `UT-0005` spy red; (b) a fixture whose AutomationId is runtime-generated but *incorrectly* accepted as rung-1 → must turn a key-stability test red; (c) a virtualized subtree mislabeled `NotExposed` instead of `NotRealized` → must fail the availability-distinction test; (d) discovery candidates in scrambled order → must fail the within-session ordering test; (e) a node with rung-1/2 identity and `UiaNative` provenance but a missing required property (no bounds) *incorrectly* classified `High` → must fail the confidence-rubric-ordering test (`R-TEST-03`).
- **IT (real, incremental)**: the mixed IT fixture app (`DES-0008` `Surveyor.ITFixture.Win32` / `Surveyor.ITFixture.WinForms`) supplies authentic legacy edges (owner-draw, MSAA-only, MDI, windowless, `WM_GETTEXT`); built out incrementally as `DES-0015` and real targets land. The same app hosts a deliberately unresponsive control for `IT-0006` cancellation/timeout response-time verification.

## Unit-Test Intent

| UT | Intent | Meaningful oracle | Anti-pattern avoided | Counter-example |
| -- | -- | -- | -- | -- |
| `UT-0003` | Discovery candidate ordering and status mapping are stable and honest | Seeded candidates (incl. `PermissionDenied`/`IntegrityMismatch`) return the fixed within-session order and modeled statuses; raw `HWND`/z-order do not affect order | Mocking the adapter to echo one DTO and asserting it returned | scrambled-input order must fail |
| `UT-0004` | Acquisition fixture → model with confidence/unavailable/provenance markers, over the `IUiaComReader` fake | Fixture with missing ids, virtualized nodes, MSAA/owner-draw yields the expected `UiElement` states, rung selection, `NotRealized` vs `NotExposed`, and confidence tiers; a fake-reader delay + cancellation-injection scenario asserts cancel-vs-COM-timeout-vs-cap precedence (`R-TEST-01`) | Testing only the happy path where every UIA field is present; faking at a level that bypasses `UiaTreeAcquisitionAdapter`'s own rung/confidence logic (`R-TEST-02`) | virtualized-as-`NotExposed` fixture must fail; rubric-misorder fixture (e) must fail |
| `UT-0005` | Read-only enforcement at the adapter seam | Spy over a full acquisition asserts the invoked COM method set is a subset of the concrete allow-list in [Read-Only Audit](#read-only-audit-rq-048-rd-032-ut-0005); any state-changing pattern, bare `SendMessage`, or `WM_SETTEXT` fails; every emitted `RunDiagnostic.SafeArgs` value matches the allow-listed shape (`R-SEC-02`) | Merely checking the port type has no mutation method | mutating fake provider must turn the spy red |

## Integration Assumptions

- Windows 11, same-integrity default, unpackaged self-contained build; `uiAccess`/elevation only on the signed opt-in path.
- Runs on the **manual developer Windows gate** (`DES-0007` §8.2, `R-OPS-01`); UT stays headless/unattended.
- `IT-0001` (target state unchanged before/after analysis), `IT-0002` (real UIA acquisition incl. legacy edges via the fixture app), `IT-0005` (environment/permission/integrity/packaging premises), `IT-0006` (UIA cancellation response time and COM-level timeout → `Timeout` status on a large legacy tree, `R-WIN-02`). Live legacy-edge coverage is incremental as the fixture app grows.
- Residual: DPI/occlusion imaging behavior is `DES-0015`; this package assumes bounds are read but normalization is not its concern.

## Downstream Handoff

- **Candidate project area**: `Surveyor.Adapters.Discovery` (`M05`), `Surveyor.Adapters.Uia` (`M06`, thin raw-COM wrapper behind the `IUiaComReader` seam), ports already in `Surveyor.Application.Ports`; tests in `Surveyor.Adapters.Discovery.Tests` / `Surveyor.Adapters.Uia.Tests`; fixtures under `tests/fixtures/uia-trees/`; live edges in `tests/it-fixtures/*`.
- **First failing tests**: `UT-0003` "discovery returns the fixed within-session order with modeled statuses"; `UT-0005` "acquisition invokes no state-changing UIA pattern" (spy, asserted against the concrete allow-list) — both written red before adapter code.
- **Implementation slices**: `IMP-0005` (discovery port + fake, #63), `IMP-0006` (acquisition port + fixture loader over the shared model, plus the `IUiaComReader` fake, #64), `IMP-0007` (read-only spy audit + banned-API extension, #65), then `IMP-0013` (real raw-COM UIA adapter implementing `IUiaComReader` with `ConnectionTimeout`/`TransactionTimeout` wired, #71) on the live gate.
- **Verification command**: `dotnet test tests/Surveyor.Adapters.Discovery.Tests`, `dotnet test tests/Surveyor.Adapters.Uia.Tests`, plus the `Surveyor.Architecture.Tests` banned-API check; `IT-0001`/`IT-0002`/`IT-0005`/`IT-0006` on the manual Windows gate.
- **Minimal context bundle** for the implementing agent: this package's [Data And Contract Design](#data-and-contract-design), [UIA Threading And Apartment Model](#uia-threading-and-apartment-model-r-win-01-r-win-02-r-win-03-rq-050), [Read-Only Audit](#read-only-audit-rq-048-rd-032-ut-0005), [Legacy Acquisition Edge Table](#legacy-acquisition-edge-table-r-win-03), and [Confidence Rubric](#acquisition-confidence-rubric); `RQ-048`/`RQ-049`/`RQ-050` from the requirement source; `DES-0009` identity ladder and `Availability`/`AcquisitionConfidence` types; `DES-0011`'s fixed `TargetReference`/`TargetCandidate`/`AcquisitionResult`/`OperationStatus`/`IStageTimeoutController`; ADR-0002 §Decision; the `IUiTreeAcquisitionPort`/`ITargetDiscoveryPort` rows of `DES-0003`. Reading `DES-0001`/`DES-0004` in full is not required.
- **Unblocks**: `DES-0015` (shares `TargetReference`/DPI context), `DES-0017` (cap calibration), `DES-0018` (adapter provider wiring), and the `IT` live-Windows track.

## Self-Review Evidence (author-side, DES-0007 §5 step 8)

Re-swept twice: after the `R-ARC-01` boundary-reshaping fix, and again after the follow-up verification pass found the first `DRP-05` sweep had missed `AcquisitionResult.Availability` (`DRP-10`), per the DES-0007 §5.3 fix-loop convention.

| Pattern | Result |
| -- | -- |
| `DRP-01` Upstream drift | fix applied. The initial version had silently redefined `ITargetDiscoveryPort`/`IUiTreeAcquisitionPort` method names and the `TargetRef`/`TargetCandidate`/`AcquisitionResult` shapes instead of consuming `DES-0011`'s already-fixed versions (`R-ARC-01`, Critical). Now consumes `ITargetDiscoveryPort.ListTargetsAsync`/`ResolveAsync`, `IUiTreeAcquisitionPort.AcquireAsync`, `TargetReference`/`TargetCandidate`/`TargetDiscoveryResult`/`TargetResolveResult`/`AcquisitionResult`/`OperationStatus`/`RunDiagnostic` verbatim from `DES-0011`, and `Availability`/`AcquisitionConfidence`/`IdentityMaterial`/`IFallbackKeyDerivation`/`ElementIdentity` verbatim from `DES-0009`; renames nothing fixed elsewhere. Rung-1 detection and confidence rubric remain *delegated* scope, not redefinitions. |
| `DRP-02` Dangling reference | fix applied. New types defined here are now `DiscoveryQuery`, `TargetProcessInfo`, `TargetIntegrityHint`, `Win32TargetHandle` (internal), `AcquisitionOptions`, `AcquisitionProvenance`, and the internal `IUiaComReader` seam; upstream types (`TargetReference`, `TargetCandidate`, `AcquisitionResult`, `OperationStatus`, `RunDiagnostic`, `IdentityMaterial`, `ElementIdentity`) resolve to `DES-0009`/`DES-0011` and are not re-declared. |
| `DRP-03` Data-flow closure | checked clean. The I/O derivation table traces every port output to a named consumer and every input to caller/prior-stage/outward-read, updated for the `DES-0011` method names. |
| `DRP-04` Round-trip asymmetry | checked clean. Fixture⇄model uses the shared `DES-0009` types; `TargetReference`/`Win32TargetHandle` are now two distinct types (not one type with a visibility split) with no inward→`Win32TargetHandle` direction; no persistence round-trip introduced. |
| `DRP-05` Unowned field | fix applied, twice. Field-ownership table now names `TargetReference.SessionTargetId`, `Win32TargetHandle.WithinSessionOrdinal`, `AcquisitionResult.Status`/`.HitElementCap`, and `AcquisitionProvenance` with single writer, timing, and fabrication rules consistent with the `DES-0011` shapes. The first sweep missed `AcquisitionResult.Availability` — a `DES-0011`-fixed run-level field this package inherited without ever specifying its population rule or its relationship to the per-node `UiElement.Availability` already inside `ScreenModel`; a second-pass verification review caught this, and the table/Acquisition section now define it as a deduplicated `Unavailable(reason)` rollup, explicitly not a second source of truth. |
| `DRP-06` Rule overlap without precedence | fix applied. Confidence rubric now states explicitly that it is evaluated top-down, first-match-wins, with a counter-example fixture guarding the identity+provenance-without-property-completeness edge (`R-TEST-03`). Rung selection, timeout-vs-cancel (now delegated to `DES-0011`'s `IStageTimeoutController` race rule), and runtime-id detection remain ordered first-match lists. |
| `DRP-07` Numeric under-specification | N/A for scoring numerics (owned by `DES-0010`); caps are integer/`TimeSpan` with fixed semantics, default values deferred to `DES-0017` with that stated. |
| `DRP-08` Missing failure semantics | fix applied. Cancel/timeout/cap/permission/disposed-target/collision each have defined behavior mapped onto `DES-0011`'s `OperationStatus`/`IStageTimeoutController` race rule; a single hung COM call is now bounded by a COM-level timeout (`R-WIN-01`) with a documented thread-abandon fallback (`R-WIN-03`) rather than relying on cooperative cancellation alone. |
| `DRP-09` Port ownership ambiguity | fix applied. Both ports are application-owned per `DES-0011`; adapters in `Surveyor.Adapters.*` depend inward; no UIA/`HWND` type crosses the port surface (`RQ-054`), now enforced by the `Win32TargetHandle`/`TargetReference` type split rather than a policy statement alone (`R-ARC-02`). |
| `DRP-10` Patch regression | fix applied, twice: the `R-ARC-01` boundary reshape triggered the first `DRP-02`–`DRP-05` re-sweep; a follow-up verification review of that fix-loop then caught the `AcquisitionResult.Availability` gap the first re-sweep left behind, both recorded above. |

DES-0007 §9: Trace/module-coverage/guardrails (`RQ-048` read-only audit, `RQ-051` within-session ordering scope + stable model order, `RQ-052` text-as-label-only, `RQ-054` no leak inward)/determinism/confidentiality/testability/unit-test-intent/handoff — all present above.

## Residual Risks

- `RSK-RD-001` closes for **UIA client selection** (ADR-0002); live legacy-edge and capture coverage remain incremental via `DES-0015` and the IT fixture app.
- The `runtime-id-rules v=1` deny-list, including the broadened embedded-digit-run check (`R-SEC-04`), is a syntactic heuristic; it reduces but does not eliminate the chance a customer/account-shaped identifier reaches rung 1 unflagged, and real frameworks may need tuning. It is versioned and report-recorded so a change is auditable; carried as a named review item.
- `WM_GETTEXT` is a documented read-only exception (query message, no state change), now bounded by `SendMessageTimeout`/`SMTO_ABORTIFHUNG` and a proactive per-class deny-list (`R-WIN-02`, `R-SEC-03`); if a specific control still proves it can have side effects, that control drops to `Unavailable(NotExposed)`. Carried.
- A genuinely wedged (not merely slow) target can still exhaust the COM-level `TransactionTimeout` without the underlying call returning; the documented mitigation is thread abandonment plus a fresh acquisition thread for the next run (`R-WIN-03`), not a guaranteed unblock of the wedged call. `IT-0006` is the live check that keeps this honest. Carried.
- The COM-level call budget (`R-WIN-01`) depends on `IUIAutomation6` (Windows 10 1809+); on an older Windows 10 build where that interface is unavailable, `RawComUiaClient` falls back to cooperative node-boundary cancellation only for the per-call budget, narrowing (not eliminating) the `R-WIN-01` mitigation on such machines. `IMP-0013` must detect and record this fallback rather than silently degrade. Carried as a named risk.
- The read-only guarantee (`RQ-048`) is defense-in-depth — type-surface absence, the structural no-pattern-object-leaves-the-wrapper guarantee, the build-time banned-symbol/`dynamic` guard, and the `UT-0005` spy — but is not a formal proof against a determined caller using unsafe/raw interop outside the wrapper's own code path (`R-SEC-01`). Carried as a named risk, meaningfully narrowed from the initial type-surface-only claim.
- `TargetReference.SessionTargetId` identity is within-session only by design; cross-run correlation is the domain `ScreenKey`'s job (`DES-0009`), not discovery.
- Cap *default values* and large-tree performance targets are deferred to `DES-0017`; until then only cap *semantics* are fixed.
- Concrete DI wiring/lifetimes of these adapters are `DES-0018`.

## Related

- [ADR-0002](../decisions/adr-0002-adapter-technology-selection.md), [DES-0003](des-0003-module-interface-basic-design.md), [DES-0004](des-0004-analysis-flow-basic-design.md), [DES-0007](des-0007-detailed-design-execution-strategy.md), [DES-0008](des-0008-project-structure-and-test-harness.md), [DES-0009](des-0009-domain-model-stable-keys-and-availability.md), [DES-0011](des-0011-port-dtos-status-model-and-use-case-orchestration.md), `DES-0015` (planned), [design-review-patterns](../process/design-review-patterns.md).
