// <copyright file="WindowsTerminalParser.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.InteropServices;
using Windows.Win32.System.Console;
using Xobex.Console.Abstractions;
using static Windows.Win32.PInvoke;

namespace Xobex.Console.Windows;

internal class WindowsTerminalParser : ITerminalParser
{
    private MouseButton _mouseButtonState;

    public WindowsTerminalParser(WindowsInputAdapter conIn)
    {
        In = conIn;
    }

    private WindowsInputAdapter In { get; }

    public bool TryGetInputEvent(out InputEvent? ev)
    {
        Span<INPUT_RECORD> inputRecors = stackalloc INPUT_RECORD[1];
        if (ReadConsoleInput(In.Handle, inputRecors, out var read) && read == 1)
        {
            var ir = inputRecors[0];
            if (ir.EventType == MOUSE_EVENT)
            {
                return CreateMouseEvent(ir.Event.MouseEvent, out ev);
            }
            else if (ir.EventType == KEY_EVENT)
            {
                return CreateKeyEvent(ir.Event.KeyEvent, out ev);
            }
        }
        ev = default;
        return false;
    }

    private static bool CreateKeyEvent(KEY_EVENT_RECORD ce, out InputEvent ev)
    {
        var mod = ConsoleModifiers.None;
        if ((ce.dwControlKeyState & (LEFT_ALT_PRESSED | RIGHT_ALT_PRESSED)) != 0)
        {
            mod |= ConsoleModifiers.Alt;
        }
        if ((ce.dwControlKeyState & (LEFT_CTRL_PRESSED | RIGHT_CTRL_PRESSED)) != 0)
        {
            mod |= ConsoleModifiers.Control;
        }
        if ((ce.dwControlKeyState & SHIFT_PRESSED) != 0)
        {
            mod |= ConsoleModifiers.Shift;
        }
        var rawData = MemoryMarshal.CreateSpan<KEY_EVENT_RECORD>(ref ce, 1);
        var keyEvent = new InputEvent.KeyEvent
        {
            KeyDown = ce.bKeyDown,
            Key = (ConsoleKey)ce.wVirtualKeyCode,
            Mod = mod,
            RepeatCount = ce.wRepeatCount,
            Ch = ce.uChar.UnicodeChar
        };
        ev = InputEvent.Create(keyEvent, MemoryMarshal.Cast<KEY_EVENT_RECORD, byte>(rawData));
        return true;
    }

    private bool CreateMouseEvent(MOUSE_EVENT_RECORD ce, out InputEvent? ev)
    {
        var mod = ConsoleModifiers.None;
        if ((ce.dwControlKeyState & (LEFT_ALT_PRESSED | RIGHT_ALT_PRESSED)) != 0)
        {
            mod |= ConsoleModifiers.Alt;
        }
        if ((ce.dwControlKeyState & (LEFT_CTRL_PRESSED | RIGHT_CTRL_PRESSED)) != 0)
        {
            mod |= ConsoleModifiers.Control;
        }
        if ((ce.dwControlKeyState & SHIFT_PRESSED) != 0)
        {
            mod |= ConsoleModifiers.Shift;
        }

        var currentButtonState = MouseButton.None;
        if ((ce.dwButtonState & FROM_LEFT_1ST_BUTTON_PRESSED) != 0)
        {
            currentButtonState |= MouseButton.Left;
        }
        if ((ce.dwButtonState & RIGHTMOST_BUTTON_PRESSED) != 0)
        {
            currentButtonState |= MouseButton.Right;
        }
        if ((ce.dwButtonState & (FROM_LEFT_2ND_BUTTON_PRESSED | FROM_LEFT_3RD_BUTTON_PRESSED | FROM_LEFT_4TH_BUTTON_PRESSED)) != 0)
        {
            currentButtonState |= MouseButton.Middle;
        }

        var action = MouseAction.None;
        var buttonDelta = _mouseButtonState ^ currentButtonState;
        var button = MouseButton.None;

        //System.Console.WriteLine($"Current button state = {currentButtonState}");
        //System.Console.WriteLine($"Previous button state = {_mouseButtonState}");
        //System.Console.WriteLine($"Delta button state = {buttonDelta}");

        _mouseButtonState = currentButtonState;
        if ((_mouseButtonState & MouseButton.Left) != 0)
        {
            button = MouseButton.Left;
        }
        else if ((_mouseButtonState & MouseButton.Right) != 0)
        {
            button = MouseButton.Right;
        }
        else if ((_mouseButtonState & MouseButton.Middle) != 0)
        {
            button = MouseButton.Middle;
        }

        if (ce.dwEventFlags == 0)
        {
            if ((buttonDelta & MouseButton.Left) != 0)
            {
                action = (currentButtonState & MouseButton.Left) == 0 ? MouseAction.Up : MouseAction.Down;
                button = MouseButton.Left;
            }
            else if ((buttonDelta & MouseButton.Right) != 0)
            {
                action = (currentButtonState & MouseButton.Right) == 0 ? MouseAction.Up : MouseAction.Down;
                button = MouseButton.Right;
            }
            else if ((buttonDelta & MouseButton.Middle) != 0)
            {
                action = (currentButtonState & MouseButton.Middle) == 0 ? MouseAction.Up : MouseAction.Down;
                button = MouseButton.Middle;
            }
        }
        else if ((ce.dwEventFlags & MOUSE_MOVED) != 0)
        {
            action = MouseAction.Move;
        }
        else if ((ce.dwEventFlags & MOUSE_HWHEELED) != 0)
        {
            if (unchecked((short)(ce.dwButtonState >> 16)) > 0)
            {
                action = MouseAction.WheelRight;
            }
            else
            {
                action = MouseAction.WheelLeft;
            }
        }
        else if ((ce.dwEventFlags & MOUSE_WHEELED) != 0)
        {
            if (unchecked((short)(ce.dwButtonState >> 16)) > 0)
            {
                action = MouseAction.WheelUp;
            }
            else
            {
                action = MouseAction.WheelDown;
            }
        }
        else if ((ce.dwEventFlags & DOUBLE_CLICK) != 0)
        {
            action = MouseAction.DblClick;
        }
        var me = new InputEvent.MouseEvent()
        {
            Action = action,
            Button = button,
            X = ce.dwMousePosition.X,
            Y = ce.dwMousePosition.Y,
            Mod = mod
        };
        var rawData = MemoryMarshal.CreateSpan<MOUSE_EVENT_RECORD>(ref ce, 1);
        ev = InputEvent.Create(me, MemoryMarshal.Cast<MOUSE_EVENT_RECORD, byte>(rawData));
        return true;
    }
}
