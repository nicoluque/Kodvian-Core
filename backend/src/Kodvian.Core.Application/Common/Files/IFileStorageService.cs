namespace Kodvian.Core.Application.Common.Files;

public interface IFileStorageService
{
    Task<string> SaveAsync(byte[] content, string extension, CancellationToken cancellationToken = default);
    Task<byte[]> ReadAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
