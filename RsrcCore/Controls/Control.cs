using RsrcCore.Geometry.Enums;
using RsrcCore.Geometry.Structs;

namespace RsrcCore.Controls;

/// <summary>
///     Represents a base for visual elements inside a dialog
/// </summary>
public abstract class Control
{
    /// <summary>
    ///     The internal padding, which children are equilateraly offset by
    /// </summary>
    public Vector2Int Padding;

    /// <summary>
    ///     The logical rectangle, relative in (X, Y) to the parent
    /// </summary>
    public Rectangle Rectangle;

    /// <summary>
    ///     The unique identifier
    /// </summary>
    public string Identifier { get; set; } = "IDC_UNKNOWN";

    /// <summary>
    ///     The horizontal alignment relative to the parent
    /// </summary>
    public Alignment HorizontalAlignment { get; set; } = Alignment.Start;

    /// <summary>
    ///     The vertical alignment relative to the parent
    /// </summary>
    public Alignment VerticalAlignment { get; set; } = Alignment.Start;
}