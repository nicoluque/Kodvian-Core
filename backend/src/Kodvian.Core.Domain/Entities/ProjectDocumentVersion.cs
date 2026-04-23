namespace Kodvian.Core.Domain.Entities;

public class ProjectDocumentVersion : BaseEntity
{
    public Guid ProjectDocumentId { get; set; }
    public Guid DocumentFileId { get; set; }
    public int VersionNumber { get; set; }
    public string? Notes { get; set; }
    public Guid UploadedById { get; set; }

    public ProjectDocument? ProjectDocument { get; set; }
    public DocumentFile? DocumentFile { get; set; }
    public User? UploadedBy { get; set; }
}
