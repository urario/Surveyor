using Surveyor.Application.Dto;

namespace Surveyor.Application.Ports;

/// <summary>
/// 対象の UI ツリーを読み取り専用で取得しドメインモデルへ変換するポートです。
/// </summary>
/// <remarks>
/// 実装は対象アプリケーションの状態を変更してはいけません (RQ-048)。同一フィクスチャ / 同一入力では要素順序・キー・
/// confidence・availability マーカーが決定的でなければなりません (RQ-051)。raw HWND、パス、ウィンドウタイトル、UI テキストは
/// キー素材へ渡さず、抽出テキストは <c>DisplayLabel</c> にのみ載せます (RQ-052)。既知の取得失敗は例外ではなく
/// <see cref="AcquisitionResult.Status"/> で返し、呼び出し側のキャンセルは <see cref="OperationCanceledException"/> として伝播します
/// (DES-0011 / DES-0014)。
/// </remarks>
public interface IUiTreeAcquisitionPort
{
    /// <summary>
    /// 対象の UI ツリーを取得しドメインモデルへ変換します。
    /// </summary>
    /// <param name="target">取得対象への参照です。</param>
    /// <param name="options">取得の実行オプションです。</param>
    /// <param name="cancellationToken">取得を中断するためのトークンです。</param>
    /// <returns>取得結果です。</returns>
    Task<AcquisitionResult> AcquireAsync(
        TargetReference target,
        AcquisitionOptions options,
        CancellationToken cancellationToken);
}
