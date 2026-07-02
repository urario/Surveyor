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

## Design Reviews

- [DES-0007 Multi-Perspective Expert Review](des-0007-review-multiperspective.md) - Independent 11-perspective expert review of the DES-0007 execution strategy with 31 structured, disposition-tracked findings (Critical 1 / High 12 / Medium 14 / Low 4); does not modify the reviewed artifact.

