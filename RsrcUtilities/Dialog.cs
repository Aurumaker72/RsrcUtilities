namespace RsrcUtilities;

/// <summary>
///     Represents a Win32 dialog
/// </summary>
public class Dialog
{
    /// <summary>
    ///     Common styles for window chromes
    /// </summary>
    public enum Chromes
    {
        /// <summary>
        ///     An invalid chrome
        /// </summary>
        Invalid,

        /// <summary>
        ///     The default dialog chrome, consisting of a close button
        /// </summary>
        Default
    }

    /// <summary>
    ///     The unique identifier, preferably in the following format: <c>IDD_YOURNAME</c>
    /// </summary>
    public string Identifier { get; set; } = "IDD_TEST";

    /// <summary>
    ///     The default caption displayed in the titlebar
    /// </summary>
    public string Caption { get; set; } = "My Dialog";

    /// <summary>
    ///     The font size, which cascades down to all non-owner drawn controls
    /// </summary>
    public int FontSize { get; set; } = 8;

    /// <summary>
    ///     The font family, which cascades down to all non-owner drawn controls
    /// </summary>
    public string FontFamily { get; set; } = "MS Shell Dlg";

    /// <summary>
    ///     The width in pixels
    /// </summary>
    public int Width { get; set; } = 300;

    /// <summary>
    ///     The height in pixels
    /// </summary>
    public int Height { get; set; } = 200;

    /// <summary>
    ///     The window chrome
    /// </summary>
    public Chromes Chrome { get; set; } = Chromes.Default;

    /// <summary>
    ///     The list of all child controls
    /// </summary>
    public List<Control> Controls { get; set; } = new();
}