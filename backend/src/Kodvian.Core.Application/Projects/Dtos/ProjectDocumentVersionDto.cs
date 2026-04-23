namespace Kodvian.Core.Application.Projects.Dtos;

public class ProjectDocumentVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? Notes { get; set; }
}
