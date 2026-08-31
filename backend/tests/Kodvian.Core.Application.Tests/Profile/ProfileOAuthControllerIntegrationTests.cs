using System.Net;
using Kodvian.Core.Application.Tests.Infrastructure;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kodvian.Core.Application.Tests.Profile;

public class ProfileOAuthControllerIntegrationTests : IClassFixture<KodvianApiFactory>
{
    private readonly KodvianApiFactory _factory;

    public ProfileOAuthControllerIntegrationTests(KodvianApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetGitHubConnect_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/profile/github/connect");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetGitHubConnect_RedirectsToGitHub_WhenAuthenticated()
    {
        await _factory.ResetDatabaseAsync();
        var userId = await _factory.SeedUserAsync();
        var email = $"dev-{userId:N}@kodvian.local";
        var client = _factory.CreateAuthenticatedClient(userId, email);

        var response = await client.GetAsync("/api/profile/github/connect");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.StartsWith("https://github.com/login/oauth/authorize", location, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("client_id=test-client-id", location, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("state=", location, StringComparison.OrdinalIgnoreCase);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        Assert.Equal(1, await db.GitHubOAuthStates.CountAsync(x => x.UserId == userId));
    }

    [Fact]
    public async Task GetGitHubCallback_AllowsAnonymous_AndRedirectsOnSuccess()
    {
        await _factory.ResetDatabaseAsync();
        var userId = await _factory.SeedUserAsync();
        var client = _factory.CreateClientWithoutRedirects();
        var state = await CreateOAuthStateAsync(userId);

        var response = await client.GetAsync($"/api/profile/github/callback?code=oauth-code&state={Uri.EscapeDataString(state)}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.Contains("/mi-perfil?connected=true", location, StringComparison.OrdinalIgnoreCase);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<KodvianDbContext>();
        var user = await db.Users.SingleAsync(x => x.Id == userId);
        Assert.Equal("devuser", user.GitHubUsername);
        Assert.Equal(99, user.GitHubUserId);
        Assert.NotNull(user.GitHubConnectedAt);
        Assert.False(string.IsNullOrWhiteSpace(user.GitHubAccessTokenEncrypted));
        Assert.DoesNotContain("gho_test_token", user.GitHubAccessTokenEncrypted);
        Assert.Equal(0, await db.GitHubOAuthStates.CountAsync());
    }

    [Fact]
    public async Task GetGitHubCallback_RedirectsToOAuthError_WhenStateInvalid()
    {
        await _factory.ResetDatabaseAsync();
        var client = _factory.CreateClientWithoutRedirects();

        var response = await client.GetAsync("/api/profile/github/callback?code=oauth-code&state=invalid-state");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.NotNull(location);
        Assert.Contains("/mi-perfil?connected=false&error=oauth", location, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> CreateOAuthStateAsync(Guid userId)
    {
        var email = $"dev-{userId:N}@kodvian.local";
        var connectClient = _factory.CreateAuthenticatedClient(userId, email);

        var connectResponse = await connectClient.GetAsync("/api/profile/github/connect");
        var location = connectResponse.Headers.Location?.ToString()
            ?? throw new InvalidOperationException("Connect did not return a redirect location.");

        var statePair = new Uri(location).Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split('=', 2))
            .First(x => x[0] == "state");

        return Uri.UnescapeDataString(statePair[1]);
    }
}
