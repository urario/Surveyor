# Trace Evidence

Use this directory for durable implementation, unit test, integration test, and cross-phase trace evidence.

Follow [Lifecycle Traceability](../process/lifecycle-traceability.md):

- Use `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, or `TRC-xxxx` when a standalone trace note is needed.
- Prefer source files, test files, and PR descriptions for local evidence that does not need a durable OKF concept.
- Record residual risk explicitly when behavior depends on real Windows UI, DPI, occlusion, process integrity, screenshot APIs, or manual validation.

## Trace Notes

- [TRC-0001 ADR-0002 Spike Measurement Evidence](trc-0001-adr-0002-spike-measurements.md) - Per-axis measurement evidence (read-only, determinism, fixtureability, permissions, packaging, performance) from running the ADR-0002 PoCs against a real MFC target (human-run), a large Chromium tree, and WGC failure-mode probes; feeds the ADR-0002 recommendation and DES-0011/DES-0014/DES-0015 design inputs.
- [TRC-0002 DES-0008 Scaffold Implementation Evidence](trc-0002-des-0008-scaffold-evidence.md) - Solution/project scaffold, core unit-lane solution filter, architecture-test behavior names, failing-first forbidden-reference red log, and verification evidence for `RQ-054` / `RQ-051` under `DES-0008`.
- [UT-0001 Stable Key Behavior Evidence](ut-0001-stable-key-behavior-evidence.md) - Behavior-test evidence for DisplayLabel/key separation, fresh-process determinism, screen-state identity, availability preservation, fallback token non-reversibility, and counter-example red evidence under `DES-0009`.
- [IMP-0001 Domain Model Stable Key Implementation](imp-0001-domain-model-stable-key-implementation.md) - Minimal Domain/Application/Policy implementation evidence for `ScreenModel`, `UiElement`, stable keys, `IdentityMaterial`, `IFallbackKeyDerivation`, and `Sha256FallbackKeyDerivation`.
- [IMP-0002 Scoring Skeleton Implementation](imp-0002-scoring-skeleton-implementation.md) - Implementation evidence for deterministic M08 scoring, classification, root-cause de-duplication, and improvement candidate generation.
- [UT-0002 Scoring Determinism Evidence](ut-0002-scoring-determinism-evidence.md) - Unit-test evidence for scoring determinism, unavailable semantics, classification boundaries, config validation, dictionary-order independence, and no fabricated priority.
- [UT-0008 Confidentiality Policy Behavior Evidence](ut-0008-confidentiality-policy-evidence.md) - Behavior-test evidence for secure-by-default decisions, both mask-all / allow-all branches, recorded opt-out, non-reversible text masking, exception sanitization, fallback-key export pseudonyms, and R-QA-01 counter-example red evidence under `DES-0013`.
- [IMP-0003 Confidentiality Policy Implementation](imp-0003-confidentiality-policy-implementation.md) - Implementation evidence for the M09 secure-by-default confidentiality policy, allowlist sensitive-value sanitizer, and fallback-key export pseudonym mapper that turn `UT-0008` green.
