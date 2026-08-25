namespace TVision
{
    public struct TSize
    {
        public int Width;
        public int Height;

        public TSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public static TSize Empty = new TSize(0, 0);
    }
}
