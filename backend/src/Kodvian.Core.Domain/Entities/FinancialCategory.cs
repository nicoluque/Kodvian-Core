using Kodvian.Core.Domain.Enums;

namespace Kodvian.Core.Domain.Entities;

public class FinancialCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public FinancialMovementType MovementType { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<FinancialMovement> FinancialMovements { get; set; } = new List<FinancialMovement>();
}
