// <copyright file="LinuxOutputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.CompilerServices;
using System.Text;

namespace Xobex.Console;

public class LinuxOutputAdapter
{
    public LinuxOutputAdapter()
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        var baseStream = System.Console.OpenStandardOutput(128 * 1024);
        Encoding noBomEncoding = new UTF8Encoding(false);
        Writer = new StreamWriter(baseStream, noBomEncoding, 128 * 1024);
    }

    private StreamWriter Writer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnableMouseInput()
    {
        // Enable mouse tracking sequences
        Write("\x1b[?1000h\x1b[?1003h\x1b[?1006h");
        Flush();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DisableMouseInput()
    {
        // Disable mouse tracking sequences
        Write("\x1b[?1000l\x1b[?1003l\x1b[?1006l");
        Flush();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(char ch)
    {
        Writer.Write(ch);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(string text)
    {
        Writer.Write(text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLine()
    {
        Writer.Write("\r\n");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLine(string text)
    {
        Writer.Write(text);
        WriteLine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Flush()
    {
        Writer.Flush();
    }
}
