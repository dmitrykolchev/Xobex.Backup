using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;
using Windows.Win32.System.Console;
using static Windows.Win32.PInvoke;

namespace TermOut;

public enum TextStyle : byte
{
    None = 0,
    Bold = 1,
    Dimmed = 2,
    Italic = 3,
    Underline = 4,
    Blink = 5,
    Invers = 6,
    Hidden = 7
}

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
public class WindowsConsole : ConsoleAdapter
{
    public WindowsConsole() : base()
    {
        EnableTrueColorSupport();
    }

    [SupportedOSPlatform("windows5.1.2600")]
    private static void EnableTrueColorSupport()
    {
        // Получаем стандартный дескриптор вывода консоли
        SafeFileHandle hOut = GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        if (hOut.IsInvalid) return;
        if (GetConsoleMode(hOut, out CONSOLE_MODE mode))
        {
            mode |= CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            mode |= CONSOLE_MODE.DISABLE_NEWLINE_AUTO_RETURN;
            //mode &= ~CONSOLE_MODE.ENABLE_WRAP_AT_EOL_OUTPUT;

            _ = SetConsoleMode(hOut, mode);
        }
        SafeFileHandle hIn = GetStdHandle_SafeHandle(STD_HANDLE.STD_INPUT_HANDLE);
        if (GetConsoleMode(hIn, out mode))
        {
            mode |= CONSOLE_MODE.ENABLE_WINDOW_INPUT;           // Report changes in buffer size
            mode |= CONSOLE_MODE.ENABLE_MOUSE_INPUT;            // Report mouse events.
            mode &= ~CONSOLE_MODE.ENABLE_PROCESSED_INPUT;       // Report CTRL+C and SHIFT+Arrow events.
            mode &= ~(CONSOLE_MODE.ENABLE_ECHO_INPUT | CONSOLE_MODE.ENABLE_LINE_INPUT); // Report Ctrl+S.
            mode |= CONSOLE_MODE.ENABLE_EXTENDED_FLAGS;         // Disable the Quick Edit mode, 
            mode &= ~CONSOLE_MODE.ENABLE_QUICK_EDIT_MODE;       // which inhibits the mouse.   
            _ = SetConsoleMode(hIn, mode);
        }
    }
}
