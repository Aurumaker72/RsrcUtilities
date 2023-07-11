using System.Numerics;

namespace RsrcCore.Geometry;

/// <summary>
///     Represents a rectangle by its position and dimensions
/// </summary>
public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
    public static Rectangle Empty => new(0, 0, 0, 0);

    public int Right => X + Width;
    public int Bottom => Y + Height;
    public int CenterX => X + Width / 2;
    public int CenterY => Y + Height / 2;

    public bool Contains(Vector2Int vector2Int)
    {
        return vector2Int.X > X && vector2Int.Y > Y && vector2Int.X < Right && vector2Int.Y < Bottom;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }
    
    public static Rectangle operator +(Rectangle left, Rectangle right)
    {
        return new Rectangle(left.X + right.X, left.Y + right.Y, left.Width + right.Width, left.Height + right.Height);
    }

    public static Rectangle operator -(Rectangle left, Rectangle right)
    {
        return new Rectangle(left.X - right.X, left.Y - right.Y, left.Width - right.Width, left.Height - right.Height);
    }

    public Rectangle Inflate(int value)
    {
        return new Rectangle(X - value, Y - value, Width + value * 2, Height + value * 2);
    }

}