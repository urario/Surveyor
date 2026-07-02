---
type: Playbook
title: GitHub Issue and Project Workflow
description: Surveyor の GitHub Issue / Project を詳細設計、実装、テスト、レビューの作業管理に使うための運用ルール。
tags: [process, github, issue, project, traceability, okf]
timestamp: 2026-07-01T00:00:00+09:00
---

# GitHub Issue and Project Workflow

## 目的

GitHub Issue / Project は Surveyor の日々のタスク状態を管理する場所である。OKF は永続ナレッジを管理する場所である。両者を混同しない。

- Issue / Project: 作業の状態、担当、優先度、キュー、レビュー待ちを管理する。
- PR: 変更差分、検証結果、レビュー会話、マージ判断を管理する。
- OKF `knowledge/`: 後続フェーズに残す設計判断、プロセス、実装・テスト証跡を管理する。

Issue は原則として日本語で書く。ただし `RQ-xxx`, `RD-xxx`, `DES-xxxx`, `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, `TRC-xxxx` の識別子と Project のフィールド名・選択肢は英語表記を維持する。

## 基本方針

- 1つの Issue は、レビュー可能な1つの作業単位にする。
- 大きい作業は親 Issue と sub-issues に分ける。
- `DES-xxxx` は詳細設計の親 Issue として扱い、実装・テスト作業を sub-issues としてぶら下げる。
- `IMP-xxxx`, `UT-xxxx`, `IT-xxxx`, `TRC-xxxx` は、永続証跡が必要な場合だけ割り当てる。小さい変更は PR 証跡だけでよい。
- Issue 本文には長い要求文をコピーしない。`RQ-xxx` / `RD-xxx` と OKF へのリンクで辿れるようにする。
- Project のフィールドは検索・フィルタ・ビューのためのメタデータであり、設計判断そのものは OKF に残す。

## Issue タイトル

Issue タイトルは日本語で、先頭に主要な artifact ID を置く。

例:

```text
DES-0009: ドメインモデル、安定キー、可用性の詳細設計
IMP-0001: ScreenModel と ElementKey の最小実装
UT-0001: 表示名変更でキーが変わらないことを検証
IT-0001: 解析前後で対象アプリ状態が変わらないことを確認
レビュー: DES-0010 のスコアリング規則を品質観点で確認
```

ID が未確定の探索作業は `Spike:` で始め、確定後に artifact ID を付け直す。

## Issue 本文

Issue 本文は日本語で、最低限次を含める。

| 項目 | 内容 |
| -- | -- |
| 目的 | この作業で決めること、作ること、検証すること |
| 範囲外 | 今回扱わないこと |
| 関連要求 | `RQ-xxx` と必要なら `RD-xxx` |
| 上流 | 関連する `ADR-xxxx`, `DES-xxxx`, OKF 文書 |
| 下流 | 予定する実装、テスト、PR、証跡 |
| 完了条件 | レビュー可能な完了条件 |
| 検証 | 実行予定または実行済みのコマンド、手順 |
| 残リスク | Windows 固有制約、手動確認、未決事項 |

## Project フィールド

Project 名の推奨は `Surveyor Lifecycle Work`。Table ビューで以下のカスタムフィールドを持つ。

| Field | Type | Values / Example | 使い方 |
| -- | -- | -- | -- |
| `Status` | Single select | `Backlog`, `Ready for Design`, `In Design`, `Design Review`, `Ready for Implementation`, `In Implementation`, `Code Review`, `Done`, `Blocked` | 作業状態。Board の列に使う。 |
| `Phase` | Single select | `Requirements`, `Architecture`, `Basic Design`, `Detailed Design`, `Implementation`, `Unit Test`, `Integration Test`, `Review` | ライフサイクル上の主フェーズ。 |
| `Artifact` | Text | `DES-0009`, `IMP-0001`, `UT-0001` | 主要 artifact ID。複数ある場合は主要なものを先頭に書く。 |
| `RQ` | Text | `RQ-048 RQ-051` | 関連要求。複数可。 |
| `RD` | Text | `RD-020 RD-022` | 派生要求。複数可。 |
| `Guardrail` | Single select | `Read-only`, `Determinism`, `Confidentiality`, `Layering`, `None` | 主 guardrail。複数ある場合は label も併用する。 |
| `Owner Role` | Single select | `Human`, `Claude Code`, `Codex` | 現在の主担当ロール。 |
| `Priority` | Single select | `P0`, `P1`, `P2` | `P0` は blocking または MVP 必須。 |
| `Target` | Single select | `MVP`, `Post-MVP`, `Spike` | 作業の到達目標。 |

Project フィールドは英語固定にする。これは GitHub filter と agent 指示を安定させるためである。
補足: `Severity` は `04-review-finding.yml` のレビュー指摘 Issue Form 専用フィールドであり、標準 Project フィールドではない。レビュー指摘を Project 上で重要度別にフィルタしたくなった場合だけ、追加の Project フィールドとして `Severity` を作る。

## 推奨ビュー

| View | Layout | Filter / Group | 目的 |
| -- | -- | -- | -- |
| `Board - Flow` | Board | Group by `Status` | 日々の作業状態を動かす。 |
| `Table - Trace` | Table | なし、または `is:open` | `Artifact`, `RQ`, `RD`, `Phase` を一覧する。 |
| `Design Queue` | Table | `phase:"Detailed Design" -status:Done is:open` | Claude Code / 人間が詳細設計と設計レビューを処理する。 |
| `Implementation Queue` | Table | `status:"Ready for Implementation","In Implementation","Code Review","Blocked" is:open` | Codex が実装・テスト・PR 準備を処理する。 |

ビューは作業者の都合で追加してよいが、上記4つは共通ビューとして維持する。

## 親 Issue と sub-issues

詳細設計パッケージは親 Issue にする。

例:

```text
親: DES-0009: ドメインモデル、安定キー、可用性の詳細設計
子: UT-0001: 表示名変更でキーが変わらないことを検証
子: IMP-0001: ScreenModel と ElementIdentity を追加
子: レビュー: DES-0009 の決定性と機密性を確認
```

親 Issue は設計・判断・下流作業の束を示す。子 Issue は実作業単位にする。PR は子 Issue に紐付けるのを基本とし、親 Issue には必要に応じて進捗要約を残す。

## ロール別の使い方

| Role | 主な使い方 |
| -- | -- |
| Human | 優先度、受け入れ判断、仕様判断、実環境確認、最終 gate close を行う。 |
| Claude Code | `Ready for Design` から詳細設計を作成・レビューし、必要な OKF 更新を提案する。 |
| Codex | `Ready for Implementation` 以降の Issue を取り、TDD 実装、検証、PR 証跡、必要な `IMP`/`UT`/`IT` 更新を行う。 |
| Project Manager Agent | GitHub Project の状態、実行順序、依存関係、担当ロール、リスク、完了時の Project 更新を管理する。 |

AI agent は Issue の日本語本文を尊重し、必要な ID を消さない。曖昧な判断が残る場合は Issue に残リスクまたは open question として追記する。

## 実行計画と依存管理

Project Manager Agent は、作業開始前または Project 整理時に Issue 群を次の観点で分類する。

| 分類 | 意味 | 例 |
| -- | -- | -- |
| `Prerequisite` | 先に完了しないと下流が進められない作業 | `DES-0009` が未レビューのため `IMP-0001` を始めない |
| `Parallel` | 同時に進めても衝突しにくい作業 | 別 artifact の詳細設計、独立した UT 設計 |
| `Gate` | レビューまたは人間判断が必要な節目 | adapter spike の技術選定、最終受け入れ |
| `Blocked` | 具体的な unblocker がないと進められない作業 | 実 Windows 環境での確認待ち |

実行計画には、少なくとも次を含める。

- 次に着手する Issue 番号と artifact ID。
- 並列可能な Issue と、並列にしない理由。
- 依存 Issue、review gate、人間判断 gate。
- 推奨する `Owner Role` と `Status`。
- guardrail risk、依存 risk、環境/manual risk、scope risk、残リスク。

`Owner Role` は「今の次アクションを取る主体」を表す。過去に作業した主体ではない。

## 完了時報告と Project 更新

Claude Code / Codex / Human は、作業完了時に Project Manager Agent へ日本語で完了報告を渡す。Project Manager Agent は報告内容を確認してから GitHub Project を更新する。

完了報告の最小テンプレート:

```text
対象Issue:
担当:
実施内容:
成果物/変更:
関連RQ: Issue / Project にある場合。なければ N/A。
関連RD: Issue / Project にある場合。なければ N/A。
検証:
OKF更新:
残リスク:
次の推奨Status:
次のOwner Role:
```

Project Manager Agent は次を確認する。

- Issue 番号、`Phase`、`Artifact` が報告に含まれる。`RQ` / `RD` は Issue / Project フィールドに存在する場合だけ必須とし、存在しない場合は `N/A` を明記してよい。Unit Test / Integration Test / Review の Issue で `RD` が無い場合、`RD-xxx` を推測して作らない。
- 検証結果、レビュー結果、または未検証理由が明記されている。
- 永続知識が変わった場合、OKF 更新と `tools/okf/Validate-Okf.ps1` の結果がある。
- `Blocked` にする場合、blocker、unblocker owner、次の質問または作業が明記されている。
- `Done` にする場合、Done 条件を満たし、人間 gate が残っていない。

標準の Status 遷移:

- 設計作業がレビュー可能になったら `Design Review`。
- 設計レビュー指摘が閉じた、または下流 Issue として追跡されたら `Ready for Implementation`。
- 実装・テスト作業が PR / 検証証跡つきでレビュー可能になったら `Code Review`。
- PR、検証、OKF、残リスク、人間 gate が閉じたら `Done`。
- unblocker が必要な場合だけ `Blocked`。この場合 `Owner Role` は unblocker の主体にする。

Codex / Claude Code から GitHub Project を更新できる範囲は、原則として item 追加、field 作成、field 値更新、item 並び替えである。ビューの作成、表示列、フィルタ、grouping は GitHub の公開 CLI/API では制御できない場合があるため、その場合は手動 UI 手順として明記する。

## Done の条件

Issue を `Done` にする前に、該当する範囲で確認する。

- Project フィールドの `Phase`, `Artifact`, `Status` が最新である。`RQ` / `RD` は Issue / Project フィールドに存在する場合だけ最新化し、存在しない場合は `N/A` または空欄を許容する。
- PR がある場合、PR に Issue へのリンクと検証結果がある。
- 永続知識が必要な場合、OKF 文書が更新されている。
- OKF を更新した場合、`tools/okf/Validate-Okf.ps1` が通っている。
- 残リスクが Issue または PR に明記されている。
- 人間の最終判断が必要な gate は、人間が明示的に close している。

## 関連

- [AI Collaboration](ai-collaboration.md)
- [Git Policy](git-policy.md)
- [Lifecycle Traceability](lifecycle-traceability.md)
- [TDD and Traceability](tdd-and-traceability.md)
- [Quality Review Policy](quality-review-policy.md)
