// <copyright file="WindowsOutputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text;
using Windows.Win32.System.Console;
using static Windows.Win32.PInvoke;

namespace Xobex.Console.Windows;

public class WindowsOutputAdapter : TerminalOutputAdapter
{
    private readonly CONSOLE_MODE _prevMode;

    public WindowsOutputAdapter(TextWriter writer) : base(writer)
    {
        using var hOut = GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        if (GetConsoleMode(hOut, out var mode))
        {
            _prevMode = mode;
            mode |= CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            mode |= CONSOLE_MODE.DISABLE_NEWLINE_AUTO_RETURN;
            mode &= ~CONSOLE_MODE.ENABLE_WRAP_AT_EOL_OUTPUT;
            SetConsoleMode(hOut, mode);
        }
    }

    public static WindowsOutputAdapter Create(int bufferSize = 128 * 1024)
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        var baseStream = System.Console.OpenStandardOutput(bufferSize);
        Encoding noBomEncoding = new UTF8Encoding(false);
        var writer = new StreamWriter(baseStream, noBomEncoding, bufferSize);
        return new WindowsOutputAdapter(writer);
    }

    protected override void Reset()
    {
        using var hOut = GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        SetConsoleMode(hOut, _prevMode);
    }
}
