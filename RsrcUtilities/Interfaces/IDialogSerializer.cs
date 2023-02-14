﻿using System.Diagnostics.Contracts;

namespace RsrcUtilities.Interfaces;

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
    /// <param name="dialog">The dialog to serialize</param>
    /// <returns>The serialized dialog</returns>
    [Pure]
    string Serialize(Dialog dialog);
}