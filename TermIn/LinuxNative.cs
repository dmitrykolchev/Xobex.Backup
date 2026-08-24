using System.Runtime.InteropServices;

namespace TermIn;

internal unsafe class LinuxNative
{
    public const int STDIN_FILENO = 0;
    public const int TCSANOW = 0;
    public const int TCSADRAIN = 1;
    public const int TCSAFLUSH = 2;

    // termios struct layout for standard 64-bit Linux
    [StructLayout(LayoutKind.Sequential)]
    public struct termios
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
    public static extern void cfmakeraw(termios* termios_p);

    [DllImport("libc")]
    public static extern int tcgetattr(int fd, termios* termios_p);

    [DllImport("libc")]
    public static extern int tcsetattr(int fd, int optional_action, termios* termios_p);

    public const short POLLIN = 0x0001;

    [StructLayout(LayoutKind.Sequential)]
    public struct pollfd
    {
        public int fd;
        public short events;
        public short revents;
    }

    [DllImport("libc")]
    public static extern int poll(pollfd* fds, ulong nfds, int timeout);

    [DllImport("libc")]
    public static extern nint read(int fd, byte* buf, nint count);

    public const int VINTR = 0;
    public const int VQUIT = 1;
    public const int VERASE = 2;
    public const int VKILL = 3;
    public const int VEOF = 4;
    public const int VTIME = 5;
    public const int VMIN = 6;
    public const int VSWTC = 7;
    public const int VSTART = 8;
    public const int VSTOP = 9;
    public const int VSUSP = 10;
    public const int VEOL = 11;
    public const int VREPRINT = 12;
    public const int VDISCARD = 13;
    public const int VWERASE = 14;
    public const int VLNEXT = 15;
    public const int VEOL2 = 16;


    public const uint IGNBRK = 0x0000001;  /* Ignore break condition.  */
    public const uint BRKINT = 0x0000002;  /* Signal interrupt on break.  */
    public const uint IGNPAR = 0x0000004;  /* Ignore characters with parity errors.  */
    public const uint PARMRK = 0x0000008;  /* Mark parity and framing errors.  */
    public const uint INPCK = 0x0000010;  /* Enable input parity check.  */
    public const uint ISTRIP = 0x0000020;  /* Strip 8th bit off characters.  */
    public const uint INLCR = 0x000040;  /* Map NL to CR on input.  */
    public const uint IGNCR = 0x000080;  /* Ignore CR.  */
    public const uint ICRNL = 0x0000100;  /* Map CR to NL on input.  */
    public const uint IUCLC = 0x000200;  /* Map uppercase characters to lowercase on input (not in POSIX).  */
    public const uint IXON = 0x000400;  /* Enable start/stop output control.  */
    public const uint IXANY = 0x000800;  /* Enable any character to restart output.  */
    public const uint IXOFF = 0x001000;  /* Enable start/stop input control.  */
    public const uint IMAXBEL = 0x002000;  /* Ring bell when input queue is full (not in POSIX).  */
    public const uint IUTF8 = 0x004000;  /* Input is UTF8 (not in POSIX).  */

    public const uint OPOST = 0x01;/* Perform output processing */
    public const uint OCRNL = 0x08;
    public const uint ONOCR = 0x10;
    public const uint ONLRET = 0x20;
    public const uint OFILL = 0x40;
    public const uint OFDEL = 0x80;

    public const uint ISIG = 0x00001;
    public const uint ICANON = 0x00002;
    public const uint XCASE = 0x00004;
    public const uint ECHO = 0x00008;
    public const uint ECHOE = 0x00010;
    public const uint ECHOK = 0x00020;
    public const uint ECHONL = 0x00040;
    public const uint NOFLSH = 0x00080;
    public const uint TOSTOP = 0x00100;
    public const uint ECHOCTL = 0x00200;
    public const uint ECHOPRT = 0x00400;
    public const uint ECHOKE = 0x00800;
    public const uint FLUSHO = 0x01000;
    public const uint PENDIN = 0x04000;
    public const uint IEXTEN = 0x08000;
    public const uint EXTPROC = 0x10000;

    public const uint CBAUD = 0x0000100f;
    public const uint CSIZE = 0x00000030;
    public const uint CS5 = 0x00000000;
    public const uint CS6 = 0x00000010;
    public const uint CS7 = 0x00000020;
    public const uint CS8 = 0x00000030;
    public const uint CSTOPB = 0x00000040;
    public const uint CREAD = 0x00000080;
    public const uint PARENB = 0x00000100;
    public const uint PARODD = 0x00000200;
    public const uint HUPCL = 0x00000400;
    public const uint CLOCAL = 0x00000800;
    public const uint CBAUDEX = 0x00001000;
    public const uint BOTHER = 0x00001000;
    public const uint B57600 = 0x00001001;
    public const uint B115200 = 0x00001002;
    public const uint B230400 = 0x00001003;
    public const uint B460800 = 0x00001004;
    public const uint B500000 = 0x00001005;
    public const uint B576000 = 0x00001006;
    public const uint B921600 = 0x00001007;
    public const uint B1000000 = 0x00001008;
    public const uint B1152000 = 0x00001009;
    public const uint B1500000 = 0x0000100a;
    public const uint B2000000 = 0x0000100b;
    public const uint B2500000 = 0x0000100c;
    public const uint B3000000 = 0x0000100d;
    public const uint B3500000 = 0x0000100e;
    public const uint B4000000 = 0x0000100f;
    public const uint CIBAUD = 0x100f0000;

}
