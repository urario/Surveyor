---
type: Architecture Note
title: Surveyor Layering Principles
description: Required layer split for the WinUI 3/C# Surveyor implementation.
resource: ../../docs/gui-testability-analyzer-requirements.md
tags: [architecture, winui3, rq-054, tdd]
timestamp: 2026-06-30T00:00:00+09:00
---

# Principle

Surveyor uses WinUI 3/C# for the application shell, while analysis, scoring, and output generation remain UI-independent.

# Layers

| Layer | Responsibility | Test Strategy |
| -- | -- | -- |
| Analysis core | UIA client ports, element model, capture abstractions | Unit tests with fake UIA/capture adapters |
| Evaluation/scoring | deterministic rules and recommendations | Pure unit tests and fixture snapshots |
| Output generation | HTML and machine-readable report generation | Golden-file or schema-focused tests |
| WinUI 3 shell | user interaction and report viewing | Thin integration tests or manual verification |
| Future CLI | reuse core/evaluation/output | command-level tests when added |

# Guardrails

- `RQ-048`: target application inspection must stay read-only.
- `RQ-051`: scoring and machine-readable outputs must be deterministic.
- `RQ-052`: screenshots and extracted text may contain confidential data.
- `RQ-054`: WinUI 3 must not leak into the core analysis or scoring layers.
- `RQ-055`: output structure should remain useful for future modernization planning without turning testability score into migration difficulty score.

# Citations

[1] [Requirement specification RQ-054](../../docs/gui-testability-analyzer-requirements.md)
