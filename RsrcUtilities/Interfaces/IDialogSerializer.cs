namespace RsrcUtilities.Interfaces;

/// <summary>
///     Represents a dialog serializer contract
/// </summary>
public interface IDialogSerializer
{
    /// <summary>
    ///     Serializes the dialog into an .rc snippet
    /// </summary>
    /// <param name="dialog">The dialog to serialize</param>
    /// <returns>The serialized dialog</returns>
    string Serialize(Dialog dialog);
}