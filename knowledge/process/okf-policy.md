---
type: Process
title: OKF Policy
description: Local OKF rules for Surveyor knowledge scope, frontmatter, canonical sources, and traceability.
tags: [process, okf, traceability]
timestamp: 2026-06-30T00:00:00+09:00
---

# Scope

Surveyor uses `knowledge/` as the OKF-style knowledge bundle. OKF conformance applies to files inside `knowledge/`, not to every markdown file in the repository.

# Frontmatter Policy

- Every non-reserved concept document under `knowledge/` must have YAML frontmatter with a non-empty `type`.
- Root `knowledge/index.md` may include frontmatter to declare bundle metadata such as `okf_version`.
- Nested `index.md` files and `log.md` files are reserved OKF navigation/history files and do not require concept frontmatter.
- Markdown files outside `knowledge/`, including requirements and workflow documents under `docs/`, are ordinary project documents unless explicitly moved into the OKF bundle.

# Canonical Requirements

The canonical requirement specification is [gui-testability-analyzer-requirements.md](../../docs/gui-testability-analyzer-requirements.md). It intentionally does not need an OKF YAML block because it is not itself an OKF concept document.

The OKF concept [Requirement Source](../requirements/source-spec.md) points to that canonical document and provides the OKF-facing metadata. The generated [Surveyor RQ Index](../requirements/rq-index.generated.md) extracts `RQ-xxx` headings for traceability.

# Naming Policy

Prefer ASCII, kebab-case filenames for durable project artifacts. This reduces friction across shells, scripts, URLs, and AI tooling. Japanese text is fine in document bodies and titles.

# Traceability Policy

- Preserve `RQ-xxx` IDs exactly.
- Link OKF concepts to source requirements, decisions, tests, and implementation artifacts as they appear.
- Do not duplicate long requirement sections in OKF; summarize and link to the canonical source.
- Update `knowledge/log.md` for meaningful knowledge changes.
- When uncertain about requirements, architecture, process, Git workflow, or prior decisions, start from `knowledge/index.md` and follow the linked concepts.

# Validation Policy

- `tools/okf/Validate-Okf.ps1` must pass before OKF-affecting changes are handed off.
- Validation covers required frontmatter, broken markdown links, reachability from `knowledge/index.md`, and generated RQ index freshness.
- `tools/requirements/Export-RqIndex.ps1` preserves the existing generated `timestamp` when refreshing the RQ index; freshness comparison ignores that timestamp line.
