namespace Kodvian.Core.Application.Integrations.GitHub.Dtos;

public class GitHubIssueSyncResultDto
{
    public int ImportedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int SkippedPullRequestsCount { get; set; }
    public int RepositoriesSynced { get; set; }
}
