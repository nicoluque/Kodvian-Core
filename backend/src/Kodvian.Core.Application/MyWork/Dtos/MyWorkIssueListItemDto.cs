namespace Kodvian.Core.Application.MyWork.Dtos;

public class MyWorkIssueListItemDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string RepositoryFullName { get; set; } = string.Empty;
    public int GitHubIssueNumber { get; set; }
    public string? GitHubIssueUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public DateTime CreatedAt { get; set; }
}
