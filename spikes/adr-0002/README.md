# ADR-0002 technology-selection spike (issue #30)

PoC scaffold + measurement harness for the `RSK-RD-001` adapter-technology spike
([DES-0007](../../knowledge/design/des-0007-detailed-design-execution-strategy.md) §4.2).
Hybrid ownership: **AI scaffolds** (this directory), the **human owner runs** the PoCs
against real legacy targets and records live behavior, **AI synthesizes** the results into
the final `ADR-0002` (draft: [adr-0002-adapter-technology-selection.md](../../knowledge/decisions/adr-0002-adapter-technology-selection.md)).

This code is **spike-grade and throwaway**: it never ships, is not referenced by any
future `src/**` project ([DES-0008](../../knowledge/design/des-0008-project-structure-and-test-harness.md)
layout), and its only job is producing comparable per-axis evidence.

## Candidates

| Dimension | Candidate A | Candidate B |
| -- | -- | -- |
| UIA client | `UiaRawComPoc` — raw COM via `Interop.UIAutomationClient` PIA | `UiaFlaUiPoc` — FlaUI.UIA3 wrapper |
| Capture | `CapturePrintWindowPoc` — GDI `PrintWindow(PW_RENDERFULLCONTENT)` | `CaptureWgcPoc` — `Windows.Graphics.Capture` |
| Packaging | see [packaging/README.md](packaging/README.md) (unpackaged vs MSIX vs hybrid) | |

## Build and run

```powershell
dotnet build spikes/adr-0002/Spike.Adr0002.slnx        # .NET 10 SDK, Windows
# one PoC directly:
src\UiaRawComPoc\bin\Debug\net10.0-windows10.0.19041.0\UiaRawComPoc.exe --title "<window title substring>"
# full measured run (all PoCs, UIA candidates twice for the determinism axis):
powershell -File spikes/adr-0002/measurement/Measure-Spike.ps1 -TargetTitle "<substring>"
```

Every PoC accepts `--title <substring>` / `--hwnd <decimal>` / `--hwnd-hex <hex>` and
`--out <dir>`, and writes a `MeasurementReport` JSON (candidate, elapsed ms, element
count, canonical-tree SHA-256, unavailable count, API-call list, errors, notes).

## Comparison axes → where each is measured

| Axis (DES-0007 §4.2/§8) | How this scaffold measures it |
| -- | -- |
| Read-only feasibility | Each report lists every target-facing API used (read APIs only); the manual before/after state check in `measurement/results-template.md` verifies the target unchanged (IT-0001-style) |
| Determinism | UIA PoCs dump a canonical tree (Name text is SHA-256-hashed, never raw — `RQ-052`) and hash it; `Measure-Spike.ps1` runs each candidate twice in fresh processes and compares hashes. Hashes are comparable **within** a candidate, not across candidates |
| Fixtureability | Recorded per target in the results template: can the observed edge be reproduced synthetically? |
| Permissions / integrity | PoCs record per-node/root COM failures; elevated-target runs per `packaging/README.md` step 2 |
| Packaging | Desk analysis + human verification steps in `packaging/README.md` |
| Performance | Elapsed ms + element count (20k element cap) / frame latency in each report |

## Known observations from the scaffold smoke run (dev machine, 2026-07-02)

- All four PoCs build (`0 warnings`) and ran read-only against desktop windows.
- WGC `CreateForWindow` throws `ArgumentException` for `Program Manager` (shell window) —
  a real capture failure mode to carry into the `DES-0015` failure-mode table; it captured
  a normal app window fine (~375 ms first frame, no foregrounding).
- PrintWindow captured 2560x1440 @ DPI 144 in ~137 ms; black-frame heuristic included.
- UIA raw COM and FlaUI both traversed with stable canonical hashes per candidate.

## What remains for the human owner (exit criteria, DES-0007 §4.2)

1. Run `Measure-Spike.ps1` against the real legacy targets (large trees, MSAA-only /
   owner-draw / MDI edges, high-DPI and mixed-DPI monitors, elevated target).
2. Fill `measurement/results-template.md` per target (pass/fail per axis + evidence).
3. Perform the packaging verification steps (`packaging/README.md`).
4. Hand the filled results back; AI synthesizes them into `ADR-0002` and the
   threading/async findings into `DES-0011`; the human approves and promotes `ADR-0002`.

Captured images and any raw window text are **confidential by default** (`RQ-052`):
result JSON/summary contain only hashed names; do not commit or share captured PNGs.
`results/` output is git-ignored.
