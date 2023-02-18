using RsrcUtilities.Geometry.Enums;
using RsrcUtilities.Geometry.Structs;

namespace RsrcUtilities.Controls;

/// <summary>
///     Represents a base for visual elements inside a dialog
/// </summary>
public abstract class Control
{
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
    public HorizontalAlignments HorizontalAlignment { get; set; } = HorizontalAlignments.Left;

    /// <summary>
    ///     The vertical alignment relative to the parent
    /// </summary>
    public VerticalAlignments VerticalAlignment { get; set; } = VerticalAlignments.Top;
}