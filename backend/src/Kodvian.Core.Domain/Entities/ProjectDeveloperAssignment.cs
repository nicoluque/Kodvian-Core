namespace Kodvian.Core.Domain.Entities;

public class ProjectDeveloperAssignment : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid DeveloperId { get; set; }
    public string? Notes { get; set; }

    public Project? Project { get; set; }
    public Developer? Developer { get; set; }
}
