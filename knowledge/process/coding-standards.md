---
type: Process
title: Coding Standards
description: SOLID application rules, Japanese XML documentation comment rules, accessibility policy, and GoF pattern vocabulary for Surveyor C# implementation.
tags: [process, coding-standards, csharp, solid, documentation, design-patterns, rq-054, rq-051]
timestamp: 2026-07-03T00:00:00+09:00
---

# 目的と適用範囲

本書は Surveyor の C# 実装コードに適用するコーディング規約を定める。対象は `src/**` の全プロダクションコードであり、`tests/**` には [テストコードの緩和](#テストコードの緩和) を適用する。

[DES-0008](../design/des-0008-project-structure-and-test-harness.md) が定めるビルド機構(`TreatWarningsAsErrors`、`Nullable`、アナライザー、`BannedApiAnalyzers`、アーキテクチャテスト)を前提とし、本書はその上に載るコード表現の規約 — 原則、ドキュメントコメント、公開範囲、デザインパターン運用 — を定める。強制方針は DES-0008 と同じく「レビューの注意力ではなく、可能な限りビルドで守る」である。

実装・レビューを行う AI エージェント(Claude Code / Codex)と人間は、コードを書く前・レビューする前に本書を読むこと。

# 決定事項サマリ

| ID | 決定 | 強制手段 |
| -- | -- | -- |
| `CS-01` | `src/**` の全公開 API に日本語 XML ドキュメントコメントを必須とする | `GenerateDocumentationFile=true` + `CS1591`(`TreatWarningsAsErrors` によりビルドエラー) |
| `CS-02` | アクセシビリティは `internal` 既定とし、アセンブリ境界を越える契約のみ `public` にする | `InternalsVisibleTo` を `Directory.Build.props` で一元付与 + レビュー |
| `CS-03` | SOLID 原則を既存アーキテクチャガードレールへ写像して適用する | 一部機械的(依存方向・banned API)、残りはレビューゲート |
| `CS-04` | GoF パターンは語彙カタログとして共有し、適用時に目的とトレードオフを一行記録する(purpose-first) | 設計レビュー / PR レビューゲート |
| `CS-05` | コード解析は Microsoft の全 CA 規則(`AnalysisLevel=latest-All`)を有効化し、抑制は理由付きで一元管理する | `.editorconfig` + `TreatWarningsAsErrors`(全違反がビルドエラー) |
| `CS-06` | コードメトリクスしきい値をビルド時に強制する: サイクロマティック複雑度 ≤ 10、継承深度 ≤ 5、保守容易性指数 ≥ 20、クラス結合度 ≤ 30 | `CA1501`/`CA1502`/`CA1505`/`CA1506` + `CodeMetricsConfig.txt` |
| `CS-07` | コア層(Domain/Application/Policy/Reports)は行カバレッジ 80% 未満でユニットテスト失敗。他層はレポート記録のみ | coverlet しきい値ゲート |
| `CS-08` | 公開 API 面は `PublicAPI.Unshipped.txt` で明示追跡し、無断の公開 API 追加・変更をビルドエラーにする | `Microsoft.CodeAnalysis.PublicApiAnalyzers` |
| `CS-09` | 整形は `dotnet format --verify-no-changes` で差分ゼロを検証する | ユニットレーンの検証コマンド |
| `CS-10` | ミューテーションスコアをスライス完了時の定期ゲートとして記録する(コア層目標 ≥ 80%) | Stryker.NET(定期実行、ビルド非ブロッキング) |

# SOLID 原則の適用

SOLID は抽象的な標語ではなく、Surveyor の既存構造に写像して判定する。

