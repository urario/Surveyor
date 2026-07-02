---
type: Decision
title: ADR-0002 Adapter Technology Selection (UIA Client, Capture API, Packaging)
description: Draft decision record for the RSK-RD-001 spike comparing UIA client (raw COM vs FlaUI), capture API (PrintWindow vs Windows.Graphics.Capture), and packaging form; final selection and promotion are a human decision.
tags: [adr, spike, uia, capture, packaging, rq-048, rq-049, rq-050, rq-051]
timestamp: 2026-07-02T00:00:00+09:00
---

# Status

**Proposed (draft)** — scaffold + desk analysis complete; per-axis measurements against real
legacy targets are pending the human owner's PoC runs. Per [DES-0007](../design/des-0007-detailed-design-execution-strategy.md)
§4.2 and §8.1, the final technology selection and the promotion of this ADR to *Accepted*
are a **human decision**; this draft is the AI-owned synthesis vehicle (issue #30).

# Context

`RSK-RD-001` leaves three adapter technology choices open, gating `DES-0014`/`DES-0015`
design and `IMP-0013`/`IMP-0014` implementation (#25, #26, #71, #72, and the concrete
wiring part of #29):

- **UIA client** for `M06` (`IUiTreeAcquisitionPort`): raw COM (`Interop.UIAutomationClient` PIA) vs FlaUI (UIA3 wrapper).
- **Capture API** for `M07` (`IScreenCapturePort`): GDI `PrintWindow(PW_RENDERFULLCONTENT)` vs `Windows.Graphics.Capture` (WGC).
- **Packaging form**: unpackaged vs MSIX vs hybrid (interacts with the `R-SEC-02` decision:
  same-integrity default, signed `uiAccess` opt-in only when a target requires it).

Comparison axes (fixed by [DES-0007](../design/des-0007-detailed-design-execution-strategy.md) §4.2/§8):
read-only feasibility (`RQ-048`), determinism (`RQ-051`), fixtureability, permissions/
integrity (`RQ-049`), packaging, performance (`RQ-050`). Driving requirements also include
`RD-001`, `RD-003`, `RD-023`, `RD-026`.

Spike assets: [spikes/adr-0002/](../../spikes/adr-0002/README.md) — four buildable PoCs, a
per-axis measurement harness (`Measure-Spike.ps1`), a results template, and packaging
verification steps. Smoke-verified on the dev machine (2026-07-02).

# Candidate analysis (desk, pre-measurement)

## UIA client

| Axis | Raw COM (`Interop.UIAutomationClient`) | FlaUI (UIA3) |
| -- | -- | -- |
| Read-only feasibility | Full control over exactly which COM calls occur; the prohibited-pattern list (`RD-032`) maps 1:1 to interface methods never referenced; easiest to prove for the `UT-0005` spy | Wrapper decides call sequences internally; read-only proof needs wrapper-source review or a COM-level spy; convenience methods may touch patterns implicitly |
| Determinism | No hidden caching unless requested; property reads explicit | CacheRequest defaults and convenience property retrieval add a layer that must be pinned |
| Fixtureability | Port-level fakes unaffected by either choice (fixture trees stay synthetic); COM interfaces are mockable at the adapter seam | Same at the port seam; FlaUI types in the adapter add a second wrapping layer to fake |
| Permissions/integrity | Direct HRESULT visibility for `PermissionDenied`/`IntegrityMismatch` mapping | HRESULTs wrapped in FlaUI exceptions; mapping is indirect |
| Packaging | One PIA NuGet, no runtime deps | FlaUI + PIA dependency; larger surface, third-party maintenance cadence risk |
| Performance | Minimal overhead; verbose code (mitigable by a thin internal wrapper we own) | Small wrapper overhead (smoke run: comparable); much less code to write |
| Maintainability (secondary) | More interop code owned by us | Mature API, faster development; community-maintained |

*Desk-analysis lean*: **raw COM**, because `RQ-048` (read-only proof) and `RQ-051`
(no hidden caching/ordering) are Critical-guardrail concerns that outweigh developer
convenience, and `M06` is a bounded adapter behind an application-owned port anyway.
**Not decided** — live legacy-edge behavior (MSAA proxies, owner-draw, threading) may
change the picture; awaiting measurements.

## Capture API

| Axis | `PrintWindow(PW_RENDERFULLCONTENT)` | `Windows.Graphics.Capture` |
| -- | -- | -- |
| Read-only feasibility | Sends a WM_PRINT-style render request into the target — technically causes the target to repaint (must verify no observable state change on legacy apps) | No message into the target; compositor-side; no foregrounding (smoke-verified) |
| Determinism | Synchronous single call; metadata trivially stable | Async frame pool; first-frame latency varies; metadata stable, image arrival timing not |
| Fixtureability | Trivial to fake at the port | Same at the port; live PoC needs D3D interop (already scaffolded) |
| Permissions/integrity | Works for same-integrity windows; known black-frame failures for layered/GPU/DWM-composited content | Requires Win10 1903+; some shell windows uncapturable (smoke run: `Program Manager` → `ArgumentException`); yellow capture border / consent semantics vary by OS build |
| Packaging | No constraints | Unpackaged OK (smoke-verified); border-removal APIs need newer builds/capabilities |
| Performance | ~137 ms full window on dev machine | ~375 ms first frame on dev machine (pool warm-up); subsequent frames cheap |

*Desk-analysis lean*: **WGC primary with PrintWindow fallback** — WGC's compositor-side
capture is the stronger fit for the `RQ-048` "no interaction with the target" guardrail and
handles occlusion, while PrintWindow covers WGC-uncapturable windows and older environments;
the `DES-0015` failure-mode table (`Unavailable(reason)`) absorbs both failure sets.
**Not decided** — border/consent behavior on real targets and mixed-DPI correctness pending.

## Packaging

Desk analysis in [spikes/adr-0002/packaging/README.md](../../spikes/adr-0002/packaging/README.md):
*lean* is **unpackaged (self-contained) primary**, keeping the signed classic-manifest
`uiAccess` escape hatch available (MSIX conflicts with `uiAccess`), with optional MSIX
distribution later if `uiAccess` proves unnecessary. **Not decided** — elevated-target
behavior measurements drive whether `uiAccess` matters at all.

# Measurement results (pending — human owner)

> To be filled from `spikes/adr-0002/measurement/results/` (per-target filled copies of
> `results-template.md`) after the human owner runs the harness against real legacy
> targets: per-axis pass/fail with evidence, threading/apartment observations, legacy
> acquisition edges, capture failure modes, elevated-target/packaging outcomes.

| Axis | uia-raw-com | uia-flaui | capture-printwindow | capture-wgc |
| -- | -- | -- | -- | -- |
| Read-only | pending | pending | pending | pending |
| Determinism | pending | pending | n/a (metadata only) | n/a (metadata only) |
| Fixtureability | pending | pending | pending | pending |
| Permissions/integrity | pending | pending | pending | pending |
| Packaging | pending | pending | pending | pending |
| Performance | pending | pending | pending | pending |

# Decision

**Pending measurements.** The recommendation will name exactly one candidate per dimension
with per-axis evidence; the human owner approves, this ADR moves to *Accepted*, and the
`DES-0014`/`DES-0015`/`DES-0018`(wiring)/`IMP-0013`/`IMP-0014` gates open.

# Consequences

- Until promotion, adapter-bound packages stay gated ([DES-0007](../design/des-0007-detailed-design-execution-strategy.md) §4.2 *Gate*).
- Threading/async findings from the runs feed the `DES-0011` acquisition-port contracts
  (cancellation/timeout, apartment model) **before** those contracts are frozen.
- The spike PoCs remain throwaway evidence tooling; whichever candidates are picked, the
  production adapters are re-implemented under `DES-0014`/`DES-0015` designs with
  `UT-0005` read-only spy coverage — no PoC code is promoted into `src/**`.
- If measurements show neither UIA candidate fully covers the legacy edges, a hybrid
  (UIA + strengthened MSAA fallback) lands as additional `DES-0014` design work (recorded
  residual risk of issue #30).

# Verification

- Scaffold: `dotnet build spikes/adr-0002/Spike.Adr0002.slnx` green (0 warnings), all four
  PoCs smoke-run read-only on the dev machine 2026-07-02 (evidence in
  [spikes/adr-0002/README.md](../../spikes/adr-0002/README.md)).
- Exit criteria ([DES-0007](../design/des-0007-detailed-design-execution-strategy.md) §4.2): every axis pass/fail with evidence; one recommended
  candidate per dimension; reproduction steps captured — tracked in the measurement
  results, not yet met.
- `tools/okf/Validate-Okf.ps1` green after this draft's registration.

# Related

- [DES-0007 Detailed Design Phase Execution Strategy](../design/des-0007-detailed-design-execution-strategy.md) — §4.2 spike definition, §8 axes, §8.1 carried human decision
- [DES-0005 V-Model Traceability and Downstream Tests](../design/des-0005-vmodel-traceability-and-downstream-tests.md) — `RSK-RD-001`
- [DES-0003 Module Interface Basic Design](../design/des-0003-module-interface-basic-design.md) — `IUiTreeAcquisitionPort` / `IScreenCapturePort` contracts
- [ADR-0001 AI Collaboration and OKF](adr-0001-ai-collaboration-and-okf.md)
- [Spike scaffold](../../spikes/adr-0002/README.md)
