---
okf_version: "0.1"
type: Knowledge Bundle
title: Surveyor Knowledge Bundle
description: OKF-style project knowledge for the Surveyor GUI testability analyzer.
tags: [surveyor, okf, traceability]
timestamp: 2026-06-30T00:00:00+09:00
---

# Surveyor Knowledge Bundle

This bundle keeps project knowledge close to the source code so agents and humans can trace requirements, decisions, tests, and implementation work.

# Requirements

* [Requirement Source](requirements/source-spec.md) - Canonical requirements document and trace rules.
* [Requirement Definition](requirements/requirements-definition.md) - Derived `RD-xxx` requirement definitions and downstream trace guidance.
* [Generated RQ Index](requirements/rq-index.generated.md) - Generated list of `RQ-xxx` headings.

# Architecture

* [Layering Principles](architecture/layering-principles.md) - Required layer split and design guardrails.
* [DES-0001 Initial Architecture](architecture/des-0001-initial-architecture.md) - Clean Architecture and MVVM design, ports, technology allocation, and downstream slices for the initial version.

# Design

* [Design Knowledge](design/index.md) - Basic and detailed design artifact home and lifecycle rules.
* [DES-0002 Module Responsibility Basic Design](design/des-0002-module-responsibility-basic-design.md) - Module responsibilities, ownership layers, and guardrail assignment for the initial version.
* [DES-0003 Module Interface Basic Design](design/des-0003-module-interface-basic-design.md) - Basic-design contracts for every port and use-case boundary.
* [DES-0004 Analysis Flow Basic Design](design/des-0004-analysis-flow-basic-design.md) - Run flow, state machine, staged contracts, and guardrail checkpoints.
* [DES-0005 V-Model Traceability and Downstream Tests](design/des-0005-vmodel-traceability-and-downstream-tests.md) - V-model mapping and planned unit/integration test obligations.
* [DES-0006 Screen (Operating UI) Basic Design](design/des-0006-screen-basic-design.md) - Operating-UI screen inventory, navigation, per-screen bindings, review-surface decision, snapshot correspondence, and usability principles.
* [DES-0007 Detailed Design Phase Execution Strategy](design/des-0007-detailed-design-execution-strategy.md) - Detailed-design execution order, OKF storage decision, trace template, unit-test intent strategy, and residual-risk closure map.
* [DES-0008 Project Structure and Test Harness Detailed Design](design/des-0008-project-structure-and-test-harness.md) - Solution/project layout, assembly boundaries, namespaces, inward dependency rule with the narrow UIA→Discovery resolver exception and mechanical verification, determinism/quality build settings, unit fixture-tree placement, and the mixed integration fixture-app harness.
* [DES-0009 Domain Model, Stable Keys, and Availability Detailed Design](design/des-0009-domain-model-stable-keys-and-availability.md) - ScreenModel/UiElement value objects, stable ScreenKey/ElementKey derivation separated from DisplayLabel, the fallback-key minimal contract finalized at model construction, availability/confidence semantics, the stable-hash/ordinal determinism rule, and the IClock abstraction.
* [DES-0010 Scoring, Classification, and Improvement Candidate Detailed Design](design/des-0010-scoring-classification-and-improvement-candidates.md) - Deterministic scoring pipeline, axis-to-UIA/MSAA signal mapping, versioned scoring configuration, classification thresholds, root-cause de-duplication, and no-priority improvement candidate generation.
* [DES-0011 Port DTOs, Status Model, and Use-Case Orchestration Detailed Design](design/des-0011-port-dtos-status-model-and-use-case-orchestration.md) - Application-layer DTOs, statuses, timeout/cancellation handling, partial-result aggregation, sanitized diagnostics, ROI handoff, metadata carriage, and `IClock` usage.
* [DES-0012 Report Schema and Deterministic Serialization Detailed Design](design/des-0012-report-schema-and-deterministic-serialization.md) - M10 report schema and writer design: versioned JSON, required HTML sections, deterministic serialization, atomic report writes, and governed report goldens.
* [DES-0013 Confidentiality, Storage, and Export Detailed Design](design/des-0013-confidentiality-storage-and-export.md) - Secure-by-default confidentiality policy, masking/redaction, fallback-key export policy, DPAPI CurrentUser local storage, user ACLs, retention, sanitized diagnostics, and policy-gated masked exports.
* [DES-0014 Discovery, UIA/MSAA Acquisition, and Read-Only Audit Detailed Design](design/des-0014-discovery-uia-msaa-acquisition-and-read-only-audit.md) - Read-only discovery and raw-COM UIA/MSAA acquisition design conforming to the DES-0011-fixed shapes: Discovery-owned opaque registry plus outward-only UIA resolver, ordering, MTA threading/cancellation, read-only audit, legacy/virtualized edges, confidence, runtime-id detection, and minimal privilege.
* [DES-0015 Capture and Snapshot Correspondence Detailed Design](design/des-0015-capture-and-snapshot-correspondence.md) - Windows.Graphics.Capture-primary/PrintWindow-fallback capture design conforming to the DES-0011-fixed `CaptureRequest`/`CaptureResult`/`RegionOfInterest` shapes: the physical-pixel coordinate contract closing DES-0009's `BoundingRect` DPI-normalization delegation (backed by the TRC-0001-measured virtualization finding), the pure `BoundingRect`→`RectangleDip` overlay mapping, the `SnapshotRef` derived-projection population rule, the capture failure-mode table, and multi-monitor/occlusion/offscreen handling.
* [DES-0016 Operating UI Detailed Design](design/des-0016-operating-ui-detailed-design.md) - WinUI operating-UI detailed design binding `M01`/`M02` to the stable application contracts: concrete navigation/dialog intent enums, the run-UI state machine with stage progress, the SCR-03 metadata gate, the deterministic SCR-05↔SCR-06 correspondence state, the confidentiality opt-out recording surface, the HTML preview host decision (external default browser, WebView2 deferred), the accessibility target, and the UT-0011/IT-0007 handoff.
* [DES-0018 Composition Root and DI Detailed Design](design/des-0018-composition-root-and-di.md) - `M13` composition-root design: explicit MEDI registration seams, headless core/production split, complete wiring table, `ITargetFacingPort`-based sanctioned-set enforcement plus clock/policy/test-clock invariants, Discovery/UIA resolver ownership, sanitized diagnostics, production-registration smoke, and the `UT-0013` (#52) / `IMP-0015` (#73) handoff.

# Decisions

* [ADR-0001 AI Collaboration and OKF](decisions/adr-0001-ai-collaboration-and-okf.md) - Initial AI role split and knowledge management decision.
* [ADR-0002 Adapter Technology Selection (accepted)](decisions/adr-0002-adapter-technology-selection.md) - Accepted adapter technology decision for the RSK-RD-001 spike with measurements integrated (TRC-0001): raw COM UIA client, Windows.Graphics.Capture primary + PrintWindow fallback, unpackaged-primary packaging; residual risks are carried to DES-0014/DES-0015/DES-0016/IT-0005.
* [ADR-0003 Review Surface - Native WinUI Primary, HTML Portable](decisions/adr-0003-review-surface-native-vs-html.md) - Native WinUI is the primary interactive review surface; the HTML/JSON report is the portable distribution artifact.

# Trace Evidence

* [Trace Evidence](traces/index.md) - Implementation, unit test, and integration test evidence notes.
* [TRC-0001 ADR-0002 Spike Measurement Evidence](traces/trc-0001-adr-0002-spike-measurements.md) - Per-axis spike measurements (real MFC target, large Chromium tree, WGC failure modes) backing the ADR-0002 recommendation.
* [IMP-0002 Scoring Skeleton Implementation](traces/imp-0002-scoring-skeleton-implementation.md) - M08 scoring skeleton implementation evidence for deterministic axis scoring, classification, root-cause de-duplication, and improvement candidate generation.
* [UT-0002 Scoring Determinism Evidence](traces/ut-0002-scoring-determinism-evidence.md) - Unit-test evidence for scoring determinism, unavailable semantics, classification boundaries, config validation, and no fabricated priority.
* [UT-0003 Discovery Ordering and Status Mapping Evidence](traces/ut-0003-discovery-ordering-evidence.md) - Unit-test evidence for deterministic discovery candidate ordering, permission/integrity/unavailable status mapping, process-name filtering, resolve semantics, and safe diagnostics.
* [UT-0008 Confidentiality Policy Behavior Evidence](traces/ut-0008-confidentiality-policy-evidence.md) - Behavior-test evidence for secure-by-default confidentiality decisions, both policy branches, sanitization, and fallback-key export pseudonyms under `DES-0013`.
* [IMP-0003 Confidentiality Policy Implementation](traces/imp-0003-confidentiality-policy-implementation.md) - M09 confidentiality policy, sensitive-value sanitizer, and fallback-key export mapper implementation evidence that turns `UT-0008` green.
* [UT-0010 Clock Determinism Evidence](traces/ut-0010-clock-determinism-evidence.md) - Behavior-test evidence for fixed-clock UTC timestamp determinism under `DES-0009`, `DES-0011`, and `DES-0012`.
* [UT-0006 Report JSON Determinism and Atomicity Evidence](traces/ut-0006-report-json-evidence.md) - Behavior-test evidence for `DES-0012` JSON byte stability, LF/no-BOM golden governance, culture/fresh-process determinism, and destination-collision no-partial writes.
* [IMP-0008 Deterministic JSON Report Writer Implementation](traces/imp-0008-report-writer-implementation.md) - Application report DTOs/port, deterministic Reports JSON writer, atomic no-partial file writes, and Reports Stryker target evidence for `DES-0012`.
* [IMP-0004 Clock Seam Implementation](traces/imp-0004-clock-seam-implementation.md) - Application-owned `IClock`, real UTC clock, fixed-clock test seam, and deterministic UTC timestamp formatting evidence for `RQ-051`.
* [IMP-0005 Discovery Port and Fake Implementation](traces/imp-0005-discovery-port-implementation.md) - Application discovery DTOs, `ITargetDiscoveryPort`, deterministic fake, solution registration, and UT-0003 closure under `DES-0011` / `DES-0014`.
* [UT-0005 Read-Only Acquisition Audit Behavior Evidence](traces/ut-0005-read-only-audit-evidence.md) - Behavior tests for the read-only audit spy under `DES-0014` (`RQ-048`), with failing-first RED evidence closed by `IMP-0007`.
* [IMP-0007 Read-Only Acquisition Audit Implementation](traces/imp-0007-read-only-audit-implementation.md) - Portable read-only acquisition audit implementation evidence that turns `UT-0005` green.
* [UT-0004 Acquisition Fixture to Model Evidence](traces/ut-0004-acquisition-fixture-evidence.md) - Behavior-test evidence for acquisition fixture tree → model confidence / `Unavailable(NotRealized)` markers, legacy edges, and the `R-QA-01` counter-example, closed by `IMP-0006`.
* [IMP-0006 Acquisition Port and Fixture Loader Implementation](traces/imp-0006-acquisition-port-implementation.md) - Application `IUiTreeAcquisitionPort` / acquisition DTOs and deterministic fixture loader implementation evidence that turns `UT-0004` green.
* [IMP-0013 Real UIA Acquisition Adapter Implementation](traces/imp-0013-uia-acquisition-adapter.md) - Real UIA acquisition adapter evidence for the opaque HWND registry, raw-reader seam, read-only audit wiring, and adapter contract tests under `DES-0014`.
* [UT-0012 Use-Case Orchestration Behavior Evidence](traces/ut-0012-use-case-orchestration-evidence.md) - Fake-based orchestration tests for stage order, cancellation, partial aggregation, policy ordering, fixed-clock timestamps, and metadata transparency, including counter-example RED evidence.
* [IMP-0011 Use-Case Orchestration Implementation](traces/imp-0011-use-case-orchestration-implementation.md) - UI-independent Application orchestration pipeline, DTO/port contracts, and quantitative verification evidence that turns `UT-0012` green.
* [UT-0011 Presentation ViewModel State Machine Evidence](traces/ut-0011-presentation-viewmodel-evidence.md) - WinUI-free ViewModel behavior tests for `DES-0016` metadata gating, run state/activity pairs, selection sync, and confidentiality preview gates.
* [IMP-0012 Presentation ViewModel and Ports Implementation](traces/imp-0012-presentation-viewmodel-implementation.md) - `Surveyor.Presentation` ports and internal ViewModel reducer implementation that turns `UT-0011` green while preserving `RQ-054`.
* [IMP-0016 Stryker.NET Baseline Implementation](traces/imp-0016-stryker-baseline.md) - CS-10 implementation evidence for the pinned Stryker.NET local tool, Domain/Policy mutation baseline scores, and below-80% improvement candidates.
* [UT-0014 Domain / Policy Mutation-Focused Coverage Evidence](traces/ut-0014-domain-policy-mutation-coverage-evidence.md) - Focused mutation-killer behavior tests for the Domain scoring config-validation / classification branches and the Policy confidentiality, sanitizer, fallback-key export, and derivation invariants under `CS-10`.

# Process

* [AI Collaboration](process/ai-collaboration.md) - Claude/Codex collaboration model.
* [AI Design Review Strategy](process/ai-design-review-strategy.md) - Defense-in-depth strategy (template / author self-review / one-shot multi-perspective review / fix-loop protocol) that converges AI detailed-design PRs in at most one re-review round.
* [Coding Standards](process/coding-standards.md) - SOLID application rules, Japanese XML documentation comment rules, internal-default accessibility policy, and GoF pattern vocabulary for C# implementation.
* [Design Review Pattern Catalog](process/design-review-patterns.md) - Living `DRP-xxx` catalog of recurring design-defect patterns used as author self-check and reviewer checklist.
* [Git Policy](process/git-policy.md) - Branch, commit, pull request, and protection rules.
* [GitHub Issue and Project Workflow](process/github-issue-project-workflow.md) - Japanese Issue writing rules, Project fields, views, and role-based task flow.
* [Lifecycle Traceability](process/lifecycle-traceability.md) - Phase artifact, ID, and trace block rules.
* [OKF Policy](process/okf-policy.md) - Local rules for OKF scope, frontmatter, and canonical sources.
* [Quality Review Policy](process/quality-review-policy.md) - ISO/IEC 25010-oriented quality review gates for lifecycle artifacts and agents.
* [Stryker Mutation Workflow](process/stryker-workflow.md) - Canonical restore/run/evidence flow for CS-10 mutation-score recording across local environments and agent models.
* [TDD and Traceability](process/tdd-and-traceability.md) - Development workflow and evidence expectations.
