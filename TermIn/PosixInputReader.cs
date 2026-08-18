using System;
using System.Runtime.InteropServices;

namespace TermIn;

public unsafe class PosixInputReader : IDisposable
{
    private const int STDIN_FILENO = 0;
    private const short POLLIN = 0x0001;

    [StructLayout(LayoutKind.Sequential)]
    private struct pollfd
    {
        public int fd;
        public short events;
        public short revents;
    }

    [DllImport("libc")]
    private static extern int poll(pollfd* fds, ulong nfds, int timeout);

    [DllImport("libc")]
    private static extern nint read(int fd, byte* buf, nuint count);

    private readonly AnsiInputParser _parser = new();
    private readonly byte[] _readBuffer = new byte[256];

    public void ReadEvents(Action<InputRecord> onEvent, int escapeTimeoutMs = 25)
    {
        fixed (byte* bufPtr = _readBuffer)
        {
            var pfd = new pollfd { fd = STDIN_FILENO, events = POLLIN, revents = 0 };

            // Проверяем наличие данных
            var ret = poll(&pfd, 1, escapeTimeoutMs);
            if (ret > 0 && (pfd.revents & POLLIN) != 0)
            {
                nint bytesRead = read(STDIN_FILENO, bufPtr, (nuint)_readBuffer.Length);
                if (bytesRead > 0)
                {
                    _parser.Parse(new ReadOnlySpan<byte>(_readBuffer, 0, (int)bytesRead), onEvent);
                }
            }
            else if (ret == 0)
            {
                // Сработал таймаут: если автомат находился в ожидании префикса ESC, сбрасываем его как клавишу ESC
                _parser.FlushEscape(onEvent);
            }
        }
    }

    public void Dispose() { }
}