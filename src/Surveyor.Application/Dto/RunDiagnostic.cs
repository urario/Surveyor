using Surveyor.Domain.Keys;

namespace Surveyor.Application.Dto;

/// <summary>
/// 実行中に発生した安全な診断情報を表します。
/// </summary>
/// <param name="Code">診断コードです。</param>
/// <param name="Stage">診断が属する実行段階です。</param>
/// <param name="Severity">診断の重要度です。</param>
/// <param name="Status">診断に対応する操作状態です。</param>
/// <param name="ScreenKey">関連する画面キーです。存在しない場合は <see langword="null"/> です。</param>
/// <param name="ElementKey">関連する要素キーです。存在しない場合は <see langword="null"/> です。</param>
/// <param name="MessageTemplateId">表示用メッセージテンプレート ID です。</param>
/// <param name="SafeArgs">ログに出せる allowlist 済み引数です。</param>
/// <param name="ExceptionKind">例外種別です。例外由来でない場合は <see langword="null"/> です。</param>
/// <param name="HResult">安全に保持できる HRESULT です。存在しない場合は <see langword="null"/> です。</param>
/// <remarks>
/// raw 例外メッセージ、ウィンドウタイトル、UI テキスト、ファイルパスを保持してはいけません (RQ-052)。
/// </remarks>
public sealed record RunDiagnostic(
    string Code,
    RunStage Stage,
    DiagnosticSeverity Severity,
    OperationStatus Status,
    ScreenKey? ScreenKey,
    ElementKey? ElementKey,
    string MessageTemplateId,
    IReadOnlyDictionary<string, string> SafeArgs,
    ExceptionKind? ExceptionKind,
    int? HResult);
