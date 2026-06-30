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
- Link to the source requirements document when a concept derives from it.
- Link decisions to requirements and implementation artifacts as they appear.
- Prefer concise concept summaries over copied requirement sections.

