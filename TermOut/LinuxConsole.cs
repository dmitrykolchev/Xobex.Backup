using System.Runtime.InteropServices;
using System.Text;

namespace TermOut;

public unsafe class LinuxConsole : ConsoleAdapter
{
    private const int TCGETS = 0x5401;
    private const int TCSETS = 0x5402;

    // termios struct layout for standard 64-bit Linux
    [StructLayout(LayoutKind.Sequential)]
    private struct Termios
    {
        public uint c_iflag;
        public uint c_oflag;
        public uint c_cflag;
        public uint c_lflag;
        public byte c_line;
        public fixed byte c_cc[32];
        public uint c_ispeed;
        public uint c_ospeed;
    }

    [DllImport("libc")]
    private static extern int ioctl(int fd, ushort request, Termios* argp);

    private Termios _originalTermios;
    private bool _rawMode = false;
    private Stream _input;

    public LinuxConsole()
    {
        Write("\x1b[?1000h\x1b[?1003h\x1b[?1006h");
        Flush();
        EnableRawMode();
        _input = Console.OpenStandardInput();
    }

    public bool GetInputEvent(out InputEvent? ev)
    {
        ev = default;
        byte[] buffer = new byte[64];
        int bytesRead = _input.Read(buffer, 0, buffer.Length);
        if (bytesRead <= 0)
        {
            return false;
        }

        string rawString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        // Handle ANSI Escape Sequences (Mouse and Special Keys)
        if (buffer[0] == 0x1b && bytesRead > 1)
        {
            // Mouse events encoded with SGR 1006 look like: \x1b[<button;x;yM or \x1b[<button;x;ym
            if (rawString.StartsWith("\x1b[<"))
            {
                ev = ParseLinuxMouse(rawString);
                if(ev != null)
                {
                    return true;
                }
            }
            else
            {
                Writer.WriteLine($"[KEY/ESCAPE] Raw Sequence: {BitConverter.ToString(buffer, 0, bytesRead)}");
            }
        }
        else if(bytesRead == 1)
        {
            ev = new KeyboardEvent(rawString[0]);
            Writer.WriteLine($"[KEY] Character: {rawString} | Byte: {buffer[0]}");
            return true;
        }
        return false;
    }

    private static MouseEvent? ParseLinuxMouse(string sequence)
    {
        try
        {
            // Remove prefix \x1b[< and parse ending character ('M' for press/move, 'm' for release)
            bool isRelease = sequence.EndsWith("m");
            string data = sequence.Substring(3, sequence.Length - 4);
            string[] parts = data.Split(';');

            if (parts.Length == 3)
            {
                int buttonCode = int.Parse(parts[0]);
                MouseEventType type = MouseEventType.Unknown;
                if ((buttonCode & 32) != 0) type = MouseEventType.MouseMove;
                else if ((buttonCode & 3) == 0) type = MouseEventType.MouseLeftClick;
                else if ((buttonCode & 3) == 1) type = MouseEventType.MouseMidleClick;
                else if ((buttonCode & 3) == 2) type = MouseEventType.MouseRightClick;

                return new MouseEvent(type, isRelease, int.Parse(parts[1]), int.Parse(parts[2]));
            }
        }
        catch
        {
        }
        return null;
    }

    protected override void Reset()
    {
        // Disable mouse tracking sequences
        Write("\x1b[?1000l\x1b[?1003l\x1b[?1006l");
        Flush();

        // Restore original terminal flags
        if (_rawMode)
        {
            fixed (Termios* ptr = &_originalTermios)
            {
                _ = ioctl(0, TCSETS, ptr);
            }
        }
    }

    private void EnableRawMode()
    {
        // 0 is standard input file descriptor (stdin)
        fixed (Termios* ptr = &_originalTermios)
        {
            if (ioctl(0, TCGETS, ptr) == 0)
            {
                Termios raw = _originalTermios;

                // Clear Canonical Mode (ICANON) and Echoing (ECHO)
                const uint ICANON = 0x00000002;
                const uint ECHO = 0x00000008;
                raw.c_lflag &= ~(ICANON | ECHO);

                if (ioctl(0, TCSETS, &raw) == 0)
                {
                    _rawMode = true;
                }
            }
        }
    }
}
