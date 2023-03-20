using System;
using System.IO;
using System.Threading.Tasks;
using RsrcArchitect.Services.Abstractions;

namespace RsrcArchitect.Views.WPF.Services.Abstractions;

/// <summary>
///     A <see langword="class" /> implementing a WPF version of <see cref="IFile" />.
/// </summary>
internal sealed class File : IFile
{
    /// <summary>
    ///     Creates a new <see cref="File" /> instance.
    /// </summary>
    /// <param name="storageFile">The input <see cref="Windows.Storage.StorageFile" /> instance to wrap.</param>
    public File(string path)
    {
        StorageFile = new FileInfo(path);
    }

    /// <summary>
    ///     The underlying <see cref="FileInfo" /> instance in use.
    /// </summary>
    internal FileInfo StorageFile { get; }

    /// <inheritdoc />
    public string DisplayName => StorageFile.Name;

    /// <inheritdoc />
    public string Path => StorageFile.FullName;

    /// <inheritdoc />
    public bool IsReadOnly => StorageFile.IsReadOnly;

    /// <inheritdoc />
    public async Task<(ulong, DateTimeOffset)> GetPropertiesAsync()
    {
        return ((ulong)StorageFile.Length, StorageFile.LastWriteTime);
    }

    /// <inheritdoc />
    public Task<Stream> OpenStreamForReadAsync()
    {
        return Task.FromResult((Stream)StorageFile.OpenRead());
    }

    /// <inheritdoc />
    public Task<Stream> OpenStreamForWriteAsync()
    {
        return Task.FromResult((Stream)StorageFile.OpenWrite());
    }

    /// <inheritdoc />
    public Task DeleteAsync()
    {
        StorageFile.Delete();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void RequestFutureAccessPermission(string metadata)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void RemoveFutureAccessPermission()
    {
        throw new NotImplementedException();
    }
}