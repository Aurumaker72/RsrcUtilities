using RsrcUtilities.Controls.Enums;
using RsrcUtilities.Controls.Layout;

namespace RsrcUtilities;

/// <summary>
///     Represents a base for visual elements inside a dialog
/// </summary>
public abstract class Control
{
    /// <summary>
    ///     The unique identifier
    /// </summary>
    public string Identifier { get; set; } = "IDC_UNKNOWN";

    /// <summary>
    ///     The margin relative to the parent frame
    /// </summary>
    public Thickness Margin;

    /// <summary>
    ///     The horizontal alignment relative to the parent
    /// </summary>
    public HorizontalAlignments HorizontalAlignment { get; set; } = HorizontalAlignments.Left;

    /// <summary>
    ///     The vertical alignment relative to the parent
    /// </summary>
    public VerticalAlignments VerticalAlignment { get; set; } = VerticalAlignments.Top;
}