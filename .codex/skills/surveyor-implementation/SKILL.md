---
name: surveyor-implementation
description: Implement Surveyor features with TDD, RQ traceability, WinUI 3/C# layer separation, OKF updates, and deterministic verification.
---

# Surveyor Implementation

Use this skill when implementing or changing Surveyor code, tests, project structure, or developer tooling.

## Workflow

1. Read the relevant requirement sections in `docs/gui-testability-analyzer-requirements.md`.
2. Identify the `RQ-xxx` IDs that the change serves.
3. Check `git status --short --branch` and create a topic branch if on `main`.
4. Check `knowledge/index.md` and related OKF files for existing decisions.
5. Check `knowledge/process/lifecycle-traceability.md` for phase artifact and trace evidence rules.
6. Write or update the smallest useful failing test first whenever the behavior is testable.
7. Implement the smallest production change that makes the test pass.
8. Run targeted tests and any relevant validation scripts.
9. Update OKF logs or trace notes when the change creates durable knowledge.

## Architecture Rules

- Keep WinUI 3 in a thin shell.
- Keep analysis core, scoring, and output generation UI-independent.
- Put UIA and screenshot APIs behind ports/interfaces.
- Treat target-application inspection as read-only.
- Make scoring and machine-readable output deterministic.
- Do not mix testability scoring with modernization/migration difficulty scoring.

## TDD Targets

Prefer tests around:

- UI element model normalization
- stable identifier and duplicate detection
- custom UI and coordinate-dependency heuristics
- scoring rules and recommendation classification
- output schema generation
- comparison keys and deterministic sorting

## References

- See `references/workflow.md` for the detailed implementation checklist.
- If unsure about requirements, architecture, process, Git workflow, or prior decisions, read `knowledge/index.md` first.
- Follow `knowledge/process/lifecycle-traceability.md` for `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, and `TRC-xxxx` evidence.
- Follow `knowledge/process/git-policy.md` for branch, commit, and PR rules.
- Use `tools/okf/Validate-Okf.ps1` after OKF edits.
- Use `tools/requirements/Export-RqIndex.ps1` when the requirement index needs refresh.
