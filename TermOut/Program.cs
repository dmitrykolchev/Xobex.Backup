using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;
using Windows.Win32.System.Console;
using static Windows.Win32.PInvoke;

namespace TermOut;

[SupportedOSPlatform("windows5.1.2600")]
internal unsafe class Program
{
    private static ConsoleAdapter _console = null!;
    private static ConsoleBuffer _buffer = null!;

    private static void Main(string[] args)
    {
        if(OperatingSystem.IsWindows())
        {
            _console = new WindowsConsole();
        }
        else
        {
            _console = new LinuxConsole();
        }
        using TerminalResizeNotifier resizeNotifier = new();
        resizeNotifier.Resized += ResizeNotifier_Resized;
        _console.AlternateScreen(true);
        _console.Flush();
        _buffer = new(_console.Width, _console.Height, Colors.Green, Colors.Black);
        FullRender();

        if (OperatingSystem.IsWindows())
        {
            var hIn = GetStdHandle_SafeHandle(STD_HANDLE.STD_INPUT_HANDLE);
            Span<INPUT_RECORD> input = stackalloc INPUT_RECORD[1];
            for (; ; )
            {
                var result = ReadConsoleInput(hIn, input, out var inputRead);
                if (input[0].EventType == MOUSE_EVENT)
                {
                    var ev = input[0].Event.MouseEvent;
                    _buffer.Background = new Color(128, 128, 128);
                    _buffer.Foreground = new Color(128, 0, 0);
                    _buffer.Fill(0, 0, ' ', _console.Width);
                    _buffer.Write(1, 0, $"Mouse Event - flags: {ev.dwEventFlags:X8}, position: ({ev.dwMousePosition.X}, {ev.dwMousePosition.Y}), buttons: {ev.dwButtonState:X8}, keys: {ev.dwControlKeyState:X8}");
                    _console.Render(_buffer);
                    _console.Flush();

                }
                else if (input[0].EventType == KEY_EVENT)
                {
                    var ev = input[0].Event.KeyEvent;
                    _buffer.Background = new Color(128, 128, 128);
                    _buffer.Foreground = new Color(128, 0, 0);
                    _buffer.Fill(0, 1, ' ', _console.Width);
                    string ch;
                    if (ev.uChar.UnicodeChar <= '\u0020')
                    {
                        ch = $"\\u{(ushort)ev.uChar.UnicodeChar:X4}";
                    }
                    else
                    {
                        ch = ev.uChar.UnicodeChar.ToString();
                    }
                    _buffer.Write(1, 1, $"Key Event - char: {ch}, count: {ev.wRepeatCount}, key state: {ev.dwControlKeyState:X8}, keycode: {ev.wVirtualKeyCode:X4}, scancode: {ev.wVirtualScanCode:X4}");
                    if (ev.bKeyDown && ev.wVirtualKeyCode == 0x43 && (ev.dwControlKeyState & 8) != 0)
                    {
                        break;
                    }
                    _console.Render(_buffer);
                    _console.Flush();
                }
            }
        }
        else
        {
            for (; ; )
            {
                if(((LinuxConsole) _console).GetInputEvent(out var ev))
                {
                    if(ev is KeyboardEvent k && k.Ch == 'q')
                    {
                        break;
                    }
                    else if(ev is MouseEvent m)
                    {
                        _buffer.Background = new Color(128, 128, 128);
                        _buffer.Foreground = new Color(128, 0, 0);
                        _buffer.Fill(0, 1, ' ', _console.Width);
                        _buffer.Write(1, 1, $"Mouse Event - {m.EventType}, {m.Release}, {m.X}, {m.Y}");
                    }
                }
            }
        }
        _console.AlternateScreen(false);
        _console.Dispose();
    }

    private static void ResizeNotifier_Resized(object? sender, EventArgs e)
    {
        _buffer = new(_console.Width, _console.Height, Colors.Green, Colors.Black);
        FullRender();
    }

    private static void FullRender()
    {
        _buffer.Clear();
        _buffer.Write(10, 10, "Hello World!");
        _buffer.Style = TextStyle.Bold;
        _buffer.Write(10, 11, "Bold text");
        _buffer.Style = TextStyle.Dimmed;
        _buffer.Write(10, 12, "Dimmed text");
        _buffer.Style = TextStyle.Italic;
        _buffer.Write(10, 13, "Italic text");
        _buffer.Style = TextStyle.Underline;
        _buffer.Write(10, 14, "Underline text");
        _buffer.Style = TextStyle.Blink;
        _buffer.Write(10, 15, "Blink text");
        _buffer.Style = TextStyle.Invers;
        _buffer.Write(10, 16, "Invers text");
        _buffer.Style = TextStyle.Hidden;
        _buffer.Write(10, 17, "Hidden text");
        _buffer.Style = TextStyle.None;

        _buffer.Background = new Color(128, 128, 128);
        _buffer.Foreground = new Color(0, 0, 128);
        _buffer.Fill(0, _console.Height - 1, ' ', _console.Width);
        _buffer.Write(1, _console.Height - 1, $"Новый размер: {Console.WindowHeight} строк, {Console.WindowWidth} столбцов");
        _console.Render(_buffer);
        _console.Flush();
    }
}
