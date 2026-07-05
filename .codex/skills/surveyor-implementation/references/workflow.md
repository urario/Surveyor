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

- Run targeted tests (core-layer coverage gate ≥ 80% line must pass, `CS-07`).
- Run `dotnet format --verify-no-changes` (`CS-09`).
- Run OKF validation if knowledge files changed.
- Confirm every new public API is genuinely boundary-crossing, carries a Japanese doc comment whose `<remarks>` names the applicable guardrail contracts, and has its `PublicAPI.Unshipped.txt` entry (`CS-08`).
- List every new analyzer/metrics suppression added in this slice with its justification; zero suppressions is the expected default.
- On slice completion (or per the agreed cadence), run Stryker.NET on the core layers and record the mutation score in the trace evidence (`CS-10`, target ≥ 80%).
  Use the canonical playbook in `knowledge/process/stryker-workflow.md`.
  Default commands: `dotnet tool restore`, then `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\stryker\Run-StrykerBaseline.ps1 -SkipVersionCheck`.
  If the score is below 80%, treat it as non-blocking baseline evidence unless the governing Issue says otherwise, and record surviving-mutant concentration plus improvement candidates.
- State which RQ IDs were touched.
- State lifecycle phase and artifact IDs, or explain why PR evidence is enough.
- State which validations were not possible locally.
- Update or summarize the linked Issue with verification results and residual risk when Issue context exists.
