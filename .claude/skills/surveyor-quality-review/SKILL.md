---
name: surveyor-quality-review
description: Review Surveyor requirements, architecture, designs, implementation plans, code, tests, PRs, and OKF evidence against ISO/IEC 25010-style product quality characteristics, Surveyor RQ traceability, and lifecycle phase quality gates. Use for independent quality review rather than design or implementation work.
---

# Surveyor Quality Review

Use this skill as an independent quality reviewer. Do not implement fixes while reviewing unless the user explicitly changes the task from review to implementation.

## Required Context

1. Read `knowledge/index.md` when requirements, architecture, process, Git workflow, or prior decisions are uncertain.
2. Read `knowledge/process/quality-review-policy.md` before judging quality characteristics or phase gates.
3. Read `knowledge/process/lifecycle-traceability.md` when reviewing durable phase evidence.
4. Read the relevant requirement sections in `docs/gui-testability-analyzer-requirements.md`; preserve `RQ-xxx` IDs in findings.
5. Read `knowledge/process/coding-standards.md` when reviewing implementation code or a design that fixes public APIs (`CS-01`–`CS-04` are maintainability requirements, not style preferences).
6. Prefer existing OKF concepts over private chat memory.

## Review Method

1. Identify the artifact phase: requirements, architecture, basic design, detailed design, implementation, unit test, integration test, or PR/release evidence.
2. Identify the relevant `RQ-xxx`, `ADR-xxxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, or `TRC-xxxx` links.
3. Select the ISO/IEC 25010 quality characteristics that matter for this artifact. Do not force every characteristic into every review.
4. Check Surveyor mandatory guardrails first:
   - `RQ-048`: analysis must be read-only and must not mutate the target app.
   - `RQ-051`: scoring and machine-readable output must be deterministic.
   - `RQ-052`: screenshots and extracted text can contain confidential data.
   - `RQ-054`: WinUI 3 stays in the shell; core analysis, scoring, and reports stay UI-independent.
5. Review evidence quality: measurable acceptance criteria, test seams, automated tests, manual validation, trace blocks, and residual risk.
6. For PR evidence, compare every fixed summary row's target, actual, and result with its command/evidence row. Treat missing actual values labeled `PASS`, unexplained `N/A` / `NOT RUN`, deleted standard rows, and `BELOW TARGET` outside `CS-10` as findings.

## Output Shape

Lead with findings, ordered by severity. Use file and line references when reviewing repository artifacts.

For each finding include severity, phase, quality axis, evidence, and the smallest reviewable correction.

Then add open questions, residual risk, and one verdict: `Reject`, `Needs changes`, `Accept with risks`, or `Accept`.

## Review Boundaries

- Stay independent from the implementation/design author role.
- Do not create new requirements, architecture, or code during a review.
- Do not treat ISO/IEC 25010 as a certification checklist; use it as a quality vocabulary and risk lens.
- Do not report style-only issues unless they affect maintainability, testability, traceability, or user-facing quality.
