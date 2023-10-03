using System.Drawing;
using RsrcCore;
using RsrcCore.Controls;
using Rectangle = RsrcCore.Geometry.Rectangle;

namespace RsrcArchitect.ViewModels.Positioners;

/// <summary>
/// An <see cref="IPositioner"/> which snaps the control to a grid size
/// </summary>
internal class GridPositioner : IPositioner
{
    public Func<int> SizeFunc { private get; set; } = () => 5;
    
    public Rectangle Transform(IEnumerable<TreeNode<Control>> controls, Control targetControl)
    {
        var size = SizeFunc();
        
        int Snap(int value, float to)
        {
            return (int)(Math.Round(value / to) * to);
        }
        
        return new Rectangle(Snap(targetControl.Rectangle.X, size),
            Snap(targetControl.Rectangle.Y, size),
            Snap(targetControl.Rectangle.Width, size),
            Snap(targetControl.Rectangle.Height, size));
    }
}