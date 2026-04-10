namespace Kodvian.Core.Application.Tasks.Dtos;

public class TaskLookupsDto
{
    public IReadOnlyCollection<TaskLookupItemDto> Projects { get; set; } = Array.Empty<TaskLookupItemDto>();
    public IReadOnlyCollection<TaskLookupItemDto> Responsibles { get; set; } = Array.Empty<TaskLookupItemDto>();
}
