// <copyright file="TerminalResizeNotifier.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Console;

public sealed class TerminalResizeNotifier : IDisposable
{
    private static readonly TimeSpan DefaultDelay = TimeSpan.FromMilliseconds(16);

    private readonly CancellationTokenSource _cts = new();
    private readonly Task _monitorTask;
    private int _lastWidth;
    private int _lastHeight;

    public event EventHandler? Resized;

    public TerminalResizeNotifier()
    {
        _lastWidth = System.Console.WindowWidth;
        _lastHeight = System.Console.WindowHeight;

        _monitorTask = Task.Run(MonitorResizeAsync);
    }

    private async Task MonitorResizeAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            var currentWidth = System.Console.WindowWidth;
            var currentHeight = System.Console.WindowHeight;

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
