using System;

namespace TVision
{
    public enum ColorType : byte
    {
        Default = 0x0,
        BIOS = 0x1,
        RGB = 0x2,
        XTerm = 0x3,
    }

    public struct TColorRGB : IEquatable<TColorRGB>
    {
        private uint _data;

        public TColorRGB() => _data = 0;

        public TColorRGB(byte r, byte g, byte b)
        {
            _data = (uint)((r << 16) | (g << 8) | b);
        }

        public TColorRGB(uint rgb)
        {
            _data = rgb & 0xFFFFFF;
        }

        public byte Red { get => (byte)(_data >> 16); set => _data = (_data & 0xFF00FFFF) | ((uint)value << 16); }
        public byte Green { get => (byte)(_data >> 8); set => _data = (_data & 0xFFFF00FF) | ((uint)value << 8); }
        public byte Blue { get => (byte)_data; set => _data = (_data & 0xFFFFFF00) | value; }

        public static implicit operator uint(TColorRGB c) => c._data;
        public static implicit operator TColorRGB(uint rgb) => new TColorRGB(rgb);

        public bool Equals(TColorRGB other) => _data == other._data;
        public override bool Equals(object obj) => obj is TColorRGB other && Equals(other);
        public override int GetHashCode() => (int)_data;
    }

    public struct TColorBIOS : IEquatable<TColorBIOS>
    {
        private byte _data;

        public TColorBIOS() => _data = 0;
        public TColorBIOS(byte irgb) => _data = irgb;

        public bool Red { get => (_data & 0x04) != 0; set => _data = (byte)(value ? _data | 0x04 : _data & ~0x04); }
        public bool Green { get => (_data & 0x02) != 0; set => _data = (byte)(value ? _data | 0x02 : _data & ~0x02); }
        public bool Blue { get => (_data & 0x01) != 0; set => _data = (byte)(value ? _data | 0x01 : _data & ~0x01); }
        public bool Intensity { get => (_data & 0x08) != 0; set => _data = (byte)(value ? _data | 0x08 : _data & ~0x08); }

        public static implicit operator byte(TColorBIOS c) => c._data;
        public static implicit operator TColorBIOS(byte irgb) => new TColorBIOS(irgb);

        public bool Equals(TColorBIOS other) => _data == other._data;
        public override bool Equals(object obj) => obj is TColorBIOS other && Equals(other);
        public override int GetHashCode() => _data;
    }

    public struct TColorXTerm : IEquatable<TColorXTerm>
    {
        private byte _data;

        public TColorXTerm() => _data = 0;
        public TColorXTerm(byte idx) => _data = idx;

        public static implicit operator byte(TColorXTerm c) => c._data;
        public static implicit operator TColorXTerm(byte idx) => new TColorXTerm(idx);

        public bool Equals(TColorXTerm other) => _data == other._data;
        public override bool Equals(object obj) => obj is TColorXTerm other && Equals(other);
        public override int GetHashCode() => _data;
    }

    public struct TColorDefault { }

    public struct TColor : IEquatable<TColor>
    {
        internal uint _data;

        public TColor() => _data = 0;

        public TColor(byte bios) => _data = (uint)(bios & 0xF) | ((uint)ColorType.BIOS << 24);
        public TColor(int rgb) => _data = (uint)(rgb & 0xFFFFFF) | ((uint)ColorType.RGB << 24);
        public TColor(TColorBIOS bios) : this((byte)bios) { }
        public TColor(TColorRGB rgb) : this((int)(uint)rgb) { }
        public TColor(TColorXTerm xterm) => _data = xterm | ((uint)ColorType.XTerm << 24);
        public TColor(TColorDefault _) => _data = 0;

        public ColorType Type => (ColorType)(_data >> 24);
        public bool IsDefault => Type == ColorType.Default;
        public bool IsBIOS => Type == ColorType.BIOS;
        public bool IsRGB => Type == ColorType.RGB;
        public bool IsXTerm => Type == ColorType.XTerm;

        public TColorBIOS AsBIOS() => new TColorBIOS((byte)_data);
        public TColorRGB AsRGB() => new TColorRGB(_data);
        public TColorXTerm AsXTerm() => new TColorXTerm((byte)_data);

        public static implicit operator TColor(byte bios) => new TColor(bios);
        public static implicit operator TColor(int rgb) => new TColor(rgb);
        public static implicit operator TColor(TColorDefault def) => new TColor(def);

