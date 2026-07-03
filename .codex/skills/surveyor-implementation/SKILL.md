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
4. If working from a GitHub Issue, read the Japanese Issue body and Project fields, then keep `Status`, `Phase`, `Artifact`, `RQ`, `RD`, and `Owner Role` current.
5. Check `knowledge/index.md` and related OKF files for existing decisions.
6. Check `knowledge/process/lifecycle-traceability.md` for phase artifact and trace evidence rules.
7. Read `knowledge/process/coding-standards.md` before writing production code; apply `CS-01`–`CS-04`.
8. Write or update the smallest useful failing test first whenever the behavior is testable.
9. Implement the smallest production change that makes the test pass.
10. Run targeted tests and any relevant validation scripts.
11. Update OKF logs or trace notes when the change creates durable knowledge.
12. Report completion to `surveyor-project-management` with the Issue, artifact, verification result, OKF update, residual risk, and recommended next Project `Status` / `Owner Role`.

## Architecture Rules

- Keep WinUI 3 in a thin shell.
- Keep analysis core, scoring, and output generation UI-independent.
- Put UIA and screenshot APIs behind ports/interfaces.
- Treat target-application inspection as read-only.
- Make scoring and machine-readable output deterministic.
- Do not mix testability scoring with modernization/migration difficulty scoring.

## Coding Standards (knowledge/process/coding-standards.md)

- Apply SOLID as mapped to Surveyor structures (`CS-03`): one responsibility per class, ports for known variation points, port contracts honored by every implementation, small use-case-shaped interfaces, inward dependencies only.
- Write Japanese XML documentation comments on every public API in `src/**` (`CS-01`): `<summary>` in concise Japanese, `<param>`/`<returns>`/`<exception>` where applicable, guardrail contracts (`RQ-048`/`RQ-051`/`RQ-052`, cancellation, threading) in `<remarks>` on the port, `<inheritdoc/>` + deltas on implementations. A missing comment is a `CS1591` build error.
- Default to `internal` and `sealed`; make `public` only assembly-boundary contracts (`CS-02`). Tests use `InternalsVisibleTo`, never visibility promotion. New public APIs also require a `PublicAPI.Unshipped.txt` entry (`CS-08`).
- Use the GoF pattern vocabulary purpose-first (`CS-04`): when a catalog situation matches, follow its recommended pattern and record pattern/purpose/rejected-simpler-alternative in one line in the PR or design artifact; never add a pattern without a stated purpose.
- Respect the quantitative gates (`CS-05`–`CS-09`): all Microsoft CA rules and the code-metrics thresholds (cyclomatic complexity ≤ 10, inheritance depth ≤ 5, maintainability index ≥ 20, class coupling ≤ 30) are build errors — refactor (extract method, Strategy) instead of suppressing. Any suppression (`.editorconfig`, `#pragma`, `[SuppressMessage]`, metrics exception) carries a written justification and is a review target. Core-layer tests must keep line coverage ≥ 80% (`CS-07`); run `dotnet format --verify-no-changes` before handoff (`CS-09`).

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
- Follow `knowledge/process/coding-standards.md` for SOLID, Japanese XML doc comments, accessibility defaults, and pattern usage.
- If unsure about requirements, architecture, process, Git workflow, or prior decisions, read `knowledge/index.md` first.
- Follow `knowledge/process/lifecycle-traceability.md` for `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, and `TRC-xxxx` evidence.
- Follow `knowledge/process/git-policy.md` for branch, commit, and PR rules.
- Follow `knowledge/process/github-issue-project-workflow.md` for Japanese Issue wording, Project fields, and Issue/PR handoff rules.
- Use `surveyor-project-management` or the `surveyor-project-manager` agent for execution planning, completion handoff, risk tracking, and Project field updates.
- Use `tools/okf/Validate-Okf.ps1` after OKF edits.
- Use `tools/requirements/Export-RqIndex.ps1` when the requirement index needs refresh.
