---
okf_version: "0.1"
type: Unit Test Evidence
id: UT-0011
title: Presentation ViewModel State Machine Evidence
status: verified-with-risk
tags: [surveyor, unit-test, presentation, viewmodel, rq-046, rq-052, rq-054]
---

# UT-0011 Presentation ViewModel State Machine Evidence

## Trace

| Field | Evidence |
| --- | --- |
| Artifact | `UT-0011`, ViewModel state machine and presentation-port fake tests |
| Issue | #50 |
| Upstream | `DES-0016`, `DES-0005`, `DES-0006`, `DES-0011`, `DES-0013`, `DES-0015` |
| Requirements | `RQ-046`, `RQ-052`, `RQ-054`; `RD-016`, `RD-022`, `RD-025`, `RD-028`, `RD-030` |
| Downstream | `IMP-0012` (#70), `IT-0007` (#59) |
| Test files | `tests/Surveyor.Presentation.Tests/UT0011*BehaviorTests.cs`, `RecordingPresentationFakes.cs`, `PresentationTestData.cs` |

## Behavior Coverage

- Metadata gate: `Run` stays disabled until a target is resolved and `ScreenSelectionMetadata` is recorded; navigation alone does not enable analysis.
- Metadata transparency: the recorded `ScreenSelectionMetadata` instance reaches `AnalysisRunRequest.ScreenSelectionMetadata` unchanged, including the `PriorityBasisSource` value.
- Run state/activity pair: progress drives `Analyzing` -> `Capturing` -> `Exporting` while `RunActivityKind` stays `AnalysisRun`, so in-run `Exporting` is distinguishable from post-review export.
- Cancel behavior: analysis-run cancel records `ConfirmRunCancel`, cancels the token, and resets to `Idle`; post-review report cancel returns to `Completed` with session results kept and no run-cancel dialog.
- SCR-05/SCR-06 selection sync: finding and region selection synchronize by `FindingId`/`RegionId`, preserve carried order, and keep uncapturable regions visible as markers.
- Confidentiality behavior: local opt-out requires a confirmed dialog and allowlisted reason code; dismissal keeps `ProtectedLocal`; plaintext preview under `ExplicitLocalOptOut` requires `ConfirmPlaintextPreview` before `IHtmlPreviewHost.OpenAsync`.

## TDD and Counter-Example Evidence

- Failing-first intent was authored before the production files, but an isolated red command was not preserved before the production patch was applied. The first recorded command after production surfaced build/API/analyzer failures, then the behavior suite reached green. This remains a DoD gap for #50 until PR review accepts the evidence or a worker reproduces a red-only snapshot.
- `R-QA-01` counter-example coverage is encoded by the oracles:
  - a gate-bypass implementation would make `CanRun` true before metadata and fail the metadata-gate test;
  - an index-based selection sync would select `finding-a`/`region-ok` incorrectly because the fixture order is deliberately non-alphabetical;
  - a plaintext preview implementation that calls the host before confirmation would fail on an empty expected preview call log.
- Second-pass smell check: tests drive command methods and fake port call logs, not property setters only; no live WinUI window, dispatcher thread, filesystem write, or adapter port is used.

## Verification

| Command | Result |
| --- | --- |
| `dotnet test tests\Surveyor.Presentation.Tests\Surveyor.Presentation.Tests.csproj --filter UT0011 --no-restore` | PASS: 7 tests |
| `dotnet test tests\Surveyor.Architecture.Tests\Surveyor.Architecture.Tests.csproj --no-restore` | PASS: 8 tests |
| `dotnet test eng\Surveyor.Unit.slnf --no-restore` | PASS: Architecture 8, Presentation 7, Application 31, Domain 59, Policy 45 |
| `dotnet build Surveyor.slnx --no-restore` | PASS: 0 warnings, 0 errors |
| `git diff --check` | PASS |

## Residual Risk

- Review gate #38 for `DES-0016` is still open as of implementation; #50 should not be marked `Done` until human/design gate disposition is recorded.
- The strict red-before-production log required by #50 was not captured as a standalone command. Behavior tests are green and counter-example oracles are present, but this evidence should be called out in PR review rather than hidden.
- Tests prove ViewModel state and port intents only. Pixel layout, keyboard traversal, live accessibility, and real external-browser behavior remain `IT-0007` scope.
- No durable log of temporary source mutations was kept; counter-example coverage is represented by fixture/oracle shape and test failure modes.
