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

            Thickness margin = Thickness.Zero;

            switch (control.HorizontalAlignment)
            {
                case HorizontalAlignments.Left:
                    margin = margin.WithLeft(parentMargin.Left);
                    break;
                case HorizontalAlignments.Stretch:
                    margin = margin.WithLeft(parentMargin.Left);
                    margin = margin.WithRight(parentMargin.Right);
                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (control.VerticalAlignment)
            {
                case VerticalAlignments.Top:
                    margin = margin.WithTop(parentMargin.Top);
                    break;
                case VerticalAlignments.Stretch:
                    margin = margin.WithTop(parentMargin.Bottom);
                    margin = margin.WithBottom(parentMargin.Bottom);
                    break;
                default:
                    throw new NotImplementedException();
            }

            control.Margin = margin + control.Margin;
            
            var x = control.Margin.Left;
            var y = control.Margin.Top;
            var width = dialog.Width - (control.Margin.Right + control.Margin.Left);
            var height = dialog.Height - (control.Margin.Bottom + control.Margin.Top);

            dictionary[control] = new Rectangle(x, y, width, height);
        }

        return dictionary;
    }
}