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

# Design

* [Design Knowledge](design/index.md) - Basic and detailed design artifact home and lifecycle rules.

# Decisions

* [ADR-0001 AI Collaboration and OKF](decisions/adr-0001-ai-collaboration-and-okf.md) - Initial AI role split and knowledge management decision.

# Trace Evidence

* [Trace Evidence](traces/index.md) - Implementation, unit test, and integration test evidence notes.

# Process

* [AI Collaboration](process/ai-collaboration.md) - Claude/Codex collaboration model.
* [Git Policy](process/git-policy.md) - Branch, commit, pull request, and protection rules.
* [Lifecycle Traceability](process/lifecycle-traceability.md) - Phase artifact, ID, and trace block rules.
* [OKF Policy](process/okf-policy.md) - Local rules for OKF scope, frontmatter, and canonical sources.
* [Quality Review Policy](process/quality-review-policy.md) - ISO/IEC 25010-oriented quality review gates for lifecycle artifacts and agents.
* [TDD and Traceability](process/tdd-and-traceability.md) - Development workflow and evidence expectations.
