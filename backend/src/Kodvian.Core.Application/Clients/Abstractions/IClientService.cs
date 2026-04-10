using Kodvian.Core.Application.Clients.Dtos;
using Kodvian.Core.Application.Clients.Requests;
using Kodvian.Core.Application.Common.Models;

namespace Kodvian.Core.Application.Clients.Abstractions;

public interface IClientService
{
    Task<PagedResultDto<ClientListItemDto>> GetPagedAsync(ClientListRequestDto request, CancellationToken cancellationToken = default);
    Task<ClientDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClientDetailDto> CreateAsync(ClientUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<ClientDetailDto?> UpdateAsync(Guid id, ClientUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<ClientDetailDto?> ChangeStatusAsync(Guid id, ChangeClientStatusRequestDto request, CancellationToken cancellationToken = default);
}
