# ADR-0002 spike results (per-axis record)

> Copy this file per measured target (e.g. `results/<target>-<date>.md`), fill every axis
> with pass/fail + evidence, and link the `results/<stamp>/` JSON directory produced by
> `Measure-Spike.ps1`. This filled record is the acceptance evidence required by
> `DES-0007` §4.2 exit criteria and feeds the final `ADR-0002` synthesis.

## Target

- Application / window (masked description, no raw confidential titles):
- Technology (Win32/MFC/WinForms/other), bitness, integrity level:
- Environment: Windows version / DPI (per monitor) / monitor layout:
- Run directory (JSON + summary.md):

## Axis results

| Axis | uia-raw-com | uia-flaui | capture-printwindow | capture-wgc |
| -- | -- | -- | -- | -- |
| Read-only feasibility (state unchanged before/after; API list is read-only) | pass/fail + evidence | pass/fail + evidence | pass/fail + evidence | pass/fail + evidence |
| Determinism (same idle target, fresh processes → identical tree hash / stable capture metadata) | | | n/a (image bytes vary) | n/a (image bytes vary) |
| Fixtureability (can the observed behavior be reproduced with a synthetic fixture/fake?) | | | | |
| Permissions / integrity (same-integrity target vs elevated target behavior) | | | | |
| Packaging (works unpackaged? MSIX constraints? signing needs — see packaging/README.md) | | | | |
| Performance (elapsed ms, element count / frame latency on the largest real target) | | | | |

## Live observations to feed DES-0011 (threading/async)

- Apartment (STA/MTA) behavior and any cross-thread marshaling errors:
- Cancellation/timeout behavior on a hung or slow target:
- Event vs polling behavior (WGC frame arrival; UIA property read latency):

## Legacy-edge observations to feed DES-0014/DES-0015

- MSAA-only / owner-draw / windowless controls encountered and how each candidate reported them:
- Capture failure modes hit (black frame, layered window, WGC-uncapturable window such as Program Manager):
- High-DPI / mixed-DPI coordinate observations:

## Read-only before/after check (manual)

Record for the target: focus, selection, scroll position, window position/z-order,
text content, toggle states — before and after each PoC run. Any change = fail.

| Check | Before | After | Unchanged? |
| -- | -- | -- | -- |
| Focused control | | | |
| Selection / caret | | | |
| Scroll positions | | | |
| Window pos / z-order | | | |
| Visible text / data | | | |

## Verdict

- Recommended UIA candidate + rationale:
- Recommended capture candidate + rationale:
- Packaging implication:
- Residual risks:
