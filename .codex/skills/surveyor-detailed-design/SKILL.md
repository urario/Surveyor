---
name: surveyor-detailed-design
description: Author Surveyor detailed-design packages with contract closure, upstream consistency, DRP self-review evidence, and the review fix-loop protocol.
---

# Surveyor Detailed Design Authoring

Use this skill when creating or revising a `DES-xxxx` detailed-design package under `knowledge/design/`, or when responding to design-review findings on one.

## Canonical Inputs

- `knowledge/design/des-0007-detailed-design-execution-strategy.md` — package definitions (§4), execution rules (§5), artifact template (§6), review checklist (§9).
- `knowledge/process/design-review-patterns.md` — the `DRP-xxx` defect-pattern catalog.
- `knowledge/process/ai-design-review-strategy.md` — why the self-review gate exists and the round target.
- `knowledge/process/coding-standards.md` — public APIs a package fixes must respect the internal-default accessibility policy (`CS-02`), be specifiable as Japanese doc-comment contracts (`CS-01`), and use the GoF pattern vocabulary purpose-first (`CS-04`).
- The package's upstream `RQ`/`RD`/`DES`/`ADR` sources named in DES-0007 §4.

## Authoring Workflow

1. Re-open the upstream sources and write the **upstream inventory** the package touches: use cases, ports, states, modules, and decisions fixed by basic design and ADRs.
2. Fill every DES-0007 §6 template section, including **Contract closure**:
   - Port-method I/O derivation table — each input names its source (caller input / prior stage output / persisted state via a defined contract); each output names its consumer.
   - DTO field-ownership table — each introduced field names its single writer, write timing, and sync rule when the value is duplicated across models.
   - Round-trip inventory — every save/load, serialize/deserialize, mask/export pair named, both directions defined with symmetric types.
3. **Self-review before the PR**: sweep every `DRP-xxx` pattern and the DES-0007 §9 checklist against your own draft. Simulate each use case end-to-end from trigger to outputs. Fix what you find.
4. Record a **Self-Review Evidence** section in the PR body: one row per `DRP-xxx` with *checked clean* / *finding fixed (what)* / *not applicable (why)*. A design PR without this section is not ready for review.
5. Update OKF indexes/log and run `tools/okf/Validate-Okf.ps1`.
6. Request review with the single standard trigger (see `knowledge/process/ai-design-review-strategy.md` §3 L2), not per-role prompts.

## Fix-Loop Protocol (responding to review findings)

1. Classify each fix: **local** (wording, values, added table rows) or **boundary-reshaping** (types, ownership, call sequences, new/changed port methods).
2. For boundary-reshaping fixes, re-run the closure patterns `DRP-02`–`DRP-05` on the *entire reshaped boundary*, not just the reported symptom — PR #81 rounds 2–4 were each caused by skipping this.
3. Prefer resolving the boundary as a whole over stacking local patches: if two fixes in a row touch the same boundary, redesign that boundary's contract section instead of patching again.
4. Reply with a **contract diff** summary: which types/methods/owners changed, plus the closure re-sweep result.
5. Update the affected Contract closure tables in the artifact — they are the record the next reviewer scopes re-review by.

## Guardrails

The four blocking guardrails apply to every package: `RQ-048` read-only, `RQ-051` determinism (integer basis-point arithmetic in decision paths, stable hashing/ordinal ordering, injected clock), `RQ-052` confidentiality (secure-by-default, sanitized diagnostics), `RQ-054` layer separation (application-owned ports, inward dependencies).
