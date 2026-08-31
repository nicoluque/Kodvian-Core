namespace Kodvian.Core.Application.MyWork.Dtos;

public class MyWorkOverviewDto
{
    public bool GitHubNotConnected { get; set; }
    public int RepositoryCount { get; set; }
    public int TotalIssuesCount { get; set; }
    public int OpenIssuesCount { get; set; }
    public IReadOnlyCollection<MyWorkRepositoryListItemDto> Repositories { get; set; } = Array.Empty<MyWorkRepositoryListItemDto>();
    public IReadOnlyCollection<MyWorkIssueListItemDto> Issues { get; set; } = Array.Empty<MyWorkIssueListItemDto>();
}
