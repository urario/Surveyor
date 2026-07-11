namespace Surveyor.TestSupport;

/// <summary>
/// フィクスチャ読み込み用にリポジトリルートを解決します。
/// </summary>
public static class FixtureRepositoryPaths
{
    /// <summary>
    /// <c>Surveyor.slnx</c> を目印にリポジトリルートを解決します。
    /// </summary>
    /// <returns>リポジトリルートの絶対パスです。</returns>
    public static string RepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Surveyor.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
