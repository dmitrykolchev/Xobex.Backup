// <copyright file="Color.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Xobex.Console;

public readonly struct Color : IEquatable<Color>
{
    public Color(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public byte R { get; }

    public byte G { get; }

    public byte B { get; }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }

    public bool Equals(Color other)
    {
        return R == other.R && G == other.G && B == other.B;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is Color other)
        {
            return Equals(other);
        }
        return false;
    }
}
