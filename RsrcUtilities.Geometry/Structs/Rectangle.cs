using System.Numerics;

namespace RsrcUtilities.Geometry.Structs;

/// <summary>
///     Represents a rectangle by its top-left position and dimensions
/// </summary>
public readonly struct Rectangle: IEquatable<Rectangle>, IAdditionOperators<Rectangle, Rectangle, Rectangle>,
    ISubtractionOperators<Rectangle, Rectangle, Rectangle>
{
    
    /// <summary>
    /// The X position in pixels
    /// </summary>
    public readonly int X;
    
    /// <summary>
    /// The Y position in pixels
    /// </summary>
    public readonly int Y;
    
    /// <summary>
    /// The width in pixels
    /// </summary>
    public readonly int Width;
    
    /// <summary>
    /// The height in pixels
    /// </summary>
    public readonly int Height;

    /// <summary>
    /// The right side in pixels
    /// </summary>
    public int Right => X + Width;
    
    /// <summary>
    /// The bottom side in pixels
    /// </summary>
    public int Bottom => Y + Height;
    
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Equals(Rectangle other)
    {
        return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    }

    public static Rectangle operator +(Rectangle left, Rectangle right)
    {
        return new Rectangle(left.X + right.X, left.Y + right.Y, left.Width + right.Width, left.Height + right.Height);
    }

    public static Rectangle operator -(Rectangle left, Rectangle right)
    {
        return new Rectangle(left.X - right.X, left.Y - right.Y, left.Width - right.Width, left.Height - right.Height);
    }
}