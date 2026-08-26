// <copyright file="InputToken.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Console;

public readonly struct InputToken
{
    public InputToken(InputTokenType tokenType, byte ch)
    {
        TokenType = tokenType;
        Ch = ch;
    }

    public InputTokenType TokenType { get; }

    public byte Ch { get; }

    public override string ToString()
    {
        return $"{TokenType}:{Ch:x2}";
    }
}
