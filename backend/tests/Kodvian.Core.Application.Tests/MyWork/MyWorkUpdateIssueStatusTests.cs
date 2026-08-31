using Kodvian.Core.Application.Integrations.GitHub;
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

public class MyWorkUpdateIssueStatusTests
{
    [Fact]
    public async Task UpdateIssueStatusAsync_ClosesIssue_InGitHubAndDatabase()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId, issueId) = await SeedIssueAsync(db);
        var fakeGitHub = new FakeGitHubApiService();
        var service = CreateService(db, fakeGitHub);

        var result = await service.UpdateIssueStatusAsync(
            developerId,
            userId,
            issueId,
            new UpdateMyWorkIssueStatusRequestDto { Status = "Closed" });

        Assert.NotNull(result);
        Assert.Equal("Closed", result!.Status);
        var link = await db.GitHubIssueLinks.SingleAsync();
        Assert.Equal(GitHubIssueStatus.Closed, link.Status);
        Assert.Equal(SyncDirection.FromKodvian, link.SyncDirection);
        Assert.NotNull(link.LastSyncedAt);
        Assert.Equal("closed", fakeGitHub.LastUpdateRequest?.State);
    }

    [Fact]
    public async Task UpdateIssueStatusAsync_ReopensIssue_InGitHub()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, projectId, issueId) = await SeedIssueAsync(db, GitHubIssueStatus.Closed);
        var fakeGitHub = new FakeGitHubApiService();
        var service = CreateService(db, fakeGitHub);

        var result = await service.UpdateIssueStatusAsync(
            developerId,
            userId,
            issueId,
            new UpdateMyWorkIssueStatusRequestDto { Status = "Open" });

        Assert.NotNull(result);
        Assert.Equal("Open", result!.Status);
        Assert.Equal("open", fakeGitHub.LastUpdateRequest?.State);
    }

    [Fact]
    public async Task UpdateIssueStatusAsync_AllowsUpdate_WhenDeveloperHasProjectAccess()
    {
        await using var db = CreateDbContext();
        var (_, _, projectId, issueId) = await SeedIssueAsync(db);
        var otherDeveloperId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        db.Developers.Add(new Developer { Id = otherDeveloperId, FullName = "Other", Activo = true });
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
        await db.SaveChangesAsync();

        var fakeGitHub = new FakeGitHubApiService();
        var service = CreateService(db, fakeGitHub);
        var result = await service.UpdateIssueStatusAsync(
            otherDeveloperId,
            otherUserId,
            issueId,
            new UpdateMyWorkIssueStatusRequestDto { Status = "Closed" });

        Assert.NotNull(result);
        Assert.Equal("Closed", result!.Status);
        Assert.Equal("closed", fakeGitHub.LastUpdateRequest?.State);
    }

    [Fact]
    public async Task UpdateIssueStatusAsync_ReturnsNull_WhenDeveloperHasNoProjectAccess()
    {
        await using var db = CreateDbContext();
        var (_, userId, _, issueId) = await SeedIssueAsync(db);
        var otherDeveloper = Guid.NewGuid();
        db.Developers.Add(new Developer { Id = otherDeveloper, FullName = "Other", Activo = true });
        await db.SaveChangesAsync();

        var service = CreateService(db, new FakeGitHubApiService());
        var result = await service.UpdateIssueStatusAsync(
            otherDeveloper,
            userId,
            issueId,
            new UpdateMyWorkIssueStatusRequestDto { Status = "Closed" });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateIssueStatusAsync_SkipsGitHub_WhenStatusUnchanged()
    {
        await using var db = CreateDbContext();
        var (developerId, userId, _, issueId) = await SeedIssueAsync(db);
        var fakeGitHub = new FakeGitHubApiService();
        var service = CreateService(db, fakeGitHub);

        var result = await service.UpdateIssueStatusAsync(
            developerId,
            userId,
            issueId,
            new UpdateMyWorkIssueStatusRequestDto { Status = "Open" });

        Assert.NotNull(result);
        Assert.Null(fakeGitHub.LastUpdateRequest);
    }

    [Fact]
    public void GitHubSyncAntiLoop_IgnoresInboundUpdate_WithinWindow()
    {
        var link = new GitHubIssueLink
        {
            SyncDirection = SyncDirection.FromKodvian,
            LastSyncedAt = DateTime.UtcNow.AddSeconds(-10)
        };

        Assert.True(GitHubSyncAntiLoop.ShouldIgnoreInboundUpdate(link, DateTime.UtcNow));
    }

    [Fact]
    public void GitHubSyncAntiLoop_AllowsInboundUpdate_AfterWindow()
    {
        var link = new GitHubIssueLink
        {
            SyncDirection = SyncDirection.FromKodvian,
            LastSyncedAt = DateTime.UtcNow.AddSeconds(-40)
        };

        Assert.False(GitHubSyncAntiLoop.ShouldIgnoreInboundUpdate(link, DateTime.UtcNow));
    }

    private static MyWorkService CreateService(KodvianDbContext db, FakeGitHubApiService gitHub)
    {
        return new MyWorkService(
            db,
            gitHub,
            new FakeGitHubTokenProvider(),
            Options.Create(new GitHubOptions()));
    }

    private static async Task<(Guid DeveloperId, Guid UserId, Guid ProjectId, Guid IssueId)> SeedIssueAsync(
        KodvianDbContext db,
        GitHubIssueStatus status = GitHubIssueStatus.Open)
    {
        var clientId = Guid.NewGuid();
        var developerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var issueId = Guid.NewGuid();

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
            GitHubUsername = "devdemo"
        });
        db.Projects.Add(new Project
        {
            Id = projectId,
            ClienteId = clientId,
            Nombre = "Proyecto Demo",
            Activo = true,
            Estado = ProjectStatus.EnCurso,
            GitHubOwner = "kodvian",
            GitHubRepoName = "alpha",
            GitHubRepoId = 100,
            GitHubRepoUrl = "https://github.com/kodvian/alpha"
        });
        db.ProjectDeveloperAssignments.Add(new ProjectDeveloperAssignment
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            Activo = true
        });
        db.GitHubIssueLinks.Add(new GitHubIssueLink
        {
            Id = issueId,
            ProjectId = projectId,
            DeveloperId = developerId,
            GitHubIssueNumber = 42,
            GitHubIssueNodeId = "node-42",
            GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/42",
            Title = "Fix login bug",
            Status = status,
            Activo = true
        });
        await db.SaveChangesAsync();
        return (developerId, userId, projectId, issueId);
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
        public UpdateGitHubIssueRequest? LastUpdateRequest { get; private set; }

        public Task<GitHubIssueDto> UpdateIssueAsync(
            string owner,
            string repo,
            int issueNumber,
            UpdateGitHubIssueRequest request,
            string token,
            CancellationToken cancellationToken = default)
        {
            LastUpdateRequest = request;
            return Task.FromResult(new GitHubIssueDto
            {
                NodeId = "node-42",
                Number = issueNumber,
                Title = "Fix login bug",
                State = request.State ?? "open",
                HtmlUrl = $"https://github.com/{owner}/{repo}/issues/{issueNumber}"
            });
        }

        public Task<bool> ValidateRepositoryAsync(string owner, string repo, string? token = null, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubIssueDto> CreateIssueAsync(string owner, string repo, CreateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<GitHubIssueDto>> ListIssuesAsync(string owner, string repo, ListGitHubIssuesRequest request, string token, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<GitHubIssueDto>>(Array.Empty<GitHubIssueDto>());

        public Task<GitHubUserDto> GetAuthenticatedUserAsync(string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubOAuthTokenDto> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
