namespace Kodvian.Core.Domain.Entities;

public class DocumentFile : BaseEntity
{
    public Guid? ProjectId { get; set; }
    public Guid? FinancialMovementId { get; set; }
    public Guid? DeveloperPaymentId { get; set; }
    public Guid UploadedById { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;

    public Project? Project { get; set; }
    public FinancialMovement? FinancialMovement { get; set; }
    public DeveloperPayment? DeveloperPayment { get; set; }
    public User? UploadedBy { get; set; }
}
