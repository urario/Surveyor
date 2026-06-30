# Surveyor OKF Conventions

Surveyor uses an OKF-style bundle under `knowledge/`.

## File Rules

- Concept documents are UTF-8 markdown files with YAML frontmatter.
- Every non-reserved concept document needs a non-empty `type`.
- `index.md` and `log.md` are reserved.
- Root `knowledge/index.md` may include frontmatter with `okf_version`.
- Use ISO dates for log headings.

## Recommended Frontmatter

```yaml
---
type: Decision
title: Short human-readable title
description: One sentence summary.
resource: optional/canonical/resource
tags: [surveyor, traceability]
timestamp: 2026-06-30T00:00:00+09:00
---
```

## Trace Links

- Use `RQ-xxx` IDs exactly.
- Use lifecycle artifact IDs for durable phase artifacts: `ADR-xxxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, and `TRC-xxxx`.
- Link to the source requirements document when a concept derives from it.
- Link decisions to requirements and implementation artifacts as they appear.
- Put durable design artifacts under `knowledge/design/` and durable trace evidence under `knowledge/traces/`.
- Prefer concise concept summaries over copied requirement sections.
