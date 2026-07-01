---
name: surveyor-quality-review
description: Review Surveyor requirements, architecture, designs, implementation plans, code, tests, PRs, and OKF evidence against ISO/IEC 25010-style product quality characteristics, Surveyor RQ traceability, and lifecycle phase quality gates. Use when Codex needs an independent quality review rather than design or implementation work.
---

# Surveyor Quality Review

Use this skill as an independent quality reviewer. Do not implement fixes while reviewing unless the user explicitly changes the task from review to implementation.

## Required Context

1. Read `knowledge/index.md` when requirements, architecture, process, Git workflow, or prior decisions are uncertain.
2. Read `knowledge/process/quality-review-policy.md` before judging quality characteristics or phase gates.
3. Read `knowledge/process/lifecycle-traceability.md` when reviewing durable phase evidence.
4. Read the relevant requirement sections in `docs/gui-testability-analyzer-requirements.md`; preserve `RQ-xxx` IDs in findings.
5. Prefer existing OKF concepts over private chat memory.
6. Read `knowledge/process/github-issue-project-workflow.md` when GitHub Issue / Project evidence is part of the artifact under review.

## Review Method

1. Identify the artifact phase: requirements, architecture, basic design, detailed design, implementation, unit test, integration test, or PR/release evidence.
2. Identify the relevant `RQ-xxx`, `ADR-xxxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, or `TRC-xxxx` links.
3. Select the ISO/IEC 25010 quality characteristics that matter for this artifact. Do not force every characteristic into every review.
4. Check Surveyor mandatory guardrails first:
   - `RQ-048`: analysis must be read-only and must not mutate the target app.
   - `RQ-051`: scoring and machine-readable output must be deterministic.
   - `RQ-052`: screenshots and extracted text can contain confidential data.
   - `RQ-054`: WinUI 3 stays in the shell; core analysis, scoring, and reports stay UI-independent.
5. Review evidence quality: measurable acceptance criteria, test seams, automated tests, manual validation, trace blocks, GitHub Issue / Project fields, and residual risk.

## Output Shape

Lead with findings, ordered by severity. Use file and line references when reviewing repository artifacts.

For each finding include:

- Severity: Blocking, High, Medium, or Low.
- Phase: the lifecycle phase being reviewed.
- Quality axis: relevant ISO/IEC 25010 characteristic or Surveyor guardrail.
- Evidence: concrete artifact, path, line, requirement ID, or missing evidence.
- Recommendation: the smallest reviewable correction.

Then add open questions, residual risk, and a short verdict:

- `Reject`: blocking quality or traceability gap.
- `Needs changes`: meaningful risk but not a hard stop.
- `Accept with risks`: acceptable if named residual risk is acknowledged.
- `Accept`: no material findings found.

## Review Boundaries

- Stay independent from the implementation/design author role.
- Do not create new requirements, architecture, or code during a review.
- Do not treat ISO/IEC 25010 as a certification checklist; use it as a quality vocabulary and risk lens.
- Do not report style-only issues unless they affect maintainability, testability, traceability, or user-facing quality.

## Reference

Use `references/review-output-template.md` when a structured review report is needed.
