---
type: Playbook
title: Stryker Mutation Workflow
description: Canonical Surveyor workflow for restoring, running, and recording CS-10 Stryker.NET mutation-score evidence across local environments and agent models.
tags: [process, stryker, mutation-testing, cs-10, quality-gates]
timestamp: 2026-07-05T00:00:00+09:00
---

# Stryker Mutation Workflow

## Purpose

This playbook standardizes how Surveyor records `CS-10` mutation-score evidence so different models, agents, and local environments converge on the same commands, outputs, and trace shape.

## Canonical Inputs

- Tool manifest: `.config/dotnet-tools.json`
- Configuration: `eng/stryker/domain.stryker-config.json`, `eng/stryker/policy.stryker-config.json`
- Canonical runner: `tools/stryker/Run-StrykerBaseline.ps1`
- Durable evidence home: `knowledge/traces/`
- Generated reports: `artifacts/stryker/domain/reports/`, `artifacts/stryker/policy/reports/`

## Standard Flow

1. Restore the pinned local tool:

```powershell
dotnet tool restore
```

2. Run the canonical baseline command from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -SkipVersionCheck
```

3. Run a single target when iterating locally:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -Target Domain -SkipVersionCheck
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -Target Policy -SkipVersionCheck
```

4. Record the resulting mutation scores, surviving-mutant concentration, and residual risk in a durable trace note under `knowledge/traces/`.

## Environment Rules

- Prefer `tools/stryker/Run-StrykerBaseline.ps1` over `dotnet tool run dotnet-stryker`. The script reads the pinned version from `.config/dotnet-tools.json` and executes the restored CLI DLL from the user's NuGet cache, which avoids local-tool runner inconsistencies seen on SDK `10.0.301`.
- Do not hardcode a Stryker package version in ad hoc commands outside the committed script. If the version changes, update the manifest first and let the script follow it.
- If the script reports that the Stryker CLI DLL is missing, rerun `dotnet tool restore`.
- If Stryker analysis succeeds in the normal user environment but fails in a sandboxed or isolated environment, record that explicitly and rerun it in the normal user environment rather than weakening the evidence.
- Keep generated reports under `artifacts/stryker/`; they are disposable outputs, not OKF evidence by themselves.

## Evidence Rules

- `CS-10` is non-blocking for the baseline itself. Keep `thresholds.break = 0` unless a later process decision changes that policy.
- The score target remains `>= 80%` for core layers.
- When a score is below 80%, do not mark the slice blocked solely for that reason. Record:
  - the score
  - the main surviving-mutant concentration by file or behavior area
  - the no-coverage concentration when material
  - the next test-improvement candidates or follow-up Issue
- When Stryker was not run, state why it was not possible locally and what unblocker is required.

## PR / Issue Expectations

- Implementation PRs that touch `CS-10` should include the exact Stryker command used, both Domain and Policy scores when applicable, and the trace link.
- Issue completion reports should name the baseline scores and whether below-target results were logged as non-blocking residual risk with improvement candidates.

## Related

- [Coding Standards](coding-standards.md)
- [Git Policy](git-policy.md)
- [TDD and Traceability](tdd-and-traceability.md)
- [IMP-0016 Stryker.NET Baseline Implementation](../traces/imp-0016-stryker-baseline.md)
