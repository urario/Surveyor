namespace Surveyor.Application.Dto;

/// <summary>
/// 対象へのアクセス整合性の見込みを表します。
/// </summary>
public enum TargetIntegrityHint
{
    /// <summary>
    /// 同等または低い整合性として扱える対象です。
    /// </summary>
    SameOrLower,

    /// <summary>
    /// 高い整合性により昇格が必要な可能性がある対象です。
    /// </summary>
    HigherRequiresElevation,

    /// <summary>
    /// 整合性が不明な対象です。
    /// </summary>
    Unknown,
}
