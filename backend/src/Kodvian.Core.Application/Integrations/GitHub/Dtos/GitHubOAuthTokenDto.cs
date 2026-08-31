namespace Kodvian.Core.Application.Integrations.GitHub.Dtos;

public class GitHubOAuthTokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}
