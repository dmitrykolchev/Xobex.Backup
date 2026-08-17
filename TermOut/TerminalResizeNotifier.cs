namespace TermOut;

public sealed class TerminalResizeNotifier : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _monitorTask;
    private int _lastWidth;
    private int _lastHeight;

    private static readonly TimeSpan DefaultDelay = TimeSpan.FromMilliseconds(16);
    public event EventHandler? Resized;

    public TerminalResizeNotifier()
    {
        _lastWidth = Console.WindowWidth;
        _lastHeight = Console.WindowHeight;

        _monitorTask = Task.Run(MonitorResizeAsync);
    }

    private async Task MonitorResizeAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            int currentWidth = Console.WindowWidth;
            int currentHeight = Console.WindowHeight;

            if (currentWidth != _lastWidth || currentHeight != _lastHeight)
            {
                _lastWidth = currentWidth;
                _lastHeight = currentHeight;
                Resized?.Invoke(this, EventArgs.Empty);
            }

            await Task.Delay(DefaultDelay, _cts.Token);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _monitorTask.Wait(); } catch (AggregateException) { }
        _cts.Dispose();
    }
}