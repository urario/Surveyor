---
type: Process
title: Lifecycle Traceability
description: Phase artifact, identifier, and evidence rules for Surveyor requirements, design, implementation, and tests.
tags: [process, traceability, lifecycle, okf]
timestamp: 2026-07-01T00:00:00+09:00
---

# Purpose

Surveyor values phase discipline in both waterfall and agile work. Phases are evidence lanes, not handoff walls: a small agile slice can still move through requirements, architecture, design, implementation, unit test, and integration evidence.

# Trace Principle

- `RQ-xxx` identifiers are the root of requirement traceability.
- Durable lifecycle artifacts must name their upstream requirement, decision, design, test, or implementation evidence.
- Do not duplicate long requirement text outside the canonical requirement source. Summarize and link.
- Prefer a small OKF concept over private chat memory when a decision or trace must survive the current task.
- A pull request can carry short-lived evidence, but durable or cross-phase evidence belongs under `knowledge/`.

# Artifact Homes

| Phase | Durable home | Rule |
| -- | -- | -- |
| Requirement definition | `docs/gui-testability-analyzer-requirements.md`, `knowledge/requirements/` | Requirement meaning is canonical in the source document; OKF provides metadata and indexes. |
| Architecture design | `knowledge/architecture/`, `knowledge/decisions/` | Architecture notes and ADRs must link to driving `RQ-xxx` IDs. |
| Basic design | `knowledge/design/` | Use a `DES-xxxx` artifact when behavior, interfaces, or module responsibilities need durable design trace. |
| Detailed design | `knowledge/design/` | Use a `DES-xxxx` artifact when algorithms, schemas, ports, or test seams need durable design trace. |
| Implementation | Source files plus `knowledge/traces/` when durable evidence is needed | Non-trivial slices must identify requirements, design inputs, changed files, tests, and verification. |
| Unit test | Test files plus `knowledge/traces/` when durable evidence is needed | Unit evidence must name behavior, test file, command, and related `RQ-xxx` or `DES-xxxx`. |
| Integration test | Integration/manual evidence plus `knowledge/traces/` | Integration evidence must name environment, fixture or target assumptions, command or manual procedure, and residual risk. |

# Identifier Rules

Use stable IDs for durable artifacts. IDs should appear in the filename when practical and in the document title or trace block.

| Prefix | Meaning | Example |
| -- | -- | -- |
| `RQ-xxx` | Canonical requirement | `RQ-051` |
| `ADR-xxxx` | Architecture or process decision | `ADR-0001` |
| `DES-xxxx` | Basic or detailed design artifact | `DES-0001` |
| `IMP-xxxx` | Durable implementation slice trace | `IMP-0001` |
| `UT-xxxx` | Durable unit test evidence | `UT-0001` |
| `IT-xxxx` | Durable integration test evidence | `IT-0001` |
| `TRC-xxxx` | Cross-phase trace note that combines several evidence types | `TRC-0001` |

IDs identify durable artifacts, not every commit, method, or individual assertion. A PR description can still be enough for a small local change when no durable project knowledge is created.

# Required Trace Block

Every durable lifecycle artifact under `knowledge/` that represents design, implementation, or test evidence should include a short trace block:

| Field | Content |
| -- | -- |
| Artifact | Stable artifact ID, title, and phase |
| Upstream | Related `RQ-xxx`, `ADR-xxxx`, `DES-xxxx`, or prior trace IDs |
| Downstream | Known implementation files, tests, reports, or follow-up trace IDs |
| Evidence | Design summary, changed files, test behavior names, commands, or manual procedure |
| Verification | Command/result or review status |
| Residual Risk | Unknowns, manual validation, Windows-specific constraints, or `None known` |

# Phase Gate Expectations

- Requirements are ready when the relevant `RQ-xxx` IDs and non-goals are visible.
- Architecture is ready when driving requirements, decisions, layer boundaries, and test seams are visible.
- Basic design is ready when responsibilities, inputs, outputs, and non-targets are traceable to requirements.
- Detailed design is ready when algorithms, schemas, ports, fixtures, and deterministic behavior are testable.
- Implementation is ready for review when changed files, tests, verification, and residual risk are tied to requirements and design inputs.
- Unit test evidence is ready when behavior names, test files, and commands are recorded.
- Integration test evidence is ready when environment assumptions, target state, commands or manual steps, and residual risk are recorded.

# Related

- [Requirement Source](../requirements/source-spec.md)
- [Generated RQ Index](../requirements/rq-index.generated.md)
- [Layering Principles](../architecture/layering-principles.md)
- [Design Knowledge](../design/index.md)
- [Trace Evidence](../traces/index.md)
- [TDD and Traceability](tdd-and-traceability.md)