        public bool Equals(TColor other) => _data == other._data;
        public override bool Equals(object obj) => obj is TColor other && Equals(other);
        public override int GetHashCode() => (int)_data;
        public static bool operator ==(TColor a, TColor b) => a._data == b._data;
        public static bool operator !=(TColor a, TColor b) => a._data != b._data;
    }

    [Flags]
    public enum StyleFlags : ushort
    {
        None = 0x000,
        Bold = 0x001,
        Italic = 0x002,
        Underline = 0x004,
        Blink = 0x008,
        Reverse = 0x010,
        Strike = 0x020,
        WindowShadow = 0x200,
    }

    public struct TColorAttr : IEquatable<TColorAttr>
    {
        private ulong _data;

        private const ulong FgMask = (1UL << 27) - 1;
        private const ulong BgMask = (1UL << 27) - 1;
        private const ulong StyleMask = (1UL << 10) - 1;

        public TColorAttr() => _data = 0;

        public TColorAttr(int bios)
        {
            this = new TColorAttr(new TColor((byte)(bios & 0xF)), new TColor((byte)(bios >> 4)));
        }

        public TColorAttr(TColor fg, TColor bg, ushort style = 0)
        {
            _data = (fg._data & FgMask) | ((bg._data & BgMask) << 27) | ((ulong)style << 54);
        }

        public TColor Foreground
        {
            get
            {
                var c = new TColor();
                c._data = (uint)(_data & FgMask);
                return c;
            }
            set { _data = (_data & ~FgMask) | (value._data & FgMask); }
        }

        public TColor Background
        {
            get
            {
                var c = new TColor();
                c._data = (uint)((_data >> 27) & BgMask);
                return c;
            }
            set { _data = (_data & ~(BgMask << 27)) | ((value._data & BgMask) << 27); }
        }

        public StyleFlags Style
        {
            get => (StyleFlags)(_data >> 54);
            set => _data = (_data & ~(StyleMask << 54)) | ((ulong)(ushort)value << 54);
        }

        public TColorAttr Reversed()
        {
            var fg = Foreground;
            var bg = Background;
            if (fg.IsDefault || bg.IsDefault)
                return new TColorAttr(fg, bg, (ushort)(Style ^ StyleFlags.Reverse));
            return new TColorAttr(bg, fg, (ushort)Style);
        }

        public byte ToBIOS()
        {
            var fg = Foreground;
            var bg = Background;
            if (fg.IsBIOS && bg.IsBIOS && Style == StyleFlags.None)
                return (byte)((byte)fg.AsBIOS() | ((byte)bg.AsBIOS() << 4));
            return 0x5F;
        }

        public static implicit operator TColorAttr(int bios) => new TColorAttr(bios);

        public byte ToBios()
        {
            if (Foreground.IsBIOS && Background.IsBIOS && Style == 0)
            {
                int f = (int)Foreground.AsBIOS() & 0x0F;
                int b = (int)Background.AsBIOS() & 0x0F;
                return (byte)(f | (b << 4));
            }
            return 0;
        }
        public static implicit operator byte(TColorAttr a) => a.ToBIOS();

        public bool Equals(TColorAttr other) => _data == other._data;
        public override bool Equals(object obj) => obj is TColorAttr other && Equals(other);
        public override int GetHashCode() => _data.GetHashCode();
        public static bool operator ==(TColorAttr a, TColorAttr b) => a._data == b._data;
        public static bool operator !=(TColorAttr a, TColorAttr b) => a._data != b._data;
    }

    public struct TAttrPair : IEquatable<TAttrPair>
    {
        private TColorAttr _low;
        private TColorAttr _high;

        public TAttrPair(TColorAttr low = default, TColorAttr high = default)
        {
            _low = low;
            _high = high;
        }

        public TAttrPair(int bios)
        {
            _low = new TColorAttr((byte)(bios & 0xFF));
            _high = new TColorAttr((byte)(bios >> 8));
        }

        public TColorAttr Low { get => _low; set => _low = value; }
        public TColorAttr High { get => _high; set => _high = value; }

        public TColorAttr this[int index]
        {
            get => index == 0 ? _low : _high;
            set { if (index == 0) _low = value; else _high = value; }
        }

        public bool Equals(TAttrPair other) => _low == other._low && _high == other._high;
        public override bool Equals(object obj) => obj is TAttrPair other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_low, _high);
        public static bool operator ==(TAttrPair a, TAttrPair b) => a.Equals(b);
        public static bool operator !=(TAttrPair a, TAttrPair b) => !a.Equals(b);
    }
}
