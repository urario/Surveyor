---
name: surveyor-git-workflow
description: Use for Surveyor Git operations, branch creation, commit message proposals, PR preparation, and enforcing no direct main commits.
---

# Surveyor Git Workflow

Use this skill when working with Git in the Surveyor repository.

## Required Behavior

1. Run `git status --short --branch` before staging, committing, branching, or pushing.
2. Do not commit directly on `main`.
3. Do not push directly to `main`.
4. Create a topic branch before project changes.
5. Keep commits focused and use Conventional Commit style.
6. Include `RQ-xxx` IDs in commit bodies or PR descriptions for requirement-bearing changes.
7. Include lifecycle phase and artifact IDs in PR descriptions when durable evidence exists.
8. Link the related Japanese GitHub Issue in the PR and keep Project fields current when issue context exists.
9. Run relevant tests or validation before suggesting merge readiness.
10. Write the PR body in Japanese using `.github/pull_request_template.md`; fill every section and mark non-applicable ones as `N/A`. Keep identifiers, Project field names, commands, and code in their original form.
11. Crystallize a self-review in the "自己レビュー" section (findings and trade-offs, not just checkboxes), confirming requirement/design alignment, layer boundaries, read-only (`RQ-048`), determinism (`RQ-051`), and confidentiality (`RQ-052`).
12. For implementation PRs (`feat` / `fix` / `refactor`), attach quantitative quality-gate evidence per `CS-01`–`CS-10` and DES-0008.

## Branch Naming

Use short kebab-case branch names:

- `feature/<topic>`
- `fix/<topic>`
- `chore/<topic>`
- `docs/<topic>`
- `test/<topic>`

## Commit Message Guidance

Prefer:

```text
type: concise imperative summary

- optional detail
- verification or trace note when useful
```

Use `chore:` for tooling, workflow, generated indexes, and agent setup. Use `docs:` for documentation-only changes. Use `test:` for test-only changes.

## References

- Read `references/rules.md` for detailed Git policy.
- Read `knowledge/process/git-policy.md` for the OKF policy record.
- Read `knowledge/process/github-issue-project-workflow.md` for Japanese Issue / Project task handoff rules.

