---
type: Decision
title: ADR-0001 AI Collaboration and OKF
description: Use Claude Code for design/review, Codex for implementation, and OKF for shared project knowledge.
tags: [adr, ai-collaboration, okf, traceability]
timestamp: 2026-06-30T00:00:00+09:00
---

# Status

Accepted.

# Context

Surveyor development needs TDD, explicit development phases, and traceability from requirements to implementation and review artifacts. The project also uses two AI assistants with different emphasis:

- Claude Code: design and review
- Codex: implementation and verification

# Decision

Store project AI instructions, skills, agents, and knowledge as repo-tracked artifacts:

- Claude Code agents in `.claude/agents/`
- Claude Code skills in `.claude/skills/`
- Codex project skills in `.codex/skills/`
- OKF-style knowledge in `knowledge/`
- validation and generation scripts in `tools/`

Use `.codex/skills/` as the source of truth for Codex skills. Copies under `~/.codex/skills` are optional personal installs for automatic discovery in future Codex sessions, not the canonical project artifact.

# Consequences

- Project behavior is reviewable and version controlled.
- Agent outputs can link back to `RQ-xxx` IDs.
- Global user configuration is not required for the repository to explain its workflow.
- Optional install scripts may copy project skills into local user-level tool folders when desired.
- When a Codex skill changes, the repo-local version is reviewed first; personal installed copies should be refreshed afterward if used.

# Related

- [AI Collaboration](../process/ai-collaboration.md)
- [TDD and Traceability](../process/tdd-and-traceability.md)
- [Requirement Source](../requirements/source-spec.md)
