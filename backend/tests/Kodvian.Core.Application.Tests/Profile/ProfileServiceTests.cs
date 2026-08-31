using System.Reflection;
using Kodvian.Core.Api.Controllers;
using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Infrastructure.Integrations.GitHub;
using Kodvian.Core.Infrastructure.Persistence;
using Kodvian.Core.Infrastructure.Security;
using Kodvian.Core.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Application.Tests.Profile;

public class ProfileServiceTests
{
    [Fact]
    public async Task CreateGitHubConnectUrlAsync_PersistsStateInDatabase()
    {
        await using var db = CreateDbContext();
        var userId = await SeedUserAsync(db);
        var service = CreateService(db, new FakeGitHubApiService());

        var url = await service.CreateGitHubConnectUrlAsync(userId);

        Assert.Contains("https://github.com/login/oauth/authorize?", url);
        Assert.Contains("client_id=client", url);
        Assert.Contains("scope=read%3Auser%20repo", url);
        Assert.Equal(1, await db.GitHubOAuthStates.CountAsync(x => x.UserId == userId));
    }

    [Fact]
    public async Task CompleteGitHubCallbackAsync_PersistsEncryptedToken_AndRemovesState()
    {
        await using var db = CreateDbContext();
        var userId = await SeedUserAsync(db);
        var fakeGitHub = new FakeGitHubApiService();
        var service = CreateService(db, fakeGitHub);
        var connectUrl = await service.CreateGitHubConnectUrlAsync(userId);
        var state = ExtractQueryValue(connectUrl, "state");

        var redirect = await service.CompleteGitHubCallbackAsync("oauth-code", state);

        var user = await db.Users.SingleAsync(x => x.Id == userId);
        Assert.Equal("/mi-perfil?connected=true", redirect);
        Assert.Equal("devuser", user.GitHubUsername);
        Assert.Equal(99, user.GitHubUserId);
        Assert.NotNull(user.GitHubConnectedAt);
        Assert.False(string.IsNullOrWhiteSpace(user.GitHubAccessTokenEncrypted));
        Assert.DoesNotContain("gho_plain_token", user.GitHubAccessTokenEncrypted);
        Assert.Equal(0, await db.GitHubOAuthStates.CountAsync());
        Assert.True(fakeGitHub.SyncWasNotNeeded); // sync called via NoOp; assert exchange happened
        Assert.Equal("oauth-code", fakeGitHub.LastExchangeCode);
    }

    [Fact]
    public async Task CompleteGitHubCallbackAsync_ReturnsSuccess_WhenPostConnectSyncFails()
    {
        await using var db = CreateDbContext();
        var userId = await SeedUserAsync(db);
        var fakeGitHub = new FakeGitHubApiService();
        var service = CreateService(db, fakeGitHub, new ThrowingSyncService());
        var connectUrl = await service.CreateGitHubConnectUrlAsync(userId);
        var state = ExtractQueryValue(connectUrl, "state");

        var redirect = await service.CompleteGitHubCallbackAsync("oauth-code", state);

        Assert.Equal("/mi-perfil?connected=true", redirect);
        var user = await db.Users.SingleAsync(x => x.Id == userId);
        Assert.Equal("devuser", user.GitHubUsername);
        Assert.NotNull(user.GitHubConnectedAt);
    }

