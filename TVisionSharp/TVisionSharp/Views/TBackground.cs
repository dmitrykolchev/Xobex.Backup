namespace TVision
{
    public class TBackground : TView
    {
        public string Pattern;

        public TBackground(TRect bounds, string aPattern) : base(bounds)
        {
            Pattern = string.IsNullOrEmpty(aPattern) ? "\u2591" : aPattern;
            GrowMode = Commands.GfGrowLoX | Commands.GfGrowLoY;
        }

        public override void Draw()
        {
            var attr = MapColor(1);
            if (attr == default)
                attr = new TColorAttr(0x17);

            var b = new TDrawBuffer(Size.X);
            for (int y = 0; y < Size.Y; y++)
            {
                for (int x = 0; x < Size.X; x++)
                {
                    char ch = Pattern.Length > 0 ? Pattern[(y * Size.X + x) % Pattern.Length] : ' ';
                    b.WriteChar(x, ch, attr);
                }
                WriteLine((short)0, (short)y, (short)Size.X, (short)1, b);
            }
        }

        public override TPalette GetPalette()
        {
            return new TPalette("\x01", 1);
        }
    }
}
