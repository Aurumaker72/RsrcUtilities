using RsrcUtilities.Controls;
using RsrcUtilities.Geometry.Enums;
using RsrcUtilities.Geometry.Structs;
using RsrcUtilities.Layout.Interfaces;

namespace RsrcUtilities.Layout.Implementations;

/// <inheritdoc/>
public class DefaultLayoutEngine : ILayoutEngine
{
    /// <inheritdoc/>
    public Dictionary<Control, Rectangle> DoLayout(Dialog dialog, TreeNode<Control> root)
    {
        Dictionary<Control, Rectangle> dictionary = new();
        
        foreach (var node in dialog.Root)
        {
            var control = node.Data;
            Thickness parentMargin = node.Parent == null
                ? Thickness.Zero
                : node.GetParents().Aggregate(Thickness.Zero, (current, parent) =>
                {
                    return new Thickness(parent.Data.Margin.Left + current.Left, parent.Data.Margin.Top + current.Top,
                        parent.Data.Margin.Right - current.Right, parent.Data.Margin.Bottom - current.Bottom);
                });

            Thickness finalMargin = control.Margin;
            
            switch (control.HorizontalAlignment)
            {
                case HorizontalAlignments.Stretch:
                    finalMargin = finalMargin.WithLeft(control.Margin.Left + parentMargin.Left);
                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (control.VerticalAlignment)
            {
                case VerticalAlignments.Stretch:
                    finalMargin = finalMargin.WithTop(control.Margin.Top + parentMargin.Top);
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            
            var x = finalMargin.Left;
            var y = finalMargin.Top;
            var width = dialog.Width - (finalMargin.Right + finalMargin.Left);
            var height = dialog.Height - (finalMargin.Bottom + finalMargin.Top);

            dictionary[control] = new Rectangle(x, y, width, height);
        }

        return dictionary;
    }
}