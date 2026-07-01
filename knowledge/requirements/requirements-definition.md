---
type: Requirement Definition
title: GUI Testability Analyzer Requirement Definition
description: OKF-facing summary of the RD-based requirement definition derived from the canonical RQ specification.
resource: ../../docs/gui-testability-analyzer-requirements-definition.md
tags: [requirements, rd, traceability, surveyor]
timestamp: 2026-07-01T00:00:00+09:00
---

# Summary

[GUI Testability Analyzer Requirement Definition](../../docs/gui-testability-analyzer-requirements-definition.md) derives implementation-ready requirement definitions `RD-001` through `RD-032` from the canonical `RQ-xxx` requirement specification.

The canonical requirement meaning remains in [gui-testability-analyzer-requirements.md](../../docs/gui-testability-analyzer-requirements.md). This OKF concept records the durable knowledge needed to navigate the derived requirement definition without duplicating the full requirements.

# Purpose

The requirement definition turns Surveyor's high-level requirement specification into traceable requirements for the initial version of the GUI testability analyzer.

It clarifies that the initial version is an analysis and decision-support tool, not a GUI test generator or GUI test runner. It supports decisions about which screens should be automated, improved first, limited to manual testing, or handled by lower-layer tests.

# Derived Requirement Clusters

| Cluster | RD range | Knowledge |
| -- | -- | -- |
| Analysis target and acquisition | `RD-001` through `RD-004` | Dynamic read-only inspection of user-displayed Windows screens, UIA/MSAA-based data acquisition, screen and element internal models. |
| Evaluation and classification | `RD-005` through `RD-016` | Stable identification, operability, observability, state controllability, custom UI risk, coordinate/image dependency, snapshots, automation strategy classification, improvement candidates, and priority support. |
| Outputs and reports | `RD-017` through `RD-024` | Human-readable reports, element issue lists, machine-readable output, deterministic comparison data, confidentiality handling, environment assumptions, and performance constraints. |
| Architecture and scope constraints | `RD-025` through `RD-032` | WinUI 3/C# shell, UI-independent core layers, unresolved design choices, MVP exclusions, expert calibration, modernization reuse, and read-only enforcement. |

# Trace Rules

- Preserve both upstream `RQ-xxx` IDs and derived `RD-xxx` IDs in downstream design, implementation, tests, reviews, and trace notes.
- Use [rq-index.generated.md](rq-index.generated.md) for the canonical `RQ-xxx` source index.
- Use the trace table in [gui-testability-analyzer-requirements-definition.md](../../docs/gui-testability-analyzer-requirements-definition.md#4-上流要求トレース) when mapping an upstream `RQ-xxx` to one or more derived `RD-xxx`.
- Do not treat `RD-xxx` as a replacement for `RQ-xxx`; `RD-xxx` refines the requirement for design and verification while `RQ-xxx` remains the upstream source requirement.

# Quality Guardrails

The canonical guardrail review result is maintained in [section 6.2 of the requirement definition](../../docs/gui-testability-analyzer-requirements-definition.md#62-ガードレール確認).

This OKF concept intentionally links to the source section instead of duplicating the guardrail-to-`RD-xxx` table, so trace updates stay in one place.

# Downstream Use

Use this requirement definition as the starting point for:

- Architecture design of UIA access, capture, permissions, packaging, and layer boundaries.
- Basic design of screen models, element models, evaluation responsibilities, and report responsibilities.
- Detailed design of scoring rules, stable keys, deterministic ordering, timeout handling, comparison behavior, and residual uncertainty reporting.
- Unit tests for UI-independent scoring, classification, reporting, trace mapping, and deterministic output.
- Integration tests for Windows environment assumptions, DPI, permissions, target application state, and screenshot behavior.

# Quality Review Result

The embedded review verdict and residual-risk narrative are maintained in [section 6.4 of the requirement definition](../../docs/gui-testability-analyzer-requirements-definition.md#64-verdict).

This OKF concept intentionally links to that source section instead of duplicating the verdict or residual-risk wording.

# Related

- [Requirement Source](source-spec.md)
- [Generated RQ Index](rq-index.generated.md)
- [Lifecycle Traceability](../process/lifecycle-traceability.md)
- [Quality Review Policy](../process/quality-review-policy.md)
