namespace Kodvian.Core.Application.Integrations.GitHub.Dtos;

public class GitHubRepositoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string OwnerLogin { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public bool Private { get; set; }
    public string? Description { get; set; }
}
