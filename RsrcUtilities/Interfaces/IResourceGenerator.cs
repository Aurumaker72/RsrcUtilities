namespace RsrcUtilities.Interfaces;

/// <summary>
///     Represents a resource generator contract
/// </summary>
public interface IResourceGenerator
{
    /// <summary>
    ///     Generates a resource header snippet for the <paramref name="controls" />
    /// </summary>
    /// <param name="controls">The controls to generate a snippet for</param>
    /// <returns>The generated resource header snippet</returns>
    string Generate(TreeNode<Control> root);
}