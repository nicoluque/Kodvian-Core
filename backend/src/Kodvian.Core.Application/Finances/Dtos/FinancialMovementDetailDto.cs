namespace Kodvian.Core.Application.Finances.Dtos;

public class FinancialMovementDetailDto
{
    public Guid Id { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }
    public Guid? ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly MovementDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
