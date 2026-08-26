using System.IO;
using System.Threading;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ConPTYHarness
{
    internal static class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct COORD { public short X; public short Y; public COORD(short x, short y) { X = x; Y = y; } }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int CreatePseudoConsole(COORD size, IntPtr hInput, IntPtr hOutput, uint dwFlags, out IntPtr phPC);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ClosePseudoConsole(IntPtr hPC);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES { public int nLength; public IntPtr lpSecurityDescriptor; public bool bInheritHandle; }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint n, out uint written, IntPtr overlapped);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags;
            public short wShowWindow, cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput, hStdOutput, hStdError;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessW(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("usage: ConPTYHarness <exe> [waitMs] [inputScript]");
                Console.Error.WriteLine("inputScript: semicolon-separated: key:0xNNN / char:X / mouse:x,y,buttons / sleep:ms");
                return;
            }
            string exe = args[0];
            int waitMs = args.Length > 1 ? int.Parse(args[1]) : 3000;

            var sa = new Native.SECURITY_ATTRIBUTES { nLength = Marshal.SizeOf<Native.SECURITY_ATTRIBUTES>(), bInheritHandle = true };
            Native.CreatePipe(out var inRead, out var inWrite, sa, 0);
            Native.CreatePipe(out var outRead, out var outWrite, sa, 0);

            int hr = Native.CreatePseudoConsole(new Native.COORD(80, 25), inRead, outWrite, 0, out var hpc);
            if (hr != 0) { Console.Error.WriteLine($"CreatePseudoConsole failed 0x{hr:X}"); return; }

            IntPtr attrSize = IntPtr.Zero;
            Native.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref attrSize);
            var attrList = Marshal.AllocHGlobal(attrSize);
            Native.InitializeProcThreadAttributeList(attrList, 1, 0, ref attrSize);
            Native.UpdateProcThreadAttribute(attrList, 0, (IntPtr)0x00020016, hpc, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);

            var si = new Native.STARTUPINFOEX();
            si.StartupInfo.cb = Marshal.SizeOf<Native.STARTUPINFOEX>();
            si.lpAttributeList = attrList;

            bool created = Native.CreateProcessW(null, $"\"{exe}\"", IntPtr.Zero, IntPtr.Zero, false,
                0x80000 /*EXTENDED_STARTUPINFO_PRESENT*/, IntPtr.Zero,
                "C:\\Projects\\2026\\Xobex.Backup\\TVisionSharp",
                ref si, out var pi);
            if (!created)
            {
                Console.Error.WriteLine($"CreateProcess failed err={Marshal.GetLastWin32Error()}");
                return;
            }

            var output = new StringBuilder();
            var readThread = new Thread(() =>
            {
                var buf = new byte[65536];
                while (Native.ReadFile(outRead, buf, (uint)buf.Length, out uint got, IntPtr.Zero) && got > 0)
                    output.Append(Encoding.UTF8.GetString(buf, 0, (int)got));
            });
            readThread.Start();

            Thread.Sleep(waitMs);

            if (args.Length > 2)
            {
                foreach (var part in args[2].Split(';'))
                {
                    if (part.StartsWith("sleep:", StringComparison.Ordinal))
                        Thread.Sleep(int.Parse(part.Substring(6)));
                    else if (part.StartsWith("char:", StringComparison.Ordinal))
                    {
                        byte[] b = Encoding.UTF8.GetBytes(part.Substring(5));
                        Native.WriteFile(inWrite, b, (uint)b.Length, out _, IntPtr.Zero);
                    }
                    else if (part.StartsWith("key:", StringComparison.Ordinal))
                    {
                        string seq = part.Substring(4) switch
                        {
                            "f1" => "\x1bOP", "f2" => "\x1bOQ", "f3" => "\x1bOR", "f4" => "\x1bOS",
                            "esc" => "\x1b", "enter" => "\r", "tab" => "\t",
                            "up" => "\x1b[A", "down" => "\x1b[B", "right" => "\x1b[C", "left" => "\x1b[D",
                            _ => ""
                        };
                        byte[] b = Encoding.UTF8.GetBytes(seq);
                        if (b.Length > 0)
                            Native.WriteFile(inWrite, b, (uint)b.Length, out _, IntPtr.Zero);
                    }
                }
                Thread.Sleep(800);
            }

            Thread.Sleep(300);
            Native.TerminateProcess(pi.hProcess, 0);
            Native.ClosePseudoConsole(hpc);
            readThread.Join(1000);

            File.WriteAllText(
                Environment.GetEnvironmentVariable("CAPTURE_OUT") ?? "capture.txt",
                output.ToString());

            Console.Error.WriteLine($"captured {output.Length} chars -> {(Environment.GetEnvironmentVariable("CAPTURE_OUT") ?? "capture.txt")}");
        }

        static void SendLine(IntPtr hIn, string line)
        {
            var parts = line.Split(';');
            foreach (var part in parts)
            {
                if (part.StartsWith("char:", StringComparison.Ordinal))
                {
                    byte[] utf8 = Encoding.UTF8.GetBytes(part.Substring(5));
                    Native.WriteFile(hIn, utf8, (uint)utf8.Length, out _, IntPtr.Zero);
                }
                else if (part.StartsWith("sleep:", StringComparison.Ordinal))
                {
                    Thread.Sleep(int.Parse(part.Substring(6)));
                }
            }
        }
    }
}
