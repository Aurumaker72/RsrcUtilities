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
}