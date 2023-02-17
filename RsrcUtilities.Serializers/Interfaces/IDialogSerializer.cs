using System.Diagnostics.Contracts;
using RsrcUtilities.Controls;
using RsrcUtilities.Layout.Interfaces;

namespace RsrcUtilities.Serializers.Interfaces;

/// <summary>
///     Represents a dialog serializer contract
/// </summary>
/// <remarks>
///     <b>
///         The <see cref="Serialize" /> method is intended to be pure, not mutating
///         anything inside the hierarchy
///     </b>
/// </remarks>
public interface IDialogSerializer
{
    /// <summary>
    ///     Serializes the dialog into an .rc snippet
    /// </summary>
    /// <param name="layoutEngine">The layout engine to run pre-serialization</param>
    /// <param name="dialog">The dialog to serialize</param>
    /// <returns>The serialized dialog</returns>
    [Pure]
    string Serialize(ILayoutEngine layoutEngine, Dialog dialog);

    /// <summary>
    ///     Deserializes the .rc snippet into a dialog
    /// </summary>
    /// <param name="serialized">The .rc snippet</param>
    /// <returns>The deserialized dialog</returns>
    [Pure]
    Dialog Deserialize(string serialized);
}