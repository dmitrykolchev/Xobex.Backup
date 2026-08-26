using System;

namespace TVision
{
    public class TEventQueue
    {
        public static ushort DoubleDelay = 8;
        public static bool MouseReverse;

        private static MouseEventType _lastMouse;
        private static MouseEventType _curMouse;
        private static MouseEventType _downMouse;
        private static ushort _downTicks;
        private static bool _mouseEvents;
        private static bool _pendingMouseUp;
        private static ushort _repeatDelay = 1;
        private static ushort _autoTicks;
        private static ushort _autoDelay = 20;

        private static string _pasteText;
        private static int _pasteTextLength;
        private static int _pasteTextIndex;
        private static TEvent[] _keyEventQueue = new TEvent[Config.MinPasteEventCount];
        private static int _keyEventCount;
        private static int _keyEventIndex;
        private static bool _pasteState;

        public static void GetMouseEvent(ref TEvent ev)
        {
            ev.What = EventCodes.EvNothing;
            if (Platform.InputBroken)
            {
                PipeInput.GetEvent(ref ev, 0);
                return;
            }

            var adapter = Platform.GetAdapter();
            while (adapter.GetMouseEvent(out var kind, out var m))
            {
                switch (kind)
                {
                    case MouseEvtKind.Down:
                        ev.What = EventCodes.EvMouseDown;
                        ev.Mouse = m;
                        _curMouse = m;
                        return;
                    case MouseEvtKind.Up:
                        ev.What = EventCodes.EvMouseUp;
                        ev.Mouse = m;
                        _lastMouse = m;
                        return;
                    case MouseEvtKind.Move:
                        ev.What = (m.Buttons != 0 || (m.EventFlags & EventCodes.MeMouseMoved) != 0)
                            ? EventCodes.EvMouseMove : EventCodes.EvNothing;
                        ev.Mouse = m;
                        _curMouse = m;
                        if (ev.What != EventCodes.EvNothing) return;
                        break;
                }
            }
        }

        public static void GetKeyEvent(ref TEvent ev)
        {
            ev.What = EventCodes.EvNothing;

            if (_pasteText != null && _pasteTextIndex < _pasteTextLength)
            {
                char c = _pasteText[_pasteTextIndex];
                ev.What = EventCodes.EvKeyDown;
                ushort code = c switch
                {
                    '\u001B' => KeyCodes.KbEsc,
                    '\r' or '\n' => KeyCodes.KbEnter,
                    '\t' => KeyCodes.KbTab,
                    '\b' => KeyCodes.KbBack,
                    _ => (ushort)c
                };
                ev.KeyDown.KeyCode = code;
                if (code >= 0x100)
                {
                    ev.KeyDown.Char0 = '\0';
                    ev.KeyDown.TextLength = 0;
                }
                else
                {
                    ev.KeyDown.Char0 = c;
                    ev.KeyDown.TextLength = 1;
                }
                _pasteTextIndex++;
                return;
            }

            if (Platform.InputBroken)
            {
                if (PipeInput.GetEvent(ref ev, 10))
                    return;
                ev.What = EventCodes.EvNothing;
                return;
            }

            var adapter = Platform.GetAdapter();
            ushort keyCode, controlKeyState;
            string text;
            if (adapter.GetKeyEvent(out keyCode, out controlKeyState, out text))
            {
                ev.What = EventCodes.EvKeyDown;
                ev.KeyDown.KeyCode = keyCode;
                ev.KeyDown.ControlKeyState = controlKeyState;
                if (text != null && text.Length > 0)
                {
                    ev.KeyDown.Char0 = text[0];
                    ev.KeyDown.TextLength = (byte)Math.Min(text.Length, 4);
                }
            }
        }

        public static void Suspend() { }
        public static void Resume() { }
        public static void WaitForEvents(int timeoutMs) { }
        public static void WakeUp() { }

        public static void SetPasteText(string text)
        {
            _pasteText = text;
            _pasteTextLength = text?.Length ?? 0;
            _pasteTextIndex = 0;
        }
    }

    public class TTimerId { }
    public delegate ulong TTimePoint();
    public delegate void TimerCallback(object id, object args);

    public class TTimer : TTimerId
    {
        public object Id;
        public uint TimeoutMs;
        public int PeriodMs;
        public ulong ExpiresAt;
        public TimerCallback Callback;
        public object Args;
        public TTimer Next;
    }

    public class TTimerQueue
    {
        private TTimer _first;
        private Func<ulong> _getTimeMs;

        public TTimerQueue() { _getTimeMs = () => (ulong)Environment.TickCount; }
        public TTimerQueue(Func<ulong> getTimeMs) { _getTimeMs = getTimeMs; }

        public TTimerId SetTimer(uint timeoutMs, int periodMs = -1)
        {
            var timer = new TTimer
            {
                TimeoutMs = timeoutMs,
                PeriodMs = periodMs,
                ExpiresAt = _getTimeMs() + timeoutMs
            };

            if (_first == null)
            {
                _first = timer;
            }
            else
            {
                var cur = _first;
                TTimer prev = null;
                while (cur != null && cur.ExpiresAt <= timer.ExpiresAt)
                {
                    prev = cur;
                    cur = cur.Next;
                }
                timer.Next = cur;
                if (prev != null) prev.Next = timer;
                else _first = timer;
            }
            return timer;
        }

        public void KillTimer(object id)
        {
            var cur = _first;
            TTimer prev = null;
            while (cur != null)
            {
                if (cur.Id == id)
                {
                    if (prev != null) prev.Next = cur.Next;
                    else _first = cur.Next;
                    return;
                }
                prev = cur;
                cur = cur.Next;
            }
        }

