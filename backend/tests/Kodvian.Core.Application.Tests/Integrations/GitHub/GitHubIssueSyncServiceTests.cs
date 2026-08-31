using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Integrations.GitHub;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Application.Tests.Integrations.GitHub;

public class GitHubIssueSyncServiceTests
{
    [Fact]
    public async Task SyncIssuesFromGitHubAsync_ImportsNewIssues()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "alpha");
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            Activo = true
        });
        await db.SaveChangesAsync();

        var fakeGitHub = new FakeGitHubApiService();
        fakeGitHub.SetIssues("kodvian", "alpha", new GitHubIssueDto
        {
            NodeId = "node-1",
            Number = 1,
            Title = "Bug fix",
            State = "open",
            HtmlUrl = "https://github.com/kodvian/alpha/issues/1",
            AssigneeLogin = "devuser"
        });

        var service = CreateService(db, fakeGitHub);
        var result = await service.SyncIssuesFromGitHubAsync(developerId, userId);

        Assert.Equal(1, result.ImportedCount);
        Assert.Equal(0, result.UpdatedCount);
        Assert.Equal(1, result.RepositoriesSynced);
        var link = await db.GitHubIssueLinks.SingleAsync();
        Assert.Equal("Bug fix", link.Title);
        Assert.Equal(GitHubIssueStatus.Open, link.Status);
    }

    [Fact]
    public async Task SyncIssuesFromGitHubAsync_UpdatesExistingIssue_ByNodeId()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "alpha");
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            Activo = true
        });
        db.GitHubIssueLinks.Add(new GitHubIssueLink
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            GitHubIssueNumber = 1,
            GitHubIssueNodeId = "node-1",
            GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/1",
            Title = "Old title",
            Status = GitHubIssueStatus.Open,
            Activo = true
        });
        await db.SaveChangesAsync();

        var fakeGitHub = new FakeGitHubApiService();
        fakeGitHub.SetIssues("kodvian", "alpha", new GitHubIssueDto
        {
            NodeId = "node-1",
            Number = 1,
            Title = "Updated title",
            State = "closed",
            HtmlUrl = "https://github.com/kodvian/alpha/issues/1",
            AssigneeLogin = "devuser"
        });

        var service = CreateService(db, fakeGitHub);
        var result = await service.SyncIssuesFromGitHubAsync(developerId, userId);

        Assert.Equal(0, result.ImportedCount);
        Assert.Equal(1, result.UpdatedCount);
        Assert.Equal(1, await db.GitHubIssueLinks.CountAsync());
        var link = await db.GitHubIssueLinks.SingleAsync();
        Assert.Equal("Updated title", link.Title);
        Assert.Equal(GitHubIssueStatus.Closed, link.Status);
    }

    [Fact]
    public async Task SyncIssuesFromGitHubAsync_SkipsPullRequests()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "alpha");
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            Activo = true
        });
        await db.SaveChangesAsync();

        var fakeGitHub = new FakeGitHubApiService();
        fakeGitHub.SetIssues("kodvian", "alpha",
            new GitHubIssueDto
            {
                NodeId = "node-pr",
                Number = 99,
                Title = "PR disguised as issue",
                State = "open",
                HtmlUrl = "https://github.com/kodvian/alpha/pull/99",
                IsPullRequest = true
            },
            new GitHubIssueDto
            {
                NodeId = "node-2",
                Number = 2,
                Title = "Real issue",
                State = "open",
                HtmlUrl = "https://github.com/kodvian/alpha/issues/2"
            });

        var service = CreateService(db, fakeGitHub);
        var result = await service.SyncIssuesFromGitHubAsync(developerId, userId);

        Assert.Equal(1, result.ImportedCount);
        Assert.Equal(1, result.SkippedPullRequestsCount);
        Assert.Single(await db.GitHubIssueLinks.ToListAsync());
    }

    [Fact]
    public async Task SyncIssuesFromGitHubAsync_DoesNotSync_InaccessibleProjects()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "secret");
        await db.SaveChangesAsync();

        var otherDeveloper = Guid.NewGuid();
        db.Developers.Add(new Developer { Id = otherDeveloper, FullName = "Other", Activo = true });
        await db.SaveChangesAsync();

        var fakeGitHub = new FakeGitHubApiService();
        fakeGitHub.SetIssues("kodvian", "secret", new GitHubIssueDto
        {
            NodeId = "node-x",
            Number = 1,
            Title = "Should not import",
            State = "open",
            HtmlUrl = "https://github.com/kodvian/secret/issues/1"
        });

        var service = CreateService(db, fakeGitHub);
        var result = await service.SyncIssuesFromGitHubAsync(otherDeveloper, userId);

        Assert.Equal(0, result.RepositoriesSynced);
        Assert.Empty(await db.GitHubIssueLinks.ToListAsync());
    }

    [Fact]
    public async Task SyncIssuesFromGitHubAsync_DoesNotReassignDeveloperId_OnUpdate()
    {
        await using var db = CreateDbContext();
        var (ownerDeveloperId, ownerUserId, projectId) = await SeedBaseAsync(db, gitHubUsername: "devowner");
        LinkGitHub(db, projectId, "kodvian", "alpha");
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = ownerDeveloperId,
            Activo = true
        });

        var otherDeveloperId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        db.Developers.Add(new Developer { Id = otherDeveloperId, FullName = "Other Dev", Activo = true });
        db.Users.Add(new User
        {
            Id = otherUserId,
            FullName = "Other Dev",
            Email = $"other-{otherUserId:N}@kodvian.local",
            PasswordHash = "hash",
            RoleId = KodvianDbContext.DeveloperRoleId,
            DeveloperId = otherDeveloperId,
            Activo = true,
            GitHubConnectedAt = DateTime.UtcNow,
            GitHubAccessTokenEncrypted = "encrypted-token",
            GitHubUsername = "devother"
        });
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = otherDeveloperId,
            Activo = true
        });
        db.GitHubIssueLinks.Add(new GitHubIssueLink
        {
            ProjectId = projectId,
            DeveloperId = ownerDeveloperId,
            GitHubIssueNumber = 1,
            GitHubIssueNodeId = "node-1",
            GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/1",
            Title = "Owned by first dev",
            Status = GitHubIssueStatus.Open,
            AssignedGitHubUsername = "devother",
            Activo = true
        });
        await db.SaveChangesAsync();

        var fakeGitHub = new FakeGitHubApiService();
        fakeGitHub.SetIssues("kodvian", "alpha", new GitHubIssueDto
        {
            NodeId = "node-1",
            Number = 1,
            Title = "Updated by second dev sync",
            State = "closed",
            HtmlUrl = "https://github.com/kodvian/alpha/issues/1",
            AssigneeLogin = "devother"
        });

        var service = CreateService(db, fakeGitHub);
        var result = await service.SyncIssuesFromGitHubAsync(otherDeveloperId, otherUserId);

        Assert.Equal(1, result.UpdatedCount);
        var link = await db.GitHubIssueLinks.SingleAsync();
        Assert.Equal(ownerDeveloperId, link.DeveloperId);
        Assert.Equal("Updated by second dev sync", link.Title);
        Assert.Equal(GitHubIssueStatus.Closed, link.Status);
    }

    [Fact]
    public async Task SyncAfterConnectAsync_Imports_WhenDeveloperIdPresent()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "alpha");
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            Activo = true
        });
        await db.SaveChangesAsync();

        var fakeGitHub = new FakeGitHubApiService();
        fakeGitHub.SetIssues("kodvian", "alpha", new GitHubIssueDto
        {
            NodeId = "node-1",
            Number = 1,
            Title = "Auto sync",
            State = "open",
            HtmlUrl = "https://github.com/kodvian/alpha/issues/1"
        });

        var service = CreateService(db, fakeGitHub);
        await service.SyncAfterConnectAsync(userId, developerId);

        Assert.Single(await db.GitHubIssueLinks.ToListAsync());
    }

    private static GitHubIssueSyncService CreateService(KodvianDbContext db, FakeGitHubApiService gitHub)
    {
        var tokenProvider = new FakeGitHubTokenProvider();
        return new GitHubIssueSyncService(db, gitHub, tokenProvider);
    }

    private static void LinkGitHub(KodvianDbContext db, Guid projectId, string owner, string repo)
    {
        var project = db.Projects.Single(x => x.Id == projectId);
        project.GitHubOwner = owner;
        project.GitHubRepoName = repo;
        project.GitHubRepoId = 100;
        project.GitHubRepoUrl = $"https://github.com/{owner}/{repo}";
    }

    private static async Task<(Guid DeveloperId, Guid UserId, Guid ProjectId)> SeedBaseAsync(
        KodvianDbContext db,
        string gitHubUsername = "devuser")
    {
        var clientId = Guid.NewGuid();
        var developerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Clients.Add(new Client { Id = clientId, CommercialName = "Cliente Demo", Activo = true });
        db.Developers.Add(new Developer { Id = developerId, FullName = "Dev Demo", Activo = true });
        db.Users.Add(new User
        {
            Id = userId,
            FullName = "Dev Demo",
            Email = $"dev-{userId:N}@kodvian.local",
            PasswordHash = "hash",
            RoleId = KodvianDbContext.DeveloperRoleId,
            DeveloperId = developerId,
            Activo = true,
            GitHubConnectedAt = DateTime.UtcNow,
            GitHubAccessTokenEncrypted = "encrypted-token",
            GitHubUsername = gitHubUsername
        });
        db.Projects.Add(new Project
        {
            Id = projectId,
            ClienteId = clientId,
            Nombre = "Proyecto Demo",
            Activo = true,
            Estado = ProjectStatus.EnCurso
        });
        await db.SaveChangesAsync();
        return (developerId, userId, projectId);
    }

    private static KodvianDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<KodvianDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new KodvianDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private sealed class FakeGitHubTokenProvider : IGitHubTokenProvider
    {
        public Task<string> GetValidTokenAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult("gho_test_token");
    }

    private sealed class FakeGitHubApiService : IGitHubApiService
    {
        private readonly Dictionary<string, List<GitHubIssueDto>> _issuesByRepo = new();

        public void SetIssues(string owner, string repo, params GitHubIssueDto[] issues)
        {
            _issuesByRepo[$"{owner}/{repo}"] = issues.ToList();
        }

        public Task<bool> ValidateRepositoryAsync(string owner, string repo, string? token = null, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubIssueDto> CreateIssueAsync(string owner, string repo, CreateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubIssueDto> UpdateIssueAsync(string owner, string repo, int issueNumber, UpdateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<GitHubIssueDto>> ListIssuesAsync(string owner, string repo, ListGitHubIssuesRequest request, string token, CancellationToken cancellationToken = default)
        {
            var key = $"{owner}/{repo}";
            if (!_issuesByRepo.TryGetValue(key, out var issues))
            {
                return Task.FromResult<IReadOnlyList<GitHubIssueDto>>(Array.Empty<GitHubIssueDto>());
            }

            var page = Math.Max(1, request.Page);
            var perPage = Math.Clamp(request.PerPage, 1, 100);
            var paged = issues.Skip((page - 1) * perPage).Take(perPage).ToList();
            return Task.FromResult<IReadOnlyList<GitHubIssueDto>>(paged);
        }

        public Task<GitHubUserDto> GetAuthenticatedUserAsync(string token, CancellationToken cancellationToken = default)
            => Task.FromResult(new GitHubUserDto { Id = 42, Login = "devuser" });

        public Task<GitHubOAuthTokenDto> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