| 原則 | Surveyor での具体規則 | 判定基準 |
| -- | -- | -- |
| SRP(単一責任) | 1 クラス = 1 責務。モジュール責務の割当は [DES-0002](../design/des-0002-module-responsibility-basic-design.md) が正であり、クラスはその中の一責務を担う。`Manager` / `Service` / `Helper` / `Util` のようなキャッチオール命名は責務不明のシグナルとして却下する(既存の設計レビューゲートと同一)。 | 「このクラスを変更する理由」が 1 文で言えること |
| OCP(開放閉鎖) | 変動が既知の点(スコアリング規則、レポート形式、マスキングポリシー、取得アダプター)はポート/Strategy で拡張に開く。変動が未知の点に投機的な抽象を足さない(YAGNI)。 | 新しいスコア規則・レポート形式の追加が既存コードの修正なしで可能なこと |
| LSP(置換可能性) | ポート実装は [DES-0003](../design/des-0003-module-interface-basic-design.md) の契約(エラー/結果モデル、キャンセル、読み取り専用 `RQ-048`、決定性 `RQ-051`)を弱めない。契約はポート側の XML ドキュメントコメントに明文化し、実装はそれを事前条件の強化・事後条件の弱化なしに満たす。 | フェイク実装と実アダプターが同じ契約テストを通ること |
| ISP(インターフェイス分離) | ポートは利用側ユースケース単位で小さく切る。実変動のない抽象・巨大な万能インターフェイスは作らない(既存の設計レビューゲートと同一)。 | 利用側が使わないメンバーを実装させられていないこと |
| DIP(依存性逆転) | 依存は内向きのみ。ポートは Application 層が所有し、具象は合成ルート(`Surveyor.App` / `M13`)だけが知る。[DES-0008](../design/des-0008-project-structure-and-test-harness.md) の `ProjectReference` グラフ + アーキテクチャテストで機械的に強制済み。 | `Surveyor.Architecture.Tests` が green であること |

# XML ドキュメントコメント(日本語)

## 対象と言語

- `src/**` の全公開 API — `public` な型(クラス、インターフェイス、構造体、record、列挙型、デリゲート)とその `public` / `protected` メンバー(メソッド、プロパティ、イベント、フィールド、コンストラクタ、演算子)— に XML ドキュメントコメントを付ける。
- 記述言語は日本語とする。型名・技術用語・要求 ID(`RQ-xxx` / `RD-xxx` / `DES-xxxx`)はそのまま英字で埋め込む。
- 用語は要求仕様の定義済み用語(UIA、AutomationId 等)に合わせ、独自の言い換えをしない。
- `internal` 型・メンバーへのコメントは必須ではないが、アセンブリ内で広く使われるものには `<summary>` を推奨する。

## 文体とタグ

- `<summary>` は簡潔な「である」調または体言止めで、1〜2 文。実装手順ではなく「何を提供するか / 何を表すか」を書く。
- 該当する場合は `<param>` / `<typeparam>` / `<returns>` / `<exception>` を省略しない。`<exception>` には呼び出し側が処理を分岐すべき例外のみを書く。
- ガードレールに関わる契約は `<remarks>` に明記する: 決定性(`RQ-051`)、読み取り専用(`RQ-048`)、機密データの扱い(`RQ-052`)、キャンセル/タイムアウト、スレッド制約。
- 契約はポート(インターフェイス)側に書き、実装側は `<inheritdoc/>` を使い、実装固有の差分(使用技術、追加の失敗モード)のみを追記する。

```csharp
/// <summary>
/// 対象アプリケーションの UI ツリーを読み取り専用で取得するポート。
/// </summary>
/// <remarks>
/// 実装は対象アプリケーションの状態を変更してはならない(RQ-048)。
/// 同一の対象・同一状態に対して決定的な結果を返すこと(RQ-051)。
/// 取得したテキストは機密データを含みうるため、そのままログへ出力してはならない(RQ-052)。
/// </remarks>
public interface IUiTreeAcquisitionPort
{
    /// <summary>
    /// 指定ウィンドウ配下の UI 要素ツリーを取得する。
    /// </summary>
    /// <param name="request">取得対象のウィンドウ参照と取得オプション。</param>
    /// <param name="cancellationToken">取得を中断するためのトークン。</param>
    /// <returns>取得結果。要素が取得不能の場合も低スコアではなく Unavailable として区別して返す。</returns>
    Task<UiTreeAcquisitionResult> AcquireAsync(UiTreeAcquisitionRequest request, CancellationToken cancellationToken);
}
```

## 機械的強制