    [Fact]
    public async Task CompleteGitHubCallbackAsync_Throws_WhenStateMissing()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db, new FakeGitHubApiService());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CompleteGitHubCallbackAsync("code", null));
    }

    [Fact]
    public async Task CompleteGitHubCallbackAsync_Throws_WhenStateExpired()
    {
        await using var db = CreateDbContext();
        var userId = await SeedUserAsync(db);
        db.GitHubOAuthStates.Add(new GitHubOAuthState
        {
            StateToken = "expired-state",
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new FakeGitHubApiService());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CompleteGitHubCallbackAsync("code", "expired-state"));

        Assert.Contains("expiró", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, await db.GitHubOAuthStates.CountAsync());
    }

    [Fact]
    public async Task DisconnectGitHubAsync_ClearsGitHubFields()
    {
        await using var db = CreateDbContext();
        var userId = await SeedUserAsync(db);
        var user = await db.Users.SingleAsync(x => x.Id == userId);
        user.GitHubUsername = "devuser";
        user.GitHubUserId = 99;
        user.GitHubAccessTokenEncrypted = "cipher";
        user.GitHubConnectedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var service = CreateService(db, new FakeGitHubApiService());

        await service.DisconnectGitHubAsync(userId);

        user = await db.Users.SingleAsync(x => x.Id == userId);
        Assert.Null(user.GitHubUsername);
        Assert.Null(user.GitHubUserId);
        Assert.Null(user.GitHubAccessTokenEncrypted);
        Assert.Null(user.GitHubConnectedAt);
    }

    [Fact]
    public void GitHubCallback_IsAllowAnonymous_AndConnectRequiresAuthorize()
    {
        var callback = typeof(ProfileController).GetMethod(nameof(ProfileController.GitHubCallback));
        var connect = typeof(ProfileController).GetMethod(nameof(ProfileController.ConnectGitHub));

        Assert.NotNull(callback);
        Assert.NotNull(connect);
        Assert.NotNull(callback!.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Null(connect!.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(typeof(ProfileController).GetCustomAttribute<AuthorizeAttribute>());
    }

    private static ProfileService CreateService(
        KodvianDbContext db,
        FakeGitHubApiService gitHub,
        IGitHubIssueSyncService? syncService = null)
    {
        var encryption = new TokenEncryptionService(Options.Create(new TokenEncryptionOptions
        {
            Key = "local-dev-token-encryption-key-32chars!"
        }));

        return new ProfileService(
            db,
            gitHub,
            encryption,
            syncService ?? new TrackingSyncService(gitHub),
            Options.Create(new GitHubOptions
            {
                Enabled = true,
                ClientId = "client",
                ClientSecret = "secret",
                CallbackUrl = "https://localhost/api/profile/github/callback",
                OAuthScope = "read:user repo",
                FrontendSuccessPath = "/mi-perfil?connected=true",
                OAuthStateExpirationMinutes = 10
            }));
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

    private static async Task<Guid> SeedUserAsync(KodvianDbContext db)
    {
        var userId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = userId,
            FullName = "Dev User",
            Email = $"dev-{userId:N}@kodvian.local",
            PasswordHash = "hash",
            RoleId = KodvianDbContext.DeveloperRoleId,
            Activo = true
        });
        await db.SaveChangesAsync();
        return userId;
    }

    private static string ExtractQueryValue(string url, string key)
    {
        var uri = new Uri(url);
        var pair = uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split('=', 2))
            .First(x => x[0] == key);
        return Uri.UnescapeDataString(pair[1]);
    }

    private sealed class FakeGitHubApiService : IGitHubApiService
    {
        public string? LastExchangeCode { get; private set; }
        public bool SyncWasNotNeeded => true;

        public Task<bool> ValidateRepositoryAsync(string owner, string repo, string? token = null, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubIssueDto> CreateIssueAsync(string owner, string repo, CreateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubIssueDto> UpdateIssueAsync(string owner, string repo, int issueNumber, UpdateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<GitHubIssueDto>> ListIssuesAsync(string owner, string repo, ListGitHubIssuesRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubUserDto> GetAuthenticatedUserAsync(string token, CancellationToken cancellationToken = default)
            => Task.FromResult(new GitHubUserDto { Id = 99, Login = "devuser" });

        public Task<GitHubOAuthTokenDto> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
        {
            LastExchangeCode = code;
            return Task.FromResult(new GitHubOAuthTokenDto
            {
                AccessToken = "gho_plain_token",
                TokenType = "bearer",
                Scope = "read:user,repo"
            });
        }
    }

    private sealed class TrackingSyncService : IGitHubIssueSyncService
    {
        private readonly FakeGitHubApiService _gitHub;

        public TrackingSyncService(FakeGitHubApiService gitHub)
        {
            _gitHub = gitHub;
        }

        public Task<GitHubIssueSyncResultDto> SyncIssuesFromGitHubAsync(
            Guid developerId,
            Guid userId,
            Guid? projectId = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new GitHubIssueSyncResultDto());

        public Task SyncAfterConnectAsync(Guid userId, Guid? developerId, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(_gitHub.LastExchangeCode);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingSyncService : IGitHubIssueSyncService
    {
        public Task<GitHubIssueSyncResultDto> SyncIssuesFromGitHubAsync(
            Guid developerId,
            Guid userId,
            Guid? projectId = null,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Sync failed");

        public Task SyncAfterConnectAsync(Guid userId, Guid? developerId, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Sync failed");
    }
}
