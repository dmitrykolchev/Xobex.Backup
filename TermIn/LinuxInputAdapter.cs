// <copyright file="LinuxInputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using static TermIn.LinuxNative;

namespace TermIn;

public unsafe class LinuxInputAdapter : IDisposable
{
    private termios _original;
    private bool _rawMode;
    private readonly LinuxConsoleAdapter _con;

    public LinuxInputAdapter(LinuxConsoleAdapter con)
    {
        _con = con;
        EnableRawMode();
        _con.Write("\x1b[?1000h\x1b[?1003h\x1b[?1006h");
        _con.Flush();
    }

    public void Dispose()
    {
        Reset();
    }

    public void Reset()
    {
        // Disable mouse tracking sequences
        _con.Write("\x1b[?1000l\x1b[?1003l\x1b[?1006l");
        _con.Flush();

        // Restore original terminal flags
        if (_rawMode)
        {
            fixed (termios* ptr = &_original)
            {
                _ = tcsetattr(STDIN_FILENO, TCSANOW, ptr);
            }
            _rawMode = false;
        }
    }

    private void EnableRawMode()
    {
        fixed (termios* ptr = &_original)
        {
            if (tcgetattr(STDIN_FILENO, ptr) == 0)
            {
                var raw = _original;
                cfmakeraw(&raw);
                var err = tcsetattr(STDIN_FILENO, TCSANOW, &raw);
                if (err != 0)
                {
                    throw new InvalidOperationException($"tcsetattr error: {err}");
                }
                _rawMode = true;
            }
        }
    }

    public bool HasInput()
    {
        pollfd pfd = new() { fd = STDIN_FILENO, events = POLLIN };
        var ret = poll(&pfd, 1, 0);
        if (ret == 1 && (pfd.revents & POLLIN) != 0)
        {
            return true;
        }
        else if (ret < 0)
        {
            throw new InvalidOperationException("poll failed");
        }
        return false;
    }

    public int Read(Span<byte> buffer)
    {
        fixed (byte* ptr = buffer)
        {
            var bytesRead = read(STDIN_FILENO, ptr, buffer.Length);
            return unchecked((int)bytesRead);
        }
    }
}
