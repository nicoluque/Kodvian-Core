using Kodvian.Core.Application.Common.Files;
using Kodvian.Core.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IOptions<StorageOptions> options)
    {
        var configuredPath = options.Value.BasePath?.Trim();
        _basePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "App_Data", "files"))
            : Path.GetFullPath(configuredPath);

        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(byte[] content, string extension, CancellationToken cancellationToken = default)
    {
        if (content.Length == 0)
        {
            throw new ArgumentException("El archivo no contiene información");
        }

        var sanitizedExtension = string.IsNullOrWhiteSpace(extension) ? ".bin" : extension.Trim();
        if (!sanitizedExtension.StartsWith(".", StringComparison.Ordinal))
        {
            sanitizedExtension = $".{sanitizedExtension}";
        }

        var relativePath = Path.Combine(DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"), $"{Guid.NewGuid():N}{sanitizedExtension}");
        var fullPath = Path.Combine(_basePath, relativePath);
        var folderPath = Path.GetDirectoryName(fullPath) ?? _basePath;
        Directory.CreateDirectory(folderPath);

        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
        return relativePath.Replace('\\', '/');
    }

    public async Task<byte[]> ReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = BuildFullPath(storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("El archivo solicitado no existe", storagePath);
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = BuildFullPath(storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string BuildFullPath(string storagePath)
    {
        var relativePath = storagePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_basePath, relativePath));
        if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("La ruta del archivo es inválida");
        }

        return fullPath;
    }
}
