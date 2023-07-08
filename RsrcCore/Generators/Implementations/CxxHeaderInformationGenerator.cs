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
    public string Generate(Dialog dialog)
    {
        StringBuilder resourceStringBuilder = new();

        var identifiers = dialog.Root.Flatten().Where(x => x is not Panel).Select(x => x.Identifier).ToList();

        identifiers.Insert(0, dialog.Identifier);
        
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