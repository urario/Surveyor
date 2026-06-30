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

# Evidence Pattern

Use this pattern in task summaries and review notes:

| Field | Content |
| -- | -- |
| Requirements | `RQ-xxx` list |
| Tests | test files and behavior names |
| Implementation | changed source files |
| Verification | commands and result |
| Residual Risk | manual or Windows-specific validation still needed |

