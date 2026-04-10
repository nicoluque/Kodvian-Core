using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Finances.Dtos;
using Kodvian.Core.Application.Finances.Requests;

namespace Kodvian.Core.Application.Finances.Abstractions;

public interface IFinancialMovementService
{
    Task<PagedResultDto<FinancialMovementListItemDto>> GetPagedAsync(FinancialMovementListRequestDto request, CancellationToken cancellationToken = default);
    Task<FinancialMovementDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FinancialMovementDetailDto> CreateAsync(Guid createdById, FinancialMovementUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<FinancialMovementDetailDto?> UpdateAsync(Guid id, FinancialMovementUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<FinanceMonthlySummaryDto> GetMonthlySummaryAsync(int? year, int? month, CancellationToken cancellationToken = default);
    Task<FinanceLookupsDto> GetLookupsAsync(CancellationToken cancellationToken = default);
}
