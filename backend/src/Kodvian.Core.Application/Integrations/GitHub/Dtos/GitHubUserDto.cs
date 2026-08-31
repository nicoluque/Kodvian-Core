namespace Kodvian.Core.Application.Integrations.GitHub.Dtos;

public class GitHubUserDto
{
    public long Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? HtmlUrl { get; set; }
}
