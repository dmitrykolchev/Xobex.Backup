// <copyright file="ConsoleCell.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Console;

public struct ConsoleCell
{
    public ConsoleCell(char ch, Color fore, Color back, TextStyle st = TextStyle.None)
    {
        Ch = ch;
        Fg = fore;
        Bg = back;
        St = st;
    }
    public char Ch;
    public TextStyle St;
    public Color Fg;
    public Color Bg;
}
