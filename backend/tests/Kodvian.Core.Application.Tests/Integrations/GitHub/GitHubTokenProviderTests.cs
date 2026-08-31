using Kodvian.Core.Application.Common.Security;
using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Application.Integrations.GitHub.Exceptions;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Infrastructure.Integrations.GitHub;
using Kodvian.Core.Infrastructure.Persistence;
using Kodvian.Core.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Application.Tests.Integrations.GitHub;

public class GitHubTokenProviderTests
{
    [Fact]
    public async Task GetValidTokenAsync_ReturnsDecryptedToken_WhenGitHubAcceptsIt()
    {
        await using var db = CreateDbContext();
        var encryption = CreateEncryption();
        var userId = await SeedConnectedUserAsync(db, encryption, "gho_valid");
        var provider = new GitHubTokenProvider(db, encryption, new FakeGitHubApiService(statusCode: 200));

        var token = await provider.GetValidTokenAsync(userId);

        Assert.Equal("gho_valid", token);
    }

    [Fact]
    public async Task GetValidTokenAsync_Throws_WhenNotConnected()
    {
        await using var db = CreateDbContext();
        var encryption = CreateEncryption();
        var userId = await SeedUserAsync(db);
        var provider = new GitHubTokenProvider(db, encryption, new FakeGitHubApiService(statusCode: 200));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetValidTokenAsync(userId));

        Assert.Equal(GitHubTokenProvider.ReconnectMessage, ex.Message);
    }

    [Fact]
    public async Task GetValidTokenAsync_ClearsConnection_WhenDecryptFails()
    {
        await using var db = CreateDbContext();
        var encryption = CreateEncryption();
        var userId = await SeedUserAsync(db);
        var user = await db.Users.SingleAsync(x => x.Id == userId);
        user.GitHubAccessTokenEncrypted = "not-valid-ciphertext";
        user.GitHubUsername = "devuser";
        user.GitHubUserId = 42;
        user.GitHubConnectedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var provider = new GitHubTokenProvider(db, encryption, new FakeGitHubApiService(statusCode: 200));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetValidTokenAsync(userId));

        Assert.Equal(GitHubTokenProvider.ReconnectMessage, ex.Message);
        user = await db.Users.SingleAsync(x => x.Id == userId);
        Assert.Null(user.GitHubAccessTokenEncrypted);
        Assert.Null(user.GitHubConnectedAt);
        Assert.Null(user.GitHubUsername);
        Assert.Null(user.GitHubUserId);
    }

    [Fact]
    public async Task GetValidTokenAsync_ClearsConnection_WhenGitHubReturns401()
    {
        await using var db = CreateDbContext();
        var encryption = CreateEncryption();
        var userId = await SeedConnectedUserAsync(db, encryption, "gho_revoked");
        var provider = new GitHubTokenProvider(db, encryption, new FakeGitHubApiService(statusCode: 401));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetValidTokenAsync(userId));

        Assert.Equal(GitHubTokenProvider.ReconnectMessage, ex.Message);
        var user = await db.Users.SingleAsync(x => x.Id == userId);
        Assert.Null(user.GitHubAccessTokenEncrypted);
        Assert.Null(user.GitHubConnectedAt);
        Assert.Null(user.GitHubUsername);
        Assert.Null(user.GitHubUserId);
    }

    private static TokenEncryptionService CreateEncryption()
        => new(Options.Create(new TokenEncryptionOptions
        {
            Key = "local-dev-token-encryption-key-32chars!"
        }));

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

    private static async Task<Guid> SeedConnectedUserAsync(KodvianDbContext db, ITokenEncryptionService encryption, string plainToken)
    {
        var userId = await SeedUserAsync(db);
        var user = await db.Users.SingleAsync(x => x.Id == userId);
        user.GitHubAccessTokenEncrypted = encryption.Encrypt(plainToken);
        user.GitHubUsername = "devuser";
        user.GitHubUserId = 42;
        user.GitHubConnectedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return userId;
    }

    private sealed class FakeGitHubApiService(int statusCode) : IGitHubApiService
    {
        public Task<bool> ValidateRepositoryAsync(string owner, string repo, string? token = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubIssueDto> CreateIssueAsync(string owner, string repo, CreateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubIssueDto> UpdateIssueAsync(string owner, string repo, int issueNumber, UpdateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<GitHubIssueDto>> ListIssuesAsync(string owner, string repo, ListGitHubIssuesRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubUserDto> GetAuthenticatedUserAsync(string token, CancellationToken cancellationToken = default)
        {
            if (statusCode == 401)
            {
                throw new GitHubApiException("Unauthorized", 401);
            }

            return Task.FromResult(new GitHubUserDto { Id = 42, Login = "devuser" });
        }

        public Task<GitHubOAuthTokenDto> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
