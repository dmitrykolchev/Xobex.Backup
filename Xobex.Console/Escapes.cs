// <copyright file="Escapes.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.CompilerServices;

namespace Xobex.Console;

public static class Escapes
{
    public static readonly string HideCursor = "\e[?25l";
    public static readonly string ShowCursor = "\e[?25h";
    public static readonly string DisableWrap = "\e[?7l";
    public static readonly string EnableWrap = "\e[?7h";
    public static readonly string HomeCursor = "\e[H";
    public static readonly string ResetColor = "\e[0m";
    public static readonly string BoldText = "\e[1m";
    public static readonly string DimText = "\e[2m";
    public static readonly string ItalicText = "\e[3m";
    public static readonly string UnderlineText = "\e[4m";
    public static readonly string BlinkText = "\e[5m";
    public static readonly string InversText = "\e[7m";
    public static readonly string HiddenText = "\e[8m";
    public static readonly string AlternateScreenOn = "\e[?1049h";
    public static readonly string AlternateScreenOff = "\e[?1049l";
    public static readonly string SaveCursorPosition = "\e[s";
    public static readonly string RestoreCursorPosition = "\e[u";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SetForeColor(Color color)
    {
        return $"\e[38;2;{color.R};{color.G};{color.B}m";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SetBackColor(Color color)
    {
        return $"\e[48;2;{color.R};{color.G};{color.B}m";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SetColor(TerminalColor bg, TerminalColor fg)
    {
        return $"\e[{(fg > TerminalColor.Gray ? (int)fg + 82 : (int)fg + 30)};{(bg > TerminalColor.Gray ? (int)bg + 92 : (int)bg + 40)}m";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SetForeColor(TerminalColor fg)
    {
        return $"\e[{(fg > TerminalColor.Gray ? (int)fg + 82 : (int)fg + 30)}m";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SetBackColor(TerminalColor bg)
    {
        return $"\e[{(bg > TerminalColor.Gray ? (int)bg + 92 : (int)bg + 40)}m";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SetCursorPosition(int x, int y)
    {
        return $"\e[{y};{x}H";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string MoveCursorUp(int rows)
    {
        return $"\e[{rows}A";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string MoveCursorDown(int rows)
    {
        return $"\e[{rows}B";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string MoveCursorRight(int cols)
    {
        return $"\e[{cols}C";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string MoveCursorLeft(int cols)
    {
        return $"\e[{cols}D";
    }

}
