namespace Kodvian.Core.Application.Integrations.GitHub.Dtos;

public class GitHubIssueDto
{
    public long Id { get; set; }
    public string NodeId { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string State { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string? AssigneeLogin { get; set; }
    public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();
    public bool IsPullRequest { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
