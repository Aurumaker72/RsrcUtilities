using RsrcUtilities.Controls.Enums;

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
    ///     The margin from the parent's left in pixels
    /// </summary>
    public int MarginLeft { get; set; }

    /// <summary>
    ///     The margin from the parent's top in pixels
    /// </summary>
    public int MarginTop { get; set; }

    /// <summary>
    ///     The width in pixels
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    ///     The height in pixels
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    ///  The horizontal alignment relative to the parent
    /// </summary>
    public HorizontalAlignments HorizontalAlignment { get; set; } = HorizontalAlignments.Left;
    
    /// <summary>
    ///  The vertical alignment relative to the parent
    /// </summary>
    public VerticalAlignments VerticalAlignment { get; set; } = VerticalAlignments.Top;
    
    /// <summary>
    /// The padding from the inner rectangle's left in pixels
    /// </summary>
    public virtual int RequiredPaddingLeft { get; } = 0;

    /// <summary>
    /// The padding from the inner rectangle's top in pixels
    /// </summary>
    public virtual int RequiredPaddingTop { get; } = 0;
}