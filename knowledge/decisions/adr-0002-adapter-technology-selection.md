---
type: Decision
title: ADR-0002 Adapter Technology Selection (UIA Client, Capture API, Packaging)
description: Accepted decision record for the RSK-RD-001 spike comparing UIA client (raw COM vs FlaUI), capture API (PrintWindow vs Windows.Graphics.Capture), and packaging form.
tags: [adr, spike, uia, capture, packaging, rq-048, rq-049, rq-050, rq-051]
timestamp: 2026-07-02T00:00:00+09:00
---

# Status

**Accepted (human-approved 2026-07-03, issue #30).** Scaffold, desk analysis,
and per-axis measurements against real targets ([TRC-0001](../traces/trc-0001-adr-0002-spike-measurements.md),
including the human owner's TortoiseGit/MFC run of 2026-07-03) are complete, and the
Decision section below fixes one selected candidate per dimension. This promotion opens the
`DES-0014`/`DES-0015`/`DES-0018`(wiring)/`IMP-0013`/`IMP-0014` gates (#25/#26/#29/#71/#72)
while carrying the residual risks listed below.

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

# Measurement results

Full evidence, raw numbers, environment, and reproduction steps: [TRC-0001](../traces/trc-0001-adr-0002-spike-measurements.md).
Targets: **T1 TortoiseGit (real C++/MFC, 48 elements — run by the human owner)**, T2 Visual
Studio Code (Chromium, 1029 elements — large-tree case), T3 shell/UWP windows (WGC
failure-mode probes). Single Windows 11 machine, DPI 144, same integrity, unpackaged.

| Axis | uia-raw-com | uia-flaui | capture-printwindow | capture-wgc |
| -- | -- | -- | -- | -- |
| Read-only | PASS (read-only API surface; idle-tree hash unchanged across 4 runs) | PASS (same basis) | PASS with caveat (WM_PRINT-style render request into the target) | PASS (compositor-side, no foregrounding) |
| Determinism | PASS (byte-identical tree hash across fresh processes; T2 post-realization) | PASS (same) | n/a (metadata only) | n/a (metadata only) |
| Fixtureability | PASS (COM mockable at adapter seam; lazy-realization case expressible as `NotRealized` fixture) | PASS (extra wrapper layer to fake) | PASS | PASS |
| Permissions/integrity | PASS same-integrity; **elevated target not measured (carried)** | same | same | same |
| Packaging | PASS unpackaged; **MSIX/`uiAccess` not exercised (carried)** | same | same | same |
| Performance | 48 elem/~125 ms; 1029 elem/~1.6 s | ~+15% vs raw (156 ms / 1.8 s) | 40 ms/frame | ~400 ms first frame (pool warm-up) |

Key live findings (detailed in TRC-0001): Chromium **lazy accessibility-tree realization**
(first touch returns a 19-element skeleton; stable 1029 thereafter → `DES-0014` warm-up +
`Unavailable(NotRealized)` rule); **DPI virtualization** (non-PMv2 process captured 455×537
while WGC delivered physical 664×796 → `DES-0015` PMv2 obligation confirmed); WGC
`CreateForWindow` `ArgumentException` on shell/UWP-frame windows (→ failure-mode table +
fallback); both UIA candidates ran correctly from **MTA**; no permission failures at same
integrity.

# Decision

**Accepted selection (one candidate per dimension):**

1. **UIA client: raw COM (`Interop.UIAutomationClient` PIA)** wrapped in a thin
   Surveyor-owned internal layer inside `Surveyor.Adapters.Uia`. Measurements confirmed the
   desk lean: identical read behavior and determinism to FlaUI, ~15–20% faster, direct
   HRESULT visibility for the `RQ-049` status mapping, and — decisive for the guardrails —
   a 1:1 auditable mapping between the `RD-032` prohibited-pattern list and COM methods that
   simply are never referenced (`RQ-048`, `UT-0005`). FlaUI's measured advantage was
   ergonomics only, which the thin internal wrapper recovers.
2. **Capture: Windows.Graphics.Capture primary, PrintWindow(PW_RENDERFULLCONTENT) fallback.**
   WGC captured the real MFC target compositor-side at physical DPI without touching the
   target; PrintWindow remains necessary for WGC-uncapturable windows (shell/UWP-frame
   `ArgumentException` cases measured) and degrades gracefully via the `DES-0015`
   failure-mode table (`Unavailable(reason)` when both fail).
3. **Packaging: unpackaged (self-contained) primary.** Verified working at same integrity
   with no prompts; keeps the signed classic-manifest `uiAccess` escape hatch available
   (MSIX conflicts with `uiAccess`); optional MSIX distribution can be added later if the
   elevated-target calibration shows `uiAccess` is never needed.

# Consequences

- Adapter-bound packages are unblocked by the human approval of issue #30; residual risks stay attached to their downstream design/test owners rather than reopening this decision.
- Threading/async findings feed the `DES-0011` acquisition-port contracts **before** they
  are frozen: MTA-safe client confirmed; first-acquisition warm-up/lazy-realization rule;
  WGC async first-frame budget (~400 ms) in the capture-stage timeout; cancellation on a
  hung target still unexercised (→ `DES-0014`/`IT-0006`).
- The spike PoCs remain throwaway evidence tooling; the production adapters are implemented
  under `DES-0014`/`DES-0015` designs with `UT-0005` read-only spy coverage — no PoC code is
  promoted into `src/**`.
- The MSAA/`IAccessible` fallback strength for legacy edges (owner-draw, MSAA-only proxies)
  remains a `DES-0014` design item with the IT fixture app; nothing measured contradicts the
  raw-COM pick for it.

## Residual risks (carried at promotion)

- **Elevated-target behavior not calibrated** — the exact `PermissionDenied`/`IntegrityMismatch`
  failure shape per candidate awaits a UAC-elevated target run; owned by `DES-0014`/`IT-0005`.
- **Mixed-DPI, `uiAccess` signed build, MSIX** not exercised; packaging settings stay
  provisional in `DES-0008` until `IT-0005` covers them.
- **Yellow capture border / consent UX** for WGC not visually recorded; affects the `RQ-052`
  user-notice design in `DES-0016`, not the API pick.
- Single machine / single Windows build (22621); OS-build variance of WGC border/consent
  semantics is known and absorbed by the fallback design.

# Verification

- Scaffold: `dotnet build spikes/adr-0002/Spike.Adr0002.slnx` green (0 warnings), all four
  PoCs smoke-run read-only on the dev machine 2026-07-02 (evidence in
  [spikes/adr-0002/README.md](../../spikes/adr-0002/README.md)).
- Exit criteria ([DES-0007](../design/des-0007-detailed-design-execution-strategy.md) §4.2): **met** — every axis has a pass/fail result with
  evidence ([TRC-0001](../traces/trc-0001-adr-0002-spike-measurements.md)), one candidate is
  selected per dimension (Decision above), and reproduction steps are captured in
  TRC-0001; the carried items are named in Residual risks with downstream owners.
- Human owner ran the harness against the real MFC target (TortoiseGit, 2026-07-03);
  supplementary large-tree/failure-mode runs executed on the same machine; human approval
  completed in issue #30 on 2026-07-03.
- `tools/okf/Validate-Okf.ps1` green after this revision's registration.

# Related

- [DES-0007 Detailed Design Phase Execution Strategy](../design/des-0007-detailed-design-execution-strategy.md) — §4.2 spike definition, §8 axes, §8.1 carried human decision
- [DES-0005 V-Model Traceability and Downstream Tests](../design/des-0005-vmodel-traceability-and-downstream-tests.md) — `RSK-RD-001`
- [DES-0003 Module Interface Basic Design](../design/des-0003-module-interface-basic-design.md) — `IUiTreeAcquisitionPort` / `IScreenCapturePort` contracts
- [ADR-0001 AI Collaboration and OKF](adr-0001-ai-collaboration-and-okf.md)
- [Spike scaffold](../../spikes/adr-0002/README.md)
- [TRC-0001 ADR-0002 Spike Measurement Evidence](../traces/trc-0001-adr-0002-spike-measurements.md)
