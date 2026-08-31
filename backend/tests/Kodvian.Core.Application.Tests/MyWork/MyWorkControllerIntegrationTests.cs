using System.Net;
using System.Net.Http.Json;
using Kodvian.Core.Application.Common.Security;
using Kodvian.Core.Application.Tests.Infrastructure;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kodvian.Core.Application.Tests.MyWork;

public class MyWorkControllerIntegrationTests : IClassFixture<KodvianApiFactory>
{
    private readonly KodvianApiFactory _factory;

    public MyWorkControllerIntegrationTests(KodvianApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostIssue_ReturnsForbidden_WithoutIssuesWritePermission()
    {
        await _factory.ResetDatabaseAsync();
        var (developerId, userId, projectId, email) = await _factory.SeedDeveloperWithGitHubProjectAsync();
        var client = _factory.CreateAuthenticatedClient(
            userId,
            email,
            developerId: developerId,
            permissions: [PermissionCodes.DeveloperWorkRead]);

        var response = await client.PostAsJsonAsync("/api/my-work/issues", new
        {
            projectId,
            title = "Nueva tarea"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PatchIssueStatus_ReturnsForbidden_WithoutStatusWritePermission()
    {
        await _factory.ResetDatabaseAsync();
        var projectId = await _factory.SeedLinkedProjectWithIssueAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        var link = await db.GitHubIssueLinks.SingleAsync(x => x.ProjectId == projectId);
        var user = await db.Users.SingleAsync(x => x.DeveloperId == link.DeveloperId);

        var client = _factory.CreateAuthenticatedClient(
            user.Id,
            user.Email,
            developerId: user.DeveloperId,
            permissions: [PermissionCodes.DeveloperWorkRead]);

        var response = await client.PatchAsJsonAsync(
            $"/api/my-work/issues/{link.Id}/status",
            new { status = "Closed" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
