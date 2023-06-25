using System.Collections.Generic;
using RsrcCore.Geometry;

namespace RsrcArchitect.Views.WPF.Rendering;

public record Slices(Rectangle TopLeft, Rectangle TopRight, Rectangle BottomLeft, Rectangle BottomRight,
    Rectangle Left, Rectangle Top, Rectangle Right, Rectangle Bottom, Rectangle Center);
