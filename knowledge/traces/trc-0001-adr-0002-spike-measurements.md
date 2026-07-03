---
type: Trace Evidence
title: TRC-0001 ADR-0002 Spike Measurement Evidence
description: Per-axis measurement evidence from running the ADR-0002 PoCs against real targets on the owner's Windows 11 machine, feeding the ADR-0002 recommendation and the DES-0011/DES-0014/DES-0015 design inputs.
tags: [trace, spike, adr-0002, uia, capture, determinism, read-only]
timestamp: 2026-07-03T00:00:00+09:00
---

# TRC-0001 ADR-0002 Spike Measurement Evidence

Cross-phase trace evidence for the `RSK-RD-001` adapter-technology spike (issue #30). PoCs and harness: [spikes/adr-0002/](../../spikes/adr-0002/README.md); synthesized decision: [ADR-0002](../decisions/adr-0002-adapter-technology-selection.md).

## Trace Block

| Field | Content |
| -- | -- |
| Artifact | `TRC-0001`, ADR-0002 Spike Measurement Evidence, architecture phase (spike) |
| Upstream | [DES-0007](../design/des-0007-detailed-design-execution-strategy.md) §4.2 (spike definition, exit criteria) / §8 (comparison axes); `RQ-048`, `RQ-049`, `RQ-050`, `RQ-051`; `RD-001`, `RD-003`, `RD-023`, `RD-026`; spike scaffold PR #78, harness fix PR #79 |
| Downstream | [ADR-0002](../decisions/adr-0002-adapter-technology-selection.md) recommendation and promotion; threading/async feed to `DES-0011`; lazy-realization and capture-failure-mode feeds to `DES-0014`/`DES-0015` |
| Evidence | Per-target harness runs (JSON + summary under `spikes/adr-0002/measurement/results/`, git-ignored; key numbers reproduced below), per-axis pass/fail tables, reproduction steps |
| Verification | `Measure-Spike.ps1` executed by the human owner (TortoiseGit target) and by Claude Code on the same machine (supplementary targets); determinism verified across fresh processes |
| Residual Risk | Single machine, single primary DPI (144); elevated-target and mixed-DPI behavior not measured; `uiAccess`/MSIX packaging not exercised; yellow-border/foregrounding observation relies on the human owner's visual confirmation — all carried into ADR-0002 |

## Environment

- Windows 11 (`10.0.22621`), .NET 10.0.9 (SDK 10.0.301), 64-bit; primary monitor DPI 144 (150%).
- All PoCs run unpackaged, same integrity level as the targets, normal user, no elevation prompts.
- Harness: `spikes/adr-0002/measurement/Measure-Spike.ps1` (UIA candidates twice each in fresh processes; captures once each).

## Targets and runs

| Target | Technology / relevance | Run id (results dir) | Run by |
| -- | -- | -- | -- |
| T1: TortoiseGit window (~48 UIA elements) | Real C++/MFC application — the representative legacy target class | `20260703-033529` | Human owner |
| T2: Visual Studio Code main window (1029 elements realized) | Chromium/Electron — large-tree performance + lazy-accessibility case | `20260703-033806`, post-warm `20260703-raw-postwarm` | Claude Code (same machine) |
| T3: UWP Settings (`ApplicationFrameHost`) and `Program Manager` (shell) | WGC failure-mode probes | `20260702-233407`, scaffold smoke 2026-07-02 | Claude Code (same machine) |

## Raw numbers

| Candidate | Target | Run | Elapsed ms | Elements | Tree SHA-256 (first 12) | Errors |
| -- | -- | -- | -- | -- | -- | -- |
| uia-raw-com | T1 | 1 | 124 | 48 | `4fda098e03f4` | 0 |
| uia-raw-com | T1 | 2 | 126 | 48 | `4fda098e03f4` | 0 |
| uia-flaui | T1 | 1 | 159 | 48 | `449d9f6f0e40` | 0 |
| uia-flaui | T1 | 2 | 153 | 48 | `449d9f6f0e40` | 0 |
| capture-printwindow | T1 | 1 | 40 | — | — | 0 |
| capture-wgc | T1 | 1 | 411 | — | — | 0 |
| uia-raw-com | T2 | 1 (first touch) | 140 | **19** | `9689d30d8e68` | 0 |
| uia-raw-com | T2 | 2 | 1652 | 1029 | `7799cb27a23a` | 0 |
| uia-raw-com | T2 | post-warm 1 | 1596 | 1029 | `7799cb27a23a` | 0 |
| uia-raw-com | T2 | post-warm 2 | 1664 | 1029 | `7799cb27a23a` | 0 |
| uia-flaui | T2 | 1 | 1788 | 1029 | `2e0b3f510017` | 0 |
| uia-flaui | T2 | 2 | 1871 | 1029 | `2e0b3f510017` | 0 |
| capture-wgc | T3 Settings | 1 | 435 | — | — | `ArgumentException` from `CreateForWindow` |
| capture-wgc | T3 Program Manager | 1 | — | — | — | `ArgumentException` from `CreateForWindow` |

Tree hashes are comparable **within** a candidate only (the two candidates encode defaults differently by design). `Name` text is SHA-256-hashed inside the canonical dump — no raw sensitive text in any committed evidence (`RQ-052`); captured PNGs remain local and uncommitted.

## Per-axis results

| Axis | uia-raw-com | uia-flaui | capture-printwindow | capture-wgc |
| -- | -- | -- | -- | -- |
| Read-only (`RQ-048`) | **PASS** — API surface is read-only (ElementFromHandle, RawViewWalker, `Current*` reads); tree hash unchanged across four consecutive runs on an idle target | **PASS** — same basis (wrapper over the same read APIs); formal wrapper-internals audit deferred to `UT-0005` spy design | **PASS with caveat** — `PrintWindow` sends a WM_PRINT-style render request *into* the target (target repaints); no state change observed, but this inherent property is recorded for `DES-0015` | **PASS** — compositor-side; no message into the target, no foregrounding observed |
| Determinism (`RQ-051`) | **PASS** — byte-identical canonical-tree hash across fresh processes on T1 and on T2 post-realization | **PASS** — same on T1 and T2 | n/a (metadata deterministic; image bytes out of scope) | n/a (same) |
| Fixtureability | **PASS** — COM interfaces mockable at the adapter seam; port-level fakes unaffected; T2's lazy-realization case is directly expressible as a synthetic `NotRealized` fixture | **PASS** — same at the port seam; extra wrapper layer to fake inside the adapter | **PASS** — trivial port fake | **PASS** — port fake trivial; live path needs D3D interop (already scaffolded) |
| Permissions / integrity (`RQ-049`) | **PASS (same-integrity)** — no prompts, `PermissionNotes` empty on all targets | same | same | same. **Elevated target: NOT MEASURED** (carried) |
| Packaging | **PASS (unpackaged)** — all PoCs ran as plain unpackaged self-contained exes | same | same | same. **MSIX / `uiAccess` signed build: NOT MEASURED** (carried) |
| Performance (`RQ-050`) | 48 elem / ~125 ms; 1029 elem / ~1.6 s | 48 elem / ~156 ms; 1029 elem / ~1.8 s (≈ +15%) | 40 ms/frame | ~400 ms first frame (pool warm-up); subsequent frames cheap |

## Findings feeding downstream designs

1. **Lazy accessibility-tree realization (→ `DES-0014`, `R-GTA-02`)**: on the Chromium target, the *first ever* UIA touch returned a 19-element skeleton in 140 ms; the full 1029-element tree materialized from the second touch onward and was then byte-stable across fresh processes. Determinism holds **post-realization**; the acquisition design needs a first-acquisition warm-up/short-settle rule and must classify unrealized subtrees as `Unavailable(NotRealized)`, never as absence.
2. **DPI virtualization (→ `DES-0015`, `R-WIN-01`)**: the non-DPI-aware PoC process received virtualized bounds 455×537 from `GetWindowRect` and captured PrintWindow at that reduced resolution, while WGC captured the same window at physical 664×796. Direct evidence that the analyzer must be Per-Monitor-V2 aware and normalize bounds to the target DPI context.
3. **WGC uncapturable windows (→ `DES-0015` failure-mode table)**: `IGraphicsCaptureItemInterop.CreateForWindow` throws `ArgumentException` for the shell window (`Program Manager`) and for the UWP Settings window hosted by `ApplicationFrameHost` (window state at the time not recorded — possibly minimized; minimized windows are a known WGC limitation). Both map to `Unavailable(reason)` + PrintWindow fallback.
4. **Threading/async (→ `DES-0011`)**: both UIA candidates ran correctly from an **MTA** thread (console default) with no cross-thread marshaling errors; WGC frame arrival is asynchronous with a ~400 ms first-frame budget on this machine — the capture-stage timeout must accommodate pool warm-up. Cancellation behavior on a hung target was not exercised (carried to `DES-0014`/`IT-0006`).
5. **Raw COM vs FlaUI**: identical read behavior and determinism; raw COM ~15–20% faster and gives direct HRESULT visibility; FlaUI's only measured advantage is API ergonomics.

## Reproduction steps

```powershell
dotnet build spikes/adr-0002/Spike.Adr0002.slnx
powershell -NoProfile -File spikes/adr-0002/measurement/Measure-Spike.ps1 -TargetTitle "<substring>"
# per-run JSON + summary.md appear under spikes/adr-0002/measurement/results/<stamp>/
```

Determinism check = the two per-candidate hashes in `summary.md` must match (target idle). Lazy-realization reproduction: pick a Chromium/Electron window that has never been touched by an AT client since launch; the first run undercounts, subsequent runs are stable.

## Open items (carried into ADR-0002 residual risk)

- Elevated-target failure-mode calibration (feeds `PermissionDenied`/`IntegrityMismatch` statuses in `DES-0011`); requires a UAC-elevated target — human-run.
- Mixed-DPI monitor layout; `uiAccess` signed-build and MSIX packaging exercises.
- Yellow capture border / consent visual observation during WGC runs (human visual confirmation).
- Legacy-edge catalogue (owner-draw, MSAA-only proxies, MDI, `WM_GETTEXT`) — owned by `DES-0014` with the IT fixture app, not blocking the technology pick.
