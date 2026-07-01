# Surveyor Knowledge Log

## 2026-07-01

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
