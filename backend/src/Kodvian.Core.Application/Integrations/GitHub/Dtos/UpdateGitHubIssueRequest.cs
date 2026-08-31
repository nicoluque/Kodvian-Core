namespace Kodvian.Core.Application.Integrations.GitHub.Dtos;

public class UpdateGitHubIssueRequest
{
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? State { get; set; }
    public IReadOnlyList<string>? Assignees { get; set; }
    public IReadOnlyList<string>? Labels { get; set; }
}
