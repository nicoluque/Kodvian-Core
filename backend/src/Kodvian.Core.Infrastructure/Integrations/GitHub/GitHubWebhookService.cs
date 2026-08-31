using System.Text.Json;
using Kodvian.Core.Application.Integrations.GitHub;
using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Integrations.GitHub;

public class GitHubWebhookService : IGitHubWebhookService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HashSet<string> SupportedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "opened",
        "closed",
        "reopened",
        "edited"
    };

    private readonly KodvianDbContext _dbContext;

    public GitHubWebhookService(KodvianDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GitHubWebhookResultDto> HandleIssueEventAsync(
        string? eventName,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(eventName, "issues", StringComparison.OrdinalIgnoreCase))
        {
            return Ignored("Evento no soportado.");
        }

        GitHubWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<GitHubWebhookPayload>(payloadJson, JsonOptions);
        }
        catch (JsonException)
        {
            return Ignored("Payload inválido.");
        }

        if (payload?.Issue is null || payload.Repository?.Owner is null)
        {
            return Ignored("Payload incompleto.");
        }

        if (!SupportedActions.Contains(payload.Action))
        {
            return Ignored("Acción no soportada.");
        }

        if (payload.Issue.PullRequest is not null)
        {
            return Ignored("Pull request ignorado.");
        }

        var owner = payload.Repository.Owner.Login.Trim();
        var repo = payload.Repository.Name.Trim();
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
        {
            return Ignored("Repositorio inválido.");
        }

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                x => x.Activo
                    && x.GitHubOwner == owner
                    && x.GitHubRepoName == repo,
                cancellationToken);

        if (project is null)
        {
            return Ignored("Repositorio no vinculado.");
        }

        var existing = await _dbContext.GitHubIssueLinks
            .FirstOrDefaultAsync(x => x.GitHubIssueNodeId == payload.Issue.NodeId, cancellationToken);

        if (existing is not null)
        {
            if (GitHubSyncAntiLoop.ShouldIgnoreInboundUpdate(existing, DateTime.UtcNow))
            {
                return Ignored("Actualización ignorada por anti-loop.");
            }

            ApplyIssueData(existing, payload.Issue, payload.Action);
            existing.SyncDirection = SyncDirection.FromGitHub;
            existing.LastSyncedAt = DateTime.UtcNow;
            existing.FechaActualizacion = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Processed("Issue actualizada.");
        }

        if (!string.Equals(payload.Action, "opened", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(payload.Action, "reopened", StringComparison.OrdinalIgnoreCase))
        {
            return Ignored("Issue no rastreada.");
        }

        var developerId = await ResolveDeveloperIdAsync(project.Id, payload.Issue.Assignee?.Login, cancellationToken);
        if (!developerId.HasValue)
        {
            return Ignored("Asignatario no vinculado.");
        }

        _dbContext.GitHubIssueLinks.Add(new GitHubIssueLink
        {
            ProjectId = project.Id,
            DeveloperId = developerId.Value,
            GitHubIssueNumber = payload.Issue.Number,
            GitHubIssueNodeId = payload.Issue.NodeId,
            GitHubIssueUrl = payload.Issue.HtmlUrl,
            Title = payload.Issue.Title,
            Description = payload.Issue.Body,
            Status = MapStatus(payload.Issue.State),
            AssignedGitHubUsername = payload.Issue.Assignee?.Login,
            LastSyncedAt = DateTime.UtcNow,
            SyncDirection = SyncDirection.FromGitHub,
            Activo = true
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Processed("Issue importada.");
    }

    private async Task<Guid?> ResolveDeveloperIdAsync(Guid projectId, string? assigneeLogin, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(assigneeLogin))
        {
            return null;
        }

        var developerId = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Activo
                && x.DeveloperId != null
                && x.GitHubUsername == assigneeLogin)
            .Select(x => x.DeveloperId!.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (developerId == Guid.Empty)
        {
            return null;
        }

        var hasAccess = await _dbContext.Projects.AsNoTracking()
            .AnyAsync(x => x.Id == projectId
                && x.Activo
                && (x.DeveloperAssignments.Any(a => a.DeveloperId == developerId && a.Activo)
                    || x.DeveloperContracts.Any(c => c.DeveloperId == developerId && c.Activo)
                    || x.Tareas.Any(t => t.DeveloperId == developerId && t.Activo)),
                cancellationToken);

        return hasAccess ? developerId : null;
    }

    private static void ApplyIssueData(GitHubIssueLink link, GitHubWebhookIssue issue, string action)
    {
        link.Title = issue.Title;
        link.Description = issue.Body;
        link.GitHubIssueNumber = issue.Number;
        link.GitHubIssueUrl = issue.HtmlUrl;
        link.AssignedGitHubUsername = issue.Assignee?.Login ?? link.AssignedGitHubUsername;
        link.Status = string.Equals(action, "closed", StringComparison.OrdinalIgnoreCase)
            ? GitHubIssueStatus.Closed
            : string.Equals(action, "reopened", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "opened", StringComparison.OrdinalIgnoreCase)
                ? GitHubIssueStatus.Open
                : MapStatus(issue.State);
        link.Activo = true;
    }

    private static GitHubIssueStatus MapStatus(string state)
    {
        return state.Equals("closed", StringComparison.OrdinalIgnoreCase)
            ? GitHubIssueStatus.Closed
            : GitHubIssueStatus.Open;
    }

    private static GitHubWebhookResultDto Processed(string reason)
    {
        return new GitHubWebhookResultDto { Processed = true, Reason = reason };
    }

    private static GitHubWebhookResultDto Ignored(string reason)
    {
        return new GitHubWebhookResultDto { Ignored = true, Reason = reason };
    }
}
