namespace Kodvian.Core.Application.Projects.Dtos;

public class ValidateGitHubRepositoryResultDto
{
    public bool Exists { get; set; }
    public long? RepoId { get; set; }
    public string? Owner { get; set; }
    public string? RepoName { get; set; }
    public string? FullName { get; set; }
    public string? HtmlUrl { get; set; }
    public bool IsPrivate { get; set; }
    public string Message { get; set; } = string.Empty;
}
