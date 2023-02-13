namespace RsrcUtilities.Interfaces;

/// <summary>
///     Represents a dialog serializer contract
/// </summary>
public interface IDialogSerializer
{
    /// <summary>
    ///     Tries to serialize the dialog into an .rc snippet
    /// </summary>
    /// <param name="dialog">The dialog to serialize</param>
    /// <param name="serialized">The serialized dialog</param>
    /// <returns><see langword="true" />, if succeeded</returns>
    bool TrySerialize(Dialog dialog, out string? serialized);
}