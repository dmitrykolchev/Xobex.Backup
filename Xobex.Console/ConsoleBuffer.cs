// <copyright file="ConsoleBuffer.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.CompilerServices;

namespace Xobex.Console;

public class ConsoleBuffer
{
    private readonly ConsoleCell[] _buffer;

    public ConsoleBuffer(int width, int height, Color defaultForeground, Color defaultBackground)
    {
        Width = width;
        Height = height;
        Foreground = DefaultForeground = defaultForeground;
        Background = DefaultBackground = defaultBackground;

        _buffer = new ConsoleCell[width * height];
    }

    public ReadOnlySpan<ConsoleCell> AsSpan()
    {
        return _buffer;
    }

    public int Width { get; }

    public int Height { get; }

    public Color DefaultForeground { get; } = Colors.White;

    public Color DefaultBackground { get; } = Colors.Black;

    public TextStyle Style
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        set
        {
            if (field != value)
            {
                if (value == TextStyle.None)
                {
                    Background = DefaultBackground;
                    Foreground = DefaultForeground;
                }
                field = value;
            }
        }

    }

    public Color Foreground
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        set;
    }

    public Color Background
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        set;
    }

    public void Reset()
    {
        Foreground = DefaultForeground;
        Background = DefaultBackground;
        Style = TextStyle.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int x, int y, char ch)
    {
        Write(x, y, new ConsoleCell(ch, Foreground, Background, Style));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int x, int y, in ConsoleCell cell)
    {
        var offset = (y * Width) + x;
        if (offset >= _buffer.Length)
        {
            return;
        }
        _buffer[offset] = cell;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int x, int y, string text)
    {
        var offset = (y * Width) + x;
        if (offset >= _buffer.Length)
        {
            return;
        }
        for (var i = 0; i < Width - x && i < text.Length; ++i)
        {
            _buffer[offset + i] = new ConsoleCell(text[i], Foreground, Background, Style);
        }
    }

    public void Fill(int x, int y, char ch, int count)
    {
        var offset = (y * Width) + x;
        if (offset >= _buffer.Length)
        {
            return;
        }
        ConsoleCell cell = new(ch, Foreground, Background, Style);
        for (var i = 0; i < Width - x && i < count; ++i)
        {
            _buffer[offset + i] = cell;
        }
    }

    public void Clear()
    {
        ConsoleCell cell = new(' ', Foreground, Background, Style);
        for (var i = 0; i < Width * Height; ++i)
        {
            _buffer[i] = cell;
        }
    }

    public ref ConsoleCell this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _buffer[(y * Width) + x];
    }
}
