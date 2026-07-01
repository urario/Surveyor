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

## GitHub Issue / Project Work

- Issue タイトルと本文は日本語を基本にする。
- Project フィールド名と選択肢は英語固定にする。
- 作業前に関連 Issue の `Status`, `Phase`, `Artifact`, `RQ`, `RD`, `Owner Role` を確認する。
- PR には関連 Issue をリンクし、検証結果と残リスクを残す。
- 詳細は `knowledge/process/github-issue-project-workflow.md` に従う。

## PR Evidence

Include:

- Phase: lifecycle phase or "N/A"
- Requirements: `RQ-xxx` IDs or "N/A"
- Artifacts: `ADR-xxxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, `TRC-xxxx`, PR-only, or "N/A"
- Tests: commands and results
- OKF: files updated or "N/A"
- Risk: residual manual validation or "None known"

## Local Hook Installation

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\git\Install-GitHooks.ps1
```

Hooks are local guardrails. GitHub branch protection should also require PRs into `main`.

