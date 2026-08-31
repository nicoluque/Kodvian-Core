using Kodvian.Core.Application.Integrations.GitHub.Dtos;

namespace Kodvian.Core.Application.Integrations.GitHub.Abstractions;

public interface IGitHubWebhookService
{
    Task<GitHubWebhookResultDto> HandleIssueEventAsync(
        string? eventName,
        string payloadJson,
        CancellationToken cancellationToken = default);
}
