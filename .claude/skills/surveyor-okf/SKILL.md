---
name: surveyor-okf
description: Maintain Surveyor OKF knowledge under knowledge/, including requirement traces, ADRs, indexes, logs, and validation.
---

# Surveyor OKF

Use this skill when creating or updating project knowledge in `knowledge/`.

## Workflow

1. Read `knowledge/index.md` to understand the bundle structure.
2. Read only the relevant concept files for the task.
3. Read `knowledge/process/lifecycle-traceability.md` when the task touches requirements, design, implementation, unit tests, or integration tests.
4. Read `knowledge/process/github-issue-project-workflow.md` when the task touches GitHub Issue / Project workflow or task handoff rules.
5. Read `knowledge/process/stryker-workflow.md` when the task touches `CS-10`, Stryker.NET, or mutation-score evidence.
6. For new concept files, add YAML frontmatter with at least `type`.
7. Prefer `title`, `description`, `resource`, `tags`, and `timestamp` when they improve retrieval.
8. Link concepts with bundle-relative markdown links where possible.
9. Update `knowledge/log.md` for meaningful additions or changes.
10. Run `tools/okf/Validate-Okf.ps1`.

## Local Conventions

- Reserved files: `index.md` and `log.md`.
- Root `knowledge/index.md` may include frontmatter to declare `okf_version`.
- Non-reserved `.md` files must include frontmatter and a non-empty `type`.
- Keep requirement text canonical in `docs/gui-testability-analyzer-requirements.md`; OKF files should summarize and link.
- Use `RQ-xxx` IDs exactly as written.
- Use lifecycle artifact IDs for durable phase artifacts: `ADR-xxxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, and `TRC-xxxx`.
- Put durable design artifacts in `knowledge/design/` and durable trace evidence in `knowledge/traces/`.

## Concept Types

Use descriptive types rather than inventing a rigid taxonomy. Preferred starting set:

- `Requirement Source`
- `Requirement Index`
- `Architecture Note`
- `Decision`
- `Design`
- `Process`
- `Playbook`
- `Trace`

## Validation

Run:

```powershell
.\tools\okf\Validate-Okf.ps1
```

To refresh the requirement index:

```powershell
.\tools\requirements\Export-RqIndex.ps1
```
