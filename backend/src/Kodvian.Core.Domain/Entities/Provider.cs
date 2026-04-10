namespace Kodvian.Core.Domain.Entities;

public class Provider : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<FinancialMovement> FinancialMovements { get; set; } = new List<FinancialMovement>();
}
