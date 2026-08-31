namespace Kodvian.Core.Application.Integrations.GitHub.Abstractions;

public interface IGitHubTokenProvider
{
    /// <summary>
    /// Returns a decrypted GitHub access token for the user.
    /// Clears the stored connection when GitHub responds 401 (revoked classic OAuth token).
    /// </summary>
    Task<string> GetValidTokenAsync(Guid userId, CancellationToken cancellationToken = default);
}
