using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Application.Integrations.GitHub.Exceptions;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Infrastructure.Integrations.GitHub;

public class GitHubApiService : IGitHubApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly GitHubOptions _options;

    public GitHubApiService(HttpClient httpClient, IOptions<GitHubOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<bool> ValidateRepositoryAsync(string owner, string repo, string? token = null, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        var accessToken = ResolveToken(token);

        using var request = CreateApiRequest(HttpMethod.Get, $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}", accessToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return true;
    }

    public async Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, string token, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        using var request = CreateApiRequest(HttpMethod.Get, $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}", token);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadRequiredAsync<GitHubRepositoryPayload>(response, cancellationToken);

        return MapRepository(payload);
    }

    public async Task<GitHubIssueDto> CreateIssueAsync(string owner, string repo, CreateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Title);

        var body = new
        {
            title = request.Title.Trim(),
            body = request.Body,
            assignees = request.Assignees,
            labels = request.Labels
        };

        using var httpRequest = CreateApiRequest(HttpMethod.Post, $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/issues", token);
        httpRequest.Content = JsonContent.Create(body, options: JsonOptions);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var payload = await ReadRequiredAsync<GitHubIssuePayload>(response, cancellationToken);
        return MapIssue(payload);
    }

    public async Task<GitHubIssueDto> UpdateIssueAsync(string owner, string repo, int issueNumber, UpdateGitHubIssueRequest request, string token, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        if (issueNumber <= 0)
        {
            throw new ArgumentException("El número de issue es inválido.", nameof(issueNumber));
        }

        var body = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            body["title"] = request.Title.Trim();
        }

        if (request.Body is not null)
        {
            body["body"] = request.Body;
        }

        if (!string.IsNullOrWhiteSpace(request.State))
        {
            body["state"] = request.State.Trim().ToLowerInvariant();
        }

        if (request.Assignees is not null)
        {
            body["assignees"] = request.Assignees;
        }

        if (request.Labels is not null)
        {
            body["labels"] = request.Labels;
        }

        using var httpRequest = CreateApiRequest(
            HttpMethod.Patch,
            $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/issues/{issueNumber}",
            token);
        httpRequest.Content = JsonContent.Create(body, options: JsonOptions);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var payload = await ReadRequiredAsync<GitHubIssuePayload>(response, cancellationToken);
        return MapIssue(payload);
    }

    public async Task<IReadOnlyList<GitHubIssueDto>> ListIssuesAsync(string owner, string repo, ListGitHubIssuesRequest request, string token, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var query = new List<string>
        {
            $"state={Uri.EscapeDataString(request.State)}",
            $"per_page={Math.Clamp(request.PerPage, 1, 100)}",
            $"page={Math.Max(1, request.Page)}"
        };

        if (!string.IsNullOrWhiteSpace(request.Assignee))
        {
            query.Add($"assignee={Uri.EscapeDataString(request.Assignee)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Labels))
        {
            query.Add($"labels={Uri.EscapeDataString(request.Labels)}");
        }

        using var httpRequest = CreateApiRequest(
            HttpMethod.Get,
            $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/issues?{string.Join('&', query)}",
            token);
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var payload = await ReadRequiredAsync<List<GitHubIssuePayload>>(response, cancellationToken);
        return payload.Select(MapIssue).ToList();
    }

    public async Task<GitHubUserDto> GetAuthenticatedUserAsync(string token, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        using var request = CreateApiRequest(HttpMethod.Get, "user", token);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadRequiredAsync<GitHubUserPayload>(response, cancellationToken);
        return new GitHubUserDto
        {
            Id = payload.Id,
            Login = payload.Login,
            Name = payload.Name,
            HtmlUrl = payload.HtmlUrl
        };
    }

    public async Task<GitHubOAuthTokenDto> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException("La integración con GitHub no tiene ClientId/ClientSecret configurados.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["code"] = code.Trim()
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadRequiredAsync<GitHubOAuthTokenPayload>(response, cancellationToken);

        if (!string.IsNullOrWhiteSpace(payload.Error))
        {
            throw new GitHubApiException(
                MapOAuthError(payload.ErrorDescription ?? payload.Error),
                (int)HttpStatusCode.BadRequest,
                payload.Error);
        }

        if (string.IsNullOrWhiteSpace(payload.AccessToken))
        {
            throw new GitHubApiException("GitHub no devolvió un access token válido.", (int)HttpStatusCode.BadRequest);
        }

        return new GitHubOAuthTokenDto
        {
            AccessToken = payload.AccessToken,
            TokenType = payload.TokenType ?? "bearer",
            Scope = payload.Scope ?? string.Empty
        };
    }

    private void EnsureEnabled()
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("La integración con GitHub está deshabilitada. Configurá GitHub__Enabled=true.");
        }
    }

    private string ResolveToken(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            return token;
        }

        if (!string.IsNullOrWhiteSpace(_options.ServiceToken))
        {
            return _options.ServiceToken;
        }

        throw new InvalidOperationException("No hay token de GitHub disponible. Conectá tu cuenta o configurá GitHub__ServiceToken.");
    }

    private HttpRequestMessage CreateApiRequest(HttpMethod method, string relativeUrl, string token)
    {
        var request = new HttpRequestMessage(method, relativeUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28");
        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new GitHubApiException(MapHttpError((int)response.StatusCode, body), (int)response.StatusCode);
    }

    private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        if (payload is null)
        {
            throw new GitHubApiException("GitHub devolvió una respuesta vacía o inválida.", (int)response.StatusCode);
        }

        return payload;
    }

    private static string MapHttpError(int statusCode, string body)
    {
        return statusCode switch
        {
            401 => "La autenticación con GitHub falló. Reconectá tu cuenta.",
            403 => body.Contains("rate limit", StringComparison.OrdinalIgnoreCase)
                ? "Se alcanzó el límite de solicitudes de GitHub. Intentá más tarde."
                : "No tenés permisos suficientes en GitHub para esta operación.",
            404 => "No se encontró el recurso en GitHub.",
            422 => "GitHub rechazó la solicitud por datos inválidos.",
            429 => "Se alcanzó el límite de solicitudes de GitHub. Intentá más tarde.",
            _ => "Ocurrió un error al comunicarse con GitHub."
        };
    }

    private static string MapOAuthError(string error)
    {
        return error.Contains("bad_verification_code", StringComparison.OrdinalIgnoreCase)
            ? "El código de autorización de GitHub es inválido o expiró."
            : $"No se pudo completar la autorización con GitHub: {error}";
    }

    private static GitHubRepositoryDto MapRepository(GitHubRepositoryPayload payload)
    {
        return new GitHubRepositoryDto
        {
            Id = payload.Id,
            Name = payload.Name,
            FullName = payload.FullName,
            OwnerLogin = payload.Owner?.Login ?? string.Empty,
            HtmlUrl = payload.HtmlUrl,
            Private = payload.Private,
            Description = payload.Description
        };
    }

    private static GitHubIssueDto MapIssue(GitHubIssuePayload payload)
    {
        return new GitHubIssueDto
        {
            Id = payload.Id,
            NodeId = payload.NodeId,
            Number = payload.Number,
            Title = payload.Title,
            Body = payload.Body,
            State = payload.State,
            HtmlUrl = payload.HtmlUrl,
            AssigneeLogin = payload.Assignee?.Login,
            Labels = payload.Labels?.Select(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToList()
                ?? new List<string>(),
            IsPullRequest = payload.PullRequest is not null,
            CreatedAt = payload.CreatedAt,
            UpdatedAt = payload.UpdatedAt
        };
    }

    private sealed class GitHubRepositoryPayload
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;
        public GitHubUserPayload? Owner { get; set; }
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
        public bool Private { get; set; }
        public string? Description { get; set; }
    }

    private sealed class GitHubUserPayload
    {
        public long Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string? Name { get; set; }
        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }
    }

    private sealed class GitHubIssuePayload
    {
        public long Id { get; set; }
        [JsonPropertyName("node_id")]
        public string NodeId { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string State { get; set; } = string.Empty;
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
        public GitHubUserPayload? Assignee { get; set; }
        public List<GitHubLabelPayload>? Labels { get; set; }
        [JsonPropertyName("pull_request")]
        public object? PullRequest { get; set; }
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    private sealed class GitHubLabelPayload
    {
        public string? Name { get; set; }
    }

    private sealed class GitHubOAuthTokenPayload
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        public string? Scope { get; set; }
        public string? Error { get; set; }
        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
    }
}
