using System.Reflection;
using Kodvian.Core.Api.Controllers;
using Kodvian.Core.Application.Common.Files;
using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Application.Projects.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Infrastructure.Integrations.GitHub;
using Kodvian.Core.Infrastructure.Persistence;
using Kodvian.Core.Infrastructure.Services;
using Kodvian.Core.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Application.Tests.Projects;

public class ProjectGitHubLinkTests
{
    [Fact]
    public async Task LinkGitHubRepositoryAsync_PersistsRepoMetadata()
    {
        await using var db = CreateDbContext();
        var projectId = await SeedProjectAsync(db);
        var service = CreateService(db, new FakeGitHubApiService(exists: true));

        var result = await service.LinkGitHubRepositoryAsync(projectId, new LinkGitHubRepositoryRequestDto
        {
            Owner = "kodvian",
            Repo = "core"
        });

        Assert.NotNull(result);
        Assert.True(result!.HasGitHubRepository);
        Assert.Equal("kodvian", result.GitHubOwner);
        Assert.Equal("core", result.GitHubRepoName);
        Assert.Equal(123, result.GitHubRepoId);
        Assert.Equal("https://github.com/kodvian/core", result.GitHubRepoUrl);
    }

    [Fact]
    public async Task LinkGitHubRepositoryAsync_Throws_WhenRepoAlreadyLinked()
    {
        await using var db = CreateDbContext();
        var firstId = await SeedProjectAsync(db, "Alpha");
        var secondId = await SeedProjectAsync(db, "Beta");
        var service = CreateService(db, new FakeGitHubApiService(exists: true));

        await service.LinkGitHubRepositoryAsync(firstId, new LinkGitHubRepositoryRequestDto
        {
            Owner = "kodvian",
            Repo = "core"
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.LinkGitHubRepositoryAsync(secondId, new LinkGitHubRepositoryRequestDto
            {
                Owner = "kodvian",
                Repo = "core"
            }));

        Assert.Contains("ya está vinculado", ex.Message);
    }

    [Fact]
    public async Task LinkGitHubRepositoryAsync_Throws_WhenRepoMissing()
    {
        await using var db = CreateDbContext();
        var projectId = await SeedProjectAsync(db);
        var service = CreateService(db, new FakeGitHubApiService(exists: false));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.LinkGitHubRepositoryAsync(projectId, new LinkGitHubRepositoryRequestDto
            {
                Owner = "kodvian",
                Repo = "missing"
            }));

        Assert.Contains("no existe", ex.Message);
    }

    [Fact]
    public async Task UnlinkGitHubRepositoryAsync_ClearsFields()
    {
        await using var db = CreateDbContext();
        var projectId = await SeedProjectAsync(db);
        var service = CreateService(db, new FakeGitHubApiService(exists: true));

        await service.LinkGitHubRepositoryAsync(projectId, new LinkGitHubRepositoryRequestDto
        {
            Owner = "kodvian",
            Repo = "core"
        });

        var result = await service.UnlinkGitHubRepositoryAsync(projectId);

        Assert.NotNull(result);
        Assert.False(result!.HasGitHubRepository);
        Assert.Null(result.GitHubOwner);
        Assert.Null(result.GitHubRepoName);
    }

    [Fact]
    public async Task ValidateGitHubRepositoryAsync_ReturnsAvailableMessage()
    {
        await using var db = CreateDbContext();
        var projectId = await SeedProjectAsync(db);
        var service = CreateService(db, new FakeGitHubApiService(exists: true));

        var result = await service.ValidateGitHubRepositoryAsync(projectId, new LinkGitHubRepositoryRequestDto
        {
            Owner = "kodvian",
            Repo = "core"
        });

        Assert.True(result.Exists);
        Assert.Equal("kodvian/core", result.FullName);
        Assert.Contains("disponible", result.Message);
    }

    [Fact]
    public async Task ValidateGitHubRepositoryAsync_Throws_WhenGitHubIntegrationDisabled()
    {
        await using var db = CreateDbContext();
        var projectId = await SeedProjectAsync(db);
        var gitHub = new GitHubApiService(
            new HttpClient { BaseAddress = new Uri("https://api.github.com/") },
            Options.Create(new GitHubOptions
            {
                Enabled = false,
                ServiceToken = "ghs_service_token"
            }));
        var service = new ProjectService(
            db,
            new NoOpFileStorageService(),
            Options.Create(new StorageOptions()),
            gitHub,
            Options.Create(new GitHubOptions
            {
                Enabled = false,
                ServiceToken = "ghs_service_token"
            }));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ValidateGitHubRepositoryAsync(projectId, new LinkGitHubRepositoryRequestDto
            {
                Owner = "kodvian",
                Repo = "core"
            }));

        Assert.Contains("deshabilitada", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GitHubRepoEndpoints_RequireAdministratorOnly()
    {
        AssertHasAdministratorOnly(nameof(ProjectsController.LinkGitHubRepository));
        AssertHasAdministratorOnly(nameof(ProjectsController.UnlinkGitHubRepository));
        AssertHasAdministratorOnly(nameof(ProjectsController.ValidateGitHubRepository));
    }

    private static void AssertHasAdministratorOnly(string methodName)
    {
        var method = typeof(ProjectsController).GetMethod(methodName);
        Assert.NotNull(method);
        var authorize = method!.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorize);
        Assert.Equal("AdministratorOnly", authorize!.Policy);
    }

    private static ProjectService CreateService(KodvianDbContext db, IGitHubApiService gitHub)
    {
        return new ProjectService(
            db,
            new NoOpFileStorageService(),
            Options.Create(new StorageOptions()),
            gitHub,
            Options.Create(new GitHubOptions
            {
                Enabled = true,
                ServiceToken = "ghs_service_token"
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

    private static async Task<Guid> SeedProjectAsync(KodvianDbContext db, string name = "Demo")
    {
        var clientId = Guid.NewGuid();
        db.Clients.Add(new Client
        {
            Id = clientId,
            CommercialName = $"Cliente {name}",
            Activo = true
        });

        var projectId = Guid.NewGuid();
        db.Projects.Add(new Project
        {
            Id = projectId,
            ClienteId = clientId,
            Nombre = name,
            Activo = true
        });
        await db.SaveChangesAsync();
        return projectId;
    }

    private sealed class FakeGitHubApiService(bool exists) : IGitHubApiService
    {
        public Task<bool> ValidateRepositoryAsync(string owner, string repo, string? token = null, CancellationToken cancellationToken = default)
            => Task.FromResult(exists);

        public Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, string token, CancellationToken cancellationToken = default)
            => Task.FromResult(new GitHubRepositoryDto
            {
                Id = 123,
                Name = "core",
                FullName = "kodvian/core",
                OwnerLogin = "kodvian",
                HtmlUrl = "https://github.com/kodvian/core",
                Private = false
            });

        public Task<GitHubIssueDto> CreateIssueAsync(string owner, string repo, CreateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubIssueDto> UpdateIssueAsync(string owner, string repo, int issueNumber, UpdateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<GitHubIssueDto>> ListIssuesAsync(string owner, string repo, ListGitHubIssuesRequest request, string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubUserDto> GetAuthenticatedUserAsync(string token, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<GitHubOAuthTokenDto> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class NoOpFileStorageService : IFileStorageService
    {
        public Task<string> SaveAsync(byte[] content, string extension, CancellationToken cancellationToken = default)
            => Task.FromResult($"noop/{Guid.NewGuid():N}{extension}");

        public Task<byte[]> ReadAsync(string storagePath, CancellationToken cancellationToken = default)
            => Task.FromResult(Array.Empty<byte>());

        public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
