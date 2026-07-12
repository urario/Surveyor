namespace Surveyor.Presentation.Ports;

/// <summary>
/// UI thread への処理移譲を表すポートです。
/// </summary>
/// <remarks>
/// ViewModel のテストでは同一 thread fake で実行し、WinUI dispatcher へ依存しません (RQ-054)。
/// </remarks>
public interface IUiDispatcher
{
    /// <summary>
    /// 指定された処理を UI thread 上で実行します。
    /// </summary>
    /// <param name="action">実行する処理です。</param>
    /// <param name="cancellationToken">処理を中断するトークンです。</param>
    /// <returns>完了を表すタスクです。</returns>
    Task RunOnUiThreadAsync(Action action, CancellationToken cancellationToken);
}
