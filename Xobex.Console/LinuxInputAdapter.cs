// <copyright file="LinuxInputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.InteropServices;
using static Xobex.Console.LinuxNative;

namespace Xobex.Console;

/// <summary>
/// LinuxInputAdapter class
/// </summary>
public unsafe class LinuxInputAdapter : IDisposable
{
    private termios _original;
    private bool _rawMode;

    private LinuxInputAdapter(LinuxOutputAdapter conOut)
    {
        Out = conOut;
    }

    /// <summary>
    /// Creates new instance of a <see cref="LinuxInputAdapter"/>
    /// </summary>
    /// <param name="conOut"></param>
    /// <returns></returns>
    public static LinuxInputAdapter Create(LinuxOutputAdapter conOut)
    {
        var conIn = new LinuxInputAdapter(conOut);
        conIn.EnableRawMode();
        try
        {
            conOut.EnableMouseInput();
            return conIn;
        }
        catch
        {
            conIn.RestoreCanonicalMode();
            throw;
        }
    }

    /// <summary>
    /// Gets Out adapters
    /// </summary>
    public LinuxOutputAdapter Out { get; }

    /// <summary>
    /// Determines whether user input is available
    /// </summary>
    /// <returns>true is user input available</returns>
    public bool HasInput()
    {
        return HasInput(0);
    }

    /// <summary>
    /// Determines whether user input is available
    /// </summary>
    /// <param name="timeoutMs">timeout in miliseconds</param>
    /// <returns>true is user input available</returns>
    public bool HasInput(int timeoutMs)
    {
        int ret;
        do
        {
            pollfd pfd = new() { fd = STDIN_FILENO, events = POLLIN };
            ret = poll(&pfd, 1, timeoutMs);
            if (ret > 0)
            {
                if ((pfd.revents & (POLLHUP | POLLERR | POLLNVAL)) != 0)
                {
                    // Peer closed / descriptor error: surface this as "input
                    // available" so the caller proceeds to Read(), which is
                    // the call that can actually report EOF or the error,
                    // rather than looping on HasInput() forever.
                    return true;
                }
                if ((pfd.revents & POLLIN) != 0)
                {
                    return true;
                }
            }
        } while (ret == -1 && Marshal.GetLastPInvokeError() == EINTR);
        ThrowIfError(ret, nameof(poll));
        return false;
    }

    /// <summary>
    /// Reads console/terminal user input
    /// </summary>
    /// <param name="buffer">Buffer</param>
    /// <returns>number of bytes read</returns>
    public int Read(Span<byte> buffer)
    {
        if (buffer.Length == 0)
        {
            return 0;
        }

        fixed (byte* ptr = buffer)
        {
            nint bytesRead;
            do
            {
                bytesRead = read(STDIN_FILENO, ptr, buffer.Length);
            } while (bytesRead == -1 && Marshal.GetLastPInvokeError() == EINTR);

            ThrowIfError((int)bytesRead, nameof(read));

            return (int)bytesRead;
        }
    }

    /// <summary>
    /// Resets state of the terminal
    /// </summary>
    public void Dispose()
    {
        Reset();
    }

    /// <summary>
    /// Resets state of the terminal
    /// </summary>
    public void Reset()
    {
        try
        {
            Out.DisableMouseInput();
        }
        finally
        {
            RestoreCanonicalMode();
        }
    }

    private void RestoreCanonicalMode()
    {
        if (_rawMode)
        {
            fixed (termios* ptr = &_original)
            {
                ThrowIfError(tcsetattr(STDIN_FILENO, TCSAFLUSH, ptr), nameof(tcsetattr));
            }
            _rawMode = false;
        }
    }

    private void EnableRawMode()
    {
        fixed (termios* ptr = &_original)
        {
            ThrowIfError(tcgetattr(STDIN_FILENO, ptr), nameof(tcgetattr));
            var raw = _original;
            cfmakeraw(&raw);
            ThrowIfError(tcsetattr(STDIN_FILENO, TCSAFLUSH, &raw), nameof(tcsetattr));
            _rawMode = true;
        }
    }

    private static void ThrowIfError(int result, string name)
    {
        if (result < 0)
        {
            var errno = Marshal.GetLastPInvokeError();
            throw new InvalidOperationException($"{name} failed {errno} - {Marshal.GetLastPInvokeErrorMessage()}");
        }
    }
}
