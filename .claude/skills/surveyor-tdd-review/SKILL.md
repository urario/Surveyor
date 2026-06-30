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

## Review Output

Report missing or weak tests as findings. Mention residual risk when behavior depends on actual Windows UI, integrity level, DPI, occlusion, or screenshot APIs.
