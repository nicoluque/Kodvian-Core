namespace Kodvian.Core.Application.Finances.Requests;

public class FinancialMovementUpsertRequestDto
{
    public string MovementType { get; set; } = "Ingreso";
    public Guid CategoryId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ProviderId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly MovementDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Status { get; set; } = "Pendiente";
    public string? PaymentMethod { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? Notes { get; set; }
}
