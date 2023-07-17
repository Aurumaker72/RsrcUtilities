using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using RsrcArchitect.Services;

namespace RsrcArchitect.Views.WPF.Services;

/// <summary>
///     A <see langword="class" /> that implements the <see cref="IFilePickerService" /> <see langword="interface" /> using WPF APIs
/// </summary>
public sealed class FilesService : IFilePickerService
{
    /// <inheritdoc />
    public async Task<string?> TryPickOpenFileAsync(string[] extensions)
    {
        CommonOpenFileDialog dialog = new();
        var list = string.Empty;

        extensions ??= new[] { "*" };

        list = extensions.Aggregate(list, (current, t) => current + $"*.{t};");

        dialog.Filters.Add(new CommonFileDialogFilter("Supported formats", list));
        dialog.EnsureFileExists = dialog.EnsurePathExists = true;

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok) return dialog.FileName;
        return null;
    }

    /// <inheritdoc />
    public async Task<string?> TryPickSaveFileAsync(string filename, (string Name, string[] Extensions) fileType)
    {
        CommonSaveFileDialog dialog = new();
        var list = fileType.Extensions.Aggregate(string.Empty, (current, t) => current + $"*.{t};");
        dialog.DefaultFileName = filename;
        dialog.Filters.Add(new CommonFileDialogFilter(fileType.Name, list));
        var result = dialog.ShowDialog();
        if (result == CommonFileDialogResult.Ok) return dialog.FileName;
        return null;
    }
}