using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using RsrcUtilities.Controls;
using RsrcUtilities.Generators.Interfaces;

namespace RsrcUtilities.Generators.Implementations;

/// <summary>
///     A resource generator specialized in .h(pp) files
/// </summary>
public class CxxHeaderResourceGenerator : IResourceGenerator
{
    /// <inheritdoc />
    [Pure]
    public string Generate(IEnumerable<Control> controls)
    {
        StringBuilder resourceStringBuilder = new();
        
        var identifiers = controls.Where(x => x is not Panel).Select(x => x.Identifier).ToImmutableList();

        var identifierIndex = 2000;
        if (identifiers.Count != identifiers.Distinct().Count())
            throw new Exception("Non-distinct identifiers in controls list are not allowed");
        foreach (var identifier in identifiers)
        {
            resourceStringBuilder.AppendLine($"#define {identifier} {identifierIndex}");
            identifierIndex++;
        }

        return resourceStringBuilder.ToString();
    }
}