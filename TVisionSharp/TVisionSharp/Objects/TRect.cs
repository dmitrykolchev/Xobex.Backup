using System;

namespace TVision
{
    public struct TRect : IEquatable<TRect>
    {
        public TPoint A;
        public TPoint B;

        public TRect(int ax, int ay, int bx, int by)
        {
            A = new TPoint(ax, ay);
            B = new TPoint(bx, by);
        }

        public TRect(TPoint p1, TPoint p2)
        {
            A = p1;
            B = p2;
        }

        public TRect()
        {
            A = TPoint.Empty;
            B = TPoint.Empty;
        }

        public int Left => A.X;
        public int Top => A.Y;
        public int Right => B.X;
        public int Bottom => B.Y;
        public int Width => B.X - A.X;
        public int Height => B.Y - A.Y;
        public TPoint Origin => A;
        public TSize Size => new TSize(Width, Height);

        public TRect Move(int dx, int dy)
        {
            return new TRect(A.X + dx, A.Y + dy, B.X + dx, B.Y + dy);
        }

        public TRect Grow(int dx, int dy)
        {
            return new TRect(A.X - dx, A.Y - dy, B.X + dx, B.Y + dy);
        }

        public TRect Intersect(TRect r)
        {
            return new TRect(
                Math.Max(A.X, r.A.X),
                Math.Max(A.Y, r.A.Y),
                Math.Min(B.X, r.B.X),
                Math.Min(B.Y, r.B.Y));
        }

        public TRect Union(TRect r)
        {
            return new TRect(
                Math.Min(A.X, r.A.X),
                Math.Min(A.Y, r.A.Y),
                Math.Max(B.X, r.B.X),
                Math.Max(B.Y, r.B.Y));
        }

        public bool Contains(TPoint p)
        {
            return p.X >= A.X && p.X < B.X && p.Y >= A.Y && p.Y < B.Y;
        }

        public bool IsEmpty()
        {
            return A.X >= B.X || A.Y >= B.Y;
        }

        public static bool operator ==(TRect a, TRect b) => a.A == b.A && a.B == b.B;
        public static bool operator !=(TRect a, TRect b) => !(a == b);
        public bool Equals(TRect other) => A == other.A && B == other.B;
        public override bool Equals(object obj) => obj is TRect other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(A, B);
        public override string ToString() => $"({A.X},{A.Y},{B.X},{B.Y})";

        public static readonly TRect Empty = new TRect();
    }
}
