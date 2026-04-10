using Kodvian.Core.Application.Finances.Dtos;
using Kodvian.Core.Application.Finances.Requests;

namespace Kodvian.Core.Application.Finances.Abstractions;

public interface IFinancialCategoryService
{
    Task<IReadOnlyCollection<FinancialCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FinancialCategoryDto> CreateAsync(FinancialCategoryUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<FinancialCategoryDto?> UpdateAsync(Guid id, FinancialCategoryUpsertRequestDto request, CancellationToken cancellationToken = default);
}
