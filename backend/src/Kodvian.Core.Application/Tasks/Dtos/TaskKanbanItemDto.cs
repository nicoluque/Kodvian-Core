namespace Kodvian.Core.Application.Tasks.Dtos;

public class TaskKanbanItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? DeveloperName { get; set; }
    public string? ResponsibleName { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
    public int KanbanOrder { get; set; }
}
