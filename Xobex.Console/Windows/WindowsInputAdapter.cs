// <copyright file="WindowsInputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Microsoft.Win32.SafeHandles;
using Windows.Win32.System.Console;
using Xobex.Console.Abstractions;
using static Windows.Win32.PInvoke;

namespace Xobex.Console.Windows;

internal class WindowsInputAdapter : ITerminalInputAdapter
{
    private readonly SafeFileHandle _handle;
    private readonly CONSOLE_MODE _prevMode;

    private WindowsInputAdapter(SafeFileHandle handleIn)
    {
        _handle = handleIn;
        GetConsoleMode(handleIn, out var prevMode);
        _prevMode = prevMode;
        var newMode = prevMode;
        newMode &= ~CONSOLE_MODE.ENABLE_ECHO_INPUT;
        SetConsoleMode(handleIn, newMode);
    }

    public static WindowsInputAdapter Create()
    {
        var handleIn = GetStdHandle_SafeHandle(STD_HANDLE.STD_INPUT_HANDLE);
        return new WindowsInputAdapter(handleIn);
    }

    public SafeFileHandle Handle => _handle;

    public ITerminalParser CreateParser()
    {
        return new WindowsTerminalParser(this);
    }

    public void Dispose()
    {
        try
        {
            Reset();
        }
        finally
        {
            _handle.Dispose();
        }
    }

    public IDisposable EnableMouseInput()
    {
        GetConsoleMode(_handle, out var prevMode);
        var newMode = prevMode | CONSOLE_MODE.ENABLE_MOUSE_INPUT | CONSOLE_MODE.ENABLE_EXTENDED_FLAGS;
        newMode &= ~CONSOLE_MODE.ENABLE_QUICK_EDIT_MODE;
        SetConsoleMode(_handle, newMode);
        return new MouseInputHandler(this);
    }

    public void DisableMouseInput()
    {
        GetConsoleMode(_handle, out var prevMode);
        var newMode = prevMode | CONSOLE_MODE.ENABLE_QUICK_EDIT_MODE | CONSOLE_MODE.ENABLE_EXTENDED_FLAGS;
        newMode &= ~CONSOLE_MODE.ENABLE_MOUSE_INPUT;
        SetConsoleMode(_handle, newMode);
    }

    public void Reset()
    {
        try
        {
            DisableMouseInput();
        }
        finally
        {
            RestoreCanonicalMode();
        }
    }

    private void RestoreCanonicalMode()
    {
        SetConsoleMode(_handle, _prevMode);
    }

    private class MouseInputHandler : IDisposable
    {
        private readonly WindowsInputAdapter _conIn;

        public MouseInputHandler(WindowsInputAdapter conIn)
        {
            _conIn = conIn;
        }

        public void Dispose()
        {
            _conIn.DisableMouseInput();
        }
    }
}