        public void CollectExpiredTimers(Action<TTimerId, object> func)
        {
            var now = _getTimeMs();
            var cur = _first;
            TTimer prev = null;
            while (cur != null)
            {
                if (cur.ExpiresAt <= now)
                {
                    func(cur, cur.Args);
                    if (prev != null) prev.Next = cur.Next;
                    else _first = cur.Next;
                    if (cur.PeriodMs >= 0)
                    {
                        cur.ExpiresAt = now + (ulong)cur.PeriodMs;
                        if (_first == null || cur.ExpiresAt <= _first.ExpiresAt)
                        {
                            cur.Next = _first;
                            _first = cur;
                        }
                    }
                    cur = prev?.Next ?? _first;
                    continue;
                }
                prev = cur;
                cur = cur.Next;
            }
        }

        public int TimeUntilNextTimeout()
        {
            if (_first == null) return -1;
            var now = _getTimeMs();
            if (_first.ExpiresAt <= now) return 0;
            return (int)(_first.ExpiresAt - now);
        }
    }

    public class TClipboard
    {
        private static TClipboard _instance;
        private static string _localText;

        public static void SetText(string text)
        {
            try
            {
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        RedirectStandardInput = true,
                        RedirectStandardOutput = false,
                        UseShellExecute = false
                    };

                    if (OperatingSystem.IsWindows())
                    {
                        psi.FileName = "clip";
                        var proc = System.Diagnostics.Process.Start(psi);
                        proc?.StandardInput.Write(text);
                        proc?.StandardInput.Close();
                        proc?.WaitForExit();
                    }
                    else
                    {
                        psi.FileName = "xclip";
                        psi.Arguments = "-selection clipboard";
                        var proc = System.Diagnostics.Process.Start(psi);
                        proc?.StandardInput.Write(text);
                        proc?.StandardInput.Close();
                        proc?.WaitForExit();
                    }
                }
            }
            catch { }
            _localText = text;
        }

        public static string GetText()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-command \"Get-Clipboard\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    var proc = System.Diagnostics.Process.Start(psi);
                    var result = proc?.StandardOutput.ReadToEnd()?.TrimEnd();
                    proc?.WaitForExit();
                    if (!string.IsNullOrEmpty(result)) return result;
                }
                else
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "xclip",
                        Arguments = "-selection clipboard -o",
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    var proc = System.Diagnostics.Process.Start(psi);
                    var result = proc?.StandardOutput.ReadToEnd();
                    proc?.WaitForExit();
                    if (!string.IsNullOrEmpty(result)) return result;
                }
            }
            catch { }
            return _localText ?? string.Empty;
        }
    }

    public class TSystemError
    {
        public static bool CtrlBreakHit;
    }

    public class TMouse
    {
        private static byte _buttonCount;
        private static bool _present;

        public TMouse()
        {
            _present = true;
            _buttonCount = 2;
        }

        public static void Show() { }
        public static void Hide() { }
        public static void SetRange(ushort rx, ushort ry) { }
        public static void GetEvent(MouseEventType ev) { }
        public static bool Present() => _present;
        public static byte ButtonCount() => _buttonCount;
    }

    public enum VideoModes
    {
        SmBW80 = 0x0002,
        SmCO80 = 0x0003,
        SmMono = 0x0007,
        SmFont8x8 = 0x0100,
        SmColor256 = 0x0200,
        SmColorHigh = 0x0400,
        SmUpdate = 0x8000
    }

    public class TDisplay
    {
        public static void ClearScreen(byte width, byte height) { }

        public static void SetCursorType(ushort cursorType) { }
        public static ushort GetCursorType() => 0;

        public static ushort GetRows()
        {
            try { return (ushort)Console.WindowHeight; }
            catch { return 25; }
        }

        public static ushort GetCols()
        {
            try { return (ushort)Console.WindowWidth; }
            catch { return 80; }
        }

        public static void SetCrtMode(ushort mode) { }
        public static ushort GetCrtMode() => 0;
    }

    public class TScreen : TDisplay
    {
        public static ushort StartupMode;
        public static ushort StartupCursor;
        public static ushort ScreenMode;
        public static ushort ScreenWidth;
        public static ushort ScreenHeight;
        public static bool HiResScreen;
        public static bool CheckSnow;
        public static TScreenCell[] ScreenBuffer;
        public static ushort CursorLines;
        public static bool ClearOnSuspend = true;

        public static void SetVideoMode(ushort mode)
        {
            ScreenMode = mode;
            SetCrtData();
        }

        public static void ClearScreen()
        {
            try { Console.Clear(); } catch { }
            if (ScreenBuffer != null)
            {
                for (int i = 0; i < ScreenBuffer.Length; i++)
                    ScreenBuffer[i] = default;
            }
        }

        public static void FlushScreen()
        {
            var adapter = Platform.GetAdapter();
            if (ScreenBuffer != null)
                adapter.FlushScreen();
        }

        public static void SetCrtData()
        {
            try
            {
                ScreenWidth = (ushort)Console.WindowWidth;
                ScreenHeight = (ushort)Console.WindowHeight;
            }
            catch
            {
                ScreenWidth = 80;
                ScreenHeight = 25;
            }
        }

        public static ushort FixCrtMode(ushort mode) => mode;

        public static void Suspend()
        {
        }

        public static void Resume()
        {
        }
    }
}
