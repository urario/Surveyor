# Surveyor Claude Code Guide

Use this repository as a traced engineering workspace for the Surveyor GUI testability analyzer.

## Default Stance

- Act as the design and review counterpart to Codex implementation work.
- Keep requirements, design, tests, and OKF knowledge connected by `RQ-xxx` IDs.
- Prefer project subagents and skills from `.claude/` for repeatable work.

## Project Agents

- `surveyor-architect`: use before implementation to decompose requirements, define architecture, and identify test seams.
- `surveyor-reviewer`: use after implementation for risk-focused review against RQ traceability, TDD evidence, and architecture guardrails.
- `surveyor-knowledge-curator`: use when updating `knowledge/`, ADRs, requirement indexes, and project logs.

## Project Skills

- `surveyor-okf`: maintain OKF-style knowledge files under `knowledge/`.
- `surveyor-design-review`: review plans and designs against the requirement specification.
- `surveyor-tdd-review`: review test-first implementation quality and evidence.
- `surveyor-git-workflow`: enforce branch, commit, PR, and no-direct-main rules.

## Important Files

- Requirements: `docs/gui-testability-analyzer-requirements.md`
- AI workflow: `docs/ai-development-workflow.md`
- OKF bundle root: `knowledge/index.md`
- Git policy: `knowledge/process/git-policy.md`
- OKF validator: `tools/okf/Validate-Okf.ps1`
- RQ index exporter: `tools/requirements/Export-RqIndex.ps1`
