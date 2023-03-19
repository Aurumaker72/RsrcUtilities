using System.Numerics;

namespace RsrcUtilities.Geometry.Structs;

/// <summary>
///     Represents a rectangle by its top-left position and dimensions
/// </summary>
public readonly struct Rectangle
{
    /// <summary>
    ///     A zero-initialized <see cref="Rectangle" />
    /// </summary>
    public static Rectangle Zero = new(0, 0, 0, 0);

    /// <summary>
    ///     The X position
    /// </summary>
    public readonly int X;

    /// <summary>
    ///     The Y position
    /// </summary>
    public readonly int Y;

    /// <summary>
    ///     The width
    /// </summary>
    public readonly int Width;

    /// <summary>
    ///     The height
    /// </summary>
    public readonly int Height;

    /// <summary>
    ///     The right side
    /// </summary>
    public int Right => X + Width;

    /// <summary>
    ///     The bottom side
    /// </summary>
    public int Bottom => Y + Height;

    /// <summary>
    ///     The horizontal center 
    /// </summary>
    public int CenterX => X + Width / 2;
    
    /// <summary>
    ///     The vertical center 
    /// </summary>
    public int CenterY => Y + Height / 2;
    
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static Rectangle operator +(Rectangle left, Rectangle right)
    {
        return new Rectangle(left.X + right.X, left.Y + right.Y, left.Width + right.Width, left.Height + right.Height);
    }

    public static Rectangle operator -(Rectangle left, Rectangle right)
    {
        return new Rectangle(left.X - right.X, left.Y - right.Y, left.Width - right.Width, left.Height - right.Height);
    }

    public Rectangle WithX(int x)
    {
        return new Rectangle(x, Y, Width, Height);
    }

    public Rectangle WithY(int y)
    {
        return new Rectangle(X, y, Width, Height);
    }

    public Rectangle WithWidth(int width)
    {
        return new Rectangle(X, Y, width, Height);
    }

    public Rectangle WithHeight(int height)
    {
        return new Rectangle(X, Y, Width, height);
    }

    public bool Contains(Vector2Int vector2Int)
    {
        return vector2Int.X > X && vector2Int.X < Right && vector2Int.Y > Y && vector2Int.Y < Bottom;
    }

    public override bool Equals(object obj)
    {
        return obj is Rectangle rectangle && GetHashCode() == rectangle.GetHashCode();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }
}