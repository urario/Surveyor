---
name: surveyor-project-management
description: Manage Surveyor GitHub Issue / Project work, including execution plans, parallel/dependency tracking, Owner Role assignment across Human / Claude Code / Codex, risk management, status transitions, and completion-report handoff updates.
---

# Surveyor Project Management

Use this skill for project management, not for doing the design or implementation work itself.

## Required Inputs

1. Read `knowledge/process/github-issue-project-workflow.md`.
2. Read `knowledge/process/lifecycle-traceability.md` when lifecycle evidence or artifact IDs matter.
3. Read `knowledge/index.md` if prior decisions or OKF concepts are unclear.
4. When live GitHub state matters, inspect the Issue and Project item through `gh issue`, `gh project`, or GraphQL before changing fields.

## Core Responsibilities

- Keep Project fields current: `Status`, `Phase`, `Artifact`, `RQ`, `RD`, `Guardrail`, `Owner Role`, `Priority`, and `Target`.
- Produce execution plans as a small dependency graph: prerequisites, parallel tasks, review gates, and blocked tasks.
- Assign the current next-action owner:
  - `Claude Code`: requirements clarification, architecture, basic/detailed design, design review, OKF curation proposals.
  - `Codex`: implementation, unit tests, deterministic verification, PR preparation, trace evidence updates.
  - `Human`: priority, acceptance, real Windows/manual execution, ambiguous product decisions, security/privacy decisions, final gate close.
- Track guardrail, dependency, environment/manual, scope, and residual risks.
- Receive completion reports from worker agents and update the GitHub Project only after evidence is sufficient.

## Completion Report Intake

Require a Japanese completion report before moving a Project item forward:

```text
対象Issue:
担当:
実施内容:
成果物/変更:
関連RQ: Issue / Project にある場合。なければ N/A。
関連RD: Issue / Project にある場合。なければ N/A。
検証:
OKF更新:
残リスク:
次の推奨Status:
次のOwner Role:
```

Verify that the report names the Issue, lifecycle phase, artifact ID, verification evidence, OKF updates when needed, and residual risk. Require related `RQ` / `RD` only when the Issue / Project fields provide them; `N/A` is acceptable for absent optional fields. Do not invent `RD-xxx` for Unit Test, Integration Test, or review items that do not carry an RD value.

## Status Rules

- Move design work to `Design Review` when the design artifact is ready for review.
- Move reviewed design to `Ready for Implementation` only when review findings are closed or tracked.
- Move implementation/test work to `Code Review` when tests and PR evidence are ready.
- Move to `Done` only when Done conditions are satisfied and no human gate remains.
- Use `Blocked` only with a named blocker, unblocker owner, and next question/action.

## Output

Reply in Japanese. Lead with the recommended action or Project update performed, then list dependencies, owner assignment, risks, and manual GitHub UI work that remains.