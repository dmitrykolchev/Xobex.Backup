using System;

namespace TVision
{
    public static class KeyCodes
    {
        public const ushort KbCtrlA = 0x0001, KbCtrlB = 0x0002, KbCtrlC = 0x0003;
        public const ushort KbCtrlD = 0x0004, KbCtrlE = 0x0005, KbCtrlF = 0x0006;
        public const ushort KbCtrlG = 0x0007, KbCtrlH = 0x0008, KbCtrlI = 0x0009;
        public const ushort KbCtrlJ = 0x000a, KbCtrlK = 0x000b, KbCtrlL = 0x000c;
        public const ushort KbCtrlM = 0x000d, KbCtrlN = 0x000e, KbCtrlO = 0x000f;
        public const ushort KbCtrlP = 0x0010, KbCtrlQ = 0x0011, KbCtrlR = 0x0012;
        public const ushort KbCtrlS = 0x0013, KbCtrlT = 0x0014, KbCtrlU = 0x0015;
        public const ushort KbCtrlV = 0x0016, KbCtrlW = 0x0017, KbCtrlX = 0x0018;
        public const ushort KbCtrlY = 0x0019, KbCtrlZ = 0x001a;

        public const ushort KbEsc = 0x011b, KbAltSpace = 0x0200, KbCtrlIns = 0x0400;
        public const ushort KbShiftIns = 0x0500, KbCtrlDel = 0x0600, KbShiftDel = 0x0700;
        public const ushort KbBack = 0x0e08, KbCtrlBack = 0x0e7f, KbShiftTab = 0x0f00;
        public const ushort KbTab = 0x0f09, KbAltQ = 0x1000, KbAltW = 0x1100;
        public const ushort KbAltE = 0x1200, KbAltR = 0x1300, KbAltT = 0x1400;
        public const ushort KbAltY = 0x1500, KbAltU = 0x1600, KbAltI = 0x1700;
        public const ushort KbAltO = 0x1800, KbAltP = 0x1900, KbCtrlEnter = 0x1c0a;
        public const ushort KbEnter = 0x1c0d, KbAltA = 0x1e00, KbAltS = 0x1f00;
        public const ushort KbAltD = 0x2000, KbAltF = 0x2100, KbAltG = 0x2200;
        public const ushort KbAltH = 0x2300, KbAltJ = 0x2400, KbAltK = 0x2500;
        public const ushort KbAltL = 0x2600, KbAltZ = 0x2c00, KbAltX = 0x2d00;
        public const ushort KbAltC = 0x2e00, KbAltV = 0x2f00, KbAltB = 0x3000;
        public const ushort KbAltN = 0x3100, KbAltM = 0x3200;
        public const ushort KbF1 = 0x3b00, KbF2 = 0x3c00, KbF3 = 0x3d00;
        public const ushort KbF4 = 0x3e00, KbF5 = 0x3f00, KbF6 = 0x4000;
        public const ushort KbF7 = 0x4100, KbF8 = 0x4200, KbF9 = 0x4300;
        public const ushort KbF10 = 0x4400;
        public const ushort KbHome = 0x4700, KbUp = 0x4800, KbPgUp = 0x4900;
        public const ushort KbGrayMinus = 0x4a2d, KbLeft = 0x4b00, KbRight = 0x4d00;
        public const ushort KbGrayPlus = 0x4e2b, KbEnd = 0x4f00, KbDown = 0x5000;
        public const ushort KbPgDn = 0x5100, KbIns = 0x5200, KbDel = 0x5300;
        public const ushort KbShiftF1 = 0x5400, KbShiftF2 = 0x5500, KbShiftF3 = 0x5600;
        public const ushort KbShiftF4 = 0x5700, KbShiftF5 = 0x5800, KbShiftF6 = 0x5900;
        public const ushort KbShiftF7 = 0x5a00, KbShiftF8 = 0x5b00, KbShiftF9 = 0x5c00;
        public const ushort KbShiftF10 = 0x5d00;
        public const ushort KbCtrlF1 = 0x5e00, KbCtrlF2 = 0x5f00;
        public const ushort KbCtrlF3 = 0x6000, KbCtrlF4 = 0x6100, KbCtrlF5 = 0x6200;
        public const ushort KbCtrlF6 = 0x6300, KbCtrlF7 = 0x6400, KbCtrlF8 = 0x6500;
        public const ushort KbCtrlF9 = 0x6600, KbCtrlF10 = 0x6700;
        public const ushort KbAltF1 = 0x6800, KbAltF2 = 0x6900, KbAltF3 = 0x6a00;
        public const ushort KbAltF4 = 0x6b00, KbAltF5 = 0x6c00, KbAltF6 = 0x6d00;
        public const ushort KbAltF7 = 0x6e00, KbAltF8 = 0x6f00, KbAltF9 = 0x7000;
        public const ushort KbAltF10 = 0x7100;
        public const ushort KbCtrlPrtSc = 0x7200, KbCtrlLeft = 0x7300, KbCtrlRight = 0x7400;
        public const ushort KbCtrlEnd = 0x7500, KbCtrlPgDn = 0x7600, KbCtrlHome = 0x7700;
        public const ushort KbAlt1 = 0x7800, KbAlt2 = 0x7900, KbAlt3 = 0x7a00;
        public const ushort KbAlt4 = 0x7b00, KbAlt5 = 0x7c00, KbAlt6 = 0x7d00;
        public const ushort KbAlt7 = 0x7e00, KbAlt8 = 0x7f00, KbAlt9 = 0x8000;
        public const ushort KbAlt0 = 0x8100, KbAltMinus = 0x8200, KbAltEqual = 0x8300;
        public const ushort KbCtrlPgUp = 0x8400, KbNoKey = 0x0000;
        public const ushort KbAltEsc = 0x0100, KbAltBack = 0x0e00;
        public const ushort KbF11 = 0x8500, KbF12 = 0x8600;
        public const ushort KbShiftF11 = 0x8700, KbShiftF12 = 0x8800;
        public const ushort KbCtrlF11 = 0x8900, KbCtrlF12 = 0x8a00;
        public const ushort KbAltF11 = 0x8b00, KbAltF12 = 0x8c00;
        public const ushort KbCtrlUp = 0x8d00, KbCtrlDown = 0x9100;
        public const ushort KbCtrlTab = 0x9400;
        public const ushort KbAltHome = 0x9700, KbAltUp = 0x9800;
        public const ushort KbAltPgUp = 0x9900, KbAltLeft = 0x9b00;
        public const ushort KbAltRight = 0x9d00, KbAltEnd = 0x9f00;
        public const ushort KbAltDown = 0xa000, KbAltPgDn = 0xa100;
        public const ushort KbAltIns = 0xa200, KbAltDel = 0xa300;
        public const ushort KbAltTab = 0xa500, KbAltEnter = 0xa600;

