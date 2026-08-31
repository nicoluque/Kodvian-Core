using Kodvian.Core.Application.Auth.Abstractions;
using Kodvian.Core.Application.Auth.Dtos;
using Kodvian.Core.Application.Common.Security;
using Kodvian.Core.Application.Integrations.GitHub;
using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kodvian.Core.Application.Tests.Infrastructure;

public class KodvianApiFactory : WebApplicationFactory<Program>
{
    public const string WebhookSecret = "webhook-test-secret";
    public const string JwtKey = "test-jwt-key-at-least-32-characters-long";
    public const string JwtIssuer = "test-issuer";
    public const string JwtAudience = "test-audience";

    private readonly string _databaseName = $"kodvian-tests-{Guid.NewGuid():N}";
    private readonly TestGitHubApiService _testGitHubApiService = new();

    public TestGitHubApiService TestGitHubApi => _testGitHubApiService;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:Issuer", JwtIssuer);
        builder.UseSetting("Jwt:Audience", JwtAudience);
        builder.UseSetting("SkipDatabaseMigration", "true");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test",
                ["TokenEncryption:Key"] = "local-dev-token-encryption-key-32chars!",
                ["GitHub:Enabled"] = "true",
                ["GitHub:ClientId"] = "test-client-id",
                ["GitHub:ClientSecret"] = "test-client-secret",
                ["GitHub:CallbackUrl"] = "http://localhost/api/profile/github/callback",
                ["GitHub:OAuthScope"] = "read:user repo",
                ["GitHub:FrontendSuccessPath"] = "/mi-perfil?connected=true",
                ["GitHub:WebhookSecret"] = WebhookSecret
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<KodvianDbContext>)
                    || d.ServiceType == typeof(KodvianDbContext)
                    || d.ServiceType == typeof(IGitHubApiService))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<KodvianDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            services.AddSingleton(_testGitHubApiService);
            services.AddSingleton<IGitHubApiService>(_testGitHubApiService);
        });
    }

    public HttpClient CreateAuthenticatedClient(
        Guid userId,
        string email,
        Guid? developerId = null,
        IReadOnlyList<string>? permissions = null)
    {
        var client = CreateClientWithoutRedirects();
        var token = CreateAccessToken(userId, email, developerId: developerId, permissions: permissions);
        client.DefaultRequestHeaders.Add("Cookie", $"auth_token={token}");
        return client;
    }

    public HttpClient CreateClientWithoutRedirects() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    public string CreateAccessToken(
        Guid userId,
        string email,
        string fullName = "Dev User",
        string role = RoleNames.Developer,
        Guid? developerId = null,
        IReadOnlyList<string>? permissions = null)
    {
        using var scope = Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        return tokenService.GenerateToken(new TokenGenerationDto
        {
            UserId = userId,
            Email = email,
            FullName = fullName,
            Role = role,
            DeveloperId = developerId,
            Permissions = permissions ?? RolePermissionMap.GetPermissions(role)
        }).AccessToken;
    }

    public string Sign(string payload) => GitHubWebhookSignatureValidator.ComputeSignature(payload, WebhookSecret);

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task<Guid> SeedUserAsync(string? email = null)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        await db.Database.EnsureCreatedAsync();

        var userId = Guid.NewGuid();
        db.Users.Add(new User
        {
            Id = userId,
            FullName = "Dev User",
            Email = email ?? $"dev-{userId:N}@kodvian.local",
            PasswordHash = "hash",
            RoleId = KodvianDbContext.DeveloperRoleId,
            Activo = true
        });
        await db.SaveChangesAsync();
        return userId;
    }

    public async Task<(Guid DeveloperId, Guid UserId, Guid ProjectId, string Email)> SeedDeveloperWithGitHubProjectAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        await db.Database.EnsureCreatedAsync();

        var clientId = Guid.NewGuid();
        var developerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var email = $"dev-{userId:N}@kodvian.local";

        db.Clients.Add(new Client { Id = clientId, CommercialName = "Cliente Demo", Activo = true });
        db.Developers.Add(new Developer { Id = developerId, FullName = "Dev Demo", Activo = true });
        db.Users.Add(new User
        {
            Id = userId,
            FullName = "Dev Demo",
            Email = email,
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
        await db.SaveChangesAsync();
        return (developerId, userId, projectId, email);
    }

    public async Task<Guid> SeedLinkedProjectWithIssueAsync(GitHubIssueStatus status = GitHubIssueStatus.Open)
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        await db.Database.EnsureCreatedAsync();

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
            ProjectId = projectId,
            DeveloperId = developerId,
            GitHubIssueNumber = 42,
            GitHubIssueNodeId = "node-42",
            GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/42",
            Title = "Issue",
            Status = status,
            Activo = true
        });
        await db.SaveChangesAsync();
        return projectId;
    }
}

public sealed class TestGitHubApiService : IGitHubApiService
{
    public Task<bool> ValidateRepositoryAsync(string owner, string repo, string? token = null, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, string token, CancellationToken cancellationToken = default)
        => Task.FromResult(new GitHubRepositoryDto { Id = 1, FullName = $"{owner}/{repo}" });

    public Task<GitHubIssueDto> CreateIssueAsync(string owner, string repo, CreateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
        => Task.FromResult(new GitHubIssueDto
        {
            NodeId = "node-new",
            Number = 1,
            Title = request.Title,
            State = "open",
            HtmlUrl = $"https://github.com/{owner}/{repo}/issues/1"
        });

    public Task<GitHubIssueDto> UpdateIssueAsync(string owner, string repo, int issueNumber, UpdateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
        => Task.FromResult(new GitHubIssueDto
        {
            NodeId = "node-42",
            Number = issueNumber,
            Title = "Issue",
            State = request.State ?? "open",
            HtmlUrl = $"https://github.com/{owner}/{repo}/issues/{issueNumber}"
        });

    public Task<IReadOnlyList<GitHubIssueDto>> ListIssuesAsync(string owner, string repo, ListGitHubIssuesRequest request, string token, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<GitHubIssueDto>>(Array.Empty<GitHubIssueDto>());

    public Task<GitHubUserDto> GetAuthenticatedUserAsync(string token, CancellationToken cancellationToken = default)
        => Task.FromResult(new GitHubUserDto { Id = 99, Login = "devuser" });

    public Task<GitHubOAuthTokenDto> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
        => Task.FromResult(new GitHubOAuthTokenDto
        {
            AccessToken = "gho_test_token",
            TokenType = "bearer",
            Scope = "read:user,repo"
        });
}
