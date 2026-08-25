namespace TVision
{
    public struct TOutlineEntry
    {
        public string Text;
        public ushort Ref;
    }

    public class TOutline : TObject
    {
        public TCollection Root;
    }

    public class TOutlineViewer : TListViewer
    {
        public TOutlineViewer(TRect bounds, TScrollBar aHScrollBar, TScrollBar aVScrollBar)
            : base(bounds, 1, aHScrollBar, aVScrollBar) { }
    }
}
