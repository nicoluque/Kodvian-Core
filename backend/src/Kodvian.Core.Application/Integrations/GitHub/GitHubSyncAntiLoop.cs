using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;

namespace Kodvian.Core.Application.Integrations.GitHub;

public static class GitHubSyncAntiLoop
{
    public static readonly TimeSpan Window = TimeSpan.FromSeconds(30);

    public static bool ShouldIgnoreInboundUpdate(GitHubIssueLink link, DateTime utcNow)
    {
        return link.SyncDirection == SyncDirection.FromKodvian
            && link.LastSyncedAt.HasValue
            && utcNow - link.LastSyncedAt.Value < Window;
    }
}
