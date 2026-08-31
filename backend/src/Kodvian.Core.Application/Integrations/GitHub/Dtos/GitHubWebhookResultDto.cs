namespace Kodvian.Core.Application.Integrations.GitHub.Dtos;

public class GitHubWebhookResultDto
{
    public bool Processed { get; set; }
    public bool Ignored { get; set; }
    public string? Reason { get; set; }
}
