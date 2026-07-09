---
name: surveyor-tdd-review
description: Review Surveyor implementation changes for TDD discipline, test evidence, deterministic behavior, and requirement coverage.
---

# Surveyor TDD Review

Use this skill after code changes or while planning a test-first slice.

## Checks

- Tests were added or updated before production behavior where feasible.
- Tests name the behavior, not implementation mechanics.
- Test data uses fixtures or fake ports instead of live GUI state when possible.
- UI Automation, capture APIs, clock, file system, and process state are abstracted for tests.
- Scoring tests cover edge cases and avoid non-deterministic ordering.
- Tests or trace notes reference relevant `RQ-xxx` IDs for non-trivial behavior.
- Durable unit test evidence uses `UT-xxxx` or a trace note when PR evidence is not enough.
- Durable integration test evidence uses `IT-xxxx` or a trace note and names environment assumptions.
- Manual verification is explicitly named when automation is not feasible.
- Tests reach `internal` members via `InternalsVisibleTo`, not by promoting production members to `public` or using reflection (`knowledge/process/coding-standards.md` `CS-02`); hard-to-test code is flagged as a missing design seam, not worked around.
- New public APIs carry Japanese XML doc comments whose content states the contract (guardrail constraints in `<remarks>`), not a restatement of the implementation (`CS-01`; existence is a build error, quality is reviewed here).
- Coverage evidence is honest: the core-layer 80% line gate (`CS-07`) is met by behavior-asserting tests, not assertion-free coverage padding; mutation-score evidence (`CS-10`) is recorded when the slice cadence calls for it, using the canonical flow in `knowledge/process/stryker-workflow.md`. When the score is below 80%, review that the trace still records the surviving-mutant concentration, no-coverage concentration when material, and concrete follow-up candidates instead of hiding the miss.
- Every new analyzer/metrics suppression (`CS-05`/`CS-06`) is listed, justified, and genuinely preferable to refactoring; treat an unexplained suppression as a finding.

## Review Output

Report missing or weak tests as findings. Mention residual risk when behavior depends on actual Windows UI, integrity level, DPI, occlusion, or screenshot APIs.
