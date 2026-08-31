using System.Security.Cryptography;
using Kodvian.Core.Application.Common.Security;
using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Profile.Abstractions;
using Kodvian.Core.Application.Profile.Dtos;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Infrastructure.Integrations.GitHub;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly KodvianDbContext _dbContext;
    private readonly IGitHubApiService _gitHubApiService;
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly IGitHubIssueSyncService _gitHubIssueSyncService;
    private readonly GitHubOptions _options;

    public ProfileService(
        KodvianDbContext dbContext,
        IGitHubApiService gitHubApiService,
        ITokenEncryptionService tokenEncryptionService,
        IGitHubIssueSyncService gitHubIssueSyncService,
        IOptions<GitHubOptions> options)
    {
        _dbContext = dbContext;
        _gitHubApiService = gitHubApiService;
        _tokenEncryptionService = tokenEncryptionService;
        _gitHubIssueSyncService = gitHubIssueSyncService;
        _options = options.Value;
    }

    public async Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId && x.Activo, cancellationToken);

        if (user is null || user.Role is null || !user.Role.Activo)
        {
            return null;
        }

        return MapProfile(user);
    }

    public async Task<string> CreateGitHubConnectUrlAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        EnsureGitHubEnabled();

        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.CallbackUrl))
        {
            throw new InvalidOperationException("GitHub OAuth no está configurado (ClientId/CallbackUrl).");
        }

        var userExists = await _dbContext.Users.AnyAsync(x => x.Id == userId && x.Activo, cancellationToken);
        if (!userExists)
        {
            throw new ArgumentException("Usuario no encontrado.");
        }

        var now = DateTime.UtcNow;
        var expired = await _dbContext.GitHubOAuthStates
            .Where(x => x.UserId == userId || x.ExpiresAt < now)
            .ToListAsync(cancellationToken);
        if (expired.Count > 0)
        {
            _dbContext.GitHubOAuthStates.RemoveRange(expired);
        }

        var stateToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        _dbContext.GitHubOAuthStates.Add(new GitHubOAuthState
        {
            StateToken = stateToken,
            UserId = userId,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(Math.Max(1, _options.OAuthStateExpirationMinutes))
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        var query = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.CallbackUrl,
            ["scope"] = _options.OAuthScope,
            ["state"] = stateToken
        };

        var queryString = string.Join('&', query.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        return $"https://github.com/login/oauth/authorize?{queryString}";
    }

    public async Task<string> CompleteGitHubCallbackAsync(string? code, string? state, CancellationToken cancellationToken = default)
    {
        EnsureGitHubEnabled();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("El callback de GitHub requiere code y state.");
        }

        var oauthState = await _dbContext.GitHubOAuthStates
            .FirstOrDefaultAsync(x => x.StateToken == state, cancellationToken);

        if (oauthState is null)
        {
            throw new ArgumentException("El state de GitHub es inválido o ya fue utilizado.");
        }

        if (oauthState.ExpiresAt < DateTime.UtcNow)
        {
            _dbContext.GitHubOAuthStates.Remove(oauthState);
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new ArgumentException("El state de GitHub expiró. Volvé a conectar tu cuenta.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == oauthState.UserId && x.Activo, cancellationToken);
        if (user is null)
        {
            _dbContext.GitHubOAuthStates.Remove(oauthState);
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new ArgumentException("Usuario no encontrado para el state de GitHub.");
        }

        var token = await _gitHubApiService.ExchangeCodeForTokenAsync(code, cancellationToken);
        var gitHubUser = await _gitHubApiService.GetAuthenticatedUserAsync(token.AccessToken, cancellationToken);

        user.GitHubAccessTokenEncrypted = _tokenEncryptionService.Encrypt(token.AccessToken);
        user.GitHubUsername = gitHubUser.Login;
        user.GitHubUserId = gitHubUser.Id;
        user.GitHubConnectedAt = DateTime.UtcNow;
        user.FechaActualizacion = DateTime.UtcNow;

        _dbContext.GitHubOAuthStates.Remove(oauthState);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await _gitHubIssueSyncService.SyncAfterConnectAsync(user.Id, user.DeveloperId, cancellationToken);
        }
        catch
        {
            // OAuth succeeded; initial sync can be retried manually from mi-trabajo.
        }

        return string.IsNullOrWhiteSpace(_options.FrontendSuccessPath)
            ? "/mi-perfil?connected=true"
            : _options.FrontendSuccessPath;
    }

    public async Task DisconnectGitHubAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && x.Activo, cancellationToken);
        if (user is null)
        {
            throw new ArgumentException("Usuario no encontrado.");
        }

        user.GitHubUsername = null;
        user.GitHubUserId = null;
        user.GitHubAccessTokenEncrypted = null;
        user.GitHubConnectedAt = null;
        user.FechaActualizacion = DateTime.UtcNow;

        var states = await _dbContext.GitHubOAuthStates.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        if (states.Count > 0)
        {
            _dbContext.GitHubOAuthStates.RemoveRange(states);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void EnsureGitHubEnabled()
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("La integración con GitHub está deshabilitada. Configurá GitHub__Enabled=true.");
        }
    }

    private static ProfileDto MapProfile(User user)
    {
        return new ProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role!.Name,
            DeveloperId = user.DeveloperId,
            GitHubConnected = user.GitHubConnectedAt.HasValue && !string.IsNullOrWhiteSpace(user.GitHubAccessTokenEncrypted),
            GitHubUsername = user.GitHubUsername,
            GitHubConnectedAt = user.GitHubConnectedAt
        };
    }
}
