using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using RsrcCore.Controls;
using RsrcCore.Generators.Interfaces;

namespace RsrcCore.Generators.Implementations;

/// <summary>
///     A resource generator specialized in .h(pp) files
/// </summary>
public class CxxHeaderInformationGenerator : IInformationGenerator
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