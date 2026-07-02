---
name: surveyor-project-management
description: Plan and maintain Surveyor GitHub Issue / Project work, including execution DAGs, parallel/dependency planning, Owner Role assignment across Human / Claude Code / Codex, risk tracking, status transitions, and completion-report handoff updates. Use when Codex needs to organize GitHub Project items, create or triage Japanese Issues, plan task execution order, update Project fields, or receive work-completion reports from other agents.
---

# Surveyor Project Management

Use this skill for project management, not for doing the design or implementation work itself.

## Required Inputs

1. Read `knowledge/process/github-issue-project-workflow.md`.
2. Read `knowledge/process/lifecycle-traceability.md` when the task spans requirements, design, implementation, tests, or review.
3. Read `knowledge/index.md` if prior decisions, OKF concepts, or trace artifacts are unclear.
4. When live GitHub state matters, inspect the Issue and Project item through `gh issue`, `gh project`, or GraphQL before changing fields.

## Core Responsibilities

- Keep Project fields current: `Status`, `Phase`, `Artifact`, `RQ`, `RD`, `Guardrail`, `Owner Role`, `Priority`, and `Target`.
- Produce execution plans as a small dependency graph: blocked prerequisites, parallelizable tasks, critical path, and review gates.
- Assign the current next-action owner:
  - `Claude Code`: requirements clarification, architecture, basic/detailed design, design review, OKF curation proposals.
  - `Codex`: implementation, unit tests, deterministic verification, PR preparation, trace evidence updates.
  - `Human`: priority, acceptance, real Windows/manual execution, ambiguous product decisions, security/privacy decisions, final gate close.
- Track risks explicitly: guardrail risk, dependency risk, environment/manual risk, scope risk, and residual risk.
- Receive completion reports from worker agents and update the GitHub Project only after evidence is sufficient.

## Planning Workflow

1. List candidate Issues with their `Phase`, `Status`, `Artifact`, available `RQ` / `RD`, `Owner Role`, `Priority`, and `Target`; use `N/A` rather than inventing missing trace IDs.
2. Group tasks by dependency:
   - `Prerequisite`: must finish before another Issue can start.
   - `Parallel`: can run now without shared-file or decision conflicts.
   - `Gate`: requires review or human decision before downstream work.
   - `Blocked`: cannot progress without a named unblocker.
3. Recommend the next batch with owner roles and status changes.
4. Keep the plan small enough to execute. Prefer the next 3-7 work items over a speculative long roadmap unless the user asks for full decomposition.

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

Then verify:

- The report names the Issue, lifecycle phase, artifact ID, and related `RQ` / `RD` only when the Issue / Project fields provide them; `N/A` is acceptable for absent optional fields. Do not invent `RD-xxx` for Unit Test, Integration Test, or review items that do not carry an RD value.
- Verification evidence is present or the missing verification is recorded as residual risk.
- Required OKF updates are present when durable knowledge changed.
- Human approval is not bypassed for final gate close.

## Status Rules

- Move design work to `Design Review` when the design artifact is ready for review.
- Move reviewed design to `Ready for Implementation` only when review findings are closed or tracked.
- Move implementation/test work to `Code Review` when tests and PR evidence are ready.
- Move to `Done` only when the Issue's Done conditions are satisfied and no human gate remains.
- Use `Blocked` only with a named blocker, unblocker owner, and next question/action.

## GitHub Project Updates

Use `gh project item-edit` for field values when authorized. If view layout, filters, or displayed columns need changes, report that GitHub's public CLI/API support is limited and give exact manual UI steps.

When updating Project fields, summarize the old-to-new change in Japanese and avoid silently changing `Priority`, `Target`, or `Owner Role` without a reason.

## Output

Reply in Japanese. Lead with the recommended action or the update performed, then list dependencies, owner assignment, risks, and any manual GitHub UI work that remains.