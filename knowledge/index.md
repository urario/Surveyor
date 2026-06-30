---
okf_version: "0.1"
type: Knowledge Bundle
title: Surveyor Knowledge Bundle
description: OKF-style project knowledge for the Surveyor GUI testability analyzer.
tags: [surveyor, okf, traceability]
timestamp: 2026-06-30T00:00:00+09:00
---

# Surveyor Knowledge Bundle

This bundle keeps project knowledge close to the source code so agents and humans can trace requirements, decisions, tests, and implementation work.

# Requirements

* [Requirement Source](requirements/source-spec.md) - Canonical requirements document and trace rules.
* [Generated RQ Index](requirements/rq-index.generated.md) - Generated list of `RQ-xxx` headings.

# Architecture

* [Layering Principles](architecture/layering-principles.md) - Required layer split and design guardrails.

# Decisions

* [ADR-0001 AI Collaboration and OKF](decisions/adr-0001-ai-collaboration-and-okf.md) - Initial AI role split and knowledge management decision.

# Process

* [AI Collaboration](process/ai-collaboration.md) - Claude/Codex collaboration model.
* [OKF Policy](process/okf-policy.md) - Local rules for OKF scope, frontmatter, and canonical sources.
* [TDD and Traceability](process/tdd-and-traceability.md) - Development workflow and evidence expectations.
