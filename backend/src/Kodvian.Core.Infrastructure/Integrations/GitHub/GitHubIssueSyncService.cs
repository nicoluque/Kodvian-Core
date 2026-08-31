using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Integrations.GitHub;

public class GitHubIssueSyncService : IGitHubIssueSyncService
{
    private const int IssuesPageSize = 100;

    private readonly KodvianDbContext _dbContext;
    private readonly IGitHubApiService _gitHubApiService;
    private readonly IGitHubTokenProvider _gitHubTokenProvider;

    public GitHubIssueSyncService(
        KodvianDbContext dbContext,
        IGitHubApiService gitHubApiService,
        IGitHubTokenProvider gitHubTokenProvider)
    {
        _dbContext = dbContext;
        _gitHubApiService = gitHubApiService;
        _gitHubTokenProvider = gitHubTokenProvider;
    }

    public async Task SyncAfterConnectAsync(Guid userId, Guid? developerId, CancellationToken cancellationToken = default)
    {
        if (!developerId.HasValue)
        {
            return;
        }

        await SyncIssuesFromGitHubAsync(developerId.Value, userId, projectId: null, cancellationToken);
    }

    public async Task<GitHubIssueSyncResultDto> SyncIssuesFromGitHubAsync(
        Guid developerId,
        Guid userId,
        Guid? projectId = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.Activo, cancellationToken)
            ?? throw new ArgumentException("Usuario no encontrado.");

        if (string.IsNullOrWhiteSpace(user.GitHubUsername))
        {
            throw new InvalidOperationException(GitHubTokenProvider.ReconnectMessage);
        }

        var token = await _gitHubTokenProvider.GetValidTokenAsync(userId, cancellationToken);
        var projects = await BuildAccessibleProjectsQuery(developerId, projectId)
            .Select(x => new
            {
                x.Id,
                Owner = x.GitHubOwner!,
                Repo = x.GitHubRepoName!
            })
            .ToListAsync(cancellationToken);

        if (projectId.HasValue && projects.Count == 0)
        {
            throw new InvalidOperationException("No tenés acceso a ese proyecto o no tiene repositorio GitHub configurado.");
        }

        var result = new GitHubIssueSyncResultDto();

        foreach (var project in projects)
        {
            result.RepositoriesSynced++;
            await SyncProjectIssuesAsync(
                developerId,
                project.Id,
                project.Owner,
                project.Repo,
                user.GitHubUsername,
                token,
                result,
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task SyncProjectIssuesAsync(
        Guid developerId,
        Guid projectId,
        string owner,
        string repo,
        string gitHubUsername,
        string token,
        GitHubIssueSyncResultDto result,
        CancellationToken cancellationToken)
    {
        var page = 1;
        while (true)
        {
            var issues = await _gitHubApiService.ListIssuesAsync(
                owner,
                repo,
                new ListGitHubIssuesRequest
                {
                    State = "all",
                    Assignee = gitHubUsername,
                    PerPage = IssuesPageSize,
                    Page = page
                },
                token,
                cancellationToken);

            if (issues.Count == 0)
            {
                break;
            }

            foreach (var issue in issues)
            {
                if (issue.IsPullRequest)
                {
                    result.SkippedPullRequestsCount++;
                    continue;
                }

                var existing = await _dbContext.GitHubIssueLinks
                    .FirstOrDefaultAsync(x => x.GitHubIssueNodeId == issue.NodeId, cancellationToken);

                if (existing is null)
                {
                    _dbContext.GitHubIssueLinks.Add(new GitHubIssueLink
                    {
                        ProjectId = projectId,
                        DeveloperId = developerId,
                        GitHubIssueNumber = issue.Number,
                        GitHubIssueNodeId = issue.NodeId,
                        GitHubIssueUrl = issue.HtmlUrl,
                        Title = issue.Title,
                        Description = issue.Body,
                        Status = MapStatus(issue.State),
                        AssignedGitHubUsername = issue.AssigneeLogin ?? gitHubUsername,
                        LastSyncedAt = DateTime.UtcNow,
                        SyncDirection = SyncDirection.FromGitHub,
                        Activo = true
                    });
                    result.ImportedCount++;
                }
                else
                {
                    existing.GitHubIssueNumber = issue.Number;
                    existing.GitHubIssueUrl = issue.HtmlUrl;
                    existing.Title = issue.Title;
                    existing.Description = issue.Body;
                    existing.Status = MapStatus(issue.State);
                    existing.AssignedGitHubUsername = issue.AssigneeLogin ?? gitHubUsername;
                    existing.LastSyncedAt = DateTime.UtcNow;
                    existing.SyncDirection = SyncDirection.FromGitHub;
                    existing.Activo = true;
                    existing.FechaActualizacion = DateTime.UtcNow;
                    result.UpdatedCount++;
                }
            }

            if (issues.Count < IssuesPageSize)
            {
                break;
            }

            page++;
        }
    }

    private IQueryable<Project> BuildAccessibleProjectsQuery(Guid developerId, Guid? projectId)
    {
        var query = _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Activo
                && x.GitHubOwner != null
                && x.GitHubRepoName != null
                && (x.DeveloperAssignments.Any(a => a.DeveloperId == developerId && a.Activo)
                    || x.DeveloperContracts.Any(c => c.DeveloperId == developerId && c.Activo)
                    || x.Tareas.Any(t => t.DeveloperId == developerId && t.Activo)));

        if (projectId.HasValue)
        {
            query = query.Where(x => x.Id == projectId.Value);
        }

        return query;
    }

    private static GitHubIssueStatus MapStatus(string state)
    {
        return state.Equals("closed", StringComparison.OrdinalIgnoreCase)
            ? GitHubIssueStatus.Closed
            : GitHubIssueStatus.Open;
    }
}
