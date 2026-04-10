namespace Kodvian.Core.Application.Clients.Dtos;

public class ClientDetailDto
{
    public Guid Id { get; set; }
    public string CommercialName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Country { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ServiceType { get; set; }
    public decimal? MonthlyAmount { get; set; }
    public int? BillingDay { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
