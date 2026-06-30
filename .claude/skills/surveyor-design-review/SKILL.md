---
name: surveyor-design-review
description: Review Surveyor plans and architecture against RQ requirements, WinUI 3 layer separation, TDD seams, and future traceability.
---

# Surveyor Design Review

Use this skill before implementation or when reviewing an ADR/design note.

## Review Checklist

- Identify the `RQ-xxx` IDs that drive the design.
- Identify the lifecycle phase and expected durable artifact ID when the design should persist (`ADR-xxxx` or `DES-xxxx`).
- Confirm the design keeps WinUI 3 in the shell and avoids UI dependencies in core logic.
- Confirm UIA access, capture, scoring, and report generation are separated behind interfaces.
- Confirm read-only behavior for the target app.
- Confirm deterministic scoring and machine-readable outputs.
- Confirm confidential screenshots/text have a handling strategy.
- Confirm the design can be tested with fixtures before requiring real Windows GUI targets.
- Confirm outputs preserve traceability for later comparison and review.
- Confirm the design has upstream requirements and downstream implementation/test obligations named when applicable.

## Output Shape

Lead with blocking concerns. Then list open questions and a short recommendation. Keep implementation details only where they clarify the design risk.
