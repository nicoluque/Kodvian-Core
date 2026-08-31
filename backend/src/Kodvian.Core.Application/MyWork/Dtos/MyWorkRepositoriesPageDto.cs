namespace Kodvian.Core.Application.MyWork.Dtos;

public class MyWorkRepositoriesPageDto
{
    public bool GitHubNotConnected { get; set; }
    public IReadOnlyCollection<MyWorkRepositoryListItemDto> Items { get; set; } = Array.Empty<MyWorkRepositoryListItemDto>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
