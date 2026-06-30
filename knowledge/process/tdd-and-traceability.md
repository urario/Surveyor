---
type: Process
title: TDD and Traceability
description: Development process for preserving test-first behavior and requirement links.
tags: [process, tdd, traceability]
timestamp: 2026-06-30T00:00:00+09:00
---

# TDD Expectations

Surveyor should use TDD for logic that can be tested without a real target GUI. This is especially important for scoring, classification, element normalization, output generation, and comparison keys.

# Traceability Expectations

Each non-trivial implementation slice should identify:

- relevant `RQ-xxx` IDs
- design or ADR links when applicable
- tests added or updated
- implementation files changed
- verification commands run

For lifecycle artifacts beyond a small PR summary, follow [Lifecycle Traceability](lifecycle-traceability.md). Durable design, implementation, unit test, and integration test evidence should use the phase artifact IDs and trace block defined there.

# Evidence Pattern

Use this pattern in task summaries and review notes:

| Field | Content |
| -- | -- |
| Phase | requirements, architecture, basic design, detailed design, implementation, unit test, or integration test |
| Artifact | `ADR-xxxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, `TRC-xxxx`, PR, or "N/A" |
| Requirements | `RQ-xxx` list |
| Design | ADR, architecture note, or design artifact links |
| Tests | test files and behavior names |
| Implementation | changed source files |
| Verification | commands and result |
| Residual Risk | manual or Windows-specific validation still needed |
