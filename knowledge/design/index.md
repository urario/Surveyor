# Design Knowledge

Use this directory for durable basic design and detailed design artifacts that are not better represented as an ADR or architecture note.

Follow [Lifecycle Traceability](../process/lifecycle-traceability.md):

- Use `DES-xxxx` for durable design artifacts.
- Link each design artifact to relevant `RQ-xxx` IDs and upstream ADRs or architecture notes.
- Include the required trace block when the design creates downstream implementation or test obligations.
- Keep transient design discussion in PRs or task notes unless it needs to survive as project knowledge.

## Basic Design Artifacts

- [DES-0002 Module Responsibility Basic Design](des-0002-module-responsibility-basic-design.md) - 13-module responsibility map, ownership layers, data ownership, and guardrail assignment refining `DES-0001`.
- [DES-0003 Module Interface Basic Design](des-0003-module-interface-basic-design.md) - Review-grade contracts for every port and use case: direction, I/O, result/error model, cancellation, read-only/determinism/confidentiality constraints, fake strategy, and RQ/RD/UT/IT trace.
- [DES-0004 Analysis Flow Basic Design](des-0004-analysis-flow-basic-design.md) - End-to-end run flow, run state machine, staged contracts, cancellation/partial-result rules, and guardrail checkpoints.
- [DES-0005 V-Model Traceability and Downstream Test Design Obligations](des-0005-vmodel-traceability-and-downstream-tests.md) - Basic-design item to detailed-design/implementation/UT/IT map, RQ/RD to DES to UT/IT trace, planned test obligations, and Codex slice candidates.
- [DES-0006 Screen (Operating UI) Basic Design](des-0006-screen-basic-design.md) - Surveyor's own WinUI operating-UI design: screen inventory (`SCR-01`-`SCR-08`), navigation/transition keyed to the run state machine, per-screen item-to-`AnalysisResult` bindings, the native-vs-HTML review decision, snapshot correspondence, confidentiality-choice and status/error surfaces, and usability principles.

## Detailed Design Artifacts

- [DES-0007 Detailed Design Phase Execution Strategy](des-0007-detailed-design-execution-strategy.md) - OKF-hosted execution strategy for detailed design, including package order, trace rules, artifact template, unit-test intent strategy, and residual-risk closure map.
- [DES-0008 Project Structure and Test Harness Detailed Design](des-0008-project-structure-and-test-harness.md) - Detailed-design package 1: the one solution / ten `src` projects / test-project layout, `M01`–`M13` project homes, the inward dependency rule enforced by the `ProjectReference` graph plus a `NetArchTest`/banned-API architecture test, central determinism/quality MSBuild settings (`R-NET-02`), synthetic UT fixture-tree placement, and the mixed (WinForms + real MFC/Win32) integration fixture-app harness (`R-OPS-03`) with unit/integration CI lanes (`R-OPS-01`).
- [DES-0009 Domain Model, Stable Keys, and Availability Detailed Design](des-0009-domain-model-stable-keys-and-availability.md) - Detailed-design package 2 (`M04`/`M11`): the `ScreenModel`/`UiElement` value-object catalogue with invariants, the identity-source ladder and canonical SHA-256/`Ordinal` key derivation separated from `DisplayLabel` (`R-NET-01`), the front-loaded fallback-key minimal contract finalized at model construction closing `RSK-DES-002` (`R-IMP-01`), `Unavailable(reason)`/confidence semantics distinct from low scores, the `IClock` abstraction, and `UT-0001`/`UT-0008` test intent with counter-example fixtures.

- [DES-0010 Scoring, Classification, and Improvement Candidate Detailed Design](des-0010-scoring-classification-and-improvement-candidates.md) - Detailed-design package 3 (`M08`): deterministic seven-axis scoring, UIA/MSAA signal mapping (`R-GTA-01`), versioned scoring config (`R-MNT-01`), basis-point rounding, class thresholds, unavailable-vs-low-score semantics, root-cause de-duplication, and improvement candidate generation without fabricated priority (`RD-016`).
- [DES-0011 Port DTOs, Status Model, and Use-Case Orchestration Detailed Design](des-0011-port-dtos-status-model-and-use-case-orchestration.md) - Detailed-design package 4 (`M03`/`M11`): application-layer DTOs, status enums, timeout/cancellation rules, partial-result aggregation, sanitized run diagnostics (`R-ARC-03`), ROI handoff, unchanged `ScreenSelectionMetadata` threading, and concrete `IClock` usage for `UT-0012` and use-case wiring.
- [DES-0012 Report Schema and Deterministic Serialization Detailed Design](des-0012-report-schema-and-deterministic-serialization.md) - Detailed-design package 5 (`M10`): versioned JSON schema, fixed HTML section outline, explicit property/order/culture/timestamp/encoding rules (`R-NET-03`), atomic report writes, and golden-file governance (`R-QA-02`) for `UT-0006`/`UT-0007`/`UT-0010`.
- [DES-0013 Confidentiality, Storage, and Export Detailed Design](des-0013-confidentiality-storage-and-export.md) - Detailed-design package 6 (`M09`/`M12`): secure-by-default masking/redaction, explicit opt-out record, fallback-key export pseudonyms, `%LOCALAPPDATA%` run storage with DPAPI `CurrentUser` plus user ACLs (`R-SEC-02`), retention, sanitized diagnostics/exceptions (`R-SEC-01`), and policy-gated masked ZIP export.
- [DES-0014 Discovery, UIA/MSAA Acquisition, and Read-Only Audit Detailed Design](des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) - Detailed-design package 7 (`M05`/`M06`): read-only discovery field detail conforming to the `DES-0011`-fixed `TargetReference`/`TargetCandidate`, an adapter-internal opaque-handle mechanism, and within-session ordering; raw-COM UIA acquisition (`ADR-0002`) behind an `IUiaComReader` seam with MSAA/bounded-`WM_GETTEXT` legacy fallbacks; UIA MTA threading with a COM-level call budget plus cooperative cancellation (`R-WIN-01`/`R-WIN-02`); the `RD-032` prohibited-pattern→COM-method read-only audit with a concrete allow-list and spy (`RQ-048`/`UT-0005`); the legacy acquisition edge table (`R-WIN-03`); virtualized-tree `NotRealized` handling (`R-GTA-02`); the confidence rubric; rung-1 runtime-id detection (delegated by `DES-0009`); and the minimal-privilege policy (`R-SEC-02`).

## Design Reviews

- [DES-0007 Multi-Perspective Expert Review](des-0007-review-multiperspective.md) - Independent 11-perspective expert review of the DES-0007 execution strategy with 31 structured, disposition-tracked findings (Critical 1 / High 12 / Medium 14 / Low 4); does not modify the reviewed artifact.

