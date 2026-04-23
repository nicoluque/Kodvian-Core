using Kodvian.Core.Application.Common.Files;
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
    Task<IReadOnlyCollection<FileMetadataDto>> GetReceiptsAsync(Guid movementId, CancellationToken cancellationToken = default);
    Task<FileMetadataDto> AddReceiptAsync(Guid movementId, Guid uploadedById, string fileName, string contentType, byte[] content, CancellationToken cancellationToken = default);
    Task<FileDownloadDto?> GetReceiptContentAsync(Guid movementId, Guid receiptId, CancellationToken cancellationToken = default);
    Task<bool> DeleteReceiptAsync(Guid movementId, Guid receiptId, CancellationToken cancellationToken = default);
}
