namespace RsrcArchitect.ViewModels.Types;

/// <summary>
/// Modes of control transformations
/// </summary>
public enum PositioningModes
{
    /// <summary>
    /// Translation and scaling is performed with pixel-precision
    /// </summary>
    Freeform,
    
    /// <summary>
    /// Translation and scaling is snapped to the nearest multiple of an arbitrary value
    /// </summary>
    Grid
}