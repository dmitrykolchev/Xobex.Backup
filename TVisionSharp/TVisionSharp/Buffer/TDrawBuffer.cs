using System;
using System.Text;

namespace TVision
{
    public class TDrawBuffer
    {
        public TScreenCell[] Data;
        public int Capacity;

        public TDrawBuffer()
        {
            Capacity = 256;
            Data = new TScreenCell[Capacity];
        }

        public TDrawBuffer(int capacity)
        {
            Capacity = capacity;
            Data = new TScreenCell[capacity];
        }

        public void Clear()
        {
            for (int i = 0; i < Capacity; i++)
                Data[i] = default;
        }

        public void WriteChar(int pos, char ch, TColorAttr attr, int count = 1)
        {
            for (int i = 0; i < count && pos + i < Capacity; i++)
                Data[pos + i] = new TScreenCell(ch, attr);
        }

        public void WriteStr(int pos, string str, int len, TColorAttr attr)
        {
            int n = Math.Min(len, str.Length);
            for (int i = 0; i < n && pos + i < Capacity; i++)
                Data[pos + i] = new TScreenCell(str[i], attr);
        }

        public void WriteBuf(int pos, int count, TScreenCell[] source, int sourceOffset)
        {
            int n = Math.Min(count, Capacity - pos);
            for (int i = 0; i < n; i++)
                Data[pos + i] = source[sourceOffset + i];
        }

        public void MoveChar(int pos, char ch, TColorAttr attr, int count)
        {
            for (int i = 0; i < count && pos + i < Capacity; i++)
                Data[pos + i] = new TScreenCell(ch, attr);
        }

        public void MoveStr(int pos, string text, int len, TColorAttr attr)
        {
            int n = Math.Min(len, text.Length);
            for (int i = 0; i < n && pos + i < Capacity; i++)
                Data[pos + i] = new TScreenCell(text[i], attr);
        }

        public void MoveCStr(int pos, string text, TAttrPair attrs)
        {
            int src = 0;
            int dst = pos;
            bool tilde = false;
            while (src < text.Length && dst < Capacity)
            {
                if (text[src] == '~')
                {
                    tilde = !tilde;
                    src++;
                    continue;
                }
                Data[dst] = new TScreenCell(text[src], tilde ? attrs.High : attrs.Low);
                dst++;
                src++;
            }
        }

        public void MoveUtf8Str(int pos, ReadOnlySpan<byte> text, int len, TColorAttr attr)
        {
            int consumed = 0;
            int written = 0;
            while (consumed < text.Length && written < len && pos + written < Capacity)
            {
                var ch = TextUtil.DecodeUtf8Char(text, consumed, out int bytesConsumed);
                if (ch == '\0') break;
                Data[pos + written] = new TScreenCell(ch, attr);
                consumed += bytesConsumed;
                written++;
            }
        }

        public void PutAttribute(int pos, TColorAttr attr)
        {
            if (pos >= 0 && pos < Capacity)
                Data[pos].Attr = attr;
        }

        public void MoveChar(int pos, char ch, TAttrPair attrs, int count = 1)
            => MoveChar(pos, ch, attrs.Low, count);

        public void MoveStr(int pos, string text, int len, TAttrPair attrs)
            => MoveStr(pos, text, len, attrs.Low);

        public void WriteChar(int pos, char ch, TAttrPair attrs, int count = 1)
            => WriteChar(pos, ch, attrs.Low, count);

        public void MoveBuf(int pos, char[] source, TAttrPair attrs, int count)
        {
            for (int i = 0; i < count && pos + i < Capacity && i < source.Length; i++)
                Data[pos + i] = new TScreenCell(source[i], attrs.Low);
        }

        public TScreenCell this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }
    }
}