- `Directory.Build.props` で `src/**` に `GenerateDocumentationFile=true` を設定する。`CS1591`(公開メンバーのドキュメント欠落)は既存の `TreatWarningsAsErrors=true` によりビルドエラーになる。
- テストプロジェクト(`tests/**`)と `Surveyor.TestSupport` はドキュメント生成の対象外とする(`GenerateDocumentationFile=false` または `NoWarn: CS1591`)。
- 生成コードは対象外とする。ただし `CS1591` は**コンパイラ警告**であって .NET アナライザー診断ではないため、`.editorconfig` の `generated_code = true` では抑制できない(`generated_code` はアナライザー診断のみに効く)。したがって生成物の除外は次のいずれかの**コンパイラ側**の機構で行う:
  - XAML/WinUI コード生成を含むプロジェクト(`Surveyor.App` / `Surveyor.Presentation`)で、生成 `Compile` 項目に限定して `CS1591` を `NoWarn` する(`<Compile Update="**/*.g.cs;**/*.g.i.cs;**/*.xaml.g.*.cs"><NoWarn>$(NoWarn);CS1591</NoWarn></Compile>`)。抑制は生成物パスに限定し、手書きコードには効かせない。
  - あるいはジェネレーターが `#pragma warning disable CS1591` を出力する生成物はそのまま尊重する。
  - この scoped-NoWarn の実配線はスキャフォールドスライスで確定する(下記残リスク)。手書きコードの `CS1591` 抑制は禁止。
- この設定は DES-0008 の決定性・品質設定表に統合済みであり、スキャフォールドスライスで適用する。

コメントの「存在」はビルドが守る。コメントの「質」(契約が書かれているか、summary が実装の言い換えになっていないか)はレビューが守る。

# 公開範囲ポリシー(internal 既定)

- 型の既定アクセシビリティは `internal` とする。`public` はアセンブリ境界を越えて使われる契約に限る:
  - `Surveyor.Application` のポート、ユースケース、DTO
  - `Surveyor.Domain` のモデル、キー、スコアリング結果、`IClock`
  - アダプター/`Reports`/`Policy` が合成ルートへ公開する登録用の型(実装クラスのコンストラクタ等)
  - `Surveyor.Presentation` の ViewModel と presentation ポート
- クラスは継承を設計した場合を除き `sealed` とする。継承を許す場合は `protected` メンバーの契約をドキュメントコメントに書く。
- テストからの `internal` アクセスは `InternalsVisibleTo` で許可する。付与は `Directory.Build.props` で一元管理し、各 `src` プロジェクト → 対応する `*.Tests` プロジェクトおよび `Surveyor.TestSupport` の規約ベース(`$(AssemblyName).Tests`)とする。
- この方針により、`CS-01` のドキュメント義務が「本当に他のアセンブリから使われる型」と正確に一致し、公開 API 面が最小化される(ISP/カプセル化の機械的裏付け)。

# GoF デザインパターン運用

## 原則: purpose-first

パターンは目的に仕えるときのみ適用する。既存の設計レビューゲート(purpose-first patterns)のとおり、目的なきパターン適用は却下される。適用時は設計文書(`DES-xxxx`)または PR 本文に「パターン名 / 目的 / 採らなかった単純案とその理由」を一行で記録する。

## 語彙カタログ

以下は Surveyor の典型状況と推奨パターンの対応表である。義務ではなく共有語彙であり、状況が一致するときに命名・構造をこの表に揃えることで実装の一貫性を得る。

| 状況 | 推奨パターン | 目的 | 過剰設計の兆候(適用しない条件) |
| -- | -- | -- | -- |
| ポート実装(UIA / Capture / Discovery / Store) | Adapter | 外部 API(COM UIA、WGC、ファイルシステム)を Application 所有ポートの背後に隔離する | — (アーキテクチャ上必須) |
| レポート出力(HTML / JSON) | Strategy + Template Method | 形式追加に開き、共通の出力手順(サニタイズ→整形→原子的書き込み)を固定する | 形式が 1 つしか存在せず追加予定もない段階での抽象化 |
| スコアリング規則・分類しきい値 | Strategy(バージョン付き設定と併用) | 規則の追加・差し替えを決定性(`RQ-051`)を保ったまま行う | 規則が定数 1 個で済む場合 |
| UIA ツリー → ドメインモデル構築 | Builder / Factory Method | 段階的構築と不変(immutable)モデル生成を分離する | 単純なコンストラクタ呼び出しで足りる小さな値オブジェクト |
| UI 要素ツリーの表現 | Composite | 画面・要素の再帰構造を統一的に扱う | — |
| 機密マスキングポリシー | Strategy(+ 必要時 Decorator) | 判断の集中(`RQ-052`)と段階的なマスク合成 | ポリシーが単一で合成が不要な段階での Decorator 導入 |
| ユースケースの入口 | Facade(ユースケースクラス) | ViewModel からの単純な入口を提供し、オーケストレーションを隠蔽する | — |
| 進捗・部分結果の通知 | Observer(`IProgress<T>` / イベント) | 実行中フィードバック(`RQ-051` に影響しない通知系) | 完了まで結果不要な処理 |
| 具象の生成と配線 | 合成ルートでの DI(Factory は合成ルート内に限定) | 具象知識を `M13` に一点集中する(DIP) | Service Locator の導入(禁止、下記) |

