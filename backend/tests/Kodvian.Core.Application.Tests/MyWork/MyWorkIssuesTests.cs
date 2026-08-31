using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Application.MyWork.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Integrations.GitHub;
using Kodvian.Core.Infrastructure.Persistence;
using Kodvian.Core.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Application.Tests.MyWork;

public class MyWorkIssuesTests
{
    [Fact]
    public async Task GetIssuesAsync_ReturnsIssues_FromAccessibleProjects()
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
            GitHubIssueNumber = 42,
            GitHubIssueNodeId = "node-42",
            GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/42",
            Title = "Fix login bug",
            Status = GitHubIssueStatus.Open,
            Priority = TaskPriority.Alta,
            Activo = true
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetIssuesAsync(developerId, new MyWorkIssueListRequestDto());

        Assert.Single(result.Items);
        Assert.Equal("Fix login bug", result.Items.First().Title);
        Assert.Equal("kodvian/alpha", result.Items.First().RepositoryFullName);
        Assert.Equal("Open", result.Items.First().Status);
    }

    [Fact]
    public async Task GetIssuesAsync_ReturnsEmpty_WhenDeveloperHasNoProjectAccess()
    {
        await using var db = CreateDbContext();
        var (developerId, _, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "alpha");
        db.GitHubIssueLinks.Add(new GitHubIssueLink
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            GitHubIssueNumber = 1,
            GitHubIssueNodeId = "node-1",
            GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/1",
            Title = "Hidden issue",
            Status = GitHubIssueStatus.Open,
            Activo = true
        });
        await db.SaveChangesAsync();

        var otherDeveloper = Guid.NewGuid();
        db.Developers.Add(new Developer { Id = otherDeveloper, FullName = "Other", Activo = true });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetIssuesAsync(otherDeveloper, new MyWorkIssueListRequestDto());

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetOverviewAsync_ReturnsCorrectKpis()
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
        db.GitHubIssueLinks.AddRange(
            new GitHubIssueLink
            {
                ProjectId = projectId,
                DeveloperId = developerId,
                GitHubIssueNumber = 1,
                GitHubIssueNodeId = "node-1",
                GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/1",
                Title = "Open issue",
                Status = GitHubIssueStatus.Open,
                Activo = true
            },
            new GitHubIssueLink
            {
                ProjectId = projectId,
                DeveloperId = developerId,
                GitHubIssueNumber = 2,
                GitHubIssueNodeId = "node-2",
                GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/2",
                Title = "Closed issue",
                Status = GitHubIssueStatus.Closed,
                Activo = true
            });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var overview = await service.GetOverviewAsync(developerId, userId);

        Assert.Equal(1, overview.RepositoryCount);
        Assert.Equal(2, overview.TotalIssuesCount);
        Assert.Equal(1, overview.OpenIssuesCount);
        Assert.False(overview.GitHubNotConnected);
        Assert.Single(overview.Repositories);
        Assert.Equal(2, overview.Issues.Count);
    }

    [Fact]
    public async Task GetIssuesAsync_FiltersByStatus()
    {
        await using var db = CreateDbContext();
        var (developerId, _, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "alpha");
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            Activo = true
        });
        db.GitHubIssueLinks.AddRange(
            new GitHubIssueLink
            {
                ProjectId = projectId,
                DeveloperId = developerId,
                GitHubIssueNumber = 1,
                GitHubIssueNodeId = "node-1",
                GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/1",
                Title = "Open issue",
                Status = GitHubIssueStatus.Open,
                Activo = true
            },
            new GitHubIssueLink
            {
                ProjectId = projectId,
                DeveloperId = developerId,
                GitHubIssueNumber = 2,
                GitHubIssueNodeId = "node-2",
                GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/2",
                Title = "Closed issue",
                Status = GitHubIssueStatus.Closed,
                Activo = true
            });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetIssuesAsync(developerId, new MyWorkIssueListRequestDto { Status = "Open" });

        Assert.Single(result.Items);
        Assert.Equal("Open", result.Items.First().Status);
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
        bool githubConnected = true)
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
            GitHubConnectedAt = githubConnected ? DateTime.UtcNow : null,
            GitHubAccessTokenEncrypted = githubConnected ? "encrypted-token" : null,
            GitHubUsername = githubConnected ? "devdemo" : null
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

    private static MyWorkService CreateService(KodvianDbContext db)
    {
        return new MyWorkService(
            db,
            new NoOpGitHubApiService(),
            new NoOpGitHubTokenProvider(),
            Options.Create(new GitHubOptions()));
    }

    private sealed class NoOpGitHubTokenProvider : IGitHubTokenProvider
    {
        public Task<string> GetValidTokenAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult("gho_test_token");
    }

    private sealed class NoOpGitHubApiService : IGitHubApiService
    {
        public Task<bool> ValidateRepositoryAsync(string owner, string repo, string? token = null, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubIssueDto> CreateIssueAsync(string owner, string repo, CreateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubIssueDto> UpdateIssueAsync(string owner, string repo, int issueNumber, UpdateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<GitHubIssueDto>> ListIssuesAsync(string owner, string repo, ListGitHubIssuesRequest request, string token, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<GitHubIssueDto>>(Array.Empty<GitHubIssueDto>());

        public Task<GitHubUserDto> GetAuthenticatedUserAsync(string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubOAuthTokenDto> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
