// <copyright file="TextStyle.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Console;

public enum TextStyle : byte
{
    None = 0,
    Bold = 1,
    Dimmed = 2,
    Italic = 3,
    Underline = 4,
    Blink = 5,
    Invers = 6,
    Hidden = 7
}

