namespace Surveyor.Domain.Model;

/// <summary>
/// 画面の同一性素材を表します。
/// </summary>
internal readonly struct ScreenIdentity : IEquatable<ScreenIdentity>
{
    /// <summary>
    /// 画面同一性を初期化します。
    /// </summary>
    /// <param name="processImageName">プロセスのファイル名。</param>
    /// <param name="normalizedWindowClass">正規化済みウィンドウクラス名。</param>
    /// <param name="role">画面の役割。</param>
    /// <param name="source">同一性素材の出所。</param>
    /// <param name="material">同一性素材。</param>
    public ScreenIdentity(
        string processImageName,
        string normalizedWindowClass,
        ScreenRole role,
        IdentitySource source,
        IdentityMaterial material)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processImageName);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedWindowClass);
        if (processImageName.Contains('\\', StringComparison.Ordinal) || processImageName.Contains('/', StringComparison.Ordinal))
        {
            throw new ArgumentException(null, nameof(processImageName));
        }

        ProcessImageName = processImageName;
        NormalizedWindowClass = normalizedWindowClass;
        Role = role;
        Source = source;
        Material = material;
    }

    /// <summary>
    /// プロセスのファイル名を取得します。
    /// </summary>
    public string ProcessImageName { get; }

    /// <summary>
    /// 正規化済みウィンドウクラス名を取得します。
    /// </summary>
    public string NormalizedWindowClass { get; }

    /// <summary>
    /// 画面の役割を取得します。
    /// </summary>
    public ScreenRole Role { get; }

    /// <summary>
    /// 同一性素材の出所を取得します。
    /// </summary>
    public IdentitySource Source { get; }

    /// <summary>
    /// 同一性素材を取得します。
    /// </summary>
    public IdentityMaterial Material { get; }

    /// <summary>
    /// ほかの画面同一性と等しいかを判定します。
    /// </summary>
    /// <param name="other">比較対象。</param>
    /// <returns>同じ画面同一性なら <see langword="true"/>。</returns>
    public bool Equals(ScreenIdentity other)
    {
        return string.Equals(ProcessImageName, other.ProcessImageName, StringComparison.Ordinal)
            && string.Equals(NormalizedWindowClass, other.NormalizedWindowClass, StringComparison.Ordinal)
            && Role == other.Role
            && Source == other.Source
            && Material.Equals(other.Material);
    }

    /// <summary>
    /// ほかのオブジェクトと等しいかを判定します。
    /// </summary>
    /// <param name="obj">比較対象。</param>
    /// <returns>同じ画面同一性なら <see langword="true"/>。</returns>
    public override bool Equals(object? obj)
    {
        return obj is ScreenIdentity other && Equals(other);
    }

    /// <summary>
    /// ハッシュコードを返します。
    /// </summary>
    /// <returns>この値オブジェクトのハッシュコード。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(ProcessImageName, NormalizedWindowClass, Role, Source, Material);
    }
}
