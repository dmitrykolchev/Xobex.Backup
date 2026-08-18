using System.Runtime.InteropServices;
using System.Text;

namespace TermIn;

public unsafe class LinuxInputAdapter
{
    private const int TCGETS = 0x5401;
    private const int TCSETS = 0x5402;

    // termios struct layout for standard 64-bit Linux
    [StructLayout(LayoutKind.Sequential)]
    private struct termios
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
    private static extern int ioctl(int fd, ushort request, termios* argp);

    [DllImport("libc")]
    private static extern void cfmakeraw(termios* termios_p);

    [DllImport("libc")]
    private static extern int tcgetattr(int fd, termios* termios_p);

    [DllImport("libc")]
    private static extern int tcsetattr(int fd, int optional_action, termios* termios_p);

    [DllImport("libc")]
    private static extern nint read(int fd, byte* buf, nint count);

    private termios _original;
    private bool _rawMode = false;

    public LinuxInputAdapter()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Stream baseStream = Console.OpenStandardOutput(128 * 1024);
        Encoding noBomEncoding = new UTF8Encoding(false);
        Writer = new StreamWriter(baseStream, noBomEncoding, 128 * 1024);

        EnableRawMode();
        Writer.Write("\x1b[?1000h\x1b[?1003h\x1b[?1006h");
        Writer.Flush();
    }

    public bool GetInputEvent(out InputEvent? ev)
    {
        ev = default;
        byte* buffer = stackalloc byte[64];
        nint bytesRead = read(STDIN_FILENO, buffer, 64);
        if (bytesRead <= 0)
        {
            return true;
        }

        ReadOnlySpan<byte> data = new(buffer, (int)bytesRead);
        StringBuilder sb = new StringBuilder();
        foreach (byte b in data)
        {
            sb.Append(b < 32 ? $"\\x{b:X2}" : (char)b);
        }

        Writer.Write($"\'{sb.ToString()}\' ({BitConverter.ToString(data.ToArray())})\r\n");
        Writer.Flush();
        if (buffer[0] == (byte)'q')
        {
            return false;
        }
        return true;
    }

    private const int STDIN_FILENO = 0;
    private const int TCSANOW = 0;
    private const int TCSADRAIN = 1;
    private const int TCSAFLUSH = 2;

    public void Reset()
    {
        // Disable mouse tracking sequences
        Writer.Write("\x1b[?1000l\x1b[?1003l\x1b[?1006l");
        Writer.Flush();

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

    public TextWriter Writer { get; }

    private const int VINTR = 0;
    private const int VQUIT = 1;
    private const int VERASE = 2;
    private const int VKILL = 3;
    private const int VEOF = 4;
    private const int VTIME = 5;
    private const int VMIN = 6;
    private const int VSWTC = 7;
    private const int VSTART = 8;
    private const int VSTOP = 9;
    private const int VSUSP = 10;
    private const int VEOL = 11;
    private const int VREPRINT = 12;
    private const int VDISCARD = 13;
    private const int VWERASE = 14;
    private const int VLNEXT = 15;
    private const int VEOL2 = 16;


    private const uint IGNBRK = 0x0000001;  /* Ignore break condition.  */
    private const uint BRKINT = 0x0000002;  /* Signal interrupt on break.  */
    private const uint IGNPAR = 0x0000004;  /* Ignore characters with parity errors.  */
    private const uint PARMRK = 0x0000008;  /* Mark parity and framing errors.  */
    private const uint INPCK = 0x0000010;  /* Enable input parity check.  */
    private const uint ISTRIP = 0x0000020;  /* Strip 8th bit off characters.  */
    private const uint INLCR = 0x000040;  /* Map NL to CR on input.  */
    private const uint IGNCR = 0x000080;  /* Ignore CR.  */
    private const uint ICRNL = 0x0000100;  /* Map CR to NL on input.  */
    private const uint IUCLC = 0x000200;  /* Map uppercase characters to lowercase on input (not in POSIX).  */
    private const uint IXON = 0x000400;  /* Enable start/stop output control.  */
    private const uint IXANY = 0x000800;  /* Enable any character to restart output.  */
    private const uint IXOFF = 0x001000;  /* Enable start/stop input control.  */
    private const uint IMAXBEL = 0x002000;  /* Ring bell when input queue is full (not in POSIX).  */
    private const uint IUTF8 = 0x004000;  /* Input is UTF8 (not in POSIX).  */


    private const uint OPOST = 0x01;/* Perform output processing */
    private const uint OCRNL = 0x08;
    private const uint ONOCR = 0x10;
    private const uint ONLRET = 0x20;
    private const uint OFILL = 0x40;
    private const uint OFDEL = 0x80;

    private const uint ISIG = 0x00001;
    private const uint ICANON = 0x00002;
    private const uint XCASE = 0x00004;
    private const uint ECHO = 0x00008;
    private const uint ECHOE = 0x00010;
    private const uint ECHOK = 0x00020;
    private const uint ECHONL = 0x00040;
    private const uint NOFLSH = 0x00080;
    private const uint TOSTOP = 0x00100;
    private const uint ECHOCTL = 0x00200;
    private const uint ECHOPRT = 0x00400;
    private const uint ECHOKE = 0x00800;
    private const uint FLUSHO = 0x01000;
    private const uint PENDIN = 0x04000;
    private const uint IEXTEN = 0x08000;
    private const uint EXTPROC = 0x10000;


    private const uint CBAUD = 0x0000100f;
    private const uint CSIZE = 0x00000030;
    private const uint CS5 = 0x00000000;
    private const uint CS6 = 0x00000010;
    private const uint CS7 = 0x00000020;
    private const uint CS8 = 0x00000030;
    private const uint CSTOPB = 0x00000040;
    private const uint CREAD = 0x00000080;
    private const uint PARENB = 0x00000100;
    private const uint PARODD = 0x00000200;
    private const uint HUPCL = 0x00000400;
    private const uint CLOCAL = 0x00000800;
    private const uint CBAUDEX = 0x00001000;
    private const uint BOTHER = 0x00001000;
    private const uint B57600 = 0x00001001;
    private const uint B115200 = 0x00001002;
    private const uint B230400 = 0x00001003;
    private const uint B460800 = 0x00001004;
    private const uint B500000 = 0x00001005;
    private const uint B576000 = 0x00001006;
    private const uint B921600 = 0x00001007;
    private const uint B1000000 = 0x00001008;
    private const uint B1152000 = 0x00001009;
    private const uint B1500000 = 0x0000100a;
    private const uint B2000000 = 0x0000100b;
    private const uint B2500000 = 0x0000100c;
    private const uint B3000000 = 0x0000100d;
    private const uint B3500000 = 0x0000100e;
    private const uint B4000000 = 0x0000100f;
    private const uint CIBAUD = 0x100f0000;

    private void EnableRawMode()
    {
        // 0 is standard input file descriptor (stdin)
        fixed (termios* ptr = &_original)
        {
            if (tcgetattr(STDIN_FILENO, ptr) == 0)
            {
                termios raw = _original;
                cfmakeraw(&raw);
                //raw.c_iflag &= ~(IGNBRK | BRKINT | PARMRK | ISTRIP
                //                | INLCR | IGNCR | ICRNL | IXON);
                //raw.c_oflag &= ~OPOST;
                //raw.c_lflag &= ~(ECHO | ECHONL | ICANON | ISIG | IEXTEN);
                //raw.c_cflag &= ~(CSIZE | PARENB);
                //raw.c_cflag |= CS8;
                //raw.c_cc[VMIN] = 1;  // Читать как минимум 1 байт (символ) за раз
                //raw.c_cc[VTIME] = 0;
                int err = tcsetattr(STDIN_FILENO, TCSANOW, &raw);
                if (err != 0)
                {
                    throw new InvalidOperationException($"tcsetattr error: {err}");
                }
                _rawMode = true;
            }
        }
    }
}
