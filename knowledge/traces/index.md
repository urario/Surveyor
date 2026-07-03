# Trace Evidence

Use this directory for durable implementation, unit test, integration test, and cross-phase trace evidence.

Follow [Lifecycle Traceability](../process/lifecycle-traceability.md):

- Use `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, or `TRC-xxxx` when a standalone trace note is needed.
- Prefer source files, test files, and PR descriptions for local evidence that does not need a durable OKF concept.
- Record residual risk explicitly when behavior depends on real Windows UI, DPI, occlusion, process integrity, screenshot APIs, or manual validation.

## Trace Notes

- [TRC-0001 ADR-0002 Spike Measurement Evidence](trc-0001-adr-0002-spike-measurements.md) - Per-axis measurement evidence (read-only, determinism, fixtureability, permissions, packaging, performance) from running the ADR-0002 PoCs against a real MFC target (human-run), a large Chromium tree, and WGC failure-mode probes; feeds the ADR-0002 recommendation and DES-0011/DES-0014/DES-0015 design inputs.

