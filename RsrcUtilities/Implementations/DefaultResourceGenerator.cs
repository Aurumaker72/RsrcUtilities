using System.Collections.Immutable;
using System.Text;
using RsrcUtilities.Interfaces;

namespace RsrcUtilities.Implementations;

/// <summary>
///     The default resource generator
/// </summary>
public class DefaultResourceGenerator : IResourceGenerator
{
    /// <inheritdoc />
    public string Generate(List<Control> controls)
    {
        StringBuilder resourceStringBuilder = new();
        var identifiers = controls.Select(x => x.Identifier).ToImmutableList();
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