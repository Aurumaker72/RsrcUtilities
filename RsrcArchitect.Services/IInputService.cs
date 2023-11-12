using RsrcArchitect.ViewModels.Types;

namespace RsrcArchitect.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides input polling functionality
/// </summary>
public interface IInputService
{
    /// <summary>
    /// Gets a key's held state
    /// </summary>
    /// <param name="key">The key to check against</param>
    /// <returns>Whether the key is currently being held</returns>
    public bool IsKeyHeld(Key key);
}