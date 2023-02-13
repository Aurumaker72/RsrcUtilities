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
    ///     The X position in pixels
    /// </summary>
    public int X { get; set; }

    /// <summary>
    ///     The Y position in pixels
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    ///     The width in pixels
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    ///     The height in pixels
    /// </summary>
    public int Height { get; set; }
}