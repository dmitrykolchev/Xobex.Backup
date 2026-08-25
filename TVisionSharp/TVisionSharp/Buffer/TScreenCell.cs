namespace TVision
{
    public struct TScreenCell
    {
        public char Ch;
        public TColorAttr Attr;

        public TScreenCell(char ch, TColorAttr attr)
        {
            Ch = ch;
            Attr = attr;
        }
    }
}
