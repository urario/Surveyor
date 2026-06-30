# Quality Review Output Template

Use this template when the user asks for a formal Surveyor quality review.

## Findings

List findings first. Omit this section only if there are no findings.

```text
[Severity] path/to/file.ext:line
Phase: Requirements | Architecture | Basic design | Detailed design | Implementation | Unit test | Integration test | PR evidence
Quality axis: ISO/IEC 25010 characteristic or Surveyor guardrail
Evidence: concrete mismatch, missing evidence, or risk
Recommendation: smallest reviewable correction
```

## Open Questions

List only questions that affect the verdict.

## Residual Risk

Name risks that remain even if the artifact is accepted, especially Windows UI, DPI, occlusion, process integrity, screenshot APIs, privacy, nondeterminism, and manual validation.

## Verdict

Use exactly one:

- `Reject`
- `Needs changes`
- `Accept with risks`
- `Accept`
