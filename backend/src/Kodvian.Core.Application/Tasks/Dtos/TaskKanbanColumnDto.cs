namespace Kodvian.Core.Application.Tasks.Dtos;

public class TaskKanbanColumnDto
{
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IReadOnlyCollection<TaskKanbanItemDto> Items { get; set; } = Array.Empty<TaskKanbanItemDto>();
}
