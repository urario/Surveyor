# Surveyor Implementation Checklist

## Before Editing

- Relevant RQ IDs are known.
- Linked GitHub Issue is read when one exists; Japanese body and Project fields are treated as the task handoff.
- Lifecycle phase and durable artifact need are known.
- Test seam is identified.
- Existing OKF decisions are checked.
- Risk is classified as logic, Windows integration, output/reporting, or workflow.

## During Editing

- Keep changes scoped to one implementation slice.
- Add tests for pure logic first.
- Use fake adapters for UIA/capture/process boundaries.
- Avoid introducing WinUI dependencies below the shell layer.
- Keep generated output ordering explicit and stable.
- Apply `knowledge/process/coding-standards.md`: Japanese XML doc comments on every public API (`CS-01`), `internal`/`sealed` by default with `public` only for assembly-boundary contracts (`CS-02`), SOLID as mapped to Surveyor structures (`CS-03`), and purpose-first GoF pattern vocabulary (`CS-04`).
- State pattern name, purpose, and the rejected simpler alternative in one line when a design pattern is applied.

## Before Handoff

- Run targeted tests.
- Run OKF validation if knowledge files changed.
- Confirm every new public API is genuinely boundary-crossing and carries a Japanese doc comment whose `<remarks>` names the applicable guardrail contracts.
- State which RQ IDs were touched.
- State lifecycle phase and artifact IDs, or explain why PR evidence is enough.
- State which validations were not possible locally.
- Update or summarize the linked Issue with verification results and residual risk when Issue context exists.
