using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace RsrcCore.Geometry;

/// <summary>
///     Represents a 2-dimensional vector with integer coordinates
/// </summary>
public readonly record struct Vector2Int(int X, int Y)
{
    public Vector2Int(Vector2 vector2) : this((int)vector2.X, (int)vector2.Y)
    {
    }
    
    public static Vector2Int operator +(Vector2Int left, Vector2Int right)
    {
        return new Vector2Int(left.X + right.X, left.Y + right.Y);
    }
}