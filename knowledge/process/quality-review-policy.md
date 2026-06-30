---
type: Process
title: Quality Review Policy
description: ISO/IEC 25010-oriented quality review model for Surveyor lifecycle artifacts and agents.
tags: [process, quality, review, iso-25010, traceability]
timestamp: 2026-07-01T00:00:00+09:00
---

# Purpose

Surveyor quality reviews use ISO/IEC 25010:2023 as a shared quality vocabulary and risk lens. The goal is disciplined, phase-aware review of requirements, architecture, design, implementation, tests, and release evidence. This policy is not a certification claim.

# Source Model

ISO/IEC 25010:2023 defines a product quality model for ICT and software products with nine quality characteristics and lifecycle uses such as requirements, design objectives, test objectives, quality control criteria, acceptance criteria, and quality measures.

Use these characteristics as review axes:

- Functional suitability
- Performance efficiency
- Compatibility
- Interaction capability
- Reliability
- Security
- Maintainability
- Flexibility
- Safety

Source: [ISO/IEC 25010:2023 product quality model](https://www.iso.org/standard/78176.html)

# Surveyor Quality Priorities

Surveyor reviews always prioritize these project-specific quality constraints before general polish:

- `RQ-048`: inspection is read-only and must not mutate the target application.
- `RQ-051`: scoring and machine-readable reports are deterministic for the same input and conditions.
- `RQ-052`: screenshots and extracted text can contain confidential data and need explicit handling.
- `RQ-054`: WinUI 3 belongs in the shell; analysis core, scoring, and report generation stay UI-independent.
- Lifecycle evidence must preserve upstream and downstream traceability through `RQ-xxx`, `ADR-xxxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, or `TRC-xxxx` IDs when durable evidence is required.

# Phase Gates

## Requirements

Review for functional suitability, interaction capability, security, safety, and measurability.

- Every durable requirement or requirement interpretation has a stable `RQ-xxx` link.
- Quality constraints are explicit enough to become acceptance criteria.
- Non-goals are visible where scope could be confused, especially modernization versus testability analysis.
- Confidentiality, determinism, read-only behavior, Windows integration constraints, and report comparability are not deferred as vague future concerns.

## Architecture

Review for maintainability, compatibility, security, reliability, flexibility, and testability.

- UI Automation, capture, filesystem, process, clock, and report output boundaries are behind testable interfaces.
- WinUI 3 dependencies do not enter core analysis, scoring, or output generation.
- Deterministic ordering and stable report keys are architectural concerns, not late formatting details.
- Failure modes for Windows UI, integrity level, DPI, occlusion, multi-monitor behavior, and unavailable screenshots are named.

## Basic Design

Review for functional suitability, maintainability, interaction capability, and traceability.

- Responsibilities, inputs, outputs, non-targets, and downstream tests are clear.
- Design artifacts identify their upstream `RQ-xxx` and architecture or ADR inputs.
- User-facing report behavior is tied to the task of finding GUI testability obstacles, not generic modernization.
- Privacy and data-retention expectations are visible for screenshots and extracted text.

## Detailed Design

Review for reliability, performance efficiency, security, maintainability, and determinism.

- Algorithms and schemas define stable ordering, stable IDs, duplicate handling, and missing-data behavior.
- Edge cases have fixture strategies before live Windows GUI testing is required.
- Error handling distinguishes unavailable data from a low quality score.
- Machine-readable outputs can support comparison and regression review.

## Implementation

Review for functional correctness, maintainability, testability, reliability, security, and performance risk.

- Code preserves layer boundaries and read-only inspection.
- Scoring and serialization avoid nondeterministic collection order, ambient clock dependence, locale surprises, and machine-specific paths unless explicitly modeled.
- Confidential data is not logged, persisted, or exposed without a documented reason.
- Tests cover pure logic with fakes or fixtures before live GUI dependencies.

## Unit Test

Review for testability, functional correctness, reliability, and determinism.

- Tests name behavior and relevant `RQ-xxx` or design IDs for non-trivial behavior.
- Fixtures cover edge cases for custom UI, missing identifiers, duplicate identifiers, coordinate dependence, screenshots unavailable, and report ordering when relevant.
- Tests avoid relying on real GUI state unless the artifact is explicitly integration evidence.

## Integration Test

Review for reliability, compatibility, interaction capability, security, and residual risk.

- Environment assumptions are explicit: Windows version, DPI awareness, monitor layout, process integrity, target fixture app, occlusion, and screenshot API behavior.
- Manual steps are reproducible and tied to expected evidence.
- Residual risk is recorded when automation is not feasible.

## Pull Request Or Release Evidence

Review for lifecycle traceability and acceptance readiness.

- The PR or trace note states phase, requirement IDs, changed files or artifacts, tests run, validation commands, and residual risk.
- Durable cross-phase evidence lives under `knowledge/` when PR-local evidence is not enough.
- OKF validation passes after knowledge edits.

# Review Verdicts

- `Reject`: blocking quality, safety, privacy, determinism, read-only, or traceability gap.
- `Needs changes`: material risk exists but can be corrected in the current slice.
- `Accept with risks`: quality is acceptable if named residual risks are explicitly carried.
- `Accept`: no material findings found for the stated scope.

# Related

- [Requirement Source](../requirements/source-spec.md)
- [Generated RQ Index](../requirements/rq-index.generated.md)
- [Layering Principles](../architecture/layering-principles.md)
- [Lifecycle Traceability](lifecycle-traceability.md)
- [AI Collaboration](ai-collaboration.md)
