namespace Kodvian.Core.Application.Integrations.GitHub.Dtos;

public class CreateGitHubIssueRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public IReadOnlyList<string>? Assignees { get; set; }
    public IReadOnlyList<string>? Labels { get; set; }
}
