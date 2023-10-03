using RsrcCore;
using RsrcCore.Controls;
using RsrcCore.Geometry;

namespace RsrcArchitect.ViewModels.Positioners;

/// <summary>
///     The default <see langword="interface" /> for a control positioner, which is responsible for transforming control positions (e.g.: grid snapping)
/// </summary>
internal interface IPositioner
{
    Rectangle Transform(IEnumerable<TreeNode<Control>> controls, Control targetControl);
}