using System.Text;
using System.Text.Json;
using Kodvian.Core.Application.Integrations.GitHub;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Integrations.GitHub;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Application.Tests.Integrations.GitHub;

public class GitHubWebhookSignatureValidatorTests
{
    [Fact]
    public void IsValid_ReturnsTrue_ForMatchingSignature()
    {
        const string payload = """{"action":"closed"}""";
        const string secret = "webhook-secret";

        var signature = GitHubWebhookSignatureValidator.ComputeSignature(payload, secret);

        Assert.True(GitHubWebhookSignatureValidator.IsValid(payload, signature, secret));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForInvalidSignature()
    {
        const string payload = """{"action":"closed"}""";

        Assert.False(GitHubWebhookSignatureValidator.IsValid(payload, "sha256=deadbeef", "webhook-secret"));
    }
}

public class GitHubWebhookServiceTests
{
    [Fact]
    public async Task HandleIssueEventAsync_UpdatesExistingIssue_OnClosed()
    {
        await using var db = CreateDbContext();
        var projectId = await SeedLinkedProjectAsync(db);
        var developerId = await db.Developers.Select(x => x.Id).SingleAsync();
        db.GitHubIssueLinks.Add(new GitHubIssueLink
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            GitHubIssueNumber = 42,
            GitHubIssueNodeId = "node-42",
            GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/42",
            Title = "Old title",
            Status = GitHubIssueStatus.Open,
            Activo = true
        });
        await db.SaveChangesAsync();

        var service = new GitHubWebhookService(db);
        var result = await service.HandleIssueEventAsync("issues", BuildPayload("closed", "closed"));

        Assert.True(result.Processed);
        var link = await db.GitHubIssueLinks.SingleAsync();
        Assert.Equal(GitHubIssueStatus.Closed, link.Status);
        Assert.Equal(SyncDirection.FromGitHub, link.SyncDirection);
        Assert.Equal("Webhook issue", link.Title);
    }

    [Fact]
    public async Task HandleIssueEventAsync_Ignores_UnlinkedRepository()
    {
        await using var db = CreateDbContext();
        var service = new GitHubWebhookService(db);

        var result = await service.HandleIssueEventAsync("issues", BuildPayload("closed", "closed"));

        Assert.True(result.Ignored);
        Assert.Empty(await db.GitHubIssueLinks.ToListAsync());
    }

    [Fact]
    public async Task HandleIssueEventAsync_Ignores_WhenAntiLoopActive()
    {
        await using var db = CreateDbContext();
        var projectId = await SeedLinkedProjectAsync(db);
        var developerId = await db.Developers.Select(x => x.Id).SingleAsync();
        db.GitHubIssueLinks.Add(new GitHubIssueLink
        {
            ProjectId = projectId,
            DeveloperId = developerId,
            GitHubIssueNumber = 42,
            GitHubIssueNodeId = "node-42",
            GitHubIssueUrl = "https://github.com/kodvian/alpha/issues/42",
            Title = "Issue",
            Status = GitHubIssueStatus.Open,
            SyncDirection = SyncDirection.FromKodvian,
            LastSyncedAt = DateTime.UtcNow,
            Activo = true
        });
        await db.SaveChangesAsync();

        var service = new GitHubWebhookService(db);
        var result = await service.HandleIssueEventAsync("issues", BuildPayload("closed", "closed"));

        Assert.True(result.Ignored);
        Assert.Equal(GitHubIssueStatus.Open, (await db.GitHubIssueLinks.SingleAsync()).Status);
    }

    [Fact]
    public async Task HandleIssueEventAsync_ImportsIssue_OnOpened_WhenAssigneeMatches()
    {
        await using var db = CreateDbContext();
        await SeedLinkedProjectAsync(db);
        var service = new GitHubWebhookService(db);

        var result = await service.HandleIssueEventAsync("issues", BuildPayload("opened", "open", "node-new", 99));

        Assert.True(result.Processed);
        Assert.Single(await db.GitHubIssueLinks.ToListAsync());
    }

    [Fact]
    public async Task HandleIssueEventAsync_IgnoresPullRequests()
    {
        await using var db = CreateDbContext();
        await SeedLinkedProjectAsync(db);
        var service = new GitHubWebhookService(db);

        var json = JsonSerializer.Serialize(new
        {
            action = "opened",
            issue = new
            {
                node_id = "node-pr",
                number = 100,
                title = "PR",
                body = "body",
                state = "open",
                html_url = "https://github.com/kodvian/alpha/pull/100",
                pull_request = new { },
                assignee = new { login = "devdemo" }
            },
            repository = new
            {
                name = "alpha",
                full_name = "kodvian/alpha",
                owner = new { login = "kodvian" }
            }
        });

        var result = await service.HandleIssueEventAsync("issues", json);

        Assert.True(result.Ignored);
        Assert.Empty(await db.GitHubIssueLinks.ToListAsync());
    }

    private static async Task<Guid> SeedLinkedProjectAsync(KodvianDbContext db)
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
        return projectId;
    }

    private static string BuildPayload(string action, string state, string nodeId = "node-42", int number = 42)
    {
        return JsonSerializer.Serialize(new
        {
            action,
            issue = new
            {
                node_id = nodeId,
                number,
                title = "Webhook issue",
                body = "Updated from webhook",
                state,
                html_url = $"https://github.com/kodvian/alpha/issues/{number}",
                assignee = new { login = "devdemo" }
            },
            repository = new
            {
                name = "alpha",
                full_name = "kodvian/alpha",
                owner = new { login = "kodvian" }
            }
        });
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
}
