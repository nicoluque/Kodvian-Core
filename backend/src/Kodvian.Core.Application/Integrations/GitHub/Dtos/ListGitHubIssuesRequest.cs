namespace Kodvian.Core.Application.Integrations.GitHub.Dtos;

public class ListGitHubIssuesRequest
{
    public string State { get; set; } = "all";
    public string? Assignee { get; set; }
    public string? Labels { get; set; }
    public int PerPage { get; set; } = 50;
    public int Page { get; set; } = 1;
}
