namespace Kodvian.Core.Application.Projects.Requests;

public class LinkGitHubRepositoryRequestDto
{
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;
}
