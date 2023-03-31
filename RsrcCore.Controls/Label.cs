namespace RsrcCore.Controls;

/// <summary>
/// Represents a text control
/// </summary>
public class Label : Control
{
    /// <summary>
    /// The default text displayed in the label
    /// </summary>
    public string Caption { get; set; } = "Hello World!";
}