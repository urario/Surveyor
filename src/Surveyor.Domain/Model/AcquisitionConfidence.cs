namespace Surveyor.Domain.Model;

/// <summary>
/// 取得した UI 情報の信頼度を表します。
/// </summary>
public enum AcquisitionConfidence
{
    /// <summary>
    /// 高い信頼度を表します。
    /// </summary>
    High,

    /// <summary>
    /// 中程度の信頼度を表します。
    /// </summary>
    Medium,

    /// <summary>
    /// 低い信頼度を表します。
    /// </summary>
    Low,
}
