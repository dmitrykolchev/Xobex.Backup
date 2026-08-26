// <copyright file="Escapes.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Console;

public static class Escapes
{
    public static readonly string HideCursor = "\x1b[?25l";
    public static readonly string ShowCursor = "\x1b[?25h";
    public static readonly string DisableWrap = "\x1b[?7l";
    public static readonly string EnableWrap = "\x1b[?7h";
    public static readonly string HomeCursor = "\x1b[H";
    public static readonly string ResetColor = "\x1b[0m";
    public static readonly string BoldText = "\x1b[1m";
    public static readonly string DimText = "\x1b[2m";
    public static readonly string ItalicText = "\x1b[3m";
    public static readonly string UnderlineText = "\x1b[4m";
    public static readonly string BlinkText = "\x1b[5m";
    public static readonly string InversText = "\x1b[7m";
    public static readonly string HiddenText = "\x1b[8m";
    public static readonly string AlternateScreenOn = "\x1b[?1049h";
    public static readonly string AlternateScreenOff = "\x1b[?1049l";
    public static readonly string SaveCursorPosition = "\x1b[s";
    public static readonly string RestoreCursorPosition = "\x1b[u";
}
