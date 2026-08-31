using System.Net;
using System.Text;
using System.Text.Json;
using Kodvian.Core.Application.Integrations.GitHub;
using Kodvian.Core.Application.Tests.Infrastructure;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kodvian.Core.Application.Tests.Integrations.GitHub;

public class GitHubWebhookControllerIntegrationTests : IClassFixture<KodvianApiFactory>
{
    private readonly KodvianApiFactory _factory;

    public GitHubWebhookControllerIntegrationTests(KodvianApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostGitHubWebhook_ReturnsUnauthorized_WhenSignatureInvalid()
    {
        var client = _factory.CreateClient();
        var payload = BuildPayload("closed", "closed");

        using var request = CreateWebhookRequest(payload, "sha256=invalid");
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostGitHubWebhook_UpdatesIssue_WhenSignatureValid()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = await _factory.SeedLinkedProjectWithIssueAsync();
        var client = _factory.CreateClient();
        var payload = BuildPayload("closed", "closed");

        using var request = CreateWebhookRequest(payload, _factory.Sign(payload));
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        var link = await db.GitHubIssueLinks.SingleAsync(x => x.ProjectId == projectId);
        Assert.Equal(GitHubIssueStatus.Closed, link.Status);
        Assert.Equal(SyncDirection.FromGitHub, link.SyncDirection);
    }

    [Fact]
    public async Task PostGitHubWebhook_ReopensIssue_WhenActionIsReopened()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = await _factory.SeedLinkedProjectWithIssueAsync(GitHubIssueStatus.Closed);
        var client = _factory.CreateClient();
        var payload = BuildPayload("reopened", "open");

        using var request = CreateWebhookRequest(payload, _factory.Sign(payload));
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        var link = await db.GitHubIssueLinks.SingleAsync(x => x.ProjectId == projectId);
        Assert.Equal(GitHubIssueStatus.Open, link.Status);
        Assert.Equal(SyncDirection.FromGitHub, link.SyncDirection);
    }

    [Fact]
    public async Task PostGitHubWebhook_UpdatesTitle_WhenActionIsEdited()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = await _factory.SeedLinkedProjectWithIssueAsync();
        var client = _factory.CreateClient();
        var payload = BuildPayload("edited", "open", "Updated webhook title");

        using var request = CreateWebhookRequest(payload, _factory.Sign(payload));
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        var link = await db.GitHubIssueLinks.SingleAsync(x => x.ProjectId == projectId);
        Assert.Equal("Updated webhook title", link.Title);
        Assert.Equal(GitHubIssueStatus.Open, link.Status);
        Assert.Equal(SyncDirection.FromGitHub, link.SyncDirection);
    }

    [Fact]
    public async Task PostGitHubWebhook_IgnoresUpdate_WhenAntiLoopActive()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = await _factory.SeedLinkedProjectWithIssueAsync();
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
            var link = await db.GitHubIssueLinks.SingleAsync(x => x.ProjectId == projectId);
            link.SyncDirection = SyncDirection.FromKodvian;
            link.LastSyncedAt = DateTime.UtcNow.AddSeconds(-10);
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var payload = BuildPayload("closed", "closed");

        using var request = CreateWebhookRequest(payload, _factory.Sign(payload));
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var verifyScope = _factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        var updatedLink = await verifyDb.GitHubIssueLinks.SingleAsync(x => x.ProjectId == projectId);
        Assert.Equal(GitHubIssueStatus.Open, updatedLink.Status);
    }

    [Fact]
    public async Task PostGitHubWebhook_ReturnsOk_AndIgnoresUnlinkedRepository()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClient();
        var payload = BuildPayload("closed", "closed");

        using var request = CreateWebhookRequest(payload, _factory.Sign(payload));
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        Assert.Empty(await db.GitHubIssueLinks.ToListAsync());
    }

    private HttpRequestMessage CreateWebhookRequest(string payload, string signature)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/github")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-GitHub-Event", "issues");
        request.Headers.Add("X-Hub-Signature-256", signature);
        return request;
    }

    private static string BuildPayload(string action, string state, string title = "Webhook issue")
    {
        return JsonSerializer.Serialize(new
        {
            action,
            issue = new
            {
                node_id = "node-42",
                number = 42,
                title,
                body = "Updated from webhook",
                state,
                html_url = "https://github.com/kodvian/alpha/issues/42",
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
}
