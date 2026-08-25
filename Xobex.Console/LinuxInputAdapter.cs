// <copyright file="LinuxInputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.InteropServices;
using static Xobex.Console.LinuxNative;

namespace Xobex.Console;

public unsafe class LinuxInputAdapter : IDisposable
{
    private termios _original;
    private bool _rawMode;

    public LinuxInputAdapter(LinuxOutputAdapter conOut)
    {
        Out = conOut;
        EnableRawMode();
        Out.EnableMouseInput();
    }

    public LinuxOutputAdapter Out { get; }

    public void Dispose()
    {
        Reset();
    }

    public void Reset()
    {
        Out.DisableMouseInput();
        // Restore original terminal flags
        if (_rawMode)
        {
            fixed (termios* ptr = &_original)
            {
                // ignore error when Dispose()
                tcsetattr(STDIN_FILENO, TCSANOW, ptr);
            }
            _rawMode = false;
        }
    }

    private static void ThrowIfError(int result)
    {
        if (result < 0)
        {
            var errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException($"read failed {errno} - {Marshal.GetLastPInvokeErrorMessage()}");
        }
    }

    private void EnableRawMode()
    {
        fixed (termios* ptr = &_original)
        {
            ThrowIfError(tcgetattr(STDIN_FILENO, ptr));
            var raw = _original;
            cfmakeraw(&raw);
            ThrowIfError(tcsetattr(STDIN_FILENO, TCSANOW, &raw));
            _rawMode = true;
        }
    }

    public bool HasInput()
    {
        for (var retry = 0; retry < 5; ++retry)
        {
            pollfd pfd = new() { fd = STDIN_FILENO, events = POLLIN };
            var ret = poll(&pfd, 1, 0);
            if (ret == 1 && (pfd.revents & POLLIN) != 0)
            {
                return true;
            }

            if (ret < 0)
            {
                var errno = Marshal.GetLastPInvokeError();
                if (errno == EINTR)
                {
                    continue;
                }
                else
                {
                    throw new InvalidOperationException($"poll failed {errno} - {Marshal.GetLastPInvokeErrorMessage()}");
                }
            }
            break;
        }
        return false;
    }

    public int Read(Span<byte> buffer)
    {
        for (var retry = 0; retry < 5; ++retry)
        {
            fixed (byte* ptr = buffer)
            {
                var bytesRead = read(STDIN_FILENO, ptr, buffer.Length);
                if (bytesRead < 0)
                {
                    var errno = Marshal.GetLastPInvokeError();
                    if (errno == EINTR)
                    {
                        continue;
                    }
                    else
                    {
                        throw new InvalidOperationException($"read failed {errno} - {Marshal.GetLastPInvokeErrorMessage()}");
                    }
                }
                return unchecked((int)bytesRead);
            }
        }
        return 0;
    }
}
