using Kodvian.Core.Domain.Enums;

namespace Kodvian.Core.Domain.Entities;

public class ProjectDocument : BaseEntity
{
    public Guid ProjectId { get; set; }
    public ProjectDocumentType Type { get; set; } = ProjectDocumentType.General;
    public string Title { get; set; } = string.Empty;
    public Guid CreatedById { get; set; }
    public Guid? DeletedById { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Project? Project { get; set; }
    public User? CreatedBy { get; set; }
    public User? DeletedBy { get; set; }
    public ICollection<ProjectDocumentVersion> Versions { get; set; } = new List<ProjectDocumentVersion>();
}
