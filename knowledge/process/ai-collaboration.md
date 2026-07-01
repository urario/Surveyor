---
type: Process
title: AI Collaboration Model
description: Working agreement for Claude Code and Codex on Surveyor.
tags: [process, ai-collaboration, claude-code, codex]
timestamp: 2026-06-30T00:00:00+09:00
---

# Roles

Claude Code is responsible for design pressure, review discipline, and knowledge curation. Codex is responsible for implementation slices, test execution, and concrete repository changes.

# Handoffs

| From | To | Handoff Content |
| -- | -- | -- |
| Claude Code | Codex | RQ IDs, design notes, test seams, risks |
| Codex | Claude Code | changed files, test evidence, unresolved risks |
| Either | OKF | durable decisions, trace links, workflow updates |

# Operating Rules

- Do not rely on private conversation memory for project policy.
- Put durable knowledge in `knowledge/`.
- Keep `RQ-xxx` visible in tasks, tests, and reviews.
- Prefer narrow implementation slices that can be reviewed independently.

# GitHub Issue / Project 運用

- 共通タスク管理は GitHub Issue / Project で行い、永続知識は OKF に残す。
- Issue のタイトルと本文は日本語を基本とし、`RQ-xxx`, `RD-xxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, `TRC-xxxx` は識別子としてそのまま使う。
- Project フィールドと標準ビューは [GitHub Issue and Project Workflow](github-issue-project-workflow.md) に従う。
- Claude Code は `Ready for Design` / `Design Review` の Issue を中心に扱う。
- Codex は `Ready for Implementation` 以降の Issue を中心に扱い、TDD 実装、検証、PR 証跡、必要な OKF 更新を行う。
- Human は優先度、仕様判断、実環境確認、最終 gate close を担当する。
