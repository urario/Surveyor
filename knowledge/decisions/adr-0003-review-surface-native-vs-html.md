---
type: Decision
title: ADR-0003 Review Surface - Native WinUI Primary, HTML Portable
description: Native WinUI is Surveyor's primary interactive result-review surface; the generated HTML/JSON report is the portable distribution artifact, not the primary interactive surface.
tags: [adr, ui, winui, report, rq-011, rq-016, rq-030, rq-054]
timestamp: 2026-07-01T00:00:00+09:00
---

# Status

Accepted. Ratifies the review-surface decision fixed in [DES-0006 §4](../design/des-0006-screen-basic-design.md#4-review-surface-decision-native-vs-html-resolves-gap-a), resolving the "HTML report display — WebView2 in-app vs external browser" open item carried from [DES-0001](../architecture/des-0001-initial-architecture.md).

# Context

Surveyor's own operating UI must let users review analysis results. Two candidate surfaces exist and the requirements pull in different directions:

- Interactive review with two-way finding↔on-screen-location correspondence (`RQ-011`, `RQ-016`, `RQ-028`, `RQ-024`) needs a responsive, stateful surface bound to `AnalysisResult`.
- A human-readable, shareable report for developers/QA/leaders and for pasting into decks, plus a machine-readable form for comparison/CI/LLM re-review (`RQ-030`, `RQ-031`, `RQ-043`, `RQ-044`), needs a portable artifact.

`DES-0001` listed the HTML display host (WebView2 in-app vs external browser) as an open implementation choice, and `M01` names both "result browsing" and "report display" without deciding whether in-app review is native WinUI or the generated HTML. Left unresolved, this ambiguity forks downstream detailed design and implementation (how much projection logic `M02` owns vs. whether interaction is rebuilt inside HTML).

# Decision

- **Native WinUI is the primary interactive review surface.** Screens `SCR-04` (overview), `SCR-05` (element findings), and `SCR-06` (snapshot viewer) are native views bound to `AnalysisResult` through `M02` ViewModels. Only native views deliver the low-latency two-way list↔image correspondence the requirements demand, stay unit-testable through ViewModels (`RQ-054`), and remain responsive on large screens.
- **The HTML/JSON report is the portable distribution artifact, not the primary interactive surface.** `M10` produces it (`RQ-030`/`RQ-031`) for sharing, offline review, LLM/tool re-review, and report decks. `SCR-07` may **preview** the generated HTML in-app.
- **The in-app HTML host (WebView2 vs external browser) remains a detailed-design/implementation choice** under this decision; it no longer blocks basic design.

# Consequences

- `M02` owns the projection view-models for `SCR-04`–`SCR-06`; `M10` owns the portable artifact; the report writer never becomes the interactive surface.
- Interaction logic (correspondence, filtering, navigation) lives once, in ViewModels, and is verified without a live WinUI window (`UT-0011`, extended per `DES-0006`).
- `RQ-054` separation and `RQ-051` determinism are preserved: native views bind via ViewModels/presentation ports only; display order follows core-owned keys.
- Detailed design still decides the HTML preview host, XAML layout, and control set; this ADR does not fix those.

# Related

- [DES-0006 Screen (Operating UI) Basic Design](../design/des-0006-screen-basic-design.md)
- [DES-0001 Initial Architecture](../architecture/des-0001-initial-architecture.md)
- [DES-0002 Module Responsibility Basic Design](../design/des-0002-module-responsibility-basic-design.md)
- [ADR-0001 AI Collaboration and OKF](adr-0001-ai-collaboration-and-okf.md)
- [Lifecycle Traceability](../process/lifecycle-traceability.md)
