---
type: Trace Evidence
title: TRC-0004 DES-0015 Review-Gate Pre-Clearance Evidence
description: AI reviewer-side (L2) pre-clearance evidence for review gate issue #37 on DES-0015, covering the four-lens sweep, the DRP-01..10 pattern sweep with upstream cross-checks, DPI-normalization and failure-mode judgments, the DES-0007 section 9 checklist, guardrail failing-first coverage, and carried residual risk. Human final approval per DES-0007 section 5.2 remains open.
tags: [trace, review-gate, des-0015, capture, dpi, coordinate-mapping, rq-011, rq-016, rq-027, rq-028]
timestamp: 2026-07-11T00:00:00+09:00
---

# TRC-0004 DES-0015 Review-Gate Pre-Clearance Evidence

Reviewer-side (L2) design-review evidence for review gate [#37](https://github.com/urario/Surveyor/issues/37) on
[DES-0015 Capture and Snapshot Correspondence](../design/des-0015-capture-and-snapshot-correspondence.md)
(parent [#26](https://github.com/urario/Surveyor/issues/26), module `M07`). This is the AI pre-clearance
required by `DES-0007` §5.2 before the gate can be closed; the human owner's final gate-close approval remains open.

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `TRC-0004`, DES-0015 Review-Gate Pre-Clearance Evidence, detailed design phase (review gate) |
| Upstream | [DES-0015](../design/des-0015-capture-and-snapshot-correspondence.md); gate scope from issue #37; [ADR-0002](../decisions/adr-0002-adapter-technology-selection.md) (#30); [TRC-0001](trc-0001-adr-0002-spike-measurements.md); [DES-0007](../design/des-0007-detailed-design-execution-strategy.md) §5.2 / §9; [Design Review Pattern Catalog](../process/design-review-patterns.md); `RQ-011`, `RQ-016`, `RQ-027`, `RQ-028`; `RD-012`, `RD-013` |
| Downstream | Gate close (or accept-with-risks) unblocks `IMP-0014` (#72) and confirms the `IT-0003` (#55) integration premises; extends `UT-0011`/`UT-0012` intents |
| Evidence | Four-lens sweep, `DRP-01`–`DRP-10` reviewer verdict with upstream cross-checks, DPI/failure-mode judgments, §9 checklist, guardrail failing-first coverage (below) |
| Verification | Cross-reference checks against `DES-0009`/`DES-0011`/`DES-0013` source and `ADR-0002`/`TRC-0001` (below); `tools/okf/Validate-Okf.ps1` for this note |
| Residual Risk | Mixed-DPI-monitor live behavior and yellow-border/consent visuals deferred to `IT-0003`; black-frame heuristic is a signal not a proof; WGC-uncapturable catalogue is from one smoke run; `SnapshotRef`-as-projection lifetime is intentional — all carried, none blocking |

## Gate Context Reconciliation

The gate's original residual-risk note ("`DES-0015` 設計ドラフト未作成 + `ADR-0002` 未決のため着手不可") is **stale as of 2026-07-11** on both counts:

- The design draft exists: [des-0015-capture-and-snapshot-correspondence.md](../design/des-0015-capture-and-snapshot-correspondence.md) (timestamp 2026-07-04), and it names review gate #37 as its downstream.
- `ADR-0002` is decided and its spike issue [#30](https://github.com/urario/Surveyor/issues/30) is **closed as completed** (2026-07-03); the accepted decision (WGC primary, PrintWindow fallback, unpackaged same-integrity) is the design's binding input, with measurements in `TRC-0001`.

The gate is therefore actionable, not blocked; this note is its pre-clearance evidence.

## Upstream Cross-Check (reviewer-verified, not author-asserted)

The package's central `DRP-01` claim is that it consumes upstream DTO shapes verbatim and only fills field detail those packages left open. Verified against source:

| Referenced by DES-0015 | Resolved definition (source) | Consistent? |
| -- | -- | -- |
| `CaptureRequest(Target, Regions, FirstFrameTimeout, RequireCapture)` | `DES-0011` `CaptureRequest` | Yes — exact field match |
| `CaptureResult(Status, IReadOnlyList<CapturedRegion> Regions, CaptureCoordinateSpace CoordinateSpace, IReadOnlyList<RunDiagnostic> Diagnostics)` | `DES-0011` `CaptureResult` | Yes — exact field match |
| `RegionOfInterest` order `(SourceFindingId, ElementKey, Id)` | `DES-0011` "ROI order is deterministic by `SourceFindingId`, `ElementKey`, then `Id`" | Yes — identical |
| `AnalysisRunOptions.CaptureFirstFrameTimeout` 5s / `ContinueWithoutCapture` true / `RequireCapture` false | `DES-0011` defaults table | Yes — identical defaults, consumed not redefined |
| `CapturedRegion` references bytes via opaque `CaptureBlobId` (field detail open) | `DES-0011`: "`CapturedRegion` references bytes through an opaque in-memory `CaptureBlobId`… File paths belong to `M12`." | Yes — DES-0011 leaves field detail open; DES-0015 fills it |
| `StoredCaptureArtifact(CaptureBlobId, RegionOfInterest Region, byte[] PngBytes, CaptureCoordinateSpace)` | `DES-0013` `StoredCaptureArtifact` | Yes — exact field match; PNG format fixed by DES-0013, consumed as-is |
| `BoundingRect` (int, "target-DPI-normalized per DES-0015"), `SnapshotRef`, `Availability`/`UnavailableReason` | `DES-0009` | Yes — DES-0015 supplies the delegated population rule without adding a field to any fixed type |

No upstream method was renamed and no field was added to any already-fixed type; the `DRP-01` drift claim holds at reviewer level.

## Four-Lens Review

- **Architect lens** — Accept. `M07` stays an adapter behind the application-owned `IScreenCapturePort`; no capture API type crosses the port; `RectangleDip.FromBoundingRect` and the `SnapshotRef` projection are `Surveyor.Domain`-homed pure helpers, keeping `RQ-054` intact (core needs no Win32/DPI access mid-orchestration). Resolving `SnapshotRef` as a derived projection rather than a new stored field is the correct call — adding a field to the `DES-0011`-fixed `AnalysisRunResult` would itself be `DRP-01` drift.
- **Implementer lens** — Accept. The WGC→PrintWindow→`Unavailable` fallback order, the black-frame heuristic definition, the failure-mode table, and the field detail (`CapturedRegion`, `CaptureRegionMetadata`, `CaptureCoordinateSpace`, `CaptureBlobId`) are concrete enough to build `IMP-0014` without inventing DPI or coordinate behavior. First failing tests are named.
- **Quality lens** — Accept with carried risks. Failure modes are keyed to distinct, non-overlapping detection signals; occlusion is explicitly called out as a non-failure and is still exercised by `IT-0003` to prevent silent regression. The residual risks are honestly named (single-DPI measurement machine, heuristic-not-proof, one-smoke-run catalogue).
- **Test lens** — Accept. `UT-0011`/`UT-0012` extensions run over fakes only (no live window), each with a meaningful oracle, an anti-pattern avoided, and a counter-example (silently-dropped `Unavailable` region; a required-region failure still reported `Completed`; a rescaling `FromBoundingRect`). Adapter-level black-frame and PMv2 self-check are unit-testable over synthetic buffers.

## DPI-Normalization and Failure-Mode Judgments (gate-specific, issue #37)

- **`R-WIN-01` DPI normalization** — Pass. The physical-pixel contract is grounded in the `TRC-0001` measurement (non-PMv2 path saw virtualized 455×537 vs physical 664×796). "target-DPI-normalized" is fixed to *physical pixels, virtual-screen origin, PMv2-aware reader, no rescale*, and effective DPI is recorded as metadata only. The `CaptureCoordinateSpace.PhysicalPixelsUnverified` self-check is a sound defense-in-depth against the exact measured virtualization bug.
- **`R-WIN-04` failure-mode table** — Pass. black frame / layered / GPU / DWM / WGC-uncapturable / disposed / offscreen / timeout each map to `Unavailable(reason)` or `Timeout`; per-region failure never aborts sibling regions (matches `ContinueWithoutCapture=true`).
- **Pure mapping separation** — Pass. `FromBoundingRect` is a total identity cast, trivially fakeable, exactly the "purely functional overlay coordinate mapping" `DES-0007` §6 requires.

**Reviewer observation (Low, non-blocking, recorded not raised as a finding):** the type name `RectangleDip` / field `BoundsDip` read as device-independent pixels, but by this package's contract they hold *physical* pixels (1 unit = 1 captured-image pixel). The document justifies this explicitly (canonical overlay space anchored to the captured image grid; on-screen DPI scaling is a `DES-0016` display-time concern), so it is a deliberate, documented naming tension rather than a defect. Flagged so `DES-0016`/`IMP-0014` do not reintroduce a DPI rescale on the strength of the name; no `DRP-xxx` match and no design change required.

## DRP-01..10 Reviewer Verdict

| Pattern | Reviewer verdict |
| -- | -- |
| `DRP-01` Upstream drift | Clean (cross-check table above). `SnapshotRef`-as-projection is the correct anti-drift resolution. |
| `DRP-02` Dangling reference | Clean. New types (`CapturedRegion`, `CaptureCoordinateSpace`, `CaptureBlobId`, `CaptureRegionMetadata`, `RectangleDip`, `SnapshotRef`, `CaptureMethod`) are defined with fields; upstream types resolve to DES-0009/0011/0013. |
| `DRP-03` Data-flow closure | Clean. Port I/O derivation traces inputs to Stage-1/2 target + Stage-6 ROI construction, and outputs to report/ViewModel via the `SnapshotRef` projection. |
| `DRP-04` Round-trip asymmetry | Clean. `CaptureRequest.Regions` ⇄ `CaptureResult.Regions` correlate 1:1 by `RegionId`; `CapturedRegion` ⇄ `StoredCaptureArtifact` symmetric; `SnapshotRef` is explicitly a recompute, not a persistence round-trip. |
| `DRP-05` Unowned field | Clean. Every new field has a single writer, timing, and fabrication rule (`ActualBoundsDip` reported even when it disagrees with the request). |
| `DRP-06` Rule overlap without precedence | Clean. Capture selection is an ordered fallback; failure-table rows are non-overlapping; occlusion is an explicit non-match. |
| `DRP-07` Numeric under-specification | N/A — no score/threshold arithmetic; `FromBoundingRect` is an identity cast; the heuristic's grid/tolerance are `IMP-0014` implementation constants, not domain decision numerics. Reviewer concurs with the N/A. |
| `DRP-08` Missing failure semantics | Clean. Every capture I/O boundary has a defined outcome; cancel-vs-timeout precedence inherited from `DES-0011`'s `IStageTimeoutController`; partial-continues-run is explicit. |
| `DRP-09` Port ownership ambiguity | Clean. `IScreenCapturePort` application-owned; adapter depends inward; helpers `Surveyor.Domain`-homed. |
| `DRP-10` Patch regression | N/A for this initial authoring pass; applies to any fix-loop round after this review. |

No Critical/High finding surfaced, so no new `DRP-xxx` catalog candidate is raised.

## DES-0007 §9 Checklist Judgments

| Checklist item | Judgment |
| -- | -- |
| Trace | Pass — scoped `RQ`/`RD`/`ADR`/`DES` upstream and `IMP`/`IT`/`UT` downstream links. |
| Pattern sweep | Pass — DRP table above; contract-closure tables present. |
| Module coverage | Pass — `M07` / `Surveyor.Adapters.Capture`. |
| Guardrails | Pass — `RQ-048` read-only capture posture (no foreground/move/activate/input; PrintWindow `WM_PRINT` caveat accepted in ADR-0002); `RQ-051` no capture-path-dependent geometry drift; `RQ-052` image confidentiality apply-point; `RQ-054` no capture API type crosses the port. |
| Determinism | Pass — physical-pixel contract, deterministic ROI order, metadata-driven (not image-analysis) correspondence, no capture-path-dependent geometry. |
| Confidentiality | Pass — pre-policy bytes stay behind `CaptureBlobId` until `IConfidentialityPolicy.Apply`; diagnostics carry only `RegionOfInterest.Id`/`CaptureMethod`/`EffectiveDpi`, no raw title/name/path; image bytes never in a diagnostic. |
| Read-only | Pass — WGC compositor-side; PrintWindow render caveat explicitly bounded by ADR-0002; never foregrounds/moves/activates/sends input. |
| Testability | Pass — synthetic capture fakes, counter-example fixtures, adapter-level heuristic/self-check tests; no live window for UT. |
| Unit-test intent | Pass — `UT-0011`/`UT-0012` name behavior, oracle, anti-pattern, counter-example. |
| Handoff | Pass — candidate project area, first failing tests, verification commands, minimal context bundle. |

Guardrail failing-first coverage (`R-QA-03`): `RQ-054` — `UT-0011`/`UT-0012` fakes-only; `RQ-051` — `UT-0012` asserts `BoundsDip` numerically equals source `BoundingRect` (rescale counter-example red); `RQ-052` — apply-point kept behind the blob table; `RQ-048` — read-only posture asserted by `Surveyor.Architecture.Tests` and live `IT-0003` occlusion/no-mutation.

## Verdict

**Accept with risks** (AI pre-clearance). The design is implementation-ready for `IMP-0014` (#72) and confirms the `IT-0003` (#55) premises. Carried residual risks (all non-blocking, all deferred to `IT-0003` or `IMP-0014` logging):

1. Mixed-DPI-monitor live behavior and yellow-capture-border/consent visuals — measured at a single DPI in `TRC-0001`, exercised live at `IT-0003`.
2. Black-frame heuristic is a pixel-uniformity signal, not a certain detector — defense-in-depth on top of WGC-primary selection.
3. WGC-uncapturable catalogue is from one smoke run — `IMP-0014` must log (not swallow) new `ArgumentException` shapes so the table grows.
4. `SnapshotRef`-as-projection lifetime is intentional — a future package must not "fix" it into a stored field without a `DES-0007` §5.3 supersede note against both `DES-0009` and `DES-0011`.
5. `RectangleDip`/`BoundsDip` naming holds physical pixels by contract (see reviewer observation) — carry forward so no downstream package reintroduces a rescale on the name alone.

**Open**: human-owner final gate-close approval (`DES-0007` §5.2). Project field `Status` should move `Blocked` → `Ready`/`In progress` once approved (manual, current token lacks `project` scope).

## Related

- [DES-0015 Capture and Snapshot Correspondence](../design/des-0015-capture-and-snapshot-correspondence.md)
- [DES-0007 Detailed Design Phase Execution Strategy](../design/des-0007-detailed-design-execution-strategy.md)
- [ADR-0002 Adapter Technology Selection](../decisions/adr-0002-adapter-technology-selection.md)
- [TRC-0001 ADR-0002 Spike Measurement Evidence](trc-0001-adr-0002-spike-measurements.md)
- [Design Review Pattern Catalog](../process/design-review-patterns.md)
- [Quality Review Policy](../process/quality-review-policy.md)
- [Lifecycle Traceability](../process/lifecycle-traceability.md)
