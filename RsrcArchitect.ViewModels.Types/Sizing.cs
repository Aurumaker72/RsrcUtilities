namespace RsrcArchitect.ViewModels.Types;

/// <summary>
/// Modes of rectangle sizing
/// </summary>
/// <param name="Left">The X coordinate and width are being transformed</param>
/// <param name="Top">The Y coordinate and height are being transformed</param>
/// <param name="Right">The width is being transformed</param>
/// <param name="Bottom">The height is being transformed</param>
public readonly record struct Sizing(bool Left, bool Top, bool Right, bool Bottom)
{
    public static Sizing Empty => new Sizing(false, false, false, false);

    public bool IsEmpty => !Left && !Top && !Right && !Bottom;
}