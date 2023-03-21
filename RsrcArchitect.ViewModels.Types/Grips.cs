namespace RsrcArchitect.ViewModels.Types;

/// <summary>
/// Modes of control rectangle transformations
/// </summary>
public enum Grips
{
    /// <summary>
    /// Translates the Left, Top coordinate
    /// </summary>
    Move,
    
    /// <summary>
    /// Translates the Left coordinate, while keeping the Right coordinate
    /// </summary>
    Left,
    
    /// <summary>
    /// Translate the Top coordinate, while keeping the Bottom coordinate
    /// </summary>
    Top,
    
    /// <summary>
    /// Translate the Right coordinate, while keeping the Left coordinate
    /// </summary>
    Right,
    
    /// <summary>
    /// Translate the Bottom coordinate, while keeping the Top coordinate
    /// </summary>
    Bottom,
    
    /// <summary>
    /// Translate the Top, Left coordinate, while keeping the Right, Bottom coordinate
    /// </summary>
    TopLeft,
    
    /// <summary>
    /// Translate the Top, Right coordinate, while keeping the Left, Bottom coordinate
    /// </summary>
    TopRight,
    
    /// <summary>
    /// Translate the Bottom, Left coordinate, while keeping the Top, Right coordinate
    /// </summary>
    BottomLeft,
    
    /// <summary>
    /// Translate the Bottom, Right coordinate, while keeping the Top, Left coordinate
    /// </summary>
    BottomRight
}