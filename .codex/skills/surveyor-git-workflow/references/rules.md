# Surveyor Git Rules

## Hard Rules

- No direct commits on `main`.
- No direct pushes to `main`.
- No force-pushes to `main`.
- No history rewriting of published branches without explicit agreement.

## Normal Flow

```powershell
git switch main
git pull --ff-only
git switch -c chore/example-topic
# edit, test, stage
git commit
git push -u origin chore/example-topic
```

Then open a pull request into `main`.

## PR Evidence

Include:

- Requirements: `RQ-xxx` IDs or "N/A"
- Tests: commands and results
- OKF: files updated or "N/A"
- Risk: residual manual validation or "None known"

## Local Hook Installation

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\git\Install-GitHooks.ps1
```

Hooks are local guardrails. GitHub branch protection should also require PRs into `main`.

