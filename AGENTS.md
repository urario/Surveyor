# Surveyor Agent Guide

This repository builds Surveyor, a Windows GUI testability analyzer for legacy C++/MFC/Win32 applications.

## Canonical Inputs

- Requirements source: `docs/gui-testability-analyzer-requirements.md`
- Requirement IDs use `RQ-xxx`; preserve these IDs in designs, tests, commits, reviews, and OKF notes.
- Project knowledge lives in `knowledge/` as an OKF-style bundle.
- Lifecycle traceability rules live in `knowledge/process/lifecycle-traceability.md`.
- When uncertain about requirements, architecture, process, Git workflow, or prior decisions, read `knowledge/index.md` first and follow the linked OKF concept files.

## AI Role Split

- Claude Code is design/review oriented. Use it to refine architecture, review implementation plans, check RQ traceability, and curate OKF knowledge.
- Codex is implementation oriented. Use it to make scoped code changes, write tests first, run verification, and update trace artifacts.
- Keep both agents working from repo-tracked artifacts, not private chat memory.

## Implementation Workflow

1. Identify the relevant RQ IDs and OKF concepts before editing code.
2. For non-trivial work, capture the design decision, lifecycle artifact, or implementation slice in OKF.
3. Create a topic branch before edits; direct commits and pushes to `main` are prohibited.
4. Use TDD: add or update failing tests before production code whenever the behavior is testable without a real Windows GUI target.
5. Implement in small vertical slices with deterministic outputs.
6. Run targeted tests and OKF validation before handoff.
7. Update trace notes so requirements, design decisions, basic/detailed design, tests, and implementation files remain connected.

## Lifecycle Traceability

- Preserve phase evidence across requirements, architecture design, basic design, detailed design, implementation, unit test, and integration test.
- Use stable artifact IDs for durable artifacts: `ADR-xxxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, and `TRC-xxxx`.
- Durable design artifacts belong under `knowledge/design/`; durable implementation and test evidence belongs under `knowledge/traces/`.
- Pull requests may carry local evidence, but durable or cross-phase evidence must be repo-tracked.

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

## Git Workflow

- Read `knowledge/process/git-policy.md` before commit or PR work.
- Do not commit directly on `main`.
- Do not push directly to `main`.
- Use topic branches and pull requests for integration.
- Install local hooks with `tools/git/Install-GitHooks.ps1` when working in this repository.
