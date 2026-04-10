using Kodvian.Core.Application.Finances.Dtos;
using Kodvian.Core.Application.Finances.Requests;

namespace Kodvian.Core.Application.Finances.Abstractions;

public interface IProviderService
{
    Task<IReadOnlyCollection<ProviderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProviderDto> CreateAsync(ProviderUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<ProviderDto?> UpdateAsync(Guid id, ProviderUpsertRequestDto request, CancellationToken cancellationToken = default);
}
