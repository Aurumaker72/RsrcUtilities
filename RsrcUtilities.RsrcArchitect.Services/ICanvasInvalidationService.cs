namespace RsrcUtilities.RsrcArchitect.Services;

/// <summary>
/// The default <see langword="interface"/> for a service which implements canvas invalidation
/// </summary>
public interface ICanvasInvalidationService
{
    /// <summary>
    /// Invalidates the canvas
    /// </summary>
    void Invalidate();
}