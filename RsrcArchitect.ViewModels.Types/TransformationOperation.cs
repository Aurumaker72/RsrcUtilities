namespace RsrcArchitect.ViewModels.Types;

/// <summary>
/// Represents a transformation performed on a control
/// </summary>
public record struct TransformationOperation
{
    public TransformationOperation(Transformation transformation, Sizing sizing)
    {
        Transformation = transformation;
        Sizing = sizing;
    }

    public static TransformationOperation Empty => new();

    public bool IsEmpty => Transformation == Transformation.None && Sizing == Sizing.Empty;
    
    public Transformation Transformation { get; set; } = Transformation.None;
    public Sizing Sizing { get; set; } = Sizing.Empty;
}