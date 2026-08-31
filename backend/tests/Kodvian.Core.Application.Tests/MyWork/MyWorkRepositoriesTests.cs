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

public class MyWorkRepositoriesTests
{
    [Fact]
    public async Task GetAssignedRepositoriesAsync_ReturnsRepo_WhenDeveloperHasAssignment()
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

        var service = CreateService(db);
        var result = await service.GetAssignedRepositoriesAsync(developerId, userId, new MyWorkRepositoryListRequestDto());

        Assert.Single(result.Items);
        Assert.Equal("kodvian", result.Items.First().GitHubOwner);
        Assert.Equal("alpha", result.Items.First().GitHubRepoName);
        Assert.False(result.GitHubNotConnected);
    }

    [Fact]
    public async Task GetAssignedRepositoriesAsync_ReturnsRepo_WhenDeveloperHasOnlyContract()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "beta");
        db.ProjectDeveloperContracts.Add(new ProjectDeveloperContract
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            PaymentMode = ContractPaymentMode.Percentage,
            Percentage = 50,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Activo = true
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetAssignedRepositoriesAsync(developerId, userId, new MyWorkRepositoryListRequestDto());

        Assert.Single(result.Items);
        Assert.Equal("beta", result.Items.First().GitHubRepoName);
    }

    [Fact]
    public async Task GetAssignedRepositoriesAsync_ReturnsRepo_WhenDeveloperHasOnlyActiveTask()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "epsilon");
        db.Tasks.Add(new TaskItem
        {
            ProyectoId = projectId,
            DeveloperId = developerId,
            CreadoPorId = userId,
            Titulo = "Tarea asignada",
            Activo = true
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetAssignedRepositoriesAsync(developerId, userId, new MyWorkRepositoryListRequestDto());

        Assert.Single(result.Items);
        Assert.Equal("kodvian", result.Items.First().GitHubOwner);
        Assert.Equal("epsilon", result.Items.First().GitHubRepoName);
    }

    [Fact]
    public async Task GetAssignedRepositoriesAsync_ReturnsEmpty_WhenDeveloperHasNoAccess()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "gamma");
        await db.SaveChangesAsync();

        var otherDeveloper = Guid.NewGuid();
        db.Developers.Add(new Developer { Id = otherDeveloper, FullName = "Other", Activo = true });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetAssignedRepositoriesAsync(otherDeveloper, userId, new MyWorkRepositoryListRequestDto());

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetAssignedRepositoriesAsync_ExcludesProjectsWithoutGitHubRepo()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db);
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            Activo = true
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetAssignedRepositoriesAsync(developerId, userId, new MyWorkRepositoryListRequestDto());

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetAssignedRepositoriesAsync_SetsGitHubNotConnected_WhenUserHasNoToken()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db, githubConnected: false);
        LinkGitHub(db, projectId, "kodvian", "delta");
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            Activo = true
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetAssignedRepositoriesAsync(developerId, userId, new MyWorkRepositoryListRequestDto());

        Assert.True(result.GitHubNotConnected);
        Assert.Single(result.Items);
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
