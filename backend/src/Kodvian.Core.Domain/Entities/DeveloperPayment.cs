namespace Kodvian.Core.Domain.Entities;

public class DeveloperPayment : BaseEntity
{
    public Guid ContractId { get; set; }
    public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public decimal Amount { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public ProjectDeveloperContract? Contract { get; set; }
    public ICollection<DocumentFile> Documents { get; set; } = new List<DocumentFile>();
}
