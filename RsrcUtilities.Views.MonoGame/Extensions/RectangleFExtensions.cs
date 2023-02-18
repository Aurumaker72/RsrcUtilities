using MonoGame.Extended;

namespace RsrcUtilities.Views.MonoGame.Extensions;

public static class RectangleFExtensions
{
    public static RectangleF InflateCopy(this RectangleF rectangleF, float x, float y)
    {
        rectangleF.Inflate(x, y);
        return rectangleF;
    }

    public static RectangleF InflateCopy(this RectangleF rectangleF, float v)
    {
        rectangleF.Inflate(v, v);
        return rectangleF;
    }
}