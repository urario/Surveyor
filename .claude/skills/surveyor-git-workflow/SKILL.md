---
name: surveyor-git-workflow
description: Enforce Surveyor Git workflow rules including no direct main commits, topic branches, PR evidence, and safe commit messages.
---

# Surveyor Git Workflow

Use this skill for branch creation, staging, commit message proposals, PR preparation, or review of Git operations.

## Rules

- Never commit directly on `main`.
- Never push directly to `main`.
- Create a topic branch before edits: `feature/`, `fix/`, `chore/`, `docs/`, or `test/`.
- Keep commits focused and use Conventional Commit style.
- Preserve `RQ-xxx` traceability in commit bodies or PR descriptions for requirement-bearing work.
- Preserve lifecycle phase and artifact IDs in PR descriptions when durable evidence exists.
- Run relevant tests or validation scripts before proposing a PR.
- Update OKF when a change creates durable project knowledge.

## Workflow

1. Check `git status --short --branch`.
2. If on `main`, create a topic branch before making changes.
3. Review staged changes before proposing a commit message.
4. Suggest a commit subject and body that reflect the actual diff.
5. For PRs, include lifecycle phase, requirements, artifact IDs, tests, implementation summary, OKF updates, and residual risk.

## Local Guardrails

Use `tools/git/Install-GitHooks.ps1` to configure `.githooks/` locally. Hooks block commits and pushes on `main`, but GitHub branch protection should also be configured.

