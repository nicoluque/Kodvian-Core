namespace Kodvian.Core.Application.Tasks.Dtos;

public class TaskDetailDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ResponsibleId { get; set; }
    public string? ResponsibleName { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? FinishedDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? RealHours { get; set; }
    public int KanbanOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
