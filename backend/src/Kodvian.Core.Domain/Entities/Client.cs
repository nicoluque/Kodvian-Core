using Kodvian.Core.Domain.Enums;

namespace Kodvian.Core.Domain.Entities;

public class Client : BaseEntity
{
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
    public ClientStatus Status { get; set; } = ClientStatus.Prospecto;
    public string? ServiceType { get; set; }
    public decimal? MonthlyAmount { get; set; }
    public int? BillingDay { get; set; }
    public string? Notes { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
