using System.Net;
using System.Text;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Application.Integrations.GitHub.Exceptions;
using Kodvian.Core.Infrastructure.Integrations.GitHub;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Application.Tests.Integrations.GitHub;

public class GitHubApiServiceTests
{
    [Fact]
    public async Task ValidateRepositoryAsync_ReturnsTrue_WhenRepositoryExists()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":1}", Encoding.UTF8, "application/json")
            });
        var service = CreateService(handler, enabled: true, serviceToken: "token");

        var result = await service.ValidateRepositoryAsync("kodvian", "demo");

        Assert.True(result);
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.EndsWith("/repos/kodvian/demo", handler.LastRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task ValidateRepositoryAsync_ReturnsFalse_WhenRepositoryMissing()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService(handler, enabled: true, serviceToken: "token");

        var result = await service.ValidateRepositoryAsync("kodvian", "missing");

        Assert.False(result);
    }

    [Fact]
    public async Task CreateIssueAsync_MapsResponse()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    """
                    {
                      "id": 10,
                      "node_id": "I_kwDOABC",
                      "number": 42,
                      "title": "Fix login",
                      "body": "Details",
                      "state": "open",
                      "html_url": "https://github.com/kodvian/demo/issues/42",
                      "assignee": { "id": 1, "login": "dev1" },
                      "labels": [{ "name": "kodvian" }],
                      "created_at": "2026-08-25T12:00:00Z",
                      "updated_at": "2026-08-25T12:00:00Z"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            });
        var service = CreateService(handler, enabled: true);

        var issue = await service.CreateIssueAsync(
            "kodvian",
            "demo",
            new CreateGitHubIssueRequest
            {
                Title = "Fix login",
                Body = "Details",
                Assignees = ["dev1"],
                Labels = ["kodvian"]
            },
            "user-token");

        Assert.Equal(42, issue.Number);
        Assert.Equal("I_kwDOABC", issue.NodeId);
        Assert.Equal("Fix login", issue.Title);
        Assert.Equal("open", issue.State);
        Assert.Equal("dev1", issue.AssigneeLogin);
        Assert.Contains("kodvian", issue.Labels);
        Assert.False(issue.IsPullRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
    }

    [Fact]
    public async Task ListIssuesAsync_MapsIssuesAndFlagsPullRequests()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    [
                      {
                        "id": 1,
                        "node_id": "I_1",
                        "number": 1,
                        "title": "Issue",
                        "state": "open",
                        "html_url": "https://github.com/kodvian/demo/issues/1",
                        "created_at": "2026-08-25T12:00:00Z",
                        "updated_at": "2026-08-25T12:00:00Z"
                      },
                      {
                        "id": 2,
                        "node_id": "PR_1",
                        "number": 2,
                        "title": "PR",
                        "state": "open",
                        "html_url": "https://github.com/kodvian/demo/pull/2",
                        "pull_request": { "url": "https://api.github.com/repos/kodvian/demo/pulls/2" },
                        "created_at": "2026-08-25T12:00:00Z",
                        "updated_at": "2026-08-25T12:00:00Z"
                      }
                    ]
                    """,
                    Encoding.UTF8,
                    "application/json")
            });
        var service = CreateService(handler, enabled: true);

        var issues = await service.ListIssuesAsync(
            "kodvian",
            "demo",
            new ListGitHubIssuesRequest { Assignee = "dev1", State = "all" },
            "user-token");

        Assert.Equal(2, issues.Count);
        Assert.False(issues[0].IsPullRequest);
        Assert.True(issues[1].IsPullRequest);
        Assert.Contains("assignee=dev1", handler.LastRequest!.RequestUri!.Query);
    }

    [Fact]
    public async Task CreateIssueAsync_ThrowsMappedException_OnForbidden()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        var service = CreateService(handler, enabled: true);

        var exception = await Assert.ThrowsAsync<GitHubApiException>(() =>
            service.CreateIssueAsync("kodvian", "demo", new CreateGitHubIssueRequest { Title = "x" }, "token"));

        Assert.Equal(403, exception.StatusCode);
        Assert.Contains("permisos", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateIssueAsync_ThrowsMappedException_OnUnprocessableEntity()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.UnprocessableEntity));
        var service = CreateService(handler, enabled: true);

        var exception = await Assert.ThrowsAsync<GitHubApiException>(() =>
            service.CreateIssueAsync("kodvian", "demo", new CreateGitHubIssueRequest { Title = "x" }, "token"));

        Assert.Equal(422, exception.StatusCode);
        Assert.Contains("inválidos", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateIssueAsync_ThrowsMappedException_OnRateLimit()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage((HttpStatusCode)429));
        var service = CreateService(handler, enabled: true);

        var exception = await Assert.ThrowsAsync<GitHubApiException>(() =>
            service.CreateIssueAsync("kodvian", "demo", new CreateGitHubIssueRequest { Title = "x" }, "token"));

        Assert.Equal(429, exception.StatusCode);
        Assert.Contains("límite", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateIssueAsync_ThrowsMappedException_OnNotFound()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService(handler, enabled: true);

        var exception = await Assert.ThrowsAsync<GitHubApiException>(() =>
            service.UpdateIssueAsync("kodvian", "demo", 99, new UpdateGitHubIssueRequest { State = "closed" }, "token"));

        Assert.Equal(404, exception.StatusCode);
        Assert.Contains("encontró", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateIssueAsync_ThrowsMappedException_OnUnauthorized()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var service = CreateService(handler, enabled: true);

        var exception = await Assert.ThrowsAsync<GitHubApiException>(() =>
            service.CreateIssueAsync("kodvian", "demo", new CreateGitHubIssueRequest { Title = "x" }, "bad-token"));

        Assert.Equal(401, exception.StatusCode);
        Assert.Contains("autenticación", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetRepositoryAsync_Throws_WhenIntegrationDisabled()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = CreateService(handler, enabled: false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetRepositoryAsync("kodvian", "demo", "token"));

        Assert.Contains("deshabilitada", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static GitHubApiService CreateService(StubHttpMessageHandler handler, bool enabled, string? serviceToken = null)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };

        var options = Options.Create(new GitHubOptions
        {
            Enabled = enabled,
            ClientId = "client",
            ClientSecret = "secret",
            CallbackUrl = "https://localhost/api/profile/github/callback",
            ApiBaseUrl = "https://api.github.com",
            DefaultLabel = "kodvian",
            ServiceToken = serviceToken
        });

        return new GitHubApiService(httpClient, options);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_responder(request));
        }
    }
}
