namespace Kodvian.Core.Application.Dashboard.Dtos;

public class DashboardPriorityTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? ResponsibleName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
}
