namespace Kodvian.Core.Application.Finances.Dtos;

public class FinancialCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
