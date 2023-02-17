using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Structs;
using RsrcUtilities.Layout.Interfaces;

namespace RsrcUtilities.Layout.Implementations;

/// <inheritdoc/>
public class DefaultLayoutEngine : ILayoutEngine
{
    /// <inheritdoc/>
    public Dictionary<Control, Rectangle> DoLayout(Dialog dialog, TreeNode<Control> root)
    {
        // TODO: port legacy prototype layout to new structure
        throw new NotImplementedException();
    }
}