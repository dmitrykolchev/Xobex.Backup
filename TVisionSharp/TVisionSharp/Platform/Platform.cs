using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TVision
{
    public enum MouseEvtKind
    {
        None = 0,
        Down,
        Up,
        Move
    }

    public abstract class ConsoleAdapter
    {
        public abstract void Init();
        public abstract void Shutdown();
        public abstract int GetRows();
        public abstract int GetCols();
        public abstract void SetCursorType(int size, bool visible);
        public abstract void SetCursorPos(int x, int y);
        public abstract void WriteCell(int x, int y, TScreenCell cell);
        public abstract void WriteBlock(int x, int y, int w, int h, TScreenCell[] buf);
        public abstract bool GetKeyEvent(out ushort keyCode, out ushort controlKeyState, out string text);
        public abstract bool GetMouseEvent(out MouseEvtKind kind, out MouseEventType ev);
        public abstract bool SetClipboardText(string text);
        public abstract bool GetClipboardText(out string text);
        public abstract void FlushScreen();

        public virtual void Invalidate()
        {
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct COORD
    {
        public short X;
        public short Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEY_EVENT_RECORD
    {
        public int bKeyDown;
        public ushort wRepeatCount;
        public ushort wVirtualKeyCode;
        public ushort wVirtualScanCode;
        public ushort UnicodeChar;
        public uint dwControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSE_EVENT_RECORD
    {
        public COORD dwMousePosition;
        public uint dwButtonState;
        public uint dwControlKeyState;
        public uint dwEventFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOW_BUFFER_SIZE_RECORD
    {
        public COORD dwSize;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct INPUT_UNION
    {
        [FieldOffset(0)] public KEY_EVENT_RECORD KeyEvent;
        [FieldOffset(0)] public MOUSE_EVENT_RECORD MouseEvent;
        [FieldOffset(0)] public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT_RECORD
    {
        public ushort EventType;
        public INPUT_UNION Event;
    }

    public class Win32ConsoleAdapter : ConsoleAdapter
    {
        private IntPtr _handleIn;
        private IntPtr _handleOut;
        private uint _savedOutputMode;
        private uint _savedInputMode;
        private TScreenCell[] _prevBuffer;
        private int _prevWidth;
        private int _prevHeight;

        private readonly Queue<KEY_EVENT_RECORD> _keyQueue = new Queue<KEY_EVENT_RECORD>();
        private readonly Queue<MOUSE_EVENT_RECORD> _mouseQueue = new Queue<MOUSE_EVENT_RECORD>();

        private uint _prevButtonState;
        private short _lastMouseX = -1;
        private short _lastMouseY = -1;
        private readonly Queue<KeyValuePair<MouseEvtKind, MouseEventType>> _pendingMouse =
            new Queue<KeyValuePair<MouseEvtKind, MouseEventType>>();

        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
        private const uint ENABLE_MOUSE_INPUT = 0x0010;
        private const uint ENABLE_WINDOW_INPUT = 0x0008;
        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        private const uint ENABLE_QUICK_EDIT_MODE = 0x0040;

        private const ushort KEY_EVENT = 0x0001;
        private const ushort MOUSE_EVENT = 0x0002;
        private const ushort WINDOW_BUFFER_SIZE_EVENT = 0x0004;
        private const uint MOUSE_MOVED = 0x0001;
        private const uint DOUBLE_CLICK = 0x0002;
        private const uint MOUSE_WHEELED = 0x0004;
        private const uint SHIFT_PRESSED = 0x0010;
        private const uint LEFT_ALT_PRESSED = 0x0002;
        private const uint LEFT_CTRL_PRESSED = 0x0008;
        private const uint RIGHT_ALT_PRESSED = 0x0001;
        private const uint RIGHT_CTRL_PRESSED = 0x0004;

        private static readonly ushort[] ScanForVirtualKey =
        {
            0x00, 0x0E, 0x0F, 0x1D, 0x38, 0x2A, 0x36, 0x1C, 0x01, 0x0F,
            0x39, 0x33, 0x34, 0x35, 0x1B, 0x52, 0x49, 0x51, 0x4F, 0x48,
            0x50, 0x4B, 0x4D, 0x47, 0x4A, 0x4E, 0x53, 0x02, 0x03, 0x04,
            0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x1A,
            0x1B, 0x07, 0x0C, 0x28, 0x29, 0x33, 0x35, 0x56, 0x0C, 0x27,
            0x26, 0x22, 0x23, 0x24, 0x25, 0x2E, 0x2F, 0x30, 0x31, 0x32,
            0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1E, 0x1F,
            0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x2C,
            0x2D, 0x2E, 0x2F, 0x11, 0x10, 0x36, 0x38, 0x45, 0x46, 0x44,
            0x43, 0x3F, 0x42, 0x41, 0x3E, 0x40, 0x41
        };

        public override void Init()
        {
            try
            {
                _handleIn = GetStdHandle(-10);
                _handleOut = GetStdHandle(-11);

                GetConsoleMode(_handleOut, out _savedOutputMode);
                GetConsoleMode(_handleIn, out _savedInputMode);

                uint outMode = _savedOutputMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING;
                SetConsoleMode(_handleOut, outMode);

                uint inMode = (uint)((int)(_savedInputMode | ENABLE_WINDOW_INPUT) & ~ENABLE_QUICK_EDIT_MODE);
                inMode |= ENABLE_EXTENDED_FLAGS | ENABLE_MOUSE_INPUT;
                SetConsoleMode(_handleIn, inMode);
            }
            catch { }

            try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            try { Console.Write("\x1b[?25l"); } catch { }
        }

        public override void Shutdown()
        {
            try
            {
                Console.Write("\x1b[?25h");
                Console.Write("\x1b[0m");
                Console.Write("\x1b[2J\x1b[H");

                if (_handleOut != IntPtr.Zero)
                    SetConsoleMode(_handleOut, _savedOutputMode);
                if (_handleIn != IntPtr.Zero)
                    SetConsoleMode(_handleIn, _savedInputMode);
            }
            catch { }
        }

        public override int GetRows()
        {
            try { return Console.WindowHeight; }
            catch { return 25; }
        }

        public override int GetCols()
        {
            try { return Console.WindowWidth; }
            catch { return 80; }
        }

        public override void SetCursorType(int size, bool visible)
        {
            try
            {
                if (visible) Console.Write("\x1b[?25h");
                else Console.Write("\x1b[?25l");
            }
            catch { }
        }

        public override void SetCursorPos(int x, int y)
        {
            try { Console.Write($"\x1b[{y + 1};{x + 1}H"); }
            catch { }
        }

        public override void WriteCell(int x, int y, TScreenCell cell)
        {
            try
            {
                Console.Write($"\x1b[{y + 1};{x + 1}H");
                Console.Write(Platform.ColorToAnsiFg(cell.Attr));
                Console.Write(Platform.ColorToAnsiBg(cell.Attr));
                Console.Write(cell.Ch == '\0' ? ' ' : cell.Ch);
                Console.Write("\x1b[0m");
            }
            catch { }
        }

        public override void WriteBlock(int x, int y, int w, int h, TScreenCell[] buf)
        {
            try
            {
                var sb = new StringBuilder(w * h * 20);
                for (int row = 0; row < h; row++)
                {
                    sb.Append("\x1b[").Append(y + row + 1).Append(';').Append(x + 1).Append('H');
                    int lastFg = -1, lastBg = -1;
                    for (int col = 0; col < w; col++)
                    {
                        var cell = buf[row * w + col];
                        int fg = (int)cell.Attr.Foreground._data;
                        int bg = (int)cell.Attr.Background._data;
                        if (fg != lastFg || lastFg == -1)
                        {
                            sb.Append(Platform.ColorToAnsiFg(cell.Attr));
                            lastFg = fg;
                        }
                        if (bg != lastBg || lastBg == -1)
                        {
                            sb.Append(Platform.ColorToAnsiBg(cell.Attr));
                            lastBg = bg;
                        }
                        sb.Append(cell.Ch == '\0' ? ' ' : cell.Ch);
                    }
                }
                sb.Append("\x1b[0m");
                Console.Write(sb.ToString());
            }
            catch { }
        }

        private void PumpInput()
        {
            if (_handleIn == IntPtr.Zero) return;
            var records = new INPUT_RECORD[32];
            while (true)
            {
                if (!PeekConsoleInput(_handleIn, records, (uint)records.Length, out uint read) || read == 0)
                    break;
                if (!ReadConsoleInput(_handleIn, records, read, out uint got) || got == 0)
                    break;
                for (uint i = 0; i < got; i++)
                {
                    switch (records[i].EventType)
                    {
                        case KEY_EVENT:
                            _keyQueue.Enqueue(records[i].Event.KeyEvent);
                            break;
                        case MOUSE_EVENT:
                            TranslateMouseEvent(records[i].Event.MouseEvent);
                            break;
                        case WINDOW_BUFFER_SIZE_EVENT:
                            break;
                    }
                }
            }
        }

        private void TranslateMouseEvent(MOUSE_EVENT_RECORD m)
        {
            int buttons = 0;
            if ((m.dwButtonState & 0x0001) != 0) buttons |= EventCodes.MbLeftButton;
            if ((m.dwButtonState & 0x0002) != 0) buttons |= EventCodes.MbRightButton;
            if ((m.dwButtonState & 0x0004) != 0) buttons |= EventCodes.MbMiddleButton;

            var where = new TPoint(m.dwMousePosition.X, m.dwMousePosition.Y);
            ushort ctrl = MapControlKeys(m.dwControlKeyState);
            bool isMove = (m.dwEventFlags & MOUSE_MOVED) != 0 && m.dwEventFlags != 0;
            bool isWheel = (m.dwEventFlags & MOUSE_WHEELED) != 0;
            bool isDouble = (m.dwEventFlags & DOUBLE_CLICK) != 0;

            if (isWheel)
            {
                short delta = (short)((m.dwButtonState >> 16) & 0xFFFF);
                var ev = new MouseEventType();
                ev.Where = where;
                ev.Buttons = (byte)buttons;
                ev.ControlKeyState = ctrl;
                ev.Wheel = delta > 0 ? (byte)EventCodes.MwUp : (byte)EventCodes.MwDown;
                ev.EventFlags = (ushort)EventCodes.MeMouseMoved;
                _pendingMouse.Enqueue(new KeyValuePair<MouseEvtKind, MouseEventType>(MouseEvtKind.Down, ev));
                return;
            }

            uint pressedNow = (uint)buttons & ~_prevButtonState;
            uint releasedNow = _prevButtonState & ~(uint)buttons;

            if (pressedNow != 0 || (buttons != _prevButtonState && !isMove))
            {
                var ev = new MouseEventType();
                ev.Where = where;
                ev.Buttons = buttons != 0 ? (byte)buttons : (byte)_prevButtonState;
                ev.ControlKeyState = ctrl;
                ev.EventFlags = isDouble ? (ushort)EventCodes.MeDoubleClick : (ushort)0;
                _pendingMouse.Enqueue(new KeyValuePair<MouseEvtKind, MouseEventType>(MouseEvtKind.Down, ev));
            }
            else if (releasedNow != 0)
            {
                var ev = new MouseEventType();
                ev.Where = where;
                ev.Buttons = (byte)buttons;
                ev.ControlKeyState = ctrl;
                ev.EventFlags = 0;
                _pendingMouse.Enqueue(new KeyValuePair<MouseEvtKind, MouseEventType>(MouseEvtKind.Up, ev));
            }
            else if (isMove && (where.X != _lastMouseX || where.Y != _lastMouseY))
            {
                var ev = new MouseEventType();
                ev.Where = where;
                ev.Buttons = (byte)buttons;
                ev.ControlKeyState = ctrl;
                ev.EventFlags = (ushort)EventCodes.MeMouseMoved;
                _pendingMouse.Enqueue(new KeyValuePair<MouseEvtKind, MouseEventType>(MouseEvtKind.Move, ev));
            }

            _prevButtonState = (uint)buttons;
            _lastMouseX = (short)where.X;
            _lastMouseY = (short)where.Y;
        }

        private static ushort MapControlKeys(uint state)
        {
            ushort r = 0;
            if ((state & (LEFT_CTRL_PRESSED | RIGHT_CTRL_PRESSED)) != 0) r |= KeyCodes.KbCtrlShift;
            if ((state & (LEFT_ALT_PRESSED | RIGHT_ALT_PRESSED)) != 0) r |= KeyCodes.KbAltShift;
            if ((state & SHIFT_PRESSED) != 0) r |= KeyCodes.KbShift;
            return r;
        }

        public override bool GetKeyEvent(out ushort keyCode, out ushort controlKeyState, out string text)
        {
            keyCode = 0;
            controlKeyState = 0;
            text = null;
            try
            {
                PumpInput();
                while (_keyQueue.Count > 0)
                {
                    var k = _keyQueue.Dequeue();
                    if (k.bKeyDown == 0) continue;
                    if (k.UnicodeChar == 0 && k.wVirtualScanCode == 0) continue;

                    controlKeyState = MapControlKeys(k.dwControlKeyState);
                    bool isCtrl = (controlKeyState & KeyCodes.KbCtrlShift) != 0;
                    bool isAlt = (controlKeyState & KeyCodes.KbAltShift) != 0;
                    byte scan = (byte)k.wVirtualScanCode;
                    char ch = (char)k.UnicodeChar;

                    keyCode = BuildKeyCode(scan, ch, isCtrl, isAlt);
                    if (keyCode != KeyCodes.KbNoKey)
                    {
                        if (ch >= ' ' && !isCtrl && !isAlt)
                            text = ch.ToString();
                        return true;
                    }
                }
            }
            catch (InvalidOperationException) { Platform.MarkInputBroken(); }
            catch (System.IO.IOException) { Platform.MarkInputBroken(); }
            catch { }
            return false;
        }

        private static ushort BuildKeyCode(byte scan, char ch, bool isCtrl, bool isAlt)
        {
            if (isAlt)
            {
                if (ch >= 'a' && ch <= 'z') return (ushort)((scan & 0xFF) << 8);
                if (scan >= 0x3B && scan <= 0x44) return (ushort)(((scan + 0x2D) & 0xFF) << 8);
                if (scan == 0x01) return KeyCodes.KbAltEsc;
                if (scan == 0x0E) return KeyCodes.KbAltBack;
                return (ushort)((scan << 8) & 0xFF00);
            }

            switch (scan)
            {
                case 0x1C: return KeyCodes.KbEnter;
                case 0x01: return KeyCodes.KbEsc;
                case 0x0E: return isCtrl ? KeyCodes.KbCtrlBack : KeyCodes.KbBack;
                case 0x0F: return KeyCodes.KbTab;
                case 0x48: return KeyCodes.KbUp;
                case 0x50: return KeyCodes.KbDown;
                case 0x4B: return KeyCodes.KbLeft;
                case 0x4D: return KeyCodes.KbRight;
                case 0x47: return KeyCodes.KbHome;
                case 0x4F: return KeyCodes.KbEnd;
                case 0x49: return KeyCodes.KbPgUp;
                case 0x51: return KeyCodes.KbPgDn;
                case 0x52: return KeyCodes.KbIns;
                case 0x53: return KeyCodes.KbDel;
                case 0x3B: case 0x3C: case 0x3D: case 0x3E:
                case 0x3F: case 0x40: case 0x41: case 0x42:
                case 0x43: case 0x44:
                    return (ushort)((scan << 8) & 0xFF00);
            }

            if (isCtrl && ch >= 1 && ch <= 26)
                return (ushort)ch;

            if (ch >= ' ')
                return (ushort)ch;

            if (scan > 0 && scan < ScanForVirtualKey.Length)
                return (ushort)((scan << 8) & 0xFF00);

            return KeyCodes.KbNoKey;
        }

        public override bool GetMouseEvent(out MouseEvtKind kind, out MouseEventType ev)
        {
            kind = MouseEvtKind.None;
            ev = default;
            try
            {
                PumpInput();
                if (_pendingMouse.Count > 0)
                {
                    var pair = _pendingMouse.Dequeue();
                    kind = pair.Key;
                    ev = pair.Value;
                    return true;
                }
            }
            catch (InvalidOperationException) { Platform.MarkInputBroken(); }
            catch (System.IO.IOException) { Platform.MarkInputBroken(); }
            catch { }
            return false;
        }

        public override bool SetClipboardText(string text)
        {
            try
            {
                TClipboard.SetText(text);
                return true;
            }
            catch { return false; }
        }

        public override bool GetClipboardText(out string text)
        {
            text = TClipboard.GetText();
            return true;
        }

        public override void FlushScreen()
        {
            try
            {
                int w = TScreen.ScreenWidth;
                int h = TScreen.ScreenHeight;
                if (w == 0 || h == 0 || TScreen.ScreenBuffer == null) return;

                if (_prevBuffer == null || _prevWidth != w || _prevHeight != h)
                {
                    _prevBuffer = new TScreenCell[w * h];
                    _prevWidth = w;
                    _prevHeight = h;
                }

                var sb = new StringBuilder(w * 40);
                bool cursorMoved = false;

                for (int row = 0; row < h; row++)
                {
                    for (int col = 0; col < w; col++)
                    {
                        int idx = row * w + col;
                        var cell = TScreen.ScreenBuffer[idx];
                        char ch = cell.Ch == '\0' ? ' ' : cell.Ch;

                        if (_prevBuffer[idx].Ch == ch && _prevBuffer[idx].Attr == cell.Attr)
                            continue;

                        if (!cursorMoved)
                        {
                            sb.Append("\x1b[").Append(row + 1).Append(';').Append(col + 1).Append('H');
                            cursorMoved = true;
                        }

                        sb.Append(Platform.ColorToAnsiFg(cell.Attr));
                        sb.Append(Platform.ColorToAnsiBg(cell.Attr));
                        sb.Append(ch);

                        _prevBuffer[idx] = new TScreenCell(ch, cell.Attr);
                    }
                    cursorMoved = false;
                }

                if (sb.Length > 0)
                {
                    sb.Append("\x1b[0m");
                    Console.Write(sb.ToString());
                }
            }
            catch { }
        }

        public override void Invalidate()
        {
            _prevBuffer = null;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool PeekConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer,
            uint nLength, out uint lpNumberOfEventsRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer,
            uint nLength, out uint lpNumberOfEventsRead);
    }

    public class UnixConsoleAdapter : ConsoleAdapter
    {
        private TScreenCell[] _prevBuffer;
        private int _prevWidth;
        private int _prevHeight;

        public override void Init()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.Write("\x1b[?25l");
            }
            catch { }
        }

        public override void Shutdown()
        {
            try
            {
                Console.Write("\x1b[?25h");
                Console.Write("\x1b[0m");
                Console.Write("\x1b[2J\x1b[H");
            }
            catch { }
        }

        public override int GetRows()
        {
            try { return Console.WindowHeight; }
            catch { return 24; }
        }

        public override int GetCols()
        {
            try { return Console.WindowWidth; }
            catch { return 80; }
        }

        public override void SetCursorType(int size, bool visible)
        {
            try
            {
                if (visible) Console.Write("\x1b[?25h");
                else Console.Write("\x1b[?25l");
            }
            catch { }
        }

        public override void SetCursorPos(int x, int y)
        {
            try { Console.Write($"\x1b[{y + 1};{x + 1}H"); }
            catch { }
        }

        public override void WriteCell(int x, int y, TScreenCell cell)
        {
            try
            {
                Console.Write($"\x1b[{y + 1};{x + 1}H");
                Console.Write(Platform.ColorToAnsiFg(cell.Attr));
                Console.Write(Platform.ColorToAnsiBg(cell.Attr));
                Console.Write(cell.Ch == '\0' ? ' ' : cell.Ch);
                Console.Write("\x1b[0m");
            }
            catch { }
        }

        public override void WriteBlock(int x, int y, int w, int h, TScreenCell[] buf)
        {
            try
            {
                var sb = new StringBuilder(w * h * 20);
                for (int row = 0; row < h; row++)
                {
                    sb.Append("\x1b[").Append(y + row + 1).Append(';').Append(x + 1).Append('H');
                    int lastFg = -1, lastBg = -1;
                    for (int col = 0; col < w; col++)
                    {
                        var cell = buf[row * w + col];
                        int fg = (int)cell.Attr.Foreground._data;
                        int bg = (int)cell.Attr.Background._data;
                        if (fg != lastFg || lastFg == -1)
                        {
                            sb.Append(Platform.ColorToAnsiFg(cell.Attr));
                            lastFg = fg;
                        }
                        if (bg != lastBg || lastBg == -1)
                        {
                            sb.Append(Platform.ColorToAnsiBg(cell.Attr));
                            lastBg = bg;
                        }
                        sb.Append(cell.Ch == '\0' ? ' ' : cell.Ch);
                    }
                }
                sb.Append("\x1b[0m");
                Console.Write(sb.ToString());
            }
            catch { }
        }

        public override void FlushScreen()
        {
            try
            {
                int w = TScreen.ScreenWidth;
                int h = TScreen.ScreenHeight;
                if (w == 0 || h == 0 || TScreen.ScreenBuffer == null) return;

                if (_prevBuffer == null || _prevWidth != w || _prevHeight != h)
                {
                    _prevBuffer = new TScreenCell[w * h];
                    _prevWidth = w;
                    _prevHeight = h;
                }

                var sb = new StringBuilder(w * 40);
                bool cursorMoved = false;

                for (int row = 0; row < h; row++)
                {
                    for (int col = 0; col < w; col++)
                    {
                        int idx = row * w + col;
                        var cell = TScreen.ScreenBuffer[idx];
                        char ch = cell.Ch == '\0' ? ' ' : cell.Ch;

                        if (_prevBuffer[idx].Ch == ch && _prevBuffer[idx].Attr == cell.Attr)
                            continue;

                        if (!cursorMoved)
                        {
                            sb.Append("\x1b[").Append(row + 1).Append(';').Append(col + 1).Append('H');
                            cursorMoved = true;
                        }

                        sb.Append(Platform.ColorToAnsiFg(cell.Attr));
                        sb.Append(Platform.ColorToAnsiBg(cell.Attr));
                        sb.Append(ch);

                        _prevBuffer[idx] = new TScreenCell(ch, cell.Attr);
                    }
                    cursorMoved = false;
                }

                if (sb.Length > 0)
                {
                    sb.Append("\x1b[0m");
                    Console.Write(sb.ToString());
                }
            }
            catch { }
        }

        public override bool GetKeyEvent(out ushort keyCode, out ushort controlKeyState, out string text)
        {
            keyCode = 0;
            controlKeyState = 0;
            text = null;
            try
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    keyCode = (ushort)key.KeyChar;
                    if (key.Key == ConsoleKey.Enter) keyCode = KeyCodes.KbEnter;
                    else if (key.Key == ConsoleKey.Escape) keyCode = KeyCodes.KbEsc;
                    else if (key.Key == ConsoleKey.Backspace) keyCode = KeyCodes.KbBack;
                    else if (key.Key == ConsoleKey.Tab) keyCode = KeyCodes.KbTab;
                    else if (key.Key == ConsoleKey.UpArrow) keyCode = KeyCodes.KbUp;
                    else if (key.Key == ConsoleKey.DownArrow) keyCode = KeyCodes.KbDown;
                    else if (key.Key == ConsoleKey.LeftArrow) keyCode = KeyCodes.KbLeft;
                    else if (key.Key == ConsoleKey.RightArrow) keyCode = KeyCodes.KbRight;
                    else if (key.Key == ConsoleKey.Home) keyCode = KeyCodes.KbHome;
                    else if (key.Key == ConsoleKey.End) keyCode = KeyCodes.KbEnd;
                    else if (key.Key == ConsoleKey.PageUp) keyCode = KeyCodes.KbPgUp;
                    else if (key.Key == ConsoleKey.PageDown) keyCode = KeyCodes.KbPgDn;
                    else if (key.Key == ConsoleKey.Insert) keyCode = KeyCodes.KbIns;
                    else if (key.Key == ConsoleKey.Delete) keyCode = KeyCodes.KbDel;
                    else if (key.Key >= ConsoleKey.F1 && key.Key <= ConsoleKey.F12)
                        keyCode = (ushort)(0x3B00 + ((int)key.Key - (int)ConsoleKey.F1) * 0x100);

                    bool isCtrl = (key.Modifiers & ConsoleModifiers.Control) != 0;
                    bool isAlt = (key.Modifiers & ConsoleModifiers.Alt) != 0;

                    int letter = -1;
                    if (key.Key >= ConsoleKey.A && key.Key <= ConsoleKey.Z)
                        letter = key.Key - ConsoleKey.A;

                    if (isCtrl && letter >= 0)
                        keyCode = (ushort)(letter + 1);
                    else if (isAlt && letter >= 0)
                        keyCode = (ushort)(AltScanCodes[letter] << 8);
                    else if (isAlt && key.Key >= ConsoleKey.F1 && key.Key <= ConsoleKey.F10)
                        keyCode += 0x2D00;

                    if (key.Key == ConsoleKey.Tab && (key.Modifiers & ConsoleModifiers.Shift) != 0)
                        keyCode = KeyCodes.KbShiftTab;

                    if (isCtrl) controlKeyState |= KeyCodes.KbCtrlShift;
                    if (isAlt) controlKeyState |= KeyCodes.KbAltShift;
                    if ((key.Modifiers & ConsoleModifiers.Shift) != 0) controlKeyState |= KeyCodes.KbShift;

                    text = key.KeyChar.ToString();
                    return true;
                }
            }
            catch (InvalidOperationException) { Platform.MarkInputBroken(); }
            catch (System.IO.IOException) { Platform.MarkInputBroken(); }
            catch { }
            return false;
        }

        private static readonly int[] AltScanCodes =
        {
            0x1E, 0x30, 0x2E, 0x20, 0x12, 0x21, 0x22, 0x23, 0x17, 0x24, 0x25, 0x26,
            0x32, 0x31, 0x18, 0x19, 0x10, 0x13, 0x1F, 0x14, 0x16, 0x2F, 0x11, 0x2D,
            0x15, 0x2C
        };

        public override bool GetMouseEvent(out MouseEvtKind kind, out MouseEventType ev)
        {
            kind = MouseEvtKind.None;
            ev = default;
            return false;
        }

        public override bool SetClipboardText(string text)
        {
            TClipboard.SetText(text);
            return true;
        }

        public override bool GetClipboardText(out string text)
        {
            text = TClipboard.GetText();
            return true;
        }

        public override void Invalidate()
        {
            _prevBuffer = null;
        }
    }

    public static class Platform
    {
        private static ConsoleAdapter _adapter;

        public static bool InputBroken { get; private set; }

        public static void MarkInputBroken()
        {
            InputBroken = true;
        }

        public static ConsoleAdapter GetAdapter()
        {
            if (_adapter != null) return _adapter;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                _adapter = new Win32ConsoleAdapter();
            else
                _adapter = new UnixConsoleAdapter();

            _adapter.Init();
            return _adapter;
        }

        public static void Init()
        {
            GetAdapter();
        }

        public static void Shutdown()
        {
            _adapter?.Shutdown();
            _adapter = null;
        }

        private static readonly int[] BiosToAnsi = { 0, 4, 2, 6, 1, 5, 3, 7 };

        public static string ColorToAnsiFg(TColorAttr attr)
        {
            var fg = attr.Foreground;
            if (fg.IsDefault) return "\x1b[39m";
            if (fg.IsBIOS)
            {
                var bios = fg.AsBIOS();
                int idx = BiosToAnsi[(int)bios & 0x07];
                bool bright = ((int)bios & 0x08) != 0;
                int code = 30 + idx;
                if (bright) return $"\x1b[{code};1m";
                return $"\x1b[{code}m";
            }
            if (fg.IsRGB)
            {
                var rgb = fg.AsRGB();
                return $"\x1b[38;2;{rgb.Red};{rgb.Green};{rgb.Blue}m";
            }
            if (fg.IsXTerm)
            {
                int idx = fg.AsXTerm();
                return $"\x1b[38;5;{idx}m";
            }
            return "\x1b[39m";
        }

        public static string ColorToAnsiBg(TColorAttr attr)
        {
            var bg = attr.Background;
            if (bg.IsDefault) return "\x1b[49m";
            if (bg.IsBIOS)
            {
                var bios = bg.AsBIOS();
                int idx = BiosToAnsi[(int)bios & 0x07];
                int code = 40 + idx;
                return $"\x1b[{code}m";
            }
            if (bg.IsRGB)
            {
                var rgb = bg.AsRGB();
                return $"\x1b[48;2;{rgb.Red};{rgb.Green};{rgb.Blue}m";
            }
            if (bg.IsXTerm)
            {
                int idx = bg.AsXTerm();
                return $"\x1b[48;5;{idx}m";
            }
            return "\x1b[49m";
        }
    }
}
