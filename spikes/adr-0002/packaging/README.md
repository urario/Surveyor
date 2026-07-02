# ADR-0002 packaging comparison notes (desk analysis + verification steps)

Packaging is the third decision dimension of `ADR-0002` (besides UIA client and capture
API). This note records the desk analysis the AI can do and the verification steps that
need the human owner's environment (signing, real elevated targets).

Binding upstream decisions ([DES-0007](../../../knowledge/design/des-0007-detailed-design-execution-strategy.md) §8.1, `R-SEC-02`):
same-integrity by default, no admin/`uiAccess` requirement; elevation/`uiAccess` is an
explicit opt-in (signed build) only when a target requires it.

## Candidates

| Form | Pros | Cons / risks | Interaction with the other picks |
| -- | -- | -- | -- |
| Unpackaged (xcopy / plain installer) + Windows App SDK self-contained | Simplest debugging; no identity requirements; works on locked-down machines; `uiAccess` possible via classic signed-manifest install under `Program Files` | Manual servicing; Windows App SDK deployment must be bundled (self-contained) or bootstrapped | WGC and UIA both work unpackaged; PrintWindow has no packaging constraint |
| MSIX (packaged, signed) | Clean install/uninstall, integrity of binaries, store-quality servicing | Requires cert management; `uiAccess=true` is **not** available to MSIX-packaged desktop apps in the classic manifest sense; some enterprise policies block sideloading | If `uiAccess` opt-in is ever needed for elevated targets, MSIX packaging conflicts with it — pushes toward unpackaged for the analyzer executable |
| Hybrid (unpackaged analyzer core + optional MSIX distribution without `uiAccess`) | Keeps the default same-integrity path MSIX-clean while preserving the signed unpackaged escape hatch | Two distribution paths to maintain | Matches the `R-SEC-02` "opt-in only when a target requires it" decision |

## What the human owner must verify (cannot be desk-decided)

1. **Same-integrity default**: run each UIA/capture PoC unpackaged against a normal-integrity
   legacy target — record that no elevation prompt or failure occurs.
2. **Elevated target behavior**: run against an elevated (admin) target — record the exact
   failure mode (UIA property gaps vs outright access denied) per candidate. This calibrates
   the `PermissionDenied`/`IntegrityMismatch` statuses of `DES-0003`/`DES-0011`.
3. **`uiAccess` path (only if 2 shows it is needed)**: signed build installed under
   `Program Files` with `uiAccess=true`; verify UIA reads that failed in 2 now succeed.
   Requires a signing certificate — availability is a recorded residual risk of issue #30.
4. **WGC consent/border**: record whether the yellow capture border appears for an
   unpackaged app on the tested Windows 11 build and whether any capability prompt occurs
   (feeds the user-facing confidentiality notice design, `RQ-052`).
5. **MSIX smoke (optional)**: package one capture PoC as MSIX and re-run 1 and 4.

## Evidence home

Record outcomes in `measurement/results-template.md` (Packaging axis) and link them from
the final `ADR-0002` synthesis.