## 禁止・注意

- **Singleton(静的可変状態)**: 決定性(`RQ-051`)とテスト独立性を壊すため禁止。共有が必要なら DI のライフタイム管理で表現する。
- **Service Locator**: 依存が署名から見えなくなるため禁止。依存はコンストラクタ注入で宣言する。
- **パターン名を目的にした間接層**: 「Strategy にしたいから interface を切る」は逆である。変動点が先、パターンが後。

# テストコードの緩和

- XML ドキュメントコメントは不要(`CS1591` 抑制)。テストの意図はテスト名で表現する — テスト名は実装の仕組みではなく振る舞いを記述する(既存の TDD レビュー基準と同一)。
- `internal` へのアクセスは `InternalsVisibleTo` 経由で行い、テストのために対象を `public` へ昇格しない。
- リフレクションによる私的メンバーへのアクセスは禁止。テスト困難は設計のシグナルとして扱い、テストシーム(ポート/フェイク)を設計へ差し戻す。
- それ以外の規約(命名、Nullable、決定性、フェイクは `Surveyor.TestSupport` へ集約)はプロダクションコードと同一に適用する。

# 機械的品質ゲート(定量)

コードの品質は主観レビューではなく、ビルド・テスト実行で落ちる定量ゲートで担保する。Visual Studio 2026 では同じアナライザー群がエディタ上でリアルタイムに動く(`EnforceCodeStyleInBuild` + .NET アナライザー)ため、違反は実装時にその場で可視化され、ビルド(=CI ユニットレーン)で最終強制される。VS の「分析 > コード メトリックスの計算」で表示される指標と、ビルドで強制される `CA1501`/`CA1502`/`CA1505`/`CA1506` は同一の定義である。

## CS-05: コード解析 — Microsoft 全規則

