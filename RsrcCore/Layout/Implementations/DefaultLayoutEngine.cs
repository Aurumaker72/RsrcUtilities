using RsrcCore.Controls;
using RsrcCore.Geometry.Enums;
using RsrcCore.Geometry.Structs;
using RsrcCore.Layout.Interfaces;

namespace RsrcCore.Layout.Implementations;

/// <inheritdoc />
public class DefaultLayoutEngine : ILayoutEngine
{
    /// <inheritdoc />
    public Dictionary<Control, Rectangle> DoLayout(Dialog dialog)
    {
        Dictionary<Control, Rectangle> dictionary = new();

        foreach (var node in dialog.Root)
        {
            var control = node.Data;

            if (node.Parent == null || !dictionary.TryGetValue(node.Parent.Data, out var parentRectangle))
                // parent wasn't computed yet, this is logically impossible unless the parent is the dialog
                parentRectangle = new Rectangle(0, 0, dialog.Width, dialog.Height);

            if (node.Parent != null)
                parentRectangle = new Rectangle(parentRectangle.X + node.Parent.Data.Padding.X,
                    parentRectangle.Y + node.Parent.Data.Padding.Y,
                    parentRectangle.Width - node.Parent.Data.Padding.X * 2,
                    parentRectangle.Height - node.Parent.Data.Padding.Y * 2);

            var finalRectangle = control.Rectangle;

            switch (control.HorizontalAlignment)
            {
                case Alignment.Start:
                    finalRectangle = finalRectangle.WithX(parentRectangle.X + control.Rectangle.X);
                    break;
                case Alignment.Fill:
                    finalRectangle = finalRectangle.WithX(parentRectangle.X + control.Rectangle.X);
                    finalRectangle = finalRectangle.WithWidth(parentRectangle.Width - control.Rectangle.X * 2);
                    break;
                case Alignment.End:
                    finalRectangle = finalRectangle.WithX(parentRectangle.Right - control.Rectangle.Right);
                    break;
                case Alignment.Center:
                    finalRectangle =
                        finalRectangle.WithX(
                            parentRectangle.X + parentRectangle.Width / 2 - control.Rectangle.Width / 2);
                    break;
                default:
                    throw new NotImplementedException();
            }

            switch (control.VerticalAlignment)
            {
                case Alignment.Start:
                    finalRectangle = finalRectangle.WithY(parentRectangle.Y + control.Rectangle.Y);
                    break;
                case Alignment.Fill:
                    finalRectangle = finalRectangle.WithY(parentRectangle.Y + control.Rectangle.Y);
                    finalRectangle = finalRectangle.WithHeight(parentRectangle.Height - control.Rectangle.Y * 2);
                    break;
                case Alignment.End:
                    finalRectangle = finalRectangle.WithY(parentRectangle.Bottom - control.Rectangle.Bottom);
                    break;
                case Alignment.Center:
                    finalRectangle = finalRectangle.WithY(parentRectangle.Y + parentRectangle.Height / 2 -
                                                          control.Rectangle.Height / 2);
                    break;
                default:
                    throw new NotImplementedException();
            }

            dictionary[control] = finalRectangle;
        }

        return dictionary;
    }
}