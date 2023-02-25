namespace RsrcUtilities.Geometry.Structs;

/// <summary>
///     Represents the thickness of a frame around a rectangle
/// </summary>
public readonly struct Thickness : IEquatable<Thickness>
{
    /// <summary>
    ///     A zero-initialized <see cref="Thickness" />
    /// </summary>
    public static Thickness Zero = new(0, 0, 0, 0);

    /// <summary>
    ///     The margin from the rectangle's left side
    /// </summary>
    public readonly int Left;

    /// <summary>
    ///     The margin from the rectangle's top side
    /// </summary>
    public readonly int Top;

    /// <summary>
    ///     The margin from the rectangle's right side
    /// </summary>
    public readonly int Right;

    /// <summary>
    ///     The margin from the rectangle's bottom side
    /// </summary>
    public readonly int Bottom;

    public Thickness(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public bool Equals(Thickness other)
    {
        return Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
    }

    public static Thickness operator +(Thickness left, Thickness right)
    {
        return new Thickness(left.Left + right.Left, left.Top + right.Top, left.Right + right.Right,
            left.Bottom + right.Bottom);
    }

    public static Thickness operator -(Thickness left, Thickness right)
    {
        return new Thickness(left.Left - right.Left, left.Top - right.Top, left.Right - right.Right,
            left.Bottom - right.Bottom);
    }

    public Thickness WithLeft(int left)
    {
        return new Thickness(left, Top, Right, Bottom);
    }

    public Thickness WithTop(int top)
    {
        return new Thickness(Left, top, Right, Bottom);
    }

    public Thickness WithRight(int right)
    {
        return new Thickness(Left, Top, right, Bottom);
    }

    public Thickness WithBottom(int bottom)
    {
        return new Thickness(Left, Top, Right, bottom);
    }
}