---
name: surveyor-design-review
description: Review Surveyor plans and architecture against RQ requirements, WinUI 3 layer separation, TDD seams, and future traceability.
---

# Surveyor Design Review

Use this skill before implementation or when creating/reviewing an ADR or architecture/design note.

## Canonical Inputs

Read these before reviewing:

- `docs/gui-testability-analyzer-requirements.md` — upstream `RQ-xxx`.
- `docs/gui-testability-analyzer-requirements-definition.md` — derived `RD-xxx`.
- `knowledge/architecture/layering-principles.md` and `knowledge/architecture/des-0001-initial-architecture.md`.
- `knowledge/process/lifecycle-traceability.md` and `knowledge/process/quality-review-policy.md`.

Carry both `RQ-xxx` and `RD-xxx` through the review. Summarize and link; do not duplicate long requirement text.

## Blocking Guardrails

These are blocking — a design that violates any is not ready:

- `RQ-048` read-only: inspection must not mutate the target; the acquisition surface exposes no state-changing operation.
- `RQ-051` determinism: scoring, keys, and ordering are stable for the same input; core owns rounding/thresholds; time only from an injected clock.
- `RQ-052` confidential data: screenshots and extracted text are secure-by-default with centralized masking.
- `RQ-054` layer separation: WinUI 3 stays in the shell; core analysis, scoring, and output generation are UI-independent.

## Review Checklist

- Identify the `RQ-xxx` and `RD-xxx` IDs that drive the design.
- Identify the lifecycle phase and expected durable artifact ID when it should persist (`ADR-xxxx` or `DES-xxxx`), including the required trace block.
- **Clean Architecture**: dependency rule holds inward; no WinUI/UIA/capture/filesystem types leak into domain or application; adapters are wired at a composition root.
- **MVVM**: Views only bind/forward input; ViewModels own UI state and command orchestration and reach behavior only through use cases/ports; ViewModels never call UIA/capture/report-writer/store directly and stay testable with fakes; navigation/dialogs are behind presentation ports.
- **Boundaries**: target/window discovery (process enumeration, handle resolution, permission/integrity checks), UIA/MSAA acquisition, screen capture, scoring, reporting, storage, and the WinUI shell are each isolated behind precise ports with clear owner layer and dependency direction. Watch for process/window discovery leaking into the shell or ViewModel.
- **Interface design**: each interface states its purpose, owner layer, I/O contract, error/result model (unavailable data kept distinct from a low score), cancellation/timeout where relevant, determinism and read-only/confidentiality constraints, and a fake/fixture path. Flag broad `Manager`/`Service` catch-alls and abstractions with no real variability.
- **Technology allocation**: layer language is limited to C#/C++; confirm `RQ-054`/`RD-025` are read as "WinUI 3/C# shell + UI-independent core", not "every layer must be C#" and not "native adapters forbidden"; the chosen allocation and rejected options carry tradeoffs.
- **Determinism**: stable keys, defined ordering, core-owned rounding/thresholds, unavailable-vs-low-score distinction, injected clock. Keys are separated from display labels; volatile/sensitive text (window title, `Name`) must not be raw key material.
- **Read-only**: enforced by port surface (no mutation operation exposed), plus an adapter-level audit test that rejects state-changing UIA pattern calls, plus a required `IT-xxxx` that asserts an explicit target-state invariant set (focus, selection, text, checked/toggle, expand/collapse, scroll offset, active tab/dialog, window position and z-order, target data) is unchanged.
- **Confidentiality**: secure-by-default masking/blur, centralized decision point, no confidential data in logs, and no raw sensitive text embedded in keys, file paths, filenames, or machine-readable ids.
- **Output/store robustness**: filesystem/output boundaries are async cancellable with atomic-write or defined partial-result semantics, with cancellation/failure-path tests.
- **Testability**: every port has a fixture/fake path enabling unit tests before real Windows GUI targets.
- **Extensibility/maintainability**: acquisition/capture/scoring/report/privacy providers extend without reshaping the core; future CLI reuse is preserved.
- **Purpose-first patterns**: each design pattern serves a stated purpose with a short tradeoff note; flag pattern-for-pattern's-sake.
- **Diagrams**: architecture artifacts include at least one Mermaid diagram (GitHub-friendly, ASCII node ids, short labels, no PlantUML/external images); check dependency direction and boundary accuracy.
- **Traceability**: upstream requirements and downstream implementation/test obligations are named.

## Output Shape

Lead with blocking concerns, then trace gaps, then open questions, then a short recommendation. Keep implementation details only where they clarify a design risk. Leave only reviewable decisions and rationale; no private chain-of-thought.
