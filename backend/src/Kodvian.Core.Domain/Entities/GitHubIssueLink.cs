using Kodvian.Core.Domain.Enums;

namespace Kodvian.Core.Domain.Entities;

public class GitHubIssueLink : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid DeveloperId { get; set; }
    public int GitHubIssueNumber { get; set; }
    public string GitHubIssueNodeId { get; set; } = string.Empty;
    public string GitHubIssueUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GitHubIssueStatus Status { get; set; } = GitHubIssueStatus.Open;
    public TaskPriority? Priority { get; set; }
    public string? AssignedGitHubUsername { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public SyncDirection SyncDirection { get; set; } = SyncDirection.None;

    public Project? Project { get; set; }
    public Developer? Developer { get; set; }
}
