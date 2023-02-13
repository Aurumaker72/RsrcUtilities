namespace RsrcUtilities.Interfaces;

/// <summary>
///     Represents a resource generator contract
/// </summary>
public interface IResourceGenerator
{
    /// <summary>
    ///     Tries to generate a resource.h snippet for the <paramref name="controls" />
    /// </summary>
    /// <param name="controls">The controls to generate a snippet for</param>
    /// <param name="generated">The generated snippet</param>
    /// <returns><see langword="true" />, if succeeded</returns>
    bool TryGenerate(List<Control> controls, out string? generated);
}