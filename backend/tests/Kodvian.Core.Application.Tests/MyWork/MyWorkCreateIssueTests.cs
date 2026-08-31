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

public class MyWorkCreateIssueTests
{
    [Fact]
    public async Task CreateIssueAsync_CreatesInGitHubAndDatabase()
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
        var service = CreateService(db, fakeGitHub);
        var result = await service.CreateIssueAsync(developerId, userId, new CreateMyWorkIssueRequestDto
        {
            ProjectId = projectId,
            Title = "Nueva feature",
            Description = "Detalle de la tarea",
            Priority = "Alta"
        });

        Assert.Equal("Nueva feature", result.Title);
        Assert.Equal("kodvian/alpha", result.RepositoryFullName);
        Assert.Equal("Open", result.Status);
        Assert.Equal("Alta", result.Priority);
        Assert.Single(await db.GitHubIssueLinks.ToListAsync());
        Assert.NotNull(fakeGitHub.LastCreateRequest);
        Assert.Equal("Nueva feature", fakeGitHub.LastCreateRequest.Title);
        Assert.Equal("devdemo", fakeGitHub.LastCreateRequest.Assignees!.Single());
        Assert.Equal("kodvian", fakeGitHub.LastCreateRequest.Labels!.Single());
        Assert.NotNull(fakeGitHub.LastCreateRequest?.Labels);
        Assert.DoesNotContain(fakeGitHub.LastCreateRequest.Labels, label => label.Equals("Alta", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateIssueAsync_Throws_WhenProjectNotAccessible()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db);
        LinkGitHub(db, projectId, "kodvian", "secret");
        await db.SaveChangesAsync();

        var otherDeveloper = Guid.NewGuid();
        db.Developers.Add(new Developer { Id = otherDeveloper, FullName = "Other", Activo = true });
        await db.SaveChangesAsync();

        var service = CreateService(db, new FakeGitHubApiService());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateIssueAsync(otherDeveloper, userId, new CreateMyWorkIssueRequestDto
            {
                ProjectId = projectId,
                Title = "Should fail"
            }));

        Assert.Contains("No tenés acceso", ex.Message);
        Assert.Empty(await db.GitHubIssueLinks.ToListAsync());
    }

    [Fact]
    public async Task CreateIssueAsync_Throws_WhenGitHubNotConnected()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId) = await SeedBaseAsync(db, githubConnected: false);
        LinkGitHub(db, projectId, "kodvian", "alpha");
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            Activo = true
        });
        await db.SaveChangesAsync();

        var service = CreateService(db, new FakeGitHubApiService());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateIssueAsync(developerId, userId, new CreateMyWorkIssueRequestDto
            {
                ProjectId = projectId,
                Title = "Should fail"
            }));

        Assert.Equal(GitHubTokenProvider.ReconnectMessage, ex.Message);
    }

    [Fact]
    public void DeveloperRole_DoesNotIncludeIssuesWrite_ForAnalyst()
    {
        var permissions = Kodvian.Core.Application.Common.Security.RolePermissionMap
            .GetPermissions(Kodvian.Core.Application.Common.Security.RoleNames.Analyst);

        Assert.DoesNotContain(Kodvian.Core.Application.Common.Security.PermissionCodes.DeveloperIssuesWrite, permissions);
    }

    private static MyWorkService CreateService(KodvianDbContext db, FakeGitHubApiService gitHub)
    {
        return new MyWorkService(
            db,
            gitHub,
            new FakeGitHubTokenProvider(),
            Options.Create(new GitHubOptions { DefaultLabel = "kodvian" }));
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

    private sealed class FakeGitHubTokenProvider : IGitHubTokenProvider
    {
        public Task<string> GetValidTokenAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult("gho_test_token");
    }

    private sealed class FakeGitHubApiService : IGitHubApiService
    {
        public CreateGitHubIssueRequest? LastCreateRequest { get; private set; }

        public Task<GitHubIssueDto> CreateIssueAsync(
            string owner,
            string repo,
            CreateGitHubIssueRequest request,
            string token,
            CancellationToken cancellationToken = default)
        {
            LastCreateRequest = request;
            return Task.FromResult(new GitHubIssueDto
            {
                NodeId = "node-new",
                Number = 101,
                Title = request.Title,
                State = "open",
                HtmlUrl = $"https://github.com/{owner}/{repo}/issues/101",
                AssigneeLogin = request.Assignees?.FirstOrDefault()
            });
        }

        public Task<bool> ValidateRepositoryAsync(string owner, string repo, string? token = null, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, string token, CancellationToken cancellationToken = default)
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
