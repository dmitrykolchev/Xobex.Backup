using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

namespace TermOut;

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

    public void ResetColors()
    {
        Foreground = DefaultForeground;
        Background = DefaultBackground;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int x, int y, char ch)
    {
        Write(x, y, new ConsoleCell(ch, Foreground, Background));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int x, int y, in ConsoleCell cell)
    {
        int offset = y * Width + x;
        if (offset >= _buffer.Length)
        {
            return;
        }
        _buffer[offset] = cell;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int x, int y, string text)
    {
        int offset = y * Width + x;
        if(offset >= _buffer.Length)
        {
            return;
        }
        for (int i = 0; i < Width - x && i < text.Length; ++i)
        {
            _buffer[offset + i] = new ConsoleCell(text[i], Foreground, Background);
        }
    }

    public void Fill(int x, int y, char ch, int count)
    {
        int offset = y * Width + x;
        if (offset >= _buffer.Length)
        {
            return;
        }
        var cell = new ConsoleCell(ch, Foreground, Background);
        for (int i = 0; i < Width - x && i < count; ++i)
        {
            _buffer[offset + i] = cell;
        }
    }

    public void Clear()
    {
        var cell = new ConsoleCell(' ', Foreground, Background);
        for (int i = 0; i < Width * Height; ++i)
        {
            _buffer[i] = cell;
        }
    }

    public ref ConsoleCell this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref _buffer[y * Width + x];
        }
    }
}
