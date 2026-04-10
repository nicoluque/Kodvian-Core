namespace Kodvian.Core.Application.Tasks.Requests;

public class TaskStatusUpdateRequestDto
{
    public string Status { get; set; } = string.Empty;
    public int KanbanOrder { get; set; }
}
