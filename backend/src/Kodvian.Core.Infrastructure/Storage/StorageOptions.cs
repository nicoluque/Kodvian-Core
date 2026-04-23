namespace Kodvian.Core.Infrastructure.Storage;

public class StorageOptions
{
    public string BasePath { get; set; } = "App_Data/files";
    public int MaxPdfSizeMb { get; set; } = 10;
}
