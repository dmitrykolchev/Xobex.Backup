using System;

namespace TVision
{
    public struct TPoint : IEquatable<TPoint>
    {
        public int X;
        public int Y;

        public TPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static TPoint operator +(TPoint a, TPoint b) => new TPoint(a.X + b.X, a.Y + b.Y);
        public static TPoint operator -(TPoint a, TPoint b) => new TPoint(a.X - b.X, a.Y - b.Y);
        public static TPoint operator +(TPoint a, TSize b) => new TPoint(a.X + b.Width, a.Y + b.Height);
        public static TPoint operator -(TPoint a, TSize b) => new TPoint(a.X - b.Width, a.Y - b.Height);

        public static bool operator ==(TPoint a, TPoint b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(TPoint a, TPoint b) => !(a == b);

        public bool Equals(TPoint other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is TPoint other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X},{Y})";

        public static readonly TPoint Empty = new TPoint(0, 0);
    }
}
