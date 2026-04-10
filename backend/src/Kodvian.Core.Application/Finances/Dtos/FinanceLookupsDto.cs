namespace Kodvian.Core.Application.Finances.Dtos;

public class FinanceLookupsDto
{
    public IReadOnlyCollection<FinancialCategoryDto> Categories { get; set; } = Array.Empty<FinancialCategoryDto>();
    public IReadOnlyCollection<FinanceLookupItemDto> Clients { get; set; } = Array.Empty<FinanceLookupItemDto>();
    public IReadOnlyCollection<FinanceLookupItemDto> Projects { get; set; } = Array.Empty<FinanceLookupItemDto>();
    public IReadOnlyCollection<FinanceLookupItemDto> Providers { get; set; } = Array.Empty<FinanceLookupItemDto>();
}
