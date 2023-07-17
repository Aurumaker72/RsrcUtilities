namespace RsrcArchitect.Services;

/// <summary>
///     The default <see langword="interface" /> for a service that provides file picking functionality
/// </summary>
public interface IFilePickerService
{
    /// <summary>
    ///     Tries to pick a file to open with a specified extension
    /// </summary>
    /// <param name="extensions">The extensions to use</param>
    /// <returns>The path to the file, if available</returns>
    Task<string?> TryPickOpenFileAsync(string[] extensions);

    /// <summary>
    ///     Tries to pick a file to save to with the specified parameters
    /// </summary>
    /// <param name="filename">The suggested filename to use</param>
    /// <param name="fileType">The info on the file type to save to</param>
    /// <returns>The path to the file, if available</returns>
    Task<string?> TryPickSaveFileAsync(string filename, (string Name, string[] Extensions) fileType);
}