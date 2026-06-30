# Surveyor Implementation Checklist

## Before Editing

- Relevant RQ IDs are known.
- Test seam is identified.
- Existing OKF decisions are checked.
- Risk is classified as logic, Windows integration, output/reporting, or workflow.

## During Editing

- Keep changes scoped to one implementation slice.
- Add tests for pure logic first.
- Use fake adapters for UIA/capture/process boundaries.
- Avoid introducing WinUI dependencies below the shell layer.
- Keep generated output ordering explicit and stable.

## Before Handoff

- Run targeted tests.
- Run OKF validation if knowledge files changed.
- State which RQ IDs were touched.
- State which validations were not possible locally.

