﻿using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using RsrcUtilities.Controls;
using RsrcUtilities.Serializers.Interfaces;

namespace RsrcUtilities.Serializers.Implementations;

/// <summary>
///     The default resource generator
/// </summary>
public class DefaultResourceGenerator : IResourceGenerator
{
    /// <inheritdoc />
    [Pure]
    public string Generate(TreeNode<Control> root)
    {
        StringBuilder resourceStringBuilder = new();

        var flattened = root.Flatten();

        var identifiers = flattened.Where(x => x is not Panel).Select(x => x.Identifier).ToImmutableList();

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