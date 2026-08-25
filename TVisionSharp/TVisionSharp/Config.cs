using System;

namespace TVision
{
    public static class Config
    {
        public const int EventQSize = 16;
        public static readonly int MaxCollectionSize = int.MaxValue / 8 - 16;
        public const int MaxFindStrLen = 80;
        public const int MaxReplaceStrLen = 80;
        public const int MinPasteEventCount = 3;
        public const int MaxCharSize = 4;
        public const int DefaultSafetyPoolSize = 4096;
        public const char Eos = '\0';
    }
}
