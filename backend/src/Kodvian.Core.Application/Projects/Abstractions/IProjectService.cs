using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Projects.Dtos;
using Kodvian.Core.Application.Projects.Requests;

namespace Kodvian.Core.Application.Projects.Abstractions;

public interface IProjectService
{
    Task<PagedResultDto<ProjectListItemDto>> GetPagedAsync(ProjectListRequestDto request, CancellationToken cancellationToken = default);
    Task<ProjectDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProjectDetailDto> CreateAsync(ProjectUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<ProjectDetailDto?> UpdateAsync(Guid id, ProjectUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<ProjectLookupsDto> GetLookupsAsync(CancellationToken cancellationToken = default);
}
