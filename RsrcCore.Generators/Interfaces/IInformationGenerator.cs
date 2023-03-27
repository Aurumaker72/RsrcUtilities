using System.Diagnostics.Contracts;
using RsrcCore.Controls;

namespace RsrcCore.Generators.Interfaces;

/// <summary>
///     Represents a supplementary information generator contract
/// </summary>
/// <remarks>
///     <b>
///         The <see cref="Generate" /> method is intended to be pure, not mutating
///         anything inside the hierarchy
///     </b>
/// </remarks>
public interface IInformationGenerator
{
    /// <summary>
    ///     Generates additional information for the <paramref name="controls" />
    /// </summary>
    /// <param name="controls">The controls to generate a snippet for</param>
    /// <returns>The generated resource header snippet</returns>
    [Pure]
    string Generate(IEnumerable<Control> controls);
}