using System.Runtime.CompilerServices;
using System.Text;

namespace TermOut;

public abstract class ConsoleAdapter : IDisposable
{
    protected const string HideCursorEscape = "\x1b[?25l";
    protected const string ShowCursorEscape = "\x1b[?25h";
    protected const string DisableWrapEscape = "\x1b[?7l";
    protected const string EnableWrapEscape = "\x1b[?7h";
    protected const string HomeCursorEscape = "\x1b[H";
    protected const string ResetColorEscape = "\x1b[0m";
    protected const string BoldTextEscape = "\x1b[1m";
    protected const string DimTextEscape = "\x1b[2m";
    protected const string ItalicTextEscape = "\x1b[3m";
    protected const string UnderlineTextEscape = "\x1b[4m";
    protected const string BlinkTextEscape = "\x1b[5m";
    protected const string InversTextEscape = "\x1b[7m";
    protected const string HiddenTextEscape = "\x1b[8m";
    protected const string AlternateScreenOnEscape = "\x1b[?1049h";
    protected const string AlternateScreenOffEscape = "\x1b[?1049l";
    protected const string SaveCursorPositionEscape = "\x1b[s";
    protected const string RestoreCursorPositionEscape = "\x1b[u";

    private readonly StreamWriter _writer;

    public ConsoleAdapter()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Stream baseStream = Console.OpenStandardOutput(128 * 1024);
        Encoding noBomEncoding = new UTF8Encoding(false);
        _writer = new StreamWriter(baseStream, noBomEncoding, 128 * 1024);
    }

    protected StreamWriter Writer => _writer;

    public int Width => Console.WindowWidth;

    public int Height => Console.WindowHeight;

    public bool Render(ConsoleBuffer buffer)
    {
        HideCursor();
        HomeCursor();
        if (buffer.Width != Width || buffer.Height != Height)
        {
            return false;
        }

        Color backColor = buffer.DefaultBackground;
        Color foreColor = buffer.DefaultForeground;
        TextStyle style = TextStyle.None;
        SetBackColor(backColor);
        SetForeColor(foreColor);
        foreach (ConsoleCell cell in buffer.AsSpan())
        {
            if (cell.Bg != backColor)
            {
                backColor = cell.Bg;
                SetBackColor(backColor);
            }
            if (cell.Fg != foreColor)
            {
                foreColor = cell.Fg;
                SetForeColor(foreColor);
            }
            if (cell.St != style)
            {
                style = cell.St;
                SetTextStyle(style);
                backColor = cell.Bg;
                SetBackColor(backColor);
                foreColor = cell.Fg;
                SetForeColor(foreColor);
            }
            _writer.Write(cell.Ch);
        }
        ResetColor();
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetForeColor(Color color)
    {
        _writer.Write($"\x1b[38;2;{color.R};{color.G};{color.B}m");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBackColor(Color color)
    {
        _writer.Write($"\x1b[48;2;{color.R};{color.G};{color.B}m");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SaveCursorPosition()
    {
        _writer.Write(SaveCursorPositionEscape);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RestoreCursorPosition()
    {
        _writer.Write(RestoreCursorPositionEscape);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void HideCursor()
    {
        _writer.Write(HideCursorEscape);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ShowCursor()
    {
        _writer.Write(ShowCursorEscape);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DisableWrap()
    {
        _writer.Write(DisableWrapEscape);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnableWrap()
    {
        _writer.Write(EnableWrapEscape);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void HomeCursor()
    {
        _writer.Write(HomeCursorEscape);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetColor()
    {
        _writer.Write(ResetColorEscape);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCursorPosition(int x, int y)
    {
        _writer.Write("\x1b[{y};{x}H");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveCursorUp(int rows)
    {
        _writer.Write("\x1b[{n}A");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveCursorDown(int rows)
    {
        _writer.Write("\x1b[{n}B");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveCursorRight(int cols)
    {
        _writer.Write("\x1b[{cols}C");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveCursorLeft(int cols)
    {
        _writer.Write("\x1b[{cols}D");
    }

    public void AlternateScreen(bool on)
    {
        if (on)
        {
            _writer.Write(AlternateScreenOnEscape);
        }
        else
        {
            _writer.Write(AlternateScreenOffEscape);
        }
    }

    public void SetTextStyle(TextStyle style)
    {
        switch (style)
        {
            case TextStyle.None:
                ResetColor();
                break;
            case TextStyle.Bold:
                _writer.Write(BoldTextEscape);
                break;
            case TextStyle.Dimmed:
                _writer.Write(DimTextEscape);
                break;
            case TextStyle.Italic:
                _writer.Write(ItalicTextEscape);
                break;
            case TextStyle.Underline:
                _writer.Write(UnderlineTextEscape);
                break;
            case TextStyle.Invers:
                _writer.Write(InversTextEscape);
                break;
            case TextStyle.Blink:
                _writer.Write(BlinkTextEscape);
                break;
            case TextStyle.Hidden:
                _writer.Write(HiddenTextEscape);
                break;
        }
    }

    public void Write(string text)
    {
        _writer.Write(text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Flush()
    {
        _writer.Flush();
    }

    protected virtual void Reset()
    {
    }

    public void Dispose()
    {
        Reset();
        Flush();
        _writer.Dispose();
    }
}
