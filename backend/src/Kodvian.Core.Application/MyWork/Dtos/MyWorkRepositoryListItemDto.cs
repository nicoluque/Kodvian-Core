namespace Kodvian.Core.Application.MyWork.Dtos;

public class MyWorkRepositoryListItemDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ProjectStatus { get; set; } = string.Empty;
    public string GitHubOwner { get; set; } = string.Empty;
    public string GitHubRepoName { get; set; } = string.Empty;
    public string FullName => string.IsNullOrWhiteSpace(GitHubOwner) || string.IsNullOrWhiteSpace(GitHubRepoName)
        ? string.Empty
        : $"{GitHubOwner}/{GitHubRepoName}";
    public string? GitHubRepoUrl { get; set; }
    public long? GitHubRepoId { get; set; }
    public int OpenIssuesCount { get; set; }
}
