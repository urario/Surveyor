# Surveyor Knowledge Log

## 2026-07-01

* **Update**: Applied independent basic-design review (`surveyor-quality-auditor`) corrections to `DES-0002`–`DES-0005`. Closed `F-Major-1` by assigning owners and downstream tests for previously unmapped in-scope requirements — `RD-002` (screen/state-differentiated evaluation unit → `M04`), `RD-015` (improvement-candidate generation → `M08`), `RD-016` (prioritization support via user-supplied `ScreenSelectionMetadata` → `M04`/`M03`/`M10`) — and corrected the `DES-0002` trace block from an overclaimed `RD-001`–`RD-032` range to the actually mapped set with explicit carve-outs for `RD-027`–`RD-029`/`RD-031`. Closed `F-Med-1` by splitting the collapsed evaluation trace into per-axis rows (`RD-007`/`RD-008`/`RD-009`/`RD-010` and `RQ-005`/`RQ-019`–`RQ-023`) with named `UT-0002` viewpoints. Addressed `F-Min-1` (M04 derives keys from non-sensitive stable identity; sensitive-fallback hashing delegated to `M09`) and `F-Min-2` (target-discovery candidate ordering scoped as within-session user-selection determinism, distinct from report/`ScreenKey` determinism). Guardrails `RQ-048`/`RQ-051`/`RQ-052`/`RQ-054` had passed the review unchanged.
* **Creation**: Added the basic design phase as `DES-0002`–`DES-0005` under `knowledge/design/`, taking `DES-0001` as input: `DES-0002` module responsibilities (13-module map, ownership layers, data ownership, guardrail assignment), `DES-0003` module interface contracts for every port and use case (direction, I/O, result/error model, cancellation, read-only/determinism/confidentiality constraints, fake strategy, open items, RQ/RD/UT/IT trace), `DES-0004` end-to-end analysis flow with run state machine, staged contracts, cancellation/partial-result rules, and guardrail checkpoints, and `DES-0005` V-model traceability mapping basic-design items to detailed design/implementation/UT/IT with planned `UT-0001`–`UT-0012` and `IT-0001`–`IT-0006` obligations and Codex slice candidates. Linked all four from `knowledge/index.md` and `knowledge/design/index.md`.
* **Creation**: Added `DES-0001` initial architecture design (Clean Architecture layers, MVVM boundaries, ports, determinism/read-only/confidentiality policies, technology allocation recommendation, and Codex slices) under `knowledge/architecture/`.
* **Update**: Applied third-party architecture review corrections to `DES-0001` — added `ITargetDiscoveryPort`/`SelectTargetUseCase` for the process/window boundary, separated `DisplayLabel` from key material with sanitization/hashing rules, made report/store operations cancellable with atomic writes, promoted read-only verification to a required adapter audit test plus an `IT-xxxx` invariant set, and aligned the trace block and per-slice `RQ`/`RD` with the driving requirements.
* **Enhancement**: Strengthened the `surveyor-architect` agent and `surveyor-design-review` skill for Clean Architecture, MVVM, interface-design, technology-allocation, and Mermaid-artifact review, with `RQ-048`/`RQ-051`/`RQ-052`/`RQ-054` as blocking guardrails, and added target-discovery-boundary, key/label separation, output/store cancellation, and read-only adapter-audit review items from the review.
* **Update**: Corrected requirement-definition review findings `F-01`, `F-03`, `F-04`, `F-05`, `F-08`, `O-01`, and `O-02` by aligning `RQ-002` traces, strengthening acceptance criteria, and linking OKF review summaries to the source document.
* **Creation**: Added OKF requirement-definition knowledge for the `RD-xxx` requirement definitions derived from the canonical `RQ-xxx` specification.
* **Creation**: Added lifecycle traceability policy for phase artifacts, stable artifact IDs, and required trace evidence blocks.
* **Creation**: Added `knowledge/design/` and `knowledge/traces/` as durable homes for design artifacts and trace evidence.
* **Creation**: Added Git policy for topic branches, pull requests, and no direct commits or pushes to `main`.
* **Creation**: Added Surveyor Git workflow skills and local Git hooks for branch guardrails.
* **Update**: Made RQ index refresh stable by preserving the existing generated timestamp and ignoring that timestamp in freshness checks.
* **Update**: Strengthened OKF validation for broken markdown links, index reachability, and generated RQ index freshness.
* **Update**: Changed generated RQ index output to UTF-8 without BOM and added OKF validation for BOM-free Markdown frontmatter previews.
* **Decision**: Added the common rule that uncertain requirements, architecture, process, Git workflow, and prior decisions start from `knowledge/index.md`.
* **Creation**: Added an ISO/IEC 25010-oriented quality review policy and independent quality review Agent/Skill assets.

## 2026-06-30

* **Initialization**: Created the OKF-style knowledge bundle for Surveyor.
* **Creation**: Added initial requirement source, architecture, process, and AI collaboration concepts.
* **Creation**: Added generated RQ index support through `tools/requirements/Export-RqIndex.ps1`.
* **Decision**: Established `.codex/skills/` as the canonical source for Codex skills; `~/.codex/skills` copies are optional personal installs.
* **Decision**: Clarified that OKF conformance applies to `knowledge/`; the requirement specification remains an ordinary canonical source document without OKF frontmatter.
* **Update**: Renamed the canonical requirements file to `docs/gui-testability-analyzer-requirements.md` for tool-friendly paths.
