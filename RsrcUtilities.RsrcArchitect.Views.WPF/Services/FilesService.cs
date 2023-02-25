using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using RsrcUtilities.RsrcArchitect.Services;
using RsrcUtilities.RsrcArchitect.Services.Abstractions;
using File = CookieCore.Views.WPF.Services.Abstractions.File;

namespace CookieCore.Views.WPF.Services;

/// <summary>
///     A <see langword="class" /> that implements the <see cref="IFilesService" /> <see langword="interface" /> using WPF
///     APIs
/// </summary>
public sealed class FilesService : IFilesService
{
    /// <inheritdoc />
    public string InstallationPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    /// <inheritdoc />
    public string TemporaryFilesPath => Path.GetTempPath();

    /// <inheritdoc />
    public async Task<IFile> GetFileFromPathAsync(string path)
    {
        return new File(path);
    }

    /// <inheritdoc />
    public async Task<IFile?> TryGetFileFromPathAsync(string path)
    {
        try
        {
            return await GetFileFromPathAsync(path);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IFile> CreateOrOpenFileFromPathAsync(string path)
    {
        var folderPath = Path.GetDirectoryName(path);
        var filename = Path.GetFileName(path);

        if (folderPath != null && !Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        if (!System.IO.File.Exists(path)) System.IO.File.Create(path);

        return new File(path);
    }

    /// <inheritdoc />
    public async Task<IFile?> TryPickOpenFileAsync(string[] extensions)
    {
        CommonOpenFileDialog dialog = new();
        var list = string.Empty;

        extensions ??= new[] { "*" };

        for (var i = 0; i < extensions.Length; i++) list += $"*.{extensions[i]};";

        dialog.Filters.Add(new CommonFileDialogFilter("Supported formats", list));
        dialog.EnsureFileExists = dialog.EnsurePathExists = true;

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok) return new File(dialog.FileName);
        return null;
    }

    /// <inheritdoc />
    public async Task<IFile?> TryPickSaveFileAsync(string filename, (string Name, string[] Extensions) fileType)
    {
        CommonSaveFileDialog dialog = new();
        var list = fileType.Extensions.Aggregate(string.Empty, (current, t) => current + $"*.{t};");
        dialog.DefaultFileName = filename;
        dialog.Filters.Add(new CommonFileDialogFilter(fileType.Name, list));
        var result = dialog.ShowDialog();
        if (result == CommonFileDialogResult.Ok) return new File(dialog.FileName);
        return null;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<(IFile, string)> GetFutureAccessFilesAsync()
    {
        yield return await Task.FromResult<(IFile, string)>((null, null));
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<bool> IsAccessible(string path)
    {
        try
        {
            await using (_ = System.IO.File.Open(path, FileMode.Open))
            {
                ;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}