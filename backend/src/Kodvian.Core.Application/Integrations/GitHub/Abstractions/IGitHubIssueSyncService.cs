using Kodvian.Core.Application.Integrations.GitHub.Dtos;

namespace Kodvian.Core.Application.Integrations.GitHub.Abstractions;

public interface IGitHubIssueSyncService
{
    Task<GitHubIssueSyncResultDto> SyncIssuesFromGitHubAsync(
        Guid developerId,
        Guid userId,
        Guid? projectId = null,
        CancellationToken cancellationToken = default);

    Task SyncAfterConnectAsync(Guid userId, Guid? developerId, CancellationToken cancellationToken = default);
}
