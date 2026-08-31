using Kodvian.Core.Application.Common.Security;
using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Exceptions;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Integrations.GitHub;

public class GitHubTokenProvider : IGitHubTokenProvider
{
    public const string ReconnectMessage = "Reconectá GitHub en Mi perfil";

    private readonly KodvianDbContext _dbContext;
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly IGitHubApiService _gitHubApiService;

    public GitHubTokenProvider(
        KodvianDbContext dbContext,
        ITokenEncryptionService tokenEncryptionService,
        IGitHubApiService gitHubApiService)
    {
        _dbContext = dbContext;
        _tokenEncryptionService = tokenEncryptionService;
        _gitHubApiService = gitHubApiService;
    }

    public async Task<string> GetValidTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Activo, cancellationToken);
        if (user is null)
        {
            throw new ArgumentException("Usuario no encontrado.");
        }

        if (string.IsNullOrWhiteSpace(user.GitHubAccessTokenEncrypted) || !user.GitHubConnectedAt.HasValue)
        {
            throw new InvalidOperationException(ReconnectMessage);
        }

        string accessToken;
        try
        {
            accessToken = _tokenEncryptionService.Decrypt(user.GitHubAccessTokenEncrypted);
        }
        catch
        {
            await ClearConnectionAsync(user, cancellationToken);
            throw new InvalidOperationException(ReconnectMessage);
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            await ClearConnectionAsync(user, cancellationToken);
            throw new InvalidOperationException(ReconnectMessage);
        }

        try
        {
            await _gitHubApiService.GetAuthenticatedUserAsync(accessToken, cancellationToken);
        }
        catch (GitHubApiException ex) when (ex.StatusCode == 401)
        {
            await ClearConnectionAsync(user, cancellationToken);
            throw new InvalidOperationException(ReconnectMessage);
        }

        return accessToken;
    }

    private async Task ClearConnectionAsync(User user, CancellationToken cancellationToken)
    {
        user.GitHubAccessTokenEncrypted = null;
        user.GitHubUsername = null;
        user.GitHubUserId = null;
        user.GitHubConnectedAt = null;
        user.FechaActualizacion = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
