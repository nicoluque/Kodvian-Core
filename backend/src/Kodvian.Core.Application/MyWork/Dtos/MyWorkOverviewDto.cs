using Kodvian.Core.Application.Projects.Dtos;
using Kodvian.Core.Application.Tasks.Dtos;

namespace Kodvian.Core.Application.MyWork.Dtos;

public class MyWorkOverviewDto
{
    public IReadOnlyCollection<ProjectListItemDto> Projects { get; set; } = Array.Empty<ProjectListItemDto>();
    public IReadOnlyCollection<TaskListItemDto> Tasks { get; set; } = Array.Empty<TaskListItemDto>();
}
