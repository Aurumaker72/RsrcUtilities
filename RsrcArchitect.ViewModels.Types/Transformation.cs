namespace RsrcArchitect.ViewModels.Types;

/// <summary>
/// Modes of rectangle transformations
/// </summary>
public enum Transformation
{
    /// <summary>
    /// No transformation
    /// </summary>
    None,
    
    /// <summary>
    /// Scales the rectangle
    /// </summary>
    Size,
    
    /// <summary>
    /// Translates the rectangle
    /// </summary>
    Move,
}