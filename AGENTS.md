# Surveyor Agent Guide

This repository builds Surveyor, a Windows GUI testability analyzer for legacy C++/MFC/Win32 applications.

## Canonical Inputs

- Requirements source: `docs/gui-testability-analyzer-requirements.md`
- Requirement IDs use `RQ-xxx`; preserve these IDs in designs, tests, commits, reviews, and OKF notes.
- Project knowledge lives in `knowledge/` as an OKF-style bundle.

## AI Role Split

- Claude Code is design/review oriented. Use it to refine architecture, review implementation plans, check RQ traceability, and curate OKF knowledge.
- Codex is implementation oriented. Use it to make scoped code changes, write tests first, run verification, and update trace artifacts.
- Keep both agents working from repo-tracked artifacts, not private chat memory.

## Implementation Workflow

1. Identify the relevant RQ IDs and OKF concepts before editing code.
2. For non-trivial work, capture the design decision or implementation slice in OKF.
3. Use TDD: add or update failing tests before production code whenever the behavior is testable without a real Windows GUI target.
4. Implement in small vertical slices with deterministic outputs.
5. Run targeted tests and OKF validation before handoff.
6. Update trace notes so requirements, design decisions, tests, and implementation files remain connected.

## Architecture Guardrails

- Target platform is WinUI 3 / C# for the application shell.
- Keep analysis core, scoring, and report generation UI-independent.
- Treat UI Automation and capture APIs behind interfaces so they can be tested with fixtures.
- Preserve RQ-048 read-only behavior: analysis must not mutate the target application.
- Preserve RQ-051 determinism: scoring and machine-readable outputs must be stable for the same input.
- Preserve RQ-052 sensitivity: screenshots and extracted text may contain confidential data.

## Local Agent Assets

- Claude Code project agents: `.claude/agents/`
- Claude Code project skills: `.claude/skills/`
- Codex project skills: `.codex/skills/`
- Optional install helpers: `tools/codex/`
