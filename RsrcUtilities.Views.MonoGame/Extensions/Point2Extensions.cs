using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace RsrcUtilities.Views.MonoGame.Extensions;

public static class Point2Extensions
{
    public static Vector2 ToVector2(this Point2 point2)
    {
        return new Vector2(point2.X, point2.Y);
    }
}