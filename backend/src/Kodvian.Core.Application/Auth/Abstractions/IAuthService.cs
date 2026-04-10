using Kodvian.Core.Application.Auth.Dtos;

namespace Kodvian.Core.Application.Auth.Abstractions;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
