namespace Kodvian.Core.Application.Clients.Dtos;

public class ClientListItemDto
{
    public Guid Id { get; set; }
    public string CommercialName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? MonthlyAmount { get; set; }
    public bool IsActive { get; set; }
}
