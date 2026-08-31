using System.Text.Json.Serialization;

namespace Kodvian.Core.Application.Integrations.GitHub.Dtos;

public class GitHubWebhookPayload
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("issue")]
    public GitHubWebhookIssue? Issue { get; set; }

    [JsonPropertyName("repository")]
    public GitHubWebhookRepository? Repository { get; set; }
}

public class GitHubWebhookIssue
{
    [JsonPropertyName("node_id")]
    public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("pull_request")]
    public object? PullRequest { get; set; }

    [JsonPropertyName("assignee")]
    public GitHubWebhookUser? Assignee { get; set; }
}

public class GitHubWebhookRepository
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("owner")]
    public GitHubWebhookUser? Owner { get; set; }
}

public class GitHubWebhookUser
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;
}
