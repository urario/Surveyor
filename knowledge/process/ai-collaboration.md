---
type: Process
title: AI Collaboration Model
description: Working agreement for Claude Code and Codex on Surveyor.
tags: [process, ai-collaboration, claude-code, codex]
timestamp: 2026-06-30T00:00:00+09:00
---

# Roles

Claude Code is responsible for design pressure, review discipline, and knowledge curation. Codex is responsible for implementation slices, test execution, and concrete repository changes.

# Handoffs

| From | To | Handoff Content |
| -- | -- | -- |
| Claude Code | Codex | RQ IDs, design notes, test seams, risks |
| Codex | Claude Code | changed files, test evidence, unresolved risks |
| Either | OKF | durable decisions, trace links, workflow updates |

# Operating Rules

- Do not rely on private conversation memory for project policy.
- Put durable knowledge in `knowledge/`.
- Keep `RQ-xxx` visible in tasks, tests, and reviews.
- Prefer narrow implementation slices that can be reviewed independently.

