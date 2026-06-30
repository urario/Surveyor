---
name: surveyor-okf
description: Create and maintain Surveyor OKF knowledge, requirement indexes, ADR links, logs, and validation scripts.
---

# Surveyor OKF

Use this skill when updating `knowledge/`, refreshing requirement indexes, recording decisions, or validating traceability.

## Workflow

1. Read `knowledge/index.md`.
2. Open only the concept files related to the task.
3. Read `knowledge/process/lifecycle-traceability.md` when the task touches requirements, design, implementation, unit tests, or integration tests.
4. For each new non-reserved `.md` file, add YAML frontmatter with a non-empty `type`.
5. Keep root requirements canonical in `docs/gui-testability-analyzer-requirements.md`.
6. Use links and concise summaries instead of duplicating long requirement text.
7. Update `knowledge/log.md` for meaningful changes.
8. Run `tools/okf/Validate-Okf.ps1`.

## References

- See `references/okf-conventions.md` for local OKF rules.
- Use `tools/requirements/Export-RqIndex.ps1` to regenerate the RQ index.
- Use `knowledge/design/` for durable design artifacts and `knowledge/traces/` for durable trace evidence.
