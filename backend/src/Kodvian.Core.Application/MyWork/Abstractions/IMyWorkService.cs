using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.MyWork.Dtos;
using Kodvian.Core.Application.Projects.Dtos;
using Kodvian.Core.Application.Projects.Requests;
using Kodvian.Core.Application.Tasks.Dtos;
using Kodvian.Core.Application.Tasks.Requests;

namespace Kodvian.Core.Application.MyWork.Abstractions;

public interface IMyWorkService
{
    Task<MyWorkOverviewDto> GetOverviewAsync(Guid developerId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<ProjectListItemDto>> GetProjectsAsync(Guid developerId, ProjectListRequestDto request, CancellationToken cancellationToken = default);
    Task<PagedResultDto<TaskListItemDto>> GetTasksAsync(Guid developerId, TaskListRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TaskKanbanColumnDto>> GetTaskKanbanAsync(Guid developerId, TaskListRequestDto request, CancellationToken cancellationToken = default);
    Task<TaskDetailDto?> GetTaskByIdAsync(Guid developerId, Guid taskId, CancellationToken cancellationToken = default);
    Task<TaskDetailDto?> UpdateTaskStatusAsync(Guid developerId, Guid taskId, TaskStatusUpdateRequestDto request, CancellationToken cancellationToken = default);
}
