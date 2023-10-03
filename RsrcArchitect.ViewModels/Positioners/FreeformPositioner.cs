using RsrcCore;
using RsrcCore.Controls;
using RsrcCore.Geometry;

namespace RsrcArchitect.ViewModels.Positioners;

/// <summary>
/// An <see cref="IPositioner"/> which allows transforming the control arbitrarily
/// </summary>
internal class FreeformPositioner : IPositioner
{
    public Rectangle Transform(IEnumerable<TreeNode<Control>> controls, Control targetControl)
    {
        return targetControl.Rectangle;
    }
}