using Kodvian.Core.Application.Profile.Dtos;

namespace Kodvian.Core.Application.Profile.Abstractions;

public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<string> CreateGitHubConnectUrlAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<string> CompleteGitHubCallbackAsync(string? code, string? state, CancellationToken cancellationToken = default);

    Task DisconnectGitHubAsync(Guid userId, CancellationToken cancellationToken = default);
}
