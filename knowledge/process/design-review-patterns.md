---
type: Process
title: Design Review Pattern Catalog
description: Living catalog of recurring design-defect patterns (DRP-xxx) used as an author-side self-check and a reviewer-side checklist for Surveyor detailed design.
tags: [process, design-review, detailed-design, patterns, self-check]
timestamp: 2026-07-03T00:00:00+09:00
---

# Design Review Pattern Catalog

This catalog turns individual review findings into reusable defect *patterns* so the same defect class is caught once per project, not once per document. Each pattern has a stable `DRP-xxx` ID, a detection question a reviewer can answer mechanically, and an author-side prevention rule.

It is used at three points, defined in [AI Design Review Strategy](ai-design-review-strategy.md):

- **Authoring (L1)**: the design author sweeps every pattern before opening the PR and records the result as Self-Review Evidence in the PR body.
- **Review (L2)**: the reviewer checks every pattern explicitly and tags findings with the matching `DRP-xxx` ID and severity.
- **Fix response (L3)**: a fix that reshapes a contract boundary re-runs the closure patterns (`DRP-02`–`DRP-05`) on that boundary before re-review is requested.

## Growth Rule

This is a living document. When a design review (or a later phase) finds a **Critical or High** defect that no existing pattern covers, `surveyor-knowledge-curator` generalizes it into a new `DRP-xxx` entry with the evidence link, in the same PR or a follow-up `docs/` PR. Patterns are never deleted; obsolete ones are marked superseded. Content-specific findings (wording, one-off values) do not become patterns — only defect *classes* that can recur in another document do.

## Patterns

| ID | Pattern | Detection question | Author-side prevention |
| -- | -- | -- | -- |
| `DRP-01` | Upstream drift | Does the design rename, merge, split, or drop any module, use case, port, state, or decision fixed by an upstream `DES`/`ADR` without a §5.3-style supersede note? | Before writing, list the upstream inventory (use cases, ports, states) the package touches; diff your design against it; any intentional change gets an explicit supersede/version note |
| `DRP-02` | Dangling reference | Is every type, config key, enum, and artifact ID referenced in API blocks, sequence diagrams, or prose also defined (fields, invariants, canonical home) somewhere in the design set? | Extract the list of referenced type names from your own document and resolve each to a definition before submitting |
| `DRP-03` | Data-flow closure | For every use-case/port method: can each input be derived from caller input, prior stage output, or persisted state reachable through a defined contract? Does each output have a named consumer? | Write the Contract Closure table (DES-0007 §6); simulate each use case end-to-end on paper from its trigger to its outputs |
| `DRP-04` | Round-trip asymmetry | For every save/load, serialize/deserialize, mask/unmask, encrypt/decrypt, or export/import pair: are the two directions defined against symmetric types and semantics, including failure cases? | Name every round-trip pair explicitly; define both directions in the same section with the same document/type vocabulary |
| `DRP-05` | Unowned field | Does every DTO field have exactly one named writer, a defined write timing, and — if the value is duplicated across models — a stated synchronization/consistency rule and a fabrication ban for consumers? | Fill the field-ownership rows of the Contract Closure table for every DTO the package introduces |
| `DRP-06` | Rule overlap without precedence | Can two classification/branching rules match the same input? If so, is there an ordered first-match-wins decision list, and is every row reachable? | Express multi-condition classifications as an ordered decision table; check reachability of each row |
| `DRP-07` | Numeric under-specification | Is every score/threshold/aggregation computation pinned to the project numeric rule (integer basis-point arithmetic, defined rounding, no floating point in decision paths, `RQ-051`)? | State the arithmetic domain and rounding of every formula; never leave "percentage" or "weight" as an unqualified real number |
| `DRP-08` | Missing failure semantics | Does every I/O boundary define atomicity, cleanup on failure/cancel, and destination-collision policy? Does every async operation define cancellation-vs-timeout precedence and how the cause is distinguished? | Fill the edge-case table (DES-0007 §6) for each I/O boundary before writing the happy path in detail |
| `DRP-09` | Port ownership ambiguity | Does every interface name its owner layer, and do implementations depend inward on application-owned ports (never the reverse)? Is each abstraction's canonical home unique? | Declare owner layer and implementing layer on every interface at its definition site |
| `DRP-10` | Patch regression | Does a review fix reshape a contract boundary (types, ownership, call sequence)? If so, were `DRP-02`–`DRP-05` re-run on the reshaped boundary, not just on the reported symptom? | Classify every fix as *local* or *boundary-reshaping*; boundary-reshaping fixes get a closure re-sweep and a contract-diff summary in the reply |

## Evidence

Seed evidence is the PR [#81](https://github.com/urario/Surveyor/pull/81) review rally on `DES-0010`/`DES-0011`/`DES-0013` (four review rounds, 2026-07-03):

| Pattern | PR #81 instance |
| -- | -- |
| `DRP-01` | `AnalyzeTargetUseCase` consolidation contradicted the four-use-case split fixed by `DES-0002`/`DES-0003`/`DES-0004` (round 1, blocking) |
| `DRP-02` | `ProtectedRunModel`, `MaskedExportModel`, `SafeArtifactReference`, retention/export DTOs referenced but undefined; `ScoringConfigReference` had no resolver (round 1) |
| `DRP-03` | `ExportResultUseCase` received `RunId` but had no defined path to obtain the persisted `AnalysisRunResult` (round 2, Critical — survived the round-1 sweep because no closure table existed) |
| `DRP-04` | Store saved `ProtectedRunModel` but load returned a bare `AnalysisRunResult`; resolved by the symmetric `StoredRunSnapshot` contract (round 3, Critical — introduced by the round-2 fix) |
| `DRP-05` | No owner or sync rule for `ConfidentialityDecision` across `PolicyApplicationResult`, sanitized result, and store metadata (round 4, Medium — surfaced by the round-3 fix) |
| `DRP-06` | `TestabilityClass` conditions overlapped with no precedence; fixed as an ordered decision list with a reachability correction in a later round (rounds 1–2) |
| `DRP-07` | Axis weights and aggregate rounding were not pinned to integer basis-point arithmetic (round 1) |
| `DRP-08` | Export ZIP atomic write, failure/cancel cleanup, destination collision, and timeout-vs-cancellation precedence were undefined (round 1) |
| `DRP-09` | `IConfidentialityPolicy` was defined ambiguously between `Surveyor.Policy` and application ports, inverting the dependency direction (round 1, blocking) |
| `DRP-10` | Rounds 2–4 were each triggered by the previous round's fix reshaping the store/export boundary without a closure re-sweep |

## Related

- [AI Design Review Strategy](ai-design-review-strategy.md)
- [DES-0007 Detailed Design Phase Execution Strategy](../design/des-0007-detailed-design-execution-strategy.md)
- [Quality Review Policy](quality-review-policy.md)
- [Lifecycle Traceability](lifecycle-traceability.md)
