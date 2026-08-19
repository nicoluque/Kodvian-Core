using Kodvian.Core.Application.Developers.Dtos;
using Kodvian.Core.Application.Developers.Requests;

namespace Kodvian.Core.Application.Developers.Abstractions;

public interface IProjectDeveloperAssignmentService
{
    Task<IReadOnlyCollection<ProjectDeveloperAssignmentDto>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ProjectDeveloperAssignmentDto> CreateAsync(Guid projectId, ProjectDeveloperAssignmentCreateRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