        public const ushort KbLeftShift = 0x0001;
        public const ushort KbRightShift = 0x0002;
        public const ushort KbShift = KbLeftShift | KbRightShift;
        public const ushort KbLeftCtrl = 0x0004;
        public const ushort KbRightCtrl = 0x0004;
        public const ushort KbCtrlShift = KbLeftCtrl | KbRightCtrl;
        public const ushort KbLeftAlt = 0x0008;
        public const ushort KbRightAlt = 0x0008;
        public const ushort KbAltShift = KbLeftAlt | KbRightAlt;
        public const ushort KbScrollState = 0x0010;
        public const ushort KbNumState = 0x0020;
        public const ushort KbCapsState = 0x0040;
        public const ushort KbEnhanced = 0x0100;
        public const ushort KbInsState = 0x0200;
        public const ushort KbPaste = 0x0400;
    }

    public struct TKey : IEquatable<TKey>
    {
        public ushort Code;
        public ushort Mods;

        public TKey() { Code = 0; Mods = 0; }

        public TKey(ushort keyCode, ushort shiftState = 0)
        {
            Code = keyCode;
            Mods = shiftState;
        }

        public static bool operator ==(TKey a, TKey b) =>
            (a.Code | ((uint)a.Mods << 16)) == (b.Code | ((uint)b.Mods << 16));

        public static bool operator !=(TKey a, TKey b) => !(a == b);
        public bool Equals(TKey other) => this == other;
        public override bool Equals(object obj) => obj is TKey other && Equals(other);
        public override int GetHashCode() => Code | (Mods << 16);
    }

    public struct CharScanType
    {
        public byte CharCode;
        public byte ScanCode;
    }

    public struct KeyDownEvent
    {
        public ushort KeyCode;
        public ushort ControlKeyState;
        public byte TextLength;
        public char Char0, Char1, Char2, Char3;

        public CharScanType CharScan
        {
            get
            {
                return new CharScanType
                {
                    CharCode = (byte)(KeyCode & 0xFF),
                    ScanCode = (byte)((KeyCode >> 8) & 0xFF)
                };
            }
        }

        public string GetText()
        {
            if (TextLength == 0) return string.Empty;
            return new string(new[] { Char0, Char1, Char2, Char3 }, 0, TextLength);
        }

        public TKey ToKey() => new TKey(KeyCode, ControlKeyState);
    }

    public struct MessageEvent
    {
        public ushort Command;
        public long InfoLong;
        public object InfoPtr;
    }

    public struct MouseEventType
    {
        public TPoint Where;
        public ushort EventFlags;
        public ushort ControlKeyState;
        public byte Buttons;
        public byte Wheel;
    }

    public struct TEvent
    {
        public int What;
        public MouseEventType Mouse;
        public KeyDownEvent KeyDown;
        public MessageEvent Message;

        public void GetMouseEvent()
        {
            TEventQueue.GetMouseEvent(ref this);
        }

        public void GetKeyEvent()
        {
            TEventQueue.GetKeyEvent(ref this);
        }
    }
}
