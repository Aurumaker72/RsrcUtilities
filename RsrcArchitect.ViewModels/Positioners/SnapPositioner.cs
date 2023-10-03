using RsrcCore;
using RsrcCore.Controls;
using RsrcCore.Geometry;

namespace RsrcArchitect.ViewModels.Positioners;

/// <summary>
/// An <see cref="IPositioner"/> which snaps the control to axes defined by other controls' bounds
/// </summary>
internal class SnapPositioner : IPositioner
{
    public Func<int> ThresholdFunc { private get; set; } = () => 5;

    public Rectangle Transform(IEnumerable<TreeNode<Control>> controls, Control targetControl)
    {
        var threshold = ThresholdFunc();
        
        // TODO: optimize by caching snap candidates
        var hasSnappedX = false;
        var hasSnappedY = false;

        var rectangle = targetControl.Rectangle;
        
        // enumerate all other controls
        foreach (var node in controls.Where(x => !x.Data.Identifier.Equals(targetControl.Identifier)))
        {
            // snap to left
            if (!hasSnappedX && Math.Abs(node.Data.Rectangle.X - targetControl.Rectangle.X) <
                threshold)
            {
                rectangle = rectangle with { X = node.Data.Rectangle.X };
                hasSnappedX = true;
            }

            // snap to internal right
            if (!hasSnappedX && Math.Abs(node.Data.Rectangle.Right - targetControl.Rectangle.Right) <
                threshold)
            {
                rectangle = rectangle with
                {
                    X = node.Data.Rectangle.Right - targetControl.Rectangle.Width
                };
                hasSnappedX = true;
            }

            // snap to external right
            if (!hasSnappedX && Math.Abs(node.Data.Rectangle.Right - targetControl.Rectangle.X) <
                threshold)
            {
                rectangle = rectangle with { X = node.Data.Rectangle.Right };
                hasSnappedX = true;
            }
            
            // snap to top
            if (!hasSnappedY && Math.Abs(node.Data.Rectangle.Y - targetControl.Rectangle.Y) <
                threshold)
            {
                rectangle = rectangle with { Y = node.Data.Rectangle.Y };
                hasSnappedY = true;
            }

            // snap to internal bottom
            if (!hasSnappedY && Math.Abs(node.Data.Rectangle.Bottom - targetControl.Rectangle.Bottom) <
                threshold)
            {
                rectangle = rectangle with
                {
                    Y = node.Data.Rectangle.Bottom - targetControl.Rectangle.Height
                };
                hasSnappedY = true;
            }

            // snap to external bottom
            if (!hasSnappedY && Math.Abs(node.Data.Rectangle.Bottom - targetControl.Rectangle.Y) <
                threshold)
            {
                rectangle = rectangle with { Y = node.Data.Rectangle.Bottom };
                hasSnappedY = true;
            }
        }

        return rectangle;

    }
}