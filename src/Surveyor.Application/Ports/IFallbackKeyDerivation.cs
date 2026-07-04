using Surveyor.Domain.Model;

namespace Surveyor.Application.Ports;

/// <summary>
/// raw 表示テキストから fallback key token を導出するポートです。
/// </summary>
/// <remarks>
/// 実装は対象アプリ由来の raw text を可逆な形で返してはなりません。導出結果は同一入力で決定的です（RQ-051、RQ-052）。
/// </remarks>
public interface IFallbackKeyDerivation
{
    /// <summary>
    /// fallback key token を導出します。
    /// </summary>
    /// <param name="scope">非機密の素材スコープ。</param>
    /// <param name="rawText">対象アプリから取得した raw text。</param>
    /// <returns>ドメインへ渡せる fallback hash token 素材。</returns>
    IdentityMaterial DeriveFallbackToken(string scope, string rawText);
}
