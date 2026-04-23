namespace Kodvian.Core.Application.Tasks.Dtos;

public class TaskLookupsDto
{
    public IReadOnlyCollection<TaskLookupItemDto> Projects { get; set; } = Array.Empty<TaskLookupItemDto>();
    public IReadOnlyCollection<TaskLookupItemDto> Developers { get; set; } = Array.Empty<TaskLookupItemDto>();
}
