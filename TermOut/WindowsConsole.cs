using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;
using System.Text;
using Windows.Win32.System.Console;
using static Windows.Win32.PInvoke;

namespace TermOut;

[SupportedOSPlatform("windows5.1.2600")]
public class WindowsConsole : IDisposable
{
    private const string HideCursorEscape = "\x1b[?25l";
    private const string ShowCursorEscape = "\x1b[?25h";
    private const string DisableWrapEscape = "\x1b[?7l";
    private const string EnableWrapEscape = "\x1b[?7h";
    private const string HomeCursorEscape = "\x1b[H";
    private const string ResetColorEscape = "\x1b[0m";

    private const string SaveCursorPositionEscape = "\x1b[s";
    private const string RestoreCursorPositionEscape = "\x1b[u";


    private readonly StreamWriter _writer;

    public WindowsConsole()
    {
        Console.OutputEncoding = Encoding.UTF8;
        EnableTrueColorSupport();
        Stream baseStream = Console.OpenStandardOutput(128 * 1024);
        Encoding noBomEncoding = new UTF8Encoding(false);
        _writer = new StreamWriter(baseStream, noBomEncoding, 128 * 1024);
    }

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
        SetBackColor(backColor);
        SetForeColor(foreColor);
        foreach(var cell in buffer.AsSpan())
        {
            if(cell.Back != backColor)
            {
                backColor = cell.Back;
                SetBackColor(backColor);
            }
            if (cell.Fore != foreColor) 
            {
                foreColor = cell.Fore;
                SetForeColor(foreColor);
            }
            _writer.Write(cell.Ch);
        }
        ResetColor();
        _writer.Flush();
        return true;
    }

    public void SetForeColor(Color color)
    {
        _writer.Write($"\x1b[38;2;{color.R};{color.G};{color.B}m");
    }

    public void SetBackColor(Color color)
    {
        _writer.Write($"\x1b[48;2;{color.R};{color.G};{color.B}m");
    }

    public void SaveCursorPosition()
    {
        _writer.Write(SaveCursorPositionEscape);
    }

    public void RestoreCursorPosition()
    {
        _writer.Write(RestoreCursorPositionEscape);
    }

    public void HideCursor()
    {
        _writer.Write(HideCursorEscape);
    }

    public void ShowCursor()
    {
        _writer.Write(ShowCursorEscape);
    }

    public void DisableWrap()
    {
        _writer.Write(DisableWrapEscape);
    }

    public void EnableWrap()
    {
        _writer.Write(EnableWrapEscape);
    }

    public void HomeCursor()
    {
        _writer.Write(HomeCursorEscape);
    }

    public void ResetColor()
    {
        _writer.Write(ResetColorEscape);
    }

    public void SetCursorPosition(int x, int y)
    {
        _writer.Write("\x1b[{y};{x}H");
    }

    public void MoveCursorUp(int rows)
    {
        _writer.Write("\x1b[{n}A");
    }

    public void MoveCursorDown(int rows)
    {
        _writer.Write("\x1b[{n}B");
    }
    public void MoveCursorRight(int cols)
    {
        _writer.Write("\x1b[{cols}C");
    }
    public void MoveCursorLeft(int cols)
    {
        _writer.Write("\x1b[{cols}D");
    }

    public void Flush()
    {
        _writer.Flush();
    }


    [SupportedOSPlatform("windows5.1.2600")]
    private static void EnableTrueColorSupport()
    {
        // Получаем стандартный дескриптор вывода консоли
        SafeFileHandle hOut = GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        if (hOut.IsInvalid) return;
        if (GetConsoleMode(hOut, out CONSOLE_MODE mode))
        {
            // Включаем флаг ENABLE_VIRTUAL_TERMINAL_PROCESSING для обработки ANSI/TrueColor последовательностей
            mode |= CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            _ = SetConsoleMode(hOut, mode);
        }
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}
