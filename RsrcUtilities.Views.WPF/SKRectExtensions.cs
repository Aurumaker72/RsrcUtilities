using SkiaSharp;

namespace RsrcUtilities.Views.WinUI;

public static class SkRectExtensions
{
    public static SKRect InflateCopy(this SKRect skRect, float x, float y)
    {
        skRect.Inflate(x, y);
        return skRect;
    }
}