- `Directory.Build.props` で `AnalysisLevel=latest-All` を設定し、既定重大度を持つ全 CA 規則を有効化する。`TreatWarningsAsErrors=true` により全違反がビルドエラーになる。
- **`latest-All` でも自動有効化されない規則がある点に注意する。** [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview#enable-additional-rules) の通り、`All` モードでも既定で無効(`severity: none`)のまま残る規則があり、`dotnet_diagnostic.CAxxxx.severity` による**明示オプトインが必須**である。これらは 2 群に分かれる。「全 CA 規則」の主張を維持するため、**両群とも規約で明示判断を確定する**(判断のない黙殺=「有効なはずが走らない」状態を残さない)。

  **(a) コードメトリクス系** — `CS-06` で `.editorconfig` にて明示昇格する: `CA1501`/`CA1502`/`CA1505`/`CA1506` を `error` へ、`CA1509`(`CodeMetricsConfig.txt` 書式検証)を `warning` 以上へ(下記 `CS-06` 表)。

  **(b) 既定無効の既存非メトリクス規則** — Microsoft が `All` でも意図的に無効にしている確立規則群。各規則の初期方針を次表で確定する:

  | 規則 | 内容 | Surveyor 初期方針 |
  | -- | -- | -- |
  | `CA1005` | ジェネリック型の過剰な型パラメータ回避 | **有効化**(`warning`)。SRP に資し、害がない |
  | `CA1060` | P/Invoke を `NativeMethods` クラスへ集約 | **有効化**(`warning`)。UIA/Capture アダプターの P/Invoke 整理に有用 |
  | `CA1045` | 参照渡し(`ref`)引数の回避 | **有効化**(`warning`)。必要な箇所は局所抑制+理由で逃がす |
  | `CA1021` | `out` パラメータの回避 | **意図的除外**。`TryXxx` パターン等 .NET 標準慣行のため。ポート契約は result 型優先(`DES-0003`)で乱用はレビューが抑制 |
  | `CA1014` | アセンブリへ `CLSCompliant` 付与 | **意図的除外**。外部ライブラリ配布せず C# 単一言語消費のため不要 |
  | `CA1017` | アセンブリへ `ComVisible` 付与 | **意図的除外**。本アセンブリを COM 公開しない(UIA COM interop は消費側であり公開側ではない) |

  上記以外に将来 SDK で既定無効規則が増えた場合も、スキャフォールドスライスでビルドログを突き合わせ、同様に**有効化するか意図的除外として理由付き記録するか**を確定する。
- プロジェクト実態に合わない規則(例: `CA1303` ローカライズ要求 — 本プロジェクトのメッセージは日本語リテラルが正、`CA2007` `ConfigureAwait` — アプリケーションコードでは不要)は `.editorconfig` の専用セクションで抑制し、**各行に理由コメントを付ける**。理由のない抑制はレビューで却下する。
- コード内の局所抑制(`#pragma warning disable` / `[SuppressMessage]`)は `Justification` 必須とし、PR レビューの明示的な確認対象とする。抑制の追加は「見えない品質劣化」ではなく「見える設計判断」として扱う。

## CS-06: コードメトリクスしきい値

`CodeMetricsConfig.txt`(`AdditionalFiles`)でしきい値を定義し、対応する CA 規則を `.editorconfig` でエラーに昇格して強制する(これらの規則は既定無効のため明示有効化が必要):

| 規則 | 指標 | しきい値 | 根拠 |
| -- | -- | -- | -- |
| `CA1502` | サイクロマティック複雑度(メソッド) | ≤ 10 | 分岐 10 超のメソッドはテストケース網羅が実務上崩れる。分割(メソッド抽出、Strategy 化)を強制する |
| `CA1501` | 継承深度 | ≤ 5 | 深い継承は LSP 違反の温床。Surveyor は合成優先(`CS-03`) |
| `CA1505` | 保守容易性指数 | ≥ 20(20 未満で違反) | VS のメトリクス計算と同一指標。長大・複雑・低凝集の複合検出 |
| `CA1506` | クラス結合度 | ≤ 30 | 結合過多は SRP 違反のシグナル。ポート分離(ISP)へ差し戻す |

- 正当な例外(enum 網羅の `switch`、UIA パターン種別の分岐など、分割すると却って読めなくなるもの)はメソッド単位の局所抑制+理由コメントで逃がす。しきい値自体は緩めない。
- 実装時の確認: ビルドで自動判定される。俯瞰したい場合は VS の「コード メトリックスの計算」でソリューション全体の数値を確認できる。

## CS-07: テストカバレッジゲート

- コア層のテストプロジェクト(`Surveyor.Domain.Tests` / `Surveyor.Application.Tests` / `Surveyor.Policy.Tests` / `Surveyor.Reports.Tests`)は coverlet のしきい値ゲートを有効にし、**行カバレッジ 80% 未満でテスト実行を失敗**させる(`/p:CollectCoverage=true /p:Threshold=80 /p:ThresholdType=line /p:ThresholdStat=total`、恒常設定はテストプロジェクト側 props に置く)。
- コア層は純粋ロジックであり TDD 前提(既存ワークフロー)のため 80% は下限であって目標ではない。カバレッジを満たすためだけのアサーションなしテストは TDD レビューで却下する。
- アダプター/`Presentation`/`App` はしきい値なし・レポート記録のみ。実 Windows API の薄いラッパーはユニットテスト対象外で、挙動は IT レーン(`IT-xxxx`)が担う。

## CS-08: 公開 API 面の追跡

- 全 `src` プロジェクトに `Microsoft.CodeAnalysis.PublicApiAnalyzers` を導入し、公開 API を `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt` で宣言的に管理する。宣言なしの公開 API 追加・シグネチャ変更はビルドエラー(`RS0016` 等)。
- これは `CS-02`(internal 既定)の機械化である: 「public にする」という判断が必ず `PublicAPI.Unshipped.txt` の diff として PR に現れ、レビュー可能になる。

## CS-09: 整形検証

- ユニットレーンの検証コマンドに `dotnet format --verify-no-changes` を含め、`.editorconfig` 違反の整形漏れを差分ゼロで検証する。

## CS-10: ミューテーションテスト(定期)

- Stryker.NET をコア層に対して定期実行し(実装スライス完了時、または複数スライスごと)、ミューテーションスコアを `knowledge/traces/` の evidence に記録する。コア層の目標スコアは **≥ 80%**。
- ビルド非ブロッキング。スコアはカバレッジより強い「テストが本当にバグを検出できるか」の指標であり、低下はレビューで扱う(カバレッジ稼ぎの形骸化テスト検出が主目的)。

## ゲートの実行タイミング

| タイミング | 動くゲート |
| -- | -- |
| エディタ入力時(VS 2026) | 全 CA 規則・スタイル規則のリアルタイム表示(実装時の即時フィードバック) |
| ビルド | `CS-01`(CS1591)、`CS-05`(全 CA 規則)、`CS-06`(メトリクス)、`CS-08`(公開 API)、依存方向(DES-0008)、banned API — 全て警告=エラー |
| ユニットテスト実行 | `CS-07`(カバレッジ 80%)、アーキテクチャテスト |
| ハンドオフ前 | `dotnet format --verify-no-changes`(`CS-09`)、対象テスト、OKF 検証 |
| スライス完了時(定期) | `CS-10`(Stryker.NET スコア記録) |

# 命名・スタイル

.NET 標準の命名規約(PascalCase 型・メンバー、camelCase ローカル/引数、`I` 接頭辞インターフェイス、`Async` 接尾辞)に従う。詳細なスタイル規則は `.editorconfig` が機械的に担い(DES-0008 所有)、本書は判断を要する規則のみ定める:

- 1 ファイル 1 型。ファイル名 = 型名。名前空間はフォルダ構造と一致([DES-0008](../design/des-0008-project-structure-and-test-harness.md) の名前空間規則)。
- 識別子・コード内の名前は英語。コメント・ドキュメントコメントは日本語。
- コメントは「コードが言えないこと」(契約、制約、根拠、`RQ-xxx` 参照)を書く。コードの逐語訳コメントは書かない。

# 強制の分担

| 規則 | 強制手段 | 所有 |
| -- | -- | -- |
| 公開 API ドキュメント存在(`CS-01`) | `CS1591` ビルドエラー | `Directory.Build.props`(DES-0008) |
| ドキュメント内容の質・日本語 | 実装 skill の自己チェック + レビュー | 本書 + レビュー skill |
| 依存方向(DIP) | `ProjectReference` + `Surveyor.Architecture.Tests` | DES-0008 |
| 決定性 API 禁止 | `BannedApiAnalyzers` | DES-0008 |
| アクセシビリティ既定(`CS-02`) | レビュー + `PublicAPI.Unshipped.txt` diff(`CS-08`) | 本書 + レビュー skill |
| SOLID(`CS-03`)・パターン運用(`CS-04`) | 設計レビュー / 実装レビューのチェックリスト | 本書 + `.claude` / `.codex` skill |
| 全 CA 規則(`CS-05`) | `AnalysisLevel=latest-All` + 理由付き抑制リスト | `Directory.Build.props` / `.editorconfig`(DES-0008) |
| メトリクスしきい値(`CS-06`) | `CA1501`/`CA1502`/`CA1505`/`CA1506` + `CodeMetricsConfig.txt` | DES-0008 |
| カバレッジ 80%(`CS-07`) | coverlet しきい値ゲート(コア層テスト実行時) | テストプロジェクト props(DES-0008) |
| 公開 API 追跡(`CS-08`) | `PublicApiAnalyzers` ビルドエラー | DES-0008 |
| 整形(`CS-09`) | `dotnet format --verify-no-changes` | ユニットレーン(DES-0008) |
| ミューテーションスコア(`CS-10`) | Stryker.NET 定期実行 + trace 記録 | 本書 + 実装 skill |
| 命名・スタイル | `.editorconfig` + `EnforceCodeStyleInBuild` | DES-0008 |

# Related

- [DES-0008 Project Structure and Test Harness](../design/des-0008-project-structure-and-test-harness.md)
- [DES-0002 Module Responsibility Basic Design](../design/des-0002-module-responsibility-basic-design.md)
- [DES-0003 Module Interface Basic Design](../design/des-0003-module-interface-basic-design.md)
- [Layering Principles](../architecture/layering-principles.md)
- [Lifecycle Traceability](lifecycle-traceability.md)
- [Quality Review Policy](quality-review-policy.md)
- [TDD and Traceability](tdd-and-traceability.md)
