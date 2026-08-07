namespace Kodvian.Core.Infrastructure.Storage;

public class StorageOptions
{
    public const string LocalProvider = "Local";
    public const string S3Provider = "S3";

    public string Provider { get; set; } = LocalProvider;
    public string BasePath { get; set; } = "App_Data/files";
    public int MaxPdfSizeMb { get; set; } = 10;

    public string? Bucket { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? ServiceUrl { get; set; }
    public string? Region { get; set; }
    public bool ForcePathStyle { get; set; }
}
