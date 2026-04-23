using Kodvian.Core.Application.Developers.Dtos;
using Kodvian.Core.Application.Developers.Requests;

namespace Kodvian.Core.Application.Developers.Abstractions;

public interface IDeveloperService
{
    Task<IReadOnlyCollection<DeveloperDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DeveloperDto> CreateAsync(DeveloperUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<DeveloperDto?> UpdateAsync(Guid id, DeveloperUpsertRequestDto request, CancellationToken cancellationToken = default);
}
