namespace Kodvian.Core.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RoleId { get; set; }

    public Role? Role { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<FinancialMovement> FinancialMovementsCreated { get; set; } = new List<FinancialMovement>();
    public ICollection<DocumentFile> UploadedDocuments { get; set; } = new List<DocumentFile>();
    public ICollection<ProjectDocument> ProjectDocumentsCreated { get; set; } = new List<ProjectDocument>();
    public ICollection<ProjectDocument> ProjectDocumentsDeleted { get; set; } = new List<ProjectDocument>();
    public ICollection<ProjectDocumentVersion> ProjectDocumentVersionsUploaded { get; set; } = new List<ProjectDocumentVersion>();
}
