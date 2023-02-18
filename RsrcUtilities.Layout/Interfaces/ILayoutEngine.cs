using System.Diagnostics.Contracts;
using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Structs;

namespace RsrcUtilities.Layout.Interfaces;

/// <summary>
///     Represents a layout engine contract, which lays out and flattens a complex control hierarchy
/// </summary>
public interface ILayoutEngine
{
    /// <summary>
    ///     Performs all layout passes, beginning at the root node
    /// </summary>
    /// <param name="dialog">The dialog containing the root node</param>
    /// <returns>A dictionary of controls and the respective flattened screen rectangle</returns>
    [Pure]
    public Dictionary<Control, Rectangle> DoLayout(Dialog dialog);
}