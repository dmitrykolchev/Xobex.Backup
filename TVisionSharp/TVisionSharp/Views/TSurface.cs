namespace TVision
{
    public class TDrawSurface : TObject
   {
        public TScreenCell[] Data;
        public int SizeX;
        public int SizeY;

        public TDrawSurface(int x, int y)
        {
            SizeX = x;
            SizeY = y;
            Data = new TScreenCell[x * y];
        }

        public void Resize(int x, int y)
        {
            SizeX = x;
            SizeY = y;
            Data = new TScreenCell[x * y];
        }
    }

    public class TSurfaceView : TView
    {
        public TDrawSurface Surface;

        public TSurfaceView(TRect bounds, TDrawSurface aSurface) : base(bounds)
        {
            Surface = aSurface;
        }

        public override void Draw() { }
    }
}
