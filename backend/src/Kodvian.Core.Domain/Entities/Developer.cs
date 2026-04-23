namespace Kodvian.Core.Domain.Entities;

public class Developer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? TaxId { get; set; }
    public string? Notes { get; set; }

    public ICollection<ProjectDeveloperContract> Contracts { get; set; } = new List<ProjectDeveloperContract>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
