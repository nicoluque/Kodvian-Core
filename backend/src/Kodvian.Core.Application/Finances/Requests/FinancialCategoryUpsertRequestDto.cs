namespace Kodvian.Core.Application.Finances.Requests;

public class FinancialCategoryUpsertRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string MovementType { get; set; } = "Ingreso";
    public bool IsActive { get; set; } = true;
}
