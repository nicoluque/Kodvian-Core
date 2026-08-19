namespace Kodvian.Core.Application.Developers.Dtos;

public class ProjectDeveloperAssignmentDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid DeveloperId { get; set; }
    public string DeveloperName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
