// <copyright file="LinuxOutputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.CompilerServices;
using System.Text;
using Xobex.Console.Abstractions;

namespace Xobex.Console.Linux;

public class LinuxOutputAdapter: ITerminalOutputAdapter
{
    public LinuxOutputAdapter(TextWriter writer)
    {
        Writer = writer;
    }

    public static LinuxOutputAdapter Create(int bufferSize = 128 * 1024)
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        var baseStream = System.Console.OpenStandardOutput(bufferSize);
        Encoding noBomEncoding = new UTF8Encoding(false);
        var writer = new StreamWriter(baseStream, noBomEncoding, bufferSize);
        return new LinuxOutputAdapter(writer);
    }

    private TextWriter Writer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
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
