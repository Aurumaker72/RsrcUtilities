using System.Numerics;
using RsrcArchitect.ViewModels.Types;
using RsrcCore.Controls;
using RsrcCore.Geometry;

namespace RsrcArchitect.ViewModels.Helpers;

internal static class TransformationHelper
{
    public static TransformationOperation GetCandidate(Control control, Vector2 position, int gripSize)
    {
        var relative = position - control.Rectangle.ToVector2();

        var transformation = Transformation.Size;
        var sizing = Sizing.Empty;

        sizing = sizing with { Left = Math.Abs(relative.X - 0) < gripSize };
        sizing = sizing with { Top = Math.Abs(relative.Y - 0) < gripSize };
        sizing = sizing with
        {
            Right = Math.Abs(relative.X - control.Rectangle.Width) < gripSize
        };
        sizing = sizing with
        {
            Bottom = Math.Abs(relative.Y - control.Rectangle.Height) < gripSize
        };

        if (sizing.IsEmpty) transformation = Transformation.Move;

        if (!control.Rectangle.Inflate(gripSize).Contains(new Vector2Int(position)))
            transformation = Transformation.None;

        return new TransformationOperation(transformation, sizing);
    }
}