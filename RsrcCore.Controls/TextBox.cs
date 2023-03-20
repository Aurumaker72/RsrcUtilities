namespace RsrcCore.Controls;

/// <summary>
///     Represents a user-input accepting textbox
/// </summary>
public class TextBox : Control
{
    /// <summary>
    ///     Filter flags which are applied to user input
    /// </summary>
    [Flags]
    public enum InputFilters
    {
        /// <summary>
        ///     Only numbers can be inputted
        /// </summary>
        Numbers = 1 << 0
    }

    /// <summary>
    ///     Whether user interactions are accepted
    /// </summary>
    public bool IsWriteable { get; set; } = true;

    /// <summary>
    ///     Whether horizontal scrolling is allowed
    /// </summary>
    public bool AllowHorizontalScroll { get; set; }

    /// <summary>
    ///     The applied input filter
    /// </summary>
    public InputFilters InputFilter { get; set; } = 0;
}