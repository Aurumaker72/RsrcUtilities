using System.Numerics;

namespace RsrcUtilities.Geometry.Structs;

/// <summary>
///     Represents a 2-dimentsional vector by its X and Y
/// </summary>
public readonly struct Vector2Int : IEquatable<Vector2Int>
{
    /// <summary>
    ///     A zero-initialized <see cref="Vector2Int" />
    /// </summary>
    public static Vector2Int Zero = new(0, 0);

    /// <summary>
    ///     The x position
    /// </summary>
    public readonly int X;

    /// <summary>
    ///     The y position
    /// </summary>
    public readonly int Y;

    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public Vector2Int(Vector2 vector2)
    {
        X = (int)vector2.X;
        Y = (int)vector2.Y;
    }

    public bool Equals(Vector2Int other)
    {
        return X == other.X && Y == other.Y;
    }

    public static Vector2Int operator +(Vector2Int left, Vector2Int right)
    {
        return new Vector2Int(left.X + right.X, left.Y + right.Y);
    }
}