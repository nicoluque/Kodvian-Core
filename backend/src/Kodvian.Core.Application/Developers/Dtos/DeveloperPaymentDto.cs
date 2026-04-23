using Kodvian.Core.Application.Common.Files;

namespace Kodvian.Core.Application.Developers.Dtos;

public class DeveloperPaymentDto
{
    public Guid Id { get; set; }
    public Guid ContractId { get; set; }
    public DateOnly PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyCollection<FileMetadataDto> Receipts { get; set; } = Array.Empty<FileMetadataDto>();
}
