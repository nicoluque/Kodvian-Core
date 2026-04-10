namespace Kodvian.Core.Application.Tasks.Requests;

public class TaskUpsertRequestDto
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ResponsibleId { get; set; }
    public string Status { get; set; } = "Pendiente";
    public string Priority { get; set; } = "Media";
    public DateOnly? StartDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? FinishedDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? RealHours { get; set; }
    public int KanbanOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
