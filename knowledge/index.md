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
* [Requirement Definition](requirements/requirements-definition.md) - Derived `RD-xxx` requirement definitions and downstream trace guidance.
* [Generated RQ Index](requirements/rq-index.generated.md) - Generated list of `RQ-xxx` headings.

# Architecture

* [Layering Principles](architecture/layering-principles.md) - Required layer split and design guardrails.
* [DES-0001 Initial Architecture](architecture/des-0001-initial-architecture.md) - Clean Architecture and MVVM design, ports, technology allocation, and downstream slices for the initial version.

# Design

* [Design Knowledge](design/index.md) - Basic and detailed design artifact home and lifecycle rules.
* [DES-0002 Module Responsibility Basic Design](design/des-0002-module-responsibility-basic-design.md) - Module responsibilities, ownership layers, and guardrail assignment for the initial version.
* [DES-0003 Module Interface Basic Design](design/des-0003-module-interface-basic-design.md) - Basic-design contracts for every port and use-case boundary.
* [DES-0004 Analysis Flow Basic Design](design/des-0004-analysis-flow-basic-design.md) - Run flow, state machine, staged contracts, and guardrail checkpoints.
* [DES-0005 V-Model Traceability and Downstream Tests](design/des-0005-vmodel-traceability-and-downstream-tests.md) - V-model mapping and planned unit/integration test obligations.
* [DES-0006 Screen (Operating UI) Basic Design](design/des-0006-screen-basic-design.md) - Operating-UI screen inventory, navigation, per-screen bindings, review-surface decision, snapshot correspondence, and usability principles.

# Decisions

* [ADR-0001 AI Collaboration and OKF](decisions/adr-0001-ai-collaboration-and-okf.md) - Initial AI role split and knowledge management decision.
* [ADR-0003 Review Surface - Native WinUI Primary, HTML Portable](decisions/adr-0003-review-surface-native-vs-html.md) - Native WinUI is the primary interactive review surface; the HTML/JSON report is the portable distribution artifact.

# Trace Evidence

* [Trace Evidence](traces/index.md) - Implementation, unit test, and integration test evidence notes.

# Process

* [AI Collaboration](process/ai-collaboration.md) - Claude/Codex collaboration model.
* [Git Policy](process/git-policy.md) - Branch, commit, pull request, and protection rules.
* [Lifecycle Traceability](process/lifecycle-traceability.md) - Phase artifact, ID, and trace block rules.
* [OKF Policy](process/okf-policy.md) - Local rules for OKF scope, frontmatter, and canonical sources.
* [Quality Review Policy](process/quality-review-policy.md) - ISO/IEC 25010-oriented quality review gates for lifecycle artifacts and agents.
* [TDD and Traceability](process/tdd-and-traceability.md) - Development workflow and evidence expectations.
