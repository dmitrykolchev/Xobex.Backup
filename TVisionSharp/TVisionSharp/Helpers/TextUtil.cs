using System;
using System.Text;

namespace TVision
{
    public static class TextUtil
    {
        public static char DecodeUtf8Char(ReadOnlySpan<byte> data, int offset, out int bytesConsumed)
        {
            if (offset >= data.Length) { bytesConsumed = 0; return '\0'; }
            byte b0 = data[offset];
            if (b0 < 0x80) { bytesConsumed = 1; return (char)b0; }
            if ((b0 & 0xE0) == 0xC0)
            {
                if (offset + 1 >= data.Length) { bytesConsumed = 0; return '\0'; }
                bytesConsumed = 2;
                return (char)(((b0 & 0x1F) << 6) | (data[offset + 1] & 0x3F));
            }
            if ((b0 & 0xF0) == 0xE0)
            {
                if (offset + 2 >= data.Length) { bytesConsumed = 0; return '\0'; }
                bytesConsumed = 3;
                return (char)(((b0 & 0x0F) << 12) | ((data[offset + 1] & 0x3F) << 6) | (data[offset + 2] & 0x3F));
            }
            if ((b0 & 0xF8) == 0xF0)
            {
                if (offset + 3 >= data.Length) { bytesConsumed = 0; return '\0'; }
                int cp = ((b0 & 0x07) << 18) | ((data[offset + 1] & 0x3F) << 12) |
                         ((data[offset + 2] & 0x3F) << 6) | (data[offset + 3] & 0x3F);
                bytesConsumed = 4;
                if (cp >= 0x10000)
                {
                    cp -= 0x10000;
                    return '?';
                }
                return (char)cp;
            }
            bytesConsumed = 1;
            return '?';
        }

        public static int GetCharWidth(char ch)
        {
            var category = char.GetUnicodeCategory(ch);
            if (category == System.Globalization.UnicodeCategory.Format ||
                category == System.Globalization.UnicodeCategory.EnclosingMark)
                return 0;
            if (char.IsControl(ch)) return 0;
            return char.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark ? 0 : 1;
        }

        public static string Strnzcpy(string src, int maxLen)
        {
            if (src == null) return string.Empty;
            return src.Length > maxLen ? src.Substring(0, maxLen) : src;
        }
    }
}
